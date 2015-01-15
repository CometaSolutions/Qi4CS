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
using CollectionsWithRoles.API;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public abstract class StructureInjectionFunctionality : InjectionFunctionality
   {

      #region InjectionFunctionality Members

      public ValidationResult InjectionPossible( SPI.Model.AbstractInjectableModel model )
      {
         var targetType = model.TargetType;
         return new ValidationResult(
            targetType.IsAssignableFrom( typeof( StructureServiceProviderSPI ) )
            || targetType.IsAssignableFrom( typeof( ApplicationSPI ) )
            || targetType.IsAssignableFrom( typeof( UsesProviderQuery ) )
            || targetType.IsAssignableFrom( typeof( CollectionsFactory ) )
            || targetType.IsAssignableFrom( typeof( CompositeModel ) )
            || ( model.CompositeModel is ServiceCompositeModel && targetType.IsAssignableFrom( typeof( ServiceCompositeModel ) ) )
            || targetType.IsAssignableFrom( typeof( CompositeInstance ) )
            || this.ValidateNonStandardInjection( model.CompositeModel, model, model.InjectionScope, targetType ),
            "Target type must be one of the fixed structure types"
            );
      }

      public Object ProvideInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         Object result = null;
         if ( targetType.IsAssignableFrom( typeof( CollectionsFactory ) ) )
         {
            result = instance.StructureOwner.Application.CollectionsFactory;
         }
         else if ( targetType.IsAssignableFrom( typeof( ApplicationSPI ) ) )
         {
            result = instance.StructureOwner.Application;
         }
         else if ( targetType.IsAssignableFrom( typeof( StructureServiceProviderSPI ) ) )
         {
            result = this.GetStructureServiceProvider( instance, model, targetType );
         }
         else if ( targetType.IsAssignableFrom( typeof( UsesProviderQuery ) ) )
         {
            result = instance.UsesContainer;
         }
         else if ( targetType.IsAssignableFrom( typeof( CompositeModel ) ) || targetType.IsAssignableFrom( typeof( ServiceCompositeModel ) ) )
         {
            result = instance.ModelInfo.Model;
         }
         else if ( targetType.IsAssignableFrom( typeof( CompositeInstance ) ) )
         {
            result = instance;
         }
         else
         {
            result = this.ProvideNonStandardInjection( instance, model, targetType );
         }
         return result;
      }

      public InjectionTime GetInjectionTime( AbstractInjectableModel model )
      {
         return InjectionTime.ON_CREATION;
      }

      #endregion

      protected abstract StructureServiceProviderSPI GetStructureServiceProvider( CompositeInstance instance, AbstractInjectableModel model, Type targetType );

      protected virtual Object ProvideNonStandardInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         return null;
      }

      protected virtual Boolean ValidateNonStandardInjection( SPI.Model.CompositeModel compositeModel, SPI.Model.AbstractInjectableModel model, Attribute scope, Type targetType )
      {
         return false;
      }
   }
}
