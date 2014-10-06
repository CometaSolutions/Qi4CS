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
using Qi4CS.Core.API.Common;

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This interface acts as a composite interface for <see cref="ModuleArchitecture"/>s.
   /// </summary>
   public interface LayerArchitecture : UsesProvider<LayerArchitecture>, DomainSpecificAssemblerAggregator<LayerArchitecture>
   {
      /// <summary>
      /// Gets the name of this <see cref="LayerArchitecture"/>.
      /// </summary>
      /// <value>The name of this <see cref="LayerArchitecture"/>.</value>
      String Name { get; }

      /// <summary>
      /// Gets the <see cref="LayeredArchitecture"/> this <see cref="LayerArchitecture"/> belongs to.
      /// </summary>
      /// <value>The <see cref="LayeredArchitecture"/> this <see cref="LayerArchitecture"/> belongs to.</value>
      LayeredArchitecture Architecture { get; }

      /// <summary>
      /// Adds other <see cref="LayerArchitecture"/>s from the same <see cref="LayeredArchitecture"/> to be used by this <see cref="LayerArchitecture"/>.
      /// </summary>
      /// <param name="layers">The <see cref="LayerArchitecture"/>s to use.</param>
      /// <remarks>
      /// If <paramref name="layers"/> is <c>null</c>, this method does nothing.
      /// Any <c>null</c> value in the <paramref name="layers"/> is ignored.
      /// </remarks>
      /// <exception cref="ArgumentException">If any element in <paramref name="layers"/> is non-<c>null</c> and belongs to different <see cref="LayeredArchitecture"/> than this <see cref="LayerArchitecture"/>.</exception>
      void UseLayers( params LayerArchitecture[] layers );

      /// <summary>
      /// Gets existing or creates a new <see cref="ModuleArchitecture"/> with given name.
      /// This implies that the names of <see cref="ModuleArchitecture"/>s contained by this <see cref="LayerArchitecture"/> will be unique within this <see cref="LayerArchitecture"/>.
      /// </summary>
      /// <param name="name">The name of the <see cref="ModuleArchitecture"/>.</param>
      /// <returns>An existing or new instance of <see cref="ModuleArchitecture"/> with its <see cref="ModuleArchitecture.Name"/> being <paramref name="name"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <c>null</c>.</exception>
      ModuleArchitecture GetOrCreateModule( String name );
   }
}
