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
using System.Reflection;
using System.Linq;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// A model of a single native event possibly managed by Qi4CS runtime.
   /// </summary>
   public interface EventModel : AbstractMemberInfoModel<EventInfo>, ModelWithAttributes
   {
      /// <summary>
      /// Gets the <see cref="CompositeMethodModel"/> that acts as addition method model for this <see cref="EventModel"/>.
      /// </summary>
      /// <value>The <see cref="CompositeMethodModel"/> that acts as addition method model for this <see cref="EventModel"/>.</value>
      CompositeMethodModel AddMethod { get; }

      /// <summary>
      /// Gets the <see cref="CompositeMethodModel"/> that acts as removal method model for this <see cref="EventModel"/>.
      /// </summary>
      /// <value>The <see cref="CompositeMethodModel"/> that acts as removal method model for this <see cref="EventModel"/>.</value>
      CompositeMethodModel RemoveMethod { get; }
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Checks whether given <see cref="EventModel"/> is non-<c>null</c> and considered to be part of the composite state.
   /// </summary>
   /// <param name="evtModel">The <see cref="EventModel"/>.</param>
   /// <returns><c>true</c> if <paramref name="evtModel"/> is non-<c>null</c> and considered to be part of the composite state; <c>false</c> otherwise.</returns>
   public static Boolean IsPartOfCompositeState( this EventModel evtModel )
   {
      var result = evtModel != null
         && evtModel.AddMethod != null
         && evtModel.RemoveMethod != null;
      if ( result )
      {
         var aMixin = evtModel.AddMethod.Mixin;
         var rMixin = evtModel.RemoveMethod.Mixin;
         result = aMixin != null
            && rMixin != null
            && evtModel.AddMethod.CompositeModel.ApplicationModel.GenericEventMixinType.Equals( aMixin.NativeInfo.DeclaringType )
            && evtModel.RemoveMethod.CompositeModel.ApplicationModel.GenericEventMixinType.Equals( rMixin.NativeInfo.DeclaringType );
      }
      return result;
   }

   /// <summary>
   /// Helper method to get the storage style associated with this <see cref="EventModel"/>.
   /// </summary>
   /// <param name="evtModel">The <see cref="EventModel"/>.</param>
   /// <returns>The <see cref="EventStorage"/> of the <see cref="EventModel"/>.</returns>
   /// <remarks>
   /// This method checks whether <see cref="EventStorageStyleAttribute"/> is defined for this method, and if so, will return the value of <see cref="EventStorageStyleAttribute.Storage"/> property.
   /// Otherwise it will return the value of <see cref="EventStorageStyleAttribute.DEFAULT_STORAGE"/> const.
   /// </remarks>
   public static EventStorage GetEventStorageKind( this EventModel evtModel )
   {
      var storageStyle = evtModel.AllAttributes.OfType<EventStorageStyleAttribute>().FirstOrDefault();
      return storageStyle == null ? EventStorageStyleAttribute.DEFAULT_STORAGE : storageStyle.Storage;
   }
}