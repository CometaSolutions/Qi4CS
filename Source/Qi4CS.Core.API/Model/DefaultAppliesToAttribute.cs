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
using System.Collections.Generic;
using System.Linq;

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// Using this attribute the fragment implementor may specify default way to apply a fragment implementing <see cref="Instance.GenericInvocator"/> interface.
   /// This default way will be acting as a fallback behaviour for applying fragment onto composite methods. 
   /// </summary>
   /// <remarks>
   /// <para>
   /// The <see cref="Type"/> given to constructors of this attribute may be
   /// <list type="bullet">
   /// <item><description>an attribute type,</description></item>
   /// <item><description>a class or interface type which should match one of the composite types, or</description></item>
   /// <item><description>a class or interface type implementing <see cref="AppliesToFilter"/>.</description></item>
   /// </list>
   /// </para>
   /// <example>
   /// <para>
   /// Example with attribute type given to this attribute constructor:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="AppliesToCode1" language="C#"/>
   /// </para>
   /// <para>
   /// The DoSomethingWithSession() method has [Sessional] attribute, therefore the SessionConcern will be placed in the call sequence of that method.
   /// And because DoSomethingWithoutSession() does not have [Sessional] attribute, the SessionConcern will not be placed in the call sequence of that method.
   /// The [Sessional] attribute can be placed either on the interface method or the implementation method, depending on whether it is a contract or implementation detail.
   /// </para>
   /// <para>
   /// Example with composite type given to this attribute constuctor:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="AppliesToCode2" language="C#"/>
   /// </para>
   /// <para>
   /// The MethodB will have its mixin be the GenericBMixin.
   /// The methods MethodA and MethodC however, will not have the GenericBMixin as their mixin.
   /// </para>
   /// <para>
   /// Finally, the example with implementation of <see cref="AppliesToFilter"/> given to this attribute constuctor:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="AppliesToCode3" language="C#"/>
   /// </para>
   /// <para>
   /// In this example, the <c>MyAppliesToFilter.AppliesTo</c> method will be invoked twice.
   /// One of the invocations will pass <c>MyComposite.FirstMethod</c> and <c>MyGenericMixin.Invoke</c> methods as <c>compositeMethod</c> and <c>fragmentMethod</c> parameters, respectively.
   /// Another one of the invocations will pass <c>MyComposite.SecondMethod</c> and <c>MyGenericMixin.Invoke</c> methods as <c>compositeMethod</c> and <c>fragmentMethod</c> parameters, respectively.
   /// Whether <c>MyGenericMixin.Invoke</c> method is added as mixin to either of these methods depends on whether MyAppliesToFilter.AppliesTo returns <c>true</c> on any of those invocations.
   /// </para>
   /// </example>
   /// </remarks>
   /// <seealso cref="AppliesToFilter"/>
   [AttributeUsage( AttributeTargets.Class )]
   public class DefaultAppliesToAttribute : Attribute
   {
      private readonly Type[] _filters;

      /// <summary>
      /// Creates new <see cref="DefaultAppliesToAttribute"/> with a single <see cref="Type"/> as filter.
      /// </summary>
      /// <param name="filter">The filter type (see description of this class).</param>
      public DefaultAppliesToAttribute( Type filter )
         : this( new[] { filter } )
      {
      }

      /// <summary>
      /// Creates new <see cref="DefaultAppliesToAttribute"/> with multiple <see cref="Type"/>s as filters.
      /// </summary>
      /// <param name="filters">The filter types (see description of this class).</param>
      public DefaultAppliesToAttribute( params Type[] filters )
      {
         this._filters = filters;
      }

      /// <summary>
      /// Gets the filter types (see description of this class) given to the constructor.
      /// </summary>
      /// <value>The filter types given to the constructor.</value>
      /// <remarks>It is enough for one filter type to match fragment method, so this enumerable can be considered as an 'or'.</remarks>
      public IEnumerable<Type> Filters
      {
         get
         {
            return this._filters.Skip( 0 );
         }
      }

   }
}
