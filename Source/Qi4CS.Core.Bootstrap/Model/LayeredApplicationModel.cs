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
using System.Collections.Generic;
using Qi4CS.Core.Bootstrap.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Bootstrap.Model
{
   /// <summary>
   /// This is model-level structural construct of <see cref="Assembling.LayeredArchitecture"/>.
   /// It extends <see cref="ApplicationModel{T}"/> to add methods specific for layered architecture.
   /// </summary>
   public interface LayeredApplicationModel : ApplicationModel<LayeredApplication>
   {
      /// <summary>
      /// Gets all the top-level <see cref="LayerModel"/>s of this <see cref="LayeredApplicationModel"/>.
      /// </summary>
      /// <value>All the top-level <see cref="LayerModel"/>s of this <see cref="LayeredApplicationModel"/>.</value>
      /// <remarks>
      /// The layer is considred to be "top-level" if there are no other layers in the same application model that use that layer.
      /// </remarks>
      IEnumerable<LayerModel> TopLevelLayers { get; }

      /// <summary>
      /// Finds the <see cref="ModuleModel"/> which contains given <see cref="CompositeModel"/>.
      /// </summary>
      /// <param name="compositeModel">The <see cref="CompositeModel"/>.</param>
      /// <returns>The <see cref="ModuleModel"/> containing <paramref name="compositeModel"/>, or <c>null</c> if no suitable <see cref="ModuleModel"/> is found.</returns>
      ModuleModel FindModuleModel( CompositeModel compositeModel );
   }
}
