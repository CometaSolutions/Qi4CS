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
   /// Attribute to denote that something is optional.
   /// </summary>
   /// <remarks>
   /// <para>
   /// There exist the following scenarios for using <see cref="OptionalAttribute"/>.
   /// <list type="bullet">
   /// <item><description>If applied to a method parameter (including return value "parameter"), the value is then allowed to be <c>null</c>. Default behaviour is that reference type and nullable parameters have to be non-<c>null</c>.</description></item>
   /// <item><description>If applied to a property declaration, then getter may return <c>null</c> and setter may accept <c>null</c>. For properties that are part of composite state, this means that the property value may be <c>null</c> after the composite is instantiated.</description></item>
   /// <item><description>If applied to an injected field, it is allowed that injection fails.</description></item>
   /// </list>
   /// For all scenarios, applying <see cref="OptionalAttribute"/> to elements with value type has no effect, except for <see cref="Nullable{T}"/>.
   /// </para>
   /// <para>
   /// Optionality is not default in Qi4CS, and if injections, property values and parameters (including return value "parameter") in methods are <c>null</c>, the Qi4CS runtime will throw <see cref="API.Instance.ConstraintViolationException"/> indicating which field, property or parameter in which composite the problem has been detected.
   /// In case of injections, a <see cref="Model.InjectionException"/> is thrown.
   /// </para>
   /// </remarks>
   /// <example>
   /// <para>
   /// If one has the following interface declaration
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="OptionalCode1" language="C#" />
   /// then assuming <c>c</c> is instantiated composite of type <c>TestComposite</c>, the call <c>c.MethodWithMandatoryParameter(null)</c> would cause <see cref="API.Instance.ConstraintViolationException"/> to be thrown, but the call <c>c.MethodWithOptionalParameter(null)</c> would pass through to mixin.
   /// </para>
   /// <para>
   /// If one has the following interface declaration
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="OptionalCode2" language="C#" />
   /// then assuming <c>b</c> is of type <see cref="API.Instance.CompositeBuilder"/>, the call <see cref="E_Qi4CS.Instantiate{T}(API.Instance.CompositeBuilder)">b.Instantiate&lt;TestComposite2&gt;()</see> will not throw <see cref="API.Instance.ConstraintViolationException"/> even if <c>MyProperty</c> will be <c>null</c> at instantiation time.
   /// </para>
   /// <para>
   /// If one has the following composite and mixin
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="OptionalCode3" language="C#" />
   /// then whenever control flow enters <c>DoSomething</c> method of <c>TestComposite3Mixin</c>, the <c>_service</c> field might be <c>null</c> if at runtime the suitable service could not be found. Without the <see cref="OptionalAttribute"/> applied to the field, if the service could not be found at runtime, an <see cref="API.Model.InjectionException"/> will be thrown.
   /// </para>
   /// </example>
   [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue )]
   public sealed class OptionalAttribute : Attribute
   {
      /// <summary>
      /// Provides easy access for an instance of this attribute when passing it to other objects.
      /// </summary>
      public static OptionalAttribute VALUE = new OptionalAttribute();


      /// <summary>
      /// All instances of <see cref="OptionalAttribute"/> are considered to be equal.
      /// Therefore this returns true if <paramref name="obj"/> is of type <see cref="OptionalAttribute"/>.
      /// </summary>
      /// <param name="obj">The other object to check equality against.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is non-<c>null</c> and is of type <see cref="OptionalAttribute"/>; <c>false</c> otherwise.</returns>
      public override Boolean Equals( Object obj )
      {
         return obj is OptionalAttribute;
      }

      /// <summary>
      /// All instances of <see cref="OptionalAttribute"/> are considered to be equal.
      /// Therefore this returns the hash code of <see cref="OptionalAttribute"/> type.
      /// </summary>
      /// <returns>The hash code of <see cref="OptionalAttribute"/> type.</returns>
      public override Int32 GetHashCode()
      {
         return typeof( OptionalAttribute ).GetHashCode();
      }
   }
}
