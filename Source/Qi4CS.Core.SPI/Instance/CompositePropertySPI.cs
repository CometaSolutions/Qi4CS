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
using System.Reflection;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This is SPI version of <see cref="CompositeProperty"/>.
   /// It provides a property to query <see cref="PropertyModel"/> associated with this <see cref="CompositePropertySPI"/>.
   /// All instances of <see cref="CompositeProperty"/> and <see cref="CompositeProperty{T}"/> are castable to this type.
   /// </summary>
   public interface CompositePropertySPI : CompositeProperty, CompositeStateParticipantSPI<PropertyInfo, PropertyModel>
   {
   }
}
