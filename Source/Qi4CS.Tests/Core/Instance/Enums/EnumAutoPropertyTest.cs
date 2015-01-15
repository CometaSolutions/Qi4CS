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
using NUnit.Framework;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using System;

namespace Qi4CS.Tests.Core.Instance.Enums
{
   [Serializable]
   public class EnumAutoPropertyTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite()
            .OfTypes( typeof( TestComposite ) );
      }

      [Test]
      public void TestCreatingNewCompositeWithNonOptionalEnumDoesNotThrow()
      {
         this.PerformTestInAppDomain( () => Assert.AreEqual( TestEnum.VALUE_1, this.NewPlainComposite<TestComposite>().TestProperty ) );
      }

      [Test]
      public void TestSettingAndGettingAutoEnumProperties()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewPlainCompositeBuilder<TestComposite>();
            builder.Prototype().TestProperty = TestEnum.VALUE_1;

            var composite = builder.Instantiate();
            Assert.AreEqual( TestEnum.VALUE_1, composite.TestProperty );

            composite.TestProperty = TestEnum.VALUE_2;
            Assert.AreEqual( TestEnum.VALUE_2, composite.TestProperty );
         } );
      }

      public enum TestEnum { VALUE_1, VALUE_2 }

      public interface TestComposite
      {
         TestEnum TestProperty { get; set; }
      }
   }
}
