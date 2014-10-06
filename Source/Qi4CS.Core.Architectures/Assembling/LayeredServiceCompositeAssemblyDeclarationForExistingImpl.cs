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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Assembling;

namespace Qi4CS.Core.Architectures.Assembling
{
   internal class LayeredServiceCompositeAssemblyDeclarationForExistingImpl : ServiceCompositeAssemblyDeclarationForExistingImpl, LayeredServiceCompositeAssemblyDeclaration
   {
      internal LayeredServiceCompositeAssemblyDeclarationForExistingImpl( Assembler assembler, CompositeModelType compositeModelType, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos )
         : base( assembler, compositeModelType, compositeAssemblyInfos )
      {

      }

      #region LayeredCompositeAssemblyDeclaration Members

      public LayeredAbstractCompositeAssemblyDeclaration VisibleIn( Visibility visibility )
      {
         this.ForEachInfo( info => ( (LayeredCompositeAssemblyInfo) info ).Visibility = visibility );
         return this;
      }

      #endregion
   }
}
