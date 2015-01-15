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
using System.Linq;
using System.Reflection;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// A model of a single native property possibly managed by Qi4CS runtime.
   /// </summary>
   public interface PropertyModel : AbstractMemberInfoModel<PropertyInfo>, ModelWithAttributes, OptionalInfo
   {
      /// <summary>
      /// Gets the <see cref="CompositeMethodModel"/> that acts as getter method model for this <see cref="PropertyModel"/>.
      /// </summary>
      /// <value>The <see cref="CompositeMethodModel"/> that acts as getter method model for this <see cref="PropertyModel"/>.</value>
      CompositeMethodModel GetterMethod { get; }

      /// <summary>
      /// Gets the <see cref="CompositeMethodModel"/> that acts as setter method model for this <see cref="PropertyModel"/>.
      /// </summary>
      /// <value>The <see cref="CompositeMethodModel"/> that acts as setter method model for this <see cref="PropertyModel"/>.</value>
      CompositeMethodModel SetterMethod { get; }

      /// <summary>
      /// Gets the value indicating whether this <see cref="PropertyModel"/> is considered to represent immutable composite property.
      /// </summary>
      /// <value><c>true</c> if this <see cref="PropertyModel"/> is part of composite state and is considered to represent immutable composite property; <c>false</c> otherwise.</value>
      Boolean IsImmutable { get; }

      /// <summary>
      /// Gets the custom default value creator callback for this <see cref="PropertyModel"/>.
      /// May be <c>null</c> if this <see cref="PropertyModel"/> has no custom default value creator callback.
      /// </summary>
      /// <value>the custom default value creator callback for this <see cref="PropertyModel"/> or <c>null</c>.</value>
      /// <remarks>
      /// The callback takes runtime <see cref="PropertyInfo"/> and <see cref="ApplicationSPI"/> as parameter, and should return property value of correct type.
      /// </remarks>
      Func<PropertyInfo, ApplicationSPI, Object> DefaultValueCreator { get; }
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Checks whether given <see cref="PropertyModel"/> is non-<c>null</c> and has <see cref="UseDefaultsAttribute"/> attribute.
   /// </summary>
   /// <param name="pModel">The <see cref="PropertyModel"/>.</param>
   /// <returns><c>true</c> if <paramref name="pModel"/> is non-<c>null</c> and has <see cref="UseDefaultsAttribute"/> attribute; <c>false</c> otherwise.</returns>
   public static Boolean IsUseDefaults( this PropertyModel pModel )
   {
      return pModel != null && pModel.AllAttributes.OfType<UseDefaultsAttribute>().Any();
   }

   /// <summary>
   /// Checks whether given <see cref="PropertyModel"/> is non-<c>null</c> and has <see cref="UseDefaultsAttribute"/> attribute, and setting that attribute to <paramref name="attr"/> if result is <c>true</c>.
   /// </summary>
   /// <param name="pModel">The <see cref="PropertyModel"/>.</param>
   /// <param name="attr">This parameter will contain the instance of <see cref="UseDefaultsAttribute"/> if return value is <c>true</c>; otherwise it will be <c>null</c>.</param>
   /// <returns><c>true</c> if <paramref name="pModel"/> is non-<c>null</c> and has <see cref="UseDefaultsAttribute"/> attribute; <c>false</c> otherwise.</returns>
   public static Boolean IsUseDefaults( this PropertyModel pModel, out UseDefaultsAttribute attr )
   {
      attr = pModel == null ? null : pModel.AllAttributes.OfType<UseDefaultsAttribute>().FirstOrDefault();
      return attr != null;
   }

   /// <summary>
   /// Checks whether given <see cref="PropertyModel"/> is non-<c>null</c> and considered to be part of the composite state.
   /// </summary>
   /// <param name="pModel">The <see cref="PropertyModel"/>.</param>
   /// <returns><c>true</c> if <paramref name="pModel"/> is non-<c>null</c> and considered to be part of the composite state; <c>false</c> otherwise.</returns>
   public static Boolean IsPartOfCompositeState( this PropertyModel pModel )
   {
      var result = pModel != null
         && pModel.GetterMethod != null
         && pModel.SetterMethod != null;
      if ( result )
      {
         var sMixin = pModel.SetterMethod.Mixin;
         var gMixin = pModel.GetterMethod.Mixin;
         result = sMixin != null
            && gMixin != null
            && pModel.SetterMethod.CompositeModel.ApplicationModel.GenericPropertyMixinType.Equals( sMixin.NativeInfo.DeclaringType )
            && pModel.GetterMethod.CompositeModel.ApplicationModel.GenericPropertyMixinType.Equals( gMixin.NativeInfo.DeclaringType );
      }
      return result;
   }
}
