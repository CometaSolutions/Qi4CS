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
using NUnit.Framework;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Tests.Core.Instance.Common
{
   [Serializable]
   [Category( "Qi4CS.Core" )]
   public class UseDefaultsOnAssemblyLevelTest : AbstractSingletonInstanceTest
   {
      private const String DEFAULT_VALUE = "DefaultValue";

      protected override void Assemble( Assembler assembler )
      {
         assembler.NewPlainComposite()
            .OfTypes( typeof( TestComposite ) )
            .WithDefaultValueFor( typeof( TestComposite ).LoadPropertyOrThrow( "DefaultValueProperty" ), DEFAULT_VALUE );
      }

      [Test]
      public void TestDefaultValue()
      {
         this.PerformTestInAppDomain( () => Assert.AreEqual( DEFAULT_VALUE, this.NewPlainComposite<TestComposite>().DefaultValueProperty ) );
      }


      public interface TestComposite
      {
         String DefaultValueProperty { get; set; }
      }
   }
}
