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
using NUnit.Framework;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Tests;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.SPI.Instance;
using System.Reflection.Emit;
using Qi4CS.Core.Architectures.Assembling;

namespace Qi4CS.Tests.Core.Instance
{
   [Serializable]
   public abstract class AgnosticismTest
   {
      private const String IMMUTABLE_PROPERTY = "ImmutableProperty";

      protected static Boolean _concernInvoked;
      protected static Boolean _mixinInvoked;
      protected static Boolean _sideEffectInvoked;
      protected static String MANDATORY_PARAM = "Mandatory";
      private static IDictionary<Type, Func<Attribute>> _transformers;

      static AgnosticismTest()
      {
         _transformers = new Dictionary<Type, Func<Attribute>>();
         _transformers.Add( typeof( MyOptionalAttribute ), () => OptionalAttribute.VALUE );
         _transformers.Add( typeof( MyImmutableAttribute ), () => new ImmutableAttribute() );
         _transformers.Add( typeof( MyThisAttribute ), () => new ThisAttribute() );
         _transformers.Add( typeof( MyServiceAttribute ), () => new ServiceAttribute() );
         _transformers.Add( typeof( MyConcernForAttribute ), () => new ConcernForAttribute() );
         _transformers.Add( typeof( MySideEffectForAttribute ), () => new SideEffectForAttribute() );
         _transformers.Add( typeof( MyStructureAttribute ), () => new StructureAttribute() );
         _transformers.Add( typeof( MyConstraintAttribute ), () => new MyConstraintDeclarationAttribute() );

      }

      private static AppDomainEmulator _domain;

      [Test]
      public void PerformTest()
      {
         _domain.DoCallBack( () =>
         {
            var architecture = Qi4CSArchitectureFactory.NewSingletonArchitecture();
            var assembler = architecture.CompositeAssembler;
            assembler
               .NewPlainComposite().OfTypes( typeof( MyComposite ) )
               .WithConcerns( typeof( MyCompositeConcern ) ).Done()
               .WithSideEffects( typeof( MyCompositeSideEffect ) ).Done()
               .WithMixins( typeof( MyCompositeMixin ) );
            assembler
               .NewService().OfTypes( typeof( MyService ) );
            architecture.AttributeProcessingEvent += new EventHandler<AttributeProcessingArgs>( architecture_AttributeProcessingEvent );

            var model = architecture.CreateModel();
            model.GenerateAndSaveAssemblies( CodeGeneration.CodeGenerationParallelization.NotParallel, logicalAssemblyProcessor: Qi4CSCodeGenHelper.EmittingArgumentsCallback );
            var application = model.NewInstance( TestConstants.APPLICATION_NAME, TestConstants.APPLICATION_MODE, TestConstants.APPLICATION_VERSION );
            application.Activate();
            try
            {
               var builder = application.StructureServices.NewPlainCompositeBuilder<MyComposite>();
               builder.Prototype().ImmutableProperty = IMMUTABLE_PROPERTY;
               var composite = builder.Instantiate();
               this.TestComposite( composite );
            }
            finally
            {
               application.Passivate();
            }
         } );
      }

      static void architecture_AttributeProcessingEvent( object sender, AttributeProcessingArgs e )
      {
         if ( _transformers.ContainsKey( e.OldAttribute.GetType() ) )
         {
            e.NewAttribute = _transformers[e.OldAttribute.GetType()]();
         }
      }

      protected abstract void TestComposite( MyComposite composite );


      [SetUp]
      public void SetUp()
      {
         _domain = Qi4CSTestUtils.CreateTestAppDomain( "Qi4CS Agnosticism Test (" + this.GetType() + ")." );
         _domain.DoCallBack( () =>
         {
            _mixinInvoked = false;
            _concernInvoked = false;
            _sideEffectInvoked = false;
         } );
      }

      [TearDown]
      public void TearDown()
      {
         _domain.Dispose();
         _domain = null;
      }

      public class MyOptionalAttribute : Attribute { }
      public class MyImmutableAttribute : Attribute { }
      public class MyThisAttribute : Attribute { }
      public class MyServiceAttribute : Attribute { }
      public class MyConcernForAttribute : Attribute { }
      public class MySideEffectForAttribute : Attribute { }
      public class MyStructureAttribute : Attribute { }
      public class MyConstraintAttribute : Attribute { }

      public interface MyComposite
      {
         [MyImmutable]
         String ImmutableProperty { get; set; }

         void MethodWithOptionalParameter( [MyConstraint] String mandatory, [MyOptional] String optional );

         void TestThisAttribute();

         void TestServiceAttribute();

         void TestConcernAttribute();

         void TestSideEffectAttribute();

         void TestStructureAttribute();
      }

      public abstract class MyCompositeConcern : MyComposite
      {
#pragma warning disable 649
         [MyConcernFor]
         private MyComposite _next;
#pragma warning restore 649

         #region MyComposite Members

         public abstract String ImmutableProperty { get; set; }

         public abstract void MethodWithOptionalParameter( String mandatory, String optional );

         public abstract void TestThisAttribute();

         public abstract void TestServiceAttribute();

         public virtual void TestConcernAttribute()
         {
            _concernInvoked = true;
            this._next.TestConcernAttribute();
         }

         public abstract void TestSideEffectAttribute();

         public abstract void TestStructureAttribute();

         #endregion
      }

      public abstract class MyCompositeSideEffect : MyComposite
      {
#pragma warning disable 649
         [MySideEffectFor]
         private MyComposite _result;
#pragma warning restore 649

         #region MyComposite Members

         public abstract String ImmutableProperty { get; set; }

         public abstract void MethodWithOptionalParameter( String mandatory, String optional );

         public abstract void TestThisAttribute();

         public abstract void TestServiceAttribute();

         public abstract void TestConcernAttribute();

         public virtual void TestSideEffectAttribute()
         {
            _sideEffectInvoked = true;
            this._result.TestSideEffectAttribute();

         }

         public abstract void TestStructureAttribute();

         #endregion
      }

      public abstract class MyCompositeMixin : MyComposite
      {
#pragma warning disable 649
         [MyThis]
         private MyComposite _me;

         [MyService]
         private MyService _service;

         [MyStructure]
         private StructureServiceProvider _structure;

         [MyStructure]
         private Application _application;

#pragma warning restore 649

         #region MyComposite Members

         public abstract String ImmutableProperty { get; set; }

         public virtual void MethodWithOptionalParameter( String mandatory, String optional )
         {
            Assert.IsNotNull( mandatory );
            _mixinInvoked = true;
         }

         public virtual void TestThisAttribute()
         {
            Assert.IsNotNull( this._me );
            _mixinInvoked = true;
         }

         public virtual void TestServiceAttribute()
         {
            Assert.IsNotNull( this._service );
            _mixinInvoked = true;
         }

         public virtual void TestConcernAttribute()
         {
            _mixinInvoked = true;
         }

         public virtual void TestSideEffectAttribute()
         {
            _mixinInvoked = true;
         }

         public virtual void TestStructureAttribute()
         {
            Assert.IsNotNull( this._structure );
            Assert.IsNotNull( this._application );
            _mixinInvoked = true;
         }

         #endregion
      }

      public interface MyService
      {

      }

      [ConstraintDeclaration]
      [DefaultConstraints( typeof( MyConstraintImpl<> ) )]
      public class MyConstraintDeclarationAttribute : Attribute
      { }

      public class MyConstraintImpl<T> : Constraint<MyConstraintDeclarationAttribute, T>
      {

         #region Constraint<MyConstraintDeclaration,string> Members

         public Boolean IsValid( MyConstraintDeclarationAttribute attribute, T value )
         {
            return value is String && MANDATORY_PARAM.Equals( value );
         }

         #endregion
      }

   }

   [Serializable]
   public class AgnosticismTestForProperties : AgnosticismTest
   {
      protected override void TestComposite( MyComposite composite )
      {
         Assert.IsNotNull( composite.ImmutableProperty );
         Assert.Throws<InvalidOperationException>( () => composite.ImmutableProperty = "Test" );
      }
   }

   [Serializable]
   public class AgnosticismTestForAutoProperties : AgnosticismTest
   {

      protected override void TestComposite( MyComposite composite )
      {
         Assert.Throws<ConstraintViolationException>( () => composite.ImmutableProperty = null );
      }

   }

   [Serializable]
   public class AgnosticismTestForConstraints : AgnosticismTest
   {
      protected override void TestComposite( MyComposite composite )
      {
         composite.MethodWithOptionalParameter( MANDATORY_PARAM, null );
         Assert.IsTrue( _mixinInvoked );
      }
   }

   [Serializable]
   public class AgnosticismTestForConstraintsWithThrow : AgnosticismTest
   {
      protected override void TestComposite( MyComposite composite )
      {
         Assert.Throws<ConstraintViolationException>( () => composite.MethodWithOptionalParameter( null, null ) );
      }
   }

   [Serializable]
   public class AgnosticismTestForCustomConstraintsWithThrow : AgnosticismTest
   {
      protected override void TestComposite( MyComposite composite )
      {
         Assert.Throws<ConstraintViolationException>( () => composite.MethodWithOptionalParameter( MANDATORY_PARAM + "test", null ) );
      }
   }

   [Serializable]
   public class AgnosticismTestForThisAttribute : AgnosticismTest
   {
      protected override void TestComposite( MyComposite composite )
      {
         composite.TestThisAttribute();
         Assert.IsTrue( _mixinInvoked );
      }
   }

   [Serializable]
   public class AgnosticismTestForServiceAttribute : AgnosticismTest
   {
      protected override void TestComposite( MyComposite composite )
      {
         composite.TestServiceAttribute();
         Assert.IsTrue( _mixinInvoked );
      }
   }

   [Serializable]
   public class AgnosticismTestForStructureAttribute : AgnosticismTest
   {
      protected override void TestComposite( MyComposite composite )
      {
         composite.TestStructureAttribute();
         Assert.IsTrue( _mixinInvoked );
      }

   }

   [Serializable]
   public class AgnosticismTestForConcernAttribute : AgnosticismTest
   {
      protected override void TestComposite( MyComposite composite )
      {
         composite.TestConcernAttribute();
         Assert.IsTrue( _concernInvoked, "Concern must've been invoked." );
         Assert.IsTrue( _mixinInvoked, "Mixin must've been invoked." );
      }
   }

   [Serializable]
   public class AgnosticismTestForSideEffectAttribute : AgnosticismTest
   {
      protected override void TestComposite( MyComposite composite )
      {
         composite.TestSideEffectAttribute();
         Assert.IsTrue( _sideEffectInvoked, "Side effect must've been invoked." );
         Assert.IsTrue( _mixinInvoked, "Mixin must've been invoked." );
      }
   }
}
