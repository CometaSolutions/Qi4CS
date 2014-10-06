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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Injection
{
   [Serializable]
   public class UsesInjectionTest : AbstractSingletonInstanceTest
   {
      private const String USES_NAME = "UsesName";
      private const String USES_FIELD_VALUE = "UsesField";
      private const String FIRST_PARAM = "FirstParam";
      private const Int32 SECOND_PARAM = 56789;

      private static Boolean _constructorInvoked = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite<> ) )
            .WithMixins( typeof( TestMixin<> ) );
      }

      public class OptionalUses
      {

      }

      [Test]
      public void TestUsesAttribute()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewCompositeBuilder<TestComposite<String>>( CompositeModelType.PLAIN );
            builder.Builder.Use( USES_FIELD_VALUE, SECOND_PARAM );
            builder.Builder.UseWithName( USES_NAME, FIRST_PARAM );

            var composite = builder.Instantiate();
            _constructorInvoked = false;
            String uses = composite.GetUsesField();
            Assert.IsTrue( _constructorInvoked, "The constructor of the fragment must've been invoked." );
            Assert.IsNotNull( uses, "The injected uses attribute must not be null" );
            Assert.AreEqual( USES_FIELD_VALUE, uses, "The value of the uses field must be the specified." );
         } );
      }

      public interface TestComposite<T>
      {
         String GetUsesField();
      }

      public class TestMixin<T> : TestComposite<T>
      {
#pragma warning disable 649

         [Uses]
         private String _uses;

         [Uses, Optional]
         private OptionalUses _optionalUses;

#pragma warning restore 649

         public TestMixin( [Uses( USES_NAME )] T param1, [Uses] Int32 param2 )
         {
            Assert.AreEqual( FIRST_PARAM, param1, "The first parameter must match the one specified." );
            Assert.AreEqual( SECOND_PARAM, param2, "The second parameter must match the one specified." );
            _constructorInvoked = true;
         }

         #region TestComposite<T> Members

         public virtual String GetUsesField()
         {
            Assert.IsNull( this._optionalUses );
            return this._uses;
         }

         #endregion
      }


   }
}
