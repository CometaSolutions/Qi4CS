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

namespace Qi4CS.Tests.Core.Instance.Injection
{
   [Serializable]
   public class UsesInjectionTestWithServiceComposite : AbstractSingletonInstanceTest
   {
      private static readonly ServiceUses USES = new ServiceUses();

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewService().OfTypes( typeof( TestServiceWithUsesGiven ) )
            .WithMixins( typeof( TestServiceMixin ) ).Done()
            .Use( USES );
         assembler
            .NewService().OfTypes( typeof( TestServiceWithNoUsesGiven ) )
            .WithMixins( typeof( TestServiceMixinWithSpecialMethod ) );
      }

      [Test]
      public void TestServiceUsesWorks()
      {
         this.PerformTestInAppDomain( () => this.FindService<TestServiceWithUsesGiven>().PerformTest() );
      }

      // TODO this test actually should throw exception already at model validation stage
      [Test]
      public void TestServiceUsesThrowsIfNoUsesProvided()
      {
         this.PerformTestInAppDomain( () => Assert.Throws<ConstraintViolationException>( () => this.FindService<TestServiceWithNoUsesGiven>().PerformTest() ) );
      }

      public class ServiceUses
      {

      }

      public interface TestService
      {
         void PerformTest();
      }

      public interface TestServiceWithUsesGiven : TestService
      {

      }

      public interface TestServiceWithNoUsesGiven : TestService
      {

      }

      public class TestServiceMixin : TestService
      {
#pragma warning disable 649

         [Uses]
         private ServiceUses _uses;

#pragma warning restore 649

         #region TestService Members

         public virtual void PerformTest()
         {
            Assert.IsTrue( Object.ReferenceEquals( this._uses, USES ) );
         }

         #endregion
      }

      public class TestServiceMixinWithSpecialMethod : TestService
      {

         #region TestService Members

         public virtual void PerformTest()
         {
            throw new NotImplementedException();
         }

         #endregion

         [Activate]
         public void Activate( [Uses] ServiceUses uses )
         {

         }
      }
   }

}
