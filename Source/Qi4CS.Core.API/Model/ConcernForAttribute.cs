/*
 * Copyright (c) 2008, Niclas Hedhman.
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
   /// This attribute should be used on fields and parameters which should be injected with the next concern or mixin in composite method invocation chain.
   /// </summary>
   /// <remarks>
   /// The type of the injectable element should be the same as concern type itself, or a <see cref="Instance.GenericInvocator"/>.
   /// </remarks>
   /// <example>
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="ConcernOfCode1" language="C#" />
   /// In the code above, the <c>MyStuffConcern</c> uses <see cref="ConcernForAttribute"/> to inject the <c>next</c> field in order to continue composite method invocation chain to next concern or mixin.
   /// Alternatively, the <c>MyStuffConcern</c> could extend <c>ConcernOf&lt;MyStuff&gt;</c> in order to get the injected field from that class, and implement <c>MyStuff</c>; the result would be equivalent to the one provided in this example.
   /// </example>
   [InjectionScope]
   [FragmentDependentInjection]
   [AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter )]
   public sealed class ConcernForAttribute : Attribute
   {

   }
}
