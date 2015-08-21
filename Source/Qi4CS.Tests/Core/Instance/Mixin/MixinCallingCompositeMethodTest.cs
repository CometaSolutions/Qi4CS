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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Mixin
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class MixinCallingCompositeMethodTest : AbstractSingletonInstanceTest
   {
      private static Boolean _mixinAInvoked = false;
      private static Boolean _mixinBInvoked = false;
      private static Boolean _concernBInvoked = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) ).Done()
            .WithConcerns( typeof( TestCompositeConcern ) );
      }

      [Test]
      public void InvokeCompositeMethodFromWithinMixinTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            _mixinAInvoked = false;
            _mixinBInvoked = false;
            _concernBInvoked = false;
            this.NewPlainComposite<TestComposite>().A();
            Assert.IsTrue( _mixinAInvoked );
            Assert.IsTrue( _mixinBInvoked );
            Assert.IsTrue( _concernBInvoked );
         } );
      }

      public interface TestComposite
      {
         void A();
         void B();
      }

      public class TestCompositeMixin : TestComposite
      {

         #region TestComposite Members

         public virtual void A()
         {
            _mixinAInvoked = true;
            this.B();
         }

         public virtual void B()
         {
            _mixinBInvoked = true;
         }

         #endregion
      }

      public abstract class TestCompositeConcern : ConcernOf<TestComposite>, TestComposite
      {

         #region TestComposite Members

         public abstract void A();

         public virtual void B()
         {
            Assert.IsTrue( _mixinAInvoked );
            Assert.IsFalse( _mixinBInvoked );
            Assert.IsFalse( _concernBInvoked );
            _concernBInvoked = true;
            this.next.B();
         }

         #endregion
      }
   }
}
