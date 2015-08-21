/*
 * Copyright (c) 2008, Niclas Hedhman.
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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.AppliesTo
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class AppliesToTest : AbstractSingletonInstanceTest
   {
      public const String FIRST_APPENDED_VALUE = "FirstAppended";
      public const String SECOND_APPENDED_VALUE = "SecondAppended";
      public const String THIRD_APPENDED_VALUE = "ThirdAppended";
      public const String FIRST_RESULT_VALUE = "FirstResult";
      public const String SECOND_RESULT_VALUE = "SecondResult";
      public const String THIRD_RESULT_VALUE = "ThirdResult";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestMixin ) ).Done()
            .WithConcerns( typeof( TestConcern ) ).ApplyWith( typeof( TestAppliesToAttribute ) );
      }

      public interface TestComposite
      {
         String TestMethod1();

         [TestAppliesTo]
         String TestMethod2();

         String TestMethod3();
      }

      [Test]
      public void GivenAnAppliesToWhenNoAnnotationExpectNoConcernInInvocationStack()
      {
         this.PerformTestInAppDomain( () => Assert.AreEqual( FIRST_RESULT_VALUE, this.NewPlainComposite<TestComposite>().TestMethod1(), "Concern must not apply to first method." ) );
      }

      [Test]
      public void GivenAnAppliesToWhenAnnotationIsOnMixinTypeExpectConcernInInvocationStack()
      {
         this.PerformTestInAppDomain( () => Assert.AreEqual( SECOND_RESULT_VALUE + SECOND_APPENDED_VALUE, this.NewPlainComposite<TestComposite>().TestMethod2(), "Concern must apply to second method." ) );
      }

      [Test]
      public void GivenAnAppliesToWhenAnnotationIsOnMixinImplementationExpectConcernInInvocationStack()
      {
         this.PerformTestInAppDomain( () =>
         {
            TestComposite composite = this.NewPlainComposite<TestComposite>();
            Assert.AreEqual( FIRST_RESULT_VALUE, composite.TestMethod1(), "Concern must not apply to first method." );
            Assert.AreEqual( SECOND_RESULT_VALUE + SECOND_APPENDED_VALUE, composite.TestMethod2(), "Concern must apply to second method." );
            Assert.AreEqual( THIRD_RESULT_VALUE + THIRD_APPENDED_VALUE, composite.TestMethod3(), "Concern must apply to third method." );
         } );
      }

      public class TestAppliesToAttribute : Attribute
      {

      }

      public class TestConcern : ConcernOf<TestComposite>, TestComposite
      {

         #region TestComposite Members

         public virtual String TestMethod1()
         {
            return this.next.TestMethod1() + FIRST_APPENDED_VALUE;
         }

         public virtual String TestMethod2()
         {
            return this.next.TestMethod2() + SECOND_APPENDED_VALUE;
         }

         public virtual String TestMethod3()
         {
            return this.next.TestMethod3() + THIRD_APPENDED_VALUE;
         }

         #endregion
      }

      public class TestMixin : TestComposite
      {

         #region TestComposite Members

         public virtual String TestMethod1()
         {
            return FIRST_RESULT_VALUE;
         }

         public virtual String TestMethod2()
         {
            return SECOND_RESULT_VALUE;
         }

         [TestAppliesTo]
         public virtual String TestMethod3()
         {
            return THIRD_RESULT_VALUE;
         }

         #endregion
      }
   }
}
