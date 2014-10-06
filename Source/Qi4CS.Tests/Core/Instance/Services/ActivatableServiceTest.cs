/*
 * Copyright (c) 2008, Rickard Öberg.
 * See NOTICE file.
 * 
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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Services
{
   [Serializable]
   public class ActivatableServiceTest : AbstractSingletonInstanceTest
   {
      private static Boolean _isActive1;
      private static Boolean _isActive2;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewService().OfTypes( typeof( ServiceComposite1 ) )
            .WithMixins( typeof( ServiceComposite1Mixin ) );

         assembler
            .NewService()
            .SetActivateWithApplication( true )
            .OfTypes( typeof( ServiceComposite2 ) )
            .WithMixins( typeof( ServiceComposite2Mixin ) );

         assembler
            .NewService().OfTypes( typeof( ServiceComposite3 ) )
            .WithMixins( typeof( ServiceComposite3Mixin ), typeof( ServiceComposite3Lifecycle ) );


         _isActive1 = false;
         _isActive2 = false;
      }

      [Test]
      public void TestServiceActivationThroughMethods()
      {
         this.PerformTestInAppDomain( () =>
         {
            this.AssertNotActive( _isActive1 );
            var reference = this.StructureServices.FindService<ServiceComposite1>();
            this.AssertNotActive( _isActive1 );

            Assert.IsFalse( reference.Active, "The service must not be active according to the service reference." );

            var service = reference.GetService();
            this.AssertNotActive( _isActive1 );

            service.DummyMethod();
            this.AssertActive( _isActive1 );

            this.Application.Passivate();
            this.AssertNotActive( _isActive1 );
         } );
      }

      [Test]
      public void TestServiceActivationThroughApplicationActivation()
      {
         this.PerformTestInAppDomain( () =>
         {
            this.AssertActive( _isActive2 );
            var reference = this.StructureServices.FindService<ServiceComposite2>();
            this.AssertActive( _isActive2 );

            Assert.IsTrue( reference.Active, "The service must be active according to the service reference." );

            var service = reference.GetService();
            this.AssertActive( _isActive2 );

            this.AssertActive( _isActive2 );

            this.Application.Passivate();
            this.AssertNotActive( _isActive2 );
         } );
      }

      [Test]
      public void TestComplexServiceCompositeActivation()
      {
         this.PerformTestInAppDomain( () =>
         {
            var service = this.FindService<ServiceComposite3>();
            Assert.AreEqual( "Hello, World", service.SayHello(), "The service mixin must've been initialized properly" );
         } );
      }

      protected void AssertNotActive( Boolean active )
      {
         Assert.IsFalse( active, "The service must not be active" );
      }

      protected void AssertActive( Boolean active )
      {
         Assert.IsTrue( active, "The service must be active" );
      }

      public interface ServiceComposite1
      {
         void DummyMethod();
      }

      public class ServiceComposite1Mixin : ServiceComposite1
      {

         #region ServiceComposite1 Members

         public virtual void DummyMethod()
         {
            // Do nothing
         }

         #endregion

         [Activate]
         public void Activate()
         {
            Assert.IsFalse( _isActive1 );
            _isActive1 = true;
         }

         [Passivate]
         public void Passivate()
         {
            Assert.IsTrue( _isActive1 );
            _isActive1 = false;
         }
      }

      public interface ServiceComposite2
      {
         void DummyMethod();
      }

      public class ServiceComposite2Mixin : ServiceComposite2
      {

         #region ServiceComposite2 Members

         public virtual void DummyMethod()
         {
            // Nothing to do
         }

         #endregion

         [Activate]
         public void Activate()
         {
            Assert.IsFalse( _isActive2 );
            _isActive2 = true;
         }

         [Passivate]
         public void Passivate()
         {
            Assert.IsTrue( _isActive2 );
            _isActive2 = false;
         }
      }

      public interface ServiceComposite3
      {
         String SayHello();

         [Optional]
         String Greeting { get; set; }

         [Optional]
         String Recepient { get; set; }
      }

      public abstract class ServiceComposite3Mixin : ServiceComposite3
      {

         #region ServiceComposite3 Members

         public virtual String SayHello()
         {
            return this.Greeting + ", " + this.Recepient;
         }

         #endregion

         #region ServiceComposite3 Members


         public abstract String Greeting
         {
            get;
            set;
         }

         public abstract String Recepient
         {
            get;
            set;
         }

         #endregion
      }

      public class ServiceComposite3Lifecycle
      {
#pragma warning disable 649

         [This]
         private ServiceComposite3 _me;

#pragma warning restore 649

         [Activate]
         public void Activate()
         {
            this._me.Greeting = "Hello";
         }

         [Initialize]
         public void Initialize()
         {
            this._me.Recepient = "World";
         }
      }
   }
}
