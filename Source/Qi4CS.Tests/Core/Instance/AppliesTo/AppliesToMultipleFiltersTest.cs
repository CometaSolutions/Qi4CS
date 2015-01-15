/*
 * (No copyright provided on this file in Qi4j)
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
   public class AppliesToMultipleFiltersTest : AbstractSingletonInstanceTest
   {
      public const String TEST_METHOD_RESULT = "TestMethodResult";
      public const String FIRST_ATTRIBUTE_VALUE = "First";
      public const String SECOND_ATTRIBUTE_VALUE = "Second";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewService().OfTypes( typeof( TestServiceWithTwoFilterAttributes ) )
            .WithMixins( typeof( TestServiceMixinWithTwoFilterAttributes ) ).Done()
            .WithConcerns( typeof( TestConcern ) ).ApplyWith( typeof( FirstAttribute ), typeof( SecondAttribute ) );
         assembler
            .NewService().OfTypes( typeof( TestServiceWithFirstFilterAttribute ) )
            .WithMixins( typeof( TestServiceMixinWithFirstFilterAttribute ) ).Done()
            .WithConcerns( typeof( TestConcern ) ).ApplyWith( typeof( FirstAttribute ), typeof( SecondAttribute ) );
         assembler
            .NewService().OfTypes( typeof( TestServiceWithSecondFilterAttribute ) )
            .WithMixins( typeof( TestServiceMixinWithSecondFilterAttribute ) ).Done()
            .WithConcerns( typeof( TestConcern ) ).ApplyWith( typeof( FirstAttribute ), typeof( SecondAttribute ) );
      }

      protected void PerformTest<ServiceType>()
         where ServiceType : class, TestServiceBase
      {
         ServiceType service = this.FindService<ServiceType>();
         service.TestMethod();
         Assert.IsTrue( service.ConcernTriggered, "The concern must've been called." );
      }

      [Test]
      public void TestMultipleFilterAttributes1()
      {
         this.PerformTestInAppDomain( () => this.PerformTest<TestServiceWithTwoFilterAttributes>() );
      }

      [Test]
      public void TestMultipleFilterAttributes2()
      {
         this.PerformTestInAppDomain( () => this.PerformTest<TestServiceWithFirstFilterAttribute>() );
      }

      [Test]
      public void TestMultipleFilterAttributes3()
      {
         this.PerformTestInAppDomain( () => this.PerformTest<TestServiceWithSecondFilterAttribute>() );
      }

      public interface TestServiceBase
      {
         String TestMethod();
         void TriggerConcern();
         Boolean ConcernTriggered { get; }
      }

      public interface TestServiceWithTwoFilterAttributes : TestServiceBase
      {

      }

      public interface TestServiceWithFirstFilterAttribute : TestServiceBase
      {

      }

      public interface TestServiceWithSecondFilterAttribute : TestServiceBase
      {

      }

      public class FirstAttribute : Attribute
      {
         private String _value;
         public FirstAttribute( String value )
         {
            this._value = value;
         }

         public String Value
         {
            get
            {
               return this._value;
            }
         }
      }

      public class SecondAttribute : Attribute
      {
         private String _value;
         public SecondAttribute( String value )
         {
            this._value = value;
         }

         public String Value
         {
            get
            {
               return this._value;
            }
         }
      }

      public abstract class TestServiceBaseMixin : TestServiceBase
      {

         private Boolean _triggered = false;

         #region TestServiceBase Members

         public abstract String TestMethod();

         public virtual void TriggerConcern()
         {
            this._triggered = true;
         }

         public virtual Boolean ConcernTriggered
         {
            get
            {
               return this._triggered;
            }
         }

         #endregion
      }

      public class TestServiceMixinWithTwoFilterAttributes : TestServiceBaseMixin
      {
         [First( FIRST_ATTRIBUTE_VALUE )]
         [Second( SECOND_ATTRIBUTE_VALUE )]
         public override String TestMethod()
         {
            return TEST_METHOD_RESULT;
         }
      }

      public class TestServiceMixinWithFirstFilterAttribute : TestServiceBaseMixin
      {
         [First( FIRST_ATTRIBUTE_VALUE )]
         public override String TestMethod()
         {
            return TEST_METHOD_RESULT;
         }
      }

      public class TestServiceMixinWithSecondFilterAttribute : TestServiceBaseMixin
      {
         [Second( SECOND_ATTRIBUTE_VALUE )]
         public override String TestMethod()
         {
            return TEST_METHOD_RESULT;
         }
      }

      public class TestConcern : GenericConcern
      {
#pragma warning disable 649
         [This]
         private TestServiceBase _service;

#pragma warning restore 649

         public override object Invoke( Object composite, System.Reflection.MethodInfo method, Object[] args )
         {
            this._service.TriggerConcern();
            return this.next.Invoke( composite, method, args );
         }
      }
   }
}
