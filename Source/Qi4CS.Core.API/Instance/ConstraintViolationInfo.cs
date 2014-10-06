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
   /// When a constraint violation is occurred (i.e. <see cref="Qi4CS.Core.API.Instance.Constraint{T,U}.IsValid(T, U)"/> has returned <c>false</c>) the information about the violation is put to this class.
   /// </summary>
   public sealed class ConstraintViolationInfo
   {

      private readonly Int32 _paramIndex;
      private readonly Object _value;
      private readonly Attribute _constraint;
      private readonly String _parameterName;

      /// <summary>
      /// Creates a new instance of <see cref="ConstraintViolationInfo"/>.
      /// This constructor is called by Qi4CS runtime.
      /// </summary>
      /// <param name="index">The zero-based index of the parameter. Will be <c>-1</c> for return parameter.</param>
      /// <param name="value">The value of the parameter.</param>
      /// <param name="constraint">The constraint attribute.</param>
      /// <param name="parameterName">The name of the parameter.</param>
      public ConstraintViolationInfo( Int32 index, Object value, Attribute constraint, String parameterName )
      {
         this._paramIndex = index;
         this._value = value;
         this._constraint = constraint;
         this._parameterName = parameterName;
      }

      /// <summary>
      /// Gets the zero-based index of the parameter that caused constraint violation. Will be <c>-1</c> for return parameter.
      /// </summary>
      /// <value>The zero-based index of the parameter that caused constraint violation. Will be <c>-1</c> for return parameter.</value>
      public Int32 ParameterIndex
      {
         get
         {
            return this._paramIndex;
         }
      }

      /// <summary>
      /// Gets the value of the parameter.
      /// </summary>
      /// <value>The value of the parameter.</value>
      public Object Value
      {
         get
         {
            return this._value;
         }
      }

      /// <summary>
      /// Gets the constraint attribute.
      /// </summary>
      /// <value>The constraint attribute.</value>
      public Attribute Constraint
      {
         get
         {
            return this._constraint;
         }
      }

      /// <summary>
      /// Gets the name of the parameter.
      /// </summary>
      /// <value>The name of the parameter.</value>
      /// <seealso cref="Model.NameAttribute"/>
      public String ParameterName
      {
         get
         {
            return this._parameterName;
         }
      }

      /// <inheritdoc/>
      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj )
            || ( obj is ConstraintViolationInfo
                 && this._paramIndex.Equals( ( (ConstraintViolationInfo) obj ).ParameterIndex )
                 && Object.Equals( this._value, ( (ConstraintViolationInfo) obj ).Value )
                 && Object.Equals( this._constraint, ( (ConstraintViolationInfo) obj ).Constraint ) );
      }

      /// <inheritdoc/>
      public override Int32 GetHashCode()
      {
         return this._constraint.GetHashCodeSafe();
      }

      /// <inheritdoc/>
      public override String ToString()
      {
         return "Constraint violation for constraint " + this._constraint + ", " + ( this._paramIndex < 0 ? "with return value" : ( "with parameter at index " + this._paramIndex ) ) + ", with value of " + ( this._value == null ? "null" : ( "'" + this._value + "'" ) ) + ".";
      }
   }
}
