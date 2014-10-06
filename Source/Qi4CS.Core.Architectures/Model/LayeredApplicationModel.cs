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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Architectures.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Architectures.Model
{
   internal class LayeredApplicationModelImmutable : ApplicationModelSkeletonImmutable<LayeredApplication>, LayeredApplicationModel
   {

      private readonly ListProxy<LayerModelMutable> _topLevelLayers;
      private readonly ListProxy<LayerModelMutable> _allLayers;

      internal LayeredApplicationModelImmutable(
         LayeredArchitecture architecture,
         Type genericCompositePropertyMixin,
         Type genericCompositeEventMixin,
         Type genericFragmentBaseType,
         DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> modelTypeAssemblyScopeSupport,
         out DictionaryQuery<CompositeModelType, CompositeModelTypeModelScopeSupport> compositeModelTypeSupport,
         out DictionaryProxy<Int32, CompositeModel> models,
         out ListProxy<LayerModelMutable> topLevelLayers,
         out ListProxy<LayerModelMutable> allLayers
         )
         : base( architecture, genericCompositePropertyMixin, genericCompositeEventMixin, genericFragmentBaseType, modelTypeAssemblyScopeSupport, out compositeModelTypeSupport, out models )
      {
         this._topLevelLayers = this.CollectionsFactory.NewListProxy<LayerModelMutable>();
         this._allLayers = this.CollectionsFactory.NewListProxy<LayerModelMutable>();

         topLevelLayers = this._topLevelLayers;
         allLayers = this._allLayers;
      }

      protected override LayeredApplication CreateNew( ApplicationValidationResultIQ validationResult, String name, String mode, String version, DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> loadingResults )
      {
         return new LayeredApplicationImpl( this, this.CompositeModelTypeModelScopeSupports.Select( kvp => Tuple.Create( kvp.Key, (CompositeModelTypeModelScopeSupport) kvp.Value ) ), validationResult, loadingResults, name, mode, version );
      }

      #region LayeredApplicationModel Members

      public IEnumerable<LayerModel> TopLevelLayers
      {
         get
         {
            return this._topLevelLayers.CQ.Select( model => model.Immutable );
         }
      }

      public ModuleModel FindModuleModel( CompositeModel compositeModel )
      {
         return this._allLayers.MQ
            .SelectMany( layer => layer.InternalModuleModels )
            .Where( module => module.IQ.CompositeModelsInThisContainer.Contains( compositeModel ) )
            .Select( module => module.IQ )
            .FirstOrDefault();
      }

      #endregion

      internal IEnumerable<LayerModel> AllLayers
      {
         get
         {
            return this._allLayers.CQ.Select( model => model.Immutable );
         }
      }

      protected override void ValidateApplicationModel( ApplicationValidationResultMutable result )
      {
         // Detect cyclic layer dependencies
         // Perform DFS for each layer
         var violatingLayers = new HashSet<LayerModel>();

         foreach ( var layerModel in this.AllLayers )
         {
            var seen = new HashSet<LayerModel>();
            var stk = new Stack<LayerModel>();

            stk.Push( layerModel );
            while ( stk.Any() )
            {
               LayerModel current = stk.Pop();
               seen.Add( current );
               var allUsed = current.UsedLayerModels;
               foreach ( LayerModel used in allUsed )
               {
                  if ( seen.Contains( used ) )
                  {
                     // Loop detected
                     violatingLayers.Add( layerModel );
                     stk.Clear();
                     break;
                  }
                  else
                  {
                     stk.Push( used );
                  }
               }
               if ( !allUsed.Any() )
               {
                  seen.Remove( current );
               }
            }
         }

         foreach ( var violatingLayer in violatingLayers )
         {
            result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The following layer has cyclic dependencies: " + violatingLayer.LayerName + ".", this ) );
         }
      }
   }

   internal static class LayeredApplicationModelUtils
   {
      //public static CompositeModel FindFirstVisibleCompositeModel( ModuleModel startingModule, CompositeModelType compositeModelType, Type matchingCompositeType )
      //{
      //   return SearchVisibleModels(
      //       model => model.ModelType.Equals( compositeModelType ) && model.PublicTypes.Any( pType => TypeUtils.TypeUtil.IsAssignableFrom( matchingCompositeType, pType ) ),
      //       ( moduleModel, compositeModel ) => compositeModel,
      //       startingModule
      //      ).FirstOrDefault();
      //}

      public static IEnumerable<ResultType> SearchVisibleModels<ResultType>(
         Func<CompositeModel, Boolean> matcher,
         Func<ModuleModel, CompositeModel, ResultType> transformer,
         ModuleModel startingModule
         )
      {
         return SearchModule( matcher, transformer, startingModule, Visibility.MODULE )
            .Concat( SearchLayer( matcher, transformer, startingModule.LayerModel, startingModule, Visibility.LAYER ) )
            .Concat( SearchAllUsedLayers( matcher, transformer, startingModule.LayerModel, Visibility.APPLICATION ) );
      }

      public static IEnumerable<ResultType> SearchAllUsedLayers<ResultType>(
         Func<CompositeModel, Boolean> matcher,
         Func<ModuleModel, CompositeModel, ResultType> transformer,
         LayerModel layerModel,
         Visibility acceptedVisibility
         )
      {
         // Using AsParallel() here actually slows things down as this is called from within verification, which is run in parallel by itself
         return layerModel.UsedLayerModels.SelectMany( used =>
            SearchLayer( matcher, transformer, used, null, acceptedVisibility )
            .Concat( SearchAllUsedLayers( matcher, transformer, used, acceptedVisibility ) )
            );
      }

      public static IEnumerable<ResultType> SearchLayer<ResultType>(
         Func<CompositeModel, Boolean> matcher,
         Func<ModuleModel, CompositeModel, ResultType> transformer,
         LayerModel layerModel,
         ModuleModel moduleToExclude,
         Visibility acceptedVisibility
         )
      {
         var modules = layerModel.ModuleModels;
         if ( moduleToExclude != null )
         {
            modules = modules.Where( module => !Object.ReferenceEquals( module, moduleToExclude ) );
         }
         // Using AsParallel() here actually slows things down as this is called from within verification, which is run in parallel by itself
         return modules.SelectMany( moduleModel => SearchModule( matcher, transformer, moduleModel, acceptedVisibility ) );
      }

      public static IEnumerable<ResultType> SearchModule<ResultType>(
         Func<CompositeModel, Boolean> matcher,
         Func<ModuleModel, CompositeModel, ResultType> transformer,
         ModuleModel moduleModel,
         Visibility acceptedVisibility
         )
      {
         // Using AsParallel() here actually slows things down as this is called from within verification, which is run in parallel by itself
         return moduleModel.CompositeModelsInThisContainer
            .Where( model => moduleModel.GetVisibility( model ) >= acceptedVisibility && matcher( model ) )
            .Select( model => transformer( moduleModel, model ) );
      }
   }
}
