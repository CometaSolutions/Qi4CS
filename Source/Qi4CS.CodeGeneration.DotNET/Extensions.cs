/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
 *
 * Licensed  under the  Apache License,  Version 2.0  (the "License");
 * you may not use  this file  except in  compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed  under the  License is distributed on an "AS IS" BASIS,
 * WITHOUT  WARRANTIES OR CONDITIONS  OF ANY KIND, either  express  or
 * implied.
 *
 * See the License for the specific language governing permissions and
 * limitations under the License. 
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Xml.Linq;
using CILAssemblyManipulator.Logical;
using CollectionsWithRoles.API;
using Qi4CS.CodeGeneration;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using CommonUtils;
using Qi4CS.Core.API.Model;
using System.Text;
using CILAssemblyManipulator.Physical;

namespace Qi4CS.CodeGeneration
{
   /// <summary>
   /// This enumeration controls the parallelization level of <see cref="E_Qi4CS_CodeGeneration.GenerateAndSaveAssemblies"/> method.
   /// </summary>
   [Flags]
   public enum CodeGenerationParallelization
   {
      /// <summary>
      /// No parallelization at all will be used, everything will be done in a single thread.
      /// </summary>
      NotParallel = 0,
      /// <summary>
      /// Emitting the <see cref="CILAssembly"/> and <see cref="CILMetaData"/> will be done in parallel.
      /// </summary>
      ParallelEmitting = 1,
      /// <summary>
      /// Saving the generated <see cref="CILMetaData"/>s to disk will be done in parallel.
      /// </summary>
      ParallelSaving = 2
   }
}
/// <summary>
/// Class containing extension methods to easily generate Qi4CS assemblies based on <see cref="ApplicationModel{T}"/>.
/// </summary>
public static class E_Qi4CS_CodeGeneration
{
   /// <summary>
   /// Helper method to call <see cref="ApplicationModel{T}.GenerateCode"/> and save the resulting Qi4CS generated assemblies to specific folder.
   /// </summary>
   /// <typeparam name="TInstance">The type of the Qi4CS application instance of <paramref name="model"/>.</typeparam>
   /// <param name="model">The <see cref="ApplicationModel{T}"/>.</param>
   /// <param name="parallelization">How to parallelize the code generation.</param>
   /// <param name="path">The path where generated Qi4CS assemblies should reside. If <c>null</c>, value of <see cref="Environment.CurrentDirectory"/> will be used.</param>
   /// <param name="isSilverlight">Whether emit assemblies compatible with Silverlight 5+.</param>
   /// <param name="logicalAssemblyProcessor">
   /// The optional callback to process <see cref="CILAssembly"/> or <see cref="EmittingArguments"/> after generation of given <see cref="CILAssembly"/>.
   /// The callback receives the existing <see cref="System.Reflection.Assembly"/> from which the Qi4CS assembly was generated, corresponding generated <see cref="CILAssembly"/>, and corresponding <see cref="EmittingArguments"/> that will be used to write the generated assembly.
   /// </param>
   /// <param name="physicalMDProcessor">
   /// The optional callback to process physical <see cref="CILMetaData"/> before writing it to disk.
   /// The callback receives the existing <see cref="System.Reflection.Assembly"/> from which the Qi4CS assembly was generated, corresponding generated <see cref="CILAssembly"/>, and the physical <see cref="CILMetaData"/> generated from the given <see cref="CILAssembly"/>.
   /// </param>
   /// <returns>
   /// The mapping from assembly used in Qi4CS model to the full path of corresponding generated Qi4CS assembly.
   /// </returns>
   public static IDictionary<System.Reflection.Assembly, String> GenerateAndSaveAssemblies<TInstance>(
      this ApplicationModel<TInstance> model,
      CodeGenerationParallelization parallelization = CodeGenerationParallelization.NotParallel,
      String path = null,
      Boolean isSilverlight = false,
      Action<System.Reflection.Assembly, CILAssembly, EmittingArguments> logicalAssemblyProcessor = null,
      Action<System.Reflection.Assembly, CILAssembly, CILMetaData> physicalMDProcessor = null
      )
      where TInstance : ApplicationSPI
   {
      if ( path == null )
      {
         path = Environment.CurrentDirectory;
      }
      else
      {
         if ( !Directory.Exists( path ) )
         {
            Directory.CreateDirectory( path );
         }
         if ( !path.EndsWith( "" + Path.DirectorySeparatorChar ) )
         {
            path += Path.DirectorySeparatorChar;
         }
         path = Path.GetDirectoryName( Path.GetFullPath( path ) );
      }


      //var refDic = new ConcurrentDictionary<String, Tuple<CILAssemblyName, Boolean>[]>();
      ConcurrentDictionary<System.Reflection.Assembly, String> resultDic;

      using ( var ctx = DotNETReflectionContext.CreateDotNETContext( parallelization == CodeGenerationParallelization.NotParallel ? CILReflectionContextConcurrencySupport.NotThreadSafe : CILReflectionContextConcurrencySupport.ThreadSafe_WithConcurrentCollections ) )
      {
         var genDic = model.GenerateCode( ctx, parallelization.HasFlag( CodeGenerationParallelization.ParallelEmitting ), isSilverlight );
         var eArgsDic = new Dictionary<CILAssembly, EmittingArguments>();
         // Before emitting, we must set the public keys of the generated assemblies, in order for cross-references to work properly.
         // Either CreateDefaultEmittingArguments or emittingInfoCreator should set the public key, if the strong name key pair is container name.
         foreach ( var kvp in genDic )
         {
            var genAss = kvp.Value;
            var nAss = kvp.Key;
            var eArgs = ctx.CreateDefaultEmittingArguments();
            if ( logicalAssemblyProcessor != null )
            {
               logicalAssemblyProcessor( nAss, genAss, eArgs );
            }

            if ( eArgs.StrongName != null && !eArgs.StrongName.KeyPair.IsNullOrEmpty() )
            {
               // Have to set the public key
               genAss.Name.PublicKey = ctx.DefaultCryptoCallbacks.CreatePublicKeyFromStrongName( eArgs.StrongName, eArgs.SigningAlgorithm );
               genAss.Name.Flags |= AssemblyFlags.PublicKey;
            }
            eArgsDic.Add( genAss, eArgs );
         }

         // This has to check for ParallelEmitting flag, since CreatePhysicalRepresentation might require concurrent features from reflection context when run in parallel!
         var physicalDic = new ConcurrentDictionary<CILAssembly, CILMetaData>();
         Qi4CS.Core.Runtime.Model.CodeGenUtils.DoPotentiallyInParallel(
            parallelization.HasFlag( CodeGenerationParallelization.ParallelEmitting ),
            genDic,
            kvp =>
            {
               var nAss = kvp.Key;
               var genAss = genDic[nAss];
               physicalDic.TryAdd( genAss, genAss.MainModule.CreatePhysicalRepresentation( false ) );
            } );


         resultDic = new ConcurrentDictionary<System.Reflection.Assembly, String>();
         Qi4CS.Core.Runtime.Model.CodeGenUtils.DoPotentiallyInParallel(
            parallelization.HasFlag( CodeGenerationParallelization.ParallelSaving ),
            genDic,
            kvp =>
            {
               foreach ( var mod in kvp.Value.Modules )
               {
                  var fileName = Path.Combine( path, mod.Name );

                  using ( var stream = File.Open( fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read ) )
                  {
                     var nAss = kvp.Key;
                     var genAss = kvp.Value;
                     var md = physicalDic[genAss];
                     if ( physicalMDProcessor != null )
                     {
                        physicalMDProcessor( nAss, genAss, md );
                     }
                     md.OrderTablesAndRemoveDuplicates();
                     md.WriteModule( stream, eArgsDic[genAss] );
                     stream.Flush();
                     resultDic.TryAdd( nAss, fileName );
                  }
               }
            } );

#if DEBUG
         foreach ( var kvp in resultDic )
         {
            String peVerify, snVerify;
            if ( Verification.RunPEVerify( null, kvp.Value, false, out peVerify, out snVerify ) )
            {
               throw new VerificationException( peVerify, snVerify );
            }
         }
#endif
      }

      return resultDic;
   }

   /// <summary>
   /// Checks whether given <see cref="CodeGenerationParallelization"/> specifies that code generation should be parallelized in any way.
   /// </summary>
   /// <param name="parallelization">The <see cref="CodeGenerationParallelization"/>.</param>
   /// <returns><c>true</c> if <paramref name="parallelization"/> is not <see cref="CodeGenerationParallelization.NotParallel"/>; <c>false</c> otherwise.</returns>
   public static Boolean HasAnyParallelization( this CodeGenerationParallelization parallelization )
   {
      return parallelization != CodeGenerationParallelization.NotParallel;
   }

   private static String GetRelativePath( String folder, String filespec )
   {
      Uri pathUri = new Uri( filespec );
      // Folders must end in a slash
      if ( !folder.EndsWith( Path.DirectorySeparatorChar.ToString() ) )
      {
         folder += Path.DirectorySeparatorChar;
      }
      Uri folderUri = new Uri( folder );
      return Uri.UnescapeDataString( folderUri.MakeRelativeUri( pathUri ).ToString().Replace( '/', Path.DirectorySeparatorChar ) );
   }

   private static EmittingArguments CreateDefaultEmittingArguments( this CILReflectionContext ctx )
   {
      return new EmittingArguments()
      {
         CryptoCallbacks = ctx.DefaultCryptoCallbacks,
         Headers = new HeadersData()
      };
   }

}
