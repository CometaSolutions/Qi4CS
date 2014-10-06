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
using System.Collections.Generic;
using System.Linq;
using CollectionsWithRoles.API;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Runtime.Common;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;


namespace Qi4CS.Core.Architectures.Instance
{
   internal class SingletonApplicationImpl : ApplicationSkeleton, SingletonApplication
   {

      private readonly StructureServiceProviderSPI _structureServiceProvider;

      private readonly ServiceContainer _serviceContainer;

      private readonly ModelInfoContainer _modelInfoContainer;


      internal SingletonApplicationImpl(
         SingletonApplicationModel applicationModel,
         IEnumerable<Tuple<CompositeModelType, CompositeModelTypeModelScopeSupport>> compositeModelTypeSupport,
         ApplicationValidationResultIQ validationResult,
         DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> loadingResults,
         String name,
         String mode,
         String version
         )
         : base(
         applicationModel,
         compositeModelTypeSupport,
         name,
         mode,
         version
         )
      {
         this._modelInfoContainer = new ModelInfoContainerImpl( this.InternalModelSupport, applicationModel.CompositeModelsInThisContainer, validationResult, loadingResults, this.CollectionsFactory, applicationModel.GenericPropertyMixinType, applicationModel.GenericEventMixinType );
         this._structureServiceProvider = new StructureServiceProviderImpl(
            this,
            this.ModelTypeSupport,
            matcher => applicationModel.CompositeModelsInThisContainer
               .Where( matcher )
               .Select( model => Tuple.Create<CompositeInstanceStructureOwner, CompositeModel>( this, model ) )
               );

         this._serviceContainer = new ThreadsafeServiceContainer();
      }

      #region SingletonApplication Members

      public StructureServiceProviderSPI StructureServices
      {
         get
         {
            return this._structureServiceProvider;
         }
      }

      #endregion

      #region CompositeInstanceStructureOwner Members

      public ApplicationSPI Application
      {
         get
         {
            return this;
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

      internal DictionaryQuery<CompositeModelType, CompositeModelTypeInstanceScopeSupport> InternalModelSupport
      {
         get
         {
            return this.ModelTypeSupport;
         }
      }

      internal static ServiceContainer GetServiceContainer( CompositeInstanceStructureOwner structureOwner )
      {
         return ( (SingletonApplicationImpl) structureOwner )._serviceContainer;
      }


   }
}
