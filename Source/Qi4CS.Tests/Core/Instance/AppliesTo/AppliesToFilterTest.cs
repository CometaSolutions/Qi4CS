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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.AppliesTo
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class AppliesToFilterTest : AbstractSingletonInstanceTest
   {
      public const String STRING_TO_APPEND = "AppendedPart";
      public const String FIRST_RESULT = "First";
      public const String SECOND_RESULT = "Second";
      public const String THIRD_RESULT = "Third";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestMixin ) ).Done()
            .WithConcerns( typeof( TestConcern ) ).ApplyWith( typeof( TestFilter ) );
      }

      [Test]
      public void GivenAnAppliesToFilterWhenAppliedThenFilterMethods()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            Assert.AreEqual( FIRST_RESULT, composite.TestMethod1(), "First method must return original value." );
            Assert.AreEqual( SECOND_RESULT + STRING_TO_APPEND, composite.TestMethod2(), "Second method must return modified value." );
            Assert.AreEqual( THIRD_RESULT, composite.TestMethod3(), "Third method must return original value." );
         } );
      }

      public interface TestComposite
      {
         String TestMethod1();
         String TestMethod2();
         String TestMethod3();
      }

      public class TestConcern : GenericConcern
      {
         public override Object Invoke( Object composite, System.Reflection.MethodInfo method, Object[] args )
         {
            String nextResult = (String) this.next.Invoke( composite, method, args );
            return nextResult + STRING_TO_APPEND;
         }
      }

      public class TestMixin : TestComposite
      {

         #region TestComposite Members

         public virtual String TestMethod1()
         {
            return FIRST_RESULT;
         }

         public virtual String TestMethod2()
         {
            return SECOND_RESULT;
         }

         public virtual String TestMethod3()
         {
            return THIRD_RESULT;
         }

         #endregion
      }

      public class TestFilter : AppliesToFilter
      {

         #region AppliesToFilter Members

         public Boolean AppliesTo( System.Reflection.MethodInfo compositeMethod, System.Reflection.MethodInfo implementingMethod )
         {
            return compositeMethod.Name.Equals( "TestMethod2" );
         }

         #endregion
      }
   }
}
