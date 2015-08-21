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
using System.Collections;
using NUnit.Framework;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Tests.Core.Instance.Common
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class UseDefaultsTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite<> ) );
      }

      [Test]
      public void PerformTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            // TODO transfer properties to private composite, to catch more errors.
            var composite = this.NewPlainComposite<TestComposite<Int32>>();
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
            Assert.AreEqual( composite.StringProperty, "" );
            Assert.AreEqual( composite.EnumProperty, TestEnum.FIRST_VALUE );
            Assert.AreEqual( composite.GenericNullable, 0 );
            //Assert.IsTrue( new ArrayList().OfType<Object>().SequenceEqual( composite.ListProperty.OfType<Object>() ) );
            Assert.IsTrue( new List<TestEnum>().SequenceEqual( composite.ListGProperty ) );
            Assert.IsTrue( new HashSet<TestEnum>().SequenceEqual( composite.SetProperty ) );
            //Assert.IsTrue( new ArrayList().OfType<Object>().SequenceEqual( composite.CollectionProperty.OfType<Object>() ) );
            Assert.IsTrue( new List<TestEnum>().SequenceEqual( composite.CollectionGProperty ) );
            //Assert.IsTrue( new Hashtable().OfType<DictionaryEntry>().SequenceEqual( composite.DicProperty.OfType<DictionaryEntry>() ) );
            Assert.IsTrue( new Dictionary<Int32, TestEnum>().SequenceEqual( composite.DicGProperty ) );
            Assert.IsTrue( new TestEnum[] { }.SequenceEqual( composite.ArrayProperty ) );
            Assert.IsTrue( new Int32[] { }.SequenceEqual( composite.ArrayGProperty ) );
            Assert.IsNotNull( composite.CustomClassProperty );
         } );
      }

      internal enum TestEnum { FIRST_VALUE, SECOND_VALUE }

      internal interface TestComposite<T>
         where T : struct
      {
         [UseDefaults]
         Byte? ByteProperty { get; set; }

         [UseDefaults]
         SByte? SByteProperty { get; set; }

         [UseDefaults]
         Int16? Int16Property { get; set; }

         [UseDefaults]
         UInt16? UInt16Property { get; set; }

         [UseDefaults]
         Char? CharProperty { get; set; }

         [UseDefaults]
         Int32? Int32Property { get; set; }

         [UseDefaults]
         UInt32? UInt32Property { get; set; }

         [UseDefaults]
         Int64? Int64Property { get; set; }

         [UseDefaults]
         UInt64? UInt64Property { get; set; }

         [UseDefaults]
         Double? DoubleProperty { get; set; }

         [UseDefaults]
         Single? SingleProperty { get; set; }

         [UseDefaults]
         Boolean? BooleanProperty { get; set; }

         [UseDefaults]
         String StringProperty { get; set; }

         //[UseDefaults]
         //IList ListProperty { get; set; }

         [UseDefaults]
         IList<TestEnum> ListGProperty { get; set; }

         [UseDefaults]
         ISet<TestEnum> SetProperty { get; set; }

         //[UseDefaults]
         //ICollection CollectionProperty { get; set; }

         [UseDefaults]
         ICollection<TestEnum> CollectionGProperty { get; set; }

         //[UseDefaults]
         //IDictionary DicProperty { get; set; }

         [UseDefaults]
         IDictionary<T, TestEnum> DicGProperty { get; set; }

         [UseDefaults]
         TestEnum? EnumProperty { get; set; }

         [UseDefaults]
         TestEnum[] ArrayProperty { get; set; }

         [UseDefaults]
         T[] ArrayGProperty { get; set; }

         [UseDefaults]
         CustomClass CustomClassProperty { get; set; }

         [UseDefaults]
         T? GenericNullable { get; set; }

      }
   }

   internal class CustomClass
   {
      internal CustomClass()
      {

      }
   }
}
