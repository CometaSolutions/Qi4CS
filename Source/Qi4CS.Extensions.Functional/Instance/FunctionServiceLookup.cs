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

namespace Qi4CS.Extensions.Functional.Instance
{
   /// <summary>
   /// This interface may be implemented by types of aggregator service so that it would be possible to change behaviour at runtime.
   /// </summary>
   /// <typeparam name="TKey">The type of the key used to index aggregated objects.</typeparam>
   /// <typeparam name="TComposite">The type all aggregated objects should implement.</typeparam>
   public interface FunctionServiceLookup<TKey, TComposite>
   {
      /// <summary>
      /// Registers aggregated objects.
      /// </summary>
      /// <param name="key">The key used in search of aggregated objects.</param>
      /// <param name="composite">The lazily initializable aggregated object.</param>
      void RegisterFunction( TKey key, Lazy<TComposite> composite );
      /// <summary>
      /// Unregisters aggregated objects with given key.
      /// </summary>
      /// <param name="key">The key used in search of aggregated objects.</param>
      void UnregisterFunction( TKey key );

      /// <summary>
      /// Checks whether this aggreator service has aggregated object for given <paramref name="key"/>.
      /// </summary>
      /// <param name="key">The key to check.</param>
      /// <returns><c>true</c> if this aggregator service has aggregated object for given <paramref name="key"/>; <c>false</c> otherwise.</returns>
      Boolean HasFunctionFor( TKey key );
   }
}
