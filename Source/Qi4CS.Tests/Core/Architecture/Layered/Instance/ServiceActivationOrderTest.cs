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
using Qi4CS.Core.Bootstrap.Assembling;
using NUnit.Framework;

namespace Qi4CS.Tests.Core.Architecture.Layered.Instance
{
   [Serializable]
   public class ServiceActivationOrderTest : AbstractLayeredArchitectureInstanceTest
   {
      private static readonly IList<String> ACTIVATIONS = new List<String>();
      private static readonly IList<String> PASSIVATIONS = new List<String>();

      private static readonly String LAYER_NAME_2 = LAYER_NAME + 2;
      private static readonly String LAYER_NAME_3 = LAYER_NAME + 3;

      protected override void Assemble( LayeredCompositeAssembler assembler )
      {
         var architecture = (LayeredArchitecture) assembler.ApplicationArchitecture;
         var layer1 = architecture.GetOrCreateLayer( LAYER_NAME );
         var module1 = layer1.GetOrCreateModule( MODULE_NAME );

         var layer2 = architecture.GetOrCreateLayer( LAYER_NAME_2 );
         var module2 = layer2.GetOrCreateModule( MODULE_NAME );

         var layer3 = architecture.GetOrCreateLayer( LAYER_NAME_3 );
         var module3 = layer3.GetOrCreateModule( MODULE_NAME );

         this.AddService( module1.CompositeAssembler, LAYER_NAME );
         this.AddService( module2.CompositeAssembler, LAYER_NAME_2 );
         this.AddService( module3.CompositeAssembler, LAYER_NAME_3 );

         layer1.UseLayers( layer2 );
         layer2.UseLayers( layer3 );
      }

      private void AddService( LayeredCompositeAssembler assembler, String serviceID )
      {
         ( (ServiceCompositeAssemblyDeclaration) assembler.NewLayeredService()
            .VisibleIn( Visibility.MODULE )
            .OfTypes( typeof( TestService ) ) )
            .SetActivateWithApplication( true )
            .Use( serviceID )
            ;
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            Assert.IsTrue( ACTIVATIONS.SequenceEqual( new String[] { LAYER_NAME_3, LAYER_NAME_2, LAYER_NAME } ) );
            this.Application.Passivate();
            Assert.IsTrue( PASSIVATIONS.SequenceEqual( new String[] { LAYER_NAME, LAYER_NAME_2, LAYER_NAME_3 } ) );
         } );
      }

      public class TestService
      {
         [Activate]
         protected void Activate( [Uses] String serviceID )
         {
            ACTIVATIONS.Add( serviceID );
         }

         [Passivate]
         protected void Passivate( [Uses] String serviceID )
         {
            PASSIVATIONS.Add( serviceID );
         }
      }
   }
}
