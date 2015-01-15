/*
 * Copyright 2011 Stanislav Muhametsin. All rights Reserved.
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
using System.Collections.Generic;
using System.Linq;
using CollectionsWithRoles.API;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// A model for single composite method.
   /// This interface contains all information about composite method invocation chain of a single native method.
   /// </summary>
   public interface CompositeMethodModel : AbstractMethodModel, AbstractModelWithParameters
   {
      /// <summary>
      /// Gets all the <see cref="ConcernMethodModel"/>s of this <see cref="CompositeMethodModel"/>.
      /// </summary>
      /// <value>All the <see cref="ConcernMethodModel"/>s of this <see cref="CompositeMethodModel"/>.</value>
      /// <remarks>The order of invocation of concerns will be same as the order of the models in the list returned by this property.</remarks>
      ListQuery<ConcernMethodModel> Concerns { get; }

      /// <summary>
      /// Gets the <see cref="MixinMethodModel"/> of this <see cref="CompositeMethodModel"/>.
      /// </summary>
      /// <value>The <see cref="MixinMethodModel"/> of this <see cref="CompositeMethodModel"/>.</value>
      MixinMethodModel Mixin { get; }

      /// <summary>
      /// Gets all the <see cref="SideEffectMethodModel"/>s of this <see cref="CompositeMethodModel"/>.
      /// </summary>
      /// <value>All the <see cref="SideEffectMethodModel"/>s of this <see cref="CompositeMethodModel"/>.</value>
      /// <remarks>The order of invocation of side effects will be same as the order of the models in the list returned by this property.</remarks>
      ListQuery<SideEffectMethodModel> SideEffects { get; }

      /// <summary>
      /// Gets the <see cref="ParameterModel"/> representing information about method's result parameter.
      /// </summary>
      /// <value>The <see cref="ParameterModel"/> representing information about method's result parameter.</value>
      ParameterModel Result { get; }

      /// <summary>
      /// Gets the index of this <see cref="CompositeMethodModel"/> in <see cref="CompositeModel.Methods"/> property.
      /// </summary>
      /// <value>The index of this <see cref="CompositeMethodModel"/> in <see cref="CompositeModel.Methods"/> property.</value>
      Int32 MethodIndex { get; }

      /// <summary>
      /// Gets the <see cref="Model.PropertyModel"/> associated with this <see cref="CompositeMethodModel"/>.
      /// May be <c>null</c> if this <see cref="CompositeMethodModel"/> is not associated with any <see cref="Model.PropertyModel"/>.
      /// </summary>
      /// <value>The <see cref="Model.PropertyModel"/> associated with this <see cref="CompositeMethodModel"/> or <c>null</c>.</value>
      PropertyModel PropertyModel { get; }

      /// <summary>
      /// Gets the <see cref="Model.EventModel"/> associated with this <see cref="CompositeMethodModel"/>.
      /// May be <c>null</c> if this <see cref="CompositeMethodModel"/> is not associated with any <see cref="Model.EventModel"/>.
      /// </summary>
      /// <value>The <see cref="Model.EventModel"/> associated with this <see cref="CompositeMethodModel"/> or <c>null</c>.</value>
      EventModel EventModel { get; }
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Gets enumerable of all method models of the <see cref="CompositeMethodModel"/>, including itself.
   /// The other method models are concatenated values of properties <see cref="CompositeMethodModel.Concerns"/>, <see cref="CompositeMethodModel.SideEffects"/> and <see cref="CompositeMethodModel.Mixin"/>.
   /// </summary>
   /// <param name="cModel">The <see cref="CompositeMethodModel"/>.</param>
   /// <returns>The enumerable of all method models of the <paramref name="cModel"/>, including <paramref name="cModel"/> itself.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="cModel"/> is <c>null</c>.</exception>
   public static IEnumerable<AbstractMemberInfoModel<System.Reflection.MethodBase>> GetAllMethodModels( this CompositeMethodModel cModel )
   {
      return Enumerable.Repeat( cModel, 1 ).Concat( cModel.Concerns.Cast<AbstractMethodModel>() ).Concat( cModel.SideEffects ).Concat( Enumerable.Repeat( cModel.Mixin, 1 ) );
   }
}
