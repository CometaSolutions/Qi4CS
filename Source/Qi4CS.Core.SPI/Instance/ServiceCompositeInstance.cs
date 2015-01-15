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

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This interface extends <see cref="CompositeInstance"/> to provide information specific to service composites.
   /// </summary>
   public interface ServiceCompositeInstance : CompositeInstance
   {
      /// <summary>
      /// Checks whether this service composite is active.
      /// The service composite is activated either with Qi4CS <see cref="API.Instance.Application"/> or when someone tries to use any of the composite methods.
      /// </summary>
      /// <value><c>true</c> if this service composite is active; <c>false</c> otherwise.</value>
      Boolean Active { get; }

      /// <summary>
      /// Gets the textual service composite ID within the <see cref="API.Instance.Application"/> it belongs to.
      /// </summary>
      /// <value>The textual service composite ID within the <see cref="API.Instance.Application"/> it belongs to.</value>
      /// <remarks>
      /// This service may not be unique within the scope of whole <see cref="API.Instance.Application"/>.
      /// It will be unique within whatever architectural unit is considered smallest in current Qi4CS application.
      /// In SingletonArchitecture, that is the whole application.
      /// In LayeredArchitecture, that is Module.
      /// </remarks>
      String ServiceID { get; }
   }
}
