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
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;


namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This interface encapsulates the mutable validation result of the whole <see cref="ApplicationModel{T}"/>.
   /// </summary>
   /// <seealso cref="ApplicationValidationResultIQ"/>
   /// <remarks>This interface may obtain more methods and properties in the future.</remarks>
   public interface ApplicationValidationResult : AbstractValidationResult<ApplicationValidationResult, ApplicationValidationResultIQ>
   {

   }

   /// <summary>
   /// This interface encapsulates the immutable validation result of the whole <see cref="Model.ApplicationModel{T}"/>.
   /// </summary>
   public interface ApplicationValidationResultIQ : AbstractValidationResultIQ
   {
      /// <summary>
      /// Gets the validation results of all composites belonging to this application.
      /// </summary>
      /// <value>The validation results of all composites belonging to this application.</value>
      DictionaryQuery<CompositeModel, CompositeValidationResultIQ> CompositeValidationResults { get; }

      /// <summary>
      /// Gets the <see cref="Model.ApplicationModel{T}"/> that this validation result is associated with.
      /// </summary>
      /// <value>The <see cref="Model.ApplicationModel{T}"/> that this validation result is associated with.</value>
      ApplicationModel<ApplicationSPI> ApplicationModel { get; }
   }

   /// <summary>
   /// This interface encapsulates the mutable validation result of a single <see cref="CompositeModel"/>.
   /// </summary>
   /// <remarks>This interface may obtain more methods and properties in the future.</remarks>
   /// <seealso cref="CompositeValidationResultIQ"/>
   public interface CompositeValidationResult : AbstractValidationResult<CompositeValidationResult, CompositeValidationResultIQ>
   {

   }

   /// <summary>
   /// This interface encapsulates the immutable validation result of a single <see cref="CompositeModel"/>.
   /// </summary>
   /// <remarks>This interface may obtain more methods and properties in the future.</remarks>
   public interface CompositeValidationResultIQ : AbstractValidationResultIQ
   {
   }

   /// <summary>
   /// Factory class to create <see cref="InjectionValidationError"/>s, <see cref="StructureValidationError"/>s and <see cref="InternalValidationError"/>s.
   /// </summary>
   public static class ValidationErrorFactory
   {
      /// <summary>
      /// Creates a new instance of <see cref="InjectionValidationError"/>.
      /// </summary>
      /// <param name="message">The textual message explaining the error.</param>
      /// <param name="model">The <see cref="AbstractInjectableModel"/> related to this error.</param>
      /// <returns>A new instance of <see cref="InjectionValidationError"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="model"/> is <c>null</c>.</exception>
      public static InjectionValidationError NewInjectionError( String message, AbstractInjectableModel model )
      {
         return new InjectionValidationErrorImpl( message, model );
      }

      /// <summary>
      /// Creates a new instance of <see cref="StructureValidationError"/> without <see cref="AbstractMemberInfoModel{T}"/>.
      /// </summary>
      /// <param name="message">The textual message explaining the error.</param>
      /// <param name="validatableItem">The <see cref="ValidatableItem"/> related to this error.</param>
      /// <returns>A new instance of <see cref="StructureValidationError"/> without <see cref="AbstractMemberInfoModel{T}"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="validatableItem"/> is <c>null</c>.</exception>
      public static StructureValidationError NewStructureError( String message, ValidatableItem validatableItem )
      {
         return NewStructureError<Object>( message, validatableItem, null );
      }

      /// <summary>
      /// Creates a new instance of <see cref="StructureValidationError"/>.
      /// </summary>
      /// <typeparam name="TMemberInfo">The type of the generic argument of <see cref="AbstractMemberInfoModel{T}"/>.</typeparam>
      /// <param name="message">The textual message explaining the error.</param>
      /// <param name="validatableItem">The <see cref="ValidatableItem"/> related to this error.</param>
      /// <param name="memberInfoModel">The <see cref="AbstractMemberInfoModel{T}"/> related to this error.</param>
      /// <returns>A new instance of <see cref="StructureValidationError"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="validatableItem"/> is <c>null</c>.</exception>
      public static StructureValidationError NewStructureError<TMemberInfo>( String message, ValidatableItem validatableItem, AbstractMemberInfoModel<TMemberInfo> memberInfoModel )
         where TMemberInfo : class
      {
         return new StructureValidationErrorImpl( message, validatableItem, memberInfoModel );
      }

      /// <summary>
      /// Creates a new instance of <see cref="InternalValidationError"/>.
      /// </summary>
      /// <param name="message">The textual message explaining the error.</param>
      /// <param name="relatedObject">Some object related to the error.</param>
      /// <returns>A new instance of <see cref="InternalValidationError"/>.</returns>
      public static InternalValidationError NewInternalError( String message, Object relatedObject )
      {
         return new InternalValidationErrorImpl( message, relatedObject );
      }
   }


}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Helper method to get enumerable of all errors of some <see cref="ApplicationValidationResultIQ"/>.
   /// </summary>
   /// <param name="result">The <see cref="ApplicationValidationResultIQ"/>.</param>
   /// <returns>Enumerable of all errors of a single <see cref="ApplicationValidationResultIQ"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="result"/> is <c>null</c>.</exception>
   public static IEnumerable<AbstractValidationError> GetAllErrors( this ApplicationValidationResultIQ result )
   {
      return result.InjectionValidationErrors
         .Cast<AbstractValidationError>()
         .Concat( result.StructureValidationErrors )
         .Concat( result.InternalValidationErrors )
         .Concat( result.CompositeValidationResults.Values.SelectMany( cResult =>
            cResult.InjectionValidationErrors.Cast<AbstractValidationError>()
            .Concat( cResult.StructureValidationErrors )
            .Concat( cResult.InternalValidationErrors )
            ) );
   }
}
