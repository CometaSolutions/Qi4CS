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
using NUnit.Framework;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Objects
{
   [Serializable]
   public class ObjectTest : AbstractSingletonInstanceTest
   {
      private static Boolean _mixinCalled;
      private static Boolean _concernCalled;
      private static Boolean _sideEffectCalled;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestObject ) );
         assembler.NewPlainComposite().OfTypes( typeof( TestObjectWithConcern ) ).WithConcerns( typeof( TestObjectConcern ) );
         assembler.NewPlainComposite().OfTypes( typeof( TestObjectWithSideEffect ) ).WithSideEffects( typeof( TestObjectSideEffect ) );
      }

      [Test]
      public void TestThatObjectTransientsCanBeCalled()
      {
         this.PerformTestInAppDomain( () =>
         {
            var obj = this.NewPlainComposite<TestObject>();
            _mixinCalled = false;
            obj.TestMethod();
            Assert.IsTrue( _mixinCalled, "Method must've been called." );
         } );
      }

      [Test]
      public void TestThatObjectTransientsWorkWithConcerns()
      {
         this.PerformTestInAppDomain( () =>
         {
            var obj = this.NewPlainComposite<TestObjectWithConcern>();
            _mixinCalled = false;
            _concernCalled = false;
            obj.TestMethod();
            Assert.IsTrue( _mixinCalled, "Mixin must've been called." );
            Assert.IsTrue( _concernCalled, "Concern must've been called." );
         } );
      }

      [Test]
      public void TestThatObjectTransientsWorkWithSideEffects()
      {
         this.PerformTestInAppDomain( () =>
         {
            var obj = this.NewPlainComposite<TestObjectWithSideEffect>();
            _mixinCalled = false;
            _sideEffectCalled = false;
            obj.TestMethod();
            Assert.IsTrue( _mixinCalled, "Mixin must've been called." );
            Assert.IsTrue( _sideEffectCalled, "Side effect must've been called." );
         } );
      }

      public class TestObject
      {
         public virtual void TestMethod()
         {
            _mixinCalled = true;
         }
      }

      public class TestObjectWithConcern
      {
         public virtual void TestMethod()
         {
            _mixinCalled = true;
         }
      }

      public class TestObjectWithSideEffect
      {
         public virtual void TestMethod()
         {
            _mixinCalled = true;
         }
      }

      public class TestObjectConcern : TestObjectWithConcern
      {
#pragma warning disable 649, 169

         [ConcernFor]
         private TestObjectWithConcern _next;

#pragma warning restore 649, 169

         public override void TestMethod()
         {
            _concernCalled = true;
            this._next.TestMethod();
         }
      }

      public class TestObjectSideEffect : TestObjectWithSideEffect
      {
#pragma warning disable 649, 169

         [SideEffectFor]
         private TestObjectWithSideEffect _result;

#pragma warning restore 649, 169

         public override void TestMethod()
         {
            this._result.TestMethod();
            _sideEffectCalled = true;
         }
      }
   }
}
