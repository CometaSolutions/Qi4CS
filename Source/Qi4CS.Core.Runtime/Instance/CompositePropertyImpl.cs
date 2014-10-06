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
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Instance
{
   public class CompositePropertyImpl<TProperty> : CompositeStateParticipantImpl<PropertyInfo, PropertyModel>, CompositePropertySPI, CompositeProperty<TProperty>
   {
      private readonly Func<TProperty> _getter;
      private readonly Action<TProperty> _setter;
      private readonly Func<TProperty, TProperty> _exchangeMethod;
      private readonly Func<TProperty, TProperty, TProperty> _compareExchangeMethod;

      public CompositePropertyImpl(
         PropertyModel propertyModel,
         PropertyInfo propertyInfo,
         RefInvokerCallback refInvoker,
         Func<TProperty> getterMethod,
         Action<TProperty> setterMethod,
         Func<TProperty, TProperty> exchangeMethod,
         Func<TProperty, TProperty, TProperty> compareExchangeMethod
         )
         : base( propertyModel, propertyInfo, refInvoker )
      {
         ArgumentValidator.ValidateNotNull( "Getter method", getterMethod );
         ArgumentValidator.ValidateNotNull( "Setter method", setterMethod );
         ArgumentValidator.ValidateNotNull( "Exchange method", exchangeMethod );
         ArgumentValidator.ValidateNotNull( "Compare exchange method", compareExchangeMethod );

         this._getter = getterMethod;
         this._setter = setterMethod;
         this._exchangeMethod = exchangeMethod;
         this._compareExchangeMethod = compareExchangeMethod;
      }

      #region CompositeProperty Members

      public Object PropertyValueAsObject
      {
         get
         {
            return this.PropertyValue;
         }
         set
         {
            this.PropertyValue = (TProperty) value;
         }
      }

      public Object ExchangeAsObject( Object newValue )
      {
         return this.Exchange( (TProperty) newValue );
      }

      public Object CompareExchangeAsObject( Object comparand, Object newValueIfSameAsComparand )
      {
         return this.CompareExchange( (TProperty) comparand, (TProperty) newValueIfSameAsComparand );
      }

      #endregion

      #region CompositeProperty<TProperty> Members

      public TProperty PropertyValue
      {
         get
         {
            return this._getter();
         }
         set
         {
            this._setter( value );
         }
      }

      public TProperty Exchange( TProperty newValue )
      {
         return this._exchangeMethod( newValue );
      }

      public TProperty CompareExchange( TProperty comparand, TProperty newValueIfSameAsComparand )
      {
         return this._compareExchangeMethod( comparand, newValueIfSameAsComparand );
      }

      #endregion
   }
}
