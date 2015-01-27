using Qi4CS.Core.API.Model;
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
      /// This method checks whether this <see cref="ConfigurationManager"/> has information about configuration instance with given configuration content type and optionally given location and serializer.
      /// </summary>
      /// <param name="configurationType">The type of configuration.</param>
      /// <param name="info">The <see cref="ConfigurationCompositeInfo"/> specifying configuration location and serializer type. Use <c>null</c> to try to retrieve information from the ones specified at bootstrap time using methods in <see cref="Assembling.ConfigurationCompositeDefaultInfo"/>.</param>
      /// <returns><c>true</c> if this <see cref="ConfigurationManager"/> has information about configuration instance with given <paramref name="configurationType"/> and <see cref="ConfigurationCompositeInfo"/>; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// Even when this method returns <c>true</c>, the <see cref="E_Qi4CSConfiguration.Create(ConfigurationManager, Type, ConfigurationCompositeInfo)"/> method might still throw, if no suitable composite found for the <paramref name="configurationType"/>.
      /// </remarks>
      Boolean HasInformationAbout( Type configurationType, ConfigurationCompositeInfo info = null );

      /// <summary>
      /// Tries to get default information specified at bootstrap time about given configuration type.
      /// </summary>
      /// <param name="configurationType">The type of configuration.</param>
      /// <param name="info">This parameter will contain the <see cref="ConfigurationCompositeInfo"/> object containing location and serializer information for given <paramref name="configurationType"/>, if this manager will has that information.</param>
      /// <returns><c>true</c> if this manager has default information about the given configuration type; <c>false</c> otherwise.</returns>
      Boolean TryGetDefaultInformationAbout( Type configurationType, [Optional] out ConfigurationCompositeInfo info );

      /// <summary>
      /// Transforms a composite instance into <see cref="ConfigurationInstance{T}"/>.
      /// </summary>
      /// <param name="configurationType">The type of the configuration.</param>
      /// <param name="composite">The composite instance, should be castable into <paramref name="configurationType"/>. If <c>null</c>, then the initial value will be loaded from resource specified at bootstrap time or by <paramref name="info"/>.</param>
      /// <param name="info">The optional <see cref="ConfigurationCompositeInfo"/> containing required information about configuration.</param>
      /// <returns>An instance of <see cref="ConfigurationInstance{T}"/> using serializer and resource specified by <paramref name="info"/> or by methods in <see cref="Assembling.ConfigurationCompositeDefaultInfo"/> at bootstrap time.</returns>
      /// <exception cref="InvalidOperationException">
      /// One of the following conditions applies:
      /// <list type="bullet">
      /// <item><description>The <paramref name="info"/> is <c>null</c> and there is no default information specified by methods in <see cref="Assembling.ConfigurationCompositeDefaultInfo"/> for configuration contents of type <paramref name="configurationType"/>.</description></item>
      /// <item><description>The exists information about <paramref name="configurationType"/> but the serializer type (<see cref="Assembling.ConfigurationCompositeDefaultInfo.SerializedBy(Type)"/>) has not been specified.</description></item>
      /// <item><description>If no serializer type is specified by given <paramref name="info"/> nor at bootstrap time.</description></item>
      /// <item><description>The serializer type information has been acquired, but the type does not implement <see cref="ConfigurationSerializer"/>.</description></item>
      /// </list>
      /// </exception>
      /// <exception cref="Qi4CS.Core.API.Instance.NoSuchCompositeTypeException">If serializer type is not Qi4CS composite or is not visible from the architectural unit this <see cref="ConfigurationManager"/> belongs to.</exception>
      /// <seealso cref="ConfigurationSerializer"/>
      ConfigurationInstance<Object> AsConfigurationInstance( Type configurationType, [Optional] Object composite, ConfigurationCompositeInfo info = null );

   }

   /// <summary>
   /// This class represents information about configuration contents to load.
   /// It is used in <see cref="E_Qi4CSConfiguration.Create(ConfigurationManager, Type, ConfigurationCompositeInfo)"/>, <see cref="ConfigurationManager.AsConfigurationInstance"/> and <see cref="ConfigurationManager.HasInformationAbout"/> methods.
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
   /// Helper method to invoke <see cref="E_Qi4CSConfiguration.Create(ConfigurationManager, Type, ConfigurationCompositeInfo)"/> method for configuration located in specific location but serialized by default serializer.
   /// </summary>
   /// <typeparam name="TConfiguration">The type of configuration contents.</typeparam>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="configurationLocation">The location of configuration. May be <c>null</c> for default location.</param>
   /// <returns>An instance of <see cref="ConfigurationInstance{T}"/>.</returns>
   /// <remarks>See <see cref="E_Qi4CSConfiguration.Create(ConfigurationManager, Type, ConfigurationCompositeInfo)"/> method for more detailed description about exceptions and return values.</remarks>
   public static ConfigurationInstance<TConfiguration> Create<TConfiguration>( this ConfigurationManager manager, Qi4CSConfigurationResource configurationLocation )
      where TConfiguration : class
   {
      return manager.Create<TConfiguration>( configurationLocation, null );
   }

   /// <summary>
   /// Helper method to invoke <see cref="E_Qi4CSConfiguration.Create(ConfigurationManager, Type, ConfigurationCompositeInfo)"/> method for configuration serialized by specific serializer but located in default location.
   /// </summary>
   /// <typeparam name="TConfiguration">The type of configuration contents.</typeparam>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="configurationSerializer">The serializer for configuration. Must implement <see cref="ConfigurationSerializer"/>. May be <c>null</c> for default serializer.</param>
   /// <returns>An instance of <see cref="ConfigurationInstance{T}"/>.</returns>
   /// <remarks>See <see cref="E_Qi4CSConfiguration.Create(ConfigurationManager, Type, ConfigurationCompositeInfo)"/> method for more detailed description about exceptions and return values.</remarks>
   public static ConfigurationInstance<TConfiguration> Create<TConfiguration>( this ConfigurationManager manager, Type configurationSerializer )
            where TConfiguration : class
   {
      return manager.Create<TConfiguration>( null, configurationSerializer );
   }

   /// <summary>
   /// Helper method to invoke <see cref="E_Qi4CSConfiguration.Create(ConfigurationManager, Type, ConfigurationCompositeInfo)"/> method for configuration located in specific location and serialized by specific serializer.
   /// </summary>
   /// <typeparam name="TConfiguration">The type of configuration contents.</typeparam>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="configurationLocation">The location of configuration. May be <c>null</c> for default location.</param>
   /// <param name="configurationSerializer">The serializer for configuration. Must implement <see cref="ConfigurationSerializer"/>. May be <c>null</c> for default serializer.</param>
   /// <returns>An instance of <see cref="ConfigurationInstance{T}"/>.</returns>
   /// <remarks>See <see cref="E_Qi4CSConfiguration.Create(ConfigurationManager, Type, ConfigurationCompositeInfo)"/> method for more detailed description about exceptions and return values.</remarks>
   public static ConfigurationInstance<TConfiguration> Create<TConfiguration>( this ConfigurationManager manager, Qi4CSConfigurationResource configurationLocation, Type configurationSerializer )
         where TConfiguration : class
   {
      var info = new ConfigurationCompositeInfo( configurationLocation, configurationSerializer );
      return manager.Create<TConfiguration>( info );
   }

   /// <summary>
   /// Creates a new <see cref="ConfigurationInstance{T}"/> using serializer and resource specified by methods in <see cref="Qi4CS.Extensions.Configuration.Assembling.ConfigurationCompositeDefaultInfo"/> at bootstrap time.
   /// </summary>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="configurationType">The type of the configuration.</param>
   /// <param name="info">The optional <see cref="ConfigurationCompositeInfo"/> containing required information about the contents.</param>
   /// <returns>An instance of <see cref="ConfigurationInstance{T}"/> using serializer and resource specified by <paramref name="info"/> or by methods in <see cref="Qi4CS.Extensions.Configuration.Assembling.ConfigurationCompositeDefaultInfo"/> at bootstrap time.</returns>
   /// <exception cref="InvalidOperationException">
   /// One of the following conditions applies:
   /// <list type="bullet">
   /// <item><description>The <paramref name="info"/> is <c>null</c> and there is no default information specified by methods in <see cref="Qi4CS.Extensions.Configuration.Assembling.ConfigurationCompositeDefaultInfo"/> for configuration contents of type <paramref name="configurationType"/>.</description></item>
   /// <item><description>The exists information about <paramref name="configurationType"/> but the serializer type (<see cref="Qi4CS.Extensions.Configuration.Assembling.ConfigurationCompositeDefaultInfo.SerializedBy(Type)"/>) has not been specified.</description></item>
   /// <item><description>If no serializer type is specified by given <paramref name="info"/> nor at bootstrap time.</description></item>
   /// <item><description>The serializer type information has been acquired, but the type does not implement <see cref="ConfigurationSerializer"/>.</description></item>
   /// </list>
   /// </exception>
   /// <exception cref="Qi4CS.Core.API.Instance.NoSuchCompositeTypeException">If serializer type is not Qi4CS composite or is not visible from the architectural unit this <see cref="ConfigurationManager"/> belongs to.</exception>
   /// <seealso cref="ConfigurationSerializer"/>
   public static ConfigurationInstance<Object> Create( this ConfigurationManager manager, Type configurationType, ConfigurationCompositeInfo info = null )
   {
      return manager.AsConfigurationInstance( configurationType, null, info );
   }

   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="TConfiguration">The type of configuration contents.</typeparam>
   /// <param name="manager"></param>
   /// <param name="info"></param>
   /// <returns></returns>
   public static ConfigurationInstance<TConfiguration> Create<TConfiguration>( this ConfigurationManager manager, ConfigurationCompositeInfo info = null )
      where TConfiguration : class
   {
      return (ConfigurationInstance<TConfiguration>) manager.Create( typeof( TConfiguration ), info );
   }

   /// <summary>
   /// Helper method for invoking the <see cref="ConfigurationManager.AsConfigurationInstance"/> method when type of configuration is known at compile time.
   /// </summary>
   /// <typeparam name="TConfiguration">The type of configuration.</typeparam>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="instance">The composite instance.</param>
   /// <param name="info">The optional configuration information.</param>
   /// <returns>The <see cref="ConfigurationInstance{T}"/> which has initial value of <paramref name="instance"/>.</returns>
   /// <remarks>
   /// See <see cref="ConfigurationManager.AsConfigurationInstance"/> method for more detailed information.
   /// </remarks>
   /// <seealso cref="ConfigurationManager.AsConfigurationInstance"/>
   public static ConfigurationInstance<TConfiguration> AsConfigurationInstance<TConfiguration>( this ConfigurationManager manager, TConfiguration instance, ConfigurationCompositeInfo info = null )
         where TConfiguration : class
   {
      return (ConfigurationInstance<TConfiguration>) manager.AsConfigurationInstance( typeof( TConfiguration ), instance, info );
   }

   /// <summary>
   /// Helper method for checking whether <see cref="ConfigurationManager"/> has default information about configuration type.
   /// </summary>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="configurationType">The type of the configuration.</param>
   /// <returns><c>true</c> if <paramref name="manager"/> has information about the given <paramref name="configurationType"/>; <c>false</c> otherwise.</returns>
   public static Boolean HasDefaultInformationAbout( this ConfigurationManager manager, Type configurationType )
   {
      ConfigurationCompositeInfo info;
      return manager.TryGetDefaultInformationAbout( configurationType, out info );
   }

   /// <summary>
   /// Helper method for checking whether <see cref="ConfigurationManager"/> has default information about configuration type, when the type is known at compile time.
   /// </summary>
   /// <typeparam name="TConfiguration">The type of the configuration.</typeparam>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <returns><c>true</c> if <paramref name="manager"/> has information about the given <typeparamref name="TConfiguration"/>; <c>false</c> otherwise.</returns>
   public static Boolean HasDefaultInformationAbout<TConfiguration>( this ConfigurationManager manager )
      where TConfiguration : class
   {
      return manager.HasDefaultInformationAbout( typeof( TConfiguration ) );
   }

   /// <summary>
   /// Helper method for invoking <see cref="ConfigurationManager.TryGetDefaultInformationAbout"/> and either returning the found information, or throwing an exception.
   /// </summary>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <param name="configurationType">The type of the configuration.</param>
   /// <returns>The <see cref="ConfigurationCompositeInfo"/> associated with the <paramref name="configurationType"/>.</returns>
   /// <exception cref="ArgumentException">If no default information is found for <paramref name="configurationType"/>.</exception>
   public static ConfigurationCompositeInfo GetDefaultInformationAbout( this ConfigurationManager manager, Type configurationType )
   {
      ConfigurationCompositeInfo retVal;
      if ( !manager.TryGetDefaultInformationAbout( configurationType, out retVal ) )
      {
         throw new ArgumentException( "The configuration manager has no default information about type " + configurationType + "." );
      }
      return retVal;
   }

   /// <summary>
   /// Helper method for invoking <see cref="ConfigurationManager.TryGetDefaultInformationAbout"/> and either returning the found information, or throwing an exception.
   /// </summary>
   /// <typeparam name="TConfiguration">The type of the configuration.</typeparam>
   /// <param name="manager">The <see cref="ConfigurationManager"/>.</param>
   /// <returns>The <see cref="ConfigurationCompositeInfo"/> associated with the <typeparamref name="TConfiguration"/>.</returns>
   /// <exception cref="ArgumentException">If no default information is found for <typeparamref name="TConfiguration"/>.</exception>
   public static ConfigurationCompositeInfo GetDefaultInformationAbout<TConfiguration>( this ConfigurationManager manager )
      where TConfiguration : class
   {
      return manager.GetDefaultInformationAbout( typeof( TConfiguration ) );
   }
}