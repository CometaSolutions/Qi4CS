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
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class CompositeMethodModelMutable : AbstractMethodModelMutable, Mutable<CompositeMethodModelMutable, CompositeMethodModel>, MutableQuery<CompositeMethodModel>
   {
      private readonly CompositeMethodModelState _state;

      public CompositeMethodModelMutable( CompositeMethodModelState state, CompositeMethodModelImmutable immutable )
         : base( state, immutable )
      {
         this._state = state;
      }

      public CompositeModelMutable Owner
      {
         get
         {
            return this._state.Owner;
         }
      }

      public ListWithRoles<ParameterModelMutable, ParameterModelMutable, ParameterModel> Parameters
      {
         get
         {
            return this._state.Parameters;
         }
      }

      public ListWithRoles<ConcernMethodModelMutable, ConcernMethodModelMutable, ConcernMethodModel> Concerns
      {
         get
         {
            return this._state.Concerns;
         }
      }

      public MixinMethodModelMutable Mixin
      {
         get
         {
            return this._state.Mixin;
         }
         set
         {
            this._state.Mixin = value;
         }
      }

      public ListWithRoles<SideEffectMethodModelMutable, SideEffectMethodModelMutable, SideEffectMethodModel> SideEffects
      {
         get
         {
            return this._state.SideEffects;
         }
      }

      public ParameterModelMutable Result
      {
         get
         {
            return this._state.ResultModel;
         }
      }

      public PropertyModelMutable PropertyModel
      {
         get
         {
            return this._state.PropertyModel;
         }
      }

      public EventModelMutable EventModel
      {
         get
         {
            return this._state.EventModel;
         }
      }

      public Int32 CompositeMethodIndex
      {
         set
         {
            this._state.CompositeMethodIndex = value;
         }
      }

      #region Mutable<ComplexMethodModelMutable,ComplexMethodModel> Members

      public CompositeMethodModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<ComplexMethodModel> Members

      public CompositeMethodModel IQ
      {
         get
         {
            return (CompositeMethodModel) base.Immutable;
         }
      }

      #endregion
   }

   public class CompositeMethodModelImmutable : AbstractMethodModelImmutable, CompositeMethodModel
   {
      private readonly CompositeMethodModelState _state;

      public CompositeMethodModelImmutable( CompositeMethodModelState state )
         : base( state )
      {
         this._state = state;
      }

      #region CompositeMethodModel Members

      public CompositeModel CompositeModel
      {
         get
         {
            return this._state.Owner.IQ;
         }
      }

      public ListQuery<ParameterModel> Parameters
      {
         get
         {
            return this._state.Parameters.MQ.IQ;
         }
      }

      public ListQuery<ConcernMethodModel> Concerns
      {
         get
         {
            return this._state.Concerns.MQ.IQ;
         }
      }

      public MixinMethodModel Mixin
      {
         get
         {
            MixinMethodModelMutable result = this._state.Mixin;
            return result == null ? null : result.Immutable;
         }
      }

      public ListQuery<SideEffectMethodModel> SideEffects
      {
         get
         {
            return this._state.SideEffects.MQ.IQ;
         }
      }

      public ParameterModel Result
      {
         get
         {
            return this._state.ResultModel.IQ;
         }
      }

      public Int32 MethodIndex
      {
         get
         {
            return this._state.CompositeMethodIndex;
         }
      }

      public PropertyModel PropertyModel
      {
         get
         {
            PropertyModelMutable result = this._state.PropertyModel;
            return result == null ? null : result.Immutable;
         }
      }

      public EventModel EventModel
      {
         get
         {
            EventModelMutable result = this._state.EventModel;
            return result == null ? null : result.IQ;
         }
      }

      #endregion
   }

   public class CompositeMethodModelState : AbstractMethodModelState
   {
      private CompositeModelMutable _owner;
      private readonly ListWithRoles<ParameterModelMutable, ParameterModelMutable, ParameterModel> _parameters;
      private readonly ListWithRoles<ConcernMethodModelMutable, ConcernMethodModelMutable, ConcernMethodModel> _concerns;
      private MixinMethodModelMutable _mixin;
      private readonly ListWithRoles<SideEffectMethodModelMutable, SideEffectMethodModelMutable, SideEffectMethodModel> _sideEffects;
      private ParameterModelMutable _resultModel;
      private Int32 _methodIndex;
      private PropertyModelMutable _property;
      private EventModelMutable _event;

      public CompositeMethodModelState( CollectionsFactory factory )
      {
         this._parameters = factory.NewList<ParameterModelMutable, ParameterModelMutable, ParameterModel>();
         this._concerns = factory.NewList<ConcernMethodModelMutable, ConcernMethodModelMutable, ConcernMethodModel>();
         this._sideEffects = factory.NewList<SideEffectMethodModelMutable, SideEffectMethodModelMutable, SideEffectMethodModel>();
      }

      public CompositeModelMutable Owner
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

      public ListWithRoles<ParameterModelMutable, ParameterModelMutable, ParameterModel> Parameters
      {
         get
         {
            return this._parameters;
         }
      }

      public ListWithRoles<ConcernMethodModelMutable, ConcernMethodModelMutable, ConcernMethodModel> Concerns
      {
         get
         {
            return this._concerns;
         }
      }

      public MixinMethodModelMutable Mixin
      {
         get
         {
            return this._mixin;
         }
         set
         {
            this._mixin = value;
         }
      }

      public ListWithRoles<SideEffectMethodModelMutable, SideEffectMethodModelMutable, SideEffectMethodModel> SideEffects
      {
         get
         {
            return this._sideEffects;
         }
      }

      public ParameterModelMutable ResultModel
      {
         get
         {
            return this._resultModel;
         }
         set
         {
            this._resultModel = value;
         }
      }

      public Int32 CompositeMethodIndex
      {
         get
         {
            return this._methodIndex;
         }
         set
         {
            this._methodIndex = value;
         }
      }

      public PropertyModelMutable PropertyModel
      {
         get
         {
            return this._property;
         }
         set
         {
            this._property = value;
         }
      }

      public EventModelMutable EventModel
      {
         get
         {
            return this._event;
         }
         set
         {
            this._event = value;
         }
      }

      public override String ToString()
      {
         String result = null;
         if ( this.NativeInfo != null )
         {
            result = this.NativeInfo.ReturnType.FullName + " " + this.NativeInfo.DeclaringType.FullName + "." + this.NativeInfo.Name + "(" + String.Join( ", ", this._parameters.CQ ) + ")";// +"\n{\n  " + String.Join( "\n  ", this._concerns.CQ ) + ( this._mixin == null ? "" : "\n  " + this._mixin.ToString() ) + "\n  " + this._resultModel + "\n" + String.Join( "\n  ", this._sideEffects.CQ ) + "\n}";
         }
         else
         {
            result = base.ToString();
         }
         return result;
      }

   }
}
