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
using System.Linq;
using System.Threading;
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Instance
{
   public abstract class ApplicationSkeleton : ApplicationSPI//, IDisposable
   {

      public const Int32 WAIT_TIME = 5;

      private readonly String _name;
      private readonly String _mode;
      private readonly String _version;
      private readonly ApplicationModel<ApplicationSPI> _model;

      private readonly CollectionsFactory _collectionsFactory;
#if SILVERLIGHT
      private readonly IDictionary<Type, InstancePool<Object>> _constraintInstancePools;
#else
      private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, InstancePool<Object>> _constraintInstancePools;
#endif

      private readonly DictionaryQuery<CompositeModelType, CompositeModelTypeInstanceScopeSupport> _compositeModelTypeSupport;

      // State changes:
      // PASSIVE -> DURING_ACTIVATION: when application is activated
      // DURING_ACTIVATION -> ACTIVE: activation phase completes successfully
      // DURING_ACTIVATION -> PASSIVE: error in activation phase
      // DURING_ACTIVATION -> DURING_PASSIVATION: application explicitly passivated from within activation phase
      // ACTIVE -> DURING_PASSIVATION: when application is passivated
      // DURING_PASSIVATION -> PASSIVE: always
      private Int32 _activationState;
      private InProgressTracker _activationInProgress;
      private InProgressTracker _passivationInProgress;
      private CancellationTokenSource _cancelTokenSource;

      public ApplicationSkeleton(
         ApplicationModel<ApplicationSPI> applicationModel,
         IEnumerable<Tuple<CompositeModelType, CompositeModelTypeModelScopeSupport>> compositeModelTypeSupport,
         String name,
         String mode,
         String version
         )
      {
         this._model = applicationModel;
         this._collectionsFactory = applicationModel.CollectionsFactory;
         this._name = name;
         this._mode = mode;
         this._version = version;
         this._constraintInstancePools = new
#if SILVERLIGHT
 Dictionary<Type, InstancePool<Object>>();
#else
 System.Collections.Concurrent.ConcurrentDictionary<Type, InstancePool<Object>>();
#endif

         this._compositeModelTypeSupport = this._model.CollectionsFactory.NewDictionaryProxy<CompositeModelType, CompositeModelTypeInstanceScopeSupport>( compositeModelTypeSupport.ToDictionary( tuple => tuple.Item1, tuple => tuple.Item2.CreateInstanceScopeSupport() ) ).CQ;

         this._activationState = (Int32) ActivationState.PASSIVE;
         //this._disposedState = Convert.ToInt32( false );
         this._activationInProgress = null;
         this._passivationInProgress = null;
         this._cancelTokenSource = new CancellationTokenSource();
      }

      #region Application Members

      public String Name
      {
         get
         {
            return this._name;
         }
      }

      public String Mode
      {
         get
         {
            return this._mode;
         }
      }

      public String Version
      {
         get
         {
            return this._version;
         }
      }

      public void Activate()
      {
         var args = new ApplicationActivationArgs( this );
         var invoked = PerformActivation( ref this._activationState, ref this._activationInProgress, this._passivationInProgress, WAIT_TIME, true, () =>
         {
            if ( this._cancelTokenSource.IsCancellationRequested )
            {
               Interlocked.Exchange( ref this._cancelTokenSource, new CancellationTokenSource() );
            }

            foreach ( var support in this._compositeModelTypeSupport.Values )
            {
               support.ApplicationActivating( this );
            }
         } );
         if ( invoked )
         {
            this.AfterActivation.InvokeEventIfNotNull( evt => evt( this, args ) );
         }
      }

      public void Passivate()
      {
         var args = new ApplicationPassivationArgs( this );

         IList<Exception> occurredExceptions = null;
         var invoked = PerformPassivation( ref this._activationState, ref this._passivationInProgress, this._activationInProgress, WAIT_TIME, () =>
         {
            this._cancelTokenSource.Cancel();

            foreach ( var support in this._compositeModelTypeSupport.Values )
            {
               try
               {
                  support.ApplicationPassivating( this );
               }
               catch ( Exception exc )
               {
                  if ( occurredExceptions == null )
                  {
                     occurredExceptions = new List<Exception>();
                  }
                  occurredExceptions.Add( exc );
               }
            }
         } );

         if ( invoked )
         {
            // Invoke after -event.
            try
            {
               this.AfterPassivation.InvokeEventIfNotNull( evt => evt( this, args ) );
            }
            catch ( Exception exc )
            {
               if ( occurredExceptions == null )
               {
                  occurredExceptions = new List<Exception>();
               }
               occurredExceptions.Add( exc );
            }

            // Throw if any exception occurred during activation.
            if ( occurredExceptions != null )
            {
               throw new ApplicationPassivationException( occurredExceptions.ToArray() );
            };

         }
      }

      public event EventHandler<ApplicationActivationArgs> AfterActivation;
      public event EventHandler<ApplicationPassivationArgs> AfterPassivation;

      public Boolean Active
      {
         get
         {
            return this._activationState == (Int32) ActivationState.ACTIVE;
         }
      }

      public Boolean Passive
      {
         get
         {
            return this._activationState == (Int32) ActivationState.PASSIVE;
         }
      }

      public CancellationToken PassivationToken
      {
         get
         {
            return this._cancelTokenSource.Token;
         }
      }

      #endregion

      #region ApplicationSPI Members

      public InstancePool<Object> GetConstraintInstancePool( Type resolvedConstraintType )
      {
         return this._constraintInstancePools.
#if SILVERLIGHT
GetOrAdd_WithLock(
#else
GetOrAdd(
#endif
 resolvedConstraintType, ( rType ) => new InstancePool<Object>() );
      }

      public InjectionService InjectionService
      {
         get
         {
            return this._model.InjectionService;
         }
      }

      public virtual CompositeInstance GetCompositeInstance( Object compositeOrFragment )
      {
         ArgumentValidator.ValidateNotNull( "Composite or fragment", compositeOrFragment );
         CompositeInstance instance = null;
         this._compositeModelTypeSupport.Values.FirstOrDefault( support => support.TryGetInstanceFromCompositeOrFragment( compositeOrFragment, out instance ) );
         if ( instance == null )
         {
            throw new ArgumentException( "Given argument " + compositeOrFragment + " is not valid composite or fragment." );
         }
         return instance;
      }

      public CollectionsFactory CollectionsFactory
      {
         get
         {
            return this._collectionsFactory;
         }
      }

      public ApplicationModel<ApplicationSPI> ApplicationModel
      {
         get
         {
            return this._model;
         }
      }

      #endregion

      protected DictionaryQuery<CompositeModelType, CompositeModelTypeInstanceScopeSupport> ModelTypeSupport
      {
         get
         {
            return this._compositeModelTypeSupport;
         }
      }

      public static Int32 ThreadsafeStateTransition( ref Int32 transitionState, Int32 initialState, Int32 intermediate, Int32 final, Boolean rollbackStateOnFailure, ref InProgressTracker inProgressTracker, Int32 waitTime, Action transitionAction )
      {
         Int32 oldValue = Interlocked.CompareExchange( ref transitionState, intermediate, initialState );
         if ( initialState == oldValue )
         {
            Boolean finished = false;
            try
            {
               Interlocked.Exchange( ref inProgressTracker, new InProgressTracker() );

               transitionAction();

               finished = true;
            }
            finally
            {
               Interlocked.Exchange( ref inProgressTracker, null );
               Interlocked.Exchange( ref transitionState, finished || !rollbackStateOnFailure ? final : initialState );
            }
         }
         else if ( final != oldValue && !( new InProgressTracker() ).Equals( inProgressTracker ) )
         {
            // We are entering mid-transition from another thread
#if SILVERLIGHT
            using ( var evt = new ManualResetEvent( false ) )
#else
            using ( var evt = new ManualResetEventSlim( false ) )
#endif
            {
               while ( inProgressTracker != null )
               {
                  // Wait
#if SILVERLIGHT
                  evt.WaitOne( waitTime );
#else
                  evt.Wait( waitTime );
#endif
               }
            }
         }
         return oldValue;
      }

      public static Boolean PerformActivation( ref Int32 transitionState, ref InProgressTracker activationInProgress, InProgressTracker passivationInProgress, Int32 waitTime, Boolean throwIfActivatingWithinPassivation, Action activationAction )
      {
         var passivationInProgressBool = new InProgressTracker().Equals( passivationInProgress );
         if ( throwIfActivatingWithinPassivation && passivationInProgressBool )
         {
            // Trying to activate within passivation => not possible
            throw new InvalidOperationException( "Can not activate from within passivation." );
         }

         Boolean result;
         Int32 prevState;
         Boolean tryAgain;
#if SILVERLIGHT
         using ( var waitEvt = new ManualResetEvent( false ) )
#else
         using ( var waitEvt = new ManualResetEventSlim( false ) )
#endif
         {
            do
            {
               prevState = ThreadsafeStateTransition( ref transitionState, (Int32) ActivationState.PASSIVE, (Int32) ActivationState.DURING_ACTIVATION, (Int32) ActivationState.ACTIVE, true, ref activationInProgress, waitTime, activationAction );

               result = prevState == (Int32) ActivationState.PASSIVE;
               // Try again only if we are waiting for passivation to end in another thread
               tryAgain = !result && prevState == (Int32) ActivationState.DURING_PASSIVATION && !passivationInProgressBool;
               if ( tryAgain )
               {
#if SILVERLIGHT
                  waitEvt.WaitOne( waitTime );
#else
                  waitEvt.Wait( waitTime );
#endif
               }
            } while ( tryAgain );
         }

         return result;
      }

      public static Boolean PerformPassivation( ref Int32 transitionState, ref InProgressTracker passivationInProgress, InProgressTracker activationInProgress, Int32 waitTime, Action passivationAction )
      {
         Int32 initialState = (Int32) ( new InProgressTracker().Equals( activationInProgress ) ? ActivationState.DURING_ACTIVATION : ActivationState.ACTIVE );
         Int32 prevState;
         Boolean actionInvoked;
         Boolean tryAgain;
#if SILVERLIGHT
         using ( var waitEvt = new ManualResetEvent( false ) )
#else
         using ( var waitEvt = new ManualResetEventSlim( false ) )
#endif
         {
            do
            {
               prevState = ApplicationSkeleton.ThreadsafeStateTransition( ref transitionState, initialState, (Int32) ActivationState.DURING_PASSIVATION, (Int32) ActivationState.PASSIVE, false, ref passivationInProgress, waitTime, passivationAction );

               actionInvoked = prevState == initialState;
               tryAgain = !actionInvoked && prevState != (Int32) ActivationState.PASSIVE && prevState != (Int32) ActivationState.DURING_PASSIVATION;
               if ( tryAgain && initialState == (Int32) ActivationState.ACTIVE )
               {
                  // Wait if we are not inside activation action,
                  // and if transition state change failed because activation is in progress.
#if SILVERLIGHT
                  waitEvt.WaitOne( waitTime );
#else
                  waitEvt.Wait( waitTime );
#endif
               }
            } while ( tryAgain );
         }
         return actionInvoked;
      }

      //#region IDisposable Members

      //public void Dispose()
      //{
      //   this.Dispose( true );
      //   GC.SuppressFinalize( this );
      //}

      //#endregion

      //protected virtual void Dispose( Boolean disposing )
      //{
      //   this._activationDoneEvent.Dispose();
      //   this._passivationDoneEvent.Dispose();
      //   Interlocked.Exchange( ref this._disposedState, Convert.ToInt32( true ) );
      //}

      //~ApplicationSkeleton()
      //{
      //   this.Dispose( false );
      //}

      //private void ThrowIfDisposed()
      //{
      //   if ( Convert.ToBoolean( this._disposedState ) )
      //   {
      //      throw new ObjectDisposedException( "This application instance has been disposed." );
      //   }
      //}
   }

   public enum ActivationState { PASSIVE, DURING_ACTIVATION, ACTIVE, DURING_PASSIVATION };


   public sealed class InProgressTracker : IEquatable<InProgressTracker>
   {
      private readonly Int32 _threadID;

      public InProgressTracker()
      {
         this._threadID =
#if WINDOWS_PHONE_APP
         Environment.CurrentManagedThreadId
#else
 Thread.CurrentThread.ManagedThreadId
#endif
;
      }

      public Int32 ThreadID
      {
         get
         {
            return this._threadID;
         }
      }

      public Boolean Equals( InProgressTracker other )
      {
         return Object.ReferenceEquals( this, other ) || ( other != null && this._threadID == other._threadID );
      }

      public override Boolean Equals( object obj )
      {
         return this.Equals( obj as InProgressTracker );
      }

      public override Int32 GetHashCode()
      {
         return this._threadID.GetHashCode();
      }
   }
}
