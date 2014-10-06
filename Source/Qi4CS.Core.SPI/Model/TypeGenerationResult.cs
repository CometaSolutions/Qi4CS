﻿/*
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
using CollectionsWithRoles.API;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This is common interface for interfaces providing information about types generated by Qi4CS.
   /// </summary>
   public interface GeneratedTypeInfo
   {
      /// <summary>
      /// Gets the type generated by Qi4CS.
      /// </summary>
      /// <value>The type generated by Qi4CS.</value>
      Type GeneratedType { get; }

      /// <summary>
      /// Gets the ID of the <see cref="GeneratedType"/>.
      /// This ID may be supplied to <see cref="CompositeFactory.CreateInstance(Int32, Type[], Object[])"/> method to create instances of type <see cref="GeneratedType"/>.
      /// </summary>
      /// <value>The ID of the <see cref="GeneratedType"/>.</value>
      /// <remarks>
      /// The ID is unique in scope of single composite model.
      /// </remarks>
      Int32 GeneratedTypeID { get; }
   }

   /// <summary>
   /// This interface extends <see cref="GeneratedTypeInfo"/> to add a way to query single type associated with a generated type.
   /// </summary>
   public interface TypeGenerationResult : GeneratedTypeInfo
   {
      /// <summary>
      /// Gets the type (usually related to <see cref="CompositeModel"/>) associated with this generated type.
      /// </summary>
      /// <value>The type (usually related to <see cref="CompositeModel"/>) associated with this generated type.</value>
      Type DeclaredType { get; }
   }

   /// <summary>
   /// This interface extends <see cref="TypeGenerationResult"/> to add a way to query whether this fragment type requires instance pool.
   /// </summary>
   public interface FragmentTypeGenerationResult : TypeGenerationResult
   {
      /// <summary>
      /// Gets information whether instance pool is required for this generated fragment type.
      /// </summary>
      /// <value><c>true</c> if instance pool is required for this generated fragment type; <c>false</c> otherwise.</value>
      Boolean InstancePoolRequired { get; }
   }

   /// <summary>
   /// This interface provides ways to query various information about all generated types related to a single <see cref="CompositeModel"/>.
   /// </summary>
   public interface PublicCompositeTypeGenerationResult
   {
      /// <summary>
      /// Gets the <see cref="CompositeFactory"/> of this <see cref="PublicCompositeTypeGenerationResult"/>.
      /// </summary>
      /// <value>The <see cref="CompositeFactory"/> of this <see cref="PublicCompositeTypeGenerationResult"/>.</value>
      CompositeFactory CompositeFactory { get; }

      /// <summary>
      /// Gets the main generated composite type.
      /// </summary>
      /// <value>The main generated composite type.</value>
      /// <remarks>
      /// The constructor of the main generated composite type will take various callback delegates as <c>ref</c> parameters.
      /// It will set the callbacks to methods, if required, that will be then invoked at various stages of composite lifecycle.
      /// </remarks>
      Type GeneratedMainPublicType { get; }

      /// <summary>
      /// Gets the maximum amount of parameters for constructor of any generated type.
      /// </summary>
      /// <value>The maximum amount of parameters for constructor of any generated type.</value>
      Int32 MaxParamCountForCtors { get; }

      /// <summary>
      /// Gets all the generated public types of this <see cref="PublicCompositeTypeGenerationResult"/>.
      /// </summary>
      /// <value>All the generated public types of this <see cref="PublicCompositeTypeGenerationResult"/>.</value>
      /// <remarks>
      /// If a <see cref="CompositeModel.PublicTypes"/> contains types from different assemblies (excluding Qi4CS.Core assembly), there will be need to generate more than one type.
      /// </remarks>
      ListQuery<GeneratedTypeInfo> GeneratedPublicTypes { get; }

      /// <summary>
      /// Gets the information about how to position the generic arguments from given composite type to the generated composite types.
      /// </summary>
      /// <value>The information about how to position the generic arguments from given composite type to the generated composite types.</value>
      /// <remarks>
      /// The key of the dictionary is the declared public composite type, which will be generic type definition.
      /// The value of the dictionary will be a list of indices of generated public composite type, in order of the generic arguments of the key.
      /// </remarks>
      DictionaryQuery<Type, ListQuery<Int32>> PublicCompositeGenericArguments { get; }

      /// <summary>
      /// Gets information about all generated fragment types of this <see cref="PublicCompositeTypeGenerationResult"/>.
      /// </summary>
      /// <value>The information about all generated fragment types of this <see cref="PublicCompositeTypeGenerationResult"/>.</value>
      /// <seealso cref="FragmentTypeGenerationResult"/>
      ListQuery<FragmentTypeGenerationResult> FragmentGenerationResults { get; }

      /// <summary>
      /// Gets information about all generated private composite types of this <see cref="PublicCompositeTypeGenerationResult"/>.
      /// </summary>
      /// <value>The information about all generated private composite types of this <see cref="PublicCompositeTypeGenerationResult"/>.</value>
      /// <seealso cref="TypeGenerationResult"/>
      ListQuery<TypeGenerationResult> PrivateCompositeGenerationResults { get; }

      /// <summary>
      /// Gets information about all generated concern invocation types of this <see cref="PublicCompositeTypeGenerationResult"/>.
      /// </summary>
      /// <value>The information about all generated concern invocation type of this <see cref="PublicCompositeTypeGenerationResult"/>s.</value>
      /// <seealso cref="TypeGenerationResult"/>
      ListQuery<TypeGenerationResult> ConcernInvocationGenerationResults { get; }

      /// <summary>
      /// Gets information about all generated side effect invocation types of this <see cref="PublicCompositeTypeGenerationResult"/>.
      /// </summary>
      /// <value>The information about al lgenerated side effect invocation types of this <see cref="PublicCompositeTypeGenerationResult"/>.</value>
      /// <seealso cref="FragmentTypeGenerationResult"/>
      ListQuery<TypeGenerationResult> SideEffectGenerationResults { get; }
   }
}