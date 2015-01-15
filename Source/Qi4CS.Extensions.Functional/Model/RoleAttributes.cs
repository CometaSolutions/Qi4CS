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
using Qi4CS.Core.API.Model;

namespace Qi4CS.Extensions.Functional.Model
{
   /// <summary>
   /// Common class for <see cref="RoleAttribute"/> and <see cref="RoleParameterAttribute"/>.
   /// </summary>
   public abstract class AbstractRoleAttribute : Attribute
   {
      private readonly String _name;

      /// <summary>
      /// This constructor only exists to satisfy CLS-compatibility, and will throw an exception.
      /// </summary>
      /// <exception cref="NotSupportedException">Always.</exception>
      protected AbstractRoleAttribute()
         : this( null )
      {
         throw new NotSupportedException();
      }

      internal AbstractRoleAttribute( String name )
      {
         this._name = name;
      }

      /// <summary>
      /// Gets the name of the role this attribute represents.
      /// </summary>
      /// <value>The name of the role this attribute represents.</value>
      public String Name
      {
         get
         {
            return this._name;
         }
      }
   }

   /// <summary>
   /// This is injection scope attribute to obtain parameters marked within aggregator service method by <see cref="RoleParameterAttribute"/>.
   /// </summary>
   /// <example>
   /// <para>Assume we have following aggregate service type:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="RoleAttributeCode1" language="C#" />
   /// </para>
   /// <para>
   /// Now assume that all aggregated objects must implement the following interface:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="RoleAttributeCode2" language="C#" />
   /// and it has been configured that <c>MyAggregateService.DoSomething</c> method is an aggregate method for <c>MyFunctionality.DoSomething</c> method (via <see cref="Assembling.FunctionAggregatorDeclaration{TKey, TComposite}"/>.
   /// </para>
   /// <para>
   /// The mixins implementing <c>MyFunctionality</c> may now access the parameters given to <c>MyAggregateService</c> via <see cref="RoleAttribute"/> injection, like this:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="RoleAttributeCode3" language="C#" />
   /// The string argument to <c>Int32</c> parameter is not necessary in this particular example, because the parameters are of different type, and the Qi4CS functional extension will know to inject correct-typed parameter.
   /// The textual string ID is useful to separate similarly-typed parameters from each other.
   /// </para>
   /// </example>
   /// <seealso cref="RoleParameterAttribute"/>
   [InjectionScope, AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false )]
   public sealed class RoleAttribute : AbstractRoleAttribute
   {
      /// <summary>
      /// Creates new instance of <see cref="RoleAttribute"/> which will search for target parameter based solely on type.
      /// </summary>
      public RoleAttribute()
         : this( null )
      {

      }

      /// <summary>
      /// Creates new instance of <see cref="RoleAttribute"/> which will search for target parameter based on type and given <paramref name="name"/>.
      /// </summary>
      /// <param name="name">The name of the parameter, should be unique within the signature of aggregator service method.</param>
      public RoleAttribute( String name )
         : base( name )
      {
      }
   }

   /// <summary>
   /// This attribute may be used to mark parameters within aggregator service methods that should be accessible via <see cref="RoleAttribute"/> injection from within aggregated objects.
   /// See <see cref="RoleAttribute"/> for code example.
   /// </summary>
   /// <seealso cref="RoleAttribute"/>
   [AttributeUsage( AttributeTargets.Parameter, AllowMultiple = false )]
   public sealed class RoleParameterAttribute : AbstractRoleAttribute
   {
      /// <summary>
      /// Creates new instance of <see cref="RoleParameterAttribute"/> which will store target parameter as unnamed parameter.
      /// </summary>
      public RoleParameterAttribute()
         : this( null )
      {

      }

      /// <summary>
      /// Creates new instance of <see cref="RoleParameterAttribute"/> which will store target parameter as named parameter, with the given <paramref name="name"/>.
      /// </summary>
      /// <param name="name">The name of the parameter to use to store parameter value.</param>
      public RoleParameterAttribute( String name )
         : base( name )
      {
      }
   }
}
