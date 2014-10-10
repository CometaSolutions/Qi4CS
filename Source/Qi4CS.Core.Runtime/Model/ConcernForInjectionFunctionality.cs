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
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class ConcernForInjectionFunctionality : InjectionFunctionality
   {

      #region InjectionFunctionality Members

      public ValidationResult InjectionPossible( SPI.Model.AbstractInjectableModel model )
      {
         var targetType = model.TargetType;
         if ( targetType.ContainsGenericParameters() && targetType.IsGenericType() )
         {
            targetType = targetType.GetGenericTypeDefinition();
         }
         return new ValidationResult(
            model.CompositeModel.ApplicationModel.GenericFragmentBaseType.IsAssignableFrom( targetType ) || model.CompositeModel.GetAllCompositeTypes().Any( tt => targetType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( tt ) ),
            "Concern must be either generic or a sub-type of composite type."
            );
      }

      public Object ProvideInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         return ( (CompositeInstanceImpl) instance ).CreateOrGetConcernInvocationBase( targetType );
      }

      public InjectionTime GetInjectionTime( SPI.Model.AbstractInjectableModel model )
      {
         return InjectionTime.ON_CREATION;
      }

      #endregion
   }
}
