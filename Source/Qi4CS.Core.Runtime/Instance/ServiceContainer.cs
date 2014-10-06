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
using Qi4CS.Core.Runtime.Common;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Instance
{
   public interface ServiceContainer
   {
      ServiceCompositeInstance GetService( CompositeInstanceStructureOwner structureOwner, CompositeModel model, IEnumerable<Type> serviceTypes, UsesContainerQuery usesContainer, String serviceID );

      IEnumerable<ServiceCompositeInstance> ExistingServices { get; }
   }

   public class ThreadsafeServiceContainer : ServiceContainer
   {
#if WP8_BUILD
      private readonly System.Collections.Generic.IDictionary<String, ServiceCompositeInstance> _services;
#else
      private readonly System.Collections.Concurrent.ConcurrentDictionary<String, ServiceCompositeInstance> _services;
#endif

      public ThreadsafeServiceContainer()
      {
#if WP8_BUILD
         this._services = new System.Collections.Generic.Dictionary<String, ServiceCompositeInstance>();
#else
         this._services = new System.Collections.Concurrent.ConcurrentDictionary<String, ServiceCompositeInstance>();
#endif
      }

      #region ServiceContainer Members

      public ServiceCompositeInstance GetService( CompositeInstanceStructureOwner structureOwner, CompositeModel model, IEnumerable<Type> serviceTypes, UsesContainerQuery usesContainer, String serviceID )
      {
#if WP8_BUILD
         lock ( this._services )
         {
#endif
            return this._services.
#if WP8_BUILD
GetOrAdd_NotThreadSafe(
#else
         GetOrAdd(
#endif
 serviceID,
            id =>
            {
               var builderUses = UsesContainerMutableImpl.CreateWithParent( model.UsesContainer );
               var instance = new ServiceCompositeInstanceImpl( structureOwner, model, serviceTypes, builderUses.Query, serviceID );
               return instance;
            } );
#if WP8_BUILD
         }
#endif
      }

      public IEnumerable<ServiceCompositeInstance> ExistingServices
      {
         get
         {
            return this._services.Values;
         }
      }

      #endregion
   }
}
