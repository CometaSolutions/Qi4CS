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
using System.Reflection;
using Qi4CS.Extensions.Configuration.Instance;

namespace Qi4CS.Extensions.Configuration.Assembling
{
   internal class ConfigurationCompositeDefaultInfoImpl : ConfigurationCompositeDefaultInfo
   {

      private readonly ConfigurationManagerDeclaration _assemblyDeclaration;
      private readonly ConfigurationManagerInfo _info;
      private readonly Type[] _types;

      internal ConfigurationCompositeDefaultInfoImpl( ConfigurationManagerDeclaration assemblyDeclaration, ConfigurationManagerInfo info, Type[] types )
      {
         this._assemblyDeclaration = assemblyDeclaration;
         this._info = info;
         this._types = types;
      }

      #region ConfigurationCompositeDefaultInfo Members

      public ConfigurationCompositeDefaultInfo SetDefaultLocationInfo( Qi4CSConfigurationResource locationInfo )
      {
         foreach ( var id in this._types )
         {
            this._info.CompositeInfos.GetOrAdd_NotThreadSafe( id, () => new ConfigurationCompositeInfo() ).Resource = locationInfo;
         }
         return this;
      }

      //public ConfigurationCompositeDefaultInfo KeepInSync( ConfigurationSyncer syncer )
      //{
      //   foreach ( var id in this._assemblyDeclaration.AffectedCompositeIDs )
      //   {
      //      this._info.CompositeInfos[id].Syncer = syncer;
      //   }
      //   return this;
      //}

      public ConfigurationCompositeDefaultInfo SerializedBy( Type serializer )
      {
         if ( serializer != null )
         {
            if ( !typeof( ConfigurationSerializer ).IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( serializer ) )
            {
               throw new ArgumentException( "Serializer type must be assignable from " + typeof( ConfigurationSerializer ) );
            }
         }

         foreach ( var id in this._types )
         {
            this._info.CompositeInfos.GetOrAdd_NotThreadSafe( id, () => new ConfigurationCompositeInfo() ).Serializer = serializer;
         }
         return this;
      }

      public ConfigurationManagerDeclaration Done()
      {
         return this._assemblyDeclaration;
      }

      #endregion
   }
}
