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
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.SPI.Instance;
using System.Reflection;
using System;

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This is SPI version of <see cref="CompositeEvent"/>.
   /// It provides a property to query <see cref="EventModel"/> associated with this <see cref="CompositeEventSPI"/>.
   /// All instances of <see cref="CompositeEvent"/> and <see cref="CompositeEvent{T}"/> are castable to this type.
   /// </summary>
   public interface CompositeEventSPI : CompositeEvent, CompositeStateParticipantSPI<EventInfo, EventModel>
   {
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// This is helper method to get event field value from the <see cref="CompositeEvent{T}"/>.
   /// </summary>
   /// <typeparam name="TEvent">The type of the event handler.</typeparam>
   /// <param name="evt">The <see cref="CompositeEvent{T}"/>.</param>
   /// <returns>The value of the field holding event handlers.</returns>
   ///// <exception cref="InvalidOperationException">If the event represented by given <see cref="CompositeEvent{T}"/> is not stored using strong references.</exception>
   ///// <seealso cref="Qi4CS.Core.API.Model.EventStorage"/>
   ///// <seealso cref="Qi4CS.Core.API.Model.EventStorageStyleAttribute"/>
   public static TEvent GetEventFieldValue<TEvent>( this CompositeEvent<TEvent> evt )
      where TEvent : class
   {
      //// TODO this should actually be a method of CompositeEvent interfaces. Since it is not possible to write this same method cleanly as extension method for CompositeEvent (without the generic parameter)
      //if ( ( (CompositeEventSPI) evt ).Model.GetEventStorageKind() != Qi4CS.Core.API.Model.EventStorage.STRONG_REFS )
      //{
      //   throw new InvalidOperationException( "Getting field value of event is only possible for events stored as strong reference." );
      //}

      return evt.InvokeFunctionWithRef<EventInfo, TEvent>( ReturnParam );
   }


   private static TEvent ReturnParam<TEvent>( ref TEvent evt )
   {
      return evt;
   }
}
