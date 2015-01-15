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
using Qi4CS.Extensions.Functional.Model;

namespace Qi4CS.Tests.Extensions.Functional.Domain2
{
   public interface TestFunction
   {
      void Validate( IDictionary<TestData1, Result> results );
   }

   public interface TestFunctionService
   {
      Boolean IsValid( TestData1 first, TestData2 second );
   }

   public interface TestFunctionServiceSPI : TestFunctionService
   {
      IDictionary<TestData1, Result> ValidateWithResults( TestData1 first, TestData2 second );
      void Validate( [RoleParameter] TestData1 first, [RoleParameter] TestData3 third, IDictionary<TestData1, Result> results );
      void Validate( [RoleParameter] TestData1 first, [RoleParameter] TestData2 second, IDictionary<TestData1, Result> results );
   }

   public sealed class Result
   {

   }

   public abstract class TestFunctionServiceMixin : TestFunctionServiceSPI
   {
      public virtual IDictionary<TestData1, Result> ValidateWithResults( TestData1 first, TestData2 second )
      {
         IDictionary<TestData1, Result> results = new Dictionary<TestData1, Result>();
         this.Validate( first, second, results );
         return results;
      }

      public abstract void Validate( TestData1 first, TestData2 second, IDictionary<TestData1, Result> results );

      public abstract void Validate( TestData1 first, TestData3 third, IDictionary<TestData1, Result> results );

      public virtual Boolean IsValid( TestData1 first, TestData2 second )
      {
         return this.ValidateWithResults( first, second ).ContainsKey( first );
      }
   }
}
