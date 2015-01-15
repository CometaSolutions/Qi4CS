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
using CollectionsWithRoles.API;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Bootstrap.Model
{
   /// <summary>
   /// This is common interface for an model unit capable of containing <see cref="CompositeModel"/>s.
   /// </summary>
   /// <seealso cref="Model.SingletonApplicationModel"/>
   /// <seealso cref="Model.ModuleModel"/>
   public interface ModelContainer
   {
      /// <summary>
      /// Gets all the <see cref="CompositeModel"/>s contained within this Qi4CS model.
      /// </summary>
      /// <value>All the <see cref="CompositeModel"/>s contained within this Qi4CS model.</value>
      SetQuery<CompositeModel> CompositeModelsInThisContainer { get; }
   }
}
