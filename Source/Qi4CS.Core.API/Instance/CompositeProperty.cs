/*
 * Copyright (c) 2007, Rickard Öberg.
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
using System.Reflection;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This interface provides ways to access and manipulate C# property which are part of composite state.
   /// The interface is without generic type arguments for situations when type of property is not known.
   /// The instances of this interface are obtained via <see cref="API.Model.StateAttribute"/> injection or from <see cref="CompositeState.Properties"/>.
   /// </summary>
   /// <seealso cref="CompositeProperty{T}"/>
   /// <seealso cref="API.Model.StateAttribute"/>
   /// <seealso cref="CompositeState.Properties"/>
   public interface CompositeProperty : CompositeStateParticipant<PropertyInfo>
   {
      /// <summary>
      /// Gets or sets the property value as <see cref="Object"/>.
      /// </summary>
      /// <value>The property value as <see cref="Object"/>.</value>
      /// <remarks>Both getter and setter are atomic and threadsafe.</remarks>
      /// <exception cref="InvalidCastException">If object of wrong type is given to setter.</exception>
      Object PropertyValueAsObject { get; set; }

      /// <summary>
      /// Performs an atomic exchange operation to this property.
      /// </summary>
      /// <param name="newValue">New property value as <see cref="Object"/>.</param>
      /// <returns>The old value of the property.</returns>
      /// <exception cref="InvalidCastException">If <paramref name="newValue"/> is of wrong type.</exception>
      /// <seealso cref="M:System.Threading.Interlocked.Exchange(System.Object@,System.Object)"/>
      Object ExchangeAsObject( Object newValue );
      /// <summary>
      /// Performs an atomic compare-exchange operation to this property.
      /// </summary>
      /// <param name="comparand">The object that the property value is compared to, as <see cref="Object"/>.</param>
      /// <param name="newValueIfSameAsComparand">The object that will replace the old property value if it is equal to <paramref name="comparand"/>, as <see cref="Object"/>.</param>
      /// <returns>The original value of this property.</returns>
      /// <exception cref="InvalidCastException">If either of <paramref name="comparand"/> and <paramref name="newValueIfSameAsComparand"/> is of wrong type.</exception>
      /// <seealso cref="System.Threading.Interlocked.CompareExchange(ref Object,Object,Object)"/>
      Object CompareExchangeAsObject( Object comparand, Object newValueIfSameAsComparand );
   }

   /// <summary>
   /// This interface provides ways to access and manipulate a C# property which is part of composite state.
   /// The interface is with generic type parameter, which should match exactly the type of the property.
   /// The instances of this interface are obtained via <see cref="API.Model.StateAttribute"/> injection or from <see cref="CompositeState.Properties"/>.
   /// </summary>
   /// <typeparam name="TProperty">The type of the property.</typeparam>
   /// <seealso cref="API.Model.StateAttribute"/>
   /// <seealso cref="CompositeState.Properties"/>
   public interface CompositeProperty<TProperty> : CompositeProperty
   {
      /// <summary>
      /// Gets or sets the property value.
      /// </summary>
      /// <value>The property value.</value>
      /// <remarks>Both getter and setter are atomic and threadsafe.</remarks>
      TProperty PropertyValue { get; set; }

      /// <summary>
      /// Performs an atomic exchange operation to this property.
      /// </summary>
      /// <param name="newValue">New property value.</param>
      /// <returns>The old value of the property.</returns>
      /// <seealso cref="M:System.Threading.Interlocked.Exchange(System.Object@,System.Object)"/>
      TProperty Exchange( TProperty newValue );

      /// <summary>
      /// Performs an atomic compare-exchange operation to this property.
      /// </summary>
      /// <param name="comparand">The value that the property value is compared to.</param>
      /// <param name="newValueIfSameAsComparand">The value that will replace the old property value if it is equal to <paramref name="comparand"/>.</param>
      /// <returns>The original value of this property.</returns>
      /// <seealso cref="System.Threading.Interlocked.CompareExchange(ref Object,Object,Object)"/>
      TProperty CompareExchange( TProperty comparand, TProperty newValueIfSameAsComparand );
   }
}
