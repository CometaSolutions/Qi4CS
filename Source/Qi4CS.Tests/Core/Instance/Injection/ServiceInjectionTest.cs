using NUnit.Framework;
/*
 * Copyright (c) 2007, Rickard Öberg.
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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using System;

namespace Qi4CS.Tests.Core.Instance.Injection
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class ServiceInjectionTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewService().OfTypes( typeof( ServiceComposite ) );
         assembler
            .NewPlainComposite().OfTypes( typeof( ServiceUser ) )
            .WithMixins( typeof( ServiceUserMixin ) );
      }

      [Test]
      public void TestServiceInjection()
      {
         this.PerformTestInAppDomain( () =>
         {
            ServiceUser composite = this.NewPlainComposite<ServiceUser>();
            composite.UseService();
         } );
      }

      [Test]
      public void TestServicesAreSingletons()
      {
         this.PerformTestInAppDomain( () =>
         {
            ServiceUser composite1 = this.NewPlainComposite<ServiceUser>();
            ServiceComposite service1 = composite1.GetService();

            ServiceUser composite2 = this.NewPlainComposite<ServiceUser>();
            ServiceComposite service2 = composite2.GetService();

            Assert.AreEqual( service1, service2, "There must be only one service composite created." );
         } );
      }

      public interface ServiceComposite
      {

      }

      public interface ServiceUser
      {
         void UseService();

         ServiceComposite GetService();
      }

      public class ServiceUserMixin : ServiceUser
      {
#pragma warning disable 649
         [Service]
         private ServiceComposite _service;
#pragma warning restore 649

         #region ServiceUser Members

         public virtual void UseService()
         {
            Assert.NotNull( this._service, "The service must be injected." );
         }

         public virtual ServiceComposite GetService()
         {
            return this._service;
         }

         #endregion
      }
   }
}
