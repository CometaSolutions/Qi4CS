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
using System.Linq;
using CollectionsWithRoles.API;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Architectures.Instance
{
   internal class LayerImpl : Layer
   {
      private readonly DictionaryQuery<ModuleModel, Module> _modules;
      private readonly LayeredApplicationImpl _application;
      private readonly LayerModel _model;

      internal LayerImpl( ApplicationValidationResultIQ validationResult, DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> loadingResults, LayeredApplicationImpl application, LayerModel layerModel )
      {
         this._application = application;
         this._model = layerModel;
         this._modules = layerModel.ApplicationModel.CollectionsFactory.NewDictionaryProxy<ModuleModel, Module>(
            layerModel.ModuleModels.ToDictionary( model => model, model => (Module) new ModuleImpl( validationResult, loadingResults, this, model ) )
            ).CQ;

      }

      #region LayerInstance Members

      public LayeredApplication Application
      {
         get
         {
            return this._application;
         }
      }

      public LayerModel LayerModel
      {
         get
         {
            return this._model;
         }
      }

      #endregion

      internal Module GetInstanceForModel( ModuleModel model )
      {
         return this._modules[model];
      }

      internal LayeredApplicationImpl InternalApplication
      {
         get
         {
            return this._application;
         }
      }

      public override String ToString()
      {
         return this._model.ToString();
      }
   }
}
