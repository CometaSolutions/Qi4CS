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
using CommonUtils;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.Runtime.Assembling
{
   public class FragmentTypeInfo
   {
      private readonly Type _fragmentType;
      private readonly ISet<Type> _appliesFilters;
      private readonly ISet<AppliesToFilter> _appliesReadyMades;

      public FragmentTypeInfo( Type fragmentType )
      {
         ArgumentValidator.ValidateNotNull( "Fragment type", fragmentType );

         this._fragmentType = fragmentType;
         this._appliesFilters = new HashSet<Type>();
         this._appliesReadyMades = new HashSet<AppliesToFilter>();
      }

      public Type FragmentType
      {
         get
         {
            return this._fragmentType;
         }
      }

      public ISet<Type> AppliesFilters
      {
         get
         {
            return this._appliesFilters;
         }
      }

      public ISet<AppliesToFilter> AppliesReadyMades
      {
         get
         {
            return this._appliesReadyMades;
         }
      }
   }
}
