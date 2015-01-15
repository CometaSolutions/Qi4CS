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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Extensions.Configuration.Assembling;
using Qi4CS.Extensions.Configuration.Instance;

/// <summary>
/// This class contains extension methods for types related to Qi4CS Configuration extension.
/// </summary>
public static partial class E_Qi4CSConfiguration
{
   /// <summary>
   /// Adds support for creating composites of type <see cref="ConfigurationInstance{T}"/> with any type as first generic type argument.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/> to add contents to.</param>
   /// <returns>The <see cref="PlainCompositeAssemblyDeclaration"/> of the added configuration instance contents.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="assembler"/> is <c>null</c>.</exception>
   /// <remarks>
   /// Usually this method is not invoked directly but the method <see cref="AddSupportForAllConfigurationInstancesAndManager(Assembler)"/> is used.
   /// </remarks>
   public static PlainCompositeAssemblyDeclaration AddSupportForAllConfigurationInstances( this Assembler assembler )
   {
      PlainCompositeAssemblyDeclaration decl;
      if ( assembler.ForNewOrExistingPlainComposite( new Type[] { typeof( ConfigurationInstance<> ) }, out decl ) )
      {
         decl.WithMixins( typeof( ConfigurationInstanceMixin<> ) );
      }
      return decl;
   }

   /// <summary>
   /// Adds support for creating contents of type <see cref="ConfigurationInstance{T}"/> with any of the given types first generic type argument.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/> to add composites to.</param>
   /// <param name="configTypes">
   /// The types that may be used as generic argument for <see cref="ConfigurationInstance{T}"/>.
   /// If the array is <c>null</c> or empty, this method will have no effect.
   /// </param>
   /// <returns>
   /// The <see cref="PlainCompositeAssemblyDeclaration"/>s of the configuration instance composites.
   /// Notice that the resulting array may be of different size if <paramref name="configTypes"/> was <c>null</c> or contained <c>null</c>s.
   /// </returns>
   /// <exception cref="NullReferenceException">If <paramref name="assembler"/> is <c>null</c>.</exception>
   /// <remarks>
   /// Usually this method is not invoked directly but the method <see cref="AddSupportForSpecificConfigurationInstancesAndManager(Assembler, Type[])"/> is used.
   /// </remarks>
   public static PlainCompositeAssemblyDeclaration[] AddSupportForSpecificConfigurationInstances( this Assembler assembler, params Type[] configTypes )
   {
      PlainCompositeAssemblyDeclaration decl;
      configTypes = configTypes.FilterNulls();
      var result = new PlainCompositeAssemblyDeclaration[configTypes.Length];

      for ( var i = 0; i < configTypes.Length; ++i )
      {
         var cType = configTypes[i];
         if ( assembler.ForNewOrExistingPlainComposite( new Type[] { typeof( ConfigurationInstance<> ).MakeGenericType( cType ) }, out decl ) )
         {
            decl.WithMixins( typeof( ConfigurationInstanceMixin<> ).MakeGenericType( cType ) );
         }
         result[i] = decl;
      }
      return result;
   }

   /// <summary>
   /// Adds support for using <see cref="ConfigurationManager"/> service in <see cref="Assembler"/>.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/> to add service contents to.</param>
   /// <returns>The <see cref="ConfigurationManagerDeclaration"/> object to further set up the <see cref="ConfigurationManager"/> service.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="assembler"/> is <c>null</c>.</exception>
   /// <seealso cref="ConfigurationManagerDeclaration"/>
   /// <remarks>
   /// If <paramref name="assembler"/> is <see cref="LayeredCompositeAssembler"/>, the service will have <see cref="Visibility.APPLICATION"/> visibility.
   /// It is possible to change visibility through <see cref="ConfigurationManagerDeclaration.ServiceDeclaration"/> property.
   /// </remarks>
   public static ConfigurationManagerDeclaration AddSupportForConfigurationManager( this Assembler assembler )
   {
      ServiceCompositeAssemblyDeclaration sDecl;
      ConfigurationManagerInfo info;
      if ( assembler.ForNewOrExistingService( new Type[] { typeof( ConfigurationManager ) }, out sDecl ) )
      {
         sDecl.WithMixins( typeof( ConfigurationManagerMixin ) );
         info = new ConfigurationManagerInfo();
         sDecl.Use( info );
         if ( sDecl is LayeredAbstractCompositeAssemblyDeclaration )
         {
            ( (LayeredAbstractCompositeAssemblyDeclaration) sDecl ).VisibleIn( Visibility.APPLICATION );
         }
      }
      else
      {
         info = sDecl.Get<ConfigurationManagerInfo>();
      }
      return new ConfigurationManagerDeclarationImpl( sDecl, info );
   }

   /// <summary>
   /// Helper method to invoke <see cref="AddSupportForAllConfigurationInstances(Assembler)"/> and then <see cref="AddSupportForConfigurationManager(Assembler)"/> and return the resulting <see cref="ConfigurationManagerDeclaration"/>.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/> to add composites to.</param>
   /// <returns><inheritdoc cref="AddSupportForConfigurationManager(Assembler)" /></returns>
   /// <exception cref="NullReferenceException"><inheritdoc cref="AddSupportForConfigurationManager(Assembler)" /></exception>
   /// <seealso cref="ConfigurationManagerDeclaration"/>
   public static ConfigurationManagerDeclaration AddSupportForAllConfigurationInstancesAndManager( this Assembler assembler )
   {
      return assembler.AddSupportForAllConfigurationInstances().Assembler.AddSupportForConfigurationManager();
   }

   /// <summary>
   /// Helper method to invoke <see cref="AddSupportForSpecificConfigurationInstances(Assembler, Type[])"/> and then <see cref="AddSupportForConfigurationManager(Assembler)"/> and return the resulting <see cref="ConfigurationManagerDeclaration"/>.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/> to add composites to.</param>
   /// <param name="configTypes"><inheritdoc cref="AddSupportForSpecificConfigurationInstances(Assembler, Type[])" /></param>
   /// <returns><inheritdoc cref="AddSupportForConfigurationManager(Assembler)" /></returns>
   /// <exception cref="NullReferenceException"><inheritdoc cref="AddSupportForConfigurationManager(Assembler)" /></exception>
   /// <seealso cref="ConfigurationManagerDeclaration"/>
   public static ConfigurationManagerDeclaration AddSupportForSpecificConfigurationInstancesAndManager( this Assembler assembler, params Type[] configTypes )
   {
      assembler.AddSupportForSpecificConfigurationInstances( configTypes );
      return assembler.AddSupportForConfigurationManager();
   }
}

