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
using Qi4CS.Core.Architectures.Assembling;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Tests;

namespace Qi4CS.Tests.Core.Model
{
   [Serializable]
   public abstract class AbstractSingletonModelTest : AbstractModelTest
   {

      protected override ApplicationArchitecture<ApplicationModel<ApplicationSPI>> CreateApplicationArchitecture()
      {
         var architecture = Qi4CSArchitectureFactory.NewSingletonArchitecture();
         Type compositeType = null;
         Type[] mixins = null, concerns = null, sideEffects = null;
         this.SetupApplicationArchitecture( ref compositeType, ref mixins, ref concerns, ref sideEffects );
         var decl = architecture.CompositeAssembler.NewPlainComposite().OfTypes( compositeType );
         if ( mixins != null )
         {
            decl.WithMixins( mixins );
         }
         if ( concerns != null )
         {
            decl.WithConcerns( concerns );
         }
         if ( sideEffects != null )
         {
            decl.WithSideEffects( sideEffects );
         }
         return architecture;

      }

      protected abstract void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects );
   }
}
