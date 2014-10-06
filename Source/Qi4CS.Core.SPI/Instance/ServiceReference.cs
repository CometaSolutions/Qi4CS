/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
using System.Linq;
using System.Text;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This is SPI version of <see cref="ServiceReference"/>.
   /// It provides ways to access SPI information about references to service composites.
   /// </summary>
   public interface ServiceReferenceSPI : ServiceReference
   {
      /// <summary>
      /// Gets the model of the service composite referenced by this <see cref="ServiceReferenceSPI"/>.
      /// </summary>
      /// <value>The model of the service composite referenced by this <see cref="ServiceReferenceSPI"/>.</value>
      ServiceCompositeModel Model { get; }
   }
}
