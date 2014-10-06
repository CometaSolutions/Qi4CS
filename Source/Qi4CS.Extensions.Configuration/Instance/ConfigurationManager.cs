using Qi4CS.Extensions.Configuration.Instance;
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
   /// This is interface for Qi4CS service which is responsible in handling <see cref="ConfigurationInstance{T}"/>s.
   /// </summary>
   public interface ConfigurationManager
   {
      /// <summary>
      /// Creates a new <see cref="ConfigurationInstance{T}"/> using serializer and resource specified by methods in <see cref="Assembling.ConfigurationCompositeDefaultInfo"/> at bootstrap time.
      /// </summary>
      /// <typeparam name="TConfiguration">The type of configuration contents.</typeparam>
      /// <param name="info">The optional <see cref="ConfigurationCompositeInfo"/> containing required information about the contents.</param>
      /// <returns>An instance of <see cref="ConfigurationInstance{T}"/> using serializer and resource specified by methods in <see cref="Assembling.ConfigurationCompositeDefaultInfo"/> at bootstrap time.</returns>
      /// <exception cref="InvalidOperationException">
      /// One of the following conditions applies:
      /// <list type="bullet">
      /// <item><description>The <paramref name="info"/> is <c>null</c> and there is no default information specified by methods in <see cref="Assembling.ConfigurationCompositeDefaultInfo"/> for configuration contents of type <typeparamref name="TConfiguration"/>.</description></item>
      /// <item><description>The exists information about <typeparamref name="TConfiguration"/> but the serializer type (<see cref="Assembling.ConfigurationCompositeDefaultInfo.SerializedBy(Type)"/>) has not been specified.</description></item>
      /// <item><description>If no serializer type is specified by given <paramref name="info"/> nor at bootstrap time.</description></item>
      /// <item><description>The serializer type information has been acquired, but the type does not implement <see cref="ConfigurationSerializer"/>.</description></item>
      /// </list>
      /// </exception>
      /// <exception cref="Qi4CS.Core.API.Instance.NoSuchCompositeTypeException">If serializer type is not Qi4CS composite or is not visible from the architectural unit this <see cref="ConfigurationManager"/> belongs to.</exception>
      /// <seealso cref="ConfigurationSerializer"/>
      ConfigurationInstance<TConfiguration> Create<TConfiguration>( ConfigurationCompositeInfo info = null )
         where TConfiguration : class;

      /// <summary>
      /// This method checks whether this <see cref="ConfigurationManager"/> has information about configuration instance with given configuration content type and optionally given location and serializer.
      /// </summary>
      /// <param name="configurationType">The type of configuration contents. This will be the type parameter for <see cref="Create"/> method.</param>
      /// <param name="info">The <see cref="ConfigurationCompositeInfo"/> specifying configuration location and serializer type. Use <c>null</c> to try to retrieve information from the ones specified at bootstrap time using methods in <see cref="Assembling.ConfigurationCompositeDefaultInfo"/>.</param>
      /// <returns><c>true</c> if this <see cref="ConfigurationManager"/> has information about configuration instance with given <paramref name="configurationType"/> and <see cref="ConfigurationCompositeInfo"/>; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// Even when this method returns <c>true</c>, the <see cref="ConfigurationManager.Create"/> method might still throw, if no suitable composite found for the <paramref name="configurationType"/>.
      /// </remarks>
      Boolean HasInformationAbout( Type configurationType, ConfigurationCompositeInfo info = null );

   }

   /// <summary>
   /// This class represents information about configuration contents to load.
   /// It is used in <see cref="ConfigurationManager.Create{T}(ConfigurationCompositeInfo)"/> method.
   /// </summary>
   public class ConfigurationCompositeInfo
   {
      /// <summary>
      /// Creates new instance of <see cref="ConfigurationCompositeInfo"/> with no information about resource nor serializer type.
      /// </summary>
      public ConfigurationCompositeInfo()
         : this( null, null )
      {
      }

      /// <summary>
      /// Creates a new instance of <see cref="ConfigurationCompositeInfo"/> with given information about resource and serializer type.
      /// </summary>
      /// <param name="resource">The context-dependent information of resource to load configuration from.</param>
      /// <param name="serializer">The type responsible of serializing and deserializing configuration contents. Must implement <see cref="ConfigurationSerializer"/>.</param>
      public ConfigurationCompositeInfo( Qi4CSConfigurationResource resource, Type serializer )
      {
         this.Resource = resource;
         this.Serializer = serializer;
      }

      /// <summary>
      /// Gets or sets the context-dependent information of resource to load configuration from.
      /// </summary>
      /// <value>The context-dependent information of resource to load configuration from.</value>
      public Qi4CSConfigurationResource Resource { get; set; }

      /// <summary>
      /// Gets or sets the type responsible of serializing and deserializing configuration contents. Must implement <see cref="ConfigurationSerializer"/>.
      /// </summary>
      /// <value>The type responsible of serializing and deserializing configuration contents. Must implement <see cref="ConfigurationSerializer"/>.</value>
      public Type Serializer { get; set; }
   }
}

public static partial class E_Qi4CSConfiguration
{
   /// <summary>
   /// Helper method to invoke <see cref="ConfigurationManager.Create"/> method for configuration located in specific location but serialized by default serializer.
   /// </summary>
   /// <typeparam name="TConfiguration">The type of configuration contents.</typeparam>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="configurationLocation">The location of configuration. May be <c>null</c> for default location.</param>
   /// <returns>An instance of <see cref="ConfigurationInstance{T}"/>.</returns>
   /// <remarks>See <see cref="ConfigurationManager.Create"/> method for more detailed description about exceptions and return values.</remarks>
   public static ConfigurationInstance<TConfiguration> Create<TConfiguration>( this ConfigurationManager manager, Qi4CSConfigurationResource configurationLocation )
      where TConfiguration : class
   {
      return manager.Create<TConfiguration>( configurationLocation, null );
   }

   /// <summary>
   /// Helper method to invoke <see cref="ConfigurationManager.Create"/> method for configuration serialized by specific serializer but located in default location.
   /// </summary>
   /// <typeparam name="TConfiguration">The type of configuration contents.</typeparam>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="configurationSerializer">The serializer for configuration. Must implement <see cref="ConfigurationSerializer"/>. May be <c>null</c> for default serializer.</param>
   /// <returns>An instance of <see cref="ConfigurationInstance{T}"/>.</returns>
   /// <remarks>See <see cref="ConfigurationManager.Create"/> method for more detailed description about exceptions and return values.</remarks>
   public static ConfigurationInstance<TConfiguration> Create<TConfiguration>( this ConfigurationManager manager, Type configurationSerializer )
            where TConfiguration : class
   {
      return manager.Create<TConfiguration>( null, configurationSerializer );
   }

   /// <summary>
   /// Helper method to invoke <see cref="ConfigurationManager.Create"/> method for configuration located in specific location and serialized by specific serializer.
   /// </summary>
   /// <typeparam name="TConfiguration">The type of configuration contents.</typeparam>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="configurationLocation">The location of configuration. May be <c>null</c> for default location.</param>
   /// <param name="configurationSerializer">The serializer for configuration. Must implement <see cref="ConfigurationSerializer"/>. May be <c>null</c> for default serializer.</param>
   /// <returns>An instance of <see cref="ConfigurationInstance{T}"/>.</returns>
   /// <remarks>See <see cref="ConfigurationManager.Create"/> method for more detailed description about exceptions and return values.</remarks>
   public static ConfigurationInstance<TConfiguration> Create<TConfiguration>( this ConfigurationManager manager, Qi4CSConfigurationResource configurationLocation, Type configurationSerializer )
         where TConfiguration : class
   {
      var info = new ConfigurationCompositeInfo( configurationLocation, configurationSerializer );
      return manager.Create<TConfiguration>( info );
   }
}