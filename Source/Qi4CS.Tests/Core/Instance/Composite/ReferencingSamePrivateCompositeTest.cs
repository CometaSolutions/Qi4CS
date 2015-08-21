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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class ReferencingSamePrivateCompositeTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixinA ), typeof( TestCompositeMixinB ) );
      }

      [Test]
      public void TestReferencingSamePrivateComposite()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();

            Assert.IsNull( composite.A() );
            Assert.IsNull( composite.B() );
         } );
      }

      public interface TestComposite
      {
         [return: Optional]
         String A();

         [return: Optional]
         Object B();
      }

      public interface TestCompositeState<PropType>
      {
         [Optional]
         PropType Property { get; set; }
      }

      public abstract class TestCompositeMixinA : TestComposite
      {
#pragma warning disable 649

         [This]
         private TestCompositeState<String> _state;

#pragma warning restore 649

         #region TestComposite Members

         public virtual String A()
         {
            return this._state.Property;
         }

         public abstract Object B();

         #endregion
      }

      public abstract class TestCompositeMixinB : TestComposite
      {
#pragma warning disable 649

         [This]
         private TestCompositeState<Object> _state;

#pragma warning restore 649

         #region TestComposite Members

         public abstract String A();

         public virtual Object B()
         {
            return this._state.Property;
         }

         #endregion
      }
   }
}
