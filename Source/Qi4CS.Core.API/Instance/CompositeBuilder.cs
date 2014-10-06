/*
 * Copyright (c) 2009, Rickard Öberg (
 * org.qi4j.api.value.ValueBuilder class)
 * Copyright (c) 2007, Niclas Hedhman (
 * org.qi4j.api.composite.TransientBuilder class)
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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// <see cref="CompositeBuilder"/>s are used to instantiate new composites.
   /// The instances of <see cref="CompositeBuilder"/>s may be acquired from <see cref="StructureServiceProvider"/>.
   /// </summary>
   /// <seealso cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, System.Collections.Generic.IEnumerable{Type})"/>
   /// <seealso cref="E_Qi4CS.NewCompositeBuilder(StructureServiceProvider, CompositeModelType, Type)"/>
   /// <seealso cref="E_Qi4CS.NewCompositeBuilder{T}(StructureServiceProvider, CompositeModelType)"/>
   public interface CompositeBuilder : UsesProvider<CompositeBuilder>
   {
      /// <summary>
      /// Marks the current composite instance as no longer a prototype and returns an instance to the composite of the specified type.
      /// </summary>
      /// <param name="type">The type of the composite to return.</param>
      /// <returns>The composite of type <paramref name="type"/>.</returns>
      /// <exception cref="CompositeInstantiationException">If there were non-optional class properties that were <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If the composite does not have public nor private composites assignable to <paramref name="type"/>.</exception>
      /// <remarks>
      /// If this composite instance is a prototype, then during prototype disabling all methods marked with <see cref="API.Model.PrototypeAttribute"/> are invoked.
      /// </remarks>
      Object InstantiateWithType( Type type );

      /// <summary>
      /// Returns a reference to a composite instance of specified type.
      /// Since the returned composite is a prototype, the constraints for methods are not checked.
      /// Therefore it is possible to access and update even immutable properties.
      /// </summary>
      /// <param name="prototypeType">The type of the composite to return.</param>
      /// <returns>The composite of type <paramref name="prototypeType"/>.</returns>
      /// <exception cref="InvalidOperationException">If composite represented by this <see cref="CompositeBuilder"/> is no longer a prototype.</exception>
      /// <exception cref="ArgumentException">If the composite does not have public nor private composites assignable to <paramref name="prototypeType"/>.</exception>
      Object PrototypeFor( Type prototypeType );
   }
}

/// <summary>
/// This class contains all extension methods for types in Qi4CS.Core.* namespaces.
/// </summary>
public static partial class E_Qi4CS
{
   /// <summary>
   /// Helper method to easily invoke <see cref="CompositeBuilder.InstantiateWithType(Type)"/> method when type is known at compile-time.
   /// </summary>
   /// <typeparam name="TComposite">The type of the composite.</typeparam>
   /// <param name="builder">The <see cref="CompositeBuilder"/>.</param>
   /// <returns>The return value of <see cref="CompositeBuilder.InstantiateWithType(Type)"/> casted to <typeparamref name="TComposite"/>.</returns>
   public static TComposite Instantiate<TComposite>( this CompositeBuilder builder )
   {
      return (TComposite) builder.InstantiateWithType( typeof( TComposite ) );
   }

   /// <summary>
   /// Helper method to easily invoke <see cref="CompositeBuilder.PrototypeFor(Type)"/> method when type is known at compile-time.
   /// </summary>
   /// <typeparam name="TComposite">The type of the composite.</typeparam>
   /// <param name="builder">The <see cref="CompositeBuilder"/>.</param>
   /// <returns>The return value of <see cref="CompositeBuilder.PrototypeFor(Type)"/> casted to <typeparamref name="TComposite"/>.</returns>
   public static TComposite Prototype<TComposite>( this CompositeBuilder builder )
   {
      return (TComposite) builder.PrototypeFor( typeof( TComposite ) );
   }
}

