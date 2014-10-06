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
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.Runtime.Common;
using Qi4CS.Core.SPI.Common;

namespace Qi4CS.Core.Architectures.Assembling
{
   internal class LayerArchitectureImpl : LayerArchitecture
   {
      private readonly String _layerName;
      private readonly LayeredArchitecture _application;
      private readonly DictionaryProxy<String, ModuleArchitectureImpl> _modules;
      private readonly DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> _modelTypeSupport;
      private readonly Action<LayerArchitecture[]> _usageAction;
      private readonly UsesContainerMutable _metaInfo;
      private readonly Func<Int32> _compositeIDGenerator;
      private readonly DomainSpecificAssemblerAggregatorImpl<LayerArchitecture> _domainSpecificAssemblers;


      internal LayerArchitectureImpl(
         LayeredArchitecture application,
         UsesContainerQuery parentContainer,
         Func<Int32> compositeIDGenerator,
         DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> modelTypeSupport,
         String name,
         Action<LayerArchitecture[]> usageAction
         )
      {
         ArgumentValidator.ValidateNotNull( "Application", application );
         ArgumentValidator.ValidateNotNull( "Model type support", modelTypeSupport );
         ArgumentValidator.ValidateNotNull( "Composite ID Generator", compositeIDGenerator );
         ArgumentValidator.ValidateNotNull( "Action to tell usage", usageAction );
         ArgumentValidator.ValidateNotNull( "Parent meta-info container", parentContainer );

         this._application = application;
         this._compositeIDGenerator = compositeIDGenerator;
         this._usageAction = usageAction;
         this._layerName = name;
         this._modelTypeSupport = modelTypeSupport;
         this._metaInfo = UsesContainerMutableImpl.CreateWithParent( parentContainer );
         this._modules = application.CollectionsFactory.NewDictionaryProxy<String, ModuleArchitectureImpl>();
         this._domainSpecificAssemblers = new DomainSpecificAssemblerAggregatorImpl<LayerArchitecture>( application.CollectionsFactory );
      }

      #region LayerArchitecture Members

      public String Name
      {
         get
         {
            return this._layerName;
         }
      }

      public LayeredArchitecture Architecture
      {
         get
         {
            return this._application;
         }
      }

      public void UseLayers( params LayerArchitecture[] layers )
      {
         this._usageAction( layers.FilterNulls() );
      }

      public ModuleArchitecture GetOrCreateModule( String name )
      {
         ArgumentValidator.ValidateNotNull( "Module name", name );

         ModuleArchitectureImpl result = null;
         if ( !this._modules.CQ.TryGetValue( name, out result ) )
         {
            result = new ModuleArchitectureImpl( name, this, this._metaInfo.Query, this._compositeIDGenerator, this._modelTypeSupport );
            this._modules.Add( name, result );
         }

         return result;
      }

      #endregion

      public override String ToString()
      {
         return this._layerName;
      }

      internal IEnumerable<ModuleArchitectureImpl> ModuleImpls
      {
         get
         {
            return this._modules.CQ.Values;
         }
      }

      #region UsesProvider<LayerArchitecture> Members

      public LayerArchitecture Use( params Object[] objects )
      {
         this._metaInfo.Use( objects );
         return this;
      }

      public LayerArchitecture UseWithName( String name, Object value )
      {
         this._metaInfo.UseWithName( name, value );
         return this;
      }

      #endregion

      #region DomainSpecificAssemblerAggregator<LayerArchitecture> Members

      public void AddDomainSpecificAssemblers( params Bootstrap.Assembling.DomainSpecificAssembler<LayerArchitecture>[] assemblers )
      {
         this._domainSpecificAssemblers.AddDomainSpecificAssemblers( assemblers );
      }

      #endregion

      internal void AssembleDomainSpecificAssemblers()
      {
         foreach ( var assembler in this._domainSpecificAssemblers.Assemblers )
         {
            assembler.AddComposites( this );
         }
      }
   }
}
