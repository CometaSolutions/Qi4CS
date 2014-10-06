using System;
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
using NUnit.Framework;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   public class RoleTest : AbstractSingletonInstanceTest
   {
      private static Boolean _roleMethodCalled = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( CompositeType ), typeof( RoleType ) )
            .WithMixins( typeof( RoleMixin ) );
      }

      [Test]
      public void TestRole()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<CompositeType>();
            Assert.IsInstanceOf<RoleType>( composite, "The composite must implement the role given during assembling stage." );
            _roleMethodCalled = false;
            ( (RoleType) composite ).RoleMethod();
            Assert.IsTrue( _roleMethodCalled, "Role method must've been called" );
         } );
      }

      public interface CompositeType
      {

      }

      public interface RoleType
      {
         void RoleMethod();
      }

      public class RoleMixin : RoleType
      {

         #region RoleType Members

         public virtual void RoleMethod()
         {
            _roleMethodCalled = true;
         }

         #endregion
      }
   }
}
