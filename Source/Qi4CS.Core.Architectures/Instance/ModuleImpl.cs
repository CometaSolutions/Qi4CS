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
using Qi4CS.Core.Architectures.Model;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Runtime.Common;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Architectures.Instance
{
   internal class ModuleImpl : Module
   {
      private readonly LayerImpl _layer;
      private readonly ServiceContainer _serviceContainer;
      private readonly StructureServiceProviderSPI _serviceProvider;
      private readonly ModelInfoContainer _modelInfoContainer;
      private readonly ModuleModel _moduleModel;
#if SILVERLIGHT
      private readonly IDictionary<CompositeModelType, IDictionary<Type[], Tuple<CompositeInstanceStructureOwner, CompositeModel>>> _compositeTypeLookupCache;
#else
      private readonly System.Collections.Concurrent.ConcurrentDictionary<CompositeModelType, System.Collections.Concurrent.ConcurrentDictionary<Type[], Tuple<CompositeInstanceStructureOwner, CompositeModel>>> _compositeTypeLookupCache;
#endif

      internal ModuleImpl( ApplicationValidationResultIQ validationResult, DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> loadingResults, LayerImpl layer, ModuleModel model )
      {
         ArgumentValidator.ValidateNotNull( "Layer instance", layer );
         ArgumentValidator.ValidateNotNull( "Module model", model );

         this._layer = layer;
         this._moduleModel = model;
         this._serviceContainer = new ThreadsafeServiceContainer();
         this._modelInfoContainer = new ModelInfoContainerImpl(
            layer.InternalApplication.ModelTypeSupportInternal,
            model.CompositeModelsInThisContainer,
            validationResult,
            loadingResults,
            layer.Application.CollectionsFactory,
            model.LayerModel.ApplicationModel.GenericPropertyMixinType,
            model.LayerModel.ApplicationModel.GenericEventMixinType
            );
#if SILVERLIGHT
         this._compositeTypeLookupCache = new Dictionary<CompositeModelType, IDictionary<Type[], Tuple<CompositeInstanceStructureOwner, CompositeModel>>>();
#else
         this._compositeTypeLookupCache = new System.Collections.Concurrent.ConcurrentDictionary<CompositeModelType, System.Collections.Concurrent.ConcurrentDictionary<Type[], Tuple<CompositeInstanceStructureOwner, CompositeModel>>>();
#endif

         this._serviceProvider = new StructureServiceProviderImpl(
            this,
            layer.InternalApplication.ModelTypeSupportInternal,
            matcher => this.FindVisibleModels( matcher )
            );
      }

      #region ModuleInstance Members

      public Layer Layer
      {
         get
         {
            return this._layer;
         }
      }

      public StructureServiceProviderSPI StructureServices
      {
         get
         {
            return this._serviceProvider;
         }
      }

      public ModuleModel ModuleModel
      {
         get
         {
            return this._moduleModel;
         }
      }

      #endregion


      #region CompositeInstanceStructureOwner Members

      public ApplicationSPI Application
      {
         get
         {
            return this.Layer.Application;
         }
      }

      public ModelInfoContainer ModelInfoContainer
      {
         get
         {
            return this._modelInfoContainer;
         }
      }

      #endregion

      internal Tuple<CompositeInstanceStructureOwner, CompositeModel> FindModelForBuilder( CompositeModelType compositeModelType, Type[] compositeTypes )
      {
         // TODO might get rid of tuple? if lookup is done by target module
#if SILVERLIGHT
         var cache = this._compositeTypeLookupCache.GetOrAdd_WithLock( compositeModelType, key => new Dictionary<Type[], Tuple<CompositeInstanceStructureOwner, CompositeModel>>( ArrayEqualityComparer<Type>.DefaultArrayEqualityComparer ) );
#else
         var cache = this._compositeTypeLookupCache.GetOrAdd( compositeModelType, key => new System.Collections.Concurrent.ConcurrentDictionary<Type[], Tuple<CompositeInstanceStructureOwner, CompositeModel>>( ArrayEqualityComparer<Type>.DefaultArrayEqualityComparer ) );
#endif

         return cache.
#if SILVERLIGHT
GetOrAdd_WithLock(
#else
GetOrAdd(
#endif
 compositeTypes, ( compositeTypez ) => // lambda will need composite model type -> can't fully optimize
            {
               var models = this.FindVisibleModels( compositeModel =>
                  compositeModel.ModelType.Equals( compositeModelType )
                  && compositeTypes.All( cType => compositeModel.PublicTypes.Any( pType => cType.GetGenericDefinitionIfGenericType().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( pType ) ) )
                  );

               var array = models.Take( 2 ).ToArray();
               if ( array.Length > 1 )
               {
                  // This can happen when same layer is reachable via two other layers, e.g.
                  //  A  
                  // / \
                  // B C
                  // \ /
                  //  D
                  // If one makes search from module A for composite type present in module D in one composite model, one will get two references to same composite model in D
                  array = models.Distinct().Take( 2 ).ToArray();
               }

               if ( array.Length == 0 )
               {
                  throw new NoSuchCompositeTypeException( compositeTypes );
               }
               else
               {
                  if ( array.Length > 1 )
                  {
                     throw new AmbiguousTypeException( compositeTypes, models.Select( tuple => tuple.Item2.PublicTypes ) );
                  }
                  else
                  {
                     // Try to search for model info. If the composite type is ambiguous in the scope of that module, the exception will be thrown.
                     //( (ModuleInstance) result.Item1 ).ModelInfoContainer.GetCompositeModelInfo( compositeModelType, compositeType );
                     return array[0];
                  }
               }
            } );
      }

      internal IEnumerable<Tuple<CompositeInstanceStructureOwner, CompositeModel>> FindVisibleModels( Func<CompositeModel, Boolean> matcher )
      {
         return LayeredApplicationModelUtils.SearchVisibleModels(
            matcher,
            ( moduleModel, compositeModel ) => Tuple.Create<CompositeInstanceStructureOwner, CompositeModel>( this._layer.InternalApplication.GetInstanceForModel( moduleModel.LayerModel ).GetInstanceForModel( moduleModel ), compositeModel ),
            this._moduleModel
            );
      }

      internal LayerImpl InternalLayer
      {
         get
         {
            return this._layer;
         }
      }

      public override String ToString()
      {
         return this._moduleModel.ToString();
      }

      internal static ServiceContainer GetServiceContainerFor( CompositeInstanceStructureOwner structureOwner )
      {
         return ( (ModuleImpl) structureOwner )._serviceContainer;
      }

      internal static CompositeBuilder CreateBuilder( CompositeModelTypeInstanceScopeSupport modelSupport, CompositeInstanceStructureOwner structureOwner, CompositeModelType compositeModelType, IEnumerable<Type> compositeTypes )
      {
         var compositeTypesArr = compositeTypes.ToArray();
         var tuple = ( (ModuleImpl) structureOwner ).FindModelForBuilder( compositeModelType, compositeTypesArr );
         var uses = UsesContainerMutableImpl.CreateWithParent( tuple.Item2.UsesContainer );
         return new CompositeBuilderImpl( compositeTypesArr, uses, (CompositeInstanceImpl) modelSupport.CreateInstance( tuple.Item1, tuple.Item2, compositeTypesArr, uses.Query ) );
      }
   }
}
