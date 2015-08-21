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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Transients
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class EqualsHashCodeTest : AbstractSingletonInstanceTest
   {
      private static Boolean _publicCompositeEqualsCalled = false;
      private static Boolean _hashCodeCalled = false;
      private static Boolean _privateCompositeEqualsCalled = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestTransient ) )
            .WithMixins( typeof( TestTransientMixin ) );
         assembler
            .NewPlainComposite().OfTypes( typeof( TestTransientWithPrivateComposite ) )
            .WithMixins( typeof( TestTransientWithPrivateCompositeMixin ), typeof( TestPrivateCompositeMixin ) );
      }

      [Test]
      public void TestEqualsMethodDoesntGetCalledWhenNotNecessary()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestTransient>();
            _publicCompositeEqualsCalled = false;
            Assert.IsFalse( composite.Equals( null ) );
            Assert.IsFalse( _publicCompositeEqualsCalled );
            Assert.IsFalse( composite.Equals( new Object() ) );
            Assert.IsFalse( _publicCompositeEqualsCalled );
            Assert.IsTrue( composite.Equals( composite ) );
            Assert.IsFalse( _publicCompositeEqualsCalled );
         } );
      }

      [Test]
      public void TestEqualsMethodGetsCalledWhenNecessary()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite1 = this.NewPlainComposite<TestTransient>();
            var composite2 = this.NewPlainComposite<TestTransient>();
            _publicCompositeEqualsCalled = false;
            Assert.IsTrue( composite1.Equals( composite2 ) );
            Assert.IsTrue( _publicCompositeEqualsCalled );
         } );
      }

      [Test]
      public void TestHashCodeGetsCalled()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestTransient>();
            _hashCodeCalled = false;
            composite.GetHashCode();
            Assert.IsTrue( _hashCodeCalled );
         } );
      }

      [Test]
      public void TestEqualsMethodDoesntGetCalledWhenNotNecessaryWithComplexComposite()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestTransientWithPrivateComposite>();

            _publicCompositeEqualsCalled = false;
            _privateCompositeEqualsCalled = false;
            Assert.IsFalse( composite.Equals( null ) );
            Assert.IsFalse( _publicCompositeEqualsCalled );
            Assert.IsFalse( composite.Equals( new Object() ) );
            Assert.IsFalse( _publicCompositeEqualsCalled );
            Assert.IsTrue( composite.Equals( composite ) );
            Assert.IsFalse( _publicCompositeEqualsCalled );
            Assert.IsFalse( composite.GetPrivate().Equals( null ) );
            Assert.IsFalse( _privateCompositeEqualsCalled );
            Assert.IsFalse( composite.GetPrivate().Equals( new Object() ) );
            Assert.IsFalse( _privateCompositeEqualsCalled );
            Assert.IsTrue( composite.GetPrivate().Equals( composite.GetPrivate() ) );
            Assert.IsFalse( _privateCompositeEqualsCalled );
         } );
      }

      [Test]
      public void TestEqualsMethodGetsCalledWhenNecessaryWithComplexComposite()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite1 = this.NewPlainComposite<TestTransientWithPrivateComposite>();
            var composite2 = this.NewPlainComposite<TestTransientWithPrivateComposite>();

            _publicCompositeEqualsCalled = false;
            _privateCompositeEqualsCalled = false;
            Assert.IsTrue( composite1.Equals( composite2 ) );
            Assert.IsTrue( _publicCompositeEqualsCalled );
            Assert.IsTrue( _privateCompositeEqualsCalled );

            _publicCompositeEqualsCalled = false;
            _privateCompositeEqualsCalled = false;
            Assert.IsTrue( composite1.GetPrivate().Equals( composite2.GetPrivate() ) );
            Assert.IsFalse( _publicCompositeEqualsCalled );
            Assert.IsTrue( _privateCompositeEqualsCalled );
         } );
      }

      public interface TestTransient
      {
         void TestMethod();
      }

      public class TestTransientMixin : TestTransient
      {

         #region TestTransient Members

         public virtual void TestMethod()
         {
            throw new NotImplementedException();
         }

         #endregion

         public override Boolean Equals( Object obj )
         {
            _publicCompositeEqualsCalled = true;
            return true;
         }

         public override Int32 GetHashCode()
         {
            _hashCodeCalled = true;
            return base.GetHashCode();
         }
      }

      public interface TestTransientWithPrivateComposite
      {
         TestPrivateComposite GetPrivate();
      }

      public interface TestPrivateComposite
      {
         void TestMethod();
      }

#pragma warning disable 659
      public class TestTransientWithPrivateCompositeMixin : TestTransientWithPrivateComposite
      {
#pragma warning disable 649

         [This]
         private TestPrivateComposite _private;

#pragma warning restore 649

         public override Boolean Equals( Object obj )
         {
            _publicCompositeEqualsCalled = true;
            return obj is TestTransientWithPrivateComposite && this._private.Equals( ( (TestTransientWithPrivateComposite) obj ).GetPrivate() );
         }

         #region TestTransientWithPrivateComposite Members

         public virtual TestPrivateComposite GetPrivate()
         {
            return this._private;
         }

         #endregion

      }
#pragma warning restore 659

      public class TestPrivateCompositeMixin : TestPrivateComposite
      {

         #region TestPrivateComposite Members

         public virtual void TestMethod()
         {
            throw new NotImplementedException();
         }

         #endregion

         public override Boolean Equals( Object obj )
         {
            _privateCompositeEqualsCalled = true;
            return obj is TestPrivateComposite;
         }

         public override Int32 GetHashCode()
         {
            _hashCodeCalled = true;
            return base.GetHashCode();
         }
      }
   }
}
