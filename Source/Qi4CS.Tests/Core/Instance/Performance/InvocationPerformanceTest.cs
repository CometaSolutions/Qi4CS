/*
 * Copyright (c) 2007, Rickard Öberg.
 * See NOTICE file.
 * 
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
using System.Reflection;
using NUnit.Framework;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Tests.Core.Architecture.Layered;
using Qi4CS.Core.Bootstrap.Assembling;
using System.Diagnostics;

namespace Qi4CS.Tests.Core.Instance.Performance
{
   [Serializable]
   public class InvocationPerformanceTest
   {
      public const Int64 ITERATIONS = 10000000L;
      public const Int32 LOOPS = 3;

      [Serializable]
      public class SingletonInvocationPerformanceTest : AbstractSingletonInstanceTest
      {

         protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
         {
            SetupTest( assembler );
         }

         [Ignore]
         [Test]
         public void PerformInvocationPerformanceTest()
         {
            this.PerformTestInAppDomain( () =>
            {
               RunInvocationPerformanceTest( this.StructureServices );
            } );
         }
      }

      [Serializable]
      public class LayeredInvocationPerformanceTest : AbstractLayeredArchitectureInstanceTest
      {

         protected override void Assemble( LayeredCompositeAssembler assembler )
         {
            SetupTest( assembler );
         }

         [Ignore]
         [Test]
         public void PerformInvocationPerformanceTest()
         {
            this.PerformTestInAppDomain( () =>
            {
               RunInvocationPerformanceTest( this.StructureServices );
            } );
         }
      }

      public static void SetupTest( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite ) );
         assembler.NewPlainComposite().OfTypes( typeof( TestWithTypedConcernComposite ) );
         assembler.NewPlainComposite().OfTypes( typeof( TestWithGenericConcernComposite ) );
      }

      public static void RunInvocationPerformanceTest( StructureServiceProvider ssp )
      {
         TestBase manualSimple = new TestMixin();

         TestBase manualTypedConcernMixin = new TestMixin();
         TestBase manualTypedConcernObj = new TestTypedConcern( manualTypedConcernMixin );

         TestBase manualGenericConcernMixin = new TestMixin();
         TestBase manualGenericConcernObj = new ManualNextInvocator( manualGenericConcernMixin );

         Int64 t0 = TestInvocationPerformanceInLoop( LOOPS, manualSimple );
         Int64 t1 = TestInvocationPerformanceInLoop( LOOPS, manualTypedConcernObj );
         Int64 t2 = TestInvocationPerformanceInLoop( LOOPS, manualGenericConcernObj );

         Int64 t3 = TestInvocationPerformanceInLoop( LOOPS, ssp.NewPlainCompositeBuilder<TestComposite>().Instantiate() );
         Int64 t4 = TestInvocationPerformanceInLoop( LOOPS, ssp.NewPlainCompositeBuilder<TestWithTypedConcernComposite>().Instantiate() );
         Int64 t5 = TestInvocationPerformanceInLoop( LOOPS, ssp.NewPlainCompositeBuilder<TestWithGenericConcernComposite>().Instantiate() );

         Console.WriteLine( "Simple: " + ( t3 / t0 ) + "x" );
         Console.WriteLine( "Typed concern: " + ( t4 / t1 ) + "x" );
         Console.WriteLine( "Generic concern: " + ( t5 / t2 ) + "x" );
      }

      protected static Int64 TestInvocationPerformanceInLoop( Int32 loops, TestBase test )
      {
         Int64 t = 0;
         for ( Int32 i = 0; i < loops; ++i )
         {
            t = t + TestInvocationPerformance( test );
         }
         return t / loops;
      }

      protected static Int64 TestInvocationPerformance( TestBase test )
      {
         var count = ITERATIONS;
         var sw = new Stopwatch();
         sw.Start();
         for ( Int64 i = 0; i < count; ++i )
         {
            test.Test();
         }
         sw.Stop();
         var time = sw.ElapsedMilliseconds;
         var callsPerSecond = ( 1000L * count ) / time;
         Console.WriteLine( "Calls per second: " + callsPerSecond );
         return time;
      }

      [DefaultMixins( typeof( TestMixin ) )]
      public interface TestBase
      {
         void Test();
      }

      public interface TestComposite : TestBase
      {

      }

      [DefaultConcerns( typeof( TestTypedConcern ) )]
      public interface TestWithTypedConcernComposite : TestBase
      {

      }

      [DefaultConcerns( typeof( TestGenericConcern ) )]
      public interface TestWithGenericConcernComposite : TestBase
      {

      }

      public class TestMixin : TestBase
      {
         private Int64 _count = 0L;

         #region TestComposite Members

         public virtual void Test()
         {
            ++this._count;
         }

         #endregion
      }

      public class TestTypedConcern : TestBase
      {
         [ConcernFor]
         private readonly TestBase _next;

         public TestTypedConcern()
            : this( null )
         {

         }

         public TestTypedConcern( TestBase next )
         {
            this._next = next;
         }

         #region TestComposite Members

         public virtual void Test()
         {
            this._next.Test();
         }

         #endregion
      }

      public class TestGenericConcern : GenericInvocator
      {
#pragma warning disable 649
         [ConcernFor]
         private readonly GenericInvocator _next;
#pragma warning restore 649

         public TestGenericConcern()
            : this( null )
         {

         }

         public TestGenericConcern( GenericInvocator next )
         {
            this._next = next;
         }

         public Object Invoke( Object composite, System.Reflection.MethodInfo method, Object[] args )
         {
            return this._next.Invoke( composite, method, args );
         }
      }

      public class ManualNextInvocator : GenericInvocator, TestBase
      {
         private TestBase _test;

         private static readonly MethodInfo METHOD = typeof( TestBase ).GetMethod( "Test" );

         public ManualNextInvocator( TestBase test )
         {
            this._test = test;
         }

         #region GenericInvocator Members

         public Object Invoke( Object composite, System.Reflection.MethodInfo method, Object[] args )
         {
            this._test.Test();
            return null;
         }

         #endregion

         #region TestBase Members

         public void Test()
         {
            this.Invoke( this, METHOD, new Object[] { } );
         }

         #endregion
      }
   }
}
