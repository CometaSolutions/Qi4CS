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
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.SideEffect
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class SideEffectTest : AbstractSingletonInstanceTest
   {
      public const String RETURN_VALUE = "ReturnValue";

      private static Boolean _sideEffectCalled = false;
      private static Boolean _sideEffectCalled2 = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) ).Done()
            .WithSideEffects( typeof( TestSideEffect ), typeof( TestSideEffect2 ) );
      }

      [Test]
      public void ReturnValueVisibleToSideEffectTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            _sideEffectCalled = false;
            _sideEffectCalled2 = false;
            var composite = this.NewPlainComposite<TestComposite>();
            composite.ReturnValue();
            Assert.IsTrue( _sideEffectCalled, "The first side-effect must've been called." );
            Assert.IsTrue( _sideEffectCalled2, "The second side-effect must've been called." );
         } );
      }

      [Test]
      public void ThrowExceptionVisibleToSideEffectTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            _sideEffectCalled = false;
            _sideEffectCalled2 = false;
            var composite = this.NewPlainComposite<TestComposite>();
            Assert.Throws<MyException>( () => composite.ThrowException(), "The composite's method must throw an exception." );
            Assert.IsTrue( _sideEffectCalled, "The first side-effect must've been called." );
            Assert.IsTrue( _sideEffectCalled2, "The second side-effect must've been called." );
         } );
      }

      [Test]
      public void VoidMethodWorksTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            _sideEffectCalled = false;
            _sideEffectCalled2 = false;
            var composite = this.NewPlainComposite<TestComposite>();
            composite.NoReturn();
            Assert.IsTrue( _sideEffectCalled, "The first side-effect must've been called." );
            Assert.IsTrue( _sideEffectCalled2, "The second side-effect must've been called." );
         } );
      }

      [Test]
      public void ViolatingConstraintTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            _sideEffectCalled = false;
            _sideEffectCalled2 = false;
            var composite = this.NewPlainComposite<TestComposite>();
            Assert.Throws<ConstraintViolationException>( () => composite.ViolatingConstraint( null ), "The composite method must throw constraint violation exception" );
            Assert.IsFalse( _sideEffectCalled, "The first side-effect must not have been called." );
            Assert.IsFalse( _sideEffectCalled2, "The second side-effect must not have been called." );
         } );
      }

      [Test]
      public void SideEffectThrowsExceptionTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            _sideEffectCalled = false;
            _sideEffectCalled2 = false;
            var composite = this.NewPlainComposite<TestComposite>();
            composite.SideEffectThrowsExceptionMustBeIgnored();
            Assert.IsFalse( _sideEffectCalled, "The first side-effect must not have been called." );
            Assert.IsTrue( _sideEffectCalled2, "The second side-effect must've been called." );
         } );
      }

      public interface TestComposite
      {
         String ReturnValue();
         String ThrowException();
         void NoReturn();
         void ViolatingConstraint( String param );
         void SideEffectThrowsExceptionMustBeIgnored();
      }

      public class TestCompositeMixin : TestComposite
      {

         #region TestComposite Members

         public virtual String ReturnValue()
         {
            return RETURN_VALUE;
         }

         public virtual String ThrowException()
         {
            throw new MyException();
         }

         public virtual void NoReturn()
         {

         }

         public virtual void ViolatingConstraint( String param )
         {

         }
         public virtual void SideEffectThrowsExceptionMustBeIgnored()
         {

         }

         #endregion
      }

      public abstract class TestSideEffect : SideEffectOf<TestComposite>, TestComposite
      {

         #region TestComposite Members

         public virtual String ReturnValue()
         {
            _sideEffectCalled = true;
            Assert.AreEqual( RETURN_VALUE, this.result.ReturnValue(), "The return value must be expected" );
            return null;
         }

         public virtual String ThrowException()
         {
            _sideEffectCalled = true;
            String result = null;
            Assert.Throws<MyException>( new TestDelegate( () =>
               {
                  this.result.ThrowException();
               } ), "The exception must be thrown by side effect invocator." );
            return result;
         }

         public virtual void NoReturn()
         {
            _sideEffectCalled = true;
            this.result.NoReturn();
         }

         public virtual void ViolatingConstraint( String param )
         {
            _sideEffectCalled = true;
         }

         public abstract void SideEffectThrowsExceptionMustBeIgnored();

         #endregion
      }

      public class TestSideEffect2 : SideEffectOf<TestComposite>, TestComposite
      {

         #region TestComposite Members

         public virtual String ReturnValue()
         {
            _sideEffectCalled2 = true;
            return this.result.ReturnValue();
         }

         public virtual String ThrowException()
         {
            _sideEffectCalled2 = true;
            return this.result.ThrowException();
         }

         public virtual void NoReturn()
         {
            _sideEffectCalled2 = true;
         }

         public virtual void ViolatingConstraint( String param )
         {
            _sideEffectCalled2 = true;
         }

         public virtual void SideEffectThrowsExceptionMustBeIgnored()
         {
            _sideEffectCalled2 = true;
            throw new MyException2();
         }

         #endregion
      }

      public class MyException : Exception
      { }

      public class MyException2 : Exception
      { }



   }
}
