/*
 * Copyright (c) 2008, Rickard Öberg.
 * See NOTICE file.
 * 
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
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Bootstrap.Model
{
   /// <summary>
   /// This is model-level structural construct of <see cref="ModuleArchitecture"/>.
   /// </summary>
   public interface ModuleModel : ValidatableItem, ModelContainer
   {
      /// <summary>
      /// Gets the <see cref="LayerModel"/> this <see cref="ModuleModel"/> belongs to.
      /// </summary>
      /// <value>The <see cref="LayerModel"/> this <see cref="ModuleModel"/> belongs to.</value>
      LayerModel LayerModel { get; }

      /// <summary>
      /// Gets the name of this <see cref="ModuleModel"/>.
      /// </summary>
      /// <value>The name of this <see cref="ModuleModel"/>.</value>
      String Name { get; }

      /// <summary>
      /// Gets the <see cref="Visibility"/> of a <see cref="CompositeModel"/> belonging to this <see cref="ModuleModel"/>.
      /// </summary>
      /// <param name="model"><see cref="CompositeModel"/> which belongs to this <see cref="ModuleModel"/>.</param>
      /// <returns>The <see cref="Visibility"/> of <paramref name="model"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="model"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If <paramref name="model"/> does not belong to this <see cref="ModuleModel"/>.</exception>
      Visibility GetVisibility( CompositeModel model );
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Helper method to get <see cref="Visibility"/> of <see cref="CompositeModel"/> which is within <see cref="LayeredApplicationModel"/>.
   /// </summary>
   /// <param name="model">The <see cref="CompositeModel"/>.</param>
   /// <returns>The <see cref="Visibility"/> of <paramref name="model"/>.</returns>
   /// <exception cref="ArgumentNullException">If <paramref name="model"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">If the <see cref="CompositeModel.ApplicationModel"/> of <paramref name="model"/> is not instance of <see cref="LayeredApplicationModel"/> or if the <see cref="ModuleModel"/> of <paramref name="model"/> could not be found.</exception>
   public static Visibility GetVisibilityWithinLayeredApplication( this CompositeModel model )
   {
      ArgumentValidator.ValidateNotNull( "Composite model", model );
      if ( model.ApplicationModel is LayeredApplicationModel )
      {
         var modMod = ( (LayeredApplicationModel) model.ApplicationModel ).FindModuleModel( model );
         if ( modMod == null )
         {
            throw new InvalidOperationException( "Could not find module model for " + model + "." );
         }
         else
         {
            return modMod.GetVisibility( model );
         }
      }
      else
      {
         throw new InvalidOperationException( "The application model of composite model must be of type " + typeof( LayeredApplicationModel ) + "." );
      }
   }
}