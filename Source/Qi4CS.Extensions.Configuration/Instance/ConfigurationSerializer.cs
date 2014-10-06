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

namespace Qi4CS.Extensions.Configuration.Instance
{
   /// <summary>
   /// This is interface that will be used by Qi4CS Configuration Extension when there is need to serialize and deserialize configuration contents.
   /// The implementing types must be Qi4CS composites.
   /// </summary>
   /// <seealso cref="Assembling.ConfigurationCompositeDefaultInfo.SerializedBy(Type)"/>
   /// <seealso cref="ConfigurationCompositeInfo.Serializer"/>
   /// <seealso cref="ConfigurationManager.Create{T}(ConfigurationCompositeInfo)"/>
   public interface ConfigurationSerializer
   {
      /// <summary>
      /// Will be called by Qi4CS Configuration Extension when configuration contents should be serialized.
      /// </summary>
      /// <param name="contents">The configuration contents.</param>
      /// <param name="resource">The resource information about where to serialize the <paramref name="contents"/>.</param>
      void Serialize( Object contents, Qi4CSConfigurationResource resource );

      /// <summary>
      /// Will be called by Qi4CS Configuration Extension when configuration contents should be deserialized.
      /// </summary>
      /// <param name="type">The requested type of contents (same as generic argument of <see cref="ConfigurationInstance{T}"/> interface).</param>
      /// <param name="resource">The resource information about where to deserialize the resulting configuration.</param>
      /// <returns>The deserialized configuration contents of given <paramref name="type"/>.</returns>
      Object Deserialize( Type type, Qi4CSConfigurationResource resource );
   }
}
