/*
 * Copyright (c) 2008, Rickard Öberg.
 * (org.qi4j.api.OptionalTest)
 * See NOTICE file.
 * 
 * Copyright 2011 Stanislav Muhametsin. All rights Reserved.
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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Common
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class OptionalTest : AbstractSingletonInstanceTest
   {
      public const String RESULT = "Stuff";
      public const String PROPERTY_VALUE = "PropVal";

      protected override void Assemble( Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite ) ).WithMixins( typeof( TestMixin ) );
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite2 ) );
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite3 ) ).WithMixins( typeof( TestComposite3Mixin ) );
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite4 ) ).WithMixins( typeof( TestComposite4Mixin ) );
      }

      [Test]
      public void GivenOptionalMethodWhenCorrectInvokeThenNoException()
      {
         this.PerformTestInAppDomain( () =>
         {
            var instance = this.NewPlainComposite<TestComposite>();
            instance.DoStuff( "Hello WOrld", "Hello World" );
         } );
      }

      [Test]
      public void GivenOptionalMethodWhenMandatoryMissingThenException()
      {
         this.PerformTestInAppDomain( () =>
         {
            var instance = this.NewPlainComposite<TestComposite>();
            Assert.Throws<ConstraintViolationException>( () => instance.DoStuff( "Hello World", null ), "Method parameters must be checked for constraints." );
         } );
      }

      [Test]
      public void GivenOptionalMethodWhenOptionalMissingThenNoException()
      {
         this.PerformTestInAppDomain( () =>
         {
            var instance = this.NewPlainComposite<TestComposite>();
            instance.DoStuff( null, "Hello World" );
         } );
      }

      [Test]
      public void GivenOptionalReturnMethodWhenCorrectInvokeThenNoException()
      {
         this.PerformTestInAppDomain( () =>
         {
            var instance = this.NewPlainComposite<TestComposite>();
            Assert.AreEqual( RESULT, instance.GetStuff(), "The result must be equal to expected" );
            Assert.AreEqual( RESULT, instance.GetOptionalStuff(), "The result must be equal to expected" );
            Assert.AreEqual( null, instance.GetOptionalStuffNull(), "The result must be equal to expected" );
         } );
      }

      [Test]
      public void GivenOptionalReturnMethodWhenReturningNullThenException()
      {
         this.PerformTestInAppDomain( () =>
         {
            var instance = this.StructureServices.NewCompositeBuilder<TestComposite>( CompositeModelType.PLAIN ).Instantiate();
            Assert.Throws<ConstraintViolationException>( () => instance.GetStuffNull(), "Null return value must be checked for constraints." );
         } );
      }

      [Test]
      public void GivenOptionalPropertyWhenOptionalMissingThenNoException()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewPlainCompositeBuilder<TestComposite2>();
            builder.Prototype().MandatoryProperty = PROPERTY_VALUE;
            builder.Instantiate();
         } );
      }

      [Test]
      public void GivenOptionalPropertyWhenOptionalSetThenNoException()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewPlainCompositeBuilder<TestComposite2>();
            builder.Prototype().MandatoryProperty = PROPERTY_VALUE;
            builder.Prototype().OptionalProperty = PROPERTY_VALUE;
            builder.Instantiate();
         } );
      }

      [Test]
      public void GivenMandatoryPropertyWhenMandatoryMissingThenException()
      {
         this.PerformTestInAppDomain( () => Assert.Throws<CompositeInstantiationException>( () => this.NewPlainComposite<TestComposite2>(), "Exception must be thrown when mandatory properties are missing." ) );
      }

      [Test]
      public void GivenNonNullNullableThenNoException()
      {
         this.PerformTestInAppDomain( () => this.NewPlainComposite<TestComposite3>().TestMethod<Int32>( 1, 2 ) );
      }

      [Test]
      public void GivenNullOptionalNullableThenNoException()
      {
         this.PerformTestInAppDomain( () => this.NewPlainComposite<TestComposite3>().TestMethod<Int32>( null, 2 ) );
      }

      [Test]
      public void GivenNullMandatoryNullableThenNoException()
      {
         this.PerformTestInAppDomain( () => this.NewPlainComposite<TestComposite3>().TestMethod<Int32>( null, null ) );
      }

      [Test]
      public void GivenDefaultParameterWithNullThenNoException()
      {
         this.PerformTestInAppDomain( () => this.NewPlainComposite<TestComposite4>().MethodWithNullParameter() );
      }

      public interface TestComposite
      {
         void DoStuff( [Optional] String optional, String mandatory );

         String GetStuff();

         String GetStuffNull();

         [return: Optional]
         String GetOptionalStuffNull();

         [return: Optional]
         String GetOptionalStuff();
      }

      public class TestMixin : TestComposite
      {
         #region TestComposite Members

         public virtual void DoStuff( String optional, String mandatory )
         {
            Assert.NotNull( mandatory, "Mandatory is not null" );
         }

         public virtual String GetStuff()
         {
            return RESULT;
         }

         public virtual String GetOptionalStuffNull()
         {
            return null;
         }

         public virtual String GetStuffNull()
         {
            return null;
         }

         public virtual String GetOptionalStuff()
         {
            return RESULT;
         }

         #endregion
      }

      public interface TestComposite2
      {
         [Optional]
         String OptionalProperty { get; set; }

         String MandatoryProperty { get; set; }
      }

      public interface TestComposite3
      {
         void TestMethod<T>( [Optional] T? nullableOptional, T? nullableMandatory )
            where T : struct;
      }

      public class TestComposite3Mixin : TestComposite3
      {

         #region TestComposite3 Members

         public virtual void TestMethod<T>( T? nullableOptional, T? nullableMandatory )
            where T : struct
         {
            // Don't do anything
         }

         #endregion
      }

      public interface TestComposite4
      {
         void MethodWithNullParameter( String param = null );
      }

      public class TestComposite4Mixin : TestComposite4
      {

         #region TestComposite4 Members

         public virtual void MethodWithNullParameter( string param = null )
         {

         }

         #endregion
      }
   }
}
