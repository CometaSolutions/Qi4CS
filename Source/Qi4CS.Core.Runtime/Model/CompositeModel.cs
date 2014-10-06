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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class CompositeModelMutable : Mutable<CompositeModelMutable, CompositeModel>, MutableQuery<CompositeModel>
   {
      private readonly CompositeModelState _state;
      private readonly CompositeModel _immutable;

      public CompositeModelMutable( CompositeModelState state, CompositeModel immutable )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         ArgumentValidator.ValidateNotNull( "Immutable", immutable );

         this._state = state;
         this._immutable = immutable;
      }

      public ListWithRoles<CompositeMethodModelMutable, CompositeMethodModelMutable, CompositeMethodModel> Methods
      {
         get
         {
            return this._state.Methods;
         }
      }

      public ListWithRoles<FieldModelMutable, FieldModelMutable, FieldModel> Fields
      {
         get
         {
            return this._state.Fields;
         }
      }

      public ListWithRoles<ConstructorModelMutable, ConstructorModelMutable, ConstructorModel> Constructors
      {
         get
         {
            return this._state.Constructors;
         }
      }

      public ListWithRoles<SpecialMethodModelMutable, SpecialMethodModelMutable, SpecialMethodModel> SpecialMethods
      {
         get
         {
            return this._state.SpecialMethods;
         }
      }

      public SetProxy<Type> PublicTypes
      {
         get
         {
            return this._state.PublicTypes;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) || ( obj is CompositeModelMutable && this._immutable.Equals( ( (CompositeModelMutable) obj ).IQ ) );
      }

      public override Int32 GetHashCode()
      {
         return this._immutable.GetHashCode();
      }

      #region Mutable<CompositeModelMutable,CompositeModel> Members

      public CompositeModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<CompositeModel> Members

      public CompositeModel IQ
      {
         get
         {
            return this._immutable;
         }
      }

      #endregion
   }

   public class CompositeModelImmutable : CompositeModel
   {

      private readonly CompositeModelState _state;

      public CompositeModelImmutable( CompositeModelState state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      #region CompositeModel Members

      public CompositeModelType ModelType
      {
         get
         {
            return this._state.ModelType;
         }
      }

      public ListQuery<CompositeMethodModel> Methods
      {
         get
         {
            return this._state.Methods.MQ.IQ;
         }
      }

      public ListQuery<FieldModel> Fields
      {
         get
         {
            return this._state.Fields.MQ.IQ;
         }
      }

      public ListQuery<ConstructorModel> Constructors
      {
         get
         {
            return this._state.Constructors.MQ.IQ;
         }
      }

      public ListQuery<SpecialMethodModel> SpecialMethods
      {
         get
         {
            return this._state.SpecialMethods.MQ.IQ;
         }
      }

      public UsesContainerQuery UsesContainer
      {
         get
         {
            return this._state.MetaInfoContainer;
         }
      }

      public SetQuery<Type> PublicTypes
      {
         get
         {
            return this._state.PublicTypes.CQ;
         }
      }

      public ApplicationModel<ApplicationSPI> ApplicationModel
      {
         get
         {
            return this._state.ApplicationModel;
         }
      }

      public Int32 CompositeModelID
      {
         get
         {
            return this._state.CompositeModelID;
         }
      }

      public Type MainCodeGenerationType
      {
         get
         {
            return this._state.MainCodeGenerationType;
         }
      }

      public event EventHandler<CompositeModelInstantiationArgs> CompositeInstantiated;

      #endregion

      #region Typeable Members

      public Type ImplementedType
      {
         get
         {
            return typeof( CompositeModel );
         }
      }

      #endregion

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ); // ||
         //( base.Equals( obj ) &&
         //  obj is CompositeModel &&
         //  TypeUtil.BothNullOrEquals( this._state.ModelType, ( (CompositeModel) obj ).ModelType ) &&
         //  this._state.PublicTypes.CQ.SetEquals( ( (CompositeModel) obj ).PublicTypes )
         //  );
      }

      public override Int32 GetHashCode()
      {
         return base.GetHashCode();// this._state.PublicTypes.CQ.Aggregate( 0, ( cur, type ) => cur += type.GetHashCode() );
      }

      #region UsesProviderQuery Members

      public Object GetObjectForName( Type type, String name )
      {
         return this._state.MetaInfoContainer.GetObjectForName( type, name );
      }

      #endregion

      public override String ToString()
      {
         return this._state.ModelType + " of types " + String.Join( ", ", this._state.PublicTypes.CQ );
      }

      internal void InvokeCompositeInstantiated( CompositeInstance instance )
      {
         this.CompositeInstantiated.InvokeEventIfNotNull( evt => evt( this, new CompositeModelInstantiationArgs( instance ) ) );
      }
   }

   public class CompositeModelState
   {
      private readonly ListWithRoles<CompositeMethodModelMutable, CompositeMethodModelMutable, CompositeMethodModel> _methods;
      private readonly ListWithRoles<FieldModelMutable, FieldModelMutable, FieldModel> _fields;
      private readonly ListWithRoles<ConstructorModelMutable, ConstructorModelMutable, ConstructorModel> _constructors;
      private readonly ListWithRoles<SpecialMethodModelMutable, SpecialMethodModelMutable, SpecialMethodModel> _specialMethods;
      private readonly SetProxy<Type> _publicTypes;
      private UsesContainerQuery _metaInfoContainer;
      private CompositeModelType _modelType;
      private ApplicationModel<ApplicationSPI> _applicationModel;
      private Int32 _compositeModelID;
      private Type _codeGenerationMainType;

      public CompositeModelState( CollectionsFactory factory )
      {
         this._methods = factory.NewList<CompositeMethodModelMutable, CompositeMethodModelMutable, CompositeMethodModel>();
         this._fields = factory.NewList<FieldModelMutable, FieldModelMutable, FieldModel>();
         this._constructors = factory.NewList<ConstructorModelMutable, ConstructorModelMutable, ConstructorModel>();
         this._specialMethods = factory.NewList<SpecialMethodModelMutable, SpecialMethodModelMutable, SpecialMethodModel>();
         this._publicTypes = factory.NewSetProxy<Type>();
      }

      public CompositeModelType ModelType
      {
         get
         {
            return this._modelType;
         }
         set
         {
            this._modelType = value;
         }
      }

      public ListWithRoles<CompositeMethodModelMutable, CompositeMethodModelMutable, CompositeMethodModel> Methods
      {
         get
         {
            return this._methods;
         }
      }

      public ListWithRoles<FieldModelMutable, FieldModelMutable, FieldModel> Fields
      {
         get
         {
            return this._fields;
         }
      }

      public ListWithRoles<ConstructorModelMutable, ConstructorModelMutable, ConstructorModel> Constructors
      {
         get
         {
            return this._constructors;
         }
      }

      public ListWithRoles<SpecialMethodModelMutable, SpecialMethodModelMutable, SpecialMethodModel> SpecialMethods
      {
         get
         {
            return this._specialMethods;
         }
      }

      public SetProxy<Type> PublicTypes
      {
         get
         {
            return this._publicTypes;
         }
      }

      public UsesContainerQuery MetaInfoContainer
      {
         get
         {
            return this._metaInfoContainer;
         }
         set
         {
            this._metaInfoContainer = value;
         }
      }

      public ApplicationModel<ApplicationSPI> ApplicationModel
      {
         get
         {
            return this._applicationModel;
         }
         set
         {
            this._applicationModel = value;
         }
      }

      public Int32 CompositeModelID
      {
         get
         {
            return this._compositeModelID;
         }
         set
         {
            this._compositeModelID = value;
         }
      }

      public Type MainCodeGenerationType
      {
         get
         {
            return this._codeGenerationMainType;
         }
         set
         {
            this._codeGenerationMainType = value;
         }
      }

      //public override String ToString()
      //{
      //   String result = null;
      //   if ( this.TypeModel != null && this._modelType != null )
      //   {
      //      result = this._modelType.ToString() + " " + this._typeModel.PublicType.FullName + "\n{\n" + String.Join( "\n", this._fields ) + "\n" + String.Join( "\n", this._constructors ) + "\n" + String.Join( "\n", this._methods ) + "\n}";
      //   }
      //   else
      //   {
      //      return base.ToString();
      //   }
      //   return result;
      //}
   }
}
