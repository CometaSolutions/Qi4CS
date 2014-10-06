/*
 * Copyright (c) 2007, Rickard Öberg.
 * See NOTICE file.
 * 
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

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This interface must be implemented by all constraints.
   /// Constraints are used for simple value validation - they can't access the composite.
   /// </summary>
   /// <typeparam name="TConstraint">The type of the constraint attribute.</typeparam>
   /// <typeparam name="TValue">The type of the value to be checked.</typeparam>
   /// <remarks>
   /// The Qi4CS runtime will know to substitute correct generic type arguments in case of generic constraint types.
   /// Additionally, the constraint instance does not support injecting nor does it have access to composite or its state.
   /// </remarks>
   public interface Constraint<in TConstraint, in TValue>
      where TConstraint : Attribute
   {
      /// <summary>
      /// This method will be invoked for each parameter onto which the constraint attribute is applied.
      /// </summary>
      /// <param name="attribute">The attribute that was applied to parameter.</param>
      /// <param name="value">The value of the parameter.</param>
      /// <returns><c>true</c> if the <paramref name="value"/> is considered to be valid; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// After all constraints have been checked, a <see cref="ConstraintViolationException"/> will be thrown by Qi4CS if any of the constraints' <see cref="IsValid"/> method returns <c>false</c>.
      /// </remarks>
      Boolean IsValid( TConstraint attribute, TValue value );
   }
}
