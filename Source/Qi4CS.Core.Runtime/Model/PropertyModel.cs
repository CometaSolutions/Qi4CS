/*
 * Copyright 2012 Stanislav Muhametsin. All rights Reserved.
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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class PropertyModelMutable : ModelWithAttributesMutable<PropertyInfo>
   {
      private readonly PropertyModelState _state;
      private readonly PropertyModelImmutable _immutable;

      public PropertyModelMutable( PropertyModelState state, PropertyModelImmutable immutable )
         : base( state, immutable )
      {
         this._state = state;
         this._immutable = immutable;
      }

      public new PropertyModel Immutable
      {
         get
         {
            return this._immutable;
         }
      }

      public CompositeMethodModelMutable GetterMethod
      {
         get
         {
            return this._state.GetterMethod;
         }
         set
         {
            this._state.GetterMethod = value;
         }
      }

      public CompositeMethodModelMutable SetterMethod
      {
         get
         {
            return this._state.SetterMethod;
         }
         set
         {
            this._state.SetterMethod = value;
         }
      }

      public Boolean PropertyIsImmutable
      {
         set
         {
            this._state.IsImmutable = value;
         }
      }
   }

   public class PropertyModelImmutable : ModelWithAttributesImmutable<PropertyInfo>, PropertyModel
   {
      private readonly PropertyModelState _state;

      public PropertyModelImmutable( PropertyModelState state )
         : base( state )
      {
         this._state = state;
      }

      #region PropertyModel Members

      public CompositeMethodModel GetterMethod
      {
         get
         {
            CompositeMethodModelMutable result = this._state.GetterMethod;
            return result == null ? null : result.IQ;
         }
      }

      public CompositeMethodModel SetterMethod
      {
         get
         {
            CompositeMethodModelMutable result = this._state.SetterMethod;
            return result == null ? null : result.IQ;
         }
      }

      public Boolean IsImmutable
      {
         get
         {
            return this._state.IsImmutable;
         }
      }

      public Func<PropertyInfo, ApplicationSPI, Object> DefaultValueCreator
      {
         get
         {
            return this._state.DefaultValueCreator;
         }
      }

      #endregion

      #region OptionalInfo Members

      public Boolean IsOptional
      {
         get
         {
            return this._state.AllAttributes.CQ.OfType<OptionalAttribute>().Any();
         }
      }

      #endregion
   }

   public class PropertyModelState : ModelWithAttributesState<PropertyInfo>
   {
      private CompositeMethodModelMutable _getterMethod;
      private CompositeMethodModelMutable _setterMethod;
      private Func<PropertyInfo, ApplicationSPI, Object> _defaultValueCreator;
      private Boolean _isImmutable;

      public PropertyModelState( CollectionsFactory factory )
         : base( factory )
      {

      }

      public CompositeMethodModelMutable GetterMethod
      {
         get
         {
            return this._getterMethod;
         }
         set
         {
            this._getterMethod = value;
         }
      }

      public CompositeMethodModelMutable SetterMethod
      {
         get
         {
            return this._setterMethod;
         }
         set
         {
            this._setterMethod = value;
         }
      }

      public Func<PropertyInfo, ApplicationSPI, Object> DefaultValueCreator
      {
         get
         {
            return this._defaultValueCreator;
         }
         set
         {
            this._defaultValueCreator = value;
         }
      }

      public Boolean IsImmutable
      {
         get
         {
            return this._isImmutable;
         }
         set
         {
            this._isImmutable = value;
         }
      }
   }
}
