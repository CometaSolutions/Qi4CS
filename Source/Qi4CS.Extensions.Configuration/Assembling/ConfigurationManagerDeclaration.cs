/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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

namespace Qi4CS.Extensions.Configuration.Assembling
{
   /// <summary>
   /// This interface may be used to set up information for the <see cref="Instance.ConfigurationManager"/>, inclusing defaults to use for certain configuration types.
   /// Instances of this interface are obtained via <see cref="E_Qi4CSConfiguration.AddSupportForConfigurationManager( Assembler )"/>, <see cref="E_Qi4CSConfiguration.AddSupportForAllConfigurationInstancesAndManager(Assembler)"/> or <see cref="E_Qi4CSConfiguration.AddSupportForSpecificConfigurationInstancesAndManager(Assembler, Type[])"/> extension methods.
   /// </summary>
   public interface ConfigurationManagerDeclaration
   {
      /// <summary>
      /// Gets the <see cref="ConfigurationCompositeDefaultInfo"/> which can be used to set up some default values for given configuration contents types.
      /// </summary>
      /// <param name="types">One or more configuration contents types.</param>
      /// <returns>The <see cref="ConfigurationCompositeDefaultInfo"/> which can be used to set up some default values for configuration contents types.</returns>
      /// <seealso cref="ConfigurationCompositeDefaultInfo"/>
      /// <remarks>If <paramref name="types"/> is <c>null</c> or empty, the methods in resulting <see cref="ConfigurationCompositeDefaultInfo"/> will have no effect.</remarks>
      ConfigurationCompositeDefaultInfo WithDefaultsFor( params Type[] types );

      /// <summary>
      /// Gets the <see cref="ServiceCompositeAssemblyDeclaration"/> of the <see cref="Instance.ConfigurationManager"/> service.
      /// </summary>
      /// <value>The <see cref="ServiceCompositeAssemblyDeclaration"/> of the <see cref="Instance.ConfigurationManager"/> service.</value>
      /// <seealso cref="Instance.ConfigurationManager"/>
      ServiceCompositeAssemblyDeclaration ServiceDeclaration { get; }
   }
}
