/*
 * Copyright 2011 Stanislav Muhametsin. All rights Reserved.
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
using System.Reflection;
using System.Threading;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

#if SILVERLIGHT
using System.Linq;
#endif

namespace Qi4CS.Core.Runtime.Instance
{
#if SILVERLIGHT
   public class ThreadLocal<T>
   {
      // TODO cleanup e.g. on every 100th access time...

      private class ValueHolder
      {
         internal T _value;
         internal ValueHolder( T value )
         {
            this._value = value;
         }
      }

      // kvp because struct
      private KeyValuePair<WeakReference, ValueHolder>[] _values;
      private readonly Func<T> _factory;

      public ThreadLocal( Func<T> factory )
      {
         CommonUtils.ArgumentValidator.ValidateNotNull( "Threadlocal value factory", factory );
         this._factory = factory;
      }

      private ValueHolder GetValueHolder()
      {
         var curThread = Thread.CurrentThread;
         var result = this._values == null ?
            null :
            this._values
               .Where( tuple => Object.ReferenceEquals( tuple.Key.Target, curThread ) )
               .Select( tuple => tuple.Value )
               .FirstOrDefault();
         if ( result == null )
         {
            var current = this._values;
            var curThreadW = new WeakReference( curThread );
            result = new ValueHolder( this._factory() );
            KeyValuePair<WeakReference, ValueHolder>[] oldCurrent;
            IList<KeyValuePair<WeakReference, ValueHolder>> newValue;

            do
            {
               oldCurrent = current;
               newValue = current == null ?
                  new List<KeyValuePair<WeakReference, ValueHolder>>() :
                  current.Where( tuple => tuple.Key.IsAlive ).ToList();
               newValue.Add( new KeyValuePair<WeakReference, ValueHolder>( curThreadW, result ) );
               current = Interlocked.CompareExchange( ref this._values, newValue.ToArray(), oldCurrent );
            } while ( current != oldCurrent );
         }
         return result;
      }

      public T Value
      {
         get
         {
            return this.GetValueHolder()._value;
         }
         set
         {
            this.GetValueHolder()._value = value;
         }
      }
   }

#endif
   public class ServiceCompositeInstanceImpl : CompositeInstanceImpl, ServiceCompositeInstance
   {

      private static readonly QualifiedName IDENTITY_QNAME = QualifiedName.FromMemberInfo( typeof( Identity ).LoadPropertyOrThrow( "Identity" ) );
      private static readonly ThreadLocal<Object[]> PUBLIC_CTOR_HOLDER = new ThreadLocal<Object[]>( () => null );
      private enum ActivatingAllowed { ALLOWED, NOT_ALLOWED }

      // State changes:
      // PASSIVE -> DURING_ACTIVATION: either when application is activated or any service method called
      // DURING_ACTIVATION -> ACTIVE: activation phase completes successfully
      // DURING_ACTIVATION -> PASSIVE: error in activation phase
      // DURING_ACTIVATION -> DURING_PASSIVATION: application explicitly passivated from within activation phase
      // ACTIVE -> DURING_PASSIVATION: when application is passivated
      // ( DURING_PASSIVATION -> ACTIVE: only possible if application would allow to activate during passivation ( it does not ) )
      // DURING_PASSIVATION -> PASSIVE: otherwise (passivation phase completes with or without errors)
      private Int32 _activationState;

      private InProgressTracker _activationInProgress;
      private readonly Action _activationAction;

      private InProgressTracker _passivationInProgress;
      private readonly Action _passivationAction;

      private readonly String _serviceID;

      private Int32 _activatingAllowed;
      private Int32 _needToSetIdentity;

      internal ServiceCompositeInstanceImpl( CompositeInstanceStructureOwner structureOwner, CompositeModel model, IEnumerable<Type> publicCompositeType, UsesContainerQuery usesContainer, String serviceID )
         : base( structureOwner, model, publicCompositeType, usesContainer, PUBLIC_CTOR_HOLDER )
      {
         var publicCtors = PUBLIC_CTOR_HOLDER.Value;

         this._activationState = (Int32) ActivationState.PASSIVE;

         this._activationInProgress = null;
         this._activationAction = (Action) publicCtors[COMPOSITE_CTOR_FIRST_ADDITIONAL_PARAM_IDX + COMPOSITE_CTOR_ADDITIONAL_PARAMS_COUNT];

         this._passivationInProgress = null;
         this._passivationAction = (Action) publicCtors[COMPOSITE_CTOR_FIRST_ADDITIONAL_PARAM_IDX + COMPOSITE_CTOR_ADDITIONAL_PARAMS_COUNT + 1];

         this._serviceID = serviceID;

         this._activatingAllowed = (Int32) ActivatingAllowed.ALLOWED;
         this._needToSetIdentity = Convert.ToInt32( true );
         PUBLIC_CTOR_HOLDER.Value = null;

      }

      public void ActivateIfNeeded( MethodInfo compositeMethod, AbstractFragmentMethodModel nextFragment )
      {
         if ( (Int32) ActivatingAllowed.ALLOWED == this._activatingAllowed )
         {
            ApplicationSkeleton.PerformActivation( ref this._activationState, ref this._activationInProgress, this._passivationInProgress, ApplicationSkeleton.WAIT_TIME, false, () =>
            {
               CompositePropertySPI idProp = (CompositePropertySPI) this.State.Properties[IDENTITY_QNAME];
               if ( Convert.ToBoolean( this._needToSetIdentity ) )
               {
                  idProp.PropertyValueAsObject = this._serviceID;
                  Interlocked.Exchange( ref this._needToSetIdentity, Convert.ToInt32( false ) );
               }

               // Disable prototype
               this.DisablePrototype( compositeMethod, nextFragment );

               if ( this._activationAction != null )
               {
                  if ( compositeMethod != null )
                  {
                     this.InvocationInfo = new InvocationInfoImpl( compositeMethod, nextFragment );
                  }
                  try
                  {
                     this._activationAction();
                  }
                  finally
                  {
                     if ( compositeMethod != null )
                     {
                        this.InvocationInfo = null;
                     }
                  }
               }
               ( (ServiceCompositeModelImmutable) this.ModelInfo.Model ).InvokeAfterActivation( this );
            } );
         }
      }

      #region ServiceCompositeInstance members

      public Boolean Active
      {
         get
         {
            return this._activationState == (Int32) ActivationState.ACTIVE;
         }
      }

      public String ServiceID
      {
         get
         {
            return this._serviceID;
         }
      }

      #endregion


      internal void RunActivationActionIfNeeded()
      {
         Interlocked.Exchange( ref this._activatingAllowed, (Int32) ActivatingAllowed.ALLOWED );
         if ( ( (ServiceCompositeModel) this.ModelInfo.Model ).ActivateWithApplication )
         {
            this.ActivateIfNeeded( null, null );
         }
      }

      internal void DisableLazyActivation()
      {
         Interlocked.Exchange( ref this._activatingAllowed, (Int32) ActivatingAllowed.NOT_ALLOWED );
      }

      internal void RunPassivationActionIfNeeded()
      {
         ApplicationSkeleton.PerformPassivation( ref this._activationState, ref this._passivationInProgress, this._activationInProgress, ApplicationSkeleton.WAIT_TIME, () =>
         {
            try
            {
               if ( this._passivationAction != null )
               {
                  this._passivationAction();
               }
            }
            finally
            {
               ( (ServiceCompositeModelImmutable) this.ModelInfo.Model ).InvokeAfterPassivation( this );
            }
         } );
      }

      //protected override CompositeInstanceImpl CreateNewCopy( IEnumerable<Type> publicCompositeTypes, UsesContainerQuery uses )
      //{
      //   throw new NotSupportedException( "Service instances may not be copied." );
      //}
   }
}
