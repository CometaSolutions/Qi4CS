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
using System.Reflection;
using NUnit.Framework;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   public class GenericCompositeTest : AbstractSingletonInstanceTest
   {

      private static readonly Type GENERIC_ARGUMENT = typeof( String );
      private static readonly Type GENERIC_METHOD_ARGUMENT = typeof( String );
      private static readonly String PARAMETER = "Parameter";

      private const String RESULT = "Result";

      private static Boolean _mixinInvoked = false;
      private static Boolean _complexMixinInvoked = false;
      private static Boolean _concernInvoked1 = false;
      private static Boolean _concernInvoked2 = false;

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( GenericComposite<> ) )
            .WithMixins( typeof( GenericCompositeMixin<> ) ).Done()
            .WithConcerns( typeof( GenericCompositeGenericConcern ), typeof( GenericCompositeConcern<> ) );
         assembler
            .NewPlainComposite().OfTypes( typeof( ComplexGenericComposite<> ) )
            .WithMixins( typeof( GenericCompositeMixin<> ), typeof( ComplexGenericCompositeMixin<> ) ).Done()
            .WithConcerns( typeof( GenericCompositeGenericConcern ), typeof( GenericCompositeConcern<> ) );
      }

      [Test]
      public void UsingGenericCompositesTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<GenericComposite<String>>();
            _mixinInvoked = false;
            _concernInvoked1 = false;
            _concernInvoked2 = false;
            composite.NormalMethod();
            Assert.IsTrue( _mixinInvoked, "Mixin must've been invoked." );
            Assert.IsTrue( _concernInvoked1, "Concern must've been invoked." );
            Assert.IsTrue( _concernInvoked2, "Concern must've been invoked." );
            _mixinInvoked = false;
            _concernInvoked1 = false;
            _concernInvoked2 = false;
            composite.GenericMethod<String>( PARAMETER );
            Assert.IsTrue( _mixinInvoked, "Mixin must've been invoked." );
            Assert.IsTrue( _concernInvoked1, "Concern must've been invoked." );
            Assert.IsTrue( _concernInvoked2, "Concern must've been invoked." );
         } );
      }

      [Test]
      public void UsingComplexGenericCompositesTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<ComplexGenericComposite<String>>();
            _complexMixinInvoked = false;
            _mixinInvoked = false;
            _concernInvoked1 = false;
            _concernInvoked2 = false;
            composite.A();
            Assert.IsTrue( _complexMixinInvoked, "Mixin of complex type must've been invoked." );
            Assert.IsTrue( _mixinInvoked, "Mixin must've been invoked." );
            Assert.IsTrue( _concernInvoked1, "Concern must've been invoked." );
            Assert.IsTrue( _concernInvoked2, "Concern must've been invoked." );
         } );
      }

      public class GenericCompositeMixin<T> : GenericComposite<T>
      {

         #region GenericComposite<T> Members

         public virtual T NormalMethod()
         {
            _mixinInvoked = true;
            Assert.AreEqual( GENERIC_ARGUMENT, typeof( T ), "The generic argument should be " + GENERIC_ARGUMENT + "." );
            return (T) (Object) RESULT;
         }

         public virtual void GenericMethod<U>( U parameter )
         where U : class
         {
            _mixinInvoked = true;
            Assert.AreEqual( GENERIC_ARGUMENT, typeof( T ), "The generic argument should be " + GENERIC_ARGUMENT + "." );
            Assert.AreEqual( GENERIC_METHOD_ARGUMENT, typeof( U ), "the generic method argument should be " + GENERIC_METHOD_ARGUMENT + "." );
            Assert.AreEqual( PARAMETER, parameter, "The parameter must be same as given." );
         }

         #endregion
      }

      public class GenericCompositeConcern<T> : ConcernOf<GenericComposite<T>>, GenericComposite<T>
      {

         #region GenericComposite<T> Members

         public virtual T NormalMethod()
         {
            _concernInvoked1 = true;
            return this.next.NormalMethod();
         }

         public virtual void GenericMethod<U>( U parameter )
            where U : class
         {
            _concernInvoked1 = true;
            this.next.GenericMethod( parameter );
         }

         #endregion
      }

      public class GenericCompositeGenericConcern : GenericConcern
      {

         public override Object Invoke( Object composite, MethodInfo method, Object[] args )
         {
            _concernInvoked2 = true;
            Assert.AreEqual( GENERIC_ARGUMENT, composite.GetType().GetGenericArguments()[0], "The generic argument should be " + GENERIC_ARGUMENT + "." );
            Assert.AreEqual( GENERIC_ARGUMENT, method.DeclaringType.GetGenericArguments()[0], "The generic argument should be " + GENERIC_ARGUMENT + "." );
            return this.next.Invoke( composite, method, args );
         }
      }

      public interface ComplexGenericComposite<T>
      {
         [return: Optional]
         T A();
      }

      public class ComplexGenericCompositeMixin<T> : ComplexGenericComposite<T>
      {
#pragma warning disable 649

         [This]
         private GenericComposite<T> _generic;

#pragma warning restore 649

         #region ComplexGenericComposite<T> Members

         public virtual T A()
         {
            _complexMixinInvoked = true;
            this._generic.GenericMethod<String>( PARAMETER );
            return default( T );
         }

         #endregion
      }
   }
}
