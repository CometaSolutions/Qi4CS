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

namespace Qi4CS.Tests.Core.Instance.Concern
{
   [Serializable]
   public class GenericConcernTest : AbstractSingletonInstanceTest
   {

      private const String MIXIN_RESULT = "MixinResult";
      private const String CONCERN_RESULT_1 = "ConcernResult1";
      private const String CONCERN_RESULT_2 = "ConcernResult2";
      private const String STRING_ARG = "Arg";

      private static Boolean _mixinCalled = false;
      private static Boolean _concern1Called = false;
      private static Boolean _concern2Called = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) ).Done()
            .WithConcerns( typeof( TestGenericConcern1 ), typeof( TestGenericConcern2 ) );
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite2 ) )
            .WithMixins( typeof( TestComposite2Mixin ) ).Done()
            .WithConcerns( typeof( TestComposite2Concern ) );
      }

      [Test]
      public void GenericConcernsGetCalledTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            _mixinCalled = false;
            _concern1Called = false;
            _concern2Called = false;
            var result = composite.A();
            Assert.IsTrue( _mixinCalled, "The mixin must have been called." );
            Assert.IsTrue( _concern1Called, "The first generic concern must have been called." );
            Assert.IsTrue( _concern2Called, "The second generic concern must have been called." );
            Assert.AreEqual( CONCERN_RESULT_1, result, "The result must come from first concern." );
         } );
      }

      [Test]
      public void GenericConcernGetsCalledWithMethodWithArgumentTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite2>();
            _mixinCalled = false;
            _concern1Called = false;
            composite.B( STRING_ARG );
            Assert.IsTrue( _mixinCalled, "The mixin must have been called." );
            Assert.IsTrue( _concern1Called, "The concern must have been called." );
         } );
      }

      public interface TestComposite
      {
         String A();
      }

      public interface TestComposite2
      {
         void B( String arg );
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

      public class TestGenericConcern1 : GenericConcern
      {
         public override Object Invoke( Object composite, MethodInfo method, Object[] args )
         {
            _concern1Called = true;
            Assert.IsEmpty( new List<Object>( args ), "The arguments must be empty." );
            Assert.IsInstanceOf<TestComposite>( composite, "The composite instance must be of composite type." );
            Assert.AreEqual( typeof( TestComposite ), method.DeclaringType, "The declaring type of the method must be of composite's type." );
            Object result = this.next.Invoke( composite, method, args );
            Assert.AreEqual( CONCERN_RESULT_2, result, "The result must be " + CONCERN_RESULT_2 + "." );
            return CONCERN_RESULT_1;
         }
      }

      public class TestGenericConcern2 : GenericConcern
      {
         public override Object Invoke( Object composite, MethodInfo method, Object[] args )
         {
            _concern2Called = true;
            Assert.IsEmpty( new List<Object>( args ), "The arguments must be empty." );
            Assert.IsInstanceOf<TestComposite>( composite, "The composite instance must be of composite type." );
            Assert.AreEqual( typeof( TestComposite ), method.DeclaringType, "The declaring type of the method must be of composite's type." );
            Object result = this.next.Invoke( composite, method, args );
            Assert.AreEqual( MIXIN_RESULT, result, "The result must be " + MIXIN_RESULT + "." );
            return CONCERN_RESULT_2;
         }
      }

      public class TestComposite2Mixin : TestComposite2
      {

         #region TestComposite2 Members

         public virtual void B( String arg )
         {
            _mixinCalled = true;
            Assert.AreEqual( STRING_ARG, arg );
         }

         #endregion
      }

      public class TestComposite2Concern : GenericConcern
      {

         public override Object Invoke( Object composite, MethodInfo method, Object[] args )
         {
            _concern1Called = true;
            return this.next.Invoke( composite, method, args );
         }
      }

   }
}
