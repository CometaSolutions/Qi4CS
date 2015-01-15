using Qi4CS.Core.API.Model;
/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
using System.Text;

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// This attribute may be applied on a method, property or event, which is not public but should be considered as part of composite nevertheless.
   /// By default, only public components are considered to be part of composite.
   /// </summary>
   [AttributeUsage( AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = false )]
   public class CompositeMethodAttribute : Attribute
   {
   }

   /// <summary>
   /// This attribute may be applied on a class to specify the visibility of the methods which are picked up as composite methods.
   /// </summary>
   [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
   public class DefaultCompositeMethodVisibilityAttribute : Attribute
   {
      private readonly CompositeMethodVisiblity _visibility;

      /// <summary>
      /// Creates a new instance of <see cref="DefaultCompositeMethodVisibilityAttribute"/> with given <see cref="Model.CompositeMethodVisiblity"/> flags.
      /// </summary>
      /// <param name="visibility">The ORed combination of composite method visibility flags.</param>
      public DefaultCompositeMethodVisibilityAttribute( CompositeMethodVisiblity visibility )
      {
         this._visibility = visibility;
      }

      /// <summary>
      /// Gets the <see cref="Model.CompositeMethodVisiblity"/> specified to this attribute.
      /// </summary>
      /// <value>The <see cref="Model.CompositeMethodVisiblity"/> specified to this attribute.</value>
      public CompositeMethodVisiblity CompositeMethodVisibility
      {
         get
         {
            return this._visibility;
         }
      }
   }

   /// <summary>
   /// This enumeration controls the default composite method visibility for some type and its sub-types.
   /// One or more value of this enum may be specified to <see cref="DefaultCompositeMethodVisibilityAttribute(CompositeMethodVisiblity)"/> constructor by combining the values with <c>|</c> operator.
   /// </summary>
   [Flags]
   public enum CompositeMethodVisiblity
   {
      /// <summary>
      /// Methods marked as <c>public</c> will be considered as composite methods.
      /// </summary>
      Public = 1,

      /// <summary>
      /// Methods marked as <c>internal</c> modifier will be considered as composite methods.
      /// </summary>
      Internal = 2,

      /// <summary>
      /// Methods marked as <c>protected</c> modifier will be considered as composite methods.
      /// </summary>
      Protected = 4,

      /// <summary>
      /// Methods marked as <c>protected internal</c> will be considered as composite methods.
      /// </summary>
      ProtectedOrInternal = 8,

      /// <summary>
      /// Methods emitted with <c>FamANDAssem</c> modifier will be considered as composite methods.
      /// </summary>
      ProtectedAndInternal = 16,
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Checks that given a certain <see cref="CompositeMethodVisiblity"/>, given <see cref="System.Reflection.MethodInfo"/> may be considered as composite method.
   /// </summary>
   /// <param name="visibility">The <see cref="CompositeMethodVisiblity"/>.</param>
   /// <param name="method">The <see cref="System.Reflection.MethodInfo"/> to check.</param>
   /// <returns><c>true</c> if given a <paramref name="visibility"/>, the <paramref name="method"/> should be considered as composite method; <c>false</c> otherwise.</returns>
   public static Boolean MethodConsideredToBeCompositeMethod( this CompositeMethodVisiblity visibility, System.Reflection.MethodInfo method )
   {
      return ( visibility.IsPublic() && method.IsPublic )
         || ( visibility.IsInternal() && method.IsAssembly )
         || ( visibility.IsProtected() && method.IsFamily )
         || ( visibility.IsProtectedOrInternal() && method.IsFamilyOrAssembly )
         || ( visibility.IsProtectedAndInternal() && method.IsFamilyAndAssembly );
   }

   /// <summary>
   /// Checks whether given <see cref="CompositeMethodVisiblity"/> has <see cref="CompositeMethodVisiblity.Public"/> flag set.
   /// </summary>
   /// <param name="visibility">The <see cref="CompositeMethodVisiblity"/>.</param>
   /// <returns><c>true</c> if <paramref name="visibility"/> has <see cref="CompositeMethodVisiblity.Public"/> flag set; <c>false</c> otherwise.</returns>
   public static Boolean IsPublic( this CompositeMethodVisiblity visibility )
   {
      return ( visibility & CompositeMethodVisiblity.Public ) != 0;
   }

   /// <summary>
   /// Checks whether given <see cref="CompositeMethodVisiblity"/> has <see cref="CompositeMethodVisiblity.Internal"/> flag set.
   /// </summary>
   /// <param name="visibility">The <see cref="CompositeMethodVisiblity"/>.</param>
   /// <returns><c>true</c> if <paramref name="visibility"/> has <see cref="CompositeMethodVisiblity.Internal"/> flag set; <c>false</c> otherwise.</returns>
   public static Boolean IsInternal( this CompositeMethodVisiblity visibility )
   {
      return ( visibility & CompositeMethodVisiblity.Internal ) != 0;
   }

   /// <summary>
   /// Checks whether given <see cref="CompositeMethodVisiblity"/> has <see cref="CompositeMethodVisiblity.Protected"/> flag set.
   /// </summary>
   /// <param name="visibility">The <see cref="CompositeMethodVisiblity"/>.</param>
   /// <returns><c>true</c> if <paramref name="visibility"/> has <see cref="CompositeMethodVisiblity.Protected"/> flag set; <c>false</c> otherwise.</returns>
   public static Boolean IsProtected( this CompositeMethodVisiblity visibility )
   {
      return ( visibility & CompositeMethodVisiblity.Protected ) != 0;
   }

   /// <summary>
   /// Checks whether given <see cref="CompositeMethodVisiblity"/> has <see cref="CompositeMethodVisiblity.ProtectedOrInternal"/> flag set.
   /// </summary>
   /// <param name="visibility">The <see cref="CompositeMethodVisiblity"/>.</param>
   /// <returns><c>true</c> if <paramref name="visibility"/> has <see cref="CompositeMethodVisiblity.ProtectedOrInternal"/> flag set; <c>false</c> otherwise.</returns>
   public static Boolean IsProtectedOrInternal( this CompositeMethodVisiblity visibility )
   {
      return ( visibility & CompositeMethodVisiblity.ProtectedOrInternal ) != 0;
   }

   /// <summary>
   /// Checks whether given <see cref="CompositeMethodVisiblity"/> has <see cref="CompositeMethodVisiblity.ProtectedAndInternal"/> flag set.
   /// </summary>
   /// <param name="visibility">The <see cref="CompositeMethodVisiblity"/>.</param>
   /// <returns><c>true</c> if <paramref name="visibility"/> has <see cref="CompositeMethodVisiblity.ProtectedAndInternal"/> flag set; <c>false</c> otherwise.</returns>
   public static Boolean IsProtectedAndInternal( this CompositeMethodVisiblity visibility )
   {
      return ( visibility & CompositeMethodVisiblity.ProtectedAndInternal ) != 0;
   }
}
