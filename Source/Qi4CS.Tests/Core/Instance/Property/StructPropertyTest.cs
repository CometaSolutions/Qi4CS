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
using System.Linq;
using System.Text;
using Qi4CS.Core.Bootstrap.Assembling;
using NUnit.Framework;

namespace Qi4CS.Tests.Core.Instance.Property
{
   [Serializable, Category( "INSTANCE.PROPERTY" )]
   public class StructPropertyTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite ) );
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            // TODO transfer properties to private composite, to catch more errors.
            var composite = this.NewPlainComposite<TestComposite>();
            Assert.AreEqual( composite.ByteProperty, default( Byte ) );
            Assert.AreEqual( composite.SByteProperty, default( SByte ) );
            Assert.AreEqual( composite.Int16Property, default( Int16 ) );
            Assert.AreEqual( composite.UInt16Property, default( UInt16 ) );
            Assert.AreEqual( composite.CharProperty, default( Char ) );
            Assert.AreEqual( composite.Int32Property, default( Int32 ) );
            Assert.AreEqual( composite.UInt32Property, default( UInt32 ) );
            Assert.AreEqual( composite.Int64Property, default( Int64 ) );
            Assert.AreEqual( composite.UInt64Property, default( UInt64 ) );
            Assert.AreEqual( composite.DoubleProperty, default( Double ) );
            Assert.AreEqual( composite.SingleProperty, default( Single ) );
            Assert.AreEqual( composite.BooleanProperty, default( Boolean ) );
            Assert.AreEqual( composite.EnumProperty, TestEnum.FIRST_VALUE );
            Assert.AreEqual( composite.CustomStructProperty, default( CustomStruct ) );
         } );
      }

      internal enum TestEnum { FIRST_VALUE, SECOND_VALUE }

      internal interface TestComposite
      {
         Byte ByteProperty { get; set; }

         SByte SByteProperty { get; set; }

         Int16 Int16Property { get; set; }

         UInt16 UInt16Property { get; set; }

         Char CharProperty { get; set; }

         Int32 Int32Property { get; set; }

         UInt32 UInt32Property { get; set; }

         Int64 Int64Property { get; set; }

         UInt64 UInt64Property { get; set; }

         Double DoubleProperty { get; set; }

         Single SingleProperty { get; set; }

         Boolean BooleanProperty { get; set; }

         TestEnum EnumProperty { get; set; }

         CustomStruct CustomStructProperty { get; set; }
      }

      internal struct CustomStruct
      {

      }
   }
}
