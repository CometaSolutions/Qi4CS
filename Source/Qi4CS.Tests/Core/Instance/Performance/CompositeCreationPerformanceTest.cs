/*
 * Copyright (c) 2008, Niclas Hedhman.
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
using NUnit.Framework;
using Qi4CS.Core.API.Instance;
using Qi4CS.Tests.Core.Architecture.Layered;
using Qi4CS.Core.Bootstrap.Assembling;
using System.Diagnostics;

namespace Qi4CS.Tests.Core.Instance.Performance
{
   [Serializable]
   public class CompositeCreationPerformanceTest
   {
      private const Int32 ITERATIONS = 1000000;
      private const Int32 LOOPS = 2;
      [Serializable]
      public class SingletonCompositeCreationPerformanceTest : AbstractSingletonInstanceTest
      {

         protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
         {
            SetupTest( assembler );
         }

         [Ignore]
         [Test]
         public void PerformCompositeCreationPerformanceTest()
         {
            this.PerformTestInAppDomain( () =>
            {
               Console.WriteLine( "Singleton performance test starting." );
               RunCompositeCreationPerformanceTest( this.StructureServices );
               Console.WriteLine( "Singleton performance test ending." );
            } );
         }
      }

      [Serializable]
      public class LayeredCompositeCreationPerformanceTest : AbstractLayeredArchitectureInstanceTest
      {

         protected override void Assemble( LayeredCompositeAssembler assembler )
         {
            SetupTest( assembler );
         }

         [Ignore]
         [Test]
         public void PerformCompositeCreationPerformanceTest()
         {
            this.PerformTestInAppDomain( () =>
            {
               Console.WriteLine( "Layered performance test starting." );
               RunCompositeCreationPerformanceTest( this.StructureServices );
               Console.WriteLine( "Layered performance test ending." );
            } );
         }
      }

      public static void SetupTest( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( AnyComposite ) );
         // TODO implementing value composite model type should be done via extension
         //assembler.NewValue().OfTypes( typeof( AnyValue ) );
         assembler.NewPlainComposite().OfTypes( typeof( AnyObject ) );
      }

      public static void RunCompositeCreationPerformanceTest( StructureServiceProvider ssp )
      {
         Int64 t0 = TestCreationPerformanceInLoop( LOOPS, () => new AnyObject(), "Minimum C# Object creation time", "object" );
         Int64 t1 = TestCreationPerformanceInLoop( LOOPS, () => ssp.NewPlainCompositeBuilder<AnyComposite>().Instantiate(), "Transient builder creation time", "composite" );
         //Int64 t2 = TestCreationPerformanceInLoop( LOOPS, () => ssp.NewValueBuilder<AnyValue>().Instantiate(), "Value builder creation time", "composite" );
         Int64 t3 = TestCreationPerformanceInLoop( LOOPS, () => ssp.NewPlainCompositeBuilder<AnyObject>().Instantiate(), "Object builder creation time", "Qi4CS object" );

         Console.WriteLine( "Transient builder: " + ( t1 / t0 ) + "x" );
         //Console.WriteLine( "Value builder: " + ( t2 / t0 ) + "x" );
         Console.WriteLine( "Object builder: " + ( t3 / t0 ) + "x" );
      }

      public static Int64 TestCreationPerformanceInLoop( Int32 loops, Action action, String actionDescription, String objectType )
      {
         Int64 t = 0;
         for ( Int32 i = 0; i < loops; ++i )
         {
            t = t + TestCreationPerformance( action, actionDescription, objectType );
         }
         return t / loops;
      }

      public static Int64 TestCreationPerformance( Action action, String actionDescription, String objectType )
      {
         var iter = ITERATIONS;
         var sw = new Stopwatch();
         sw.Start();
         for ( Int64 i = 0; i < iter; ++i )
         {
            action();
         }
         sw.Stop();
         var time = sw.ElapsedTicks;
         Console.WriteLine( actionDescription + ":" + time + " ticks total." );
         return time;
      }

      public interface AnyComposite
      {
      }

      //public interface AnyValue
      //{
      //}

      public class AnyObject
      {
      }
   }
}
