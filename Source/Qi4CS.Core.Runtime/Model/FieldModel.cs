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
   public class FieldModelMutable : AbstractInjectableModelMutableImpl<FieldInfo>, Mutable<FieldModelMutable, FieldModel>, MutableQuery<FieldModel>
   {
      private readonly FieldModelState _state;

      public FieldModelMutable( FieldModelState state, FieldModelImmutable immutable )
         : base( state, immutable )
      {
         this._state = state;
      }

      public Int32 FieldIndex
      {
         get
         {
            return this._state.FieldIndex;
         }
         set
         {
            this._state.FieldIndex = value;
         }
      }

      #region AbstractInjectableModelMutable Members

      public override Type TargetType
      {
         get
         {
            return this._state.NativeInfo.FieldType;
         }
      }

      #endregion

      #region Mutable<FieldModelMutable,FieldModel> Members

      public FieldModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<FieldModel> Members

      public FieldModel IQ
      {
         get
         {
            return (FieldModel) base.Immutable;
         }
      }

      #endregion
   }

   public class FieldModelImmutable : AbstractInjectableModelImmutable<FieldInfo>, FieldModel
   {
      private readonly FieldModelState _state;

      public FieldModelImmutable( FieldModelState state )
         : base( state )
      {
         this._state = state;
      }

      #region FieldModel Members


      public Int32 FieldIndex
      {
         get
         {
            return this._state.FieldIndex;
         }
      }

      #endregion

      public override Type DeclaringType
      {
         get
         {
            return this._state.NativeInfo.DeclaringType;
         }
      }

      public override CompositeModel CompositeModel
      {
         get
         {
            return this._state.Composite.IQ;
         }
      }
   }

   public class FieldModelState : AbstractInjectableModelState<FieldInfo>
   {
      private CompositeModelMutable _compositeModel;
      private Int32 _fieldIndex;

      public FieldModelState( CollectionsFactory factory )
         : base( factory )
      {
         this._fieldIndex = -1;
      }

      public CompositeModelMutable Composite
      {
         get
         {
            return this._compositeModel;
         }
         set
         {
            this._compositeModel = value;
         }
      }

      public Int32 FieldIndex
      {
         get
         {
            return this._fieldIndex;
         }
         set
         {
            this._fieldIndex = value;
         }
      }

      public override String ToString()
      {
         String result = null;
         if ( this.NativeInfo != null )
         {
            result = base.ToString() + " " + this.NativeInfo.FieldType.FullName + " " + this.NativeInfo.Name + ";";
         }
         else
         {
            result = base.ToString();
         }
         return result;
      }
   }
}
