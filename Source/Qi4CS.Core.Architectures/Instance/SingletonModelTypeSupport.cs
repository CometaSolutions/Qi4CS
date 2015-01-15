/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using Qi4CS.Core.Architectures.Model;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Architectures.Instance
{
   internal sealed class SingletonPlainModelTypeInstanceScopeSupport : AbstractPlainModelTypeInstanceScopeSupport
   {
      public SingletonPlainModelTypeInstanceScopeSupport( SingletonPlainModelTypeModelScopeSupport modelScopeSupport )
         : base( modelScopeSupport )
      {
      }
   }

   internal sealed class SingletonServiceInstanceScopeSupport : AbstractServiceModelTypeInstanceScopeSupport
   {
      public SingletonServiceInstanceScopeSupport( SingletonServiceModelTypeModelScopeSupport modelScopeSupport )
         : base( modelScopeSupport )
      {

      }

      protected override ServiceContainer GetServiceContainerFor( CompositeInstanceStructureOwner structureOwner )
      {
         return SingletonApplicationImpl.GetServiceContainer( structureOwner );
      }

      protected override IEnumerable<CompositeInstanceStructureOwner> GetAllStructureOwners( ApplicationSPI application, Boolean isActivation )
      {
         return Enumerable.Repeat( (SingletonApplication) application, 1 );
      }

      protected override IEnumerable<CompositeModel> GetAllModels( CompositeInstanceStructureOwner structureOwner, Boolean isActivation )
      {
         return structureOwner.Application.ApplicationModel.CompositeModels.Values;
      }
   }

   //internal sealed class SingletonValueModelTypeInstanceScopeSupport : AbstractValueModelTypeInstanceScopeSupport
   //{
   //   public SingletonValueModelTypeInstanceScopeSupport( SingletonValueModelTypeModelScopeSupport modelScopeSupport )
   //      : base( modelScopeSupport )
   //   {
   //   }

   //}
}
