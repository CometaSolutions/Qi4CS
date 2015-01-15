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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils;

namespace Qi4CS.Core.API.Common
{
   /// <summary>
   /// Base interface for registering for activation related events.
   /// </summary>
   /// <typeparam name="TActivationEventArgs">The type of activation event arguments.</typeparam>
   /// <typeparam name="TPassivationEventArgs">The type of passivation event arguments.</typeparam>
   /// <seealso cref="API.Instance.Application"/>
   /// <seealso cref="T:Qi4CS.Core.SPI.Model.ServiceCompositeModel"/>
   public interface ActivationObservable<TActivationEventArgs, TPassivationEventArgs>
      where TActivationEventArgs : EventArgs
      where TPassivationEventArgs : EventArgs
   {
      /// <summary>
      /// This event will only be triggered after successful activation.
      /// </summary>
      event EventHandler<TActivationEventArgs> AfterActivation;

      /// <summary>
      /// This event will always be triggered after passivation.
      /// </summary>
      event EventHandler<TPassivationEventArgs> AfterPassivation;
   }

   /// <summary>
   /// This is helper class to perform something only after something is activated for a first time. After the <see cref="ActivationObservable{T,U}.AfterActivation"/> event is triggered, this class takes care of deregeristering the handler from the event.
   /// </summary>
   /// <typeparam name="TActivationEventArgs">The type of activation event arguments.</typeparam>
   /// <typeparam name="TPassivationEventArgs">The type of passivation event arguments.</typeparam>
   public sealed class AfterActivationFirstTimeOnlyHelper<TActivationEventArgs, TPassivationEventArgs>
      where TActivationEventArgs : EventArgs
      where TPassivationEventArgs : EventArgs
   {
      private readonly EventHandler<TActivationEventArgs> _event;
      private readonly ActivationObservable<TActivationEventArgs, TPassivationEventArgs> _observable;

      /// <summary>
      /// Creates new instance of <see cref="AfterActivationFirstTimeOnlyHelper{T,U}"/>.
      /// </summary>
      /// <param name="observable">The observable to which add event handler to.</param>
      /// <param name="evt">The event handler to add.</param>
      public AfterActivationFirstTimeOnlyHelper( ActivationObservable<TActivationEventArgs, TPassivationEventArgs> observable, EventHandler<TActivationEventArgs> evt )
      {
         ArgumentValidator.ValidateNotNull( "Observable", observable );
         ArgumentValidator.ValidateNotNull( "Event", evt );

         this._observable = observable;
         this._event = evt;
         observable.AfterActivation += new EventHandler<TActivationEventArgs>( this.AfterActivation );
      }

      private void AfterActivation( Object sender, TActivationEventArgs e )
      {
         try
         {
            this._event( sender, e );
         }
         finally
         {
            this._observable.AfterActivation -= new EventHandler<TActivationEventArgs>( this.AfterActivation );
         }
      }

   }
}
