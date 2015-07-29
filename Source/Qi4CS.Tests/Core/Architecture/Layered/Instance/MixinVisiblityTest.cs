/*
 * Copyright (c) 2007, Rickard Öberg.
 * (org.qi4j.runtime.structure.MixinVisibilityTest)
 * See NOTICE file.
 * 
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
using NUnit.Framework;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Tests;
using Qi4CS.Core.SPI.Model;
using System.Reflection.Emit;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.Architectures.Assembling;

namespace Qi4CS.Tests.Core.Architecture.Layered.Instance
{
   [Serializable]
   public abstract class MixinVisiblityTest
   {
      public const String TEST_1_OK = "OK";
      public const String TEST_2_OK = "abc";
      public const String LAYER_2_NAME = "Layer2";
      public const String MODULE_2_NAME = "Module2";
      public const String MODULE_3_NAME = "Module3";

      private readonly Type _expectedException;

      protected MixinVisiblityTest()
         : this( null )
      {

      }

      protected MixinVisiblityTest( Type exceptionType )
      {
         this._expectedException = exceptionType;
      }

      [Test]
      public void PerformTest()
      {
         using ( var ad = Qi4CSTestUtils.CreateTestAppDomain( "Qi4CS Mixin Visiblity Test" ) )
         {
            ad.DoCallBack( () =>
            {
               Action a = () =>
               {
                  var architecture = Qi4CSArchitectureFactory.NewLayeredArchitecture();
                  var testPerformerLayer = architecture.GetOrCreateLayer( AbstractLayeredArchitectureInstanceTest.LAYER_NAME );
                  var mainAssembler = testPerformerLayer.GetOrCreateModule( AbstractLayeredArchitectureInstanceTest.MODULE_NAME ).CompositeAssembler;
                  mainAssembler.NewPlainComposite().OfTypes( typeof( TestPerformer ) ).WithMixins( typeof( TestPerformerMixin ) );
                  this.Assemble( architecture, testPerformerLayer, mainAssembler );
                  var model = architecture.CreateModel();
                  model.GenerateAndSaveAssemblies( CodeGeneration.CodeGenerationParallelization.NotParallel, logicalAssemblyProcessor: Qi4CSCodeGenHelper.EmittingArgumentsCallback );
                  var application = model.NewInstance( TestConstants.APPLICATION_NAME, TestConstants.APPLICATION_MODE, TestConstants.APPLICATION_VERSION );
                  application.Activate();
                  var performer = application.FindModule( AbstractLayeredArchitectureInstanceTest.LAYER_NAME, AbstractLayeredArchitectureInstanceTest.MODULE_NAME ).StructureServices.NewPlainCompositeBuilder<TestPerformer>().Instantiate();
                  Assert.AreEqual( TEST_1_OK, performer.Test1() );
                  Assert.AreEqual( TEST_2_OK, performer.Test2() );
               };
               if ( this._expectedException == null )
               {
                  a();
               }
               else
               {
                  Assert.Throws( this._expectedException, new TestDelegate( a ) );
               }
            } );
         }
      }

      protected abstract void Assemble( LayeredArchitecture architecture, LayerArchitecture testPerformerLayer, LayeredCompositeAssembler assembler );
   }

   [Serializable]
   public class TestMixinInModuleIsVisible : MixinVisiblityTest
   {
      protected override void Assemble( LayeredArchitecture architecture, LayerArchitecture testPerformerLayer, LayeredCompositeAssembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( B1Composite ) ).WithMixins( typeof( BMixin ) );
      }
   }

   [Serializable]
   public class TestMultipleMixinsInModuleWillFail : MixinVisiblityTest
   {
      public TestMultipleMixinsInModuleWillFail()
         : base( typeof( AmbiguousTypeException ) )
      {

      }

      protected override void Assemble( LayeredArchitecture architecture, LayerArchitecture testPerformerLayer, LayeredCompositeAssembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( B1Composite ) ).WithMixins( typeof( BMixin ) );
         assembler.NewPlainComposite().OfTypes( typeof( B2Composite ) ).WithMixins( typeof( BMixin ) );
      }
   }

   [Serializable]
   public class TestMixinInLayerIsNotVisible : MixinVisiblityTest
   {
      public TestMixinInLayerIsNotVisible()
         : base( typeof( NoSuchCompositeTypeException ) )
      {

      }

      protected override void Assemble( LayeredArchitecture architecture, LayerArchitecture testPerformerLayer, LayeredCompositeAssembler assembler )
      {
         testPerformerLayer.GetOrCreateModule( MODULE_2_NAME ).CompositeAssembler.NewPlainComposite().OfTypes( typeof( B1Composite ) ).WithMixins( typeof( BMixin ) );
      }
   }

   [Serializable]
   public class TestMixinInLayerIsVisible : MixinVisiblityTest
   {
      protected override void Assemble( LayeredArchitecture architecture, LayerArchitecture testPerformerLayer, LayeredCompositeAssembler assembler )
      {
         testPerformerLayer.GetOrCreateModule( MODULE_2_NAME ).CompositeAssembler.NewLayeredPlainComposite().VisibleIn( Visibility.LAYER ).OfTypes( typeof( B1Composite ) ).WithMixins( typeof( BMixin ) );
      }
   }

   [Serializable]
   public class TestMultipleMixinsInLayerWillFailSameModule : MixinVisiblityTest
   {
      public TestMultipleMixinsInLayerWillFailSameModule()
         : base( typeof( AmbiguousTypeException ) )
      {

      }

      protected override void Assemble( LayeredArchitecture architecture, LayerArchitecture testPerformerLayer, LayeredCompositeAssembler assembler )
      {
         testPerformerLayer.GetOrCreateModule( MODULE_2_NAME ).CompositeAssembler.NewLayeredPlainComposite().VisibleIn( Visibility.LAYER ).OfTypes( typeof( B1Composite ) ).WithMixins( typeof( BMixin ) );
         testPerformerLayer.GetOrCreateModule( MODULE_2_NAME ).CompositeAssembler.NewLayeredPlainComposite().VisibleIn( Visibility.LAYER ).OfTypes( typeof( B2Composite ) ).WithMixins( typeof( BMixin ) );
      }
   }

   [Serializable]
   public class TestMultipleMixinsInLayerWillFailDiffModule : MixinVisiblityTest
   {
      public TestMultipleMixinsInLayerWillFailDiffModule()
         : base( typeof( AmbiguousTypeException ) )
      {

      }

      protected override void Assemble( LayeredArchitecture architecture, LayerArchitecture testPerformerLayer, LayeredCompositeAssembler assembler )
      {
         testPerformerLayer.GetOrCreateModule( MODULE_2_NAME ).CompositeAssembler.NewLayeredPlainComposite().VisibleIn( Visibility.LAYER ).OfTypes( typeof( B1Composite ) ).WithMixins( typeof( BMixin ) );
         testPerformerLayer.GetOrCreateModule( MODULE_3_NAME ).CompositeAssembler.NewLayeredPlainComposite().VisibleIn( Visibility.LAYER ).OfTypes( typeof( B2Composite ) ).WithMixins( typeof( BMixin ) );
      }
   }

   [Serializable]
   public class TestMixinInLowerLayerIsVisible : MixinVisiblityTest
   {

      protected override void Assemble( LayeredArchitecture architecture, LayerArchitecture testPerformerLayer, LayeredCompositeAssembler assembler )
      {
         var layer2 = architecture.GetOrCreateLayer( LAYER_2_NAME );
         layer2.GetOrCreateModule( MODULE_2_NAME ).CompositeAssembler.NewLayeredPlainComposite().VisibleIn( Visibility.APPLICATION ).OfTypes( typeof( B1Composite ) ).WithMixins( typeof( BMixin ) );
         testPerformerLayer.UseLayers( layer2 );
      }
   }

   [Serializable]
   public class TestMixinInLowerLayerIsNotVisible : MixinVisiblityTest
   {
      public TestMixinInLowerLayerIsNotVisible()
         : base( typeof( NoSuchCompositeTypeException ) )
      {

      }

      protected override void Assemble( LayeredArchitecture architecture, LayerArchitecture testPerformerLayer, LayeredCompositeAssembler assembler )
      {
         var layer2 = architecture.GetOrCreateLayer( LAYER_2_NAME );
         layer2.GetOrCreateModule( MODULE_2_NAME ).CompositeAssembler.NewLayeredPlainComposite().VisibleIn( Visibility.LAYER ).OfTypes( typeof( B1Composite ) ).WithMixins( typeof( BMixin ) );
         testPerformerLayer.UseLayers( layer2 );
      }
   }

   public interface TestPerformer
   {
      String Test1();
      String Test2();
   }

   public class TestPerformerMixin : TestPerformer
   {
#pragma warning disable 649
      [Structure]
      private StructureServiceProvider _serviceProvider;
#pragma warning restore 649

      #region TestPerformer Members

      public virtual String Test1()
      {
         B1 instance = this._serviceProvider.NewPlainCompositeBuilder<B1>().Instantiate();
         return instance.Test();
      }

      public virtual String Test2()
      {
         var builder = this._serviceProvider.NewPlainCompositeBuilder<B2>();
         builder.Prototype().B2 = MixinVisiblityTest.TEST_2_OK;
         return builder.Instantiate().B2;
      }

      #endregion
   }

   public interface B1Composite : B1
   {

   }

   public interface B2Composite : B2
   {

   }

   public interface B2
   {
      [Optional]
      String B2 { get; set; }
   }

   public interface B1 : B2
   {
      String Test();
   }

   public abstract class BMixin : B1
   {

      #region B1 Members

      public virtual String Test()
      {
         return MixinVisiblityTest.TEST_1_OK;
      }

      #endregion

      #region B2 Members

      public abstract String B2 { get; set; }

      #endregion
   }
}
