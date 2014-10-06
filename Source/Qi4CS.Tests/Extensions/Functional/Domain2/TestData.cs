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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Tests.Extensions.Functional.Domain2
{
   public interface TestData1
   {
      [Optional]
      String Data { get; set; }
   }

   public interface TestData2
   {
      [Optional]
      TestData3 Data { get; set; }
   }

   public interface TestData3
   {
      [Optional]
      String Data { get; set; }
   }
}
