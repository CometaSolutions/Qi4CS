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
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This interface represents a declaration of zero or more fragment types.
   /// It is acquired via methods in <see cref="AbstractCompositeAssemblyDeclaration"/>.
   /// </summary>
   public interface FragmentAssemblyDeclaration
   {
      /// <summary>
      /// Returns the <see cref="AbstractCompositeAssemblyDeclaration"/> this <see cref="FragmentAssemblyDeclaration"/> originated from.
      /// </summary>
      /// <returns>The <see cref="AbstractCompositeAssemblyDeclaration"/> this <see cref="FragmentAssemblyDeclaration"/> originated from.</returns>
      AbstractCompositeAssemblyDeclaration Done();

      /// <summary>
      /// Adds specific apply filter types for fragment types represented by this <see cref="FragmentAssemblyDeclaration"/>.
      /// </summary>
      /// <param name="applyFilters">The types of apply filters. These must implement <see cref="AppliesToFilter"/> interface.</param>
      /// <returns>This <see cref="FragmentAssemblyDeclaration"/></returns>
      /// <remarks>
      /// This method will do nothing if <paramref name="applyFilters"/> is <c>null</c>.
      /// This method also ignores all <c>null</c> values within <paramref name="applyFilters"/> when it is not <c>null</c>.
      /// </remarks>
      FragmentAssemblyDeclaration ApplyWith( params Type[] applyFilters );

      /// <summary>
      /// Adds specific apply filter instances for fragment types represented by this <see cref="FragmentAssemblyDeclaration"/>.
      /// </summary>
      /// <param name="readyMadeApplyFilters">The apply filter instances.</param>
      /// <returns>This <see cref="FragmentAssemblyDeclaration"/>.</returns>
      /// <remarks>
      /// This method will do nothing if <paramref name="readyMadeApplyFilters"/> is <c>null</c>.
      /// This method also ignores all <c>null</c> values within <paramref name="readyMadeApplyFilters"/> when it is not <c>null</c>.
      /// </remarks>
      FragmentAssemblyDeclaration ApplyWith( params AppliesToFilter[] readyMadeApplyFilters );
   }
}
