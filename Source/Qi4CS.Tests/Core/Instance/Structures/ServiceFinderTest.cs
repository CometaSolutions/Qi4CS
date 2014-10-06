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
using System.Linq;
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using System;

namespace Qi4CS.Tests.Core.Instance.Structures
{
   [Serializable]
   public class ServiceFinderTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewService().OfTypes( typeof( TestService1 ) );
         assembler.NewService().OfTypes( typeof( TestService2 ) );
      }

      [Test]
      public void TestServiceFinderUsingBaseType()
      {
         this.PerformTestInAppDomain( () =>
         {
            var refs = this.StructureServices.FindServices<BaseInterface>().ToArray();
            Assert.AreEqual( 2, refs.Length, "The amount of service found must be correct." );
            Assert.AreNotEqual( refs[0], refs[1], "The services must be different." );
         } );
      }

      public interface BaseInterface
      {

      }

      public interface TestService1 : BaseInterface
      {

      }

      public interface TestService2 : BaseInterface
      {

      }
   }
}
