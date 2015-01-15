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
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Injection
{
   [Serializable]
   public class InvocationScopeInjectionTest : AbstractSingletonInstanceTest
   {

      private static Int32Holder<String> _holder;
      private static Int32Holder<String> _holder2;
      private static Boolean _injectionValidationCalled = false;
      private static Int32 _mixinCreationCounter = 0;

      public const Int32 FIRST_VALUE = 456;
      public const Int32 SECOND_VALUE = -1234;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         _injectionValidationCalled = false;
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite<> ) )
            .WithMixins( typeof( TestCompositeMixin<> ) );
      }

      protected override SingletonArchitecture CreateArchitecture()
      {
         var arch = base.CreateArchitecture();
         arch.AdditionalInjectionFunctionalities.Add( typeof( CustomInjectionAttribute ), new CustomInjectionFunctionality() );
         return arch;
      }

      [Test]
      public void PerformCustomInvocationInjectionTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            Assert.IsTrue( _injectionValidationCalled );
            var counter = _mixinCreationCounter;
            var composite = this.NewPlainComposite<TestComposite<String>>();
            _holder = new Int32Holder<string>();
            _holder.Value = FIRST_VALUE;
            composite.PerformCheck( FIRST_VALUE );
            _holder = null;
            _holder2 = new Int32Holder<string>();
            _holder2.Value = SECOND_VALUE;
            composite.PerformCheck( SECOND_VALUE );
            _holder2 = null;
            Assert.AreEqual( counter + 1, _mixinCreationCounter );
         } );
      }

      public interface TestComposite<T>
      {
         void PerformCheck( Int32 value );
      }

      public class TestCompositeMixin<T> : TestComposite<T>
      {
#pragma warning disable 649

         [CustomInjection]
         private Int32Holder<T> _holder;

#pragma warning restore 649

         public TestCompositeMixin()
         {
            ++_mixinCreationCounter;
         }

         #region TestComposite<T> Members

         public virtual void PerformCheck( Int32 value )
         {
            Assert.AreEqual( value, this._holder.Value );
         }

         #endregion
      }

      public class Int32Holder<T>
      {
         private Int32 _value;
         public Int32 Value
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

      [InjectionScope]
      public class CustomInjectionAttribute : Attribute
      {

      }

      public class CustomInjectionFunctionality : InjectionFunctionality
      {

         #region InjectionFunctionality Members

         public ValidationResult InjectionPossible( AbstractInjectableModel model )
         {
            _injectionValidationCalled = true;
            return new ValidationResult( typeof( Int32Holder<> ).IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( model.TargetType ), null );
         }

         public Object ProvideInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType )
         {
            return _holder == null ? _holder2 : _holder;
         }

         public InjectionTime GetInjectionTime( AbstractInjectableModel model )
         {
            return InjectionTime.ON_METHOD_INVOKATION;
         }

         #endregion
      }
   }
}
