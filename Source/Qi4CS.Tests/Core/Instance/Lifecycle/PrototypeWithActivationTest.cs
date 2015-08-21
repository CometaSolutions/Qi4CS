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

namespace Qi4CS.Tests.Core.Instance.Lifecycle
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class PrototypeWithActivationTest : AbstractSingletonInstanceTest
   {

      private static Boolean _prototypeCalled = false;
      private static Boolean _activateCalled = false;
      private static Boolean _mixinCalled = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewService().OfTypes( typeof( TestService ) )
            .WithMixins( typeof( TestServiceMixin ) );
      }

      [Test]
      public void GivenServiceWithLifecycleMethodsNoCodeGenVerificationErrors()
      {
         this.PerformTestInAppDomain( () =>
         {
            _prototypeCalled = false;
            _activateCalled = false;
            _mixinCalled = false;
            var service = this.FindService<TestService>();
            service.PerformTest();
            Assert.IsTrue( _prototypeCalled );
            Assert.IsTrue( _activateCalled );
            Assert.IsTrue( _mixinCalled );
         } );
      }

      public interface TestService
      {
         void PerformTest();
      }

      public class TestServiceMixin : TestService
      {

         #region TestService Members

         public virtual void PerformTest()
         {
            _mixinCalled = true;
         }

         #endregion

         [Prototype]
         public void SetupPrototype()
         {
            _prototypeCalled = true;
         }

         [Activate]
         public void Activate()
         {
            _activateCalled = true;
         }

         [Passivate]
         public void Passivate()
         {

         }
      }
   }
}
