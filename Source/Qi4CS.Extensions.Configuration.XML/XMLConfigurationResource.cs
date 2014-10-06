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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils;
using Qi4CS.Extensions.Configuration.Instance;
using System.IO;

namespace Qi4CS.Extensions.Configuration.XML
{
   /// <summary>
   /// Represents information about where to serialize and deserialize XML configuration.
   /// This information consists of textual resource of XML document and optional textual XPath expression representing the element within the document that holds the information.
   /// </summary>
   public class XMLConfigurationResource : Qi4CSConfigurationResource
   {
      private readonly String _documentResource;
      private readonly String _xpath;

      /// <summary>
      /// Creates a new instance of <see cref="XMLConfigurationResource"/> which will use root element of the XML document to (de)serialize configuration.
      /// </summary>
      /// <param name="documentResource">The textual resource path to XML document.</param>
      /// <exception cref="ArgumentException">If <paramref name="documentResource"/> is <c>null</c> or empty.</exception>
      public XMLConfigurationResource( String documentResource )
         : this( documentResource, null )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="XMLConfigurationResource"/> which will use specific element within the XML document to (de)serialize configuration.
      /// </summary>
      /// <param name="documentResource">The textual resource path to XML document.</param>
      /// <param name="xpath">The textual path to the element to use to (de)serialize configuration. IF <c>null</c>, root element will be used.</param>
      /// <exception cref="ArgumentException">If <paramref name="documentResource"/> is <c>null</c> or empty.</exception>
      public XMLConfigurationResource( String documentResource, String xpath )
      {
         ArgumentValidator.ValidateNotEmpty( "Document resource", documentResource );

         this._documentResource = documentResource;
         this._xpath = xpath;
      }

      /// <summary>
      /// Returns the textual resource path to the XML document to use.
      /// </summary>
      /// <value>The textual resource path to the XML document to use.</value>
      public String DocumentResource
      {
         get
         {
            return this._documentResource;
         }
      }

      /// <summary>
      /// Returns the XPath expression of the element to use to (de)serialize configuration. May be <c>null</c>.
      /// </summary>
      /// <value>The XPath expression of the element to use to (de)serialize configuration.</value>
      public String XPath
      {
         get
         {
            return this._xpath;
         }
      }

      /// <inheritdoc />
      public override String ToString()
      {
         var retVal = this._documentResource;
         if ( !String.IsNullOrEmpty( this._xpath ) )
         {
            retVal += ", element XPath: " + this._xpath;
         }

         return retVal;
      }

      /// <inheritdoc />
      public Qi4CSConfigurationResource CreateResourceWithinConfiguration( Object resource )
      {
         Qi4CSConfigurationResource retVal;
         if ( resource == null )
         {
            retVal = null;
         }
         else
         {
            var resStr = resource as String;
            if ( resStr != null )
            {
               retVal = new XMLConfigurationResource( Path.GetFullPath( Path.IsPathRooted( resStr ) ? resStr : Path.Combine( Path.GetDirectoryName( this._documentResource ), resStr ) ) );
            }
            else
            {
               throw new NotSupportedException( "Unsupported resource: " + resource );
            }
         }
         return retVal;
      }
   }
}
