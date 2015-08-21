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

namespace Qi4CS.Tests.Core.Instance.Constraint
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class ConstraintOnReadOnlyPropertyTest : AbstractSingletonInstanceTest
   {
      private static Boolean _constraintCalled = false;
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite3 ) )
            .WithMixins( typeof( TestComposite3Mixin ) ).Done()
            .WithConstraints( typeof( TestConstraintImpl ) );
      }

      [Test]
      public void WithReadOnlyPropertyReturningNullDontInvokeConstraint()
      {
         this.PerformTestInAppDomain( () =>
         {
            _constraintCalled = false;
            String str = null;
            Assert.Throws<ConstraintViolationException>( () => str = this.NewPlainComposite<TestComposite3>().MandatoryGetter );
            Assert.IsFalse( _constraintCalled );
         } );
      }

      public interface TestComposite3
      {
         [TestConstraint]
         String MandatoryGetter { get; }
      }

      public class TestComposite3Mixin : TestComposite3
      {

         #region TestComposite3 Members

         public virtual String MandatoryGetter
         {
            get
            {
               return null;
            }
         }

         #endregion
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
            _constraintCalled = true;
            return true;
         }

         #endregion
      }
   }
}
