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

namespace Qi4CS.Tests.Core.Instance.Injection
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class ParameterInjectionTest : AbstractSingletonInstanceTest
   {

      private static Boolean _mixinInvoked = false;
      private static Boolean _concernInvoked = false;


      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) ).Done()
            .WithConcerns( typeof( TestCompositeConcern ) );
      }

      [Test]
      public void TestInjectionConcernAsParameter()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            _mixinInvoked = false;
            _concernInvoked = false;
            composite.Test( null );
            Assert.IsTrue( _mixinInvoked, "Mixin must've been invoked." );
            Assert.IsTrue( _concernInvoked, "Concern must've been invoked." );
         } );
      }

      public interface TestComposite
      {
         void Test( TestComposite dummy );
      }

      public class TestCompositeMixin : TestComposite
      {

         #region TestComposite Members

         public virtual void Test( TestComposite dummy )
         {
            Assert.IsNull( dummy );
            _mixinInvoked = true;
         }

         #endregion
      }

      public class TestCompositeConcern : TestComposite
      {
         #region TestComposite Members

         public virtual void Test( [ConcernFor] TestComposite dummy )
         {
            _concernInvoked = true;
            dummy.Test( null );
         }

         #endregion
      }
   }
}
