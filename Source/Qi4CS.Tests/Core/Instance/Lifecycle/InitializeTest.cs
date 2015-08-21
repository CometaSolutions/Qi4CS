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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Lifecycle
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class InitializeTest : AbstractSingletonInstanceTest
   {
      private static Boolean _subMethodCalled = false;
      private static Boolean _subSubMethodCalled = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) );
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite2 ) )
            .WithMixins( typeof( TestCompositeMixinSub ) );
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite3 ) )
            .WithMixins( typeof( TestCompositeMixinSubSub ) );
      }

      [Test]
      public void InitializationTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            Assert.IsTrue( composite.IsInitialized(), "The initialization method of composite mixin must've been invoked." );
         } );
      }

      [Test]
      public void InitializationWithInheritanceTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            _subMethodCalled = false;
            var composite = this.NewPlainComposite<TestComposite2>();
            Assert.IsTrue( composite.IsInitialized(), "The initialization method of composite mixin must've been invoked." );
            Assert.IsTrue( _subMethodCalled, "The initialization of sub-type method must've been called." );
         } );
      }

      [Test]
      public void InitializationWithOverridingTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite3>();
            _subMethodCalled = false;
            _subSubMethodCalled = false;
            Assert.IsTrue( composite.IsInitialized(), "The initialization method of composite mixin must've been invoked." );
            Assert.IsFalse( _subMethodCalled, "The initialization of sub-type method must not have been called." );
            Assert.IsTrue( _subSubMethodCalled, "The initialization of sub-sub-type method must've been called." );
         } );
      }

      public interface TestComposite
      {
         Boolean IsInitialized();
      }

      public class TestCompositeMixin : TestComposite
      {
         private Boolean _initialized;

         public TestCompositeMixin()
         {
            this._initialized = false;
         }

         #region TestComposite Members

         public virtual Boolean IsInitialized()
         {
            return this._initialized;
         }

         #endregion

         [Initialize]
         public void DoInitialize()
         {
            this._initialized = true;
         }

         protected Boolean GetInitialized()
         {
            return this._initialized;
         }
      }

      public class TestCompositeMixinSub : TestCompositeMixin
      {
         private Boolean _isInitialized;

         public TestCompositeMixinSub()
         {
            this._isInitialized = false;
         }

         [Initialize]
         public virtual void DoInitializeSub()
         {
            Assert.IsFalse( this._isInitialized );
            Assert.IsTrue( this.GetInitialized() );
            _subMethodCalled = true;
            this._isInitialized = true;
         }

         protected Boolean GetIsSubInitialized()
         {
            return this._isInitialized;
         }
      }

      public interface TestComposite2 : TestComposite
      {

      }

      public class TestCompositeMixinSubSub : TestCompositeMixinSub
      {
         public override void DoInitializeSub()
         {
            Assert.IsFalse( this.GetIsSubInitialized() );
            _subSubMethodCalled = true;
         }
      }

      public interface TestComposite3 : TestComposite
      {

      }
   }
}
