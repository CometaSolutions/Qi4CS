/*
 * Copyright 2012 Stanislav Muhametsin. All rights Reserved.
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
using System.Linq;
using System.Text;
using Qi4CS.Tests.Core.Instance;
using System.Net;
using Qi4CS.Core.API.Model;
using System.Xml.Linq;
using System.IO;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using NUnit.Framework;
using System.Threading;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.Architectures.Assembling;
using Qi4CS.Extensions.Configuration.Instance;
using System.Reflection;
using Qi4CS.Extensions.Configuration.XML;

namespace Qi4CS.Tests.Extensions.Configuration
{
   [Serializable, Category( "EXTENSIONS.CONFIG" )]
   public class ConfigurationTest
   {
      private const String CONFIG_FILE_NAME = @"..\..\..\..\test_db_setup.xml";

      private static readonly String CONFIG_FILE_FULL_PATH = Path.Combine( Path.GetDirectoryName( new Uri( typeof( ConfigurationTest ).Assembly.CodeBase ).LocalPath ), CONFIG_FILE_NAME );

      //private static Func<StructureServiceProvider, Schema<SchemaQ<SchemaIQ>, SchemaIQ>> IP_END_POINT_SCHEMA_CREATION_FUNC = ssp =>
      //{
      //   var schemaFactory = ssp.FindService<SchemaFactory>().GetService();
      //   var acceptorFactory = ssp.FindService<AcceptorFactory>().GetService();

      //   var IPEPSchema = schemaFactory.CreateDictionarySchema();
      //   var nonEmptyStringSchema = schemaFactory.CreateLeafSchema<StringObjectIQ>();
      //   nonEmptyStringSchema.Acceptor = acceptorFactory.CreateNonEmpty();
      //   var nonNullIntSchema = schemaFactory.CreateLeafSchema<Int32ObjectIQ>();
      //   nonNullIntSchema.Acceptor = acceptorFactory.CreateAcceptNonNull();
      //   IPEPSchema.AddSchemaForKey( IPEndPointTransformation.ADDRESS_NAME, nonEmptyStringSchema );
      //   IPEPSchema.AddSchemaForKey( IPEndPointTransformation.PORT_NAME, nonNullIntSchema );

      //   return IPEPSchema;
      //};

      private static StructureServiceProvider _ssp;

      private static AppDomainEmulator _appDomain;

      [SetUp]
      public void SetUp()
      {
         _appDomain = Qi4CSTestUtils.CreateTestAppDomain( "Qi4CS Configuration test." );
         _appDomain.DoCallBack( () =>
         {
            // Typical usecase scenario, first create architecture
            var architecture = Qi4CSArchitectureFactory.NewLayeredArchitecture();

            // Add layer for all configuration composites
            var configLayer = architecture.GetOrCreateLayer( "ConfigLayer" );

            // Add a module for all configuration composites
            var configModule = configLayer.GetOrCreateModule( "ConfigModule" );

            // Assembler for configuration module
            var assembler = configModule.CompositeAssembler;

            var customSerializers = new List<XMLConfigurationSerializerHelper>();

            // IPEndPoint is not a Qi4CS composite - add custom (de)serialization support for it
            customSerializers.Add( new XMLConfigurationSerializerWithCallbacks(
               ( obj, type ) => typeof( IPEndPoint ).Equals( type ),
               ( obj, type, parent ) => parent.Add( new XElement( "Address", ( (IPEndPoint) obj ).Address.ToString() ), new XElement( "Port", ( (IPEndPoint) obj ).Port.ToString() ) ),
               ( element, type ) => typeof( IPEndPoint ).Equals( type ),
               ( element, type ) =>
               {
                  var addressString = element.Element( "Address" ).Value;
                  IPAddress address;
                  if ( !IPAddress.TryParse( addressString, out address ) )
                  {
                     address = Dns.GetHostEntry( addressString ).AddressList[0];
                  }
                  return new IPEndPoint( address, Int32.Parse( element.Element( "Port" ).Value ) );
               }
               ) );

            // Add serialization composite
            assembler.AddXMLSerializationSupport( null, customSerializers );
            // Add composites part of the configuration
            assembler
               .NewPlainComposite()
               .OfTypes( typeof( DatabaseConfiguration ) );
            assembler
               .NewLayeredPlainComposite()
               .VisibleIn( Visibility.MODULE )
               .OfTypes( typeof( DatabaseSetup ) );

            // Add support for configuration service and instances
            assembler.AddSupportForAllConfigurationInstancesAndManager()
               .WithDefaultsFor( typeof( DatabaseConfiguration ) ) // Set default values for DatabaseConfiguration
               .SerializedByXML() // Make DatabaseConfiguration (de)serialization process use XMLConfigurationSerializer
               .LocatedInXMLDocument( CONFIG_FILE_FULL_PATH ); // Default location for DatabaseConfiguration

            // Add the test composite
            var testLayer = architecture.GetOrCreateLayer( "TestLayer" );
            var testModule = testLayer.GetOrCreateModule( "TestModule" );
            assembler = testModule.CompositeAssembler;
            assembler.NewLayeredPlainComposite().OfTypes( typeof( CompositeUsingConfiguration ) ).WithMixins( typeof( CompositeUsingConfigurationMixin ) );

            testLayer.UseLayers( configLayer );

            var model = architecture.CreateModel();
            model.GenerateAndSaveAssemblies( CodeGeneration.CodeGenerationParallelization.NotParallel, logicalAssemblyProcessor: Qi4CSCodeGenHelper.EmittingArgumentsCallback );
            var application = model.NewInstance( TestConstants.APPLICATION_NAME, TestConstants.APPLICATION_MODE, TestConstants.APPLICATION_VERSION );
            _ssp = assembler.GetStructureServiceProvider( application );
            application.Activate();
         } );
         //var configSSP = configModule.CompositeAssembler.GetStructureServiceProvider( application );
         //var suckaObjBuilder = configSSP.NewPlainCompositeBuilder<DatabaseConfiguration>();
         //suckaObjBuilder.Prototype().DatabaseSetups = new Dictionary<String, DatabaseSetup>();
         //var localConnBuilder = configSSP.NewPlainCompositeBuilder<DatabaseSetup>();
         //localConnBuilder.Prototype().DatabaseName = "SomeDB";
         //localConnBuilder.Prototype().DatabaseConnectionInformation = new IPEndPoint( IPAddress.Loopback, 1000 );
         //suckaObjBuilder.Prototype().DatabaseSetups.Add( "Local", localConnBuilder.Instantiate() );

         //var sucka = new XElement( "DatabaseConfiguration" );
         //configSSP.FindService<XMLSerializationService>().Service.Serialize( suckaObjBuilder.Instantiate(), sucka );
      }

      [TearDown]
      public void TearDown()
      {
         _appDomain.DoCallBack( () =>
         {
            _ssp.Application.Passivate();
         } );
         _appDomain.Dispose();
         _appDomain = null;
      }

      [Test]
      public void PerformTest()
      {
         _appDomain.DoCallBack( () =>
         {
            var composite = _ssp.NewPlainCompositeBuilder<CompositeUsingConfiguration>().Instantiate();
            //var setupFromFile = composite.SetupFromXMLFile;
            var setupFromConfig = composite.SetupFromConfigInjection;

            // TODO proper value types to make .Equals easier
            Assert.IsTrue( setupFromConfig.DatabaseSetups.Count == 1 );
            Assert.IsTrue( setupFromConfig.DatabaseSetups.ContainsKey( "Local" ) );
            var dbConn = setupFromConfig.DatabaseSetups["Local"];

            //Assert.IsTrue( String.Equals( dbConn.DatabaseConnectionInformation.Address.ToString(), "127.0.0.1" ) || String.Equals( dbConn.DatabaseConnectionInformation.Address.ToString(), "::1" ) );
            Assert.AreEqual( dbConn.DatabaseConnectionInformation.Address.ToString(), "127.0.0.1" );
            Assert.AreEqual( dbConn.DatabaseConnectionInformation.Port, 1000 );
            Assert.AreEqual( dbConn.DatabaseName, "SomeDB" );

            composite = null;
            //setupFromFile = null;
            setupFromConfig = null;

            // TODO write new xml file and load from it and test if they are the same.
         } );
      }

      [Test]
      [Ignore( "Need to make DotNET version of Qi4CS.Extensions.Configuration for this, OR don't use file watcher to check file, and use separate syncing thread." )]
      public void TestThatConfigurationStaysInSync()
      {
         _appDomain.DoCallBack( () =>
         {
            var configFileContent = XElement.Load( CONFIG_FILE_FULL_PATH );
            try
            {
               var configManager = _ssp.FindService<ConfigurationManager>().GetService();
               var instance = configManager.Create<DatabaseConfiguration>();

               Assert.AreEqual( instance.Configuration.DatabaseSetups["Local"].DatabaseName, "SomeDB" );
               var newConfigFileContent = XElement.Load( CONFIG_FILE_FULL_PATH );
               newConfigFileContent.Descendants( "DatabaseName" ).First().Nodes().OfType<XText>().First().Value = "AnotherDB";
               newConfigFileContent.Save( CONFIG_FILE_FULL_PATH );
               Thread.Sleep( 100 );
               Assert.AreEqual( instance.Configuration.DatabaseSetups["Local"].DatabaseName, "AnotherDB" );

               // Run GC to free file system watchers.
               System.GC.Collect();
            }
            finally
            {
               configFileContent.Save( CONFIG_FILE_FULL_PATH );
            }
         } );
      }

      [Immutable]
      public interface DatabaseConfiguration
      {
         IDictionary<String, DatabaseSetup> DatabaseSetups { get; set; }
      }

      [Immutable]
      public interface DatabaseSetup
      {
         IPEndPoint DatabaseConnectionInformation { get; set; }

         String DatabaseName { get; set; }
      }

      public interface CompositeUsingConfiguration
      {
         DatabaseConfiguration SetupFromConfigInjection { get; }
         //DatabaseConfiguration SetupFromXMLFile { get; }
      }

      public class CompositeUsingConfigurationMixin : CompositeUsingConfiguration
      {
#pragma warning disable 649

         //[Service]
         //private XMLSerializationService _xmlSerialization;

         [This]
         private CompositeUsingConfigurationState _state;

#pragma warning restore 649

         #region CompositeUsingConfiguration Members

         public virtual DatabaseConfiguration SetupFromConfigInjection
         {
            get
            {
               return this._state.Configuration.Configuration;
            }
         }

         //public virtual DatabaseConfiguration SetupFromXMLFile
         //{
         //   get
         //   {
         //      DatabaseConfiguration config;
         //      this._xmlSerialization.Deserialize( XElement.Load( CONFIG_FILE_FULL_PATH ), out config );
         //      return config;
         //   }
         //}

         #endregion

         [Prototype]
         protected void InitState( [Service] ConfigurationManager confManager )
         {
            this._state.Configuration = confManager.Create<DatabaseConfiguration>();
         }
      }

      public interface CompositeUsingConfigurationState
      {
         ConfigurationInstance<DatabaseConfiguration> Configuration { get; set; }
      }
   }
}
