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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   public class SpecificCompositeTest : AbstractSingletonInstanceTest
   {

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( SpecifiedComposite ) )
            .WithMixins( typeof( BaseGenericCompositeMixin<> ) );
      }

      [Test]
      public void TestSpecifiedCompositeWorking()
      {
         this.PerformTestInAppDomain( () => Assert.IsNull( this.NewPlainComposite<SpecifiedComposite>().DoSomething() ) );
      }

      public interface BaseGenericComposite<T>
      {
         [return: Optional]
         T DoSomething();
      }

      public interface SpecifiedComposite : BaseGenericComposite<String>
      {

      }

      public class BaseGenericCompositeMixin<T> : BaseGenericComposite<T>
      {
#pragma warning disable 649, 169

         [This]
         private BaseGenericComposite<T> _me;

#pragma warning restore 649, 169

         #region BaseGenericComposite<T> Members

         public virtual T DoSomething()
         {
            return default( T );
         }

         #endregion

         [Prototype]
         public void MethodWithInjectedParameter( [This] BaseGenericComposite<T> me )
         {
            Assert.IsTrue( Object.ReferenceEquals( this._me, me ) );
         }
      }
   }
}
