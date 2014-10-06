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
#if !LOAD_ONLY
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Xml.Linq;
using CILAssemblyManipulator.API;
using CILAssemblyManipulator.DotNET;
using CollectionsWithRoles.API;
using Qi4CS.CodeGeneration.DotNET;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using CommonUtils;
using Qi4CS.Core.API.Model;
using System.Text;

/// <summary>
/// Class containing extension methods to easily generate Qi4CS assemblies based on <see cref="ApplicationModel{T}"/>.
/// </summary>
public static class E_Qi4CS_CodeGeneration
{
   /// <summary>
   /// Helper method to call <see cref="ApplicationModel{T}.GenerateCode(CILReflectionContext, Boolean)"/> and save the resulting Qi4CS generated assemblies to specific folder.
   /// </summary>
   /// <typeparam name="TInstance">The type of the Qi4CS application instance of <paramref name="model"/>.</typeparam>
   /// <param name="model">The <see cref="ApplicationModel{T}"/>.</param>
   /// <param name="path">The path where generated Qi4CS assemblies should reside. If <c>null</c>, value of <see cref="Environment.CurrentDirectory"/> will be used.</param>
   /// <param name="isWP8OrSL5Emit">Whether emit assemblies compatible with Windows Phone 8 or Silverlight 5.</param>
   /// <param name="emittingInfoCreator">
   /// The callback to create an <see cref="EmittingArguments"/> for emitting an assembly.
   /// It receives the existing <see cref="System.Reflection.Assembly"/> from which the Qi4CS assembly was generated, corresponding generated <see cref="CILAssembly"/>.
   /// It should return <see cref="EmittingArguments"/> to be used to emit the Qi4CS assembly.
   /// If the callback is <c>null</c> or returns <c>null</c>, the result of <see cref="EmittingArguments.CreateForEmittingDLL(StrongNameKeyPair, ImageFileMachine, TargetRuntime, ModuleFlags)"/> is used, with no strong name and <see cref="ImageFileMachine.I386"/> and <see cref="TargetRuntime.Net_4_0"/> as parameters.
   /// </param>
   /// <returns>
   /// The mapping from assembly used in Qi4CS model to the full path of corresponding generated Qi4CS assembly.
   /// </returns>
   public static IDictionary<System.Reflection.Assembly, String> GenerateAndSaveAssemblies<TInstance>( this ApplicationModel<TInstance> model, String path = null, Boolean isWP8OrSL5Emit = false, Func<System.Reflection.Assembly, CILAssembly, EmittingArguments> emittingInfoCreator = null )
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
      var fileNames = DotNETReflectionContext.UseDotNETContext( ctx =>
      {
         var genDic = model.GenerateCode( ctx, isWP8OrSL5Emit );
         var eArgsDic = new Dictionary<CILAssembly, EmittingArguments>();
         // Before emitting, we must set the public keys of the generated assemblies, in order for cross-references to work properly.
         // Either CreateDefaultEmittingArguments or emittingInfoCreator should set the public key, if the strong name key pair is container name.
         foreach ( var kvp in genDic )
         {
            var genAss = kvp.Value;
            var nAss = kvp.Key;
            EmittingArguments eArgs;
            if ( emittingInfoCreator == null )
            {
               eArgs = CreateDefaultEmittingArguments();
            }
            else
            {
               eArgs = ( emittingInfoCreator( nAss, genAss ) ?? CreateDefaultEmittingArguments() );
            }
            if ( eArgs.StrongName != null && !eArgs.StrongName.KeyPair.IsNullOrEmpty() && genAss.Name.PublicKey.IsNullOrEmpty() )
            {
               // Have to set the public key
               genAss.Name.PublicKey = Utils.ExtractPublicKey( eArgs.StrongName.KeyPair.ToArray(), ( eArgs.SigningAlgorithm ?? AssemblyHashAlgorithm.SHA1 ) );
               genAss.Name.Flags |= AssemblyFlags.PublicKey;
            }
            eArgsDic.Add( genAss, eArgs );
         }


         var resultDic = new ConcurrentDictionary<System.Reflection.Assembly, String>();
         System.Threading.Tasks.Parallel.ForEach( genDic, kvp =>
         {
            foreach ( var mod in kvp.Value.Modules )
            {
               var fileName = Path.Combine( path, mod.Name );

               using ( var stream = File.Open( fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read ) )
               {
                  var eArgs = eArgsDic[kvp.Value];
                  mod.EmitModule( stream, eArgs );
                  stream.Flush();
                  //refDic.TryAdd( fileName, eArgs.AssemblyRefs.Select( aRef => Tuple.Create( aRef, IsDotNETAssembly( aRef, portabilityHelper, eArgs.PCLProfile ) ) ).ToArray() );
                  resultDic.TryAdd( kvp.Key, fileName );
               }
            }
         } );

         return resultDic;
      } );

      return fileNames;
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

   private static EmittingArguments CreateDefaultEmittingArguments()
   {
      return EmittingArguments.CreateForEmittingDLL( null, ImageFileMachine.I386, TargetRuntime.Net_4_0 );
   }

}
#endif
