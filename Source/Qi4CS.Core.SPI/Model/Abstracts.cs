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
using CollectionsWithRoles.API;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This is common interface for all validation errors.
   /// </summary>
   public interface AbstractValidationError
   {
      /// <summary>
      /// Gets the textual message explaining the error.
      /// </summary>
      /// <value>The textual message explaining the error.</value>
      String Message { get; }
   }

   /// <summary>
   /// A common interface for all validation results of Qi4CS models.
   /// Typically a single Qi4CS model is associated with a single validation result.
   /// </summary>
   /// <typeparam name="TResult">The actual mutable type of validation result.</typeparam>
   /// <typeparam name="TResultIQ">The actual immutable type of validation result.</typeparam>
   /// <seealso cref="AbstractValidationResultIQ"/>
   /// <seealso cref="CompositeValidationResult"/>
   /// <seealso cref="CompositeValidationResultIQ"/>
   /// <seealso cref="ApplicationValidationResult"/>
   /// <seealso cref="ApplicationValidationResultIQ"/>
   public interface AbstractValidationResult<out TResult, out TResultIQ> : Mutable<TResult, TResultIQ>, MutableQuery<TResultIQ>
      where TResult : AbstractValidationResult<TResult, TResultIQ>
      where TResultIQ : AbstractValidationResultIQ
   {

      /// <summary>
      /// Gets the collection to add <see cref="StructureValidationError"/>s to this validation result.
      /// </summary>
      /// <value>The collection to add <see cref="StructureValidationError"/>s to this validation result.</value>
      CollectionAdditionOnly<StructureValidationError> StructureValidationErrors { get; }

      /// <summary>
      /// Gets the collection to add <see cref="InjectionValidationError"/>s to this validation result.
      /// </summary>
      /// <value>The collection to add <see cref="InjectionValidationError"/>s to this validation result.</value>
      CollectionAdditionOnly<InjectionValidationError> InjectionValidationErrors { get; }

      /// <summary>
      /// Gets the collection to add <see cref="InternalValidationError"/>s to this validation result.
      /// </summary>
      /// <value>The collection to add <see cref="InternalValidationError"/>s to this validation result.</value>
      CollectionAdditionOnly<InternalValidationError> InternalValidationErrors { get; }
   }

   /// <summary>
   /// A common interface for all validation result types that provide read-only access to the validation result.
   /// </summary>
   public interface AbstractValidationResultIQ
   {
      /// <summary>
      /// Gets all the <see cref="InjectionValidationError"/>s of this validation result.
      /// </summary>
      /// <value>All the <see cref="InjectionValidationError"/>s of this validation result.</value>
      /// <seealso cref="InjectionValidationError"/>
      ListQuery<InjectionValidationError> InjectionValidationErrors { get; }

      /// <summary>
      /// Gets all the <see cref="StructureValidationError"/>s of this validation result.
      /// </summary>
      /// <value>All the <see cref="StructureValidationError"/>s of this validation result.</value>
      /// <seealso cref="StructureValidationError"/>
      ListQuery<StructureValidationError> StructureValidationErrors { get; }

      /// <summary>
      /// Gets all the <see cref="InternalValidationError"/>s of this validation result.
      /// </summary>
      /// <value>All the <see cref="InternalValidationError"/>s of this validation result.</value>
      /// <seealso cref="InternalValidationError"/>
      ListQuery<InternalValidationError> InternalValidationErrors { get; }

      /// <summary>
      /// Checks whether this valiation result has any errors.
      /// </summary>
      /// <returns><c>true</c> if this validation result has any errors; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// Since this is a common interface for validation results, sub-types of this interface may introduce other errors contained in this validation result.
      /// Therefore, this method should be used to check whether this validation result has any errors instead of manually checking <see cref="InjectionValidationErrors"/>, <see cref="StructureValidationErrors"/> and <see cref="InternalValidationErrors"/> properties.
      /// </remarks>
      Boolean HasAnyErrors();
   }
}
