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
   /// This injection scope attribute may be used to inject references to the Qi4CS environment.
   /// </summary>
   /// <remarks>
   /// By default, the valid targets are 
   /// <list type="bullet">
   /// <item><description><see cref="API.Instance.StructureServiceProvider"/>,</description></item>
   /// <item><description><see cref="Qi4CS.Core.API.Instance.Application"/>,</description></item>
   /// <item><description><see cref="T:Qi4CS.Core.SPI.Model.CompositeModel"/>,</description></item>
   /// <item><description><see cref="T:Qi4CS.Core.SPI.Instance.ApplicationSPI"/>, and</description></item>
   /// <item><description><see cref="T:Qi4CS.Core.SPI.Instance.CompositeInstance"/>.</description></item>
   /// </list>
   /// The Qi4CS architectures may provide additional targets.
   /// </remarks>
   [InjectionScope, AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false )]
   public class StructureAttribute : Attribute
   {
   }
}
