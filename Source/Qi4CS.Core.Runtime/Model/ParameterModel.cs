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
using System.Linq;
using System.Reflection;
using CollectionsWithRoles.API;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class ParameterModelMutable : AbstractInjectableModelMutableImpl<ParameterInfo>, Mutable<ParameterModelMutable, ParameterModel>, MutableQuery<ParameterModel>
   {
      private readonly ParameterModelState _state;

      public ParameterModelMutable( ParameterModelState state, ParameterModelImmutable immutable )
         : base( state, immutable )
      {
         this._state = state;
      }

      public ListWithRoles<ConstraintModelMutable, ConstraintModelMutable, ConstraintModel> ConstraintsMutable
      {
         get
         {
            return this._state.Constraints;
         }
      }

      #region AbstractInjectableModelMutable Members

      public override Type TargetType
      {
         get
         {
            return this._state.NativeInfo.ParameterType;
         }
      }

      #endregion

      #region Mutable<ParameterModelMutable,ParameterModel> Members

      public ParameterModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<ParameterModel> Members

      public ParameterModel IQ
      {
         get
         {
            return (ParameterModel) base.Immutable;
         }
      }

      #endregion

      public Boolean IsOptional
      {
         set
         {
            this._state.IsOptional = value;
         }
      }
   }

   public class ParameterModelImmutable : AbstractInjectableModelImmutable<ParameterInfo>, ParameterModel
   {

      private readonly ParameterModelState _state;

      public ParameterModelImmutable( ParameterModelState state )
         : base( state )
      {
         this._state = state;
      }

      #region ParameterModel Members

      public AbstractModelWithParameters Owner
      {
         get
         {
            return this._state.Owner;
         }
      }

      public String Name
      {
         get
         {
            return this._state.Name;
         }
      }

      #endregion

      #region AbstractConstrainableModel Members

      public ListQuery<ConstraintModel> Constraints
      {
         get
         {
            return this._state.Constraints.MQ.IQ;
         }
      }

      #endregion

      public override Type DeclaringType
      {
         get
         {
            return this._state.NativeInfo.Member.DeclaringType;
         }
      }

      public override CompositeModel CompositeModel
      {
         get
         {
            return this._state.Owner.CompositeModel;
         }
      }
   }

   public class ParameterModelState : AbstractInjectableModelState<ParameterInfo>
   {
      private AbstractModelWithParameters _owner;
      private readonly ListWithRoles<ConstraintModelMutable, ConstraintModelMutable, ConstraintModel> _constraints;
      private String _name;

      public ParameterModelState( CollectionsFactory factory )
         : base( factory )
      {
         this._constraints = factory.NewList<ConstraintModelMutable, ConstraintModelMutable, ConstraintModel>();
      }

      public AbstractModelWithParameters Owner
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

      public ListWithRoles<ConstraintModelMutable, ConstraintModelMutable, ConstraintModel> Constraints
      {
         get
         {
            return this._constraints;
         }
      }

      public String Name
      {
         get
         {
            return this._name;
         }
         set
         {
            this._name = value;
         }
      }

      public override String ToString()
      {
         String result = null;
         if ( this.NativeInfo != null )
         {
            result = base.ToString() + " " + String.Join( ", ", this._constraints.MQ.Select( constraint => "[" + constraint + "]" ) ) + this.NativeInfo.ParameterType.FullName + " " + this._name;
         }
         else
         {
            result = base.ToString();
         }
         return result;
      }

   }
}
