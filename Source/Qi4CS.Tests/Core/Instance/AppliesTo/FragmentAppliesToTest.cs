/*
 * Copyright (c) 2007-2009, Niclas Hedhman.
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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.AppliesTo
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class FragmentAppliesToTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( BaseTypeFirstMixin ), typeof( BaseTypeSecondMixin ), typeof( CounterImpl ) ).Done()
            .WithSideEffects( typeof( CountCallsSideEffect ) ).ApplyWith( typeof( TestAppliesToAttribute ) );
      }

      [Test]
      public void TestMixin()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            Assert.IsNotNull( composite.GetStructureServices(), "Injection must've been successful." );
            Assert.IsNotNull( composite.GetMeAsSecond(), "Injection must've been successful." );
            Assert.AreEqual( 1, composite.Value, "Counter must've been incremented exactly once." );
            composite.GetStructureServices();
            composite.GetStructureServices();
            composite.GetStructureServices();
            Assert.AreEqual( 4, composite.Value, "Count must've been incremented exactly thrice." );
         } );
      }

      public interface TestComposite : BaseTypeFirst, BaseTypeSecond, Counter
      {

      }

      public interface BaseTypeFirst
      {
         StructureServiceProvider GetStructureServices();

         BaseTypeSecond GetMeAsSecond();
      }

      public class BaseTypeFirstMixin : BaseTypeFirst
      {
#pragma warning disable 649
         [Structure]
         private StructureServiceProvider _structureServices;

         [This]
         private BaseTypeSecond _meAsSecond;

#pragma warning restore 649

         #region BaseTypeFirst Members

         [TestAppliesTo]
         public virtual StructureServiceProvider GetStructureServices()
         {
            return this._structureServices;
         }

         public virtual BaseTypeSecond GetMeAsSecond()
         {
            return this._meAsSecond;
         }

         #endregion
      }

      public interface BaseTypeSecond
      {

      }

      public class BaseTypeSecondMixin : BaseTypeSecond
      {

      }

      public interface Counter
      {
         void Increment();
         void Clear();
         Int32 Value { get; }
      }

      public class CounterImpl : Counter
      {
         private Int32 _value;

         #region Counter Members

         public virtual void Increment()
         {
            ++this._value;
         }

         public virtual void Clear()
         {
            this._value = 0;
         }

         public virtual Int32 Value
         {
            get
            {
               return this._value;
            }
         }

         #endregion
      }

      public class CountCallsSideEffect : GenericSideEffect
      {
#pragma warning disable 649
         [This]
         private Counter _counter;

#pragma warning restore 649

         protected override void DoInvoke( object composite, System.Reflection.MethodInfo method, object[] args )
         {
            this._counter.Increment();
         }
      }

      public class TestAppliesToAttribute : Attribute
      {

      }
   }
}
