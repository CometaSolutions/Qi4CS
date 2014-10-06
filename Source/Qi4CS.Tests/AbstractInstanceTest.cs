/*
 * Copyright 2011 Stanislav Muhametsin. All rights Reserved.
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
using System.Reflection;
using CollectionsWithRoles.API;
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Tests
{
   public class TestConstants
   {
      public const String APPLICATION_NAME = "TestApplication";
      public const String APPLICATION_MODE = "test";
      public const String APPLICATION_VERSION = null;
   }

   public class Qi4CSCodeGenHelper
   {
      public static CILAssemblyManipulator.API.EmittingArguments EmittingArgumentsCallback(
         Assembly original,
         CILAssemblyManipulator.API.CILAssembly generated
         )
      {
         var keyDir = System.IO.Path.Combine(
               System.IO.Path.GetDirectoryName( new Uri( typeof( Qi4CSCodeGenHelper ).Assembly.CodeBase ).LocalPath ),
               "..", "..", "..", "..", "..", "..", "..",
               "Keys"
               );
         var snPath = System.IO.Path.Combine(
            keyDir,
            original.GetName().Name.Equals( typeof( Qi4CSCodeGenHelper ).Assembly.GetName().Name ) ?
            "Qi4CS.Tests.Generation.snk" :
            "Qi4CS.Extensions.Generation.snk" );

         return CILAssemblyManipulator.API.EmittingArguments.CreateForEmittingDLL(
            new CILAssemblyManipulator.API.StrongNameKeyPair( System.IO.File.ReadAllBytes( snPath ) ),
               CILAssemblyManipulator.API.ImageFileMachine.I386,
               CILAssemblyManipulator.API.TargetRuntime.Net_4_0
            );
      }
   }

   [Serializable]
   public abstract class AbstractInstanceTest<ArchitectureType, ModelType, ApplicationType>
      where ApplicationType : ApplicationSPI
      where ModelType : class, ApplicationModel<ApplicationType>
      where ArchitectureType : ApplicationArchitecture<ApplicationModel<ApplicationSPI>>
   {
      protected static AppDomainEmulator _appDomain;

      private static ApplicationType _application;
      private static StructureServiceProvider _structureServices;
      private static ModelType _model;

      [SetUp]
      public virtual void SetUp()
      {
         _appDomain = Qi4CSTestUtils.CreateTestAppDomain( "Qi4CS Instance Test" );
         _appDomain.DoCallBack( () =>
         {
            ArchitectureType architecture = this.CreateArchitecture();
            this.SetUpArchitecture( architecture );
            var ass = Types.QI4CS_ASSEMBLY;
            _model = this.CreateModel( architecture );
            _model.GenerateAndSaveAssemblies( emittingInfoCreator: Qi4CSCodeGenHelper.EmittingArgumentsCallback );
            _application = _model.NewInstance( TestConstants.APPLICATION_NAME, TestConstants.APPLICATION_MODE, TestConstants.APPLICATION_VERSION );
            _structureServices = this.GetStructureProvider( _application );
            _application.Activate();
         } );
      }

      [TearDown]
      public virtual void TearDown()
      {
         try
         {
            _appDomain.DoCallBack( () =>
            {
               if ( _application != null )
               {
                  _application.Passivate();
               }
               _model = null;
               _structureServices = null;
            } );
         }
         finally
         {
            _appDomain.Dispose();
            _appDomain = null;
         }
      }

      protected abstract ArchitectureType CreateArchitecture();

      protected abstract void SetUpArchitecture( ArchitectureType architecture );

      protected abstract StructureServiceProvider GetStructureProvider( ApplicationType application );

      protected virtual ModelType CreateModel( ArchitectureType architecture )
      {
         return (ModelType) architecture.CreateModel();
      }

      protected ApplicationType Application
      {
         get
         {
            return _application;
         }
      }

      public StructureServiceProvider StructureServices
      {
         get
         {
            return _structureServices;
         }
      }

      protected ModelType Model
      {
         get
         {
            return _model;
         }
      }

      protected T NewPlainComposite<T>()
         where T : class
      {
         return this.StructureServices.NewCompositeBuilder<T>( CompositeModelType.PLAIN ).Instantiate();
      }

      protected T FindService<T>()
         where T : class
      {
         return this.StructureServices.FindService<T>().GetService();
      }

      protected ServiceReferenceInfo<T> FindServiceReference<T>()
         where T : class
      {
         return this.StructureServices.FindService<T>();
      }

      protected void PerformTestInAppDomain( Action action )
      {
         _appDomain.DoCallBack( action );
      }
   }
}
