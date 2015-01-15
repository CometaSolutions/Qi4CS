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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This is common interface for all models of reflection elements that can be injected by Qi4CS.
   /// </summary>
   /// <seealso cref="ParameterModel"/>
   /// <seealso cref="FieldModel"/>
   public interface AbstractInjectableModel : ModelWithAttributes, OptionalInfo
   {
      /// <summary>
      /// Gets the injection scope attribute of this injectable model.
      /// </summary>
      /// <value>The injection scope attribute of this injectable model.</value>
      Attribute InjectionScope { get; }

      /// <summary>
      /// Gets the target type of the injection.
      /// </summary>
      /// <value>The target type of the injection.</value>
      /// <remarks>If field or parameter type is <see cref="Lazy{T}"/>, the target type will be the generic argument of the field or parameter type.</remarks>
      Type TargetType { get; }

      /// <summary>
      /// Gets the declaring type of the reflection element modeled by this model.
      /// </summary>
      /// <value>The declaring type of the reflection element modeled by this model.</value>
      Type DeclaringType { get; }

      /// <summary>
      /// Gets the <see cref="CompositeModel"/> this injectable model belongs to.
      /// </summary>
      /// <value>The <see cref="CompositeModel"/> this injectable model belongs to.</value>
      CompositeModel CompositeModel { get; }
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Checks whether given <see cref="AbstractInjectableModel"/> is non-<c>null</c> and related to a given fragment type.
   /// </summary>
   /// <param name="model">The <see cref="AbstractInjectableModel"/>.</param>
   /// <param name="fragmentType">The fragment type.</param>
   /// <returns><c>true</c> if <paramref name="model"/> is non-<c>null</c> and <see cref="AbstractInjectableModel.DeclaringType"/> or generic definition of it if it is generic type matches <paramref name="fragmentType"/>; <c>false</c> otherwise.</returns>
   public static Boolean IsRelatedToFragment( this AbstractInjectableModel model, Type fragmentType )
   {
      var typeToCompare = model == null ? null : model.DeclaringType;
      return typeToCompare != null && typeToCompare.GetGenericDefinitionIfGenericType().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( fragmentType );
   }

   /// <summary>
   /// Gets the injection time of given <see cref="AbstractInjectableModel"/>.
   /// </summary>
   /// <param name="model">The <see cref="AbstractInjectableModel"/>.</param>
   /// <returns>The injection time for <paramref name="model"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="model"/> is <c>null</c>.</exception>
   /// <seealso cref="InjectionTime"/>
   public static InjectionTime GetInjectionTime( this AbstractInjectableModel model )
   {
      return model.CompositeModel.ApplicationModel.InjectionService.GetInjectionTime( model );
   }

   /// <summary>
   /// Checks whether given <see cref="AbstractInjectableModel"/> is non-<c>null</c> and the injectable value will be dependant on fragment instance.
   /// </summary>
   /// <param name="model">The <see cref="AbstractInjectableModel"/>.</param>
   /// <returns><c>true</c> if <paramref name="model"/> is non-<c>null</c> and the injection scope attribute has <see cref="FragmentDependentInjectionAttribute"/> applied to it; <c>false</c> otherwise.</returns>
   public static Boolean IsFragmentDependant( this AbstractInjectableModel model )
   {
      return model != null && model.InjectionScope != null && model.GetAttributesOfAttribute( model.InjectionScope.GetType() ).ContainsKey( typeof( FragmentDependentInjectionAttribute ) );
   }
}