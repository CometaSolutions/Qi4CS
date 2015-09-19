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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using CommonUtils;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.Bootstrap.Model;
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Physical;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Diagnostics;
using Microsoft.Build.Framework;
using System.Collections.Concurrent;

namespace Qi4CS.CodeGeneration.MSBuild
{
   /// <summary>
   /// This enumeration extends the <see cref="CodeGenerationParallelization"/> enumeration to provide addition parallelization customization.
   /// </summary>
   [Flags]
   public enum MSBuildCodeGenerationParallelization
   {
      /// <summary>
      /// No parallelization at all will be used, everything will be done in a single thread.
      /// </summary>
      /// <seealso cref="CodeGenerationParallelization.NotParallel"/>
      NotParallel = CodeGenerationParallelization.NotParallel,
      /// <summary>
      /// Emitting the <see cref="CILAssembly"/> and <see cref="CILMetaData"/> will be done in parallel.
      /// </summary>
      /// <seealso cref="CodeGenerationParallelization.ParallelEmitting"/>
      ParallelEmitting = CodeGenerationParallelization.ParallelEmitting,
      /// <summary>
      /// Saving the generated <see cref="CILMetaData"/>s to disk will be done in parallel.
      /// </summary>
      /// <seealso cref="CodeGenerationParallelization.ParallelSaving"/>
      ParallelSaving = CodeGenerationParallelization.ParallelSaving,
      /// <summary>
      /// If verifying assemblies, it should be done in parallel.
      /// </summary>
      ParallelVerification = 4,
      /// <summary>
      /// If copying target assemblies to different folder, then whether to copy in parallel.
      /// </summary>
      ParallelTargetAssemblyCopying = 8,
      /// <summary>
      /// A mask for <see cref="CodeGenerationParallelization"/> values.
      /// </summary>
      CodeGenMask = 3,
   }
   /// <summary>
   /// This task will generate the Qi4CS assemblies for the Qi4CS application model of the assembly being built.
   /// </summary>
   /// <seealso cref="ModelFactory"/>
   public class GenerateQi4CSAssemblies : AppDomainIsolatedTask
   {

      /// <summary>
      /// Gets or sets the directory where the generated Qi4CS assemblies should be located.
      /// May be relative or absolute path.
      /// </summary>
      /// <value>The directory where the generated Qi4CS assemblies should be located.</value>
      public String AssembliesDir { get; set; }

      /// <summary>
      /// Gets or sets the name of the object implemeting <see cref="Qi4CS.Core.Bootstrap.Model.Qi4CSModelProvider{T}"/> interface.
      /// </summary>
      /// <value>The name of the object implemeting <see cref="Qi4CS.Core.Bootstrap.Model.Qi4CSModelProvider{T}"/> interface.</value>
      public String ModelFactory { get; set; }

      /// <summary>
      /// Gets or sets the name of the source assembly
      /// </summary>
      [Required]
      public String SourceAssembly { get; set; }

      /// <summary>
      /// Gets or sets the textual value assembly information, which is really XML.
      /// </summary>
      /// <value>The textual value assembly information, which is really XML.</value>
      public String AssemblyInformation { get; set; }

      /// <summary>
      /// Gets or sets the textual ID of target framework.
      /// </summary>
      /// <value>The textual ID of target framework.</value>
      [Required]
      public String TargetFW { get; set; }

      /// <summary>
      /// Gets or sets the textual version of target framework.
      /// </summary>
      /// <value>The textual version of target framework.</value>
      [Required]
      public String TargetFWVersion { get; set; }

      /// <summary>
      /// Gets or sets the textual profile name of target framework.
      /// </summary>
      /// <value>The textual profile name of target framework.</value>
      public String TargetFWProfile { get; set; }

      /// <summary>
      /// Gets or sets the textual version of target platform.
      /// </summary>
      /// <value>The textual version of target platform.</value>
      public String TargetPlatform { get; set; }

      /// <summary>
      /// Gets or sets the directory which contains the .csproj file currently being processed.
      /// </summary>
      /// <value>The directory which contains the .csproj file currently being processed.</value>
      public String OriginatingProjectDir { get; set; }

      /// <summary>
      /// Gets or sets the output directory of the .csproj file currently being processed.
      /// </summary>
      /// <value>The output directory of the .csproj file currently being processed.</value>
      public String OutputDir { get; set; }

      /// <summary>
      /// Gets or sets the base directory where target framework assemblies reside.
      /// </summary>
      /// <value>The base directory where target frameowkr assemblies reside.</value>
      [Required]
      public String ReferenceAssembliesDir { get; set; }

      /// <summary>
      /// Gets or sets value indicating whether to run PEVerify on Qi4CS generated assemblies.
      /// </summary>
      /// <value>The value indicating whether to run PEVerify on Qi4CS generated assemblies.</value>
      public Boolean PerformVerify { get; set; }

      /// <summary>
      /// Gets or sets the path to latest Windows SDK Version.
      /// </summary>
      /// <value>The path to latest Windows SDK Version.</value>
      public String WindowsSDKDir { get; set; }

      /// <summary>
      /// Gets or sets the path where Silverlight version directories containing runtime DLLs are located.
      /// </summary>
      public String SilverlightRuntimeBaseDir { get; set; }

      /// <summary>
      /// Gets or sets whether code generation should be parallelized.
      /// </summary>
      /// <remarks>This should be stringified value from <see cref="CodeGenerationParallelization"/> enumeration.</remarks>
      public String Parallelization { get; set; }

      /// <summary>
      /// Gets or sets the full file names of generated Qi4CS assemblies.
      /// </summary>
      /// <value>The full file names of generated Qi4CS assemblies.</value>
      /// <remarks>
      /// Each task item for generated assembly will contain metadata <c>OriginalAssemblyPath</c> with information about original assembly.
      /// </remarks>
      [Output]
      public TaskItem[] GeneratedAssemblies { get; set; }

      /// <inheritdoc />
      public override Boolean Execute()
      {
         Boolean retVal = false;
         try
         {
            var projectDir = String.IsNullOrEmpty( this.OriginatingProjectDir ) ?
               Path.GetDirectoryName( this.BuildEngine.ProjectFileOfTaskNode ) :
               Path.GetFullPath( this.OriginatingProjectDir );

            // Process assemblies directory
            var assDir = this.AssembliesDir;
            if ( String.IsNullOrEmpty( assDir ) )
            {
               assDir = projectDir;
            }
            else if ( !Path.IsPathRooted( assDir ) )
            {
               assDir = Path.Combine( projectDir, assDir );
            }

            assDir = Path.GetFullPath( assDir );

            // Process source assembly
            var sourceAss = this.SourceAssembly;

            if ( String.IsNullOrEmpty( sourceAss ) )
            {
               this.Log.LogError( "The source assembly for Qi4CS assembly generation was empty or null, could not continue." );
            }
            else
            {
               var od = this.OutputDir;
               if ( !Path.IsPathRooted( sourceAss ) )
               {
                  sourceAss = Path.GetFullPath( Path.Combine( projectDir, od == null || Path.IsPathRooted( od ) ? "" : od, sourceAss ) );
               }

               if ( !File.Exists( sourceAss ) )
               {
                  this.Log.LogError( "The source assembly ({0}) does not exist.", sourceAss );
               }
               else
               {

                  // Set application domain application base to this assembly directory in order to capture the reference to
                  // Qi4CS assembly capable of generating code.
                  var thisAssemblyPath = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase ).LocalPath;

                  //this.Log.LogMessage( Microsoft.Build.Framework.MessageImportance.High, "Generating Qi4CS assemblies for {0}, target directory is {1}, verifying: {2}.", sourceAss, assDir, this.PerformVerify );

                  // TODO I'm not sure this current directory change is required anymore.
                  var oldCurDir = Environment.CurrentDirectory;
                  Environment.CurrentDirectory = Path.GetDirectoryName( sourceAss );

                  var pStr = this.Parallelization;
                  MSBuildCodeGenerationParallelization parallelization;
                  if ( !Enum.TryParse<MSBuildCodeGenerationParallelization>( pStr, true, out parallelization ) )
                  {
                     this.Log.LogWarning( "Unsupported parallelization value: {0}, defaulting to not parallel.", pStr );
                     parallelization = MSBuildCodeGenerationParallelization.NotParallel;
                  }

                  try
                  {
                     var generator = new Qi4CSAssemblyGenerator( sourceAss, this.ModelFactory, this.ResolveSLRuntimeDir() );

                     if ( generator.CanGenerate )
                     {
                        //this.BuildEngine3.Yield();
                        try
                        {
                           // If there is no .ToString() call, one will get type load exception, as MSBuildCodeGenerationParallelization enumeration is not loaded in the app domain of the caller of this task. 
                           this.Log.LogMessage( MessageImportance.High, "Starting Qi4CS code generation, parallelization: {0}.", parallelization.ToString() );
                           var sw = new Stopwatch();
                           sw.Start();
                           var fileNameDic = generator.GenerateAssemblies(
                              projectDir,
                              this.TargetFW,
                              this.TargetFWVersion,
                              this.TargetFWProfile,
                              this.ReferenceAssembliesDir,
                              this.TargetPlatform,
                              assDir,
                              this.AssemblyInformation,
                              Path.GetDirectoryName( sourceAss ),
                              parallelization,
                              this.PerformVerify,
                              this.WindowsSDKDir );
                           sw.Stop();
                           retVal = true;

                           this.GeneratedAssemblies = fileNameDic
                              .Select( kvp =>
                              {
                                 var item = new TaskItem( kvp.Value );
                                 item.SetMetadata( "OriginalAssemblyPath", kvp.Key );
                                 return item;
                              } )
                              .ToArray();
                           this.Log.LogMessage( MessageImportance.High, "Qi4CS code generation ended in {0}, results:\n{1}", sw.Elapsed, String.Join( "\n", fileNameDic.Select( kvp => "\t" + kvp.Key + " -> " + kvp.Value ).ToArray() ) );
                        }
                        finally
                        {
                           //this.BuildEngine3.Reacquire();
                        }
                     }
                     else
                     {
                        this.Log.LogMessage( MessageImportance.High, "Skipping Qi4CS assembly generation as suitable model provider type was not found." );
                        retVal = true;
                        this.GeneratedAssemblies = Empty<TaskItem>.Array;
                     }
                  }
                  catch ( System.Reflection.TargetInvocationException tie )
                  {
                     if ( tie.InnerException is Qi4CSBuildException )
                     {
                        this.Log.LogError( tie.InnerException.Message );
                     }
                     else
                     {
                        throw;
                     }
                  }
                  catch ( Qi4CSBuildException qExc )
                  {
                     this.Log.LogError( qExc.Message );
                  }
                  catch ( Exception exc )
                  {
                     this.Log.LogErrorFromException( exc, true, true, null );
                  }
                  finally
                  {
                     Environment.CurrentDirectory = oldCurDir;
                  }
               }
            }
         }
         catch ( Exception exc )
         {
            this.Log.LogErrorFromException( exc, true, true, null );
         }
         return retVal;
      }

      private static Boolean IsSilverlight( String targetFW )
      {
         return "Silverlight".Equals( targetFW, StringComparison.InvariantCultureIgnoreCase ) ||
            "WindowsPhone".Equals( targetFW, StringComparison.InvariantCultureIgnoreCase );
      }

      private String ResolveSLRuntimeDir()
      {
         var targetFW = this.TargetFW;
         return IsSilverlight( targetFW ) ?
            Path.Combine( this.SilverlightRuntimeBaseDir, Directory.EnumerateDirectories( this.SilverlightRuntimeBaseDir, this.GetTargetFWMajorVersion() + "*" ).OrderByDescending( s => s ).Last() ) :
            null;
      }

      private String GetTargetFWMajorVersion()
      {
         // "v5.0" => "5"
         var targetFWVersion = this.TargetFWVersion;
         var dotIndex = targetFWVersion.IndexOf( '.' ) - 1;
         return dotIndex > 0 ? targetFWVersion.Substring( 1, dotIndex - 1 ) : targetFWVersion.Substring( 1 );
      }
   }

   /// <summary>
   /// This class is responsible for generating Qi4CS assemblies based on <see cref="Qi4CSModelProvider{T}"/> provided in the source code.
   /// </summary>
   public class Qi4CSAssemblyGenerator
   {
      private const String VERSION_NAME = "Version";
      private const String PEVERIFY_EXE = "PEVerify.exe";
      private const String ANYCPU = "AnyCPU";
      private const String X86 = "x86";
      private const String X64 = "x64";

      private static readonly System.Reflection.ConstructorInfo TARGET_FW_ATTR_CTOR = typeof( System.Runtime.Versioning.TargetFrameworkAttribute ).LoadConstructorOrThrow( new[] { typeof( String ) } );

      private delegate Boolean ParsingDelegate<TIn, TOut>( TIn element, out TOut result );

      private readonly Qi4CSModelProvider<ApplicationModel<ApplicationSPI>> _modelFactory;
      private readonly String _slRuntimeDir;

      /// <summary>
      /// Creates new instance of <see cref="Qi4CSAssemblyGenerator"/>.
      /// </summary>
      /// <param name="sourceAssembly">The path to the assembly containing type implementing <see cref="Qi4CSModelProvider{T}"/>. This will be given to <see cref="System.Reflection.Assembly.LoadFrom(String)"/> method.</param>
      /// <param name="modelFactoryName">The name of the type within the assembly which implements <see cref="Qi4CSModelProvider{T}"/> and has a public parameterless constructor. If <c>null</c>, then first suitable type, in unspecified order, will be selected.</param>
      /// <param name="slRuntimeDir">Resolved silverlight runtime directory.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="sourceAssembly"/> is <c>null</c>.</exception>
      /// <exception cref="Qi4CSBuildException">If <paramref name="modelFactoryName"/> is not <c>null</c>, this is thrown when the specified type is not found within the specified assembly, or when the type is not suitable.</exception>
      public Qi4CSAssemblyGenerator( String sourceAssembly, String modelFactoryName, String slRuntimeDir )
      {
         ArgumentValidator.ValidateNotNull( "Source assembly", sourceAssembly );

         // Load SDK version of all Qi4CS assemblies
         foreach ( var fn in Directory.EnumerateFiles( Path.GetDirectoryName( new Uri( this.GetType().Assembly.CodeBase ).LocalPath ), "Qi4CS.*.dll" ) )
         {
            System.Reflection.Assembly.Load( Path.GetFileNameWithoutExtension( fn ) );
         }

         // Add to resolve event to load target application assemblies
         //AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;

         var ass = System.Reflection.Assembly.LoadFrom( sourceAssembly );
         var mfNameGiven = !String.IsNullOrEmpty( modelFactoryName );

         this._slRuntimeDir = slRuntimeDir;

         if ( this.IsSilverlight )
         {
            // We are running SL code from desktop application, so have to resolve SL-specific DLLs.
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
         }

         Type mfType;
         try
         {
            mfType = mfNameGiven ?
            ass.GetType( modelFactoryName, false, false ) :
            ass.GetTypes().FirstOrDefault( t => IsSuitableType( t ) && HasSuitableCtor( t ) );
         }
         catch ( System.Reflection.ReflectionTypeLoadException rte )
         {
            throw new Qi4CSBuildException( String.Join( "\n\n", (Object[]) rte.LoaderExceptions ), rte );
         }
         if ( mfType == null )
         {
            if ( mfNameGiven )
            {
               throw new Qi4CSBuildException( String.Format( "The Qi4CS model factory type {0} is not found within assembly {1}", modelFactoryName, ass ) );
            }
         }
         else if ( mfNameGiven )
         {
            if ( !IsSuitableType( mfType ) )
            {
               throw new Qi4CSBuildException( String.Format( "The given Qi4CS model factory type {0} with assembly {1} does not implement {2}", mfType, ass, typeof( Qi4CSModelProvider<> ) ) );
            }
         }

         if ( mfType != null )
         {
            var ctor = GetSuitableCtor( sourceAssembly, mfType );
            if ( ctor == null )
            {
               throw new Qi4CSBuildException( String.Format( "The Qi4CS model factory type {0} must have a public parameterless constructor.", mfType ) );
            }

            this._modelFactory = (Qi4CSModelProvider<ApplicationModel<ApplicationSPI>>) ctor.Invoke( Type.EmptyTypes );
         }
      }

      /// <summary>
      /// Get the value indicating whether Qi4CS assemblies can be generated.
      /// </summary>
      /// <value>The value indicating whether Qi4CS assemblies can be generated.</value>
      public Boolean CanGenerate
      {
         get
         {
            return this._modelFactory != null;
         }
      }

      private System.Reflection.Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
      {
         Console.WriteLine( "SL BUILD trying to resolve " + args.Name + " using SL runtime directory " + this._slRuntimeDir + "." );

         // This only gets invoked in Silverlight builds
         System.Reflection.Assembly result = null;
         CILAssemblyName aName;
         if ( CILAssemblyName.TryParse( args.Name, out aName ) )
         {
            var suitablePath = Path.Combine( this._slRuntimeDir, aName.Name + ".dll" );
            if ( File.Exists( suitablePath ) )
            {
               result = System.Reflection.Assembly.LoadFrom( suitablePath );
            }
         }
         return result;
      }

      private static Boolean IsSuitableType( Type t )
      {
         return t.IsClass
            && !t.IsAbstract
            && t.GetGenericArguments().Length == 0
            && typeof( Qi4CSModelProvider<> ).IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( t );
      }

      private static Boolean HasSuitableCtor( Type t )
      {
         return t.GetConstructor( Type.EmptyTypes ) != null;
      }

      private static System.Reflection.ConstructorInfo GetSuitableCtor( String sourceAssembly, Type t )
      {
         var ctor = t.GetConstructors( System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic ).FirstOrDefault( c => c.GetParameters().Length == 0 );
         ThrowIfNotSuitableCtor( sourceAssembly, ctor );
         return ctor;
      }

      private static void ThrowIfNotSuitableCtor( String sourceAssembly, System.Reflection.ConstructorInfo ctor )
      {
         if ( ctor == null )
         {
            throw new Qi4CSBuildException( String.Format( "The given Qi4CS model factory type {0} with assembly {1} does not have a public parameterless constructor.", ctor.DeclaringType, sourceAssembly ) );
         }
      }

      /// <summary>
      /// This method will generate the Qi4CS assemblies.
      /// </summary>
      /// <param name="projectDir">The project directory.</param>
      /// <param name="targetFWID">The target framework identifier.</param>
      /// <param name="targetFWVersion">The target framework version.</param>
      /// <param name="targetFWProfile">The target framework profile.</param>
      /// <param name="referenceAssembliesDir">The base directory where target framework assemblies reside. Should be the directory where target framework assemblies are found under subdirectory <c>&lt;framework-id&gt;\&lt;framework-version&gt;</c>, e.g. <c>".NETFramework\v4.0"</c>.</param>
      /// <param name="targetPlatform">The textual name of target platform.</param>
      /// <param name="path">The path where to store assemblies.</param>
      /// <param name="assemblySNInfo">The file containing strongname information about the assemblies to be emitted.</param>
      /// <param name="qi4CSDir">The directory where Qi4CS assemblies actually used by the application reside.</param>
      /// <param name="parallelization">Whether to paralellize code generation.</param>
      /// <param name="verify">Whether to run PEVerify on generated Qi4CS assemblies.</param>
      /// <param name="winSDKDir">The directory where the Windows SDK resides, needed to detect PEVerify executable.</param>
      public IDictionary<String, String> GenerateAssemblies(
         String projectDir,
         String targetFWID,
         String targetFWVersion,
         String targetFWProfile,
         String referenceAssembliesDir,
         String targetPlatform,
         String path,
         String assemblySNInfo,
         String qi4CSDir,
         MSBuildCodeGenerationParallelization parallelization,
         Boolean verify,
         String winSDKDir
         )
      {
         qi4CSDir = Path.GetFullPath( qi4CSDir );
         path = Path.GetFullPath( path );

         XElement assemblyInfo = null;
         if ( !String.IsNullOrEmpty( assemblySNInfo ) )
         {
            try
            {
               assemblyInfo = XElement.Load( new StringReader( assemblySNInfo ) );
            }
            catch ( Exception exc )
            {
               throw new Qi4CSBuildException( "Invalid assembly info element " + assemblySNInfo + ".\n" + exc );
            }
         }
         Func<String, Stream> streamOpener = str => File.Open( str, FileMode.Open, FileAccess.Read, FileShare.Read );

         //referenceAssembliesDir = Path.Combine( referenceAssembliesDir, targetFWID, targetFWVersion );
         //if ( !String.IsNullOrEmpty( targetFWProfile ) )
         //{
         //   referenceAssembliesDir = Path.Combine( referenceAssembliesDir, "Profile", targetFWProfile );
         //}

         if ( !Directory.Exists( referenceAssembliesDir ) )
         {
            throw new Qi4CSBuildException( "The reference assemblies directory " + referenceAssembliesDir + " does not exist." );
         }

         //referenceAssembliesDir += Path.DirectorySeparatorChar;

         var isX86 = X86.Equals( targetPlatform, StringComparison.InvariantCultureIgnoreCase );
         var targetMachine = String.IsNullOrEmpty( targetPlatform ) || ANYCPU.Equals( targetPlatform, StringComparison.InvariantCultureIgnoreCase ) || isX86 ?
            ImageFileMachine.I386 :
            ImageFileMachine.AMD64; // TODO more machines
         var mFlags = ModuleFlags.ILOnly;
         if ( isX86 )
         {
            mFlags |= ModuleFlags.Required32Bit;
         }

         var snDic = new ConcurrentDictionary<String, Tuple<StrongNameKeyPair, AssemblyHashAlgorithm?>>();

         if ( assemblyInfo != null )
         {
            foreach ( var elem in assemblyInfo.XPathSelectElements( "assembly" ) )
            {
               snDic[elem.Attribute( "name" ).Value] = Tuple.Create(
                  SubElementTextOrFallback( elem, "sn", ( XElement snElem, out StrongNameKeyPair sn ) =>
                  {
                     var type = snElem.Attribute( "type" );
                     sn = "container".Equals( type.Value, StringComparison.InvariantCultureIgnoreCase ) ?
                        new StrongNameKeyPair( snElem.Value ) :
                        new StrongNameKeyPair( "inline".Equals( type.Value, StringComparison.InvariantCultureIgnoreCase ) ? snElem.Value.CreateHexBytes() : ReadAllBytes( projectDir, snElem.Value ) );
                     return true;
                  }, null ),
                  SubElementAttributeOrFallback( elem, "hashAlgorithm", ( String algoStr, out AssemblyHashAlgorithm? algo ) =>
                  {
                     AssemblyHashAlgorithm algoo;
                     var retVal = Enum.TryParse<AssemblyHashAlgorithm>( algoStr, out algoo );
                     algo = retVal ? algoo : (AssemblyHashAlgorithm?) null;
                     return retVal;
                  }, null ) );
            }
         }

         var runtimeRootDir = Path.GetDirectoryName( new Uri( typeof( Object ).Assembly.CodeBase ).LocalPath );


         var actualPath = path;
         var needToMove = verify && !String.Equals( actualPath, qi4CSDir );
         if ( needToMove )
         {
            // When verifying strong-named assemblies in different location than application's out dir, the .dll.config file
            // should hold recursively all non-.NET references, which is quite complicated and maybe not even enough.
            // Instead, emit Qi4CS assemblies into out dir and move them after verifying
            actualPath = qi4CSDir;
         }

         IDictionary<System.Reflection.Assembly, String> genAssFilenames;
         try
         {
            var cryptoCallbacks = new CryptoCallbacksDotNET();
            var loaderCallbacks = new CILMetaDataLoaderResourceCallbacksForFiles( referenceAssembliesDir, qi4CSDir );
            var thisFWMoniker = new TargetFrameworkInfo( targetFWID, targetFWVersion, targetFWProfile );
            using ( var loader = parallelization.HasFlag( MSBuildCodeGenerationParallelization.ParallelSaving ) ?
               (CILMetaDataLoaderWithCallbacks) new CILMetaDataLoaderThreadSafeConcurrentForFiles( callbacks: loaderCallbacks ) :
               new CILMetaDataLoaderNotThreadSafeForFiles( callbacks: loaderCallbacks ) )
            {
               var fwMapper = parallelization.HasFlag( MSBuildCodeGenerationParallelization.ParallelSaving ) ?
                  (TargetFrameworkMapper) new TargetFrameworkMapperConcurrent() :
                  new TargetFrameworkMapperNotThreadSafe();

               genAssFilenames = this._modelFactory.Model.GenerateAndSaveAssemblies(
                  (CodeGenerationParallelization) ( parallelization & MSBuildCodeGenerationParallelization.CodeGenMask ),
                  actualPath,
                  this.IsSilverlight,
                  ( nAss, gAss, eArgs ) =>
                  {
                     // Add target framework information
                     gAss.AddCustomAttribute(
                        gAss.ReflectionContext.NewWrapper( TARGET_FW_ATTR_CTOR ),
                        new[]
                        {
                           CILCustomAttributeFactory.NewTypedArgument( thisFWMoniker.ToString(), gAss.ReflectionContext )
                        },
                        null
                        );

                     eArgs.Headers.ModuleFlags = mFlags;
                     eArgs.Headers.Machine = targetMachine;

                     Tuple<StrongNameKeyPair, AssemblyHashAlgorithm?> snTuple;
                     snDic.TryGetValue( nAss.GetName().Name, out snTuple );
                     if ( snTuple != null )
                     {
                        eArgs.StrongName = snTuple.Item1;
                        eArgs.SigningAlgorithm = snTuple.Item2;
                     }
                  },
                  ( nAss, gAss, md ) =>
                  {
                     fwMapper.ChangeTargetFramework( md, loader, thisFWMoniker );
                  } );
            }
         }
         catch ( InvalidApplicationModelException apme )
         {
            throw new Qi4CSBuildException( "The Qi4CS model was not valid:\n" + apme.ValidationResult, apme );
         }

         if ( verify )
         {
            try
            {
               Qi4CS.Core.Runtime.Model.CodeGenUtils.DoPotentiallyInParallel( parallelization.HasFlag( MSBuildCodeGenerationParallelization.ParallelVerification ), genAssFilenames, fn =>
               {
                  try
                  {
                     Verification.RunPEVerify( null, fn.Value, snDic != null && snDic.ContainsKey( fn.Key.GetName().Name ) );
                  }
                  catch ( Exception e )
                  {
                     throw new Qi4CSBuildException( "Verification failed for " + fn.Value + ":\n" + e.Message + "." );
                  }
               } );
            }
            finally
            {
               if ( needToMove )
               {

                  if ( !Directory.Exists( path ) )
                  {
                     Directory.CreateDirectory( path );
                  }
                  var genAssFilenamesArray = genAssFilenames.ToArray();
                  Qi4CS.Core.Runtime.Model.CodeGenUtils.DoPotentiallyInParallel( parallelization.HasFlag( MSBuildCodeGenerationParallelization.ParallelTargetAssemblyCopying ), 0, genAssFilenamesArray.Length, idx =>
                  {
                     var kvp = genAssFilenamesArray[idx];
                     var fn = kvp.Value;
                     try
                     {
                        var targetFn = Path.Combine( path, Path.GetFileName( fn ) );
                        if ( !String.Equals( fn, targetFn ) )
                        {

                           if ( File.Exists( targetFn ) )
                           {
                              try
                              {
                                 File.Delete( targetFn );
                              }
                              catch
                              {
                                 // Ignore
                              }
                           }
                           File.Move( fn, targetFn );
                           genAssFilenamesArray[idx] = new KeyValuePair<System.Reflection.Assembly, String>( kvp.Key, targetFn );
                        }
                     }
                     catch
                     {
                        // Ignore
                     }
                  } );


                  genAssFilenames = genAssFilenamesArray.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
               }

            }
         }

         return genAssFilenames.ToDictionary(
            kvp => new Uri( kvp.Key.CodeBase ).LocalPath,
            kvp => kvp.Value
            );
      }

      private Boolean IsSilverlight
      {
         get
         {
            return !String.IsNullOrEmpty( this._slRuntimeDir );
         }
      }

      private static T SubElementTextOrFallback<T>( XElement element, String subElementName, ParsingDelegate<XElement, T> parser, T fallback )
      {
         var elem = element.XPathSelectElement( subElementName );
         T result;
         if ( elem == null || !parser( elem, out result ) )
         {
            result = fallback;
         }
         return result;
      }

      private static T SubElementAttributeOrFallback<T>( XElement element, String attributeName, ParsingDelegate<String, T> parser, T fallback )
      {
         var attr = element.Attribute( attributeName );
         T result;
         if ( attr == null || !parser( attr.Value, out result ) )
         {
            result = fallback;
         }
         return result;
      }

      private static Byte[] ReadAllBytes( String projectDir, String path )
      {
         path = Path.IsPathRooted( path ) ? path : Path.Combine( projectDir, path );
         try
         {
            return File.ReadAllBytes( path );
         }
         catch ( Exception exc )
         {
            throw new Qi4CSBuildException( "Error accessing file " + path + ". Message: " + exc.Message );
         }
      }
   }

   /// <summary>
   /// This exception will be thrown by <see cref="Qi4CSAssemblyGenerator"/> if target assembly is in some way unsuitable for Qi4CS code generaiton.
   /// </summary>
   //[Serializable]
   public class Qi4CSBuildException : Exception
   {

      /// <summary>
      /// Creates a new instance of <see cref="Qi4CSBuildException"/> with specified message.
      /// </summary>
      /// <param name="msg">The message.</param>
      /// <param name="inner">The inner exception, if any.</param>
      public Qi4CSBuildException( String msg, Exception inner = null )
         : base( msg, inner )
      {

      }
   }
}
