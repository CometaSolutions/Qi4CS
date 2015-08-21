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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Concern
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class ConcernTest : AbstractSingletonInstanceTest
   {

      private const String MIXIN_A = "MixinA";
      private const String MIXIN_B = "MixinB";
      private const String CONCERN_A = "ConcernA";
      private const String CONCERN_B_1 = "ConcernB1";
      private const String CONCERN_B_2 = "ConcernB2";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) ).Done()
            .WithConcerns( typeof( AbstractTestConcern ), typeof( TestConcern ) );
      }

      [Test]
      public void TestConcerns()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.StructureServices.NewCompositeBuilder<TestComposite>( CompositeModelType.PLAIN ).Instantiate();
            Assert.AreEqual( CONCERN_A, composite.A(), "Return value must be modified by concern." );
            Assert.AreEqual( CONCERN_B_2, composite.B(), "Return value must be modified by concern." );
         } );
      }

      public interface TestComposite
      {
         String A();
         String B();
      }

      public class TestCompositeMixin : TestComposite
      {

         #region TestComposite Members

         public virtual String A()
         {
            return MIXIN_A;
         }

         public virtual String B()
         {
            return MIXIN_B;
         }

         #endregion
      }

      public class TestConcern : ConcernOf<TestComposite>, TestComposite
      {

         #region TestComposite Members

         public virtual String A()
         {
            String res = this.next.A();
            Assert.AreEqual( MIXIN_A, res, "The return value must be from mixin" );
            return CONCERN_A;
         }

         public virtual String B()
         {
            String res = this.next.B();
            Assert.AreEqual( MIXIN_B, res, "The return value must be from mixin" );
            return CONCERN_B_1;
         }

         #endregion
      }

      public abstract class AbstractTestConcern : ConcernOf<TestComposite>, TestComposite
      {

         #region TestComposite Members

         public virtual String B()
         {
            String res = this.next.B();
            Assert.AreEqual( CONCERN_B_1, res, "The return value must be from previous concern" );
            return CONCERN_B_2;
         }

         public abstract String A();

         #endregion
      }
   }
}
