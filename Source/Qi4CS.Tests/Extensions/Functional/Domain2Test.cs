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
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Tests.Extensions.Functional.Domain2;
using Qi4CS.Core.API.Common;
using NUnit.Framework;
using Qi4CS.Tests.Core.Instance;
using Qi4CS.Extensions.Functional.Assembling;

namespace Qi4CS.Tests.Extensions.Functional
{
   [Serializable]
   [Category( "Qi4CS.Extensions.Functional" )]
   public class Domain2Test : AbstractSingletonInstanceTest
   {

      protected override void Assemble( Assembler assembler )
      {
         assembler.NewService().AsFunctionAggregator<Type, TestFunction>()
            .MapServiceMethodToFunctionMethod(
               typeof( TestFunctionServiceSPI ).LoadMethodWithParamTypesOrThrow( "Validate", new Type[] { typeof( TestData1 ), typeof( TestData2 ), typeof( IDictionary<TestData1, Result> ) } ),
               typeof( TestFunction ).LoadMethodOrThrow( "Validate", null ),
               FunctionAssemblerUtils.TypeBasedFunctionLookUp( 1 ),
               args => new Object[] { args[2] },
               null,
               null
            ).MapServiceMethodToFunctionMethod(
               typeof( TestFunctionServiceSPI ).LoadMethodWithParamTypesOrThrow( "Validate", new Type[] { typeof( TestData1 ), typeof( TestData3 ), typeof( IDictionary<TestData1, Result> ) } ),
               typeof( TestFunction ).LoadMethodOrThrow( "Validate", null ),
               FunctionAssemblerUtils.TypeBasedFunctionLookUp( 1 ),
               args => new Object[] { args[2] },
               null,
               null
            ).WithDefaultFunctions(
               Tuple.Create<Type[], Func<StructureServiceProvider, TestFunction>>( new Type[] { typeof( TestData2Impl ) }, ssp => ssp.NewPlainCompositeBuilder<TestFunction1Impl>().Instantiate() ),
               Tuple.Create<Type[], Func<StructureServiceProvider, TestFunction>>( new Type[] { typeof( TestData3Impl ) }, ssp => ssp.NewPlainCompositeBuilder<TestFunction2Impl>().Instantiate() )
            ).Done()
            .WithMixins( typeof( TestFunctionServiceMixin ) ).Done()
            .OfTypes( typeof( TestFunctionServiceSPI ) );

         assembler.NewPlainComposite()
            .OfTypes( typeof( TestData1Impl ) );
         assembler.NewPlainComposite()
            .OfTypes( typeof( TestData2Impl ) );
         assembler.NewPlainComposite()
            .OfTypes( typeof( TestData3Impl ) );
         assembler.NewPlainComposite()
            .OfTypes( typeof( TestFunction1Impl ) );
         assembler.NewPlainComposite()
            .OfTypes( typeof( TestFunction2Impl ) );
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var data1 = this.NewPlainComposite<TestData1Impl>();
            var data2 = this.NewPlainComposite<TestData2Impl>();
            var data3 = this.NewPlainComposite<TestData3Impl>();
            data2.Data = data3;

            this.FindService<TestFunctionService>().IsValid( data1, data2 );

            Assert.IsTrue( TestFunction1Impl.called );
            Assert.IsTrue( TestFunction2Impl.called );
         } );
      }
   }
}
