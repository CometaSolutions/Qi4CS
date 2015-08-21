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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Property
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class CompositePropertyTest : AbstractSingletonInstanceTest
   {
      private static Int32 _activatedCount;
      private static Int32 _passivatedCount;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite()
            .OfTypes( typeof( ActivatableComposite ) )
            .WithMixins( typeof( ActivatableCompositeMixin ) );
      }

      [Test]
      public void TestCompositePropertyThreadsafety()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<ActivatableComposite>();

            _activatedCount = 0;
            Parallel.Invoke( Enumerable.Repeat<Action>( () => composite.Activate(), 100 ).ToArray() );
            Assert.AreEqual( 1, _activatedCount, "The composite must've been activated exactly once." );

            _passivatedCount = 0;
            Parallel.Invoke( Enumerable.Repeat<Action>( () => composite.Passivate(), 100 ).ToArray() );
            Assert.AreEqual( 1, _passivatedCount, "The composite must've been passivated exactly once." );
         } );
      }

      public interface ActivatableComposite
      {
         Boolean IsActive { get; }
         Boolean IsPassive { get; }
         void Activate();
         void Passivate();
      }

      public enum ActivatableStates { PASSIVE, ACTIVATING, ACTIVE, PASSIVATING }

      public interface ActivatableState
      {
         ActivatableStates CurrentState { get; set; }
      }

      public class ActivatableCompositeMixin : ActivatableComposite
      {
#pragma warning disable 649
         [This]
         private ActivatableState _state;

         [State( "CurrentState" )]
         private CompositeProperty _property;

#pragma warning restore 649

         #region ActivatableComposite Members

         public virtual Boolean IsActive
         {
            get
            {
               return ActivatableStates.ACTIVE.Equals( this._state.CurrentState );
            }
         }

         public virtual Boolean IsPassive
         {
            get
            {
               return ActivatableStates.PASSIVE.Equals( this._state.CurrentState );
            }
         }

         public virtual void Activate()
         {
            if ( ActivatableStates.PASSIVE.Equals( this._property.CompareExchangeAsObject( ActivatableStates.PASSIVE, ActivatableStates.ACTIVATING ) ) )
            {
               // Interlocked is not really required, but it is used in case the generated code is not threadsafe as it should be
               try
               {
                  Interlocked.Increment( ref _activatedCount );
               }
               finally
               {
                  this._property.ExchangeAsObject( ActivatableStates.ACTIVE );
               }
            }
         }

         public virtual void Passivate()
         {
            if ( ActivatableStates.ACTIVE.Equals( this._property.CompareExchangeAsObject( ActivatableStates.ACTIVE, ActivatableStates.PASSIVATING ) ) )
            {
               // Interlocked is not really required, but it is used in case the generated code is not threadsafe as it should be
               try
               {
                  Interlocked.Increment( ref _passivatedCount );
               }
               finally
               {
                  this._property.ExchangeAsObject( ActivatableStates.PASSIVE );
               }
            }
         }

         #endregion

         [Prototype]
         public void InitState()
         {
            this._state.CurrentState = ActivatableStates.PASSIVE;
         }
      }
   }
}
