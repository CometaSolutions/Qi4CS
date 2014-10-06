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

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This interface represents structural Qi4CS element which can act as owner for a <see cref="CompositeInstance"/>.
   /// </summary>
   public interface CompositeInstanceStructureOwner
   {
      /// <summary>
      /// Gets the <see cref="ApplicationSPI"/> this <see cref="CompositeInstanceStructureOwner"/> belongs to.
      /// </summary>
      /// <value>The <see cref="ApplicationSPI"/> this <see cref="CompositeInstanceStructureOwner"/> belongs to.</value>
      ApplicationSPI Application { get; }

      /// <summary>
      /// Gets the <see cref="ModelInfoContainer"/> of this <see cref="CompositeInstanceStructureOwner"/>.
      /// </summary>
      /// <value>The <see cref="ModelInfoContainer"/> of this <see cref="CompositeInstanceStructureOwner"/>.</value>
      ModelInfoContainer ModelInfoContainer { get; }

      /// <summary>
      /// Gets the <see cref="StructureServiceProviderSPI"/> of this <see cref="CompositeInstanceStructureOwner"/>.
      /// </summary>
      /// <value>The <see cref="StructureServiceProviderSPI"/> of this <see cref="CompositeInstanceStructureOwner"/>.</value>
      StructureServiceProviderSPI StructureServices { get; }
   }
}