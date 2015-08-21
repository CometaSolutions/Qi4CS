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
using Qi4CS.Core.API.Model;
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Services
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class MultiplePassivateMethodsTest : AbstractSingletonInstanceTest
   {
      private static Boolean _firstTriggered;
      private static Boolean _secondTriggered;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewService().SetActivateWithApplication( true ).OfTypes( typeof( TestService ) );
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            _firstTriggered = false;
            _secondTriggered = false;
            Assert.Throws<ApplicationPassivationException>( () => this.Application.Passivate() );
            Assert.IsTrue( _firstTriggered );
            Assert.IsTrue( _secondTriggered );
         } );
      }

      public class TestService
      {
         [Passivate]
         protected void FirstPassivation()
         {
            _firstTriggered = true;
            throw new Exception();
         }

         [Passivate]
         protected void SecondPassivation()
         {
            _secondTriggered = true;
            throw new Exception();
         }
      }
   }
}
