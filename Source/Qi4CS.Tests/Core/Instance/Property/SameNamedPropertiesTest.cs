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

namespace Qi4CS.Tests.Core.Instance.Property
{
   [Serializable]
   public class SameNamedPropertiesTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestPublicComposite ) ).WithMixins( typeof( TestPublicCompositeMixin ) );
      }

      [Test]
      public void TestSameNamedPropertiesWork()
      {
         this.PerformTestInAppDomain( () => Assert.IsNull( this.NewPlainComposite<TestPublicComposite>().MyProperty ) );
      }

      public interface TestPublicComposite
      {
         String MyProperty { [return: Optional] get; }
      }

      public interface TestPrivateComposite
      {
         [Optional]
         String MyProperty { get; set; }
      }

      public class TestPublicCompositeMixin : TestPublicComposite
      {
#pragma warning disable 649

         [This]
         private TestPrivateComposite _state;

#pragma warning restore 649

         #region TestPublicComposite Members

         public virtual String MyProperty
         {
            get
            {
               return this._state.MyProperty;
            }
         }

         #endregion
      }
   }
}
