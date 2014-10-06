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
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Common
{
   [Serializable]
   public class InvokingCompositeMethodWithinMixinTest : AbstractSingletonInstanceTest
   {
      private static Boolean _ok = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin1 ), typeof( TestCompositeMixin2 ) );
      }

      [Test]
      public void InvokeCompositeMethodWithinMixinTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            _ok = false;
            this.NewPlainComposite<TestComposite>().B();
            Assert.IsTrue( _ok, "The method must have been called." );
         } );
      }

      public interface TestComposite
      {
         void A();
         void B();
      }

      public abstract class TestCompositeMixin1 : TestComposite
      {

         #region TestComposite Members

         public abstract void A();

         public virtual void B()
         {
            this.A();
         }

         #endregion
      }

      public abstract class TestCompositeMixin2 : TestComposite
      {

         #region TestComposite Members

         public virtual void A()
         {
            _ok = true;
         }

         public abstract void B();

         #endregion
      }
   }
}
