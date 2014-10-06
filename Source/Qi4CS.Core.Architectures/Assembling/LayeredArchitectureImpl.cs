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
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Architectures.Common;
using Qi4CS.Core.Architectures.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Architectures.Assembling
{
   internal class LayeredArchitectureImpl : ApplicationArchitectureSkeleton<LayeredApplicationModel>, LayeredArchitecture
   {

      private readonly DictionaryProxy<LayerArchitectureImpl, ListProxy<LayerArchitectureImpl>> _usageInfos;
      private readonly DictionaryProxy<String, LayerArchitectureImpl> _allLayers;
      private readonly Func<Int32> _compositeIDGenerator;

      internal LayeredArchitectureImpl( IEnumerable<CompositeModelTypeAssemblyScopeSupport> modelTypeSupport )
         : base( modelTypeSupport )
      {
         this._allLayers = this.CollectionsFactory.NewDictionaryProxy<String, LayerArchitectureImpl>();
         this._usageInfos = this.CollectionsFactory.NewDictionaryProxy<LayerArchitectureImpl, ListProxy<LayerArchitectureImpl>>();
         this._compositeIDGenerator = new CompositeIDGenerator().IDGeneratorFunction;
      }

      #region LayeredApplicationArchitecture Members

      public LayerArchitecture GetOrCreateLayer( String name )
      {
         ArgumentValidator.ValidateNotNull( "Layer name", name );
         LayerArchitectureImpl result = null;
         if ( !this._allLayers.CQ.TryGetValue( name, out result ) )
         {
            ListProxy<LayerArchitectureImpl> usageInfo = this.CollectionsFactory.NewListProxy<LayerArchitectureImpl>();
            result = new LayerArchitectureImpl( this, this.UsesContainer.Query, this._compositeIDGenerator, this.ModelTypeSupport, name, layers =>
               {
                  foreach ( var layer in layers )
                  {
                     if ( !Object.ReferenceEquals( this, layer.Architecture ) )
                     {
                        throw new ArgumentException( "Given layer " + layer + " is not from this architecture." );
                     }
                     else
                     {
                        usageInfo.Add( (LayerArchitectureImpl) layer );
                     }
                  }
               } );
            this._allLayers.Add( name, result );
            this._usageInfos.Add( result, usageInfo );
         }
         return result;
      }

      #endregion

      public override IEnumerable<Assembler> AllAssemblers
      {
         get
         {
            return this._allLayers.CQ.Values.SelectMany( layer => layer.ModuleImpls ).Select( module => module.CompositeAssembler );
         }
      }

      protected override LayeredApplicationModel DoCreateModel()
      {
         ListProxy<LayerModelMutable> topLevelLayers, allLayers;
         DictionaryQuery<CompositeModelType, CompositeModelTypeModelScopeSupport> compositeModelTypeSupport;
         DictionaryProxy<Int32, CompositeModel> models;
         var result = new LayeredApplicationModelImmutable(
            this,
            ArchitectureDefaults.GENERIC_COMPOSITE_PROPERTY_MIXIN_TYPE,
            ArchitectureDefaults.GENERIC_COMPOSITE_EVENT_MIXIN_TYPE,
            ArchitectureDefaults.GENERIC_FRAGMENT_TYPE,
            this.ModelTypeSupport,
            out compositeModelTypeSupport,
            out models,
            out topLevelLayers,
            out allLayers
            );

         var layers = new Dictionary<LayerArchitectureImpl, LayerModelMutable>();
         foreach ( var layer in this._allLayers.CQ.Values )
         {
            var lState = new LayerModelState( this.CollectionsFactory );
            var lImmutable = new LayerModelImmutable( lState );
            var lMutable = new LayerModelMutable( lState, lImmutable );

            lState.ApplicationModel = result;
            lState.LayerName = layer.Name;
            lState.ModuleModels.AddRange( layer.ModuleImpls.Select( module => this.NewModuleModel( compositeModelTypeSupport, models, lMutable, module ) ) );
            layers.Add( layer, lMutable );
            allLayers.Add( lMutable );
         }

         foreach ( KeyValuePair<LayerArchitectureImpl, ListProxy<LayerArchitectureImpl>> kvp in this._usageInfos.CQ )
         {
            layers[kvp.Key].AddUsedLayers( kvp.Value.CQ.Select( layer => layers[layer] ) );
         }

         topLevelLayers.AddRange( layers.Where( layerKvp => !this._usageInfos.CQ.Any( kvp => !kvp.Key.Equals( layerKvp.Key ) && this._usageInfos.CQ[kvp.Key].CQ.Contains( layerKvp.Key ) ) ).Select( layerKvp => layerKvp.Value ) );

         return result;
      }

      protected ModuleModelMutable NewModuleModel( DictionaryQuery<CompositeModelType, CompositeModelTypeModelScopeSupport> compositeModelTypeSupport, DictionaryProxy<Int32, CompositeModel> models, LayerModelMutable owningLayer, ModuleArchitectureImpl module )
      {
         var state = new ModuleModelState( this.CollectionsFactory );
         var immutable = new ModuleModelImmutable( state );
         var result = new ModuleModelMutable( state, immutable );

         state.ModuleName = module.Name;
         state.LayerModel = owningLayer;
         state.Assembler = module.LayeredCompositeAssembler;

         foreach ( var modelType in this.ModelTypeSupport.Keys )
         {
            IDictionary<CompositeModel, Visibility> visibilities = null;
            if ( !state.CompositeVisibilities.TryGetValue( modelType, out visibilities ) )
            {
               visibilities = new Dictionary<CompositeModel, Visibility>( ReferenceEqualityComparer<CompositeModel>.ReferenceBasedComparer );
               state.CompositeVisibilities.Add( modelType, visibilities );
            }
            foreach ( LayeredCompositeAssemblyInfo info in module.LayeredCompositeAssembler.GetInfos( modelType ).Cast<LayeredCompositeAssemblyInfo>() )
            {
               var cModel = this.NewCompositeModel( compositeModelTypeSupport, owningLayer.IQ.ApplicationModel, info, owningLayer.IQ.LayerName + "-" + module.Name );
               models.Add( cModel.CompositeModelID, cModel );
               state.CompositeModels.Add( cModel );
               visibilities.Add( cModel, info.Visibility );
            }
         }

         return result;
      }

      protected override void AssembleDomainSpecificAssemblers()
      {
         foreach ( LayerArchitectureImpl layer in this._allLayers.CQ.Values )
         {
            layer.AssembleDomainSpecificAssemblers();
            foreach ( ModuleArchitectureImpl module in layer.ModuleImpls )
            {
               module.AssembleDomainSpecificAssemblers();
            }
         }
      }

      protected override Tuple<Type, InjectionFunctionality>[] GetInjectionFunctionality()
      {
         return new[]
         {
            Tuple.Create<Type, InjectionFunctionality>( typeof( ThisAttribute ), new ThisInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( ConcernForAttribute ), new ConcernForInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( SideEffectForAttribute ), new SideEffectForInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( StateAttribute ), new StateInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( UsesAttribute ), new UsesInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( InvocationAttribute ), new InvocationInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( ServiceAttribute ), new LayeredApplicationServiceInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( StructureAttribute ), new LayeredApplicationStructureInjectionFunctionality() )
         };
      }
   }
}
