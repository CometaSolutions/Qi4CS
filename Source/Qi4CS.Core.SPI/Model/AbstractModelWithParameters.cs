﻿/*
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
using CollectionsWithRoles.API;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This is common interface for all Qi4CS models which contain <see cref="ParameterModel"/>s.
   /// </summary>
   /// <seealso cref="CompositeMethodModel"/>
   /// <seealso cref="ConstructorModel"/>
   /// <seealso cref="SpecialMethodModel"/>
   public interface AbstractModelWithParameters
   {
      /// <summary>
      /// Gets all the <see cref="ParameterModel"/>s contained in this Qi4CS model.
      /// </summary>
      /// <value>All the <see cref="ParameterModel"/>s contained in this Qi4CS model.</value>
      ListQuery<ParameterModel> Parameters { get; }

      /// <summary>
      /// Gets the <see cref="CompositeModel"/> this Qi4CS model belongs to.
      /// </summary>
      /// <value>The <see cref="CompositeModel"/> this Qi4CS model belongs to.</value>
      CompositeModel CompositeModel { get; }
   }
}
