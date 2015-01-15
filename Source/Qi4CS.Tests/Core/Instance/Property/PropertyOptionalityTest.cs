/*
 * Copyright (c) 2008, Rickard Öberg.
 * (org.qi4j.api.common.OptionalTest)
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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Property
{
   [Serializable]
   public class PropertyOptionalityTest : AbstractSingletonInstanceTest
   {
      protected override void Assemble( Qi4CS.Core.Bootstrap.Assembling.Assembler assembler )
      {
         assembler.NewPlainComposite().OfTypes( typeof( TestComposite ) );
      }

      [Test]
      public void GivenOptionalPropertyWhenOptionalMissingThenNoException()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewCompositeBuilder<TestComposite>( CompositeModelType.PLAIN );
            builder.Prototype().MandatoryProperty = "Hello World";
            var testComposite = builder.Instantiate();
         } );
      }

      [Test]
      public void GivenOptionalPropertyWhenOptionalSetThenNoException()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewCompositeBuilder<TestComposite>( CompositeModelType.PLAIN );
            builder.Prototype().MandatoryProperty = "Hello World";
            builder.Prototype().OptionalProperty = "Hello World";
            var testComposite = builder.Instantiate();
         } );
      }

      [Test]
      public void GivenMandatoryPropertyWhenMandatoryMissingThenException()
      {
         this.PerformTestInAppDomain( () =>
         Assert.Throws<CompositeInstantiationException>(
            () => this.NewPlainComposite<TestComposite>(),
            "The transient creation must fail because the non-optional property is null"
            ) );
      }

      [Test]
      public void GivenPrototypeSettingInvalidValuesOK()
      {
         this.PerformTestInAppDomain( () =>
         {
            var builder = this.StructureServices.NewCompositeBuilder<TestComposite>( CompositeModelType.PLAIN );
            builder.Prototype().MandatoryProperty = null;
            builder.Prototype().MandatoryProperty = "";
            var instance = builder.Instantiate();
            Assert.Throws<ConstraintViolationException>( () => instance.MandatoryProperty = null );
         } );
      }

      public interface TestComposite
      {
         [Optional]
         String OptionalProperty { get; set; }

         String MandatoryProperty { get; set; }
      }
   }
}
