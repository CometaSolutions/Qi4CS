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
using CollectionsWithRoles.API;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This is common interface for all Qi4CS models which can contain attributes.
   /// </summary>
   /// <seealso cref="FieldModel"/>
   /// <seealso cref="ParameterModel"/>
   /// <seealso cref="EventModel"/>
   /// <seealso cref="PropertyModel"/>
   /// <seealso cref="SpecialMethodModel"/>
   public interface ModelWithAttributes
   {
      /// <summary>
      /// Gets all the attributes from <see cref="AllAttributes"/> types of which have attributes of type <paramref name="markingAttributeType"/>.
      /// </summary>
      /// <param name="markingAttributeType">The type of attribute that must be present on types of elements of <see cref="AllAttributes"/>.</param>
      /// <returns>All the attributes from <see cref="AllAttributes"/> types of which have attributes of type <paramref name="markingAttributeType"/>.</returns>
      ListQuery<Attribute> GetAttributesMarkedWith( Type markingAttributeType );

      /// <summary>
      /// Gets all the attributes that this model has.
      /// </summary>
      /// <value>All the attributes that this model has.</value>
      ListQuery<Attribute> AllAttributes { get; }

      /// <summary>
      /// Gets information about all attributes applied on given type of attributes in <see cref="AllAttributes"/> property.
      /// </summary>
      /// <param name="attributeType">The attribute type.</param>
      /// <returns>Information about all attributes applied on given type of attributes in <see cref="AllAttributes"/> property.</returns>
      DictionaryQuery<Type, ListQuery<Attribute>> GetAttributesOfAttribute( Type attributeType );
   }
}
