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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Extensions.Configuration.Instance
{
   internal abstract class ConfigurationInstanceMixin<TConfiguration> : ConfigurationInstance<TConfiguration>
      where TConfiguration : class
   {
#pragma warning disable 649

      [This]
      private ConfigurationInstanceState<TConfiguration> _state;

      [This]
      private ConfigurationInstance<TConfiguration> _meAsInstance;

      [State( "AfterReload" )]
      private CompositeEvent<EventHandler<ConfigurationEventArgs>> _reloadRequestedEvent;

      //[State( "AfterLoad" )]
      //private CompositeEvent<EventHandler<ConfigurationEventArgs>> _afterLoadEvent;

      [State( "AfterSave" )]
      private CompositeEvent<EventHandler<ConfigurationEventArgs>> _afterSaveEvent;

#pragma warning restore 649

      #region ConfigurationInstance Members

      public virtual void Reload( Qi4CSConfigurationResource newResource )
      {
         newResource = newResource ?? this.Resource;
         var invokeEvent = this._state.Configuration.Item1.IsValueCreated;
         this.SetConfigurationComposite( newResource );
         this._reloadRequestedEvent.InvokeAction( this._meAsInstance, new ConfigurationEventArgs( this._meAsInstance, newResource ) );
      }

      public virtual void Save( Qi4CSConfigurationResource newResource )
      {
         newResource = newResource ?? this.Resource;
         this.SaveState( newResource );
      }

      public virtual TConfiguration Configuration
      {
         get
         {
            // TODO - this may cause _afterLoadEvent occur many times for a single configuration in concurrent scenarios
            //var invokeEvent = !this._state.Configuration.Item1.IsValueCreated;
            //var result = this._state.Configuration;
            //if ( invokeEvent )
            //{
            //   this._afterLoadEvent.InvokeAction( this._meAsInstance, new ConfigurationEventArgs( this._meAsInstance, result.Item2 ) );
            //}
            //return result.Item1.Value;
            return this._state.Configuration.Item1.Value;
         }
      }

      public virtual Qi4CSConfigurationResource Resource
      {
         get
         {
            return this._state.Configuration.Item2;
         }
      }

      public abstract event EventHandler<ConfigurationEventArgs> AfterReload;
      //public abstract event EventHandler<ConfigurationEventArgs> AfterLoad;
      public abstract event EventHandler<ConfigurationEventArgs> AfterSave;

      #endregion

      [Prototype]
      protected void InitState( [Uses] Tuple<ConfigurationSerializer, Qi4CSConfigurationResource> configurationSerializationInfo )
      {
         this._state.Serializer = configurationSerializationInfo.Item1;
         this.SetConfigurationComposite( configurationSerializationInfo.Item2 );
      }

      private void SetConfigurationComposite( Qi4CSConfigurationResource resource )
      {
         this._state.Configuration = Tuple.Create( new Lazy<TConfiguration>( () =>
         {
            return (TConfiguration) this._state.Serializer.Deserialize(
                     typeof( TConfiguration ),
                     resource );
            // Don't invoke afterLoad event here as it might cause another request to get configuration -> an exception will be thrown
         },
         System.Threading.LazyThreadSafetyMode.ExecutionAndPublication ), resource );
      }

      private void SaveState( Qi4CSConfigurationResource resource )
      {
         var configInfo = this._state.Configuration;
         this._state.Serializer.Serialize(
            configInfo.Item1.Value,
            resource );
         this._afterSaveEvent.InvokeAction( this._meAsInstance, new ConfigurationEventArgs( this._meAsInstance, resource ) );
      }
   }

   internal interface ConfigurationInstanceState<TConfiguration>
      where TConfiguration : class
   {
      Tuple<Lazy<TConfiguration>, Qi4CSConfigurationResource> Configuration { get; set; }

      [Immutable]
      ConfigurationSerializer Serializer { get; set; }
   }
}
