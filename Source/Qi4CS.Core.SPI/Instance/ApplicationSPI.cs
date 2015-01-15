/*
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
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This is abstract of instance pool for certain type.
   /// In Windows Phone 8 and Silverlight 5 builds, this is a simple instance pool using lockless algorithms to implement its methods.
   /// In other builds, this is a thin wrapper around <see cref="T:System.Collections.Concurrent.ConcurrentBag`1"/>.
   /// </summary>
   /// <typeparam name="T">The type of instances.</typeparam>
   public sealed class InstancePool<T>
   {
#if SILVERLIGHT
      private readonly LocklessInstancePoolGeneric<T> _pool;
#else
      private readonly System.Collections.Concurrent.ConcurrentBag<T> _pool;
#endif

      /// <summary>
      /// Creates new instance of <see cref="InstancePool{T}"/>.
      /// </summary>
      public InstancePool()
      {
#if SILVERLIGHT
         this._pool = new LocklessInstancePoolGeneric<T>();
#else
         this._pool = new System.Collections.Concurrent.ConcurrentBag<T>();
#endif
      }

      /// <summary>
      /// Attempts to remove an instance from this <see cref="InstancePool{T}"/>.
      /// </summary>
      /// <param name="item">When this method returns, this will contain instance of <typeparamref name="T"/> if this method returned <c>true</c>, or default value of <typeparamref name="T"/> if this method returned <c>false</c>.</param>
      /// <returns><c>true</c> if an instance was acquired successfully; <c>false</c> otherwise.</returns>
      public Boolean TryTake( out T item )
      {
         return this._pool.TryTake( out item );
      }

      /// <summary>
      /// Attemts to fetch an instance from this <see cref="InstancePool{T}"/> without removing it.
      /// </summary>
      /// <param name="item">When this method returns, this will contain instance of <typeparamref name="T"/> if this method returned <c>true</c>, or default value of <typeparamref name="T"/> if this method returned <c>false</c>.</param>
      /// <returns><c>true</c> if an instance was fetched successfully; <c>false</c> otherwise.</returns>
      public Boolean TryPeek( out T item )
      {
         return this._pool.TryPeek( out item );
      }

      /// <summary>
      /// Returns an existing instance to this <see cref="InstancePool{T}"/>.
      /// </summary>
      /// <param name="item">The instance to return.</param>
      public void Return( T item )
      {
#if SILVERLIGHT
         this._pool.ReturnInstance( item );
#else
         this._pool.Add( item );
#endif
      }
   }

   /// <summary>
   /// This interface is SPI version of <see cref="Application"/>.
   /// It provides additional methods, mostly to be used by generated code or by extensions.
   /// </summary>
   /// <remarks>
   /// All instances of <see cref="Application"/> may be casted to this interface.
   /// </remarks>
   public interface ApplicationSPI : Application
   {
      /// <summary>
      /// Gets instance pool for specific constraint type.
      /// </summary>
      /// <param name="resolvedConstraintType">The type of the constraint.</param>
      /// <returns>Instance pool for given constraint type.</returns>
      InstancePool<Object> GetConstraintInstancePool( Type resolvedConstraintType );
      /// <summary>
      /// Gets the <see cref="InjectionService"/> of this <see cref="ApplicationSPI"/>.
      /// </summary>
      /// <value>The <see cref="InjectionService"/> of this <see cref="ApplicationSPI"/>.</value>
      InjectionService InjectionService { get; }

      /// <summary>
      /// Given a reference to a composite or a fragment, tries to get the related <see cref="CompositeInstance"/> object of the composite or fragment.
      /// </summary>
      /// <param name="compositeOrFragment">The composite or fragment.</param>
      /// <returns>The related <see cref="CompositeInstance"/> object.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="compositeOrFragment"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If <paramref name="compositeOrFragment"/> is not composite or fragment.</exception>
      CompositeInstance GetCompositeInstance( Object compositeOrFragment );

      /// <summary>
      /// Gets the <see cref="CollectionsWithRoles.API.CollectionsFactory"/> associated with this <see cref="ApplicationSPI"/>.
      /// This is helper property so that the static value of <see cref="CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY"/> property would not be needed to use.
      /// </summary>
      /// <value>The <see cref="CollectionsWithRoles.API.CollectionsFactory"/> instance of this <see cref="ApplicationSPI"/>.</value>
      CollectionsFactory CollectionsFactory { get; }

      /// <summary>
      /// Gets the <see cref="Model.ApplicationModel{T}"/> of this <see cref="ApplicationSPI"/>.
      /// </summary>
      /// <value>The <see cref="Model.ApplicationModel{T}"/> of this <see cref="ApplicationSPI"/>.</value>
      ApplicationModel<ApplicationSPI> ApplicationModel { get; }
   }
}
