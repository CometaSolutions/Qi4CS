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
using Qi4CS.Extensions.Configuration.Assembling;
using Qi4CS.Extensions.Configuration.XML;
using Qi4CS.Core.Bootstrap.Assembling;

/// <summary>
/// Class containing extensions methods for XML (de)serialization support of Qi4CS configuration composites.
/// </summary>
public static partial class E_Qi4CSConfigurationXML
{
   /// <summary>
   /// Adds XML serialization support for configuration composite represented by this <see cref="ConfigurationCompositeDefaultInfo"/>.
   /// </summary>
   /// <param name="info">This <see cref="ConfigurationCompositeDefaultInfo"/>.</param>
   /// <returns>This <see cref="ConfigurationCompositeDefaultInfo"/>.</returns>
   public static ConfigurationCompositeDefaultInfo SerializedByXML( this ConfigurationCompositeDefaultInfo info )
   {
      return info.SerializedBy( typeof( XMLConfigurationSerializer ) );
   }

   /// <summary>
   /// Adds default XML document information for this configuration composite.
   /// </summary>
   /// <param name="info">This <see cref="ConfigurationCompositeDefaultInfo"/>.</param>
   /// <param name="documentPath">The path to the XML document holding serialized configuration.</param>
   /// <param name="xpath">The optional textual XPath expression to retrieve the actual element within the XML document to (de)serialize the configuration.</param>
   /// <returns>The <paramref name="info"/>.</returns>
   /// <exception cref="ArgumentException">If <paramref name="documentPath"/> is <c>null</c> or empty string.</exception>
   public static ConfigurationCompositeDefaultInfo LocatedInXMLDocument( this ConfigurationCompositeDefaultInfo info, String documentPath, String xpath = null )
   {
      return info.SetDefaultLocationInfo( new XMLConfigurationResource( documentPath, xpath ) );
   }

   /// <summary>
   /// Adds reqired composites to this <see cref="Assembler"/> so that XML serialization of configuration composites would be possible.
   /// This method should be invoked to same module as where <see cref="E_Qi4CSConfiguration.AddSupportForConfigurationManager"/> method has been called.
   /// </summary>
   /// <param name="assembler">This <see cref="Assembler"/>.</param>
   /// <param name="streamCallback">The optional callback to get streams. If this is <c>null</c>, the default callback will be used which will interpret <see cref="XMLConfigurationResource.DocumentResource"/> as path to file.</param>
   /// <param name="customSerializers">The custom serializers.</param>
   /// <seealso cref="XMLConfigurationSerializerHelper"/>
   public static void AddXMLSerializationSupport( this Assembler assembler, StreamCallback streamCallback, IList<XMLConfigurationSerializerHelper> customSerializers )
   {
      PlainCompositeAssemblyDeclaration decl;
      if ( assembler
         .ForNewOrExistingPlainComposite( new Type[] { typeof( XMLConfigurationSerializer ) }, out decl ) )
      {
         decl.OfTypes( typeof( XMLConfigurationSerializer ) );
      }
      var serializers = decl.Get<List<XMLConfigurationSerializerHelper>>();
      if ( serializers == null )
      {
         serializers = new List<XMLConfigurationSerializerHelper>();
      }
      if ( customSerializers != null )
      {
         serializers.AddRange( customSerializers );
      }

      decl.Use( streamCallback ?? new DotNETStreamHelper(), serializers );
   }

   // TODO xml configuration serializer info. It would hold root element names for various types, and could hold information for list element names.
}