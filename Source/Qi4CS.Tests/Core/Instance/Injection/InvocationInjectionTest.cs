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
using System.Linq;
using System.Reflection;
using CollectionsWithRoles.API;
using NUnit.Framework;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Tests.Core.Instance.Injection
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class InvocationInjectionTest : AbstractSingletonInstanceTest
   {
      public const String FIRST_TEST_STRING = "1";
      public const String SECOND_TEST_STRING = "2";
      public const String THIRD_TEST_STRING = "X";
      public const String THIRD_TEST_STRING_MIXIN = "3";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite()
            .OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestMixin ) ).Done()
            .WithConcerns( typeof( TestConcern ) );
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();

            composite.DoStuff();
            composite.DoStuff2();
            composite.DoStuff3();
         } );
      }

      public interface TestComposite
      {
         [Foo( FIRST_TEST_STRING )]
         void DoStuff();

         void DoStuff2();

         [Foo( THIRD_TEST_STRING )]
         void DoStuff3();
      }

      public class TestConcern : ConcernOf<TestComposite>, TestComposite
      {
         private static readonly MethodInfo DO_STUFF_METHOD = typeof( TestComposite ).LoadMethodOrThrow( "DoStuff", null );
         private static readonly MethodInfo DO_STUFF_2_METHOD = typeof( TestComposite ).LoadMethodOrThrow( "DoStuff2", null );
         private static readonly MethodInfo DO_STUFF_3_METHOD = typeof( TestComposite ).LoadMethodOrThrow( "DoStuff3", null );

#pragma warning disable 649

         [Invocation]
         private MethodInfo _method;

         [Invocation]
         private FooAttribute _foo;

         [Invocation]
         private AttributeHolder _attributeHolder;

#pragma warning restore 649

         #region TestComposite Members

         public virtual void DoStuff()
         {
            Assert.AreEqual( FIRST_TEST_STRING, this._foo.Value, "Attribute has been injected" );
            Assert.AreEqual( FIRST_TEST_STRING, ( (FooAttribute) this._attributeHolder.AllAttributes[typeof( FooAttribute )].First() ).Value, "Attribute holder is correct" );
            Assert.AreEqual( DO_STUFF_METHOD, this._method, "Correct Method has been injected." );
            this.next.DoStuff();
         }

         public virtual void DoStuff2()
         {
            Assert.AreEqual( SECOND_TEST_STRING, this._foo.Value, "Attribute has been injected" );
            Assert.AreEqual( SECOND_TEST_STRING, ( (FooAttribute) this._attributeHolder.AllAttributes[typeof( FooAttribute )].First() ).Value, "Attribute holder is correct" );
            Assert.AreEqual( DO_STUFF_2_METHOD, this._method, "Correct Method has been injected." );
            this.next.DoStuff2();
         }

         public virtual void DoStuff3()
         {
            Assert.IsNotNull( this._foo, this._foo.Value, "Attribute has been injected" );
            ListQuery<Attribute> attrs = this._attributeHolder.AllAttributes[typeof( FooAttribute )];
            Assert.IsTrue( attrs.Cast<FooAttribute>().Any( attr => attr.Value.Equals( THIRD_TEST_STRING ) ), "Attribute holder must contain attribute declared in composite type." );
            Assert.IsTrue( attrs.Cast<FooAttribute>().Any( attr => attr.Value.Equals( THIRD_TEST_STRING_MIXIN ) ), "Attribute holder must contain attribute declared in mixin type." );
            Assert.AreEqual( DO_STUFF_3_METHOD, this._method, "Correct Method has been injected." );
            this.next.DoStuff3();
         }

         #endregion
      }

      public class TestMixin : TestComposite
      {

         #region TestComposite Members

         public virtual void DoStuff()
         {
         }

         [Foo( SECOND_TEST_STRING )]
         public virtual void DoStuff2()
         {
         }

         [Foo( THIRD_TEST_STRING_MIXIN )]
         public virtual void DoStuff3()
         {
         }

         #endregion
      }

      public class FooAttribute : Attribute
      {
         private readonly String _value;
         public FooAttribute( String value )
         {
            this._value = value;
         }

         public String Value
         {
            get
            {
               return this._value;
            }
         }
      }
   }
}
