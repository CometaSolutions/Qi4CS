using NUnit.Framework;
using Qi4CS.Core.API.Model;
/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   public class NonPublicCompositeMethodsTest : AbstractSingletonInstanceTest
   {
      private static Boolean _methodCalled;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite()
            .OfTypes( typeof( TestComposite ) );
      }

      [Test]
      public void TestCompositeWithNonPublicMethods()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();

            _methodCalled = false;
            composite.PublicMethod();
            Assert.IsTrue( _methodCalled );

            _methodCalled = false;
            composite.InternalMethod();
            Assert.IsTrue( _methodCalled );

            _methodCalled = false;
            composite.CallProtectedMethod();
            Assert.IsTrue( _methodCalled );

            _methodCalled = false;
            composite.ProtectedOrInternalMethod();
            Assert.IsTrue( _methodCalled );

         } );
      }

      public class TestComposite
      {
#pragma warning disable 649
         [This]
         private TestComposite _composite;
#pragma warning restore 649


         public virtual void PublicMethod()
         {
            Assert.IsNotNull( this._composite );
            _methodCalled = true;
         }

         [CompositeMethod]
         internal virtual void InternalMethod()
         {
            Assert.IsNotNull( this._composite );
            _methodCalled = true;
         }

         [CompositeMethod]
         protected virtual void ProtectedMethod()
         {
            Assert.IsNotNull( this._composite );
            _methodCalled = true;
         }

         [CompositeMethod]
         protected virtual internal void ProtectedOrInternalMethod()
         {
            Assert.IsNotNull( this._composite );
            _methodCalled = true;
         }

         public virtual void CallProtectedMethod()
         {
            this.ProtectedMethod();
         }
      }
   }

   [Serializable]
   public class DefaultCompositeMethodVisiblityTest : AbstractSingletonInstanceTest
   {
      private static Boolean _methodCalled;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite()
            .OfTypes( typeof( TestComposite ) );
      }

      [Test]
      public void TestCompositeWithDefaultCompositeMethodVisiblityAttribute()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();

            _methodCalled = false;
            composite.PublicMethod();
            Assert.IsTrue( _methodCalled );

            _methodCalled = false;
            composite.InternalMethod();
            Assert.IsTrue( _methodCalled );

            _methodCalled = false;
            composite.CallProtectedMethod();
            Assert.IsTrue( _methodCalled );

            _methodCalled = false;
            composite.ProtectedOrInternalMethod();
            Assert.IsTrue( _methodCalled );

         } );
      }

      [DefaultCompositeMethodVisibility( CompositeMethodVisiblity.Public | CompositeMethodVisiblity.Internal | CompositeMethodVisiblity.Protected | CompositeMethodVisiblity.ProtectedOrInternal )]
      public class TestComposite
      {
#pragma warning disable 649
         [This]
         private TestComposite _composite;
#pragma warning restore 649


         public virtual void PublicMethod()
         {
            Assert.IsNotNull( this._composite );
            _methodCalled = true;
         }

         internal virtual void InternalMethod()
         {
            Assert.IsNotNull( this._composite );
            _methodCalled = true;
         }

         protected virtual void ProtectedMethod()
         {
            Assert.IsNotNull( this._composite );
            _methodCalled = true;
         }

         protected virtual internal void ProtectedOrInternalMethod()
         {
            Assert.IsNotNull( this._composite );
            _methodCalled = true;
         }

         public virtual void CallProtectedMethod()
         {
            this.ProtectedMethod();
         }
      }
   }
}
