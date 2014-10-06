/*
 * Copyright (c) 2008, Niclas Hedhman.
 * See NOTICE file.
 * 
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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Constraint
{
   [Serializable]
   public class ConstraintTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( MyOne ) )
            .WithMixins( typeof( MyOneMixin ) ).Done()
            .WithConstraints( typeof( TestConstraintImpl ), typeof( NonEmptyCollectionConstraint<> ), typeof( GenericConstraintImpl<> ) );
      }

      private const String STRING_PARAM = "StringParam";
      private const String VIOLATING_STRING_PARAM = "ViolatingStringParam";
      private const String LIST_ITEM = "OneItem";
      private const String LIST_ITEM2 = "AnotherItem";
      private const Int32 INT_PARAM = 223;

      [Test]
      public void GivenCompositeWithoutConstraintsWhenInstantiatedThenUseDeclarationOnConstraint()
      {
         this.PerformTestInAppDomain( () =>
         {
            var my = this.StructureServices.NewCompositeBuilder<MyOne>( CompositeModelType.PLAIN ).Instantiate();
            var list = new List<String>();
            list.Add( LIST_ITEM );
            my.DoSomething( STRING_PARAM, list );
            ConstraintViolationException exc = Assert.Throws<ConstraintViolationException>( new TestDelegate( () =>
            {
               my.DoSomething( VIOLATING_STRING_PARAM, new List<String>() );
            } ), "Should have thrown a ConstraintViolationException." );
            Assert.AreEqual( 2, exc.Violations.Length );
         } );
      }

      [Test]
      public void GivenConstrainedGenericWildcardParameterWhenInvokedThenUseConstraint()
      {
         this.PerformTestInAppDomain( () =>
         {
            var myOne = this.StructureServices.NewCompositeBuilder<MyOne>( CompositeModelType.PLAIN ).Instantiate();
            var list = new List<Object>();
            list.Add( LIST_ITEM2 );
            myOne.DoSomething2( list );
         } );
      }

      [Test]
      public void GivenCompositeConstraintWhenInvokedThenUseAllConstraints()
      {
         this.PerformTestInAppDomain( () =>
         {
            var myOne = this.StructureServices.NewCompositeBuilder<MyOne>( CompositeModelType.PLAIN ).Instantiate();
            var list = new List<Object>();
            list.Add( LIST_ITEM2 );
            myOne.DoSomething3( list );
         } );
      }

      [Test]
      public void GivenGenericConstraintThenDeduceCorrectArguments()
      {
         this.PerformTestInAppDomain( () =>
         {
            var myOne = this.StructureServices.NewCompositeBuilder<MyOne>( CompositeModelType.PLAIN ).Instantiate();
            myOne.DoSomething4<String>( STRING_PARAM );
         } );
      }

      [Test]
      public void GivenGenericConstraintAndWrongArgumentThenConstraintViolation()
      {
         this.PerformTestInAppDomain( () =>
         {
            var myOne = this.StructureServices.NewCompositeBuilder<MyOne>( CompositeModelType.PLAIN ).Instantiate();
            Assert.Throws<ConstraintViolationException>( () => myOne.DoSomething4<Int32>( INT_PARAM ), "The constraint must've returned false." );
         } );
      }


      public interface MyOne
      {
         void DoSomething( [Optional] [TestConstraint] String abc, [TestConstraint] IList<String> collection );

         void DoSomething2( [TestConstraint] [NonEmptyCollection] IList<Object> collection );

         void DoSomething3( [CompositeConstraint] [Name( "somecollection" )] IList<Object> collection );

         void DoSomething4<T>( [GenericConstraint] T param );
      }

      public class MyOneMixin : MyOne
      {

         #region MyOne Members

         public virtual void DoSomething( String abc, IList<String> collection )
         {
            if ( abc == null || collection == null )
            {
               throw new ArgumentNullException();
            }
         }

         public virtual void DoSomething2( IList<Object> collection )
         {
            if ( collection == null )
            {
               throw new ArgumentNullException();
            }
         }

         public virtual void DoSomething3( IList<Object> collection )
         {
            if ( collection == null )
            {
               throw new ArgumentNullException();
            }
         }

         public virtual void DoSomething4<T>( T param )
         {
            if ( param == null )
            {
               throw new ArgumentNullException();
            }
         }
         #endregion
      }

      [ConstraintDeclaration]
      public class TestConstraintAttribute : Attribute
      {
      }

      [ConstraintDeclaration]
      public class NonEmptyCollection : Attribute
      {

      }

      [ConstraintDeclaration]
      public class GenericConstraintAttribute : Attribute
      {

      }

      [ConstraintDeclaration]
      [TestConstraint]
      [NonEmptyCollection]
      public class CompositeConstraint : Attribute
      {

      }

      public class TestConstraintImpl : Constraint<TestConstraintAttribute, Object>
      {
         public Boolean IsValid( TestConstraintAttribute attribute, Object value )
         {
            if ( value is String )
            {
               return ( (String) value ).StartsWith( STRING_PARAM );
            }
            return value is ICollection && ( (ICollection) value ).Count > 0;
         }
      }

      public class NonEmptyCollectionConstraint<T> : Constraint<NonEmptyCollection, ICollection<T>>
      {
         public Boolean IsValid( NonEmptyCollection attribute, ICollection<T> value )
         {
            return value.Count > 0;
         }
      }

      public class GenericConstraintImpl<T> : Constraint<GenericConstraintAttribute, T>
      {

         #region Constraint<GenericConstraintAttribute,T> Members

         public Boolean IsValid( GenericConstraintAttribute attribute, T value )
         {
            return typeof( String ).Equals( typeof( T ) );
         }

         #endregion
      }
   }
}
