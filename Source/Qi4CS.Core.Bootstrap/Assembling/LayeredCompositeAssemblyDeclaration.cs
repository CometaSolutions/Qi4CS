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

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This interface extends <see cref="AbstractCompositeAssemblyDeclaration"/> to add methods specific for <see cref="LayeredArchitecture"/>.
   /// </summary>
   /// <remarks>The default visibility for composites is <see cref="Visibility.MODULE"/>.</remarks>
   public interface LayeredAbstractCompositeAssemblyDeclaration : AbstractCompositeAssemblyDeclaration
   {
      /// <summary>
      /// Sets visibility for the composites represented by this <see cref="LayeredAbstractCompositeAssemblyDeclaration"/>.
      /// </summary>
      /// <param name="visibility">The <see cref="Visibility"/> for the the composites represented by this <see cref="LayeredAbstractCompositeAssemblyDeclaration"/>.</param>
      /// <returns>This <see cref="LayeredAbstractCompositeAssemblyDeclaration"/>.</returns>
      /// <seealso cref="Visibility"/>
      LayeredAbstractCompositeAssemblyDeclaration VisibleIn( Visibility visibility );
   }

   /// <summary>
   /// This interface represents 
   /// </summary>
   public interface LayeredPlainCompositeAssemblyDeclaration : PlainCompositeAssemblyDeclaration, LayeredAbstractCompositeAssemblyDeclaration
   {

   }
}
