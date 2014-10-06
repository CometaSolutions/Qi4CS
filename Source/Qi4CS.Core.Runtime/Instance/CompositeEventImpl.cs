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
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Instance
{
   public class CompositeEventImpl<TEvent> : CompositeStateParticipantImpl<EventInfo, EventModel>, CompositeEventSPI, CompositeEvent<TEvent>
      where TEvent : class
   {
      private readonly TEvent _invokeAction;
      private readonly Action<TEvent> _addAction;
      private readonly Action<TEvent> _removeAction;
      private readonly Action _clearAction;
      private readonly Func<Boolean> _hasAnyHandlersFunc;

      public CompositeEventImpl(
         EventModel eventModel,
         EventInfo eventInfo,
         RefInvokerCallback refInvoker,
         TEvent invokeAction,
         Action<TEvent> addAction,
         Action<TEvent> removeAction,
         Action clearAction,
         Func<Boolean> hasAnyHandlersFunc
         )
         : base( eventModel, eventInfo, refInvoker )
      {
         ArgumentValidator.ValidateNotNull( "Event invocation action", invokeAction );
         ArgumentValidator.ValidateNotNull( "Event addition action", addAction );
         ArgumentValidator.ValidateNotNull( "Event removal action", removeAction );
         ArgumentValidator.ValidateNotNull( "Clear action", clearAction );
         ArgumentValidator.ValidateNotNull( "Field value checker", hasAnyHandlersFunc );

         this._invokeAction = invokeAction;
         this._addAction = addAction;
         this._removeAction = removeAction;
         this._clearAction = clearAction;
         this._hasAnyHandlersFunc = hasAnyHandlersFunc;
      }

      #region CompositeEvent Members

      public Object InvokeActionAsObject
      {
         get
         {
            return this._invokeAction;
         }
      }

      public void AddEventHandlerAsObject( Object handler )
      {
         this.AddEventHandler( (TEvent) handler );
      }

      public void RemoveEventHandlerAsObject( Object handler )
      {
         this.RemoveEventHandler( (TEvent) handler );
      }

      public void Clear()
      {
         this._clearAction();
      }

      public Boolean HasAnyEventHandlers
      {
         get
         {
            return this._hasAnyHandlersFunc();
         }
      }

      #endregion

      #region CompositeEvent<TEvent> Members

      public TEvent InvokeAction
      {
         get
         {
            return this._invokeAction;
         }
      }

      public void AddEventHandler( TEvent handler )
      {
         ArgumentValidator.ValidateNotNull( "Event handler", handler );
         this._addAction( handler );
      }

      public void RemoveEventHandler( TEvent handler )
      {
         ArgumentValidator.ValidateNotNull( "Event handler", handler );
         this._removeAction( handler );
      }

      #endregion
   }
}
