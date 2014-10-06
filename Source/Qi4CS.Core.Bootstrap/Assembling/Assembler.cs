using System;
using System.Collections.Generic;
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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This interface represents a single unit in <see cref="Assembling.ApplicationArchitecture{T}"/> where one can add or modify composite declarations.
   /// </summary>
   public interface Assembler : UsesProvider<Assembler>
   {
      /// <summary>
      /// Creates a new composite declaration for given <see cref="CompositeModelType"/> and adds it to this <see cref="Assembler"/>.
      /// </summary>
      /// <typeparam name="TCompositeDeclaration">The actual type of the composite declaration.</typeparam>
      /// <param name="compositeModelType">The <see cref="CompositeModelType"/>.</param>
      /// <returns>New composite declaration for <paramref name="compositeModelType"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="compositeModelType"/> is <c>null</c>.</exception>
      /// <seealso cref="AbstractCompositeAssemblyDeclaration"/>
      TCompositeDeclaration NewComposite<TCompositeDeclaration>( CompositeModelType compositeModelType )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration;

      /// <summary>
      /// Gets composite declaration which affects only existing composite information within this <see cref="Assembler"/> for given <see cref="CompositeModelType"/> .
      /// </summary>
      /// <typeparam name="TCompositeDeclaration">The actual type of the composite declaration.</typeparam>
      /// <param name="compositeModelType">The <see cref="CompositeModelType"/></param>
      /// <returns>Instance declaration which affects only existing composite information within this <see cref="Assembler"/> for <paramref name="compositeModelType"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="compositeModelType"/> is <c>null</c>.</exception>
      /// <seealso cref="AbstractCompositeAssemblyDeclaration"/>
      TCompositeDeclaration ForExistingComposite<TCompositeDeclaration>( CompositeModelType compositeModelType )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration;

      /// <summary>
      /// Check whether any composite declaration exists for given <see cref="CompositeModelType"/> and composite types, and creates a new if it doesn't.
      /// </summary>
      /// <typeparam name="TCompositeDeclaration">The actual type of the composite declaration.</typeparam>
      /// <param name="compositeModelType">The <see cref="CompositeModelType"/>.</param>
      /// <param name="types">The composite types.</param>
      /// <param name="result">This parameter will hold the resulting composite declaration.</param>
      /// <returns><c>true</c> if there didn't exist composite information for given <paramref name="compositeModelType"/> and <paramref name="types"/> within this <see cref="Assembler"/>; <c>false</c> otherwise.</returns>
      Boolean ForNewOrExistingComposite<TCompositeDeclaration>( CompositeModelType compositeModelType, IEnumerable<Type> types, out TCompositeDeclaration result )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration;

      /// <summary>
      /// Gets the <see cref="Assembling.ApplicationArchitecture{T}"/> this <see cref="Assembler"/> belongs to.
      /// </summary>
      /// <value>The <see cref="Assembling.ApplicationArchitecture{T}"/> this <see cref="Assembler"/> belongs to.</value>
      ApplicationArchitecture<ApplicationModel<ApplicationSPI>> ApplicationArchitecture { get; }

      /// <summary>
      /// Gets the <see cref="StructureServiceProviderSPI"/> which represents this <see cref="Assembler"/> from the given <see cref="Application"/>.
      /// </summary>
      /// <param name="application">The <see cref="Application"/> instantiated from model instantiated from the <see cref="Assembling.ApplicationArchitecture{T}"/> this <see cref="Assembler"/> belongs to.</param>
      /// <returns>The <see cref="StructureServiceProviderSPI"/> which represents this <see cref="Assembler"/> from the given <see cref="Application"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="application"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If <paramref name="application"/> is of wrong type or it is not made from the model that was made from the <see cref="Assembling.ApplicationArchitecture{T}"/> this <see cref="Assembler"/> belongs to.</exception>
      StructureServiceProviderSPI GetStructureServiceProvider( Application application );
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Helper method to invoke <see cref="Assembler.NewComposite{T}(CompositeModelType)"/> method with <see cref="CompositeModelType.PLAIN"/> as parameter.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <returns>The return value of <see cref="Assembler.NewComposite{T}(CompositeModelType)"/> casted to <see cref="PlainCompositeAssemblyDeclaration"/>.</returns>
   public static PlainCompositeAssemblyDeclaration NewPlainComposite( this Assembler assembler )
   {
      return assembler.NewComposite<PlainCompositeAssemblyDeclaration>( CompositeModelType.PLAIN );
   }

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.NewComposite{T}(CompositeModelType)"/> method with <see cref="CompositeModelType.PLAIN"/> as parameter.
   /// </summary>
   /// <typeparam name="TCompositeDeclaration">The type of composite declaration.</typeparam>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <returns>The return value of <see cref="Assembler.NewComposite{T}(CompositeModelType)"/>.</returns>
   public static TCompositeDeclaration NewPlainComposite<TCompositeDeclaration>( this Assembler assembler )
      where TCompositeDeclaration : PlainCompositeAssemblyDeclaration
   {
      return assembler.NewComposite<TCompositeDeclaration>( CompositeModelType.PLAIN );
   }

   // TODO implementing value composite model type should be done via extension
   //public static CompositeAssemblyDeclaration NewValue( this Assembler assembler )
   //{
   //   return assembler.NewComposite<CompositeAssemblyDeclaration>( CompositeModelType.VALUE );
   //}

   //public static TCompositeDeclaration NewValue<TCompositeDeclaration>( this Assembler assembler )
   //   where TCompositeDeclaration : CompositeAssemblyDeclaration
   //{
   //   return assembler.NewComposite<TCompositeDeclaration>( CompositeModelType.VALUE );
   //}

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.NewComposite{T}(CompositeModelType)"/> method with <see cref="CompositeModelType.SERVICE"/> as parameter.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <returns>The return value of <see cref="Assembler.NewComposite{T}(CompositeModelType)"/> casted to <see cref="ServiceCompositeAssemblyDeclaration"/>.</returns>
   public static ServiceCompositeAssemblyDeclaration NewService( this Assembler assembler )
   {
      return assembler.NewComposite<ServiceCompositeAssemblyDeclaration>( CompositeModelType.SERVICE );
   }

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.NewComposite{T}(CompositeModelType)"/> method with <see cref="CompositeModelType.SERVICE"/> as parameter.
   /// </summary>
   /// <typeparam name="TServiceDeclaration">The type of composite declaration.</typeparam>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <returns>The return value of <see cref="Assembler.NewComposite{T}(CompositeModelType)"/>.</returns>
   public static TServiceDeclaration NewService<TServiceDeclaration>( this Assembler assembler )
      where TServiceDeclaration : ServiceCompositeAssemblyDeclaration
   {
      return assembler.NewComposite<TServiceDeclaration>( CompositeModelType.SERVICE );
   }

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.ForExistingComposite{T}(CompositeModelType)"/> method with <see cref="CompositeModelType.PLAIN"/> as parameter.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <returns>The return value of <see cref="Assembler.ForExistingComposite{T}(CompositeModelType)"/> casted to <see cref="PlainCompositeAssemblyDeclaration"/>.</returns>
   public static PlainCompositeAssemblyDeclaration ForExistingPlainComposite( this Assembler assembler )
   {
      return assembler.ForExistingComposite<PlainCompositeAssemblyDeclaration>( CompositeModelType.PLAIN );
   }

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.ForExistingComposite{T}(CompositeModelType)"/> method with <see cref="CompositeModelType.PLAIN"/> as parameter.
   /// </summary>
   /// <typeparam name="TCompositeDeclaration">The type of composite declaration.</typeparam>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <returns>The return value of <see cref="Assembler.ForExistingComposite{T}(CompositeModelType)"/>.</returns>
   public static TCompositeDeclaration ForExistingPlainComposite<TCompositeDeclaration>( this Assembler assembler )
      where TCompositeDeclaration : PlainCompositeAssemblyDeclaration
   {
      return assembler.ForExistingComposite<TCompositeDeclaration>( CompositeModelType.PLAIN );
   }

   // TODO implementing value composite model type should be done via extension
   //public static CompositeAssemblyDeclaration ForExistingValue( this Assembler assembler )
   //{
   //   return assembler.ForExistingComposite<CompositeAssemblyDeclaration>( CompositeModelType.VALUE );
   //}

   //public static TCompositeDeclaration ForExistingValue<TCompositeDeclaration>( this Assembler assembler )
   //   where TCompositeDeclaration : CompositeAssemblyDeclaration
   //{
   //   return assembler.ForExistingComposite<TCompositeDeclaration>( CompositeModelType.VALUE );
   //}

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.ForExistingComposite{T}(CompositeModelType)"/> method with <see cref="CompositeModelType.SERVICE"/> as parameter.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <returns>The return value of <see cref="Assembler.ForExistingComposite{T}(CompositeModelType)"/> casted to <see cref="ServiceCompositeAssemblyDeclaration"/>.</returns>
   public static ServiceCompositeAssemblyDeclaration ForExistingService( this Assembler assembler )
   {
      return assembler.ForExistingComposite<ServiceCompositeAssemblyDeclaration>( CompositeModelType.SERVICE );
   }

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.ForExistingComposite{T}(CompositeModelType)"/> method with <see cref="CompositeModelType.SERVICE"/> as parameter.
   /// </summary>
   /// <typeparam name="TServiceDeclaration">The type of composite declaration.</typeparam>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <returns>The return value of <see cref="Assembler.ForExistingComposite{T}(CompositeModelType)"/>.</returns>
   public static TServiceDeclaration ForExistingService<TServiceDeclaration>( this Assembler assembler )
      where TServiceDeclaration : ServiceCompositeAssemblyDeclaration
   {
      return assembler.ForExistingComposite<TServiceDeclaration>( CompositeModelType.SERVICE );
   }

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.ForNewOrExistingComposite{T}(CompositeModelType,IEnumerable{System.Type},out T)"/> method with <see cref="CompositeModelType.PLAIN"/> as parameter.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <param name="types">The composite types.</param>
   /// <param name="result">This parameter will hold the resulting composite declaration.</param>
   /// <returns>The return value of <see cref="Assembler.ForNewOrExistingComposite{T}(CompositeModelType,IEnumerable{System.Type},out T)"/>.</returns>
   public static Boolean ForNewOrExistingPlainComposite( this Assembler assembler, IEnumerable<Type> types, out PlainCompositeAssemblyDeclaration result )
   {
      return assembler.ForNewOrExistingComposite<PlainCompositeAssemblyDeclaration>( CompositeModelType.PLAIN, types, out result );
   }

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.ForNewOrExistingComposite{T}(CompositeModelType,IEnumerable{System.Type},out T)"/> method with <see cref="CompositeModelType.PLAIN"/> as parameter.
   /// </summary>
   /// <typeparam name="TCompositeDeclaration">The type of composite declaration.</typeparam>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <param name="types">The composite types.</param>
   /// <param name="result">This parameter will hold the resulting composite declaration.</param>
   /// <returns>The return value of <see cref="Assembler.ForNewOrExistingComposite{T}(CompositeModelType,IEnumerable{System.Type},out T)"/>.</returns>
   public static Boolean ForNewOrExistingPlainComposite<TCompositeDeclaration>( this Assembler assembler, IEnumerable<Type> types, out TCompositeDeclaration result )
      where TCompositeDeclaration : PlainCompositeAssemblyDeclaration
   {
      return assembler.ForNewOrExistingComposite<TCompositeDeclaration>( CompositeModelType.PLAIN, types, out result );
   }

   // TODO implementing value composite model type should be done via extension
   //public static Boolean ForNewOrExistingValue( this Assembler assembler, IEnumerable<Type> types, out CompositeAssemblyDeclaration result )
   //{
   //   return assembler.ForNewOrExistingComposite<CompositeAssemblyDeclaration>( CompositeModelType.VALUE, types, out result );
   //}

   //public static Boolean ForNewOrExistingValue<TCompositeDeclaration>( this Assembler assembler, IEnumerable<Type> types, out TCompositeDeclaration result )
   //   where TCompositeDeclaration : CompositeAssemblyDeclaration
   //{
   //   return assembler.ForNewOrExistingComposite<TCompositeDeclaration>( CompositeModelType.VALUE, types, out result );
   //}

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.ForNewOrExistingComposite{T}(CompositeModelType,IEnumerable{System.Type},out T)"/> method with <see cref="CompositeModelType.SERVICE"/> as parameter.
   /// </summary>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <param name="types">The composite types.</param>
   /// <param name="result">This parameter will hold the resulting composite declaration.</param>
   /// <returns>The return value of <see cref="Assembler.ForNewOrExistingComposite{T}(CompositeModelType,IEnumerable{System.Type},out T)"/>.</returns>
   public static Boolean ForNewOrExistingService( this Assembler assembler, IEnumerable<Type> types, out ServiceCompositeAssemblyDeclaration result )
   {
      return assembler.ForNewOrExistingComposite<ServiceCompositeAssemblyDeclaration>( CompositeModelType.SERVICE, types, out result );
   }

   /// <summary>
   /// Helper method to invoke <see cref="Assembler.ForNewOrExistingComposite{T}(CompositeModelType,IEnumerable{System.Type},out T)"/> method with <see cref="CompositeModelType.SERVICE"/> as parameter.
   /// </summary>
   /// <typeparam name="TServiceDeclaration">The type of composite declaration.</typeparam>
   /// <param name="assembler">The <see cref="Assembler"/>.</param>
   /// <param name="types">The composite types.</param>
   /// <param name="result">This parameter will hold the resulting composite declaration.</param>
   /// <returns>The return value of <see cref="Assembler.ForNewOrExistingComposite{T}(CompositeModelType,IEnumerable{System.Type},out T)"/>.</returns>
   public static Boolean ForNewOrExistingService<TServiceDeclaration>( this Assembler assembler, IEnumerable<Type> types, out TServiceDeclaration result )
      where TServiceDeclaration : ServiceCompositeAssemblyDeclaration
   {
      return assembler.ForNewOrExistingComposite<TServiceDeclaration>( CompositeModelType.SERVICE, types, out result );
   }
}
