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
   /// This injection scope attribute can be used to inject things related to composite state.
   /// </summary>
   /// <remarks>
   /// Possible target types are the following:
   /// <list type="definition">
   /// <item><term>CompositeState</term><description>The reference to the <see cref="API.Instance.CompositeState"/> will be injected. The <see cref="StateAttribute.DeclaringType"/> and <see cref="StateAttribute.ElementName"/> propertis will be ignored in this case.</description></item>
   /// <item><term>CompositeProperty</term><description>The reference to the <see cref="API.Instance.CompositeProperty"/> or <see cref="API.Instance.CompositeProperty{T}"/>. In latter case, the type of the generic argument and in both cases optionally <see cref="StateAttribute.DeclaringType"/> and <see cref="StateAttribute.ElementName"/> are used to filter suitable property.</description></item>
   /// <item><term>CompositeEvent</term><description>The reference to the <see cref="API.Instance.CompositeEvent"/> or <see cref="API.Instance.CompositeEvent{T}"/>. The latter case, the type of the generic argument and in both cases optionally <see cref="StateAttribute.DeclaringType"/> and <see cref="StateAttribute.ElementName"/> are used to filter suitable event.</description></item>
   /// <item><term><c>PropertyOrEventType</c></term><description>The current value of composite property or invocation action for composite event typed <c>PropertyOrEventType</c> is injected. Optionally, <see cref="StateAttribute.DeclaringType"/> and <see cref="StateAttribute.ElementName"/> are used to filter suitable property or event.</description></item>
   /// </list>
   /// </remarks>
   /// <seealso cref="API.Instance.CompositeState"/>
   /// <seealso cref="API.Instance.CompositeProperty"/>
   /// <seealso cref="API.Instance.CompositeProperty{T}"/>
   /// <seealso cref="API.Instance.CompositeEvent"/>
   /// <seealso cref="API.Instance.CompositeEvent{T}"/>
   /// <example>
   /// <para>
   /// Assuming we have the following composite
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="StateAttributeCode1" language="C#" />
   /// then the following are legal injections:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="StateAttributeCode2" language="C#" />
   /// </para>
   /// </example>
   [InjectionScope, AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false )]
   public class StateAttribute : Attribute
   {

      private readonly Type _declaringType;
      private readonly String _name;

      /// <summary>
      /// Creates a new <see cref="StateAttribute"/>.
      /// Only the type of the injectable element will be used to filter suitable result.
      /// </summary>
      public StateAttribute()
         : this( null, null )
      {

      }

      /// <summary>
      /// Creates a new <see cref="StateAttribute"/> with given <see cref="ElementName"/>.
      /// The type of the injectable element and <see cref="ElementName"/> will be used to filter suitable result.
      /// </summary>
      /// <param name="elementName">The name of the property or event in case there are multiple with same type.</param>
      public StateAttribute( String elementName )
         : this( null, elementName )
      {

      }

      /// <summary>
      /// Creates a new <see cref="StateAttribute"/> with given <see cref="DeclaringType"/> and <see cref="ElementName"/>.
      /// The type of the injectable element and both <see cref="DeclaringType"/> and <see cref="ElementName"/> will be used to filter suitable result.
      /// </summary>
      /// <param name="declaringType">The declaring type of the property or event.</param>
      /// <param name="elementName">The name of the property or event.</param>
      public StateAttribute( Type declaringType, String elementName )
      {
         this._declaringType = declaringType;
         this._name = elementName;
      }

      /// <summary>
      /// Gets the declaring type of the property or event to be injected.
      /// </summary>
      /// <value>The declaring type of the property or event to be injected.</value>
      public Type DeclaringType
      {
         get
         {
            return this._declaringType;
         }
      }

      /// <summary>
      /// Gets the name of the property or event to be injected.
      /// </summary>
      /// <value>The name of the property or event to be injected.</value>
      public String ElementName
      {
         get
         {
            return this._name;
         }
      }
   }
}
