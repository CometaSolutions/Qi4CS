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
using System.Threading;
using NUnit.Framework;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Tests.Core.Instance.Event
{
   [Serializable]
   public abstract class AbstractEventTest : AbstractSingletonInstanceTest
   {
      private static Boolean _genericEventInvoked = false;
      private static Boolean _normalEventInvoked = false;
      private static Boolean _typicalEventInvoked = false;
      private static Boolean _eventWithReturnTypeThrowInvoked = false;
      private static Boolean _eventWithReturnTypeReturnInvoked = false;
      private static Boolean _eventWithoutReturnTypeThrowInvoked = false;
      private static Boolean _eventWithoutReturnTypeReturnInvoked = false;

      private const String EVENT_STRING_PARAM = "NormalEventParam";
      private const String EVENT_STRING_PARAM_SUFFIX = "AfterInvokation";

      [Test]
      public void TestCompositeEvents()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite<String>>();

            var handler = new EventHandlerClass();
            composite.GenericEvent += new Func<string, string>( handler.composite_GenericEvent );
            composite.NormalEvent += new Action<string>( handler.composite_NormalEvent );
            composite.TypicalEvent += new EventHandler<EventArgs>( handler.composite_TypicalEvent );

            _genericEventInvoked = false;
            String result = composite.FireGenericEvent( EVENT_STRING_PARAM );
            Assert.IsTrue( _genericEventInvoked, "Generic event must've been invoked." );
            Assert.AreEqual( EVENT_STRING_PARAM + EVENT_STRING_PARAM_SUFFIX, result );

            _normalEventInvoked = false;
            composite.FireNormalEvent( EVENT_STRING_PARAM );
            Assert.IsTrue( _normalEventInvoked, "Normal event must've been invoked." );

            _typicalEventInvoked = false;
            composite.FireTypicalEvent( null );
            Assert.IsTrue( _typicalEventInvoked, "Typical event must've been invoked." );
         } );
      }

      [Test]
      public void TestEmptyCompositeEvents()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite<String>>();

            _genericEventInvoked = false;
            Assert.Throws<InvalidOperationException>( () => composite.FireGenericEvent( EVENT_STRING_PARAM ) );
            Assert.IsFalse( _genericEventInvoked, "Generic event must not have been invoked." );

            _normalEventInvoked = false;
            composite.FireNormalEvent( EVENT_STRING_PARAM );
            Assert.IsFalse( _normalEventInvoked, "Normal event must not have been invoked." );

            _typicalEventInvoked = false;
            composite.FireTypicalEvent( null );
            Assert.IsFalse( _typicalEventInvoked, "Typical event must not have been invoked." );
         } );
      }

      public abstract void TestInvocationStyles();

      protected void TestDirectInvokeEvent( Boolean testWeak )
      {
         TestComposite<String> composite = this.NewPlainComposite<TestComposite<String>>();
         EventHandlerClass handler = new EventHandlerClass();

         _eventWithoutReturnTypeReturnInvoked = false;
         _eventWithoutReturnTypeThrowInvoked = false;
         composite.DirectInvokeEvent += new Action<String>( handler.EventWithoutReturnTypeReturn );
         composite.DirectInvokeEvent += new Action<String>( handler.EventWithoutReturnTypeThrow );
         Assert.Throws<Exception>( () => composite.FireDirectInvokeEvent( EVENT_STRING_PARAM ) );
         Assert.IsTrue( _eventWithoutReturnTypeReturnInvoked );
         Assert.IsTrue( _eventWithoutReturnTypeThrowInvoked );

         if ( testWeak )
         {
            handler = null;
            _eventWithoutReturnTypeReturnInvoked = false;
            _eventWithoutReturnTypeThrowInvoked = false;
            PerformGC();
            composite.FireDirectInvokeEvent( EVENT_STRING_PARAM );
            Assert.IsFalse( _eventWithoutReturnTypeReturnInvoked );
            Assert.IsFalse( _eventWithoutReturnTypeThrowInvoked );
         }
      }

      protected void TestDirectInvokeWithReturnTypeEvent( Boolean testWeak )
      {
         TestComposite<String> composite = this.NewPlainComposite<TestComposite<String>>();
         EventHandlerClass handler = new EventHandlerClass();

         _eventWithReturnTypeReturnInvoked = false;
         _eventWithReturnTypeThrowInvoked = false;
         composite.DirectInvokeEventWithReturnType += new Func<String>( handler.EventWithReturnTypeReturn );
         composite.DirectInvokeEventWithReturnType += new Func<String>( handler.EventWithReturnTypeThrow );
         Assert.Throws<Exception>( () => Assert.AreEqual( EVENT_STRING_PARAM, composite.FireDirectInvokeEventWithReturnType() ) );
         Assert.IsTrue( _eventWithReturnTypeReturnInvoked );
         Assert.IsTrue( _eventWithReturnTypeThrowInvoked );

         if ( testWeak )
         {
            handler = null;
            _eventWithReturnTypeReturnInvoked = false;
            _eventWithReturnTypeThrowInvoked = false;
            PerformGC();
            Assert.Throws<InvalidOperationException>( () => composite.FireDirectInvokeEventWithReturnType() );
            Assert.IsFalse( _eventWithReturnTypeReturnInvoked );
            Assert.IsFalse( _eventWithReturnTypeThrowInvoked );
         }
      }

      protected void TestInvokeAllRethrowCustomWithoutReturnType( Boolean testWeak )
      {
         TestComposite<String> composite = this.NewPlainComposite<TestComposite<String>>();
         EventHandlerClass handler = new EventHandlerClass();

         _eventWithoutReturnTypeReturnInvoked = false;
         _eventWithoutReturnTypeThrowInvoked = false;
         composite.InvokeAllEventRethrowNoReturnType += new Action<String>( handler.EventWithoutReturnTypeThrow );
         composite.InvokeAllEventRethrowNoReturnType += new Action<String>( handler.EventWithoutReturnTypeReturn );
         MyException exc = Assert.Throws<MyException>( () => composite.FireInvokeAllEventRethrowNoReturnTypeEvent( EVENT_STRING_PARAM ) );
         Assert.AreEqual( 1, exc.Exceptions.Length );
         Assert.AreEqual( typeof( Exception ), exc.Exceptions[0].GetType() );
         Assert.IsTrue( _eventWithoutReturnTypeReturnInvoked );
         Assert.IsTrue( _eventWithoutReturnTypeThrowInvoked );

         if ( testWeak )
         {
            handler = null;
            _eventWithoutReturnTypeReturnInvoked = false;
            _eventWithoutReturnTypeThrowInvoked = false;
            PerformGC();
            composite.FireInvokeAllEventRethrowNoReturnTypeEvent( EVENT_STRING_PARAM );
            Assert.IsFalse( _eventWithoutReturnTypeReturnInvoked );
            Assert.IsFalse( _eventWithoutReturnTypeThrowInvoked );
         }
      }

      protected void TestInvokeAllRethrowCustomWithReturnType( Boolean testWeak )
      {
         TestComposite<String> composite = this.NewPlainComposite<TestComposite<String>>();
         EventHandlerClass handler = new EventHandlerClass();

         _eventWithReturnTypeReturnInvoked = false;
         _eventWithReturnTypeThrowInvoked = false;
         composite.InvokeAllEventRethrowWithReturnType += new Func<String>( handler.EventWithReturnTypeThrow );
         composite.InvokeAllEventRethrowWithReturnType += new Func<String>( handler.EventWithReturnTypeReturn );
         MyException exc = Assert.Throws<MyException>( () => Assert.AreEqual( EVENT_STRING_PARAM, composite.FireInvokeAllEventRethrowWithReturnTypeEvent() ) );
         Assert.AreEqual( 1, exc.Exceptions.Length );
         Assert.AreEqual( typeof( Exception ), exc.Exceptions[0].GetType() );
         Assert.IsTrue( _eventWithReturnTypeReturnInvoked );
         Assert.IsTrue( _eventWithReturnTypeThrowInvoked );

         if ( testWeak )
         {
            handler = null;
            _eventWithReturnTypeReturnInvoked = false;
            _eventWithReturnTypeThrowInvoked = false;
            PerformGC();
            Assert.Throws<InvalidOperationException>( () => composite.FireInvokeAllEventRethrowWithReturnTypeEvent() );
            Assert.IsFalse( _eventWithReturnTypeReturnInvoked );
            Assert.IsFalse( _eventWithReturnTypeThrowInvoked );
         }
      }

      protected void TestInvokeAllRethrowDefaultWithoutReturnType( Boolean testWeak )
      {
         TestComposite<String> composite = this.NewPlainComposite<TestComposite<String>>();
         EventHandlerClass handler = new EventHandlerClass();

         // "NoThrow" is remnant of old API, it really supposed to throw AggregateException

         _eventWithoutReturnTypeReturnInvoked = false;
         _eventWithoutReturnTypeThrowInvoked = false;
         composite.InvokeAllEventNoThrowNoReturnType += new Action<String>( handler.EventWithoutReturnTypeThrow );
         composite.InvokeAllEventNoThrowNoReturnType += new Action<String>( handler.EventWithoutReturnTypeReturn );
         AggregateException aExc = Assert.Throws<AggregateException>( () => composite.FireInvokeAllEventNoThrowNoReturnTypeEvent( EVENT_STRING_PARAM ) );
         Assert.AreEqual( 1, aExc.InnerExceptions.Count );
         Assert.AreEqual( typeof( Exception ), aExc.InnerExceptions[0].GetType() );
         Assert.IsTrue( _eventWithoutReturnTypeReturnInvoked );
         Assert.IsTrue( _eventWithoutReturnTypeThrowInvoked );

         if ( testWeak )
         {
            handler = null;
            _eventWithoutReturnTypeReturnInvoked = false;
            _eventWithoutReturnTypeThrowInvoked = false;
            PerformGC();
            composite.FireInvokeAllEventNoThrowNoReturnTypeEvent( EVENT_STRING_PARAM );
            Assert.IsFalse( _eventWithoutReturnTypeReturnInvoked );
            Assert.IsFalse( _eventWithoutReturnTypeThrowInvoked );
         }
      }

      protected void TestInvokeAllRethrowDefaultWithReturnType( Boolean testWeak )
      {
         TestComposite<String> composite = this.NewPlainComposite<TestComposite<String>>();
         EventHandlerClass handler = new EventHandlerClass();

         // "NoThrow" is remnant of old API, it really supposed to throw AggregateException

         _eventWithReturnTypeReturnInvoked = false;
         _eventWithReturnTypeThrowInvoked = false;
         composite.InvokeAllEventNoThrowWithReturnType += new Func<String>( handler.EventWithReturnTypeThrow );
         composite.InvokeAllEventNoThrowWithReturnType += new Func<String>( handler.EventWithReturnTypeReturn );
         AggregateException aExc = Assert.Throws<AggregateException>( () => Assert.AreEqual( EVENT_STRING_PARAM, composite.FireInvokeAllEventNoThrowWithReturnTypeEvent() ) );
         Assert.AreEqual( 1, aExc.InnerExceptions.Count );
         Assert.AreEqual( typeof( Exception ), aExc.InnerExceptions[0].GetType() );
         Assert.IsTrue( _eventWithReturnTypeReturnInvoked );
         Assert.IsTrue( _eventWithReturnTypeThrowInvoked );

         if ( testWeak )
         {
            handler = null;
            _eventWithReturnTypeReturnInvoked = false;
            _eventWithReturnTypeThrowInvoked = false;
            PerformGC();
            Assert.Throws<InvalidOperationException>( () => composite.FireInvokeAllEventNoThrowWithReturnTypeEvent() );
            Assert.IsFalse( _eventWithReturnTypeReturnInvoked );
            Assert.IsFalse( _eventWithReturnTypeThrowInvoked );
         }
      }

      private static void PerformGC()
      {
         // TODO in .NET 4 enhanced by .NET 4.5, this won't work. 
         System.GC.Collect();
         System.GC.WaitForPendingFinalizers();
         for ( Int32 x = 0; x < System.GC.MaxGeneration; ++x )
         {
            System.GC.Collect( x, GCCollectionMode.Forced );
         }
      }

      public class EventHandlerClass
      {

         internal void composite_TypicalEvent( object sender, EventArgs e )
         {
            Assert.IsInstanceOf<TestComposite<String>>( sender );
            Assert.IsNull( e );
            _typicalEventInvoked = true;
         }

         internal void composite_NormalEvent( string obj )
         {
            Assert.AreEqual( EVENT_STRING_PARAM, obj );
            _normalEventInvoked = true;
         }

         internal string composite_GenericEvent( string arg )
         {
            _genericEventInvoked = true;
            return arg + EVENT_STRING_PARAM_SUFFIX;
         }

         internal String EventWithReturnTypeThrow()
         {
            _eventWithReturnTypeThrowInvoked = true;
            throw new Exception();
         }

         internal String EventWithReturnTypeReturn()
         {
            _eventWithReturnTypeReturnInvoked = true;
            return EVENT_STRING_PARAM;
         }

         internal void EventWithoutReturnTypeThrow( String param )
         {
            _eventWithoutReturnTypeThrowInvoked = true;
            Assert.AreEqual( EVENT_STRING_PARAM, param );
            throw new Exception();
         }

         internal void EventWithoutReturnTypeReturn( String param )
         {
            _eventWithoutReturnTypeReturnInvoked = true;
            Assert.AreEqual( EVENT_STRING_PARAM, param );
         }
      }

      public interface TestComposite<T>
      {
         event Func<T, String> GenericEvent;
         event Action<String> NormalEvent;
         event EventHandler<EventArgs> TypicalEvent;

         [EventInvocationStyle( EventInvocation.INVOKE_DIRECTLY )]
         event Action<T> DirectInvokeEvent;

         [EventInvocationStyle( EventInvocation.INVOKE_DIRECTLY )]
         event Func<T> DirectInvokeEventWithReturnType;

         [EventInvocationStyle( typeof( MyException ) )]
         event Action<T> InvokeAllEventRethrowNoReturnType;

         [EventInvocationStyle( typeof( MyException ) )]
         event Func<T> InvokeAllEventRethrowWithReturnType;

         [EventInvocationStyle( EventInvocation.INVOKE_ALL )]
         event Action<T> InvokeAllEventNoThrowNoReturnType;

         [EventInvocationStyle( EventInvocation.INVOKE_ALL )]
         event Func<T> InvokeAllEventNoThrowWithReturnType;

         String FireGenericEvent( T param );
         void FireNormalEvent( String param );
         void FireTypicalEvent( [Optional] EventArgs args );
         void FireDirectInvokeEvent( T param );
         T FireDirectInvokeEventWithReturnType();
         void FireInvokeAllEventRethrowNoReturnTypeEvent( T param );
         T FireInvokeAllEventRethrowWithReturnTypeEvent();
         void FireInvokeAllEventNoThrowNoReturnTypeEvent( T param );
         T FireInvokeAllEventNoThrowWithReturnTypeEvent();
      }

      public abstract class TestCompositeMixin<T> : TestComposite<T>
      {
#pragma warning disable 649

         [This]
         private TestComposite<T> _self;

         [State( typeof( TestComposite<> ), "GenericEvent" )]
         private Func<T, String> _genericEventInvoker;

         [State( "NormalEvent" )]
         private Action<String> _normalEventInvoker;

         [State]
         private EventHandler<EventArgs> _typicalEventInvoker;

         [State( typeof( TestComposite<> ), "NormalEvent" )]
         private CompositeEvent _cEvent;

         [State( "DirectInvokeEvent" )]
         private Action<T> _directEventInvoker;

         [State( "DirectInvokeEventWithReturnType" )]
         private Func<T> _directEventWithReturnTypeInvoker;

         [State( "InvokeAllEventRethrowNoReturnType" )]
         private Action<T> _invokeAllEventRethrowNoReturnTypeInvoker;

         [State( "InvokeAllEventRethrowWithReturnType" )]
         private Func<T> _invokeAllEventRethrowWithReturnTypeInvoker;

         [State( "InvokeAllEventNoThrowNoReturnType" )]
         private Action<T> _invokeAllEventNoThrowNoReturnTypeInvoker;

         [State( "InvokeAllEventNoThrowWithReturnType" )]
         private Func<T> _invokeAllEventNoThrowWithReturntypeInvoker;

#pragma warning restore 649

         #region TestComposite<T> Members

         public abstract event Func<T, String> GenericEvent;

         public abstract event Action<String> NormalEvent;

         public abstract event EventHandler<EventArgs> TypicalEvent;

         public abstract event Action<T> DirectInvokeEvent;

         public abstract event Action<T> InvokeAllEventRethrowNoReturnType;

         public abstract event Func<T> InvokeAllEventRethrowWithReturnType;

         public abstract event Action<T> InvokeAllEventNoThrowNoReturnType;

         public abstract event Func<T> InvokeAllEventNoThrowWithReturnType;

         public abstract event Func<T> DirectInvokeEventWithReturnType;

         public virtual String FireGenericEvent( T param )
         {
            return this._genericEventInvoker( param );
         }

         public virtual void FireNormalEvent( string param )
         {
            Assert.IsNotNull( this._cEvent );
            this._normalEventInvoker( param );
         }

         public virtual void FireTypicalEvent( EventArgs args )
         {
            this._typicalEventInvoker( this._self, args );
         }

         public virtual void FireDirectInvokeEvent( T param )
         {
            this._directEventInvoker( param );
         }

         public virtual void FireInvokeAllEventRethrowNoReturnTypeEvent( T param )
         {
            this._invokeAllEventRethrowNoReturnTypeInvoker( param );
         }

         public virtual T FireInvokeAllEventRethrowWithReturnTypeEvent()
         {
            return this._invokeAllEventRethrowWithReturnTypeInvoker();
         }

         public virtual void FireInvokeAllEventNoThrowNoReturnTypeEvent( T param )
         {
            this._invokeAllEventNoThrowNoReturnTypeInvoker( param );
         }

         public virtual T FireInvokeAllEventNoThrowWithReturnTypeEvent()
         {
            return this._invokeAllEventNoThrowWithReturntypeInvoker();
         }

         public virtual T FireDirectInvokeEventWithReturnType()
         {
            return this._directEventWithReturnTypeInvoker();
         }

         #endregion
      }

      //      public abstract class TestCompositeMixinWithWeakEvents<T> : TestComposite<T>
      //      {
      //#pragma warning disable 649

      //         [This]
      //         private TestComposite<T> _self;

      //         [State( typeof( TestComposite<> ), "GenericEvent" )]
      //         private Func<T, String> _genericEventInvoker;

      //         [State( "NormalEvent" )]
      //         private Action<String> _normalEventInvoker;

      //         [State]
      //         private EventHandler<EventArgs> _typicalEventInvoker;

      //         [State( typeof( TestComposite<> ), "NormalEvent" )]
      //         private CompositeEvent _cEvent;

      //         [State( "DirectInvokeEvent" )]
      //         private Action<T> _directEventInvoker;

      //         [State( "DirectInvokeEventWithReturnType" )]
      //         private Func<T> _directEventWithReturnTypeInvoker;

      //         [State( "InvokeAllEventRethrowNoReturnType" )]
      //         private Action<T> _invokeAllEventRethrowNoReturnTypeInvoker;

      //         [State( "InvokeAllEventRethrowWithReturnType" )]
      //         private Func<T> _invokeAllEventRethrowWithReturnTypeInvoker;

      //         [State( "InvokeAllEventNoThrowNoReturnType" )]
      //         private Action<T> _invokeAllEventNoThrowNoReturnTypeInvoker;

      //         [State( "InvokeAllEventNoThrowWithReturnType" )]
      //         private Func<T> _invokeAllEventNoThrowWithReturntypeInvoker;

      //#pragma warning restore 649

      //         #region TestComposite<T> Members

      //         [EventStorageStyle( EventStorage.WEAK_REFS )]
      //         public abstract event Func<T, String> GenericEvent;

      //         [EventStorageStyle( EventStorage.WEAK_REFS )]
      //         public abstract event Action<String> NormalEvent;

      //         [EventStorageStyle( EventStorage.WEAK_REFS )]
      //         public abstract event EventHandler<EventArgs> TypicalEvent;

      //         [EventStorageStyle( EventStorage.WEAK_REFS )]
      //         public abstract event Action<T> DirectInvokeEvent;

      //         [EventStorageStyle( EventStorage.WEAK_REFS )]
      //         public abstract event Action<T> InvokeAllEventRethrowNoReturnType;

      //         [EventStorageStyle( EventStorage.WEAK_REFS )]
      //         public abstract event Func<T> InvokeAllEventRethrowWithReturnType;

      //         [EventStorageStyle( EventStorage.WEAK_REFS )]
      //         public abstract event Action<T> InvokeAllEventNoThrowNoReturnType;

      //         [EventStorageStyle( EventStorage.WEAK_REFS )]
      //         public abstract event Func<T> InvokeAllEventNoThrowWithReturnType;

      //         [EventStorageStyle( EventStorage.WEAK_REFS )]
      //         public abstract event Func<T> DirectInvokeEventWithReturnType;

      //         public virtual String FireGenericEvent( T param )
      //         {
      //            return this._genericEventInvoker( param );
      //         }

      //         public virtual void FireNormalEvent( string param )
      //         {
      //            Assert.IsNotNull( this._cEvent );
      //            this._normalEventInvoker( param );
      //         }

      //         public virtual void FireTypicalEvent( EventArgs args )
      //         {
      //            this._typicalEventInvoker( this._self, args );
      //         }

      //         public virtual void FireDirectInvokeEvent( T param )
      //         {
      //            this._directEventInvoker( param );
      //         }

      //         public virtual void FireInvokeAllEventRethrowNoReturnTypeEvent( T param )
      //         {
      //            this._invokeAllEventRethrowNoReturnTypeInvoker( param );
      //         }

      //         public virtual T FireInvokeAllEventRethrowWithReturnTypeEvent()
      //         {
      //            return this._invokeAllEventRethrowWithReturnTypeInvoker();
      //         }

      //         public virtual void FireInvokeAllEventNoThrowNoReturnTypeEvent( T param )
      //         {
      //            this._invokeAllEventNoThrowNoReturnTypeInvoker( param );
      //         }

      //         public virtual T FireInvokeAllEventNoThrowWithReturnTypeEvent()
      //         {
      //            return this._invokeAllEventNoThrowWithReturntypeInvoker();
      //         }

      //         public virtual T FireDirectInvokeEventWithReturnType()
      //         {
      //            return this._directEventWithReturnTypeInvoker();
      //         }

      //         #endregion
      //      }


      public class MyException : Exception
      {
         private Exception[] _exceptions;
         public MyException( IEnumerable<Exception> exceptions )
            : base( String.Join( ", ", (Object[]) exceptions.ToArray() ) )
         {
            this._exceptions = exceptions.ToArray();
         }

         public Exception[] Exceptions
         {
            get
            {
               return this._exceptions;
            }
         }
      }
   }
}
