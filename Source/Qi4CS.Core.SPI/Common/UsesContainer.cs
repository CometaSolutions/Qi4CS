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
using System.Collections.Generic;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.SPI.Common;

namespace Qi4CS.Core.SPI.Common
{
   /// <summary>
   /// This is mutable role interface for <see cref="UsesProvider{T}"/>.
   /// </summary>
   public interface UsesContainerMutable : UsesProvider<UsesContainerMutable>
   {
      /// <summary>
      /// Gets the query role interface (<see cref="UsesContainerQuery"/>) for this <see cref="UsesContainerMutable"/>.
      /// </summary>
      /// <value>The query role interface (<see cref="UsesContainerQuery"/>) for this <see cref="UsesContainerMutable"/>.</value>
      UsesContainerQuery Query { get; }
   }

   /// <summary>
   /// This interface may be queried for objects provided via <see cref="M:E_Qi4CS.Use``1(Qi4CS.Core.API.Common.UsesProvider{``0},System.Object[])"/> and <see cref="UsesProvider{T}.UseWithName(String, Object)"/> methods.
   /// Unnamed objects are stores with <c>null</c> as name.
   /// </summary>
   public interface UsesProviderQuery
   {
      /// <summary>
      /// Gets named or unnamed object which is assignable from given type.
      /// </summary>
      /// <param name="type">The type for result to be assignable from.</param>
      /// <param name="name">The name of the object. May be <c>null</c> if unnamed object should be searched.</param>
      /// <returns>
      /// The suitable object provided via <see cref="M:E_Qi4CS.Use``1(Qi4CS.Core.API.Common.UsesProvider{``0},System.Object[])"/> and <see cref="UsesProvider{T}.UseWithName(String, Object)"/> methods.
      /// If the suitable object is not found, returns <c>null</c>.
      /// Also returns <c>null</c> if <paramref name="type"/> is <c>null</c>.
      /// </returns>
      /// <seealso cref="E_Qi4CS.Get{T}(UsesProviderQuery)"/>
      /// <seealso cref="E_Qi4CS.Get(UsesProviderQuery, Type)"/>
      /// <seealso cref="E_Qi4CS.GetForName{T}(UsesProviderQuery, String)"/>
      Object GetObjectForName( Type type, String name );
   }

   /// <summary>
   /// This interface extends <see cref="UsesProviderQuery"/> to provide more complex methods for querying state.
   /// Most importantly, the hierarchical nature of <see cref="UsesProviderQuery"/> is exposed via <see cref="Parent"/> property.
   /// </summary>
   public interface UsesContainerQuery : UsesProviderQuery
   {
      /// <summary>
      /// Checks whether this <see cref="UsesContainerQuery"/> or its parent, if this has one, has object with given name and whether the object type same or sub-type of given type.
      /// </summary>
      /// <param name="type">The type of the object to be castable to.</param>
      /// <param name="name">The name of the object. May be <c>null</c> for unnamed objects.</param>
      /// <returns><c>true</c> if this <see cref="UsesContainerQuery"/> has object with given <paramref name="name"/> and that object can be casted to <paramref name="type"/>.</returns>
      Boolean HasValue( Type type, String name );

      /// <summary>
      /// Gets all types of all objects in this <see cref="UsesContainerQuery"/> and its parent, if this has one.
      /// </summary>
      /// <value>All types of all objects in this <see cref="UsesContainerQuery"/> and its parent, if this has one.</value>
      IEnumerable<Type> ContainedTypes { get; }

      /// <summary>
      /// Gets the parent <see cref="UsesContainerQuery"/>, if this has one.
      /// Will be <c>null</c> if this <see cref="UsesContainerQuery"/> doesn't have a parent.
      /// </summary>
      /// <value>The parent <see cref="UsesContainerQuery"/>, if this has one.</value>
      UsesContainerQuery Parent { get; }

      /// <summary>
      /// Gets information about all the named objects in this <see cref="UsesContainerQuery"/>.
      /// The parent <see cref="UsesContainerQuery"/>, if this has one, is not queried.
      /// </summary>
      /// <value>Information about all the named objects in this <see cref="UsesContainerQuery"/>, excluding transitively named objects in <see cref="Parent"/>.</value>
      IEnumerable<KeyValuePair<String, Object>> ThisNamedObjects { get; }

      /// <summary>
      /// Gets information about all the unnamed objects in this <see cref="UsesContainerQuery"/>.
      /// The parent <see cref="UsesContainerQuery"/>, if this has one, is not queried.
      /// </summary>
      /// <value>Information about all the unnamed objects in this <see cref="UsesContainerQuery"/>, excluding transitively unnamed objects in <see cref="Parent"/>.</value>
      IEnumerable<Object> ThisUnnamedObjects { get; }
   }
}

#pragma warning disable 1591
public static partial class E_Qi4CS
#pragma warning restore 1591
{
   /// <summary>
   /// Helper method to get unnamed object from <see cref="UsesProviderQuery"/> when the type of the object is known at compile time.
   /// </summary>
   /// <typeparam name="T">The type of the object to get.</typeparam>
   /// <param name="uses">The <see cref="UsesProviderQuery"/>.</param>
   /// <returns>Result of <see cref="UsesProviderQuery.GetObjectForName(Type, String)"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="uses"/> is <c>null</c> or if <typeparamref name="T"/> is value type and <see cref="UsesProviderQuery.GetObjectForName(Type, String)"/> returned <c>null</c>.</exception>
   /// <seealso cref="UsesProviderQuery.GetObjectForName(Type, String)"/>
   public static T Get<T>( this UsesProviderQuery uses )
   {
      return (T) uses.Get( typeof( T ) );
   }

   /// <summary>
   /// Helper method to get named object from <see cref="UsesProviderQuery"/> when the type of the object is known at compile time.
   /// </summary>
   /// <typeparam name="T">The type of the object to get.</typeparam>
   /// <param name="uses">The <see cref="UsesProviderQuery"/>.</param>
   /// <param name="name">The name of the object. May be <c>null</c> in order to search for unnamed object.</param>
   /// <returns>Result of <see cref="UsesProviderQuery.GetObjectForName(Type, String)"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="uses"/> is <c>null</c> or if <typeparamref name="T"/> is value type and <see cref="UsesProviderQuery.GetObjectForName(Type, String)"/> returned <c>null</c>.</exception>
   /// <seealso cref="UsesProviderQuery.GetObjectForName(Type, String)"/>
   public static T GetForName<T>( this UsesProviderQuery uses, String name )
   {
      return (T) uses.GetObjectForName( typeof( T ), name );
   }

   /// <summary>
   /// Helper method to get unnamed object from <see cref="UsesProviderQuery"/>.
   /// </summary>
   /// <param name="uses">The <see cref="UsesProviderQuery"/>.</param>
   /// <param name="type">The type of the unnamed object to get.</param>
   /// <returns>Result of <see cref="UsesProviderQuery.GetObjectForName(Type, String)"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="uses"/> is <c>null</c>.</exception>
   /// <seealso cref="UsesProviderQuery.GetObjectForName(Type, String)"/>
   public static Object Get( this UsesProviderQuery uses, Type type )
   {
      return uses.GetObjectForName( type, null );
   }
}