/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Architectures.Instance
{
   internal sealed class LayeredPlainCompositeModelTypeInstanceScopeSupport : AbstractPlainModelTypeInstanceScopeSupport
   {
      public LayeredPlainCompositeModelTypeInstanceScopeSupport( AbstractPlainCompositeModelTypeModelScopeSupport modelScopeSupport )
         : base( modelScopeSupport )
      {

      }

      public override CompositeBuilder CreateBuilder( StructureServiceProviderSPI structureServiceProvider, Type[] compositeTypes )
      {
         return ModuleImpl.CreateBuilder( this, structureServiceProvider.Structure, this.ModelScopeSupport.AssemblyScopeSupport.ModelType, compositeTypes );
      }

   }

   internal sealed class LayeredServiceModelTypeInstanceScopeSupport : AbstractServiceModelTypeInstanceScopeSupport
   {
      public LayeredServiceModelTypeInstanceScopeSupport( AbstractServiceModelTypeModelScopeSupport modelScopeSupport )
         : base( modelScopeSupport )
      {

      }

      protected override ServiceContainer GetServiceContainerFor( CompositeInstanceStructureOwner structureOwner )
      {
         return ModuleImpl.GetServiceContainerFor( structureOwner );
      }


      protected override IEnumerable<CompositeInstanceStructureOwner> GetAllStructureOwners( ApplicationSPI application, Boolean isActivation )
      {
         // Perform BFS on dependency hierarchy
         var result = ( (LayeredApplicationModel) application.ApplicationModel ).TopLevelLayers.SelectMany( layerModel => layerModel.AsBreadthFirstEnumerable( model => model.UsedLayerModels ) );

         if ( isActivation )
         {
            result = result.Reverse();
         }

         return result
            .Distinct()
            .Select( layer => ( (LayeredApplicationImpl) application ).GetInstanceForModel( layer ) )
            .SelectMany( layerInstance => layerInstance.LayerModel.ModuleModels.Select( moduleModel => layerInstance.GetInstanceForModel( moduleModel ) ) );
      }

      protected override IEnumerable<CompositeModel> GetAllModels( CompositeInstanceStructureOwner structureOwner, Boolean isActivation )
      {
         return ( (Module) structureOwner ).ModuleModel.CompositeModelsInThisContainer;
      }
   }

   //internal sealed class LayeredValueModelTypeInstanceScopeSupport : AbstractValueModelTypeInstanceScopeSupport
   //{
   //   public LayeredValueModelTypeInstanceScopeSupport( AbstractValueModelTypeModelScopeSupport modelScopeSupport )
   //      : base( modelScopeSupport )
   //   {

   //   }

   //   public override CompositeBuilder CreateBuilder( StructureServiceProviderSPI structureServiceProvider, Type[] compositeTypes )
   //   {
   //      return ModuleImpl.CreateBuilder( this, structureServiceProvider.Structure, this.ModelScopeSupport.AssemblyScopeSupport.ModelType, compositeTypes );
   //   }
   //}
}
