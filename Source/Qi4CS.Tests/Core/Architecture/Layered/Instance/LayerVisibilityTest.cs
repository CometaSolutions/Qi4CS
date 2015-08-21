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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using NUnit.Framework;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Architecture.Layered.Instance
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class LayerVisibilityTest : AbstractLayeredArchitectureInstanceTest
   {
      protected override void Assemble( LayeredCompositeAssembler assembler )
      {
         assembler.NewLayeredService().VisibleIn( Visibility.APPLICATION ).OfTypes( typeof( TestService ) );
         var architecture = (LayeredArchitecture) assembler.ApplicationArchitecture;
         var serviceLayer = architecture.GetOrCreateLayer( LAYER_NAME );
         var midLayer = architecture.GetOrCreateLayer( "MidLayer" );
         var transientLayer = architecture.GetOrCreateLayer( "TransientLayer" );
         transientLayer.GetOrCreateModule( "TransientModule" ).CompositeAssembler.NewPlainComposite().OfTypes( typeof( TestComposite ) );

         transientLayer.UseLayers( midLayer );
         midLayer.UseLayers( serviceLayer );
      }


      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.Application.FindModule( "TransientLayer", "TransientModule" ).StructureServices.NewPlainCompositeBuilder<TestComposite>().Instantiate();
            composite.DoSomething();
         } );
      }

      public interface TestService
      {

      }

      public class TestComposite
      {
#pragma warning disable 649

         [Service]
         private TestService _service;

#pragma warning restore 649

         public virtual void DoSomething()
         {
            Assert.IsNotNull( this._service );
         }
      }
   }
}
