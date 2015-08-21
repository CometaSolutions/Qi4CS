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

namespace Qi4CS.Tests.Core.Instance.Objects
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class ComplexObjectTest : AbstractSingletonInstanceTest
   {
      private static Boolean _ifaceMethodCalled;
      private static Boolean _ifaceMethod2Called;
      private static Boolean _objectMethodCalled;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( RealObjectComposite ) )
            .WithMixins( typeof( IFaceImpl ), typeof( IFaceImpl2 ) );
      }

      [Test]
      public void TestComplexObject()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<ObjectComposite>();
            _ifaceMethodCalled = false;
            _ifaceMethod2Called = false;
            _objectMethodCalled = false;
            composite.ObjectMethod();
            Assert.IsTrue( _ifaceMethodCalled, "Interface method must've been called." );
            Assert.IsTrue( _ifaceMethod2Called, "Second interface method must've been called." );
            Assert.IsTrue( _objectMethodCalled, "Object method must've been called." );
         } );
      }


      public interface IFace
      {
         void IFaceMethod();

         void IFaceMethod2();
      }

      public abstract class IFaceImpl : IFace
      {
         #region IFace Members

         public virtual void IFaceMethod()
         {
            _ifaceMethodCalled = true;
            this.IFaceMethod2();
         }

         public abstract void IFaceMethod2();

         #endregion
      }

      public abstract class IFaceImpl2 : IFace
      {

         #region IFace Members

         public abstract void IFaceMethod();

         public virtual void IFaceMethod2()
         {
            _ifaceMethod2Called = true;
         }

         #endregion
      }

      public abstract class ObjectComposite : IFace
      {
#pragma warning disable 649

         [This]
         private IFace _meAsIFace;

#pragma warning restore 649

         #region IFace Members

         public abstract void IFaceMethod();

         public abstract void IFaceMethod2();

         #endregion

         public virtual void ObjectMethod()
         {
            this._meAsIFace.IFaceMethod();
            _objectMethodCalled = true;
            this.DoSomething();
         }

         protected abstract void DoSomething();

         [Prototype]
         public virtual void DummyMethod()
         {

         }
      }

      public abstract class RealObjectComposite : ObjectComposite
      {
         protected override void DoSomething()
         {
            // Do nothing.
         }
      }
   }
}
