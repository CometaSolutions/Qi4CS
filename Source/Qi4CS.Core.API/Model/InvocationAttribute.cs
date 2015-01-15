/*
 * Copyright (c) 2007, Rickard Öberg.
 * Copyright (c) 2007, Niclas Hedhman.
 * See NOTICE file.
 * 
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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// This injection scope attribute can be used to inject invocation-related items.
   /// </summary>
   /// <remarks>
   /// The acceptable targets are:
   /// <list type="bullet">
   /// <item><description>a <see cref="System.Reflection.MethodInfo"/> of the currently invoked composite method,</description></item>
   /// <item><description>any sub-type of <see cref="Attribute"/>, assuming composite method or any fragment method in composite method invocation chain contains such attribute,</description></item>
   /// <item><description>an <see cref="T:Qi4CS.Core.SPI.Instance.AttributeHolder"/>, which gives access to attributes applied to composite method or any fragment method in composite method invocation chain,</description></item>
   /// <item><description>a <see cref="T:Qi4CS.Core.SPI.Model.CompositeMethodModel"/> for the current composite method, or</description></item>
   /// <item><description>any sub-type of <see cref="T:Qi4CS.Core.SPI.Model.AbstractFragmentMethodModel"/> for the current fragment method.</description></item>
   /// </list>
   /// </remarks>
   [InjectionScope, AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false )]
   public class InvocationAttribute : Attribute
   {
   }
}
