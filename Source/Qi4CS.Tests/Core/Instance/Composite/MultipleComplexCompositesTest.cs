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
using System.Collections.Generic;
using NUnit.Framework;
using Qi4CS.Tests.Core.Instance.Composite.Domain;
using Qi4CS.Core.Bootstrap.Assembling;
using System;

namespace Qi4CS.Tests.Core.Instance.Composite
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class MultipleComplexCompositesTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( CollectionWithRoles<,> ) )
            .WithMixins( typeof( CollectionWithRolesMixin<,> ), typeof( CollectionMutableQueryMixin<,> ), typeof( CollectionImmutableQueryMixin<,> ) );
         assembler
            .NewPlainComposite().OfTypes( typeof( ListWithRoles<,> ) )
            .WithMixins( typeof( ListWithRolesMixin<,> ), typeof( ListMutableQueryMixin<,> ), typeof( ListImmutableQueryMixin<,> ), typeof( CollectionImmutableQueryMixin<,> ), typeof( CollectionMutableQueryMixin<,> ), typeof( CollectionWithRolesMixin<,> ) );
         assembler
            .NewService().OfTypes( typeof( CollectionsFactory ) )
            .WithMixins( typeof( CollectionsFactoryMixin ) );
         assembler
            .NewPlainComposite().OfTypes( typeof( Int32Object ) )
            .WithMixins( typeof( Int32ObjectMixin ), typeof( Int32ObjectIQMixin ) );
         assembler
            .NewService().OfTypes( typeof( ObjectsFactory ) )
            .WithMixins( typeof( ObjectsFactoryMixin ) );
      }

      [Test]
      public void TestUsingComposites()
      {
         this.PerformTestInAppDomain( () =>
         {
            var objFactory = this.FindService<ObjectsFactory>();
            var obj = objFactory.NewInt32Object();
            obj.AssignableValue = 666;

            var factory = this.FindService<CollectionsFactory>();
            var list = factory.NewList<Int32Object, Int32ObjectIQ>();
            list.AsCollectionWithRoles.Add( obj );
            Assert.AreEqual( list.MQ.IQ.AsCollectionImmutableQuery.Count, 1, "List must contain the object." );
            Assert.AreEqual( list.MQ[0], obj, "List must contain the object." );
            Assert.AreEqual( list.MQ.IQ[0], obj.IQ, "List must contain the object." );
            Assert.IsNotInstanceOf<IList<Int32Object>>( list.MQ.AsCollectionMutableQuery.EnumerableMutables, "The mutable query must not return modifiable list." );
            Assert.AreEqual( obj.IQ.ImplementedType, typeof( Int32Object ), "The implemented type must be Int32Object." );
         } );
      }
   }
}
