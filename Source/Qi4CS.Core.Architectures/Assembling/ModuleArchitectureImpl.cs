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
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.Runtime.Common;
using Qi4CS.Core.SPI.Common;

namespace Qi4CS.Core.Architectures.Assembling
{
   internal class ModuleArchitectureImpl : ModuleArchitecture
   {
      private readonly String _moduleName;
      private readonly LayerArchitecture _layer;
      private readonly LayeredCompositeAssemblerImpl _assembler;
      private readonly UsesContainerMutable _metaInfo;
      private readonly DomainSpecificAssemblerAggregatorImpl<LayeredCompositeAssembler> _domainSpecificAssemblers;

      internal ModuleArchitectureImpl(
         String moduleName,
         LayerArchitecture layer,
         UsesContainerQuery parentContainer,
         Func<Int32> compositeIDGenerator,
         DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> modelTypeSupport
         )
      {
         ArgumentValidator.ValidateNotNull( "Layer", layer );
         ArgumentValidator.ValidateNotNull( "Parent meta-info container", parentContainer );

         this._moduleName = moduleName;
         this._layer = layer;
         this._metaInfo = UsesContainerMutableImpl.CreateWithParent( parentContainer );
         this._assembler = new LayeredCompositeAssemblerImpl( this, compositeIDGenerator, modelTypeSupport, this._metaInfo, layer.Architecture.CollectionsFactory );
         this._domainSpecificAssemblers = new DomainSpecificAssemblerAggregatorImpl<LayeredCompositeAssembler>( layer.Architecture.CollectionsFactory );
      }

      #region ModuleArchitecture Members

      public String Name
      {
         get
         {
            return this._moduleName;
         }
      }

      public LayerArchitecture Layer
      {
         get
         {
            return this._layer;
         }
      }

      public LayeredCompositeAssembler CompositeAssembler
      {
         get
         {
            return this._assembler;
         }
      }

      #endregion

      public override String ToString()
      {
         return this._moduleName;
      }

      internal LayeredCompositeAssemblerImpl LayeredCompositeAssembler
      {
         get
         {
            return this._assembler;
         }
      }

      #region UsesProvider<ModuleArchitecture> Members

      public ModuleArchitecture Use( params Object[] objects )
      {
         this._metaInfo.Use( objects );
         return this;
      }

      public ModuleArchitecture UseWithName( String name, Object value )
      {
         this._metaInfo.UseWithName( name, value );
         return this;
      }

      #endregion

      #region DomainSpecificAssemblerAggregator<LayeredCompositeAssembler> Members

      public void AddDomainSpecificAssemblers( params Bootstrap.Assembling.DomainSpecificAssembler<LayeredCompositeAssembler>[] assemblers )
      {
         this._domainSpecificAssemblers.AddDomainSpecificAssemblers( assemblers );
      }

      #endregion

      internal void AssembleDomainSpecificAssemblers()
      {
         foreach ( DomainSpecificAssembler<LayeredCompositeAssembler> assembler in this._domainSpecificAssemblers.Assemblers )
         {
            assembler.AddComposites( this._assembler );
         }
      }

   }
}
