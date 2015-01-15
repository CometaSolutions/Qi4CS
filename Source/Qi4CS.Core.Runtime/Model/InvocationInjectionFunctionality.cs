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
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class InvocationInjectionFunctionality : InjectionFunctionality
   {

      #region InjectionFunctionality Members

      public ValidationResult InjectionPossible( AbstractInjectableModel model )
      {
         // TODO might check for model here already if the type is assignable from Attribute.
         var targetType = model.TargetType;
         return new ValidationResult(
            Object.Equals( typeof( MethodInfo ), targetType ) ||
            Object.Equals( typeof( AttributeHolder ), targetType ) ||
            typeof( Attribute ).IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( targetType ) ||
            Object.Equals( typeof( CompositeMethodModel ), targetType ) ||
            typeof( AbstractFragmentMethodModel ).IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( targetType ),
            "Target type must be either " + typeof( MethodInfo ) + ", " + typeof( AttributeHolder ) + ", or any sub-type of " + typeof( Attribute ) + ", or " + typeof( CompositeMethodModel ) + ", or any sub-type of " + typeof( AbstractFragmentMethodModel ) + "." );
      }

      public Object ProvideInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         InvocationInfo invocationInfo = instance.InvocationInfo;
         Object result;
         if ( Object.Equals( typeof( MethodInfo ), targetType ) )
         {
            result = invocationInfo.CompositeMethod;
         }
         else if ( Object.Equals( typeof( CompositeMethodModel ), targetType ) )
         {
            result = invocationInfo.FragmentMethodModel.CompositeMethod;
         }
         else if ( typeof( AbstractFragmentMethodModel ).IsAssignableFrom( targetType ) )
         {
            result = invocationInfo.FragmentMethodModel;
         }
         else
         {
            AttributeHolder holder = instance.ModelInfo.CompositeMethodAttributeHolders[invocationInfo.FragmentMethodModel.CompositeMethod.MethodIndex];
            if ( typeof( Attribute ).IsAssignableFrom( targetType ) )
            {
               ListQuery<Attribute> list;
               holder.AllAttributes.TryFindInTypeDictionarySearchBottommostType( targetType, out list );
               result = list.FirstOrDefault();
            }
            else // target type must be attribute holder
            {
               result = holder;
            }
         }

         return result;
      }

      public InjectionTime GetInjectionTime( AbstractInjectableModel model )
      {
         return InjectionTime.ON_METHOD_INVOKATION;
      }

      #endregion
   }
}
