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
using NUnit.Framework;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using System;

namespace Qi4CS.Tests.Core.Instance
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class MultipleApplicationActivationsTest : AbstractSingletonInstanceTest
   {
      public interface TestComposite
      { }

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite ) );
      }

      [Test]
      public void CreateAndActivateApplicationFromWithin()
      {
         this.PerformTestInAppDomain( () =>
         {
            using ( var ad = Qi4CSTestUtils.CreateTestAppDomain( "Multiple activations test" ) )
            {
               var application = this.Model.NewInstance( null, null, null );
               try
               {
                  application.Activate();
               }
               finally
               {
                  application.Passivate();
               }
            }
         } );
      }
   }
}
