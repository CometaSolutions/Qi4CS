using System;
using CollectionsWithRoles.API;
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
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This interface binds together the <see cref="CompositeModel"/> of the composite from the model scope and type generation results and other things required at instance scope.
   /// </summary>
   public interface CompositeModelInfo
   {
      /// <summary>
      /// Gets the <see cref="CompositeModel"/> of this <see cref="CompositeModelInfo"/>.
      /// </summary>
      /// <value>The <see cref="CompositeModel"/> of this <see cref="CompositeModelInfo"/>.</value>
      /// <seealso cref="CompositeModel"/>
      CompositeModel Model { get; }

      /// <summary>
      /// Gets the <see cref="PublicCompositeTypeGenerationResult"/> of this <see cref="CompositeModelInfo"/>.
      /// </summary>
      /// <value>The <see cref="PublicCompositeTypeGenerationResult"/> of this <see cref="CompositeModelInfo"/>.</value>
      /// <remarks>The returned interface provides easy access to the types generated based on the <see cref="CompositeModel"/>.</remarks>
      /// <seealso cref="PublicCompositeTypeGenerationResult"/>
      PublicCompositeTypeGenerationResult Types { get; }

      /// <summary>
      /// Gets the <see cref="AttributeHolder"/>s of each composite method.
      /// The <see cref="AttributeHolder"/> at index <c>i</c> belongs to a <see cref="CompositeMethodModel"/> with <see cref="CompositeMethodModel.MethodIndex"/> of <c>i</c>.
      /// </summary>
      /// <value>The <see cref="AttributeHolder"/>s of each composite method.</value>
      /// <seealso cref="AttributeHolder"/>
      ListQuery<AttributeHolder> CompositeMethodAttributeHolders { get; }
   }

   /// <summary>
   /// This interface provides easy querying of attributes of the composite method, based on attribute type.
   /// </summary>
   public interface AttributeHolder
   {
      /// <summary>
      /// Gets all the attributes of the composite method, based on type.
      /// </summary>
      /// <value>All the attributes of the composite method, based on type.</value>
      DictionaryQuery<Type, ListQuery<Attribute>> AllAttributes { get; }
   }
}
