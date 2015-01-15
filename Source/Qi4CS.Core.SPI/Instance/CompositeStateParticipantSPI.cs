using System.Reflection;
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
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This is SPI version of <see cref="CompositeStateParticipant{T}"/>.
   /// It serves as common interface for <see cref="CompositePropertySPI"/> and <see cref="CompositeEventSPI"/>.
   /// </summary>
   /// <typeparam name="TReflectionInfo">The reflection object of this property or event.</typeparam>
   /// <typeparam name="TModel">The type of the Qi4CS model associated with this state participant.</typeparam>
   /// <seealso cref="CompositeStateParticipant{T}"/>
   public interface CompositeStateParticipantSPI<TReflectionInfo, TModel> : CompositeStateParticipant<TReflectionInfo>
      where TReflectionInfo : MemberInfo
   {
      /// <summary>
      /// Gets the Qi4CS model (<see cref="SPI.Model.PropertyModel"/> for properties and <see cref="SPI.Model.EventModel"/> for events) associated with this participant.
      /// </summary>
      /// <value>The Qi4CS model (<see cref="SPI.Model.PropertyModel"/> for properties and <see cref="SPI.Model.EventModel"/> for events) associated with this participant.</value>
      TModel Model { get; }
   }
}
