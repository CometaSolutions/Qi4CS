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

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   public class PrivateCompositeTest : AbstractSingletonInstanceTest
   {
      private static Boolean _privateMethodInvoked = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite()
            .OfTypes( typeof( PublicComposite ) )
            .WithMixins( typeof( PublicCompositeMixin ), typeof( PrivateCompositeMixin ) );
      }

      [Test]
      public void TestPrivateCompositeTypeResolving()
      {
         this.PerformTestInAppDomain( () =>
         {
            PublicComposite composite = this.NewPlainComposite<PublicComposite>();
            Assert.IsInstanceOf<PrivateCompositeSub>( composite.Private );
            _privateMethodInvoked = false;
            ( (PrivateCompositeSub) composite.Private ).SubMethod();
            Assert.IsTrue( _privateMethodInvoked );
         } );
      }

      public interface PublicComposite
      {
         PrivateComposite Private { get; }
      }

      public interface PrivateComposite
      {

      }

      public interface PrivateCompositeSub : PrivateComposite
      {
         void SubMethod();
      }

      public class PublicCompositeMixin : PublicComposite
      {
#pragma warning disable 649

         [This]
         private PrivateComposite _private;

#pragma warning restore 649



         #region PublicComposite Members

         public virtual PrivateComposite Private
         {
            get
            {
               return this._private;
            }
         }

         #endregion
      }

      public class PrivateCompositeMixin : PrivateCompositeSub
      {

         #region PrivateCompositeSub Members

         public virtual void SubMethod()
         {
            _privateMethodInvoked = true;
         }

         #endregion
      }
   }
}
