using NUnit.Framework;
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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using System;

namespace Qi4CS.Tests.Core.Instance.Services
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class CallMethodsWhilePassivatingTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewService().OfTypes( typeof( TestServiceComposite ) )
            .WithMixins( typeof( TestServiceMixin ) );
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () => this.FindService<TestServiceComposite>().DoSomething() );
      }

      public interface TestServiceComposite
      {
         void DoSomething();
      }

      public class TestServiceMixin : TestServiceComposite
      {

         #region TestServiceComposite Members

         public virtual void DoSomething()
         {
            // Do nothing
         }

         #endregion

         [Passivate]
         public void Passivate()
         {
            this.DoSomething();
         }
      }


   }
}
