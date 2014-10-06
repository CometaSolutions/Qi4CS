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
using Qi4CS.Core.Architectures.Assembling;
using Qi4CS.Core.Architectures.Model;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Architectures.Instance
{
   internal class LayeredApplicationImpl : ApplicationSkeleton, LayeredApplication
   {
      private readonly DictionaryQuery<LayerModel, LayerImpl> _allLayers;

      internal LayeredApplicationImpl(
         LayeredApplicationModel model,
         IEnumerable<Tuple<CompositeModelType, CompositeModelTypeModelScopeSupport>> compositeModelTypeSupport,
         ApplicationValidationResultIQ validationResult,
         DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> loadingResults,
         String name,
         String mode,
         String version
         )
         : base(
         model,
         compositeModelTypeSupport,
         name,
         mode,
         version
         )
      {
         DictionaryProxy<LayerModel, LayerImpl> allLayers = model.CollectionsFactory.NewDictionaryProxy<LayerModel, LayerImpl>();
         foreach ( LayerModel layerModel in model.TopLevelLayers )
         {
            this.ProcessLayers( validationResult, loadingResults, allLayers, layerModel );
         }

         this._allLayers = allLayers.CQ;
      }

      private void ProcessLayers( ApplicationValidationResultIQ validationResult, DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> loadingResults, DictionaryProxy<LayerModel, LayerImpl> layers, LayerModel layerModel )
      {
         if ( !layers.CQ.ContainsKey( layerModel ) )
         {
            layers.Add( layerModel, new LayerImpl( validationResult, loadingResults, this, layerModel ) );
         }

         foreach ( LayerModel used in layerModel.UsedLayerModels )
         {
            this.ProcessLayers( validationResult, loadingResults, layers, used );
         }
      }

      internal DictionaryQuery<CompositeModelType, CompositeModelTypeInstanceScopeSupport> ModelTypeSupportInternal
      {
         get
         {
            return this.ModelTypeSupport;
         }
      }

      internal LayerImpl GetInstanceForModel( LayerModel model )
      {
         return this._allLayers[model];
      }

      #region LayeredApplication Members

      public Layer FindLayer( String layerName )
      {
         return layerName == null ? null : this._allLayers.Values.FirstOrDefault( layer => String.Equals( layer.LayerModel.LayerName, layerName ) );
      }

      public Module FindModule( String layerName, String moduleName )
      {
         Module result;
         var layer = this.FindLayer( layerName );
         if ( layer == null || moduleName == null )
         {
            result = null;
         }
         else
         {
            result = layer.LayerModel.ModuleModels
#if !WP8_BUILD
.AsParallel() // TODO - is this really necessary?
#endif
.Where( moduleModel => String.Equals( moduleModel.Name, moduleName ) )
               .Select( moduleModel => ( (LayerImpl) layer ).GetInstanceForModel( moduleModel ) )
               .FirstOrDefault();
         }
         return result;
      }

      #endregion

      internal Module FindModule( LayeredCompositeAssemblerImpl assembler )
      {
         return this._allLayers
#if !WP8_BUILD
.AsParallel() // TODO - is this really necessary?
#endif
.SelectMany( layer => layer.Key.ModuleModels.Select( muudel => Tuple.Create( muudel, layer.Value.GetInstanceForModel( muudel ) ) ) )
            .Where( tuple => ( (ModuleModelImmutable) tuple.Item1 ).AssemblerMatches( assembler ) )
            .Select( tuple => tuple.Item2 )
            .FirstOrDefault();
      }

      internal LayeredApplicationModel InternalModel
      {
         get
         {
            return (LayeredApplicationModel) this.ApplicationModel;
         }
      }
   }
}
