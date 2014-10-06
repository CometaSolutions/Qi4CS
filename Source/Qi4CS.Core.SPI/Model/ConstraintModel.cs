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

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// A model for a single constraint of the <see cref="ParameterModel"/>.
   /// </summary>
   public interface ConstraintModel
   {
      /// <summary>
      /// Gets the constraint attribute of this constraint.
      /// </summary>
      /// <value>The constraint attribute of this constraint.</value>
      Attribute ConstraintAttribute { get; }

      /// <summary>
      /// Gets the type implementing <see cref="API.Instance.Constraint{T,U}"/> interface.
      /// </summary>
      /// <value>The type implementing <see cref="API.Instance.Constraint{T,U}"/> interface.</value>
      Type ConstraintType { get; }

      /// <summary>
      /// Gets the <see cref="ParameterModel"/> this <see cref="ConstraintModel"/> belongs to.
      /// </summary>
      /// <value>The <see cref="ParameterModel"/> this <see cref="ConstraintModel"/> belongs to.</value>
      ParameterModel Owner { get; }
   }
}
