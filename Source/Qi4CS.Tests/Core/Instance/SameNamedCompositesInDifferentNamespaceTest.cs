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
using System;
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance
{
   namespace First
   {
      public interface TheTestComposite
      {
         String A();
      }
      public class TheTestMixin : TheTestComposite
      {

         #region TheTestComposite Members

         public virtual String A()
         {
            throw new NotImplementedException();
         }

         #endregion
      }
   }
   namespace Second
   {
      public interface TheTestComposite
      {
         String B();
      }
      public class TheTestMixin : TheTestComposite
      {

         #region TheTestComposite Members

         public virtual String B()
         {
            throw new NotImplementedException();
         }

         #endregion
      }
   }

   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class SameNamedCompositesInDifferentNamespaceTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( First.TheTestComposite ) ).WithMixins( typeof( First.TheTestMixin ) );
         assembler.NewPlainComposite().OfTypes( typeof( Second.TheTestComposite ) ).WithMixins( typeof( Second.TheTestMixin ) );
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            // TODO make CompositeModelInfos protected, and use composite builder + newInstance to compare type assignability and GUIDs.
            First.TheTestComposite first = this.StructureServices.NewCompositeBuilder<First.TheTestComposite>( CompositeModelType.PLAIN ).Instantiate();
            Second.TheTestComposite second = this.StructureServices.NewCompositeBuilder<Second.TheTestComposite>( CompositeModelType.PLAIN ).Instantiate();

            Assert.AreNotEqual( first.GetType().GUID, second.GetType().GUID, "The two different composite types must produce two different generated types." );
         } );
      }
   }
}
