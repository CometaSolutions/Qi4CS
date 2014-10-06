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
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Core.Runtime.Assembling
{
   public class FragmentAssemblyDeclarationImpl : FragmentAssemblyDeclaration
   {
      private readonly AbstractCompositeAssemblyDeclaration _compositeAssemblyDeclaration;
      private readonly Type[] _fragmentTypes;
      private readonly FragmentAssemblyInfo[] _fragmentAssemblies;

      public FragmentAssemblyDeclarationImpl( AbstractCompositeAssemblyDeclaration compositeAssemblyDeclaration, Type[] fragmentTypes, FragmentAssemblyInfo[] fragmentAssemblies )
      {
         ArgumentValidator.ValidateNotNull( "Composite assembly declaration", compositeAssemblyDeclaration );
         ArgumentValidator.ValidateNotNull( "Fragment types", fragmentTypes );
         ArgumentValidator.ValidateNotNull( "Fragment assembly", fragmentAssemblies );

         this._compositeAssemblyDeclaration = compositeAssemblyDeclaration;
         this._fragmentTypes = fragmentTypes;
         this._fragmentAssemblies = fragmentAssemblies;
      }

      #region FragmentAssemblyDeclaration Members

      public AbstractCompositeAssemblyDeclaration Done()
      {
         return this._compositeAssemblyDeclaration;
      }

      public FragmentAssemblyDeclaration ApplyWith( params Type[] applyFilters )
      {
         applyFilters = applyFilters.FilterNulls();
         foreach ( var info in this._fragmentAssemblies )
         {
            foreach ( var fragmentType in this._fragmentTypes )
            {
               var typeInfo = info.TypeInfos[fragmentType];
               typeInfo.AppliesFilters.UnionWith( applyFilters );
            }
         }
         return this;
      }

      public FragmentAssemblyDeclaration ApplyWith( params AppliesToFilter[] readyMadeApplyFilters )
      {
         readyMadeApplyFilters = readyMadeApplyFilters.FilterNulls();
         foreach ( var info in this._fragmentAssemblies )
         {
            foreach ( var fragmentType in this._fragmentTypes )
            {
               var typeInfo = info.TypeInfos[fragmentType];
               typeInfo.AppliesReadyMades.UnionWith( readyMadeApplyFilters );
            }
         }
         return this;
      }

      #endregion
   }
}
