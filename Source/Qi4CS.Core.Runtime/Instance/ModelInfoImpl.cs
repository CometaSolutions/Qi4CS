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
using Qi4CS.Core.Runtime.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Instance
{
   public class ModelInfoImpl : CompositeModelInfo
   {
      private readonly CompositeModel _model;
      private readonly PublicCompositeTypeGenerationResult _types;
      private readonly ListQuery<AttributeHolder> _compositeMethodAttributeHolders;

      public ModelInfoImpl(
         CompositeModel model,
         PublicCompositeTypeGenerationResult types,
         CollectionsFactory collectionsFactory
         )
      {
         this._model = model;
         this._types = types;
         this._compositeMethodAttributeHolders = collectionsFactory.NewListProxy(
            model.Methods
               .Select( cMethod => (AttributeHolder) new AttributeHolderImpl( collectionsFactory, cMethod.GetAllMethodModels().Select( mModel => mModel.NativeInfo ) ) )
               .ToList()
            ).CQ;
      }

      #region ModelInfo Members

      public CompositeModel Model
      {
         get
         {
            return this._model;
         }
      }

      public PublicCompositeTypeGenerationResult Types
      {
         get
         {
            return this._types;
         }
      }

      public ListQuery<AttributeHolder> CompositeMethodAttributeHolders
      {
         get
         {
            return this._compositeMethodAttributeHolders;
         }
      }

      #endregion

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) || ( obj is ModelInfoImpl && this._types.Equals( ( ( (ModelInfoImpl) obj )._types ) ) );
      }

      public override Int32 GetHashCode()
      {
         return this._types.GetHashCode();
      }
   }
}
