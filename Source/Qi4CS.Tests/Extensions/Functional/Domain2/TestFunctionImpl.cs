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
using Qi4CS.Core.API.Model;
using NUnit.Framework;
using Qi4CS.Extensions.Functional.Model;

namespace Qi4CS.Tests.Extensions.Functional.Domain2
{
   public class TestFunction1Impl : TestFunction
   {
      internal static Boolean called = false;

      [Role]
      public TestData1 _first;

      [Role]
      public TestData2 _second;

#pragma warning disable 649
      [Service]
      private TestFunctionServiceSPI _functionService;

#pragma warning restore 649

      #region TestFunctionWithFirst Members

      public virtual void Validate( IDictionary<TestData1, Result> results )
      {
         called = true;
         this._functionService.Validate( this._first, this._second.Data, results );
         results.Add( this._first, new Result() );
      }

      #endregion
   }

   public class TestFunction2Impl : TestFunction
   {
      internal static Boolean called = false;

      [Role]
      public TestData1 _first;

      [Role]
      public TestData2 _second;

      [Role]
      public TestData3 _third;

      #region TestFunctionWithSecond Members

      public virtual void Validate( IDictionary<TestData1, Result> results )
      {
         Assert.IsNotNull( this._first, "First" );
         Assert.IsNotNull( this._second, "Second" );
         Assert.IsNotNull( this._third, "Third" );

         called = true;
      }

      #endregion
   }
}
