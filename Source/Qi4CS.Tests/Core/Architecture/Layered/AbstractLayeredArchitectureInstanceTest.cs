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
using Qi4CS.Tests;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.Architectures.Assembling;

namespace Qi4CS.Tests.Core.Architecture.Layered
{
   [Serializable]
   public abstract class AbstractLayeredArchitectureInstanceTest : AbstractInstanceTest<LayeredArchitecture, LayeredApplicationModel, LayeredApplication>
   {
      public const String LAYER_NAME = "Layer";
      public const String MODULE_NAME = "Module";

      protected override LayeredArchitecture CreateArchitecture()
      {
         return Qi4CSArchitectureFactory.NewLayeredArchitecture();
      }

      protected override void SetUpArchitecture( LayeredArchitecture architecture )
      {
         this.Assemble( architecture.GetOrCreateLayer( LAYER_NAME ).GetOrCreateModule( MODULE_NAME ).CompositeAssembler );
      }

      protected abstract void Assemble( LayeredCompositeAssembler assembler );

      protected override StructureServiceProvider GetStructureProvider( LayeredApplication application )
      {
         return application.FindModule( LAYER_NAME, MODULE_NAME ).StructureServices;
      }
   }
}
