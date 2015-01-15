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
using NUnit.Framework;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Property
{
   [Serializable]
   public class PropertyTest : AbstractSingletonInstanceTest
   {
      private const String GETTER_ONLY_PROP = "GetterOnly";
      private const String SETTER_ONLY_PROP = "SetterOnly";
      private const String GETTER_AND_SETTER_PROP = "GetterAndSetter";

      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler
            .NewPlainComposite().OfTypes( typeof( TestComposite ) )
            .WithMixins( typeof( TestCompositeMixin ) );
      }

      [Test]
      public void ManualPropertiesTest()
      {
         this.PerformTestInAppDomain( () =>
         {
            var composite = this.NewPlainComposite<TestComposite>();
            Assert.AreEqual( GETTER_ONLY_PROP, composite.GetterOnly, "The getter property must be the one that is set." );
            composite.SetterOnly = SETTER_ONLY_PROP;
            Assert.AreEqual( SETTER_ONLY_PROP, composite.GetSetterOnlyProp(), "The setter property must be the one that is set." );
            composite.GetterAndSetter = GETTER_AND_SETTER_PROP;
            Assert.AreEqual( GETTER_AND_SETTER_PROP, composite.GetterAndSetter, "The getter and setter property must be the one that is set." );
         } );
      }

      public interface TestComposite
      {
         String GetterOnly { get; }
         String SetterOnly { set; }
         String GetterAndSetter { get; set; }

         String GetSetterOnlyProp();
      }

      public class TestCompositeMixin : TestComposite
      {
         private readonly String _getter;
         private String _setter;
         private String _getterAndSetter;

         public TestCompositeMixin()
         {
            this._getter = GETTER_ONLY_PROP;
            this._setter = null;
            this._getterAndSetter = null;
         }

         #region TestComposite Members

         public virtual String GetterOnly
         {
            get
            {
               return this._getter;
            }
         }

         public virtual String SetterOnly
         {
            set
            {
               this._setter = value;
            }
         }

         public virtual String GetterAndSetter
         {
            get
            {
               return this._getterAndSetter;
            }
            set
            {
               this._getterAndSetter = value;
            }
         }

         public virtual String GetSetterOnlyProp()
         {
            return this._setter;
         }

         #endregion
      }
   }
}
