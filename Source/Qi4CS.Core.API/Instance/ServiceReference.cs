/*
 * Copyright (c) 2008, Rickard Öberg.
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
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This interface provides access to and information about Qi4CS service composites without actually referencing to the composite itself.
   /// The actual service composite is accessible through <see cref="ServiceReference.GetServiceWithType(Type)"/> or <see cref="E_Qi4CS.GetService{T}(ServiceReference)"/> methods.
   /// </summary>
   public interface ServiceReference
   {
      /// <summary>
      /// Checks whether the service composite referenced to by this <see cref="ServiceReference"/> is active.
      /// </summary>
      /// <value>
      /// <c>true</c> if the service composite referenced to by this <see cref="ServiceReference"/> is active; <c>false</c> otherwise.
      /// </value>
      Boolean Active { get; }

      /// <summary>
      /// Gets the textual value of service identity.
      /// </summary>
      /// <value>
      /// The textual value of service identity.
      /// </value>
      /// <seealso cref="Identity"/>
      String ServiceID { get; }

      /// <summary>
      /// Activates the service, if it is not activated already.
      /// </summary>
      void Activate();

      /// <summary>
      /// Gets the reference to service composite of specified type.
      /// </summary>
      /// <param name="serviceType">The required type of the service composite.</param>
      /// <returns>Service composite castable to <paramref name="serviceType"/>.</returns>
      /// <exception cref="ArgumentException">If <paramref name="serviceType"/> is not publicly visible type of the model of the referenced service composite, or if the composite with given type is not found.</exception>
      Object GetServiceWithType( Type serviceType );
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Helper method to get correctly typed service composite reference from <see cref="ServiceReference"/>.
   /// </summary>
   /// <typeparam name="TService">The type of the service to cast.</typeparam>
   /// <param name="reference">The <see cref="ServiceReference"/>.</param>
   /// <returns>The service composite returned by <see cref="ServiceReference.GetServiceWithType(Type)"/> method casted to <typeparamref name="TService"/>.</returns>
   /// <seealso cref="ServiceReference.GetServiceWithType(Type)"/>
   public static TService GetService<TService>( this ServiceReference reference )
   {
      return (TService) reference.GetServiceWithType( typeof( TService ) );
   }
}

