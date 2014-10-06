/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Qi4CS.Extensions.Configuration.XML
//{
//   /// <summary>
//   /// This attribute controls how each array element is named in XML configuration.
//   /// </summary>
//   [AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
//   public sealed class ListOrArrayElementNameAttribute : Attribute
//   {
//      private readonly String _elementName;

//      /// <summary>
//      /// Creates new instance of <see cref="ListOrArrayElementNameAttribute"/> with given element name.
//      /// </summary>
//      /// <param name="elementName">The name of the XML element containing data for each array or list element.</param>
//      public ListOrArrayElementNameAttribute( String elementName )
//      {
//         this._elementName = elementName;
//      }

//      /// <summary>
//      /// Gets the XML element name for list or array elements.
//      /// </summary>
//      /// <value>The XML element name for list or array elements.</value>
//      public String ElementName
//      {
//         get
//         {
//            return this._elementName;
//         }
//      }
//   }
//}
