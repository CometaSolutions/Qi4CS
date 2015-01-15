/*
 * Copyright 2011 Stanislav Muhametsin. All rights Reserved.
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
using CommonUtils;
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
   internal class SingletonApplicationModelImmutable : ApplicationModelSkeletonImmutable<SingletonApplication>, SingletonApplicationModel
   {
      private readonly SetProxy<CompositeModel> _compositeModels;
      private readonly Assembler _assembler;

      internal SingletonApplicationModelImmutable(
         SingletonArchitecture architecture,
         Type genericCompositePropertyMixin,
         Type genericCompositeEventMixin,
         Type genericFragmentBaseType,
         DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> modelTypeAssemblyScopeSupport,
         out DictionaryQuery<CompositeModelType, CompositeModelTypeModelScopeSupport> compositeModelTypeSupport,
         out DictionaryProxy<Int32, CompositeModel> models,
         out SetProxy<CompositeModel> modelsForContainer,
         Assembler assembler
         )
         : base( architecture, genericCompositePropertyMixin, genericCompositeEventMixin, genericFragmentBaseType, modelTypeAssemblyScopeSupport, out compositeModelTypeSupport, out models )
      {
         ArgumentValidator.ValidateNotNull( "Assembler", assembler );

         this._assembler = assembler;
         this._compositeModels = this.CollectionsFactory.NewSetProxy<CompositeModel>();

         modelsForContainer = this._compositeModels;
      }

      #region ModelContainer Members

      public SetQuery<CompositeModel> CompositeModelsInThisContainer
      {
         get
         {
            return this._compositeModels.CQ;
         }
      }

      #endregion

      protected override SingletonApplication CreateNew( ApplicationValidationResultIQ validationResult, String name, String mode, String version, DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> loadingResults )
      {
         return new SingletonApplicationImpl( this, this.CompositeModelTypeModelScopeSupports.Select( kvp => Tuple.Create( kvp.Key, kvp.Value ) ), validationResult, loadingResults, name, mode, version );
      }

      protected override void ValidateApplicationModel( ApplicationValidationResultMutable result )
      {
         // Nothing to do
      }

      internal Assembler Assembler
      {
         get
         {
            return this._assembler;
         }
      }
   }
}
