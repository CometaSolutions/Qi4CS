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

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This interface extends <see cref="AbstractCompositeAssemblyDeclaration"/> to provide some methods specific for assembling service composites.
   /// </summary>
   public interface ServiceCompositeAssemblyDeclaration : AbstractCompositeAssemblyDeclaration
   {
      /// <summary>
      /// Specifies custom textual service ID for the service composites affected by this <see cref="ServiceCompositeAssemblyDeclaration"/>.
      /// </summary>
      /// <param name="serviceID">The textual service ID. May be <c>null</c> to let Qi4CS runtime auto-generate the ID.</param>
      /// <returns>This <see cref="ServiceCompositeAssemblyDeclaration"/>.</returns>
      ServiceCompositeAssemblyDeclaration WithServiceID( String serviceID );

      /// <summary>
      /// Specifies whether service composites affected by this <see cref="ServiceCompositeAssemblyDeclaration"/> should be activated at the same time when the <see cref="SPI.Instance.ApplicationSPI"/> they belong to is activated.
      /// </summary>
      /// <param name="activateWithApplication"><c>true</c> if the service composites should be activated at the same time when the <see cref="SPI.Instance.ApplicationSPI"/> they belong to is activated; <c>false</c> otherwise.</param>
      /// <returns>This <see cref="ServiceCompositeAssemblyDeclaration"/></returns>
      ServiceCompositeAssemblyDeclaration SetActivateWithApplication( Boolean activateWithApplication );
   }
}
