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
   /// This attribute should be used on fields and parameters which should be injected with the element providing the invocation result.
   /// </summary>
   /// <remarks>
   /// The type of the injectable element should be the same as concern type itself, or a <see cref="Instance.GenericInvocator"/>.
   /// </remarks>
   /// <example>
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="SideEffectForAttributeCode1" language="C#" />
   /// In the code above, the <c>MyStuffSideEffect</c> uses <see cref="SideEffectForAttribute"/> to inject the <c>result</c> field in order to get invocation result of concerns (if any) and a mixin.
   /// Alternatively, the <c>MyStuffSideEffect</c> could extend <c>SideEffectOf&lt;MyStuff&gt;</c> in order to get the injected field from that class, and implement <c>MyStuff</c>; the result would be equivalent to the one provided in this example.
   /// </example>
   [InjectionScope]
   [FragmentDependentInjection]
   [AttributeUsage( AttributeTargets.Field | AttributeTargets.Parameter )]
   public sealed class SideEffectForAttribute : Attribute
   {

   }
}
