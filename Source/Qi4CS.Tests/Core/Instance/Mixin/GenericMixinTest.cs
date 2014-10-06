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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Mixin
{
   [Serializable]
   public class GenericMixinTest : AbstractSingletonInstanceTest
   {
      private static Boolean _genericMixinCalled;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestMixin ) );
      }

      [Test]
      public void GenericMixinGetsCalledTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            _genericMixinCalled = false;
            composite.A();
            Assert.IsTrue( _genericMixinCalled, "The generic mixin must've been called." );
         } );
      }

      [Test]
      public void GenericMixinGetsCalledTypeConversionTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            _genericMixinCalled = false;
            Assert.AreEqual( composite.B(), default( Int32 ) );
            Assert.IsTrue( _genericMixinCalled, "The generic mixin must've been called." );
         } );
      }

      public interface TestComposite
      {
         void A();

         Int32 B();
      }

      public class TestMixin : GenericInvocator
      {

         #region GenericInvocator Members

         public virtual Object Invoke( Object composite, MethodInfo method, Object[] args )
         {
            _genericMixinCalled = true;
            Assert.IsEmpty( new List<Object>( args ), "The arguments must be empty." );
            Assert.IsInstanceOf<TestComposite>( composite, "The composite instance must be of composite type." );
            Assert.AreEqual( typeof( TestComposite ), method.DeclaringType, "The declaring type of the method must be of composite's type." );
            return null;
         }

         #endregion
      }
   }
}
