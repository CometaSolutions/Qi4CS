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
using CommonUtils;

namespace Qi4CS.Extensions.Configuration.Instance
{
   /// <summary>
   /// This is interface which wraps the actual contents of configuration type.
   /// This interface provides some information about the configuration and commands to reload and save the configuration.
   /// </summary>
   /// <typeparam name="TConfiguration">The configuration type.</typeparam>
   public interface ConfigurationInstance<out TConfiguration>
      where TConfiguration : class
   {
      /// <summary>
      /// Gets the context-dependent resource this configuration was retrieved from.
      /// </summary>
      /// <value>The context-dependent resource this configuration was retrieved from.</value>
      Qi4CSConfigurationResource Resource { get; }

      /// <summary>
      /// Reloads the contents of this configuration from possibly different resource.
      /// </summary>
      /// <param name="newResource">
      /// The resource to load configuration from.
      /// If <c>null</c>, the value of <see cref="Resource"/> property will be used.
      /// Does not change the value of <see cref="Resource"/> property.
      /// </param>
      /// <remarks>
      /// The <see cref="AfterReload"/> event will be fired after reloading the configuration has been completed.
      /// </remarks>
      void Reload( Qi4CSConfigurationResource newResource = null );

      /// <summary>
      /// Saves the contents of this configuration to possibly different resource.
      /// </summary>
      /// <param name="newResource">
      /// The resource to save configuration to.
      /// If <c>null</c>, the value of <see cref="Resource"/> property will be used.
      /// Does not change the value of <see cref="Resource"/> property.
      /// </param>
      /// <remarks>
      /// The <see cref="AfterSave"/> event will be fired after saving the configuration has been completed.
      /// </remarks>
      void Save( Qi4CSConfigurationResource newResource = null );

      /// <summary>
      /// Gets the configuration contents of this configuration instance.
      /// </summary>
      /// <value>The configuration contents of this configuration instance.</value>
      ///// <remarks>
      ///// The first time this property is accessed after creating this <see cref="ConfigurationInstance{T}"/> or after calling <see cref="Reload(String)"/> method will cause <see cref="AfterLoad"/> event to trigger.
      ///// </remarks>
      TConfiguration Configuration { get; }

      /// <summary>
      /// This event will be triggered after each call to <see cref="Reload(Qi4CSConfigurationResource)"/> method.
      /// </summary>
      event EventHandler<ConfigurationEventArgs> AfterReload;

      ///// <summary>
      ///// This event will be triggered after <see cref="Configuration"/> property is accessed for a first time after creating this <see cref="ConfigurationInstance{T}"/> or after calling <see cref="Reload(String)"/> method.
      ///// </summary>
      //event EventHandler<ConfigurationEventArgs> AfterLoad;

      /// <summary>
      /// This event will be triggered after each call to <see cref="Save(Qi4CSConfigurationResource)"/> method.
      /// </summary>
      event EventHandler<ConfigurationEventArgs> AfterSave;
   }

   /// <summary>
   /// This event arguments class is used in <see cref="Instance.ConfigurationInstance{T}.AfterReload"/> and <see cref="Instance.ConfigurationInstance{T}.AfterSave"/> events.
   /// It provides information about the configuration and the related resource.
   /// </summary>
   public class ConfigurationEventArgs : EventArgs
   {
      private readonly ConfigurationInstance<Object> _instance;
      private readonly Qi4CSConfigurationResource _resource;

      internal ConfigurationEventArgs( ConfigurationInstance<Object> instance, Qi4CSConfigurationResource resource )
      {
         ArgumentValidator.ValidateNotNull( "Configuration instance", instance );
         ArgumentValidator.ValidateNotNull( "Resource", resource );

         this._instance = instance;
         this._resource = resource;
      }

      /// <summary>
      /// Gets the <see cref="Instance.ConfigurationInstance{T}"/> associated with the event.
      /// </summary>
      /// <value>The <see cref="Instance.ConfigurationInstance{T}"/> associated with the event.</value>
      public ConfigurationInstance<Object> ConfigurationInstance
      {
         get
         {
            return this._instance;
         }
      }

      /// <summary>
      /// Gets the resource associated with the event.
      /// </summary>
      /// <value>The resource associated with the event.</value>
      /// <seealso cref="Instance.ConfigurationInstance{T}.Resource"/>
      /// <seealso cref="Instance.ConfigurationInstance{T}.Reload(Qi4CSConfigurationResource)"/>
      /// <seealso cref="Instance.ConfigurationInstance{T}.Save(Qi4CSConfigurationResource)"/>
      public Qi4CSConfigurationResource Resource
      {
         get
         {
            return this._resource;
         }
      }
   }

   /// <summary>
   /// This interface represents the resource of Qi4CS configuration, which will be used when (de)serializing the configuration.
   /// Typically this is a filename.
   /// </summary>
   public interface Qi4CSConfigurationResource
   {
      /// <summary>
      /// Creates another <see cref="Qi4CSConfigurationResource"/> which represents a resource to which something in this configuration references to.
      /// </summary>
      /// <param name="resource">The resource object.</param>
      /// <returns>A new instance of <see cref="Qi4CSConfigurationResource"/> representing given resource object within this configuration.</returns>
      /// <remarks>This method is useful when e.g. XML configuration file contains reference to another XML configuration file to be used.</remarks>
      Qi4CSConfigurationResource CreateResourceWithinConfiguration( Object resource );
   }

   /// <summary>
   /// This is default implementation for <see cref="Qi4CSConfigurationResource"/> using <see cref="Uri"/>s.
   /// </summary>
   public class UriConfigurationResource : Qi4CSConfigurationResource
   {
      private readonly Uri _uri;

      /// <summary>
      /// Creates a new instance of <see cref="UriConfigurationResource"/> with given textual URI.
      /// </summary>
      /// <param name="uri">The textual URI.</param>
      public UriConfigurationResource( String uri )
         : this( new Uri( uri ) )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="UriConfigurationResource"/> with given URI.
      /// </summary>
      /// <param name="uri">The URI.</param>
      public UriConfigurationResource( Uri uri )
      {
         ArgumentValidator.ValidateNotNull( "URI", uri );

         this._uri = uri;
      }

      /// <summary>
      /// Creates a new <see cref="Uri"/> out from result of <see cref="Object.ToString"/> method of <paramref name="resource"/>.
      /// If the newly created <see cref="Uri"/> is not absolute, a new <see cref="Uri"/> is created using <see cref="Uri.MakeRelativeUri"/> method from this URI.
      /// Finally, the newest created <see cref="Uri"/> is given to constructor of <see cref="UriConfigurationResource"/> and that is returned as result.
      /// </summary>
      /// <param name="resource">The resource.</param>
      /// <returns>A new <see cref="UriConfigurationResource"/> with relative URI to this.</returns>
      public Qi4CSConfigurationResource CreateResourceWithinConfiguration( Object resource )
      {
         var uri = new Uri( resource.ToString() );
         return new UriConfigurationResource( uri.IsAbsoluteUri ? uri : this._uri.MakeRelativeUri( uri ) );
      }

      /// <inheritdoc />
      public override String ToString()
      {
         return this._uri.ToString();
      }
   }
}
