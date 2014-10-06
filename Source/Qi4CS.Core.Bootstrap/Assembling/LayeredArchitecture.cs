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
using Qi4CS.Core.Bootstrap.Model;

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This kind of Qi4CS application architecture exposes structure of <see cref="LayerArchitecture"/>, each layer containing <see cref="ModuleArchitecture"/>s.
   /// Each <see cref="ModuleArchitecture"/> acts as a container for various composites.
   /// Each <see cref="LayerArchitecture"/> may be told to use (<see cref="LayerArchitecture.UseLayers(LayerArchitecture[])"/> method) other <see cref="LayerArchitecture"/>s, conceptually positioning the using <see cref="LayerArchitecture"/> above the used <see cref="LayerArchitecture"/>.
   /// Usage of layers determines how composites are visible to other layers, see <see cref="Assembling.Visibility"/> for more information.
   /// </summary>
   /// <remarks>
   /// The instances of this type may be created via <see cref="M:Qi4CS.Core.Architectures.Assembling.Qi4CSArchitectureFactory.NewLayeredArchitecture(Qi4CS.Core.Runtime.Assembling.CompositeModelTypeAssemblyScopeSupport[])"/>.
   /// </remarks>
   public interface LayeredArchitecture : ApplicationArchitecture<LayeredApplicationModel>
   {
      /// <summary>
      /// Gets existing or creates a new <see cref="LayerArchitecture"/> with given name.
      /// This implies that the names of <see cref="LayerArchitecture"/>s contained by this <see cref="LayeredArchitecture"/> will be unique within this <see cref="LayeredArchitecture"/>.
      /// </summary>
      /// <param name="name">The name of the <see cref="LayerArchitecture"/>.</param>
      /// <returns>An existing or new instance of <see cref="LayerArchitecture"/> with its <see cref="LayerArchitecture.Name"/> being <paramref name="name"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <c>null</c>.</exception>
      LayerArchitecture GetOrCreateLayer( String name );
   }
}
