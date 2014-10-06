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
using System.Reflection;
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   public class ParameterModificationsTest : AbstractSingletonInstanceTest
   {
      private const Int32 REF_PARAM_CORRECT_VALUE = 345;
      private const Int32 REF_PARAM_INCORRECT_VALUE = 2356;
      private static Boolean _sideEffectSuccessfulyExecuted = false;

      protected override void Assemble( Assembler assembler )
      {
         assembler
            .NewPlainComposite()
            .OfTypes( typeof( TestComposite ) )
            .WithConcerns( typeof( TestCompositeConcern ) ).Done()
            .WithMixins( typeof( TestCompositeMixin ) ).Done()
            .WithSideEffects( typeof( TestCompositeSideEffect ) ).Done()
            .WithConstraints( typeof( TestConstraintImpl ) );
      }

      [Test]
      public void TestParameterModifiers()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            var refParam = REF_PARAM_CORRECT_VALUE;
            TestParam outParam;
            _sideEffectSuccessfulyExecuted = false;
            composite.TestMethod( ref refParam, out outParam );
            Assert.AreEqual( refParam, REF_PARAM_CORRECT_VALUE );
            Assert.IsNotNull( outParam );
            Assert.IsTrue( _sideEffectSuccessfulyExecuted );
         } );
      }

      public class TestParam
      {
         private String _value;
         public TestParam()
         {
            this._value = null;
         }

         public String Value
         {
            get
            {
               return this._value;
            }
            set
            {
               this._value = value;
            }
         }
      }

      public interface TestComposite
      {
         void TestMethod( [TestConstraint] ref Int32 refParam, out TestParam outParam );
      }

      public class TestCompositeConcern : GenericConcern
      {

         public override Object Invoke( Object composite, MethodInfo method, Object[] args )
         {
            Assert.AreEqual( REF_PARAM_CORRECT_VALUE, args[0] );
            Assert.IsNull( args[1] );
            Object result = this.next.Invoke( composite, method, args );
            Assert.IsNotNull( args[1] );
            return result;
         }
      }

      public class TestCompositeMixin : TestComposite
      {

         #region TestComposite Members

         public virtual void TestMethod( ref Int32 refParam, out TestParam outParam )
         {
            Assert.AreEqual( REF_PARAM_CORRECT_VALUE, refParam );
            outParam = new TestParam();
         }

         #endregion
      }

      public class TestCompositeSideEffect : SideEffectOf<TestComposite>, TestComposite
      {

         #region TestComposite Members

         public virtual void TestMethod( ref int refParam, out TestParam outParam )
         {
            Assert.AreEqual( refParam, REF_PARAM_CORRECT_VALUE );
            // Test that setting parameters will not affect final output
            outParam = null;
            refParam = REF_PARAM_INCORRECT_VALUE;

            // Test that results of byRef params are visible to us
            Int32 refParamResult = REF_PARAM_INCORRECT_VALUE;
            TestParam outParamResult;
            this.result.TestMethod( ref refParamResult, out outParamResult );
            Assert.AreEqual( refParamResult, REF_PARAM_CORRECT_VALUE );
            Assert.IsNotNull( outParamResult );
            _sideEffectSuccessfulyExecuted = true;
         }

         #endregion
      }

      [ConstraintDeclaration]
      public class TestConstraintAttribute : Attribute
      {

      }

      public class TestConstraintImpl : Constraint<TestConstraintAttribute, Int32>
      {

         #region Constraint<TestConstraintAttribute,int> Members

         public Boolean IsValid( TestConstraintAttribute attribute, Int32 value )
         {
            return REF_PARAM_CORRECT_VALUE == value;
         }

         #endregion
      }
   }
}
