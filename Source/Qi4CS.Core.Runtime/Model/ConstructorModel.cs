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
using System.Reflection;
using CollectionsWithRoles.API;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{

   public class ConstructorModelMutable : AbstractMemberInfoModelMutable<ConstructorInfo>, Mutable<ConstructorModelMutable, ConstructorModel>, MutableQuery<ConstructorModel>
   {
      private readonly ConstructorModelState _state;
      private readonly ConstructorModelImmutable _immutable;

      public ConstructorModelMutable( ConstructorModelState state, ConstructorModelImmutable immutable )
         : base( state, immutable )
      {
         this._state = state;
         this._immutable = immutable;
      }

      public CompositeModelMutable CompositeModelMutable
      {
         get
         {
            return this._state.Composite;
         }
      }

      public ListWithRoles<ParameterModelMutable, ParameterModelMutable, ParameterModel> Parameters
      {
         get
         {
            return this._state.Parameters;
         }
      }

      public Int32 ConstructorIndex
      {
         set
         {
            this._state.ConstructorIndex = value;
         }
      }

      #region Mutable<ConstraintModelMutable,ConstructorModel> Members

      public ConstructorModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<ConstructorModel> Members

      public ConstructorModel IQ
      {
         get
         {
            return this._immutable;
         }
      }

      #endregion
   }

   public class ConstructorModelImmutable : AbstractMemberInfoModelImmutable<ConstructorInfo>, ConstructorModel
   {
      private readonly ConstructorModelState _state;

      public ConstructorModelImmutable( ConstructorModelState state )
         : base( state )
      {
         this._state = state;
      }

      #region ConstructorModel Members

      public CompositeModel CompositeModel
      {
         get
         {
            return this._state.Composite.IQ;
         }
      }

      public ListQuery<ParameterModel> Parameters
      {
         get
         {
            return this._state.Parameters.MQ.IQ;
         }
      }

      public int ConstructorIndex
      {
         get
         {
            return this._state.ConstructorIndex;
         }
      }

      #endregion
   }

   public class ConstructorModelState : AbstractMemberInfoModelState<ConstructorInfo>
   {
      private CompositeModelMutable _composite;
      private readonly ListWithRoles<ParameterModelMutable, ParameterModelMutable, ParameterModel> _parameters;
      private Int32 _constructorIndex;

      public ConstructorModelState( CollectionsFactory factory )
      {
         this._parameters = factory.NewList<ParameterModelMutable, ParameterModelMutable, ParameterModel>();
      }

      public CompositeModelMutable Composite
      {
         get
         {
            return this._composite;
         }
         set
         {
            this._composite = value;
         }
      }

      public ListWithRoles<ParameterModelMutable, ParameterModelMutable, ParameterModel> Parameters
      {
         get
         {
            return this._parameters;
         }
      }

      public Int32 ConstructorIndex
      {
         get
         {
            return this._constructorIndex;
         }
         set
         {
            this._constructorIndex = value;
         }
      }

      public override String ToString()
      {
         String result = null;
         if ( this.NativeInfo != null )
         {
            result = this.NativeInfo.DeclaringType.Name + "(" + String.Join( ", ", this._parameters.MQ.IQ ) + ");";
         }
         else
         {
            result = base.ToString();
         }
         return result;
      }
   }
}
