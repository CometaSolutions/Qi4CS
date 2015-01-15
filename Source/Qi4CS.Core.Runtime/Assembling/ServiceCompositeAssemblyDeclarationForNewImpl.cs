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
using CollectionsWithRoles.API;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Core.Runtime.Assembling
{
   public class ServiceCompositeAssemblyDeclarationForNewImpl : AbstractCompositeAssemblyDeclarationForNewImpl, ServiceCompositeAssemblyDeclaration
   {
      public ServiceCompositeAssemblyDeclarationForNewImpl( Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo info, CollectionsFactory collectionsFactory )
         : base( assembler, compositeAssemblyInfos, info, collectionsFactory )
      {
      }

      #region ServiceCompositeAssemblyDeclaration Members

      public ServiceCompositeAssemblyDeclaration WithServiceID( String serviceID )
      {
         ( (ServiceCompositeAssemblyInfo) this.Info ).ServiceID = serviceID;
         return this;
      }

      public ServiceCompositeAssemblyDeclaration SetActivateWithApplication( Boolean activateWithApplication )
      {
         ( (ServiceCompositeAssemblyInfo) this.Info ).ActivateWithApplication = activateWithApplication;
         return this;
      }

      #endregion
   }
}
