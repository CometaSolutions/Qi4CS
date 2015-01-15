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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using System;
using NUnit.Framework;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Common;
namespace Qi4CS.Tests.Core.Instance.Injection
{
   [Serializable]
   public class StructureInjectionTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite ) ).WithMixins( typeof( TestCompositeMixin ) );
      }

      [Test]
      public void TestStructureInjection()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();

            Assert.IsNotNull( composite.ServiceProvider, "Structure service provider injection must work." );
            Assert.IsNotNull( composite.ServiceProviderSPI, "Structure service provider SPI injection must work." );
            Assert.IsNotNull( composite.Application, "Application injection must work." );
            Assert.IsNotNull( composite.ApplicationSPI, "Application SPI injection must work." );
            Assert.IsNotNull( composite.Uses, "Uses container injection must work." );
         } );
      }

      public interface TestComposite
      {
         StructureServiceProvider ServiceProvider { get; }

         StructureServiceProviderSPI ServiceProviderSPI { get; }

         Application Application { get; }

         ApplicationSPI ApplicationSPI { get; }

         UsesProviderQuery Uses { get; }
      }

      public class TestCompositeMixin : TestComposite
      {
#pragma warning disable 649
         [Structure]
         private StructureServiceProvider _provider;

         [Structure]
         private StructureServiceProviderSPI _providerSPI;

         [Structure]
         private Application _application;

         [Structure]
         private ApplicationSPI _applicationSPI;

         [Structure]
         private UsesProviderQuery _uses;

#pragma warning restore 649

         #region TestComposite Members

         public virtual StructureServiceProvider ServiceProvider
         {
            get
            {
               return this._provider;
            }
         }

         public virtual StructureServiceProviderSPI ServiceProviderSPI
         {
            get
            {
               return this._providerSPI;
            }
         }

         public virtual Application Application
         {
            get
            {
               return this._application;
            }
         }

         public virtual ApplicationSPI ApplicationSPI
         {
            get
            {
               return this._applicationSPI;
            }
         }

         public virtual UsesProviderQuery Uses
         {
            get
            {
               return this._uses;
            }
         }

         #endregion
      }
   }
}
