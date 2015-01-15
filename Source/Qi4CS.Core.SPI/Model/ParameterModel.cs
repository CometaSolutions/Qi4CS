using System;
using System.Reflection;
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
using CollectionsWithRoles.API;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// A model related to parameter of <see cref="CompositeMethodModel"/>, <see cref="ConstructorModel"/> or <see cref="SpecialMethodModel"/>.
   /// </summary>
   public interface ParameterModel : AbstractMemberInfoModel<ParameterInfo>, AbstractInjectableModel
   {
      /// <summary>
      /// Gets all the constraints of this <see cref="ParameterModel"/>.
      /// </summary>
      /// <value>All the constraints of this <see cref="ParameterModel"/>.</value>
      ListQuery<ConstraintModel> Constraints { get; }

      /// <summary>
      /// Gets the owner of this <see cref="ParameterModel"/>.
      /// </summary>
      /// <value>The owner of this <see cref="ParameterModel"/>.</value>
      /// <seealso cref="CompositeMethodModel"/>
      /// <seealso cref="ConstructorModel"/>
      /// <seealso cref="SpecialMethodModel"/>
      AbstractModelWithParameters Owner { get; }

      /// <summary>
      /// Gets the name of this parameter.
      /// </summary>
      /// <value>The name of this parameter.</value>
      /// <remarks>
      /// The name may be either from <see cref="ParameterInfo"/> or from <see cref="API.Model.NameAttribute"/> applied to this parameter.
      /// </remarks>
      String Name { get; }
   }
}
