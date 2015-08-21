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
using System.Linq;
using NUnit.Framework;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class MixinTest : AbstractSingletonInstanceTest
   {

      public static readonly String EXPECTED_RESULT_1 = "Tezt";

      public static readonly Object[] EXPECTED_RESULT_2 = { 6, false, false, true };

      public static readonly Object[] EXPECTED_RESULT_3 = { 1, 2, 3 };

      public static readonly Int32 EXPECTED_RESULT_4 = 666;

      protected override void Assemble( Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) );
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite2 ) )
            .WithMixins( typeof( TestComposite2Mixin1 ), typeof( TestComposite2Mixin2 ) );
      }

      [Test]
      public void SimpleMixinTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();

            Assert.AreEqual( EXPECTED_RESULT_1, composite.A(), "The return value must be expected" );
            Assert.AreEqual( EXPECTED_RESULT_2, composite.B( (Int32) EXPECTED_RESULT_2[0], (Boolean) EXPECTED_RESULT_2[1], (Boolean) EXPECTED_RESULT_2[2], (Boolean) EXPECTED_RESULT_2[3] ), "The return value must be expected" );
            Assert.AreEqual( EXPECTED_RESULT_3, composite.C( (Int32) EXPECTED_RESULT_3[0], (Int32) EXPECTED_RESULT_3[1], (Int32) EXPECTED_RESULT_3[2] ), "The return value must be expected" );
            Assert.AreEqual( EXPECTED_RESULT_4, composite.D(), "The return value must be expected" );
         } );
      }

      [Test]
      public void MultipleMixinTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite2>();

            Assert.AreEqual( EXPECTED_RESULT_1, composite.A(), "The return value must be expected" );
            Assert.AreEqual( EXPECTED_RESULT_2, composite.B( (Int32) EXPECTED_RESULT_2[0], (Boolean) EXPECTED_RESULT_2[1], (Boolean) EXPECTED_RESULT_2[2], (Boolean) EXPECTED_RESULT_2[3] ), "The return value must be expected" );
            Assert.AreEqual( EXPECTED_RESULT_3, composite.C( (Int32) EXPECTED_RESULT_3[0], (Int32) EXPECTED_RESULT_3[1], (Int32) EXPECTED_RESULT_3[2] ), "The return value must be expected" );
            Assert.AreEqual( EXPECTED_RESULT_4, composite.D(), "The return value must be expected" );
         } );
      }

      public interface TestComposite
      {
         String A();

         Object[] B( Int32 x, params Boolean[] y );

         Object[] C( params Int32[] x );

         Int32 D();
      }

      public class TestCompositeMixin : TestComposite
      {
         #region TestComposite Members

         public virtual String A()
         {
            return EXPECTED_RESULT_1;
         }

         public virtual Object[] B( Int32 x, params Boolean[] y )
         {
            Object[] result = new Object[1 + y.Length];
            result[0] = x;
            for ( Int32 idx = 0; idx < y.Length; ++idx )
            {
               result[1 + idx] = y[idx];
            }
            return result;
         }

         public virtual Object[] C( params Int32[] x )
         {
            return x.Select( e => (Object) e ).ToArray();
         }

         public virtual Int32 D()
         {
            return EXPECTED_RESULT_4;
         }

         #endregion
      }

      public interface TestComposite2
      {
         String A();

         Object[] B( Int32 x, params Boolean[] y );

         Object[] C( params Int32[] x );

         Int32 D();
      }

      public abstract class TestComposite2Mixin1 : TestComposite2
      {

         #region TestComposite2 Members

         public virtual String A()
         {
            return EXPECTED_RESULT_1;
         }

         public virtual Object[] B( Int32 x, params Boolean[] y )
         {
            Object[] result = new Object[1 + y.Length];
            result[0] = x;
            for ( Int32 idx = 0; idx < y.Length; ++idx )
            {
               result[1 + idx] = y[idx];
            }
            return result;
         }

         public abstract Object[] C( params Int32[] x );

         public abstract Int32 D();

         #endregion
      }

      public abstract class TestComposite2Mixin2 : TestComposite2
      {
         #region TestComposite2 Members

         public abstract String A();

         public abstract Object[] B( Int32 x, params Boolean[] y );

         public virtual Object[] C( params Int32[] x )
         {
            return x.Select( e => (Object) e ).ToArray();
         }

         public virtual Int32 D()
         {
            return EXPECTED_RESULT_4;
         }

         #endregion
      }
   }
}
