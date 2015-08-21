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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Lifecycle
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class PrototypeTest : AbstractSingletonInstanceTest
   {
      private static Boolean _prototypeMethodCalled = false;
      public const String PROPERTY_VALUE = "Prop";
      public const Int32 MINIMUM_VALUE = 5;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) )
            ;
      }

      [Test]
      public void TestPrototypeMethod()
      {
         this.PerformTestInAppDomain( () =>
         {
            _prototypeMethodCalled = false;
            var builder = this.StructureServices.NewPlainCompositeBuilder<TestComposite>();
            builder.Builder.Use( MINIMUM_VALUE );
            builder.Instantiate();
            Assert.IsTrue( _prototypeMethodCalled );
         } );
      }

      [Test]
      public void TestPrototypeMethodWithInvalidParameters()
      {
         this.PerformTestInAppDomain( () =>
         {
            _prototypeMethodCalled = false;
            var builder = this.StructureServices.NewPlainCompositeBuilder<TestComposite>();
            builder.Builder.Use( MINIMUM_VALUE - 1 );
            Assert.Throws<ConstraintViolationException>( () => builder.Instantiate() );
            Assert.IsFalse( _prototypeMethodCalled );
         } );
      }

      public interface TestComposite
      {

      }

      public interface TestState
      {
         [Immutable]
         String MyProperty { get; set; }
      }

      public class TestCompositeMixin
      {
         [Prototype]
         public void PrototypeInitializer( [This] TestState state, [Uses][TestConstraint] Int32 someParam )
         {
            Assert.IsNull( state.MyProperty );
            state.MyProperty = PROPERTY_VALUE;
            Assert.AreEqual( state.MyProperty, PROPERTY_VALUE );
            _prototypeMethodCalled = true;
         }
      }

      [ConstraintDeclaration]
      [DefaultConstraints( typeof( TestConstraintImpl ) )]
      public class TestConstraintAttribute : Attribute
      {
      }

      public class TestConstraintImpl : Constraint<TestConstraintAttribute, Int32>
      {
         #region Constraint<TestConstraintAttribute,int> Members

         public Boolean IsValid( TestConstraintAttribute attribute, Int32 value )
         {
            return value >= MINIMUM_VALUE;
         }

         #endregion
      }
   }
}
