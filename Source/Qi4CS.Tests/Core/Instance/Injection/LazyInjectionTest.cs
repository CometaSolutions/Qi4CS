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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Tests.Core.Instance.Injection
{
   [Serializable]
   public class LazyInjectionTest : AbstractSingletonInstanceTest
   {
      private const String STR_VAL = "StrVal";
      private static Boolean _mixinMethod1Called;
      private static Boolean _mixinMethod2Called;

      protected override void Assemble( Assembler assembler )
      {
         assembler.NewService().OfTypes( typeof( TestService ) );
         assembler.NewPlainComposite()
            .OfTypes( typeof( TestComposite<> ) )
            .WithMixins( typeof( TestCompositeMixin<> ) ).Done()
            .WithConcerns( typeof( TestCompositeConcern<> ) ).Done()
            .WithSideEffects( typeof( TestCompositeSideEffect<> ) ).Done()
            ;
         assembler.ApplicationArchitecture.AdditionalInjectionFunctionalities[typeof( TestInjectionAttribute )] = new TestInjectionFunctionality();
      }

      public interface TestComposite<T>
      {
         void SomeMethodWithTestInjection( [This] Lazy<TestComposite<T>> meAsLazy );

         void SomeMethodWithoutTestInjection();
      }

      [Test]
      public void TestLazyInjectionsInject()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite<String>>();
            TestInjectionFunctionality.InjectableObject = STR_VAL;
            composite.SomeMethodWithTestInjection( null );
         } );
      }

      [Test]
      public void TestLazyInjectionsThrow()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite<String>>();
            TestInjectionFunctionality.InjectableObject = null;
            composite.SomeMethodWithoutTestInjection();
         } );
      }

      public class TestCompositeMixin<T> : TestComposite<T>
      {
#pragma warning disable 649
         [Service]
         private Lazy<TestService> _lazyService;

         [TestInjection]
         public Lazy<T> _testInjectionPublic;

         [TestInjection]
         private Lazy<T> _testInjectionPrivate;

#pragma warning restore 649

         public TestCompositeMixin( [This] Lazy<TestComposite<T>> meAsLazy )
         {
            Assert.IsNotNull( meAsLazy );
            Assert.IsFalse( meAsLazy.IsValueCreated );
            Assert.IsNotNull( meAsLazy.Value );
         }

         #region TestComposite<T> Members

         public virtual void SomeMethodWithTestInjection( Lazy<TestComposite<T>> meAsLazy )
         {
            Assert.IsNotNull( this._lazyService );
            //Assert.IsFalse( this._lazyService.IsValueCreated );
            Assert.IsNotNull( this._testInjectionPrivate );
            //Assert.IsFalse( this._testInjectionPrivate.IsValueCreated );
            Assert.IsNotNull( this._testInjectionPublic );
            //Assert.IsFalse( this._testInjectionPublic.IsValueCreated );

            Assert.IsNotNull( this._lazyService.Value );
            Assert.IsNotNull( this._testInjectionPrivate.Value );
            Assert.IsNotNull( this._testInjectionPublic.Value );
            _mixinMethod1Called = true;
         }

         public virtual void SomeMethodWithoutTestInjection()
         {
            Assert.IsNotNull( this._lazyService );
            //Assert.IsFalse( this._lazyService.IsValueCreated );
            Assert.IsNotNull( this._testInjectionPrivate );
            //Assert.IsFalse( this._testInjectionPrivate.IsValueCreated );
            Assert.IsNotNull( this._testInjectionPublic );
            //Assert.IsFalse( this._testInjectionPublic.IsValueCreated );

            Assert.IsNotNull( this._lazyService.Value );
            T tmp = default( T );
            Assert.Throws<InjectionException>( () => tmp = this._testInjectionPrivate.Value );
            Assert.Throws<InjectionException>( () => tmp = this._testInjectionPublic.Value );
            _mixinMethod2Called = true;
         }

         #endregion
      }

      public class TestCompositeConcern<T> : ConcernOf<Lazy<TestComposite<T>>>, TestComposite<T>
      {

         public TestCompositeConcern( [ConcernFor] Lazy<TestComposite<T>> nextInCtor )
         {
            _mixinMethod1Called = false;
            _mixinMethod2Called = false;
            Assert.IsNotNull( nextInCtor );
            Assert.IsFalse( nextInCtor.IsValueCreated );
            var oldInjectableObject = TestInjectionFunctionality.InjectableObject;
            TestInjectionFunctionality.InjectableObject = null;
            nextInCtor.Value.SomeMethodWithoutTestInjection();
            Assert.IsFalse( _mixinMethod1Called );
            Assert.IsTrue( _mixinMethod2Called );
            TestInjectionFunctionality.InjectableObject = oldInjectableObject;
         }

         #region TestComposite<T> Members

         public virtual void SomeMethodWithTestInjection( Lazy<TestComposite<T>> meAsLazy )
         {
            _mixinMethod1Called = false;
            _mixinMethod2Called = false;
            Assert.IsNotNull( this.next );
            //Assert.IsFalse( this._next.IsValueCreated );
            this.next.Value.SomeMethodWithTestInjection( meAsLazy );
            Assert.IsTrue( _mixinMethod1Called );
            Assert.IsFalse( _mixinMethod2Called );
         }

         public virtual void SomeMethodWithoutTestInjection()
         {
            _mixinMethod1Called = false;
            _mixinMethod2Called = false;
            Assert.IsNotNull( this.next );
            //Assert.IsFalse( this._next.IsValueCreated );
            this.next.Value.SomeMethodWithoutTestInjection();
            Assert.IsFalse( _mixinMethod1Called );
            Assert.IsTrue( _mixinMethod2Called );
         }

         #endregion
      }

      public class TestCompositeSideEffect<T> : TestComposite<T>
      {
#pragma warning disable 649
         [SideEffectFor]
         private Lazy<TestComposite<T>> _result;
#pragma warning restore 649


         #region TestComposite<T> Members

         public virtual void SomeMethodWithTestInjection( Lazy<TestComposite<T>> meAsLazy )
         {
            Assert.IsTrue( _mixinMethod1Called );
            Assert.IsFalse( _mixinMethod2Called );
            Assert.IsNotNull( this._result );
            //Assert.IsFalse( this._result.IsValueCreated );
            this._result.Value.SomeMethodWithTestInjection( null );
         }

         public virtual void SomeMethodWithoutTestInjection()
         {
            Assert.IsFalse( _mixinMethod1Called );
            Assert.IsTrue( _mixinMethod2Called );
            Assert.IsNotNull( this._result );
            //Assert.IsFalse( this._result.IsValueCreated );
            this._result.Value.SomeMethodWithoutTestInjection();
         }

         #endregion
      }

      public interface TestService
      {
      }

      [InjectionScope]
      [AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false )]
      public class TestInjectionAttribute : Attribute
      {
      }

      public class TestInjectionFunctionality : InjectionFunctionality
      {
         public static Object InjectableObject;

         #region InjectionFunctionality Members

         public ValidationResult InjectionPossible( Qi4CS.Core.SPI.Model.AbstractInjectableModel model )
         {
            return new ValidationResult( true, null );
         }

         public object ProvideInjection( Qi4CS.Core.SPI.Instance.CompositeInstance instance, AbstractInjectableModel model, Type targetType )
         {
            return InjectableObject;
         }

         #endregion

         #region InjectionFunctionality Members

         public InjectionTime GetInjectionTime( Qi4CS.Core.SPI.Model.AbstractInjectableModel model )
         {
            return InjectionTime.ON_METHOD_INVOKATION;
         }

         #endregion
      }
   }
}
