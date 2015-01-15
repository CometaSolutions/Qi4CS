/*
 * Copyright (c) 2009, Niclas Hedhman.
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
using System.Collections.Generic;
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Services
{
   [Serializable]
   public class PassivatableTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewService().OfTypes( typeof( SuccessDataService ) )
            .WithMixins( typeof( SuccessDataServiceMixin ) );
         assembler
            .NewService().OfTypes( typeof( FailingDataService ) )
            .WithMixins( typeof( FailingDataServiceMixin ) );
      }

      [Test]
      public void GivenSuccessPassivationWhenPassivatingExpectNoExceptions()
      {
         this.PerformTestInAppDomain( () =>
         {
            var sRef = this.FindServiceReference<SuccessDataService>();
            Assert.IsFalse( sRef.Active, "Service should not be Active before accessed" );
            Assert.IsTrue( sRef.GetService().Data.activated, "Service should be activated during access." );
            Assert.IsTrue( sRef.Active, "Service should be Active after access." );

            this.Application.Passivate();
         } );
      }

      [Test]
      public void GivenMixedSuccessFailurePassivationWhenPassivatingExpectAllPassivateMethodsToBeCalled()
      {
         this.PerformTestInAppDomain( () =>
         {
            var refs = this.StructureServices.FindServices<DataAccess>();

            var datas = new List<Data>();
            foreach ( var sRef in refs )
            {
               Assert.IsFalse( sRef.Active, "Service should not be Active before accessed" );
               var data = sRef.GetService().Data;
               if ( sRef.GetService() is SuccessDataService )
               {
                  datas.Add( data );
               }
               Assert.IsTrue( data.activated, "Service should be activated during access." );
               Assert.IsTrue( sRef.Active, "Service should be Active after access." );
            }

            Assert.Throws<ApplicationPassivationException>( () => this.Application.Passivate(), "The passivate method should've thrown exception." );

            // Still ensure that all services has been shutdown.
            foreach ( var data in datas )
            {
               Assert.IsFalse( data.activated, "Service should've been passivated with application." );
            }
         } );
      }

      public class Data
      {
         public Boolean activated = false;
      }

      public interface DataAccess
      {
         Data Data { get; }
      }

      public interface FailingDataService : DataAccess
      {
      }

      public interface SuccessDataService : DataAccess
      {
      }

      public class FailingDataServiceMixin : FailingDataService
      {

         private readonly Data _data = new Data();

         #region DataAccess Members

         public virtual Data Data
         {
            get
            {
               return this._data;
            }
         }

         #endregion

         [Activate]
         public void Activate()
         {
            this._data.activated = true;
         }

         [Passivate]
         public void Passivate()
         {
            throw new InvalidOperationException();
         }
      }

      public class SuccessDataServiceMixin : SuccessDataService
      {
         private readonly Data _data = new Data();

         #region DataAccess Members

         public virtual Data Data
         {
            get
            {
               return this._data;
            }
         }

         #endregion

         [Activate]
         public void Activate()
         {
            this._data.activated = true;
         }

         [Passivate]
         public void Passivate()
         {
            this._data.activated = false;
         }
      }

   }
}
