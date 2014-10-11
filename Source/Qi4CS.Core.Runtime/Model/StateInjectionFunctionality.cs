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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.Runtime.Instance;

namespace Qi4CS.Core.Runtime.Model
{
   public class StateInjectionFunctionality : InjectionFunctionality
   {

      #region InjectionFunctionality Members

      public ValidationResult InjectionPossible( SPI.Model.AbstractInjectableModel model )
      {
         var targetType = model.TargetType;
         Boolean possible = typeof( CompositeState ).IsAssignableFrom( targetType );
         ValidationResult result;
         if ( possible )
         {
            result = new ValidationResult( typeof( CompositeState ).IsAssignableFrom( targetType ), null );
         }
         else
         {
            var attribute = model.InjectionScope as StateAttribute;
            if ( attribute != null )
            {
               var suitableModels = this.FindSuitable<EventInfo, EventModel>(
                  model.CompositeModel,
                  model,
                  attribute.ElementName,
                  attribute.DeclaringType,
                  method => method.EventModel,
                  GetActualTargetType<CompositeEvent>( targetType ),
                  eventInfo => eventInfo.EventHandlerType
                  ).ToArray();
               var suitablePModels = this.FindSuitable<PropertyInfo, PropertyModel>(
                  model.CompositeModel,
                  model,
                  attribute.ElementName,
                  attribute.DeclaringType,
                  method => method.PropertyModel,
                  GetActualTargetType<CompositeProperty>( targetType ),
                  pInfo => pInfo.PropertyType
                  ).ToArray();
               Int32 totalLength = suitableModels.Length + suitablePModels.Length;
               result = new ValidationResult( ( model.IsOptional && totalLength <= 1 ) || ( !model.IsOptional && totalLength == 1 ), totalLength == 0 ? "No suitable events nor properties found." : ( "Too much suitable events and/or properties: " + String.Join( ", ", (Object[]) suitableModels ) + String.Join( ", ", (Object[]) suitablePModels ) + "." ) );
            }
            else
            {
               result = new ValidationResult( false, "Injection scope of " + model + " was not of type " + typeof( StateAttribute ) );
            }
         }
         return result;
      }

      public Object ProvideInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         var scope = model.InjectionScope;
         Object result;
         if ( typeof( CompositeState ).IsAssignableFrom( targetType ) )
         {
            result = instance.State;
         }
         else
         {
            var attribute = (StateAttribute) scope;
            var isCompositeProperty = typeof( CompositeProperty ).IsAssignableFrom( targetType );
            var prop = this.FindSuitable<CompositeProperty, PropertyInfo>(
                  instance.State.Properties.Values,
                  attribute.DeclaringType,
                  attribute.ElementName,
                  GetActualTargetType<CompositeProperty>( targetType ),
                  cProp => cProp.ReflectionInfo,
                  pInfo => pInfo.PropertyType
                  );
            if ( prop != null )
            {
               result = isCompositeProperty ? prop : prop.PropertyValueAsObject;
            }
            else
            {
               var isCompositeEvent = typeof( CompositeEvent ).IsAssignableFrom( targetType );
               var evt = this.FindSuitable<CompositeEvent, EventInfo>(
                  instance.State.Events.Values,
                  attribute.DeclaringType,
                  attribute.ElementName,
                  GetActualTargetType<CompositeEvent>( targetType ),
                  cEvent => cEvent.ReflectionInfo,
                  eventInfo => eventInfo.EventHandlerType
                  );
               if ( evt != null )
               {
                  result = isCompositeEvent ? evt : evt.InvokeActionAsObject;
               }
               else
               {
                  result = null;
               }
            }
         }
         return result;
      }

      public InjectionTime GetInjectionTime( AbstractInjectableModel model )
      {
         var targetType = model.TargetType;
         var attribute = (StateAttribute) model.InjectionScope;
         return typeof( CompositeProperty ).IsAssignableFrom( targetType )
            || typeof( CompositeEvent ).IsAssignableFrom( targetType )
            || !this.FindSuitable<PropertyInfo, PropertyModel>(
               model.CompositeModel,
               model,
               attribute.ElementName,
               attribute.DeclaringType,
               method => method.PropertyModel,
               GetActualTargetType<CompositeProperty>( targetType ),
               pInfo => pInfo.PropertyType
               ).Any() ?
               InjectionTime.ON_CREATION :
               InjectionTime.ON_METHOD_INVOKATION;
      }

      #endregion

      protected TCompositeElement FindSuitable<TCompositeElement, TMemberInfo>(
         IEnumerable<TCompositeElement> elements,
         Type declaringType,
         String name,
         Type targetType,
         Func<TCompositeElement, TMemberInfo> memberInfoGetter,
         Func<TMemberInfo, Type> targetTypeGetter
         )
         where TMemberInfo : MemberInfo
      {
         return elements.Where( element =>
            ( name == null || name.Equals( memberInfoGetter( element ).Name ) )
            && ( declaringType == null || declaringType.Equals( memberInfoGetter( element ).DeclaringType ) || declaringType.Equals( memberInfoGetter( element ).DeclaringType.GetGenericDefinitionIfGenericType() ) )
            && ( targetType == null || targetType.Equals( targetTypeGetter( memberInfoGetter( element ) ) ) )
            ).FirstOrDefault();
      }

      protected IEnumerable<TModel> FindSuitable<TMemberInfo, TModel>(
         CompositeModel compositeModel,
         AbstractInjectableModel model,
         String name,
         Type declaringType,
         Func<CompositeMethodModel, TModel> methodModelGetter,
         Type targetType,
         Func<TMemberInfo, Type> memberInfoTargetTypeGetter
         )
         where TMemberInfo : MemberInfo
         where TModel : AbstractMemberInfoModel<TMemberInfo>
      {
         //         Type mixinDeclaringType = model is ParameterModel ? ( (ParameterModel) model ).NativeInfo.Member.DeclaringType : ( (FieldModel) model ).NativeInfo.DeclaringType;
         return compositeModel.Methods
            .Select( method => methodModelGetter( method ) )
            .Where( targetModel => targetModel != null )
            .Distinct()
            .Where( targetModel =>
               ( name == null || name.Equals( targetModel.NativeInfo.Name ) )
               && ( declaringType == null || declaringType.Equals( targetModel.NativeInfo.DeclaringType.GetGenericDefinitionIfContainsGenericParameters() ) )
               && (targetType == null || ReflectionHelper.AreStructurallySame(targetType, memberInfoTargetTypeGetter(targetModel.NativeInfo), true))
            );
      }

      public static Type GetActualTargetType<TCompositeStateParticipant>( Type targetType )
      {
         return typeof( TCompositeStateParticipant ).IsAssignableFrom( targetType ) ?
            ( Object.Equals( typeof( TCompositeStateParticipant ), targetType ) ? null : targetType.GetGenericArguments()[0] )
            : targetType;
      }
   }
}
