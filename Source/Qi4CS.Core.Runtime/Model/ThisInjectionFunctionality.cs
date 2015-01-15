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
         var targetType = model.TargetType.GetGenericDefinitionIfContainsGenericParameters();

         return new ValidationResult(
            model.CompositeModel.GetAllCompositeTypes().Any( c => targetType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( c) ),
            "Injection target must be composite type"
            );
      }

      public Object ProvideInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         Object retVal;
         instance.Composites.TryGetValue(targetType, out retVal);
         return retVal;
      }

      public InjectionTime GetInjectionTime( AbstractInjectableModel model )
      {
         return InjectionTime.ON_CREATION;
      }

      #endregion
   }
}
