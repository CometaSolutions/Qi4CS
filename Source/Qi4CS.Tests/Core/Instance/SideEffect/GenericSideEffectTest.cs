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
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.SideEffect
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class GenericSideEffectTest : AbstractSingletonInstanceTest
   {

      private const String MIXIN_RESULT = "MixinResult";

      private static Boolean _mixinCalled;
      private static Boolean _sideEffect1Called;
      private static Boolean _sideEffect2Called;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) ).Done()
            .WithSideEffects( typeof( TestGenericSideEffect1 ), typeof( TestGenericSideEffect2 ) );
      }

      [Test]
      public void GenericSideEffectCallingTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            _mixinCalled = false;
            _sideEffect1Called = false;
            _sideEffect2Called = false;
            var result = composite.A();
            Assert.IsTrue( _mixinCalled, "Mixin must've been called." );
            Assert.IsTrue( _sideEffect1Called, "First side effect must've been called" );
            Assert.IsTrue( _sideEffect2Called, "Second side-effect must've been called" );
            Assert.AreEqual( MIXIN_RESULT, result, "The result must be " + MIXIN_RESULT + "." );
         } );
      }

      public interface TestComposite
      {
         String A();
      }

      public class TestCompositeMixin : TestComposite
      {

         #region TestComposite Members

         public virtual String A()
         {
            _mixinCalled = true;
            return MIXIN_RESULT;
         }

         #endregion
      }

      public class TestGenericSideEffect1 : GenericSideEffect
      {

         protected override void DoInvoke( Object composite, MethodInfo method, Object[] args )
         {
            _sideEffect1Called = true;
            Assert.IsEmpty( new List<Object>( args ), "The arguments must be empty." );
            Assert.IsInstanceOf<TestComposite>( composite, "The composite instance must be of composite type." );
            Assert.AreEqual( typeof( TestComposite ), method.DeclaringType, "The declaring type of the method must be of composite's type." );
            Object result = this.result.Invoke( composite, method, args );
            Assert.AreEqual( MIXIN_RESULT, result, "The result must be " + MIXIN_RESULT + "." );
            throw new Exception();
         }
      }

      public class TestGenericSideEffect2 : GenericSideEffect
      {

         protected override void DoInvoke( Object composite, MethodInfo method, Object[] args )
         {
            _sideEffect2Called = true;
            Assert.IsEmpty( new List<Object>( args ), "The arguments must be empty." );
            Assert.IsInstanceOf<TestComposite>( composite, "The composite instance must be of composite type." );
            Assert.AreEqual( typeof( TestComposite ), method.DeclaringType, "The declaring type of the method must be of composite's type." );
            Object result = this.result.Invoke( composite, method, args );
            Assert.AreEqual( MIXIN_RESULT, result, "The result must be " + MIXIN_RESULT + "." );
         }
      }
   }
}
