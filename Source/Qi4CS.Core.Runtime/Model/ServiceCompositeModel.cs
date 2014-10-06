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
using CollectionsWithRoles.API;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class ServiceCompositeModelMutable : CompositeModelMutable
   {
      private readonly ServiceCompositeModel _immutable;
      private readonly ServiceCompositeModelState _state;

      public ServiceCompositeModelMutable( ServiceCompositeModelState state, ServiceCompositeModel immutable )
         : base( state, immutable )
      {
         this._state = state;
         this._immutable = immutable;
      }

      public String ServiceID
      {
         get
         {
            return this._state.ServiceID;
         }
         set
         {
            this._state.ServiceID = value;
         }
      }

      public Boolean ActivateWithApplication
      {
         get
         {
            return this._state.ActivateWithApplication;
         }
         set
         {
            this._state.ActivateWithApplication = value;
         }
      }

      public new ServiceCompositeModel IQ
      {
         get
         {
            return this._immutable;
         }
      }
   }

   public class ServiceCompositeModelImmutable : CompositeModelImmutable, ServiceCompositeModel
   {

      private readonly ServiceCompositeModelState _state;

      public ServiceCompositeModelImmutable( ServiceCompositeModelState state )
         : base( state )
      {
         this._state = state;
      }

      #region ServiceCompositeModel Members

      public String ServiceID
      {
         get
         {
            return this._state.ServiceID;
         }
      }

      public Boolean ActivateWithApplication
      {
         get
         {
            return this._state.ActivateWithApplication;
         }
      }

      public event EventHandler<ServiceActivationArgs> AfterActivation;

      public event EventHandler<ServiceActivationArgs> AfterPassivation;

      #endregion

      internal void InvokeAfterActivation( CompositeInstance instance )
      {
         this.AfterActivation.InvokeEventIfNotNull( evt => evt( this, new ServiceActivationArgs( instance ) ) );
      }

      internal void InvokeAfterPassivation( CompositeInstance instance )
      {
         this.AfterPassivation.InvokeEventIfNotNull( evt => evt( this, new ServiceActivationArgs( instance ) ) );
      }
   }

   public class ServiceCompositeModelState : CompositeModelState
   {
      private String _serviceID;
      private Boolean _activateWithApplication;

      public ServiceCompositeModelState( CollectionsFactory factory )
         : base( factory )
      {

      }

      public String ServiceID
      {
         get
         {
            return this._serviceID;
         }
         set
         {
            this._serviceID = value;
         }
      }

      public Boolean ActivateWithApplication
      {
         get
         {
            return this._activateWithApplication;
         }
         set
         {
            this._activateWithApplication = value;
         }
      }
   }
}
