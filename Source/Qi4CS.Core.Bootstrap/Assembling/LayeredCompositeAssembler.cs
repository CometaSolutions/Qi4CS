using System;
using System.Collections.Generic;
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
using Qi4CS.Core.Bootstrap.Assembling;

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This interface extends <see cref="Assembler"/> to provide methods specific for assemblers in <see cref="LayeredArchitecture"/>.
   /// </summary>
   public interface LayeredCompositeAssembler : Assembler
   {
      /// <summary>
      /// Gets the <see cref="ModuleArchitecture"/> this <see cref="LayeredCompositeAssembler"/> belongs to.
      /// </summary>
      /// <value>The <see cref="ModuleArchitecture"/> this <see cref="LayeredCompositeAssembler"/> belongs to.</value>
      ModuleArchitecture Module { get; }
   }
}

#pragma warning disable 1591
public static partial class E_Qi4CS
#pragma warning restore 1591
{
   /// <summary>
   /// Helper method to call <see cref="E_Qi4CS.NewPlainComposite{T}(Assembler)"/> for <see cref="LayeredCompositeAssembler"/> and returning correctly-typed result.
   /// </summary>
   /// <param name="assembler">The <see cref="LayeredCompositeAssembler"/>-</param>
   /// <returns>The result of <see cref="E_Qi4CS.NewPlainComposite{T}(Assembler)"/> method.</returns>
   /// <seealso cref="E_Qi4CS.NewPlainComposite{T}(Assembler)"/>
   public static LayeredPlainCompositeAssemblyDeclaration NewLayeredPlainComposite( this LayeredCompositeAssembler assembler )
   {
      return assembler.NewPlainComposite<LayeredPlainCompositeAssemblyDeclaration>();
   }

   // TODO implementing value composite model type should be done via extension
   //public static LayeredCompositeAssemblyDeclaration NewLayeredValue( this LayeredCompositeAssembler assembler )
   //{
   //   return assembler.NewValue<LayeredCompositeAssemblyDeclaration>();
   //}

   /// <summary>
   /// Helper method to call <see cref="E_Qi4CS.NewService{T}(Assembler)"/> for <see cref="LayeredCompositeAssembler"/> and returning correctly-typed result.
   /// </summary>
   /// <param name="assembler">The <see cref="LayeredCompositeAssembler"/>-</param>
   /// <returns>The result of <see cref="E_Qi4CS.NewService{T}(Assembler)"/> method.</returns>
   /// <seealso cref="E_Qi4CS.NewService{T}(Assembler)"/>
   public static LayeredServiceCompositeAssemblyDeclaration NewLayeredService( this LayeredCompositeAssembler assembler )
   {
      return assembler.NewService<LayeredServiceCompositeAssemblyDeclaration>();
   }

   /// <summary>
   /// Helper method to call <see cref="E_Qi4CS.ForExistingPlainComposite{T}(Assembler)"/> for <see cref="LayeredCompositeAssembler"/> and returning correctly-typed result.
   /// </summary>
   /// <param name="assembler">The <see cref="LayeredCompositeAssembler"/>-</param>
   /// <returns>The result of <see cref="E_Qi4CS.ForExistingPlainComposite{T}(Assembler)"/> method.</returns>
   /// <seealso cref="E_Qi4CS.ForExistingPlainComposite{T}(Assembler)"/>
   public static LayeredPlainCompositeAssemblyDeclaration ForExistingLayeredPlainComposite( this LayeredCompositeAssembler assembler )
   {
      return assembler.ForExistingPlainComposite<LayeredPlainCompositeAssemblyDeclaration>();
   }

   // TODO implementing value composite model type should be done via extension
   //public static LayeredCompositeAssemblyDeclaration ForExistingLayeredValue( this LayeredCompositeAssembler assembler )
   //{
   //   return assembler.ForExistingValue<LayeredCompositeAssemblyDeclaration>();
   //}

   /// <summary>
   /// Helper method to call <see cref="E_Qi4CS.ForExistingService{T}(Assembler)"/> for <see cref="LayeredCompositeAssembler"/> and returning correctly-typed result.
   /// </summary>
   /// <param name="assembler">The <see cref="LayeredCompositeAssembler"/>-</param>
   /// <returns>The result of <see cref="E_Qi4CS.ForExistingService{T}(Assembler)"/> method.</returns>
   /// <seealso cref="E_Qi4CS.ForExistingService{T}(Assembler)"/>
   public static LayeredServiceCompositeAssemblyDeclaration ForExistingLayeredService( this LayeredCompositeAssembler assembler )
   {
      return assembler.ForExistingService<LayeredServiceCompositeAssemblyDeclaration>();
   }

   /// <summary>
   /// Helper method to call <see cref="E_Qi4CS.ForNewOrExistingPlainComposite{T}(Assembler,IEnumerable{System.Type},out T)"/> for <see cref="LayeredServiceCompositeAssemblyDeclaration"/> and returning correctly-typed result.
   /// </summary>
   /// <param name="assembler">The <see cref="LayeredCompositeAssembler"/>.</param>
   /// <param name="types">The composite types.</param>
   /// <param name="result">This parameter will hold the resulting composite declaration.</param>
   /// <returns>The result of <see cref="E_Qi4CS.ForNewOrExistingPlainComposite{T}(Assembler,IEnumerable{System.Type},out T)"/> method.</returns>
   /// <seealso cref="E_Qi4CS.ForNewOrExistingPlainComposite{T}(Assembler,IEnumerable{System.Type},out T)"/>
   public static Boolean ForNewOrExistingLayeredPlainComposite( this LayeredCompositeAssembler assembler, IEnumerable<Type> types, out LayeredPlainCompositeAssemblyDeclaration result )
   {
      return assembler.ForNewOrExistingPlainComposite<LayeredPlainCompositeAssemblyDeclaration>( types, out result );
   }

   // TODO implementing value composite model type should be done via extension
   //public static Boolean ForNewOrExistingLayeredValue( this Assembler assembler, IEnumerable<Type> types, out LayeredCompositeAssemblyDeclaration result )
   //{
   //   return assembler.ForNewOrExistingValue<LayeredCompositeAssemblyDeclaration>( types, out result );
   //}

   /// <summary>
   /// Helper method to call <see cref="E_Qi4CS.ForNewOrExistingService{T}(Assembler,IEnumerable{System.Type},out T)"/> for <see cref="LayeredServiceCompositeAssemblyDeclaration"/> and returning correctly-typed result.
   /// </summary>
   /// <param name="assembler">The <see cref="LayeredCompositeAssembler"/>-</param>
   /// <param name="types">The composite types.</param>
   /// <param name="result">This parameter will hold the resulting composite declaration.</param>
   /// <returns>The result of <see cref="E_Qi4CS.ForNewOrExistingService{T}(Assembler,IEnumerable{System.Type},out T)"/> method.</returns>
   /// <seealso cref="E_Qi4CS.ForNewOrExistingService{T}(Assembler,IEnumerable{System.Type},out T)"/>
   public static Boolean ForNewOrExistingLayeredService( this LayeredCompositeAssembler assembler, IEnumerable<Type> types, out LayeredServiceCompositeAssemblyDeclaration result )
   {
      return assembler.ForNewOrExistingService<LayeredServiceCompositeAssemblyDeclaration>( types, out result );
   }
}
