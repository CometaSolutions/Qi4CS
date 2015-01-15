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

// This kind of "event declarer decides storage style" type of model causes problems at code generation (especially in wp8)
// Additionally, it is also wrong in a way that it is the event user who should decide the storage style (strong or weak) of event handler to register.

//using System;

//namespace Qi4CS.Core.API.Model
//{
//   /// <summary>
//   /// By using this attribute, one can control how Qi4CS runtime stores the events which part part of composite state.
//   /// </summary>
//   /// <seealso cref="API.Instance.CompositeState"/>
//   /// <seealso cref="EventStorage"/>
//   [AttributeUsage( AttributeTargets.Event, AllowMultiple = false )]
//   public sealed class EventStorageStyleAttribute : Attribute
//   {
//      private readonly EventStorage _storage;

//      /// <summary>
//      /// Creates new instance of <see cref="EventStorageStyleAttribute"/> with specified <see cref="EventStorage"/>.
//      /// </summary>
//      /// <param name="storage">The enum telling how events should be stored.</param>
//      /// <seealso cref="EventStorage"/>
//      public EventStorageStyleAttribute( EventStorage storage )
//      {
//         this._storage = storage;
//      }

//      /// <summary>
//      /// Gets the event storage for the associated event.
//      /// </summary>
//      /// <value>The event storage for the associated event.</value>
//      /// <seealso cref="EventStorage"/>
//      public EventStorage Storage
//      {
//         get
//         {
//            return this._storage;
//         }
//      }

//      /// <summary>
//      /// The default storage style for events is strong reference.
//      /// </summary>
//      /// <seealso cref="EventStorage"/>
//      public const EventStorage DEFAULT_STORAGE = EventStorage.STRONG_REFS;
//   }

//   /// <summary>
//   /// <para>
//   /// This enum provides two different storage styles for events, strong reference and weak reference.
//   /// </para>
//   /// <para>
//   /// With <see cref="EventStorage.STRONG_REFS"/>, the event handlers are stored using strong references, in identical way how C# compiler does it.
//   /// </para>
//   /// <para>
//   /// With <see cref="EventStorage.WEAK_REFS"/>, the event handlers are stored using weak references, and Qi4CS generates code which will handle the addition, removal, and cleanup of such event so that adding to and removing from the event requires same code in both cases.
//   /// </para>
//   /// </summary>
//   public enum EventStorage
//   {
//      /// <summary>
//      /// Using this event storage style, the event handlers are stored using strong references, in identical way how C# compiler does it.
//      /// </summary>
//      STRONG_REFS,

//      /// <summary>
//      /// Using this event storage style, the event handlers are stored using weak references, and Qi4CS generates code which will handle the addition, removal, and cleanup of such event so that adding to and removing from the event requires same code as with strong reference style.
//      /// </summary>
//      WEAK_REFS
//   }
//}
