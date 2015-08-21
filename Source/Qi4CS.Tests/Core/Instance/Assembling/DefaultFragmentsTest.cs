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

namespace Qi4CS.Tests.Core.Instance.Assembling
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class DefaultFragmentsTest : AbstractSingletonInstanceTest
   {
      private static Boolean _constraintInvoked;
      private static Boolean _mixinInvoked;
      private static Boolean _concernInvoked;
      private static Boolean _sideEffectInvoked;

      public const String STRING_PARAM = "Kek";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite ) );
      }

      [Test]
      public void TestAllFragmentsUsed()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            _concernInvoked = false;
            _constraintInvoked = false;
            _mixinInvoked = false;
            _sideEffectInvoked = false;
            composite.Method( STRING_PARAM );
            Assert.IsTrue( _constraintInvoked, "Constraint must've been invoked." );
            Assert.IsTrue( _concernInvoked, "Concern must've been invoked." );
            Assert.IsTrue( _mixinInvoked, "Mixin must've been invoked." );
            Assert.IsTrue( _sideEffectInvoked, "Side effect must've been invoked." );
         } );
      }

      [DefaultConcerns( typeof( TestConcern ) )]
      [DefaultSideEffects( typeof( TestSideEffect ) )]
      [DefaultMixins( typeof( TestMixin ) )]
      public interface TestComposite
      {
         void Method( [TestConstraint] String parameter );
      }

      [ConstraintDeclaration]
      [DefaultConstraints( typeof( TestConstraintImpl ) )]
      public class TestConstraintAttribute : Attribute
      {

      }

      public class TestConstraintImpl : Constraint<TestConstraintAttribute, String>
      {

         #region Constraint<TestConstraintAttribute,string> Members

         public Boolean IsValid( TestConstraintAttribute attribute, String value )
         {
            _constraintInvoked = true;
            return value.Length > 0;
         }

         #endregion
      }

      public class TestConcern : ConcernOf<TestComposite>, TestComposite
      {

         #region TestComposite Members

         public virtual void Method( String parameter )
         {
            _concernInvoked = true;
            this.next.Method( parameter );
         }

         #endregion
      }

      public class TestMixin : TestComposite
      {

         #region TestComposite Members

         public virtual void Method( String parameter )
         {
            _mixinInvoked = true;
         }

         #endregion
      }

      public class TestSideEffect : SideEffectOf<TestComposite>, TestComposite
      {

         #region TestComposite Members

         public virtual void Method( String parameter )
         {
            _sideEffectInvoked = true;
            this.result.Method( parameter );
         }

         #endregion
      }
   }
}
