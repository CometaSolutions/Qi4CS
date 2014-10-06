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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using NUnit.Framework;
using Qi4CS.Core.API.Common;

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   public class GenericCompositeTestWithArrayTypes : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Assembler assembler )
      {
         assembler.NewPlainComposite()
            .OfTypes( typeof( GenericComposite<> ) )
            .WithMixins( typeof( GenericCompositeMixin<> ) );


         assembler.NewPlainComposite()
            .OfTypes( typeof( GenericCompositeWithoutMethods<> ) )
            .WithMixins( typeof( GenericCompositeWithoutMethodsMixin<> ) );
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () => this.DoPerformTest<GenericComposite<String>, String>() );
      }

      [Test]
      public void PerformTest2()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewPlainCompositeBuilder<GenericCompositeWithoutMethods<String>>();
            builder.Builder.Use( "" );
            builder.Instantiate().SomeMethod();
         } );
      }

      protected void DoPerformTest<TComposite, TArrayElement>()
         where TComposite : class, GenericCompositeParent<TArrayElement>
      {
         var array = new TArrayElement[] { };

         var builder = this.StructureServices.NewPlainCompositeBuilder<TComposite>();
         builder.Builder.Use( new Object[] { array } );
         Assert.AreSame( array, builder.Instantiate().GetArray() );
      }

      public interface GenericCompositeParent<T>
      {
         T[] GetArray();
      }

      public interface GenericComposite<T> : GenericCompositeParent<T>
      {
      }

      public class GenericCompositeMixin<T> : GenericComposite<T>
      {
#pragma warning disable 649

         [Uses]
         private T[] _array;

#pragma warning restore 649

         #region GenericComposite<T> Members

         public virtual T[] GetArray()
         {
            return this._array;
         }

         #endregion
      }

      public interface NonGenericComposite
      {
         void SomeMethod();
      }

      public interface GenericCompositeWithoutMethods<T> : NonGenericComposite
      {

      }

      public abstract class AbstractNonGenericCompositeMixin<T, U> : NonGenericComposite
      {
         [Uses]
         public U dummy;

         #region NonGenericComposite Members

         public abstract void SomeMethod();

         #endregion
      }

      public class GenericCompositeWithoutMethodsMixin<T> : AbstractNonGenericCompositeMixin<T[], String>, GenericCompositeWithoutMethods<T>
      {
         public override void SomeMethod()
         {

         }
      }
   }
}
