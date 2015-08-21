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
using NUnit.Framework;
using Qi4CS.Core.Architectures.Assembling;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Tests.Core.Architecture.Layered.Model
{
   [Serializable]
   public abstract class LayerUsageTest : AbstractLayeredArchitectureModelTest
   {
      public const String LAYER_1 = "Layer1";
      public const String LAYER_2 = "Layer2";
   }

   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class CyclicLayerDependencyTestClass : LayerUsageTest
   {
      protected override ApplicationArchitecture<ApplicationModel<ApplicationSPI>> CreateApplicationArchitecture()
      {
         var architecture = Qi4CSArchitectureFactory.NewLayeredArchitecture();
         var layer1 = architecture.GetOrCreateLayer( LAYER_1 );
         var layer2 = architecture.GetOrCreateLayer( LAYER_2 );
         layer1.UseLayers( layer2 );
         layer2.UseLayers( layer1 );
         return architecture;
      }

      [Test]
      public void CyclicLayerDependencyTest()
      {
         this.PerformTest( 0, 2, 0 );
      }
   }

   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class CyclicLayerDependencyTestWithOneLayerClass : LayerUsageTest
   {
      protected override ApplicationArchitecture<ApplicationModel<ApplicationSPI>> CreateApplicationArchitecture()
      {
         var architecture = Qi4CSArchitectureFactory.NewLayeredArchitecture();
         var layer1 = architecture.GetOrCreateLayer( LAYER_1 );
         layer1.UseLayers( layer1 );
         return architecture;
      }

      [Test]
      public void CyclicLayerDependencyTestWithOneLayer()
      {
         this.PerformTest( 0, 1, 0 );
      }
   }
}
