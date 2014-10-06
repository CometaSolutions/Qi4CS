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
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Extensions.Configuration.Instance;

namespace Qi4CS.Extensions.Configuration.Assembling
{
   /// <summary>
   /// This interface may be used to setup some default information about one or more configuration contents types.
   /// The instances of this interface are obtained via <see cref="ConfigurationManagerDeclaration.WithDefaultsFor(Type[])"/> method.
   /// </summary>
   /// <seealso cref="ConfigurationManagerDeclaration.WithDefaultsFor( Type[])"/>
   public interface ConfigurationCompositeDefaultInfo
   {
      /// <summary>
      /// Sets the default location for this configuration contents type.
      /// </summary>
      /// <param name="locationInfo">
      /// The default location of this configuration contents type.
      /// The actual contents will be interpreted by the serializer specified by <see cref="SerializedBy(Type)"/> method.
      /// The <c>null</c> value may be used to signal there is no default location for this configuration contents type.
      /// </param>
      /// <returns>This <see cref="ConfigurationCompositeDefaultInfo"/>.</returns>
      /// <seealso cref="SerializedBy(Type)"/>
      ConfigurationCompositeDefaultInfo SetDefaultLocationInfo( Qi4CSConfigurationResource locationInfo );

      /// <summary>
      /// Sets the serializer type for this configuration contents type.
      /// The serializer type must implement <see cref="Instance.ConfigurationSerializer"/> interface, and it must be a Qi4CS plain contents accessible by the <see cref="Assembler"/> of the <see cref="ConfigurationManagerDeclaration"/> that this <see cref="ConfigurationCompositeDefaultInfo"/> was obtained from.
      /// </summary>
      /// <param name="serializer">
      /// The type implemention configuration contents serialization logic.
      /// The <c>null</c> value may be used to signal there is no default serializer for this configuration contents.
      /// </param>
      /// <returns>This <see cref="ConfigurationCompositeDefaultInfo"/>.</returns>
      /// <exception cref="ArgumentException">If <paramref name="serializer"/> does not implement <see cref="Instance.ConfigurationSerializer"/> type.</exception>
      ConfigurationCompositeDefaultInfo SerializedBy( Type serializer );

      //ConfigurationCompositeDefaultInfo KeepInSync( ConfigurationSyncer syncer );

      /// <summary>
      /// Returns the <see cref="ConfigurationManagerDeclaration"/> that this <see cref="ConfigurationCompositeDefaultInfo"/> was obtained from.
      /// </summary>
      /// <returns>The <see cref="ConfigurationManagerDeclaration"/> that this <see cref="ConfigurationCompositeDefaultInfo"/> was obtained from.</returns>
      ConfigurationManagerDeclaration Done();
   }
}
