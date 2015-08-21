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
using NUnit.Framework;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using System;
using Qi4CS.Core.Bootstrap.Model;

namespace Qi4CS.Tests.Core.Architecture.Layered.Instance
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class UsesVisibilityTest : AbstractLayeredArchitectureInstanceTest
   {
      protected override void Assemble( LayeredCompositeAssembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) ).Done()
            .Use( new ModelWideUses() );
      }

      protected override LayeredApplicationModel CreateModel( LayeredArchitecture architecture )
      {
         architecture.Use( new ApplicationWideUses() );
         LayerArchitecture layer = architecture.GetOrCreateLayer( LAYER_NAME );
         layer.Use( new LayerWideUses() );
         ModuleArchitecture module = layer.GetOrCreateModule( MODULE_NAME );
         module.Use( new ModuleWideUses() );

         return base.CreateModel( architecture );
      }

      [Test]
      public void TestAllUsesWork()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewPlainCompositeBuilder<TestComposite>();
            builder.Builder.Use( new BuilderWideUses() );
            builder.Instantiate().PerformTest();
         } );
      }

      public class BuilderWideUses
      {

      }

      public class ModelWideUses
      {

      }

      public class ModuleWideUses
      {

      }

      public class LayerWideUses
      {

      }

      public class ApplicationWideUses
      {

      }

      public interface TestComposite
      {
         void PerformTest();
      }

      public class TestCompositeMixin : TestComposite
      {
#pragma warning disable 649

         [Uses]
         private ApplicationWideUses _appUses;

         [Uses]
         private LayerWideUses _layerUses;

         [Uses]
         private ModuleWideUses _moduleUses;

         [Uses]
         private ModelWideUses _modelUses;

         [Uses]
         private BuilderWideUses _builderUses;

#pragma warning restore 649

         #region TestComposite Members

         public virtual void PerformTest()
         {
            Assert.IsNotNull( this._appUses );
            Assert.IsNotNull( this._layerUses );
            Assert.IsNotNull( this._moduleUses );
            Assert.IsNotNull( this._modelUses );
            Assert.IsNotNull( this._builderUses );
         }

         #endregion
      }
   }
}
