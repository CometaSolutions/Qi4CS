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
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Tests;
using System;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.Architectures.Assembling;

namespace Qi4CS.Tests.Core.Instance
{
   [Serializable]
   public abstract class AbstractSingletonInstanceTest : AbstractInstanceTest<SingletonArchitecture, SingletonApplicationModel, SingletonApplication>
   {
      protected override SingletonArchitecture CreateArchitecture()
      {
         return Qi4CSArchitectureFactory.NewSingletonArchitecture();
      }

      protected override void SetUpArchitecture( SingletonArchitecture architecture )
      {
         this.Assemble( architecture.CompositeAssembler );
      }

      protected override StructureServiceProvider GetStructureProvider( SingletonApplication application )
      {
         return application.StructureServices;
      }

      protected abstract void Assemble( Assembler assembler );
   }
}
