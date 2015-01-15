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
   /// This interface acts a holder for <see cref="LayeredCompositeAssembler"/> that can be used to add composites, which will be logically contained within this <see cref="ModuleArchitecture"/>.
   /// </summary>
   public interface ModuleArchitecture : UsesProvider<ModuleArchitecture>, DomainSpecificAssemblerAggregator<LayeredCompositeAssembler>
   {
      /// <summary>
      /// Gets the name of this <see cref="ModuleArchitecture"/>.
      /// </summary>
      /// <value>The name of this <see cref="ModuleArchitecture"/>.</value>
      String Name { get; }

      /// <summary>
      /// Gets the <see cref="LayerArchitecture"/> this <see cref="ModuleArchitecture"/> belongs to.
      /// </summary>
      /// <value>The <see cref="LayerArchitecture"/> this <see cref="ModuleArchitecture"/> belongs to.</value>
      LayerArchitecture Layer { get; }

      /// <summary>
      /// Gets the <see cref="LayeredCompositeAssembler"/>, a specialized version of <see cref="Bootstrap.Assembling.Assembler"/>, of this <see cref="ModuleArchitecture"/>.
      /// </summary>
      /// <value>The <see cref="LayeredCompositeAssembler"/> of this <see cref="ModuleArchitecture"/>.</value>
      LayeredCompositeAssembler CompositeAssembler { get; }
   }
}
