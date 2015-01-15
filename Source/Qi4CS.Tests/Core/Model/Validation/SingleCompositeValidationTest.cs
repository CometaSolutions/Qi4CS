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
using Qi4CS.Core.API.Model;

namespace Qi4CS.Tests.Core.Model.Validation
{
   [Serializable]
   public abstract class SingleCompositeSingleCompositeValidationTest : AbstractSingletonModelTest
   {
      public interface TestComposite
      {
         String A();
      }

      public interface DerivedComposite : TestComposite
      {
         String B();
      }

      public class TestCompositeMixin : TestComposite
      {

         #region TestComposite Members

         public virtual String A()
         {
            throw new NotImplementedException();
         }

         #endregion
      }

      public class TestCompositeMixinNoVirtual : TestComposite
      {

         #region TestComposite Members

         public String A()
         {
            throw new NotImplementedException();
         }

         #endregion
      }

      public class DerivedCompositeMixin : TestCompositeMixin, DerivedComposite
      {

         #region DerivedComposite Members

         public virtual String B()
         {
            throw new NotImplementedException();
         }

         #endregion
      }

      public abstract class CompositeMixinWithAbstractMethod : TestComposite
      {

         #region TestComposite Members

         public virtual String A()
         {
            throw new NotImplementedException();
         }

         #endregion

         protected abstract void MyAbstractMethod();
      }

      public class Dummy { }

      public interface Empty { }

      public interface GenericCompositeParent<T>
      {
         T[] GetArray();
      }

      public interface GenericComposite2<T> : GenericCompositeParent<T>
      {
      }

      public class GenericCompositeMixinWithArrayGType<T> : GenericComposite2<T[]>
      {

#pragma warning disable 649

         [Uses]
         private T[][] _array;

#pragma warning restore 649

         #region GenericComposite2<T[]> Members

         public virtual T[][] GetArray()
         {
            return this._array;
         }

         #endregion
      }
   }

   [Serializable]
   public class TestValidModelClass : SingleCompositeSingleCompositeValidationTest
   {
      [Test]
      public void TestValidModel()
      {
         this.PerformTest( 0, 0, 0 );
      }

      protected override void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects )
      {
         compositeType = typeof( TestComposite );
         mixins = new Type[] { typeof( TestCompositeMixin ) };
      }
   }

   [Serializable]
   public class TestNoVirtualMethodClass : SingleCompositeSingleCompositeValidationTest
   {
      [Test]
      public void TestNoVirtualMethod()
      {
         this.PerformTest( 0, 1, 0 );
      }

      protected override void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects )
      {
         compositeType = typeof( TestComposite );
         mixins = new Type[] { typeof( TestCompositeMixinNoVirtual ) };
      }
   }

   [Serializable]
   public class TestInterfaceMixinClass : SingleCompositeSingleCompositeValidationTest
   {

      [Test]
      public void TestInterfaceMixin()
      {
         this.PerformTest( 0, 1, 0 );
      }

      protected override void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects )
      {
         compositeType = typeof( TestComposite );
         mixins = new Type[] { typeof( TestComposite ) };
      }
   }

   [Serializable]
   public class TestIncompleteCompositeClass : SingleCompositeSingleCompositeValidationTest
   {

      [Test]
      public void TestIncompleteComposite()
      {
         this.PerformTest( 0, 1, 0 );
      }

      protected override void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects )
      {
         compositeType = typeof( TestComposite );
         mixins = new Type[] { typeof( Dummy ) };
      }

   }

   [Serializable]
   public class TestEmptyCompositeClass : SingleCompositeSingleCompositeValidationTest
   {
      [Test]
      public void TestEmptyComposite()
      {
         this.PerformTest( 0, 0, 0 );
      }

      protected override void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects )
      {
         compositeType = typeof( Empty );
      }
   }

   [Serializable]
   public class TestDerivedCompositeClass : SingleCompositeSingleCompositeValidationTest
   {
      [Test]
      public void TestDerivedComposite()
      {
         this.PerformTest( 0, 0, 0 );
      }

      protected override void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects )
      {
         compositeType = typeof( DerivedComposite );
         mixins = new Type[] { typeof( DerivedCompositeMixin ) };
      }
   }

   [Serializable]
   public class TestIncompleteFragmentClass : SingleCompositeSingleCompositeValidationTest
   {
      [Test]
      public void TestIncompleteFragment()
      {
         this.PerformTest( 0, 1, 0 );
      }

      protected override void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects )
      {
         compositeType = typeof( TestComposite );
         mixins = new Type[] { typeof( CompositeMixinWithAbstractMethod ) };
      }

   }

   [Serializable]
   public class TestFragmentWithImplementingArrayGenericTypeClass : SingleCompositeSingleCompositeValidationTest
   {
      [Test]
      public void TestFragmentWithImplementingArrayGenericType()
      {
         this.PerformTest( 0, 1, 0 );
      }

      protected override void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects )
      {
         compositeType = typeof( GenericComposite2<> );
         mixins = new Type[] { typeof( GenericCompositeMixinWithArrayGType<> ) };
      }

   }

   [Serializable]
   public class TestCompositeWithDefaultParameterClass : SingleCompositeSingleCompositeValidationTest
   {
      [Test]
      public void TestCompositeWithDefaultParameterMethod()
      {
         this.PerformTest( 0, 0, 0 );
      }

      protected override void SetupApplicationArchitecture( ref Type compositeType, ref Type[] mixins, ref Type[] concerns, ref Type[] sideEffects )
      {
         compositeType = typeof( TestCompositeWithDefaultParameter );
         mixins = new Type[] { typeof( TestCompositeWithDefaultParameterMixin ) };
      }

      public interface TestCompositeWithDefaultParameter
      {
         void DoSomething( Boolean paramz = false );
      }

      public class TestCompositeWithDefaultParameterMixin : TestCompositeWithDefaultParameter
      {

         #region TestCompositeWithDefaultParameter Members

         public virtual void DoSomething( Boolean paramz = false )
         {

         }

         #endregion
      }
   }
}
