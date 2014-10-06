﻿/*
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

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This is common interface for Qi4CS architectural unit that supports adding <see cref="DomainSpecificAssembler{T}"/>s.
   /// </summary>
   /// <typeparam name="TAssemblingUnit">The type of the assembling unit.</typeparam>
   /// <seealso cref="LayerArchitecture"/>
   /// <seealso cref="ModuleArchitecture"/>
   /// <seealso cref="SingletonArchitecture"/>
   public interface DomainSpecificAssemblerAggregator<out TAssemblingUnit>
   {
      /// <summary>
      /// Adds domain-specific assemblers to this architectural unit.
      /// During application model creation, this unit will invoke the <see cref="DomainSpecificAssembler{T}.AddComposites(T)"/> method of all assemblers added to this unit.
      /// </summary>
      /// <param name="assemblers">The assemblers to add.</param>
      /// <remarks>
      /// This method does noething if <paramref name="assemblers"/> is <c>null</c>.
      /// This method also ignores all <c>null</c> values within <paramref name="assemblers"/> when it is not <c>null</c>.
      /// </remarks>
      void AddDomainSpecificAssemblers( params DomainSpecificAssembler<TAssemblingUnit>[] assemblers );
   }
}