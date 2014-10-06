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
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.API.Model;
using NUnit.Framework;

namespace Qi4CS.Tests.Core.Instance.Lifecycle
{
   [Serializable]
   public class ProtectedSpecialMethodTest : AbstractSingletonInstanceTest
   {
      private static Boolean _prototypeMethodInvoked = false;

      protected override void Assemble( Assembler assembler )
      {
         assembler
            .NewPlainComposite()
            .OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) );
      }

      [Test]
      public void TestCompositeWithProtectedSpecialMethod()
      {
         this.PerformTestInAppDomain( () =>
         {
            _prototypeMethodInvoked = false;
            var transu = this.NewPlainComposite<TestComposite>();
            Assert.IsTrue( _prototypeMethodInvoked );
         } );
      }

      public interface TestComposite
      {

      }

      public class TestCompositeMixin : TestComposite
      {

         [Prototype]
         protected void PrototypeMethod()
         {
            _prototypeMethodInvoked = true;
         }
      }
   }
}
