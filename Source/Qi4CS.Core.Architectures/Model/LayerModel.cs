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
using Qi4CS.Core.Bootstrap.Model;

namespace Qi4CS.Core.Architectures.Model
{
   internal class LayerModelMutable : Mutable<LayerModelMutable, LayerModelImmutable>, MutableQuery<LayerModelImmutable>
   {
      private readonly LayerModelState _state;
      private readonly LayerModelImmutable _immutable;

      internal LayerModelMutable( LayerModelState state, LayerModelImmutable immutable )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         ArgumentValidator.ValidateNotNull( "Immutable", immutable );

         this._state = state;
         this._immutable = immutable;
      }

      public LayerModelImmutable Immutable
      {
         get
         {
            return this._immutable;
         }
      }

      public void AddUsedLayers( IEnumerable<LayerModelMutable> usedLayers )
      {
         this._state.UsedLayers.AddRange( usedLayers );
      }

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) || ( obj is LayerModelMutable && this._immutable.Equals( ( (LayerModelMutable) obj ).Immutable ) );
      }

      public override Int32 GetHashCode()
      {
         return this._immutable.GetHashCode();
      }

      #region MutableQuery<LayerModel> Members

      public LayerModelImmutable IQ
      {
         get
         {
            return this._immutable;
         }
      }

      #endregion

      #region Mutable<LayerModelMutable,LayerModel> Members

      public LayerModelMutable MQ
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

      internal IEnumerable<ModuleModelMutable> InternalModuleModels
      {
         get
         {
            return this._state.ModuleModels.MQ;
         }
      }
   }

   internal class LayerModelImmutable : LayerModel
   {
      private readonly LayerModelState _state;

      internal LayerModelImmutable( LayerModelState state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      #region LayerModel Members

      public LayeredApplicationModel ApplicationModel
      {
         get
         {
            return this._state.ApplicationModel;
         }
      }

      public IEnumerable<ModuleModel> ModuleModels
      {
         get
         {
            return this._state.ModuleModels.MQ.IQ;
         }
      }

      public IEnumerable<LayerModel> UsedLayerModels
      {
         get
         {
            return this._state.UsedLayers.MQ.IQ;
         }
      }

      public String LayerName
      {
         get
         {
            return this._state.LayerName;
         }
      }

      #endregion

      public override Int32 GetHashCode()
      {
         return this._state.LayerName.GetHashCodeSafe();
      }

      public override Boolean Equals( Object obj )
      {
         return base.Equals( obj );
      }

      public override String ToString()
      {
         return this._state.ToString();
      }

      internal IEnumerable<ModuleModelImmutable> InternalModuleModels
      {
         get
         {
            return this._state.ModuleModels.MQ.IQ;
         }
      }
   }

   internal class LayerModelState
   {
      private LayeredApplicationModel _applicationModel;
      private readonly ListWithRoles<ModuleModelMutable, ModuleModelMutable, ModuleModelImmutable> _moduleModels;
      private readonly ListWithRoles<LayerModelMutable, LayerModelMutable, LayerModel> _usedLayers;
      private String _layerName;

      internal LayerModelState( CollectionsFactory collectionsFactory )
      {
         this._moduleModels = collectionsFactory.NewList<ModuleModelMutable, ModuleModelMutable, ModuleModelImmutable>();
         this._usedLayers = collectionsFactory.NewList<LayerModelMutable, LayerModelMutable, LayerModel>();
      }

      internal LayeredApplicationModel ApplicationModel
      {
         get
         {
            return this._applicationModel;
         }
         set
         {
            this._applicationModel = value;
         }
      }

      internal String LayerName
      {
         get
         {
            return this._layerName;
         }
         set
         {
            this._layerName = value;
         }
      }

      internal ListWithRoles<ModuleModelMutable, ModuleModelMutable, ModuleModelImmutable> ModuleModels
      {
         get
         {
            return this._moduleModels;
         }
      }

      internal ListWithRoles<LayerModelMutable, LayerModelMutable, LayerModel> UsedLayers
      {
         get
         {
            return this._usedLayers;
         }
      }

      public override String ToString()
      {
         return this._layerName;
      }
   }
}
