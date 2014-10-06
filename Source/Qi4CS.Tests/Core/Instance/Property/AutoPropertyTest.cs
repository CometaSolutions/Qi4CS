/*
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

namespace Qi4CS.Tests.Core.Instance.Property
{
   [Serializable]
   public class AutoPropertyTest : AbstractSingletonInstanceTest
   {
      private const String PROP_VALUE = "Value";
      private const String IMPROP_VALUE = PROP_VALUE + "sdf";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) );
         assembler
            .NewPlainComposite()
            .OfTypes( typeof( TestCompositeWithConstraints ) )
            .WithConstraints( typeof( TestConstraintImpl ) );
         assembler
            .NewPlainComposite()
            .OfTypes( typeof( TestCompositeWithBooleanProperty ) );
         assembler
            .NewPlainComposite()
            .OfTypes( typeof( TestComposite64 ) ); // This composite is only for checking that code generation emits appropriate read methods and check in compostei constructor.
      }

      [Test]
      public void TestSettingAndGettingAutomaticallyImplementedProperties()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            Assert.IsNull( composite.Property, "The initial value must be null." );
            composite.Property = PROP_VALUE;
            Assert.AreEqual( PROP_VALUE, composite.Property, "The value must equal the one which was set." );
         } );
      }

      [Test]
      public void TestSettingAutomaticallyImplementedPropertiesWithConstraints()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestCompositeWithConstraints>();

            Assert.IsNull( composite.Property, "The initial value must be null." );
            composite.Property = PROP_VALUE;
            Assert.AreEqual( PROP_VALUE, composite.Property, "The value must equal the one which was set." );

            Assert.Throws<ConstraintViolationException>( () => composite.Property = IMPROP_VALUE );
            Assert.AreEqual( PROP_VALUE, composite.Property, "The value must remain the same." );
         } );
      }

      [Test]
      public void TestSettingAndGettingAutomaticallyImplementedBooleanProperties()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewPlainCompositeBuilder<TestCompositeWithBooleanProperty>();
            builder.Prototype().BooleanProperty = true;

            var composite = builder.Instantiate();
            Assert.IsTrue( composite.BooleanProperty );
            composite.BooleanProperty = false;
            Assert.IsFalse( composite.BooleanProperty );
         } );
      }

      [Test]
      public void TestValuePropertyWithoutSetting()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewPlainCompositeBuilder<TestCompositeWithBooleanProperty>();
            var composite = builder.Instantiate();
            Assert.IsFalse( composite.BooleanProperty );
         } );
      }

      public interface TestComposite
      {
         [Optional]
         String Property { get; set; }
      }

      public interface TestCompositeWithConstraints
      {
         [Optional, TestConstraint]
         String Property { get; set; }
      }

      public interface TestCompositeWithBooleanProperty
      {
         Boolean BooleanProperty { get; set; }
      }

      [ConstraintDeclaration]
      public class TestConstraintAttribute : Attribute
      {

      }

      public class TestConstraintImpl : Constraint<TestConstraintAttribute, String>
      {

         #region Constraint<TestConstraintAttribute,string> Members

         public Boolean IsValid( TestConstraintAttribute attribute, String value )
         {
            return PROP_VALUE.Equals( value );
         }

         #endregion
      }

      public interface TestComposite64
      {
         // Reading Int64 atomically varies between 32bit and 64bit processes.
         Int64 Property { get; set; }
      }
   }
}
