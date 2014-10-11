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
using CILAssemblyManipulator.API;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Diagnostics;
using Microsoft.Build.Framework;
using System.Collections.Concurrent;
using CILAssemblyManipulator.DotNET;

namespace Qi4CS.CodeGeneration.MSBuild
{
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
      /// Gets or sets the full file names of generated Qi4CS assemblies.
      /// </summary>
      /// <value>The full file names of generated Qi4CS assemblies.</value>
      [Output]
      public String[] GeneratedFilenames { get; set; }

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

                  this.Log.LogMessage( Microsoft.Build.Framework.MessageImportance.High, "Generating Qi4CS assemblies for {0}, target directory is {1}, verifying: {2}.", sourceAss, assDir, this.PerformVerify );

                  var oldCurDir = Environment.CurrentDirectory;
                  Environment.CurrentDirectory = Path.GetDirectoryName( sourceAss );

                  try
                  {
                     var generator = new Qi4CSAssemblyGenerator( sourceAss, this.ModelFactory );// (Qi4CSAssemblyGenerator) ad.CreateInstanceFromAndUnwrap( thisAssemblyPath, typeof( Qi4CSAssemblyGenerator ).FullName, false, System.Reflection.BindingFlags.CreateInstance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new Object[] { sourceAss, this.ModelFactory }, null, null );
                     //this.BuildEngine3.Yield();
                     try
                     {
                        this.GeneratedFilenames = generator.GenerateAssemblies(
                           projectDir,
                           this.TargetFW,
                           this.TargetFWVersion,
                           this.TargetFWProfile,
                           this.ReferenceAssembliesDir,
                           this.TargetPlatform,
                           assDir,
                           this.AssemblyInformation,
                           Path.GetDirectoryName( sourceAss ),
                           this.PerformVerify,
                           this.WindowsSDKDir
                           );
                        retVal = true;
                     }
                     finally
                     {
                        //this.BuildEngine3.Reacquire();
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

      // TODO parametrize this
      private const String PCL_FW_NAME = FrameworkMonikerInfo.DEFAULT_PCL_FW_NAME;

      private delegate Boolean ParsingDelegate<TIn, TOut>( TIn element, out TOut result );

      private readonly Qi4CSModelProvider<ApplicationModel<ApplicationSPI>> _modelFactory;

      /// <summary>
      /// Creates new instance of <see cref="Qi4CSAssemblyGenerator"/>.
      /// </summary>
      /// <param name="sourceAssembly">The path to the assembly containing type implementing <see cref="Qi4CSModelProvider{T}"/>. This will be given to <see cref="System.Reflection.Assembly.LoadFrom(String)"/> method.</param>
      /// <param name="modelFactoryName">The name of the type within the assembly which implements <see cref="Qi4CSModelProvider{T}"/> and has a public parameterless constructor. If <c>null</c>, then first suitable type, in unspecified order, will be selected.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="sourceAssembly"/> is <c>null</c>.</exception>
      /// <exception cref="Qi4CSBuildException">If <paramref name="modelFactoryName"/> is <c>null</c>, then this is thrown when no suitable type is found. Otherwise this is thrown when the specified type is not found within the specified assembly, or when the type is not suitable.</exception>
      /// <remarks>
      /// Additionally any exception thrown by <see cref="System.Reflection.Assembly.LoadFrom(String)"/> is passed directly through to the caller of this constructor.
      /// </remarks>
      public Qi4CSAssemblyGenerator( String sourceAssembly, String modelFactoryName )
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
         var mfType = mfNameGiven ?
            ass.GetType( modelFactoryName, false, false ) :
            ass.GetTypes().FirstOrDefault( t => IsSuitableType( t ) && HasSuitableCtor( t ) );
         if ( mfType == null )
         {
            throw new Qi4CSBuildException( mfNameGiven ?
               String.Format( "The Qi4CS model factory type {0} is not found within assembly {1}", modelFactoryName, ass ) :
               String.Format( "The assembly {0} does not contain suitable Qi4CS model factory type (which should implement {1} and have a public parameterless constructor)", ass, typeof( Qi4CSModelProvider<> ) )
               );
         }
         else if ( mfNameGiven )
         {
            if ( !IsSuitableType( mfType ) )
            {
               throw new Qi4CSBuildException( String.Format( "The given Qi4CS model factory type {0} with assembly {1} does not implement {2}", mfType, ass, typeof( Qi4CSModelProvider<> ) ) );
            }
         }
         var ctor = GetSuitableCtor( sourceAssembly, mfType );
         if ( ctor == null )
         {
            throw new Qi4CSBuildException( String.Format( "The Qi4CS model factory type {0} must have a public parameterless constructor.", mfType ) );
         }

         this._modelFactory = (Qi4CSModelProvider<ApplicationModel<ApplicationSPI>>) ctor.Invoke( Type.EmptyTypes );
      }

      //private System.Reflection.Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
      //{
      //   CILAssemblyName an;
      //   System.Reflection.Assembly result = null;
      //   if ( CILAssemblyName.TryParse( args.Name, out an ) )
      //   {
      //      var pathFragment = Path.Combine( Path.GetDirectoryName( new Uri( args.RequestingAssembly.CodeBase ).LocalPath ), an.Name );
      //      var suitablePath = File.Exists( pathFragment + ".dll" ) ?
      //         ( pathFragment + ".dll" ) :
      //         ( File.Exists( pathFragment + ".exe" ) ?
      //         ( pathFragment + ".exe" ) : null );
      //      if ( suitablePath != null )
      //      {
      //         result = System.Reflection.Assembly.LoadFrom( suitablePath );
      //      }
      //   }
      //   return result;
      //}

      private static Boolean IsSuitableType( Type t )
      {
         return t.IsClass
            && t.GetInterfaces().Any( i => i.IsGenericType && typeof( Qi4CSModelProvider<> ).Equals( i.GetGenericTypeDefinition() ) );
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
      /// <param name="verify">Whether to run PEVerify on generated Qi4CS assemblies.</param>
      /// <param name="winSDKDir">The directory where the Windows SDK resides, needed to detect PEVerify executable.</param>
      public String[] GenerateAssemblies( String projectDir, String targetFWID, String targetFWVersion, String targetFWProfile, String referenceAssembliesDir, String targetPlatform, String path, String assemblySNInfo, String qi4CSDir, Boolean verify, String winSDKDir )
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

         referenceAssembliesDir = Path.Combine( referenceAssembliesDir, targetFWID, targetFWVersion );
         if ( !String.IsNullOrEmpty( targetFWProfile ) )
         {
            referenceAssembliesDir = Path.Combine( referenceAssembliesDir, "Profile", targetFWProfile );
         }

         String msCorLibName; String fwDisplayName; String targetFWDir;
         var thisFWMoniker = new FrameworkMonikerInfo( targetFWID, targetFWVersion, targetFWProfile, DotNETReflectionContext.ReadAssemblyInformationFromRedistXMLFile( Path.Combine( referenceAssembliesDir, "RedistList", "FrameworkList.xml" ), out msCorLibName, out fwDisplayName, out targetFWDir ), msCorLibName, fwDisplayName );

         if ( !String.IsNullOrEmpty( targetFWDir ) )
         {
            referenceAssembliesDir = targetFWDir;
         }

         if ( !Directory.Exists( referenceAssembliesDir ) )
         {
            throw new Qi4CSBuildException( "The reference assemblies directory " + referenceAssembliesDir + " does not exist." );
         }

         referenceAssembliesDir += Path.DirectorySeparatorChar;

         var isX86 = X86.Equals( targetPlatform, StringComparison.InvariantCultureIgnoreCase );
         var targetMachine = String.IsNullOrEmpty( targetPlatform ) || ANYCPU.Equals( targetPlatform, StringComparison.InvariantCultureIgnoreCase ) || isX86 ?
            ImageFileMachine.I386 :
            ImageFileMachine.AMD64; // TODO more machines
         var mFlags = ModuleFlags.ILOnly;
         if ( isX86 )
         {
            mFlags |= ModuleFlags.Required32Bit;
         }

         var snDic = new ConcurrentDictionary<String, Tuple<StrongNameKeyPair, AssemblyHashAlgorithm>>();

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
                        new StrongNameKeyPair( "inline".Equals( type.Value, StringComparison.InvariantCultureIgnoreCase ) ? StringConversions.HexStr2ByteArray( snElem.Value ) : ReadAllBytes( projectDir, snElem.Value ) );
                     return true;
                  }, null ),
                  SubElementAttributeOrFallback( elem, "hashAlgorithm", ( String algoStr, out AssemblyHashAlgorithm algo ) =>
                  {
                     return Enum.TryParse<AssemblyHashAlgorithm>( algoStr, out algo );
                  }, AssemblyHashAlgorithm.SHA1 ) );
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
            genAssFilenames = this._modelFactory.Model.GenerateAndSaveAssemblies(
            actualPath,
            IsSilverlight( targetFWID ),
            ( nAss, gAss ) =>
            {
               Tuple<StrongNameKeyPair, AssemblyHashAlgorithm> snTuple;
               snDic.TryGetValue( nAss.GetName().Name, out snTuple );
               var sn = snTuple == null ? null : snTuple.Item1;
               var eArgs = EmittingArguments.CreateForEmittingWithMoniker( gAss.ReflectionContext, targetMachine, TargetRuntime.Net_4_0, ModuleKind.Dll, null, runtimeRootDir, referenceAssembliesDir, streamOpener, sn, thisFWMoniker, String.Equals( thisFWMoniker.FrameworkName, PCL_FW_NAME ), mFlags );

               gAss.AddTargetFrameworkAttributeWithMonikerInfo( thisFWMoniker, eArgs.AssemblyMapper );
               if ( snTuple != null )
               {
                  eArgs.SigningAlgorithm = snTuple.Item2;
               }
               return eArgs;
            } );
         }
         catch ( InvalidApplicationModelException apme )
         {
            throw new Qi4CSBuildException( "The Qi4CS model was not valid:\n" + apme.ValidationResult, apme );
         }

         if ( verify )
         {
            try
            {
               winSDKDir = FindWinSDKBinPath( winSDKDir );
               foreach ( var fn in genAssFilenames )
               {
                  Verify( winSDKDir, fn.Value, snDic != null && snDic.ContainsKey( fn.Key.GetName().Name ) );
               }
            }
            finally
            {
               if ( !Directory.Exists( path ) )
               {
                  Directory.CreateDirectory( path );
               }

               foreach ( var fn in genAssFilenames.Values )
               {
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
                     }
                  }
                  catch
                  {
                     // Ignore
                  }
               }
            }
         }

         return genAssFilenames.Values.ToArray();
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

      private static String FindWinSDKBinPath( String winSDKDir )
      {
         String binPath;
         if ( !String.IsNullOrEmpty( winSDKDir ) && Directory.Exists( winSDKDir ) )
         {
            binPath = Path.Combine( winSDKDir, "bin" );
            if ( Directory.Exists( winSDKDir ) )
            {
               // The PEVerify may be right here, in v7.1A and older, or it may reside in subdirectory, in v8.0A or newer
               // The directly found PEVerify might be using too old runtime, so prefer the one in subdirectory
               binPath = Directory.EnumerateDirectories( binPath ).FirstOrDefault( bp => File.Exists( Path.Combine( bp, PEVERIFY_EXE ) ) );
               if ( binPath == null )
               {
                  if ( !File.Exists( Path.Combine( binPath, PEVERIFY_EXE ) ) )
                  {
                     throw new Qi4CSBuildException( "Failed to detect path to PEVerify based on Windows SDK directory " + winSDKDir + "." );
                  }
               }
            }
            else
            {
               throw new Qi4CSBuildException( "The 'bin' subdirectory did not exist in " + binPath + "." );
            }
         }
         else
         {
            throw new Qi4CSBuildException( "The verification of generated assemblies is on, but " +
               ( String.IsNullOrEmpty( winSDKDir ) ? "no path to Windows SDK was specified" : ( "the specified path " + winSDKDir + " does not exist" ) ) +
               ". Make sure there is Windows SDK installed on the machine." );
         }
         return binPath;
      }

      private static Boolean IsSilverlight( String targetFW )
      {
         return "Silverlight".Equals( targetFW, StringComparison.InvariantCultureIgnoreCase ) ||
            "WindowsPhone".Equals( targetFW, StringComparison.InvariantCultureIgnoreCase );
      }

      private static void Verify( String winSDKBinDir, String fileName, Boolean verifyStrongName )
      {
         fileName = Path.GetFullPath( fileName );
         var peVerifyPath = Path.Combine( winSDKBinDir, PEVERIFY_EXE );

         var validationPath = Path.GetDirectoryName( fileName );

         // Call PEVerify
         var startInfo = new ProcessStartInfo();
         startInfo.FileName = peVerifyPath;
         // Ignore loading direct pointer to delegate ctors.
         startInfo.Arguments = "/IL /MD /VERBOSE /NOLOGO /HRESULT /IGNORE=0x80131861" + " \"" + fileName + "\"";
         startInfo.CreateNoWindow = true;
         startInfo.WorkingDirectory = validationPath;
         startInfo.RedirectStandardOutput = true;
         startInfo.RedirectStandardError = true;
         startInfo.UseShellExecute = false;
         var process = Process.Start( startInfo );

         // First 'read to end', only then wait for exit.
         // Otherwise, might get stuck (forgot the link to StackOverflow which explained this).
         var results = process.StandardOutput.ReadToEnd();
         process.WaitForExit();

         if ( !results.StartsWith( "All Classes and Methods in " + fileName + " Verified." ) )
         {
            throw new Qi4CSBuildException( "PEVerify detected the following errors in " + fileName + ":\n" + results );
         }

         if ( verifyStrongName )
         {
            startInfo.FileName = Path.Combine( winSDKBinDir, "sn.exe" );
            if ( File.Exists( startInfo.FileName ) )
            {
               startInfo.Arguments = "-vf \"" + fileName + "\"";
               process = Process.Start( startInfo );

               results = process.StandardOutput.ReadToEnd();
               process.WaitForExit();

               var lines = results.Split( new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries );

               if ( !( lines.Length == 3 && String.Equals( lines[2], "Assembly '" + fileName + "' is valid" ) ) )
               {
                  throw new Qi4CSBuildException( "Strong name validation detected the following errors in " + fileName + ":\n" + results );
               }

            }
            else
            {
               throw new Qi4CSBuildException( "The strong name utility sn.exe is not in same path as " + PEVERIFY_EXE + "." );
            }
         }
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
