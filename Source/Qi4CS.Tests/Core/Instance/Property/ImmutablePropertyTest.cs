/*
 * Copyright (c) 2008, Edward Yakop.
 * See NOTICE file.
 * 
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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Property
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class ImmutablePropertyTest : AbstractSingletonInstanceTest
   {

      private const String STRING_VALUE = "StringValue";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite ) );
      }

      [Test]
      public void PrototypeAssignmentWorksTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.CreateComposite( STRING_VALUE );
            this.TestProperty( composite, STRING_VALUE );
         } );
      }

      [Test]
      public void PrototypeAssignmentWorksWithPlainPrototypeMethodTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewCompositeBuilder<TestComposite>( CompositeModelType.PLAIN );
            builder.Prototype().TestValue = STRING_VALUE;
            var composite = builder.Instantiate();
            this.TestProperty( composite, STRING_VALUE );
         } );
      }

      [Test]
      public void WhenAssigningValueToImmutablePropertyThenExceptionTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.CreateComposite( STRING_VALUE );
            Assert.Throws<InvalidOperationException>( () =>
            {
               composite.TestValue = "SomethingElse";
            }, "The setter must throw an exception." );
            this.TestProperty( composite, STRING_VALUE );
         } );
      }

      protected TestComposite CreateComposite( String propValue )
      {
         var builder = this.StructureServices.NewCompositeBuilder<TestComposite>( CompositeModelType.PLAIN );
         builder.Builder.Prototype<TestComposite>().TestValue = propValue;
         return builder.Instantiate();
      }

      protected void TestProperty( TestComposite composite, String propVal )
      {
         Assert.IsNotNull( composite.TestValue, "The auto-generated property for composite must be non-null." );
         Assert.AreEqual( propVal, composite.TestValue, "The auto-generated property value must be as specified." );
      }

      public interface TestComposite
      {
         [Immutable]
         String TestValue { get; set; }
      }
   }
}
