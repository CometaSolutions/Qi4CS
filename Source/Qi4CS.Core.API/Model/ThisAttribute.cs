/*
 * Copyright (c) 2007, Rickard Öberg.
 * Copyright (c) 2007, Niclas Hedhman.
 * See NOTICE file.
 * 
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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// This injection scope attribute may be used to inject references to the composites that are part of the same composite instance as the fragment.
   /// The private composites (i.e. types that are not part of the public types of the composite) may be referenced with this injection scope attribute as well.
   /// </summary>
   /// <remarks>
   /// Calls to the injected composite will have same effect as if the method would have been called from outside the composite.
   /// </remarks>
   [InjectionScope, AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false )]
   public sealed class ThisAttribute : Attribute
   {
   }
}
