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
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class ConstraintModelMutable : Mutable<ConstraintModelMutable, ConstraintModel>, MutableQuery<ConstraintModel>
   {
      private readonly ConstraintModelState _state;
      private readonly ConstraintModelImmutable _immutable;

      public ConstraintModelMutable( ConstraintModelState state, ConstraintModelImmutable immutable )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         ArgumentValidator.ValidateNotNull( "Immutable", immutable );

         this._state = state;
         this._immutable = immutable;
      }

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) || ( obj is ConstraintModelMutable && this._immutable.Equals( ( (ConstraintModelMutable) obj ).IQ ) );
      }

      public override Int32 GetHashCode()
      {
         return this._immutable.GetHashCode();
      }

      public override String ToString()
      {
         return this._state.ToString();
      }

      #region Mutable<ConstraintModelMutable,ConstraintModel> Members

      public ConstraintModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<ConstraintModel> Members

      public ConstraintModel IQ
      {
         get
         {
            return this._immutable;
         }
      }

      #endregion
   }

   public class ConstraintModelImmutable : ConstraintModel
   {
      private readonly ConstraintModelState _state;

      public ConstraintModelImmutable( ConstraintModelState state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      #region ConstraintModel Members

      public Attribute ConstraintAttribute
      {
         get
         {
            return this._state.ConstraintAttribute;
         }
      }

      public Type ConstraintType
      {
         get
         {
            return this._state.ConstraintType;
         }
      }

      public ParameterModel Owner
      {
         get
         {
            return this._state.Owner;
         }
      }

      #endregion

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) ||
            ( obj is ConstraintModel
              && Object.Equals( this._state.ConstraintAttribute, ( (ConstraintModel) obj ).ConstraintAttribute )
              && Object.Equals( this._state.ConstraintType, ( (ConstraintModel) obj ).ConstraintType )
            );
      }

      public override Int32 GetHashCode()
      {
         return this._state.ConstraintAttribute == null ? 0 : this._state.ConstraintAttribute.GetType().GetHashCode();
      }

      public override String ToString()
      {
         return this._state.ToString();
      }

   }

   public class ConstraintModelState
   {
      private Attribute _attribute;
      private Type _constraintType;
      private ParameterModel _owner;

      public ConstraintModelState()
      { }

      public Attribute ConstraintAttribute
      {
         get
         {
            return this._attribute;
         }
         set
         {
            this._attribute = value;
         }
      }

      public Type ConstraintType
      {
         get
         {
            return this._constraintType;
         }
         set
         {
            this._constraintType = value;
         }
      }

      public ParameterModel Owner
      {
         get
         {
            return this._owner;
         }
         set
         {
            this._owner = value;
         }
      }

      public override String ToString()
      {
         return "[Constraint::" + ( this._attribute == null ? "??" : this._attribute.GetType().ToString() ) + "] " + this._constraintType;
      }
   }
}
