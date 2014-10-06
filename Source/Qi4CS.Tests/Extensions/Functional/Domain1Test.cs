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
using Qi4CS.Core.API.Instance;
using Qi4CS.Tests.Extensions.Functional.Domain1;
using NUnit.Framework;
using Qi4CS.Tests.Core.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Extensions.Functional.Assembling;

namespace Qi4CS.Tests.Extensions.Functional
{
   [Serializable]
   public class Domain1Test : AbstractSingletonInstanceTest
   {
      private const String TEST_STRING = "TestString";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewService().AsFunctionAggregator<Type, TestFunction>()
            .WithFunctionType(
            FunctionAssemblerUtils.TypeBasedFunctionLookUp( 0 )
            )
            .WithDefaultFunctions(
               Tuple.Create<Type[], Func<StructureServiceProvider, TestFunction>>( new Type[] { typeof( MyData ) }, ssp => ssp.NewPlainCompositeBuilder<MyFunction>().Instantiate() )
            ).Done()
            .OfTypes( typeof( TestService ) );

         assembler.NewPlainComposite().OfTypes( typeof( MyData ) );
         assembler.NewPlainComposite().OfTypes( typeof( MyFunction ) );
      }

      [Test]
      public void TestFunction()
      {
         this.PerformTestInAppDomain( () =>
         {
            TestFunction service = this.FindService<TestFunction>();

            TestData data = this.NewPlainComposite<MyData>();
            data.TestData = TEST_STRING;

            Assert.AreEqual( service.TestFunction( data ), TEST_STRING );
         } );
      }

      public interface TestService
      {
      }
   }
}
