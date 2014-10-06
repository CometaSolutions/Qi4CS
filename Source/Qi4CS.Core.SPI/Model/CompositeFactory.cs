/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This interface provides a way to create instances of generated composite types.
   /// </summary>
   /// <remarks>
   /// The motivation for this interface is that when there are no generic arguments, each type may be created without the use of reflection.
   /// </remarks>
   public interface CompositeFactory
   {
      /// <summary>
      /// Creates a new instance of generated type.
      /// Reflection will be used only if the type is generic.
      /// </summary>
      /// <param name="typeID">The type ID of the type. The ID of the type is available through <see cref="GeneratedTypeInfo.GeneratedTypeID"/>.</param>
      /// <param name="gArgs">Generic arguments for a generic type. May be <c>null</c> if not a generic type.</param>
      /// <param name="args">The arguments for constructor of the type.</param>
      /// <returns>A new instance of type with ID <paramref name="typeID"/>.</returns>
      /// <remarks>
      /// Type IDs are unique in scope of a single composite model.
      /// </remarks>
      Object CreateInstance( Int32 typeID, Type[] gArgs, Object[] args );
   }
}
