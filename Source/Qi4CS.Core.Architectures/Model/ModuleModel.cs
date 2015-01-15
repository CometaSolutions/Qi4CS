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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Architectures.Assembling;
using Qi4CS.Core.Architectures.Common;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Architectures.Model
{
   internal class ModuleModelMutable : Mutable<ModuleModelMutable, ModuleModelImmutable>, MutableQuery<ModuleModelImmutable>
   {
      private readonly ModuleModelState _state;
      private readonly ModuleModelImmutable _immutable;

      public ModuleModelMutable( ModuleModelState state, ModuleModelImmutable immutable )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         ArgumentValidator.ValidateNotNull( "Immutable", immutable );

         this._state = state;
         this._immutable = immutable;
      }

      public ModuleModelImmutable Immutable
      {
         get
         {
            return this._immutable;
         }
      }

      #region MutableQuery<ModuleModel> Members

      public ModuleModelImmutable IQ
      {
         get
         {
            return this._immutable;
         }
      }

      #endregion

      #region Mutable<ModuleModelMutable,ModuleModel> Members

      public ModuleModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      public override String ToString()
      {
         return this._state.ToString();
      }

      internal LayeredCompositeAssembler Assembler
      {
         get
         {
            return this._state.Assembler;
         }
      }
   }

   internal class ModuleModelImmutable : ModuleModel
   {
      private readonly ModuleModelState _state;

      public ModuleModelImmutable( ModuleModelState state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      #region ModuleModel Members

      public LayerModel LayerModel
      {
         get
         {
            return this._state.LayerModel.Immutable;
         }
      }

      public SetQuery<SPI.Model.CompositeModel> CompositeModelsInThisContainer
      {
         get
         {
            return this._state.CompositeModels.MQ.IQ;
         }
      }

      public String Name
      {
         get
         {
            return this._state.ModuleName;
         }
      }

      public Visibility GetVisibility( CompositeModel model )
      {
         ArgumentValidator.ValidateNotNull( "Composite model", model );

         IDictionary<CompositeModel, Visibility> dic;
         Visibility vis;
         if ( this._state.CompositeVisibilities.TryGetValue( model.ModelType, out dic )
            && dic.TryGetValue( model, out vis ) )
         {
            return vis;
         }
         else
         {
            throw new ArgumentException( "Given composite model " + model + " does not belong to module " + this + "." );
         }
      }

      #endregion

      public override Int32 GetHashCode()
      {
         return this._state.ModuleName.GetHashCodeSafe();
      }

      public override Boolean Equals( Object obj )
      {
         return base.Equals( obj );
      }

      internal Boolean AssemblerMatches( LayeredCompositeAssemblerImpl assembler )
      {
         return Object.ReferenceEquals( this._state.Assembler, assembler );
      }

      public override String ToString()
      {
         return this._state.ToString();
      }

      internal LayeredCompositeAssembler Assembler
      {
         get
         {
            return this._state.Assembler;
         }
      }
   }

   internal class ModuleModelState : StateWithAssembler<LayeredCompositeAssemblerImpl>
   {
      private LayerModelMutable _layerModel;
      private readonly SetProxy<CompositeModel> _compositeModels;
      private String _moduleName;
      private readonly IDictionary<CompositeModelType, IDictionary<CompositeModel, Visibility>> _compositeVisibilities;
      private LayeredCompositeAssemblerImpl _assembler;

      public ModuleModelState( CollectionsFactory collectionsFactory )
      {
         this._compositeModels = collectionsFactory.NewSetProxy<CompositeModel>();
         this._compositeVisibilities = new Dictionary<CompositeModelType, IDictionary<CompositeModel, Visibility>>();
      }

      public LayerModelMutable LayerModel
      {
         get
         {
            return this._layerModel;
         }
         set
         {
            this._layerModel = value;
         }
      }

      public String ModuleName
      {
         get
         {
            return this._moduleName;
         }
         set
         {
            this._moduleName = value;
         }
      }

      public LayeredCompositeAssemblerImpl Assembler
      {
         get
         {
            return this._assembler;
         }
         set
         {
            this._assembler = value;
         }
      }

      public SetProxy<CompositeModel> CompositeModels
      {
         get
         {
            return this._compositeModels;
         }
      }

      public IDictionary<CompositeModelType, IDictionary<CompositeModel, Visibility>> CompositeVisibilities
      {
         get
         {
            return this._compositeVisibilities;
         }
      }

      public override String ToString()
      {
         return this._moduleName;
      }
   }
}
