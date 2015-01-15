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
   public class SpecialMethodModelMutable : ModelWithAttributesMutable<MethodInfo>, Mutable<SpecialMethodModelMutable, SpecialMethodModel>, MutableQuery<SpecialMethodModel>
   {
      private readonly SpecialMethodModelState _state;
      private readonly SpecialMethodModel _immutable;

      public SpecialMethodModelMutable( SpecialMethodModelState state, SpecialMethodModelImmutable immutable )
         : base( state, immutable )
      {
         this._state = state;
         this._immutable = immutable;
      }

      public ListWithRoles<ParameterModelMutable, ParameterModelMutable, ParameterModel> Parameters
      {
         get
         {
            return this._state.Parameters;
         }
      }

      public ParameterModelMutable Result
      {
         get
         {
            return this._state.ResultModel;
         }
      }

      public Int32 SpecialMethodIndex
      {
         set
         {
            this._state.SpecialMethodIndex = value;
         }
      }

      #region Mutable<SpecialMethodModelMutable,SpecialMethodModel> Members

      public SpecialMethodModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<SpecialMethodModel> Members

      public SpecialMethodModel IQ
      {
         get
         {
            return this._immutable;
         }
      }

      #endregion
   }

   public class SpecialMethodModelImmutable : ModelWithAttributesImmutable<MethodInfo>, SpecialMethodModel
   {
      private readonly SpecialMethodModelState _state;

      public SpecialMethodModelImmutable( SpecialMethodModelState state )
         : base( state )
      {
         this._state = state;
      }

      #region SpecialMethodModel Members

      public CompositeModel CompositeModel
      {
         get
         {
            return this._state.CompositeModel.IQ;
         }
      }

      public Int32 SpecialMethodIndex
      {
         get
         {
            return this._state.SpecialMethodIndex;
         }
      }

      #endregion

      #region AbstractModelWithParameters Members

      public ListQuery<ParameterModel> Parameters
      {
         get
         {
            return this._state.Parameters.MQ.IQ;
         }
      }

      public ParameterModel ResultModel
      {
         get
         {
            return this._state.ResultModel.IQ;
         }
      }

      #endregion


      #region AbstractMethodModelWithFragment Members

      public Type FragmentType
      {
         get
         {
            return this._state.FragmentType;
         }
      }

      #endregion
   }

   public class SpecialMethodModelState : ModelWithAttributesState<MethodInfo>
   {
      private readonly ListWithRoles<ParameterModelMutable, ParameterModelMutable, ParameterModel> _parameters;
      private CompositeModelMutable _composite;
      private ParameterModelMutable _resultModel;
      private Int32 _specialMethodIndex;
      private Type _fragmentType;

      public SpecialMethodModelState( CollectionsFactory factory )
         : base( factory )
      {
         this._parameters = factory.NewList<ParameterModelMutable, ParameterModelMutable, ParameterModel>();
      }

      public ListWithRoles<ParameterModelMutable, ParameterModelMutable, ParameterModel> Parameters
      {
         get
         {
            return this._parameters;
         }
      }

      public CompositeModelMutable CompositeModel
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

      public Int32 SpecialMethodIndex
      {
         get
         {
            return this._specialMethodIndex;
         }
         set
         {
            this._specialMethodIndex = value;
         }
      }

      public Type FragmentType
      {
         get
         {
            return this._fragmentType;
         }
         set
         {
            this._fragmentType = value;
         }
      }
   }
}
