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
using NUnit.Framework;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.CodeGen
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class InternalFragmentTest : AbstractSingletonInstanceTest
   {
      private static Boolean _mixinTriggered = false;
      internal static Boolean _mixin2Triggered = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite()
            .OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ), typeof( TestCompositeMixin2 ) );
      }

      [Test]
      public void TestCompositeWithInternalNestedMixin()
      {
         this.PerformTestInAppDomain( () =>
         {
            _mixinTriggered = false;
            this.NewPlainComposite<TestComposite>().MyMethod();
            Assert.IsTrue( _mixinTriggered );
         } );
      }

      [Test]
      public void TestcompositeWithInternalMixin()
      {
         this.PerformTestInAppDomain( () =>
         {
            _mixin2Triggered = false;
            this.NewPlainComposite<TestComposite>().MyMethod2();
            Assert.IsTrue( _mixin2Triggered );
         } );
      }

      public interface TestComposite
      {
         void MyMethod();
         void MyMethod2();
      }

      internal abstract class TestCompositeMixin : TestComposite
      {

         #region TestComposite Members

         public virtual void MyMethod()
         {
            _mixinTriggered = true;
         }

         public abstract void MyMethod2();

         #endregion
      }
   }

   internal abstract class TestCompositeMixin2 : InternalFragmentTest.TestComposite
   {

      #region TestComposite Members

      public abstract void MyMethod();

      public virtual void MyMethod2()
      {
         InternalFragmentTest._mixin2Triggered = true;
      }

      #endregion
   }
}
