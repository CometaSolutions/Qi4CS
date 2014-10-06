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
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Extensions.Functional.Instance;

namespace Qi4CS.Extensions.Functional.Model
{
   internal class FunctionInvocationDataInjectionFunctionality : InjectionFunctionality
   {
      internal static readonly Type SCOPE = typeof( FunctionInvocationDataAttribute );
      internal static readonly InjectionFunctionality INSTANCE = new FunctionInvocationDataInjectionFunctionality();

      #region InjectionFunctionality Members

      public ValidationResult InjectionPossible( AbstractInjectableModel model )
      {
         return new ValidationResult( true, null );
      }

      public Object ProvideInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         KeyedRoleMap roleMap = RoleMapHolder.INVOCATION_DATA.Value.RoleMap;
         Object result;
         if ( roleMap.ThisHasRole( targetType ) )
         {
            result = roleMap.Get<Object>( targetType );
         }
         else
         {
            FunctionInvocationDataAttribute attr = (FunctionInvocationDataAttribute) model.InjectionScope;
            result = attr.GetFactory( targetType ).NewInvocationData();
            roleMap.Set( result );
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
