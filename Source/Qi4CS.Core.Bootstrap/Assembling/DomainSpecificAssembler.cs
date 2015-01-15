/*
 * Copyright (c) 2009, Rickard Öberg.
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

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This interface may be implemented by user code in order to perform assembling 
   /// </summary>
   /// <typeparam name="TAssemblingUnit">The unit of the assembler. Determined on <see cref="DomainSpecificAssemblerAggregator{T}"/>.</typeparam>
   public interface DomainSpecificAssembler<in TAssemblingUnit>
   {
      /// <summary>
      /// Called by Qi4CS just before creation of <see cref="SPI.Model.ApplicationModel{T}"/>.
      /// This method should add composites to given assembling unit.
      /// </summary>
      /// <param name="unit">The assembling unit to add composites to.</param>
      void AddComposites( TAssemblingUnit unit );
   }
}
