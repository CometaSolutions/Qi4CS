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
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using CommonUtils;

namespace Qi4CS.Core.Runtime.Model
{
   public class ThisInjectionFunctionality : InjectionFunctionality
   {

      #region InjectionFunctionality Members

      public ValidationResult InjectionPossible( SPI.Model.AbstractInjectableModel model )
      {
         var targetType = model.TargetType;
         if ( targetType.IsGenericType )
         {
            targetType = targetType.GetGenericTypeDefinition();
         }
         Type[] thisTypes = this.GetThisTypesToCheck( model.CompositeModel, targetType );
         return new ValidationResult(
            thisTypes.All( thisType => model.CompositeModel.Methods.Select( cMethod => cMethod.NativeInfo.ReflectedType ).Any( type => thisType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( type ) ) ),
            "Injection target must be composite type"
            );
      }

      public Object ProvideInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         return instance.Composites.Values.Where( composite => targetType.IsAssignableFrom( composite.GetType() ) ).FirstOrDefault();
      }

      public InjectionTime GetInjectionTime( AbstractInjectableModel model )
      {
         return InjectionTime.ON_CREATION;
      }

      #endregion

      protected virtual Type[] GetThisTypesToCheck( CompositeModel compositeModel, Type targetType )
      {
         var types = targetType.GetAllParentTypes().ToArray();
         Type[] result;
         if ( types.SelectMany( type => type.GetMethods() ).Any() )
         {
            result = Empty<Type>.Array;
         }
         else
         {
            if ( !targetType.GetMethods().Any() )
            {
               result = types.Where( tType => tType.GetMethods().Any() ).GetBottomTypes();
            }
            else
            {
               result = new Type[] { targetType };
            }
         }
         return result;
      }
   }
}
