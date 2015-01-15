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
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// A model for service composites.
   /// It is specialized version of <see cref="CompositeModel"/>, adding some methods specific for service composites.
   /// </summary>
   public interface ServiceCompositeModel : CompositeModel, ActivationObservable<ServiceActivationArgs, ServiceActivationArgs>
   {
      /// <summary>
      /// Gets the textual service ID supplied by application architecture.
      /// Will be <c>null</c> if the application architecture did not supply custom service ID.
      /// </summary>
      /// <value>The textual service ID supplied by application architecture, or <c>null</c>.</value>
      String ServiceID { get; }

      /// <summary>
      /// Gets the value indicating whether instance(s) of this service should be activated along with <see cref="ApplicationSPI"/> which was constructed from the <see cref="ApplicationModel{T}"/> this <see cref="ServiceCompositeModel"/> belongs to.
      /// </summary>
      /// <value><c>true</c> if instance(s) of this service should be activated along with <see cref="ApplicationSPI"/> which was constructed from the <see cref="ApplicationModel{T}"/> this <see cref="ServiceCompositeModel"/> belongs to; <c>false</c> otherwise.</value>
      Boolean ActivateWithApplication { get; }
   }

   /// <summary>
   /// This is event args for <see cref="ActivationObservable{T,U}.AfterActivation"/> and <see cref="ActivationObservable{T,U}.AfterPassivation"/> events of <see cref="ServiceCompositeModel"/>.
   /// </summary>
   public sealed class ServiceActivationArgs : EventArgs
   {
      private readonly CompositeInstance _service;

      /// <summary>
      /// Creates a new instance of <see cref="ServiceActivationArgs"/>.
      /// </summary>
      /// <param name="service">The <see cref="CompositeInstance"/> of the service composite.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="service"/> is <c>null</c>.</exception>
      public ServiceActivationArgs( CompositeInstance service )
      {
         ArgumentValidator.ValidateNotNull( "Service", service );

         this._service = service;
      }

      /// <summary>
      /// Gets the <see cref="CompositeInstance"/> of the service being activated.
      /// </summary>
      /// <value>The <see cref="CompositeInstance"/> of the service being activated.</value>
      public CompositeInstance Service
      {
         get
         {
            return this._service;
         }
      }
   }
}
