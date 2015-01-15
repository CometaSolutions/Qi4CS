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
using System.Reflection;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This interface provides ways to access and manipulate a C# event which is part of composite state.
   /// The interface is without generic arguments for situations when type of event is not known.
   /// The instances of this interface are obtained via <see cref="API.Model.StateAttribute"/> injection or from <see cref="CompositeState.Events"/>.
   /// </summary>
   /// <seealso cref="CompositeEvent{T}"/>
   /// <seealso cref="API.Model.StateAttribute"/>
   /// <seealso cref="CompositeState.Events"/>
   public interface CompositeEvent : CompositeStateParticipant<EventInfo>
   {
      /// <summary>
      /// Gets the action to invoke the event as <see cref="Object"/>.
      /// </summary>
      /// <value>The action to invoke the event as <see cref="Object"/>.</value>
      /// <remarks>The return value will never be <c>null</c>, but the action won't do anything if there are no event handlers registered to this event.</remarks>
      Object InvokeActionAsObject { get; }

      /// <summary>
      /// Adds new event handler to this event.
      /// </summary>
      /// <param name="handler">The event handler as object.</param>
      /// <exception cref="InvalidCastException">If <paramref name="handler"/> is of wrong type.</exception>
      /// <exception cref="ArgumentNullException">If <paramref name="handler"/> is <c>null</c>.</exception>
      void AddEventHandlerAsObject( Object handler );

      /// <summary>
      /// Removes an event handler from this event.
      /// </summary>
      /// <param name="handler">The event handler as object.</param>
      /// <exception cref="InvalidCastException">If <paramref name="handler"/> is of wrong type.</exception>
      /// <exception cref="ArgumentNullException">If <paramref name="handler"/> is <c>null</c>.</exception>
      void RemoveEventHandlerAsObject( Object handler );

      /// <summary>
      /// Returns <c>true</c> if this event has any event handlers; <c>false</c> otherwise.
      /// </summary>
      /// <value><c>true</c> if this event has any event handlers; <c>false</c> otherwise.</value>
      Boolean HasAnyEventHandlers { get; }

      /// <summary>
      /// Removes all event handlers from this event.
      /// </summary>
      void Clear();
   }

   /// <summary>
   /// This interface provides ways to access and manipulate a C# event which is part of composite state.
   /// The interface is with generic type parameter, which should match exactly the type of the event.
   /// The instances of this interface are obtained via <see cref="API.Model.StateAttribute"/> injection or from <see cref="CompositeState.Events"/>.
   /// </summary>
   /// <typeparam name="TEvent">The type of the event.</typeparam>
   /// <seealso cref="API.Model.StateAttribute"/>
   /// <seealso cref="CompositeState.Events"/>
   public interface CompositeEvent<TEvent> : CompositeEvent
      where TEvent : class
   {
      /// <summary>
      /// Gets the action to invoke this event.
      /// </summary>
      /// <value>The action to invoke this event.</value>
      /// <remarks>The return value will never be <c>null</c>, but the action won't do anything if there are no event handlers registered to this event.</remarks>
      TEvent InvokeAction { get; }

      /// <summary>
      /// Adds new event handler to this event.
      /// </summary>
      /// <param name="handler">The event handler.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="handler"/> is <c>null</c>.</exception>
      void AddEventHandler( TEvent handler );

      /// <summary>
      /// Removes an event handler from this event.
      /// </summary>
      /// <param name="handler">The event handler.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="handler"/> is <c>null</c>.</exception>
      void RemoveEventHandler( TEvent handler );
   }
}
