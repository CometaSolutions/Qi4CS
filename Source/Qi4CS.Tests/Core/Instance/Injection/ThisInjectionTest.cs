/*
 * Copyright (c) 2007, Rickard Öberg.
 * See NOTICE file.
 * 
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

namespace Qi4CS.Tests.Core.Instance.Injection
{
   [Serializable]
   public class ThisInjectionTest : AbstractSingletonInstanceTest
   {
      private const String RESULT_1 = "Foo";
      private const Boolean RESULT_2 = true;
      private static Boolean _sideEffectInjected = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( Test ) )
            .WithMixins( typeof( TestMixin ), typeof( TestPrivateMixin ) ).Done()
            .WithConcerns( typeof( TestConcern ) ).Done()
            .WithSideEffects( typeof( TestSideEffect ) );
      }

      [Test]
      public void givenCompositeWithThisInjectionsWhenInstantiatedThenCompositeIsInjected()
      {
         this.PerformTestInAppDomain( () =>
         {
            var testComposite = this.NewPlainComposite<Test>();
            _sideEffectInjected = false;
            Assert.IsTrue( testComposite.isInjected() && _sideEffectInjected, "Injection worked" );
         } );
      }

      public interface Test
      {
         Boolean isInjected();

         String test();
      }

      public interface TestPrivate
      {
         Boolean testPrivate();
      }

      public class TestMixin : Test
      {
#pragma warning disable 649

         [This]
         private Test _test;

         [This]
         private TestPrivate _testPrivate;

#pragma warning restore 649

         #region Test Members

         public virtual Boolean isInjected()
         {
            return this._test != null && this._testPrivate.testPrivate() == RESULT_2;
         }

         public virtual String test()
         {
            return RESULT_1;
         }
         #endregion

      }

      public class TestPrivateMixin : TestPrivate
      {

         #region TestPrivate Members

         public virtual Boolean testPrivate()
         {
            return RESULT_2;
         }

         #endregion
      }

      public abstract class TestConcern : ConcernOf<Test>, Test
      {
#pragma warning disable 649

         [This]
         private Test _test;

         [This]
         private TestPrivate _testPrivate;

#pragma warning restore 649

         #region Test Members

         public virtual Boolean isInjected()
         {
            return this._test != null && this._test.test() == RESULT_1 &&
                   this._testPrivate.testPrivate() == RESULT_2 &&
                   this.next.isInjected();
         }

         public abstract String test();

         #endregion
      }

      public abstract class TestSideEffect : SideEffectOf<Test>, Test
      {
#pragma warning disable 649

         [This]
         private Test _test;

         [This]
         private TestPrivate _testPrivate;

#pragma warning restore 649

         #region Test Members

         public virtual Boolean isInjected()
         {
            _sideEffectInjected = this._test != null && this._testPrivate.testPrivate() == RESULT_2;

            return false;
         }

         public abstract String test();

         #endregion
      }
   }
}
