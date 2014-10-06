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
using NUnit.Framework;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Tests.Core.Instance.Services
{
   [Category( "INSTANCE.SERVICES" )]
   [Serializable]
   public class QualifierTest : AbstractSingletonInstanceTest
   {
      private const String SPECIFIC_SERVICE_ID = "SpecificTestServiceID";
      private const String ACTIVE_SERVICE_ID = "ActiveTestServiceID";
      private const String PASSIVE_SERVICE_ID = "PassiveTestServiceID";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewService()
            .WithServiceID( SPECIFIC_SERVICE_ID )
            .SetActivateWithApplication( false )
            .OfTypes( typeof( TestService ) )
            .WithMixins( typeof( TestServiceMixin ) );
         assembler.NewService()
            .WithServiceID( ACTIVE_SERVICE_ID )
            .SetActivateWithApplication( true )
            .OfTypes( typeof( TestService ) )
            .WithMixins( typeof( TestServiceMixin ) );
         assembler.NewService()
            .WithServiceID( PASSIVE_SERVICE_ID )
            .SetActivateWithApplication( false )
            .OfTypes( typeof( TestService ) )
            .WithMixins( typeof( TestServiceMixin ) );

         assembler.NewPlainComposite()
            .OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) );
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var c = this.NewPlainComposite<TestComposite>();
            Assert.AreEqual( SPECIFIC_SERVICE_ID, c.IDMarkedServiceID );
            Assert.AreEqual( ACTIVE_SERVICE_ID, c.ActiveMarkedServiceID );

            // Access specific ID service so it would activate
            var dummy = this.StructureServices.FindServices<TestService>().First( sRef => String.Equals( sRef.ServiceID, SPECIFIC_SERVICE_ID ) ).GetService();
            var dummyID = ( (Identity) dummy ).Identity;


            Assert.AreEqual( PASSIVE_SERVICE_ID, c.NonActiveMarkedServiceID );
         } );
      }

      public interface TestService
      {
         //String ServiceID { get; }
      }

      public interface TestComposite
      {
         String IDMarkedServiceID { get; }
         String ActiveMarkedServiceID { get; }
         String NonActiveMarkedServiceID { get; }
      }

      public class TestServiceMixin : TestService
      {
         //#pragma warning disable 649

         //         [Structure]
         //         private ServiceCompositeModel _myModel;

         //#pragma warning restore 649

         //         public virtual String ServiceID
         //         {
         //            get
         //            {
         //               return this._myModel.ServiceID;
         //            }
         //         }
      }

      public class TestCompositeMixin : TestComposite
      {
#pragma warning disable 649
         [Service, IdentifiedBy( SPECIFIC_SERVICE_ID )]
         private ServiceReferenceInfo<TestService> _serviceWithID;

         [Service, Active( true )]
         private ServiceReferenceInfo<TestService> _activeService;

         [Service, Active( false )]
         private ServiceReferenceInfo<TestService> _passiveService;
#pragma warning restore 649

         public virtual String IDMarkedServiceID
         {
            get
            {
               return this._serviceWithID.ServiceID;
            }
         }

         public virtual String ActiveMarkedServiceID
         {
            get
            {
               return this._activeService.ServiceID;
            }
         }

         public virtual String NonActiveMarkedServiceID
         {
            get
            {
               return this._passiveService.ServiceID;
            }
         }
      }
   }
}
