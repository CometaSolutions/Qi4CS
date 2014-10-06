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
using CollectionsWithRoles.API;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Architectures.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.SPI.Common;

namespace Qi4CS.Core.Architectures.Assembling
{
   internal class LayeredCompositeAssemblerImpl : AssemblerImpl<LayeredApplicationImpl>, LayeredCompositeAssembler
   {
      private readonly ModuleArchitecture _module;

      internal LayeredCompositeAssemblerImpl( ModuleArchitecture moduleArchitecture, Func<Int32> newCompositeIDRequestor, DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> modelSupport, UsesContainerMutable parentContainer, CollectionsFactory collectionsFactory )
         : base( moduleArchitecture.Layer.Architecture, newCompositeIDRequestor, modelSupport, parentContainer, collectionsFactory )
      {
         this._module = moduleArchitecture;
      }

      protected override SPI.Instance.StructureServiceProviderSPI DoGetStructureServiceProvider( LayeredApplicationImpl application )
      {
         var module = application.FindModule( this );
         if ( module == null )
         {
            throw new ArgumentException( "Could not find suitable module for this assembler, are you sure this assembler is part of the given application?" );
         }
         return module.StructureServices;
      }

      #region LayeredCompositeAssembler Members

      public ModuleArchitecture Module
      {
         get
         {
            return this._module;
         }
      }

      #endregion
   }
}
