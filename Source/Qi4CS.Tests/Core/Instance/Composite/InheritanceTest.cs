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
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   public class InheritanceTest : AbstractSingletonInstanceTest
   {

      private static readonly String STRING_PARAM = "StringParam";
      private static readonly Int32 INT32_PARAM = 8343;
      private static Boolean _composite1MixinCalled = false;
      private static Boolean _baseMixinCalled = false;
      private static Type _baseMixinType = null;
      private static Boolean _composite3MixinCalled1 = false;
      private static Boolean _composite3MixinCalled2 = false;
      private static Boolean _composite5MixinCalled = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes(
            typeof( Composite1 )
            ).WithMixins(
            typeof( Composite1Mixin )
            );
         assembler.NewPlainComposite().OfTypes(
            typeof( Composite2 )
            ).WithMixins(
            typeof( BaseMixin<String> )
            );
         assembler.NewPlainComposite().OfTypes(
            typeof( Composite3 )
            ).WithMixins(
            typeof( Composite3Mixin )
            );
         assembler.NewPlainComposite().OfTypes(
            typeof( Composite4 )
            ).WithMixins(
            typeof( BaseMixin<String> ), typeof( BaseMixin<Int32> )
            );
         assembler.NewPlainComposite().OfTypes(
            typeof( Composite5<> )
            ).WithMixins(
            typeof( Composite5Mixin<> )
            );
         assembler.NewPlainComposite().OfTypes(
            typeof( Composite6<> )
            ).WithMixins(
            typeof( BaseMixin<> )
            );
      }

      [Test]
      public void BaseInterfaceWithClosedGenericParametersMixinImplementsCompositeTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<Composite1>();
            _composite1MixinCalled = false;
            composite.BaseMethod( STRING_PARAM );
            Assert.IsTrue( _composite1MixinCalled, "The mixin of the composite must've been called." );
         } );
      }

      [Test]
      public void BaseInterfaceWithClosedGenericParametersMixinImplementsBaseInterfaceTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<Composite2>();
            _baseMixinCalled = false;
            _baseMixinType = null;
            composite.BaseMethod( STRING_PARAM );
            Assert.IsTrue( _baseMixinCalled, "The mixin of the composite must've been called." );
            Assert.AreEqual( typeof( String ), _baseMixinType, "Mixin generic type must've been correct." );
         } );
      }

      [Test]
      public void BaseInterfaceWithClosedGenericParametersMultipleTimesMixinImplementsCompositeTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<Composite3>();
            _composite3MixinCalled1 = false;
            _composite3MixinCalled2 = false;
            composite.BaseMethod( STRING_PARAM );
            Assert.IsTrue( _composite3MixinCalled1, "The first mixin must've been called." );
            Assert.IsFalse( _composite3MixinCalled2, "The second mixin must not have been called." );
            _composite3MixinCalled1 = false;
            _composite3MixinCalled2 = false;
            composite.BaseMethod( INT32_PARAM );
            Assert.IsFalse( _composite3MixinCalled1, "The first mixin must've been called." );
            Assert.IsTrue( _composite3MixinCalled2, "The second mixin must not have been called." );
         } );
      }

      [Test]
      public void BaseInterfaceWithClosedGenericParametersMultipleTimesMixinImplementsBaseInterfaceTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<Composite4>();
            _baseMixinCalled = false;
            _baseMixinType = null;
            composite.BaseMethod( STRING_PARAM );
            Assert.IsTrue( _baseMixinCalled, "The mixin of the composite must've been called." );
            Assert.AreEqual( typeof( String ), _baseMixinType, "Mixin generic type must've been correct." );
            _baseMixinCalled = false;
            _baseMixinType = null;
            composite.BaseMethod( INT32_PARAM );
            Assert.IsTrue( _baseMixinCalled, "The mixin of the composite must've been called." );
            Assert.AreEqual( typeof( Int32 ), _baseMixinType, "Mixin generic type must've been correct." );
         } );
      }

      [Test]
      public void BaseInterfaceWithOpenGenericParametersMixinImplementsCompositeTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<Composite5<String>>();
            _composite5MixinCalled = false;
            _baseMixinType = null;
            composite.BaseMethod( STRING_PARAM );
            Assert.IsTrue( _composite5MixinCalled, "The mixin of the composite must've been called." );
            Assert.AreEqual( typeof( String ), _baseMixinType, "Mixin generic type must've been correct." );
         } );
      }

      public interface BaseInterface<T>
      {
         void BaseMethod( T param );
      }

      public class BaseMixin<T> : BaseInterface<T>
      {
         public virtual void BaseMethod( T param )
         {
            if ( param is String )
            {
               Assert.AreEqual( STRING_PARAM, param, "Parameter visible in mixin must be the same given to composite" );
            }
            _baseMixinCalled = true;
            _baseMixinType = typeof( T );
         }
      }

      // Test scenario 1:
      // non-generic composite inherits generic interface with closed generic parameter(s)
      // Mixin implements the composite interface
      public interface Composite1 : BaseInterface<String>
      {

      }

      public class Composite1Mixin : Composite1
      {

         public virtual void BaseMethod( String param )
         {
            Assert.AreEqual( STRING_PARAM, param, "Parameter visible in mixin must be the same given to composite" );
            _composite1MixinCalled = true;
         }
      }

      // Test scenario 2:
      // non-generic composite inherits generic interface with closed generic parameter(s)
      // Mixin implements the generic interface
      public interface Composite2 : BaseInterface<String>
      {

      }

      // Test scenario 3:
      // Non-generic composite inherits generic interface with closed generic parameter(s) twice or more
      // Mixin implements composite
      public interface Composite3 : BaseInterface<String>, BaseInterface<Int32>
      {

      }

      public class Composite3Mixin : Composite3
      {

         public virtual void BaseMethod( String param )
         {
            Assert.AreEqual( STRING_PARAM, param );
            _composite3MixinCalled1 = true;
         }

         public virtual void BaseMethod( Int32 param )
         {
            Assert.AreEqual( INT32_PARAM, param );
            _composite3MixinCalled2 = true;
         }
      }

      // Test scenario 4
      // Non-generic composite inherits generic interface with closed generic parameter(s) twice or more
      // Mixin implements base interface
      public interface Composite4 : BaseInterface<String>, BaseInterface<Int32>
      {

      }

      // Test scenario 5
      // Generic composite inherits generic interface with open generic parameter(s)
      // Mixin implements composite
      public interface Composite5<T> : BaseInterface<T>
      {

      }

      public class Composite5Mixin<T> : Composite5<T>
      {
         public virtual void BaseMethod( T param )
         {
            if ( param is String )
            {
               Assert.AreEqual( STRING_PARAM, param, "Parameter visible in mixin must be the same given to composite" );
            }
            _composite5MixinCalled = true;
            _baseMixinType = typeof( T );
         }
      }

      // Test scenario 6
      // Generic composite inherits generic interface with open generic parameter(s)
      // Mixin implements base interface
      public interface Composite6<T> : BaseInterface<T>
      {

      }
   }
}
