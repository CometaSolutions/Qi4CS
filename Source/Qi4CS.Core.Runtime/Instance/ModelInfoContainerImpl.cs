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
using System.Reflection;
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Instance
{
   public class ModelInfoContainerImpl : ModelInfoContainer
   {
      public const String DYNAMIC_ASSEMBLY_NAME = "DynamicAssembly";
      public const String MODULE_NAME_POSTFIX = ".module";

      private readonly DictionaryQuery<CompositeModelType, SetQuery<CompositeModelInfo>> _compositeModels;

      public ModelInfoContainerImpl(
         DictionaryQuery<CompositeModelType, CompositeModelTypeInstanceScopeSupport> modelSupport,
         SetQuery<CompositeModel> models,
         ApplicationValidationResultIQ validationResult,
         DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> loadingResults,
         CollectionsFactory collectionsFactory,
         Type genericPropertyMixinType,
         Type genericEventMixinType
         )
      {
         ArgumentValidator.ValidateNotNull( "Model support", modelSupport );
         ArgumentValidator.ValidateNotNull( "Models", models );
         ArgumentValidator.ValidateNotNull( "Validation result", validationResult );

         PublicCompositeTypeGenerationResult[] typeGenResults = new PublicCompositeTypeGenerationResult[models.Count];
         Int32 gIdx = 0;
         foreach ( CompositeModel model in models )
         {
            typeGenResults[gIdx] = loadingResults[model];
            ++gIdx;
         }

         this._compositeModels = collectionsFactory.NewDictionaryProxy<CompositeModelType, SetQuery<CompositeModelInfo>>(
            models
               .Select( model => model.ModelType )
               .Distinct()
               .ToDictionary(
                  modelType => modelType,
                  modelType2 =>
                     collectionsFactory.NewSetProxy<CompositeModelInfo>( new HashSet<CompositeModelInfo>( models
                        .Select( ( modelInner, idx ) => new KeyValuePair<CompositeModel, PublicCompositeTypeGenerationResult>( modelInner, typeGenResults[idx] ) )
                        .Where( kvp => kvp.Key.ModelType.Equals( modelType2 ) )
                        .Select( kvp => new ModelInfoImpl( kvp.Key, kvp.Value, collectionsFactory ) ),
                        ReferenceEqualityComparer<CompositeModelInfo>.ReferenceBasedComparer ) ).CQ
                )
            ).CQ;
      }

      #region CompositeModelInfoContainer Members

      public CompositeModelInfo GetCompositeModelInfo( CompositeModelType modelType, IEnumerable<Type> compositeTypes )
      {
         ArgumentValidator.ValidateNotNull( "Composite model type", modelType );
         ArgumentValidator.ValidateNotNull( "Composite types", compositeTypes );

         CompositeModelInfo result = null;
         SetQuery<CompositeModelInfo> dic = null;
         if ( this._compositeModels.TryGetValue( modelType, out dic ) )
         {
            result = this.ResolveModelInfo( dic, compositeTypes );
         }
         return result;
      }

      public CompositeModelInfo GetCompositeModelInfo( CompositeModel model )
      {
         ArgumentValidator.ValidateNotNull( "Composite model", model );

         SetQuery<CompositeModelInfo> set;
         CompositeModelInfo result;
         if ( this._compositeModels.TryGetValue( model.ModelType, out set ) )
         {
            result = set.FirstOrDefault( info => Object.ReferenceEquals( info.Model, model ) );
         }
         else
         {
            result = null;
         }

         if ( result == null )
         {
            throw new ArgumentException( "Could not get composite model info for " + model + "." );
         }
         return result;
      }

      #endregion

      protected CompositeModelInfo ResolveModelInfo(
         SetQuery<CompositeModelInfo> set,
         IEnumerable<Type> types
         )
      {
         CompositeModelInfo result = null;
         CompositeModelInfo[] modelInfos = set.Where( kvp => types.All( type => kvp.Model.PublicTypes.Any( pType => type.IsAssignableFrom( pType ) || ( type.IsGenericType() && pType.Equals( type.GetGenericTypeDefinition() ) ) ) ) ).Select( kvp => kvp ).ToArray();
         if ( modelInfos.Length > 0 )
         {
            if ( modelInfos.Length == 1 )
            {
               result = modelInfos[0];
            }
            else
            {
               result = modelInfos.FirstOrDefault( info => types.All( type => info.Model.PublicTypes.Contains( type ) ) );
               if ( result == null )
               {
                  throw new AmbiguousTypeException( types, modelInfos.Select( info => info.Model.PublicTypes ) );
               }
            }
         }
         if ( result == null )
         {
            throw new NoSuchCompositeTypeException( types );
         }
         return result;
      }
   }
}
