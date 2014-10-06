/*
 * Copyright (c) 2008, Rickard Öberg.
 * See NOTICE file.
 * 
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
using Qi4CS.Core.SPI.Model;


namespace Qi4CS.Core.Bootstrap.Model
{
   /// <summary>
   /// This is model-level structural construct of <see cref="Assembling.LayerArchitecture"/>.
   /// </summary>
   public interface LayerModel : ValidatableItem
   {
      /// <summary>
      /// Gets the <see cref="LayeredApplicationModel"/> that this <see cref="LayerModel"/> belongs to.
      /// </summary>
      /// <value>The <see cref="LayeredApplicationModel"/> that this <see cref="LayerModel"/> belongs to.</value>
      LayeredApplicationModel ApplicationModel { get; }

      /// <summary>
      /// Gets all the <see cref="ModuleModel"/>s contained within this layer.
      /// </summary>
      /// <value>All the <see cref="ModuleModel"/>s contained within this layer.</value>
      IEnumerable<ModuleModel> ModuleModels { get; }

      /// <summary>
      /// Gets all the <see cref="LayerModel"/>s used by this <see cref="LayerModel"/>.
      /// </summary>
      /// <value>All the <see cref="LayerModel"/>s used by this <see cref="LayerModel"/>.</value>
      IEnumerable<LayerModel> UsedLayerModels { get; }

      /// <summary>
      /// Gets the name of this layer.
      /// </summary>
      /// <value>The name of this layer.</value>
      String LayerName { get; }
   }
}
