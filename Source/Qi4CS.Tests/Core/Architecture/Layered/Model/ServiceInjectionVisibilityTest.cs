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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Architectures.Assembling;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Tests.Core.Architecture.Layered.Model
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class ServiceInjectionVisibilityTest : AbstractLayeredArchitectureModelTest
   {
      public const String LAYER_NAME = "Layer";
      public const String MODULE_1_NAME = "Module1";
      public const String MODULE_2_NAME = "Module2";

      protected override ApplicationArchitecture<ApplicationModel<ApplicationSPI>> CreateApplicationArchitecture()
      {
         var architecture = Qi4CSArchitectureFactory.NewLayeredArchitecture();
         var layer = architecture.GetOrCreateLayer( LAYER_NAME );
         var module1 = layer.GetOrCreateModule( MODULE_1_NAME );
         module1.CompositeAssembler.NewLayeredService().VisibleIn( Visibility.MODULE ).OfTypes( typeof( ServiceComposite ) );

         var module2 = layer.GetOrCreateModule( MODULE_2_NAME );
         module2.CompositeAssembler.NewLayeredPlainComposite().OfTypes( typeof( TransientComposite ) ).WithMixins( typeof( TransientMixin ) );
         return architecture;
      }

      [Test]
      public void TestServiceInjectionValidation()
      {
         this.PerformTest( 1, 0, 0 );
      }

      public interface ServiceComposite
      {

      }

      public interface TransientComposite
      {

      }

      public class TransientMixin : TransientComposite
      {
#pragma warning disable 649, 169

         [Service]
         private ServiceComposite _service;

#pragma warning restore 649, 169
      }
   }
}
