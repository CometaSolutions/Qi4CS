/*
 * Copyright (c) 2008, Niclas Hedhman.
 * See NOTICE file.
 * 
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
using System.Reflection;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Core.Bootstrap.Assembling
{
   using Qi4CS.Core.SPI.Common;
   using TDefaultCreator = Func<PropertyInfo, ApplicationSPI, Object>;

   /// <summary>
   /// This interface represents a declaration for zero or more composites.
   /// It is acquired via methods in <see cref="Assembler"/> interface.
   /// </summary>
   public interface AbstractCompositeAssemblyDeclaration : UsesProvider<AbstractCompositeAssemblyDeclaration>, UsesProviderQuery
   {
      /// <summary>
      /// Adds types to be used as source of mixin methods for this <see cref="AbstractCompositeAssemblyDeclaration"/>.
      /// </summary>
      /// <param name="mixinTypes">The mixin types.</param>
      /// <returns>A <see cref="FragmentAssemblyDeclaration"/> representing the given types.</returns>
      /// <remarks>
      /// If <paramref name="mixinTypes"/> is <c>null</c>, the methods called on returned <see cref="FragmentAssemblyDeclaration"/> will have no effect.
      /// Any <c>null</c> value in the <paramref name="mixinTypes"/> is ignored.
      /// </remarks>
      FragmentAssemblyDeclaration WithMixins( params Type[] mixinTypes );

      /// <summary>
      /// Adds types to be used as source of concern methods for this <see cref="AbstractCompositeAssemblyDeclaration"/>.
      /// </summary>
      /// <param name="concernTypes">The concern types.</param>
      /// <returns>A <see cref="FragmentAssemblyDeclaration"/> representing the given types.</returns>
      /// <remarks>
      /// If <paramref name="concernTypes"/> is <c>null</c>, the methods called on returned <see cref="FragmentAssemblyDeclaration"/> will have no effect.
      /// Any <c>null</c> value in the <paramref name="concernTypes"/> is ignored.
      /// </remarks>
      FragmentAssemblyDeclaration WithConcerns( params Type[] concernTypes );

      /// <summary>
      /// Adds types to be used as source of side-effect methods for this <see cref="AbstractCompositeAssemblyDeclaration"/>.
      /// </summary>
      /// <param name="sideEffectTypes">The side-effect types.</param>
      /// <returns>A <see cref="FragmentAssemblyDeclaration"/> representing the given types.</returns>
      /// <remarks>
      /// If <paramref name="sideEffectTypes"/> is <c>null</c>, the methods called on returned <see cref="FragmentAssemblyDeclaration"/> will have no effect.
      /// Any <c>null</c> value in the <paramref name="sideEffectTypes"/> is ignored.
      /// </remarks>
      FragmentAssemblyDeclaration WithSideEffects( params Type[] sideEffectTypes );

      /// <summary>
      /// Adds types to be used as constraint implementations for this <see cref="AbstractCompositeAssemblyDeclaration"/>.
      /// </summary>
      /// <param name="constraintTypes">The constraint types (should implement <see cref="API.Instance.Constraint{T,U}"/> interface).</param>
      /// <returns>This <see cref="AbstractCompositeAssemblyDeclaration"/>.</returns>
      /// <remarks>
      /// If <paramref name="constraintTypes"/> is <c>null</c>, this method does nothing.
      /// Any <c>null</c> value in the <paramref name="constraintTypes"/> is ignored.
      /// </remarks>
      AbstractCompositeAssemblyDeclaration WithConstraints( params Type[] constraintTypes );

      /// <summary>
      /// Adds publicly visible types to this composite.
      /// </summary>
      /// <param name="types">The types to add.</param>
      /// <returns>This <see cref="AbstractCompositeAssemblyDeclaration"/>.</returns>
      /// <remarks>
      /// If <paramref name="types"/> is <c>null</c>, this method does nothing.
      /// Any <c>null</c> value in the <paramref name="types"/> is ignored.
      /// </remarks>
      AbstractCompositeAssemblyDeclaration OfTypes( params Type[] types );

      /// <summary>
      /// Gets the IDs of composites affected by this <see cref="AbstractCompositeAssemblyDeclaration"/>.
      /// </summary>
      /// <value>The IDs of composites affected by this <see cref="AbstractCompositeAssemblyDeclaration"/>.</value>
      /// <remarks>
      /// <para>
      /// If this <see cref="AbstractCompositeAssemblyDeclaration"/> was retrieved by <see cref="Assembling.Assembler.ForExistingComposite{T}(API.Instance.CompositeModelType)"/> method or from <see cref="Assembling.Assembler.ForNewOrExistingComposite{T}(API.Instance.CompositeModelType, IEnumerable{Type}, out T)"/> method which returned <c>false</c>, the affected IDs may change between invocations, if <see cref="OfTypes(Type[])"/> is invoked.
      /// For <see cref="AbstractCompositeAssemblyDeclaration"/> retrieved by <see cref="Assembling.Assembler.NewComposite{T}(API.Instance.CompositeModelType)"/> method or from <see cref="Assembling.Assembler.ForNewOrExistingComposite{T}(API.Instance.CompositeModelType, IEnumerable{Type}, out T)"/> method which returned <c>true</c>, this property always returns 1 ID and never changes.
      /// </para>
      /// <para>
      /// Each ID corresponds to single <see cref="SPI.Model.CompositeModel"/> at model layer.
      /// </para>
      /// </remarks>
      IEnumerable<Int32> AffectedCompositeIDs { get; }

      /// <summary>
      /// Gets the <see cref="Assembler"/> this <see cref="AbstractCompositeAssemblyDeclaration"/> originated from.
      /// </summary>
      /// <value>The <see cref="Assembler"/> this <see cref="AbstractCompositeAssemblyDeclaration"/> originated from.</value>
      Assembler Assembler { get; }

      /// <summary>
      /// Gets the <see cref="CompositeModelType"/> of composites this <see cref="AbstractCompositeAssemblyDeclaration"/> affects.
      /// </summary>
      /// <value>The <see cref="CompositeModelType"/> of composites this <see cref="AbstractCompositeAssemblyDeclaration"/> affects.</value>
      CompositeModelType CompositeModelType { get; }

      /// <summary>
      /// Specifies default value for certain property as assembly level.
      /// The property does not require to have <see cref="API.Model.UseDefaultsAttribute"/> applied to it.
      /// </summary>
      /// <param name="property">The <see cref="PropertyInfo"/> of the property.</param>
      /// <param name="defaultProvider">The callback providing the default value.</param>
      /// <returns>This <see cref="AbstractCompositeAssemblyDeclaration"/>.</returns>
      /// <remarks>This method does nothing if <paramref name="property"/> or <paramref name="defaultProvider"/> is <c>null</c>.</remarks>
      AbstractCompositeAssemblyDeclaration WithDefaultFor( PropertyInfo property, TDefaultCreator defaultProvider );
   }

   /// <summary>
   /// This interface represents a declaration for zero or more composites, which are not service composites.
   /// </summary>
   public interface PlainCompositeAssemblyDeclaration : AbstractCompositeAssemblyDeclaration
   {

   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Helper method for specifying default value for certain property when the value will be always the same (e.g. when property does not contain generic parameters).
   /// </summary>
   /// <typeparam name="T">The type of <see cref="AbstractCompositeAssemblyDeclaration"/>.</typeparam>
   /// <param name="decl">The <see cref="AbstractCompositeAssemblyDeclaration"/>.</param>
   /// <param name="property">The <see cref="PropertyInfo"/> of the property.</param>
   /// <param name="value">The default value of the property.</param>
   /// <returns>The <paramref name="decl"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="decl"/> is <c>null</c>.</exception>
   public static T WithDefaultValueFor<T>( this T decl, PropertyInfo property, Object value )
      where T : AbstractCompositeAssemblyDeclaration
   {
      return (T) decl.WithDefaultFor( property, ( info, app ) => value );
   }
}

