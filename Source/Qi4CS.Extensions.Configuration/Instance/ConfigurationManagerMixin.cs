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
using System.Linq;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Extensions.Configuration.Instance
{
   internal class ConfigurationManagerMixin : ConfigurationManager
   {
#pragma warning disable 649

      [This]
      private ConfigurationManagerState _state;

      [Structure]
      private StructureServiceProvider _ssp;

#pragma warning restore 649

      #region ConfigurationManager Members

      public virtual ConfigurationInstance<TConfiguration> Create<TConfiguration>( ConfigurationCompositeInfo info )
         where TConfiguration : class
      {
         var configType = typeof( TConfiguration );
         info = this.GetInfoForCreation( configType, info );
         if ( info == null )
         {
            throw new InvalidOperationException( "Could not find information about configuration with type " + configType + "." );
         }
         if ( info == null )
         {
            if ( !this._state.CompositeInfos.TryGetValue( configType, out info ) )
            {
               throw new InvalidOperationException( "Could not find information about configuration with type " + configType + "." );
            }
         }
         else
         {
            info = new ConfigurationCompositeInfo( info.Resource, info.Serializer );
            ConfigurationCompositeInfo existingInfo;
            if ( this._state.CompositeInfos.TryGetValue( configType, out existingInfo ) )
            {
               if ( info.Resource == null )
               {
                  info.Resource = existingInfo.Resource;
               }
               if ( info.Serializer == null )
               {
                  info.Serializer = existingInfo.Serializer;
               }
            }
         }

         return (ConfigurationInstance<TConfiguration>) this.DoCreate( configType, info );
      }

      public virtual Boolean HasInformationAbout( Type configurationType, ConfigurationCompositeInfo info = null )
      {
         info = this.GetInfoForCreation( configurationType, info );
         String errorMsg;
         return info != null && this.CheckInfo( configurationType, info, out errorMsg );
      }

      #endregion

      private ConfigurationCompositeInfo GetInfoForCreation( Type configType, ConfigurationCompositeInfo suppliedInfo )
      {
         if ( suppliedInfo == null )
         {
            if ( !this._state.CompositeInfos.TryGetValue( configType, out suppliedInfo ) )
            {
               suppliedInfo = null;
            }
         }
         else
         {
            suppliedInfo = new ConfigurationCompositeInfo( suppliedInfo.Resource, suppliedInfo.Serializer );
            ConfigurationCompositeInfo existingInfo;
            if ( this._state.CompositeInfos.TryGetValue( configType, out existingInfo ) )
            {
               if ( suppliedInfo.Resource == null )
               {
                  suppliedInfo.Resource = existingInfo.Resource;
               }
               if ( suppliedInfo.Serializer == null )
               {
                  suppliedInfo.Serializer = existingInfo.Serializer;
               }
            }
         }
         return suppliedInfo;
      }

      private Boolean CheckInfo( Type configType, ConfigurationCompositeInfo info, out String errorMsg )
      {
         var resource = info.Resource;
         var serializerType = info.Serializer;
         if ( serializerType == null )
         {
            errorMsg = "No (de)serializer found for configuration of type" + configType + " located in " + info.Resource + ".";
         }
         else if ( !typeof( ConfigurationSerializer ).IsAssignableFrom( serializerType ) )
         {
            errorMsg = "The serializer type " + serializerType + " does not implement " + typeof( ConfigurationSerializer ) + ".";
         }
         else
         {
            errorMsg = null;
         }

         return errorMsg == null;
      }

      [Prototype]
      internal void InitState( [Uses] ConfigurationManagerInfo configInfo )
      {
         this._state.CompositeInfos = configInfo.CompositeInfos.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
      }

      private ConfigurationInstance<Object> DoCreate( Type configType, ConfigurationCompositeInfo info )
      {
         var configInstanceType = typeof( ConfigurationInstance<> ).MakeGenericType( configType );
         var builder = this._ssp.NewPlainCompositeBuilder( configInstanceType );

         String errorMsg;
         if ( !this.CheckInfo( configType, info, out errorMsg ) )
         {
            throw new InvalidOperationException( errorMsg );
         }

         var serializer = this._ssp.NewPlainCompositeBuilder( info.Serializer ).Instantiate<ConfigurationSerializer>();
         builder.Use( Tuple.Create( serializer, info.Resource ) );

         return (ConfigurationInstance<Object>) builder.InstantiateWithType( typeof( Object ) );
      }
   }

   internal interface ConfigurationManagerState
   {
      [Immutable]
      IDictionary<Type, ConfigurationCompositeInfo> CompositeInfos { get; set; }
   }
}
