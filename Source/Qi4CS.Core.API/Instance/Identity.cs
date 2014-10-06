/*
 * Copyright (c) 2007, Rickard Öberg.
 * Copyright (c) 2007, Niclas Hedhman.
 * See NOTICE file.
 * 
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
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This interface will be implemented by all service composites.
   /// </summary>
   /// <remarks>
   /// TODO only getter for identity. Since the information comes from model layer, it shouldn't be settable.
   /// Then predefined mixin would be added to service's mixin declaration, implementing this interface through using [Structure] CompositeModel injection.
   /// </remarks>
   public interface Identity
   {
      /// <summary>
      /// Gets the textual identity of the service.
      /// </summary>
      /// <value>The textual identity of the service.</value>
      /// <remarks>
      /// The identity of the service may be specified during bootstrapping stage.
      /// If the identity is not specified, Qi4CS runtime will generate the identity automatically.
      /// </remarks>
      [Immutable]
      String Identity { get; set; }
   }
}
