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

namespace Qi4CS.Extensions.Configuration.Instance
{
   internal class ConfigurationManagerInfo
   {
      private readonly IDictionary<Type, ConfigurationCompositeInfo> _compositeInfos;

      internal ConfigurationManagerInfo()
      {
         this._compositeInfos = new Dictionary<Type, ConfigurationCompositeInfo>();
      }

      internal IDictionary<Type, ConfigurationCompositeInfo> CompositeInfos
      {
         get
         {
            return this._compositeInfos;
         }
      }
   }
}
