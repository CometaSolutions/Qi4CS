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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// By using this attribute, one can control how Qi4CS runtime invokes the event part of the composite state.
   /// </summary>
   /// <seealso cref="API.Instance.CompositeState"/>
   /// <seealso cref="EventInvocation"/>
   [AttributeUsage( AttributeTargets.Event, AllowMultiple = false )]
   public class EventInvocationStyleAttribute : Attribute
   {
      private readonly Type _rethrowExceptionType;
      private readonly EventInvocation _invocationStyle;

      /// <summary>
      /// Creates a new instance of <see cref="EventInvocationStyleAttribute"/> with specified event invocation style.
      /// </summary>
      /// <param name="invocationStyle">The <see cref="EventInvocation"/> style for target event.</param>
      /// <remarks>
      /// If <paramref name="invocationStyle"/> is <see cref="EventInvocation.INVOKE_ALL"/>, then the exception type will be <see cref="AggregateException"/>.
      /// </remarks>
      /// <seealso cref="EventInvocation"/>
      public EventInvocationStyleAttribute( EventInvocation invocationStyle )
         : this( invocationStyle, EventInvocation.INVOKE_ALL.Equals( invocationStyle ) ? DEFAULT_RETHROW_EXCEPTION_TYPE : null )
      {
      }

      /// <summary>
      /// Creates a new instance of <see cref="EventInvocationStyleAttribute"/> with <see cref="EventInvocation.INVOKE_ALL"/> invocation style and specified exception type to throw when any of event handlers throws an exception.
      /// </summary>
      /// <param name="rethrowExceptionType">The type of the exception to throw if any of the event handlers throws an exception.</param>
      /// <remarks>
      /// The exception type should have a constructor which takes an array of <see cref="Exception"/>s as its single parameter.
      /// </remarks>
      /// <seealso cref="EventInvocation"/>
      public EventInvocationStyleAttribute( Type rethrowExceptionType )
         : this( EventInvocation.INVOKE_ALL, rethrowExceptionType )
      {
      }

      private EventInvocationStyleAttribute( EventInvocation invocationStyle, Type rethrowExceptionType )
      {
         this._invocationStyle = invocationStyle;
         this._rethrowExceptionType = rethrowExceptionType;
      }

      /// <summary>
      /// Gets the <see cref="EventInvocation"/> style of the event this attribute is applied to.
      /// </summary>
      /// <value>The <see cref="EventInvocation"/> style of the event this attribute is applied to.</value>
      /// <seealso cref="EventInvocation"/>
      public EventInvocation InvocationStyle
      {
         get
         {
            return this._invocationStyle;
         }
      }

      /// <summary>
      /// Gets the <see cref="Type"/> of the exception to throw if any of the event handlers throws an exception.
      /// </summary>
      /// <value>The <see cref="Type"/> of the exception to throw if any of the event handlers throws an exception.</value>
      /// <remarks>
      /// Applicable only if <see cref="InvocationStyle"/> is <see cref="EventInvocation.INVOKE_ALL"/>.
      /// Is <c>null</c> otherwise.
      /// </remarks>
      /// <seealso cref="EventInvocation"/>
      public Type RethrowException
      {
         get
         {
            return this._rethrowExceptionType;
         }
      }

      /// <summary>
      /// The default invocation style is same as normally for events - direct invocation.
      /// </summary>
      /// <seealso cref="EventInvocation"/>
      public const EventInvocation DEFAULT_INVOCATION_STYLE = EventInvocation.INVOKE_DIRECTLY;

      /// <summary>
      /// The default type of the exception to throw if any of the event handlers throws exception in <see cref="EventInvocation.INVOKE_ALL"/> invocation style.
      /// </summary>
      public static readonly Type DEFAULT_RETHROW_EXCEPTION_TYPE = typeof( AggregateException );
   }

   /// <summary>
   /// <para>
   /// This enum provides two different invocation styles for events part of the composite state.
   /// </para>
   /// <para>
   /// With <see cref="EventInvocation.INVOKE_DIRECTLY"/>, each handler is invoked until either the list of the handlers ends or exception occurs.
   /// </para>
   /// <para>
   /// With <see cref="EventInvocation.INVOKE_ALL"/>, each handler is invoked even if some handler in between throws exception.
   /// After all handlers have been invoked, if any of the handlers threw an exception, all occurred exceptions are encapsulated in <see cref="AggregateException"/> or exception type specified by <see cref="EventInvocationStyleAttribute.RethrowException"/> property, and that exception is thrown.
   /// </para>
   /// </summary>
   /// <seealso cref="API.Instance.CompositeState"/>
   public enum EventInvocation
   {
      /// <summary>
      /// Using this event invocation style, each handler is invoked until either the list of the handlers ends or exception occurs.
      /// </summary>
      INVOKE_DIRECTLY,

      /// <summary>
      /// Using this event invocation style, each handler is invoked even if some handler in between throws exception.
      /// After all handlers have been invoked, if any of the handlers threw an exception, all occurred exceptions are encapsulated in <see cref="AggregateException"/> or exception type specified by <see cref="EventInvocationStyleAttribute.RethrowException"/> property, and that exception is thrown.
      /// </summary>
      INVOKE_ALL
   }
}
