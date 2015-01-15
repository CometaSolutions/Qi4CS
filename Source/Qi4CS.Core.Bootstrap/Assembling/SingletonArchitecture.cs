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
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Bootstrap.Model;

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This Qi4CS architecture that has no internal structure or visibility rules or anything else, all composites are just contained within the architecture.
   /// This is ideal for tests or very simple applications.
   /// </summary>
   /// <remarks>
   /// The instances of this type can be created via <see cref="M:Qi4CS.Core.Architectures.Assembling.Qi4CSArchitectureFactory.NewSingletonArchitecture(Qi4CS.Core.Runtime.Assembling.CompositeModelTypeAssemblyScopeSupport[])"/>.
   /// </remarks>
   public interface SingletonArchitecture : ApplicationArchitecture<SingletonApplicationModel>, DomainSpecificAssemblerAggregator<Assembler>
   {
      /// <summary>
      /// Gets the <see cref="Assembler"/> of this <see cref="SingletonArchitecture"/>.
      /// </summary>
      /// <value>The <see cref="Assembler"/> of this <see cref="SingletonArchitecture"/>.</value>
      Assembler CompositeAssembler { get; }
   }
}
