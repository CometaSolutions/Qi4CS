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
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Structures
{
   [Serializable]
   public class CompositeBuilderTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite1 ) );
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite2 ) );
      }

      [Test]
      public void TestDynamicCompositeBuilder()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewCompositeBuilder( CompositeModelType.PLAIN, typeof( TestComposite1 ) );
            var composite = builder.Instantiate<TestComposite1>();
         } );
      }

      [Test]
      public void TestInstantiatingServiceComposite()
      {
         this.PerformTestInAppDomain( () =>
         Assert.Throws<InvalidCompositeModelTypeException>(
            () => this.StructureServices.NewCompositeBuilder<TestComposite1>( CompositeModelType.SERVICE ),
            "The structure service provider must not instantiate composite builders for services."
            ) );
      }

      [Test]
      public void TestInstantiatingUsingAmbiguousCompositeType()
      {
         this.PerformTestInAppDomain( () =>
         Assert.Throws<AmbiguousTypeException>(
            () => this.StructureServices.NewCompositeBuilder<BaseType>( CompositeModelType.PLAIN ),
            "The structure service provider must throw exception on ambiguous types."
            ) );
      }

      public interface BaseType
      {

      }

      public interface TestComposite1 : BaseType
      {

      }

      public interface TestComposite2 : BaseType
      {

      }

   }
}
