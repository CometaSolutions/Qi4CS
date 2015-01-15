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
using System.Collections.Generic;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Tests.Core.Instance.Composite.Domain
{
   public interface CollectionsFactory
   {
      ListWithRoles<TMutableQuery, TImmutableQuery> NewList<TMutableQuery, TImmutableQuery>()
         where TMutableQuery : MutableQuery<TImmutableQuery>;
   }

   public class CollectionsFactoryMixin : CollectionsFactory
   {
#pragma warning disable 649

      [Structure]
      private StructureServiceProvider _structureServices;
#pragma warning restore 649

      #region CollectionsFactory Members

      public virtual ListWithRoles<TMutableQuery, TImmutableQuery> NewList<TMutableQuery, TImmutableQuery>() where TMutableQuery : MutableQuery<TImmutableQuery>
      {
         var builder = this._structureServices.NewPlainCompositeBuilder<ListWithRoles<TMutableQuery, TImmutableQuery>>();
         var list = new List<TMutableQuery>();
         builder.Builder.Prototype<CollectionWithRolesState<TMutableQuery>>().MutableCollection = list;
         builder.Builder.Prototype<ListWithRolesState<TMutableQuery>>().List = list;
         return builder.Instantiate();
      }

      #endregion
   }
}
