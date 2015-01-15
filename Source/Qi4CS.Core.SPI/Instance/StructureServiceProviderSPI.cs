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
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This is the SPI version of <see cref="StructureServiceProvider"/>.
   /// It provides additional information about the <see cref="StructureServiceProvider"/>.
   /// </summary>
   /// <remarks>
   /// All instances of <see cref="StructureServiceProvider"/> are castable to <see cref="StructureServiceProviderSPI"/>.
   /// </remarks>
   public interface StructureServiceProviderSPI : StructureServiceProvider
   {
      /// <summary>
      /// Gets the <see cref="CompositeInstanceStructureOwner"/> associated with this <see cref="StructureServiceProviderSPI"/>.
      /// </summary>
      /// <value>The <see cref="CompositeInstanceStructureOwner"/> associated with this <see cref="StructureServiceProviderSPI"/>.</value>
      CompositeInstanceStructureOwner Structure { get; }
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Helper to create a new <see cref="CompositeBuilder"/> from a specific <see cref="CompositeModel"/>.
   /// </summary>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <param name="model">The <see cref="CompositeModel"/> of the composite.</param>
   /// <returns>A <see cref="CompositeBuilder"/> to build instances of composites modeled by <see cref="CompositeModel"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>See <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, System.Collections.Generic.IEnumerable{Type})"/> method for more exception scenarios.</remarks>
   public static CompositeBuilder NewCompositeBuilder( this StructureServiceProvider ssp, CompositeModel model )
   {
      return ssp.NewCompositeBuilder( model.ModelType, model.PublicTypes );
   }
}

