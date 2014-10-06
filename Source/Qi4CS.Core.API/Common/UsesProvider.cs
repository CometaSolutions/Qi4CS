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
using Qi4CS.Core.API.Common;

namespace Qi4CS.Core.API.Common
{
   /// <summary>
   /// This is common interface for <see cref="API.Instance.CompositeBuilder"/>, <see cref="API.Instance.StructureServiceProvider"/> and other types.
   /// This interface provides a way to pass information to composites being created via <see cref="Model.UsesAttribute"/> injection.
   /// </summary>
   /// <typeparam name="TUsesProvider">The type of the type implementing this interface.</typeparam>
   /// <seealso cref="Model.UsesAttribute"/>
   /// <seealso cref="E_Qi4CS.Use{T}(UsesProvider{T}, Object[])"/>
   public interface UsesProvider<out TUsesProvider>
      where TUsesProvider : UsesProvider<TUsesProvider>
   {
      /// <summary>
      /// Passes <paramref name="value"/> as named or unnamed object to be used via <see cref="Model.UsesAttribute"/> injection. 
      /// </summary>
      /// <param name="name">The name of the object to pass. May be <c>null</c> for unnamed object.</param>
      /// <param name="value">The object to pass.</param>
      /// <returns>This object.</returns>
      /// <remarks>
      /// The <paramref name="value"/> will be visible via <see cref="Model.UsesAttribute(String)"/> constructor.
      /// If <paramref name="value"/> is <c>null</c>, then it will not be added to this <see cref="UsesProvider{T}"/>.
      /// </remarks>
      /// <seealso cref="Model.UsesAttribute"/>
      /// <seealso cref="E_Qi4CS.Use{T}(UsesProvider{T}, Object[])"/>
      TUsesProvider UseWithName( String name, Object value );
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Passes <paramref name="objects"/> as unnamed objects to be used via <see cref="Qi4CS.Core.API.Model.UsesAttribute"/> injection. 
   /// </summary>
   /// <typeparam name="TUses">The generic parameter for <see cref="UsesProvider{T}"/>.</typeparam>
   /// <param name="uses">The <see cref="UsesProvider{T}"/>.</param>
   /// <param name="objects">The objects to pass.</param>
   /// <returns>This object.</returns>
   /// <seealso cref="Qi4CS.Core.API.Model.UsesAttribute"/>
   /// <exception cref="NullReferenceException">If <paramref name="uses"/> is <c>null</c>.</exception>
   public static TUses Use<TUses>( this UsesProvider<TUses> uses, params Object[] objects )
      where TUses : UsesProvider<TUses>
   {
      if ( objects != null )
      {
         foreach ( var obj in objects )
         {
            uses.UseWithName( null, obj );
         }
      }
      return (TUses) uses;
   }
}
