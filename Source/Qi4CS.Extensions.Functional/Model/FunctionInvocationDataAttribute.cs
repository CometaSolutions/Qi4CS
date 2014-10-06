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
using System.Threading;
using Qi4CS.Core.API.Model;
using CommonUtils;

namespace Qi4CS.Extensions.Functional.Model
{
   /// <summary>
   /// This injection scope attribute may be used to inject data, using some type implementing <see cref="FunctionInvocationDataFactory"/> as data factory.
   /// The injection will be checked on every composite method invocation.
   /// Once the data is created, it will be visible to all composites which are directly or indirectly used by composite which obtained injection, via this same attribute.
   /// </summary>
   /// <example>
   /// Let's assume we have two composites and mixins for them like follows:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="FunctionInvocationDataAttributeCode1" language="C#" />
   /// </example>
   [InjectionScope, AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false )]
   public class FunctionInvocationDataAttribute : Attribute
   {
      private FunctionInvocationDataFactory _factory;
      private readonly Type _factoryType;

      /// <summary>
      /// Creates a new <see cref="FunctionInvocationDataAttribute"/> which will use <see cref="FunctionInvocationDataFactoryUsingParameterlessCtor{T}"/> as factory for the data.
      /// </summary>
      /// <seealso cref="FunctionInvocationDataFactoryUsingParameterlessCtor{T}"/>
      public FunctionInvocationDataAttribute()
         : this( null )
      {

      }

      /// <summary>
      /// Creates a new <see cref="FunctionInvocationDataAttribute"/> which will use instance of given <paramref name="factoryType"/> as <see cref="FunctionInvocationDataFactory"/> for this injection.
      /// </summary>
      /// <param name="factoryType">The type implementing <see cref="FunctionInvocationDataFactory"/>.</param>
      public FunctionInvocationDataAttribute( Type factoryType )
      {
         this._factoryType = factoryType ?? typeof( FunctionInvocationDataFactoryUsingParameterlessCtor<> );
      }

      /// <summary>
      /// Gets the <see cref="FunctionInvocationDataFactory"/> associated with this attribute.
      /// If it is not yet created, attempts to create it.
      /// </summary>
      /// <param name="targetType">The target type of the injection.</param>
      /// <returns>The <see cref="FunctionInvocationDataFactory"/> associated with this attribute.</returns>
      /// <exception cref="InvalidOperationException">If factory type given to this attribute has more than one generic argument or contains open generic parameter or if parmaeterless constructor is not found.</exception>
      public FunctionInvocationDataFactory GetFactory( Type targetType )
      {
         if ( this._factory == null )
         {
            var type = ( this._factoryType.IsGenericTypeDefinition ? this._factoryType.MakeGenericType( targetType ) : this._factoryType );
            var ctor = type.GetConstructor( Empty<Type>.Array );
            if ( ctor == null )
            {
               throw new InvalidOperationException( "Could not find parameterless constructor for " + type + "." );
            }
            Interlocked.Exchange( ref this._factory, (FunctionInvocationDataFactory) ctor.Invoke( null ) );
         }
         return this._factory;
      }
   }

   /// <summary>
   /// This interface is used to create instances of data associated with <see cref="FunctionInvocationDataAttribute"/> injection.
   /// </summary>
   public interface FunctionInvocationDataFactory
   {
      /// <summary>
      /// Creates a new instance of data castable to injection target type.
      /// </summary>
      /// <returns>A new instance of data castable to injection target type.</returns>
      Object NewInvocationData();
   }

   /// <summary>
   /// Default implementation of <see cref="FunctionInvocationDataFactory"/> which uses constraint to ensure the target type has parameterless constructor.
   /// </summary>
   /// <typeparam name="T">The target type for injection.</typeparam>
   public class FunctionInvocationDataFactoryUsingParameterlessCtor<T> : FunctionInvocationDataFactory
      where T : new()
   {
      #region FunctionInvocationDataFactory Members

      /// <inheritdoc />
      public Object NewInvocationData()
      {
         return new T();
      }

      #endregion
   }
}
