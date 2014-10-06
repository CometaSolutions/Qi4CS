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
using System.Collections.Generic;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// <see cref="ModelInfoContainer"/> provides ways to search and get <see cref="CompositeModelInfo"/>s.
   /// </summary>
   /// <see cref="CompositeInstanceStructureOwner"/>
   public interface ModelInfoContainer
   {
      /// <summary>
      /// Gets the <see cref="CompositeModelInfo"/> of model which has <see cref="CompositeModel.ModelType"/> as reference to given <paramref name="modelType"/> and which has <see cref="CompositeModel.PublicTypes"/> logically assignable from given <paramref name="compositeTypes"/>.
      /// </summary>
      /// <param name="modelType">The <see cref="CompositeModelType"/> of composite to find.</param>
      /// <param name="compositeTypes">The public types of composite to find.</param>
      /// <returns><see cref="CompositeModelInfo"/> of the <see cref="CompositeModel"/> suitable to given search criteria.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="modelType"/> or <paramref name="compositeTypes"/> is <c>null</c>.</exception>
      /// <exception cref="API.Instance.AmbiguousTypeException">If more than one suitable <see cref="CompositeModelInfo"/> is found with given search criteria.</exception>
      /// <exception cref="NoSuchCompositeTypeException">If no suitable <see cref="CompositeModelInfo"/> is found with given search criteria.</exception>
      CompositeModelInfo GetCompositeModelInfo( CompositeModelType modelType, IEnumerable<Type> compositeTypes );

      /// <summary>
      /// When <see cref="CompositeModel"/> instance is known, retrieves the <see cref="CompositeModelInfo"/> associated with it.
      /// This method is potentially much faster than <see cref="GetCompositeModelInfo(CompositeModelType, IEnumerable{Type})"/>.
      /// </summary>
      /// <param name="model">The <see cref="CompositeModel"/> of the composite to find.</param>
      /// <returns>The <see cref="CompositeModelInfo"/> associated with given <paramref name="model"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="model"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If this <see cref="ModelInfoContainer"/> does not have <see cref="CompositeModelInfo"/> associated with given <paramref name="model"/>.</exception>
      CompositeModelInfo GetCompositeModelInfo( CompositeModel model );
   }
}
