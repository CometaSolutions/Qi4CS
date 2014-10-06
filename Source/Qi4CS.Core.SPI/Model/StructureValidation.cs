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
using CommonUtils;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This validation error is related to situations when something is structurally wrong.
   /// This includes situations when there are no matching mixin methods found, or when architecture detects some structural error.
   /// </summary>
   /// <seealso cref="ValidationErrorFactory.NewStructureError(String, ValidatableItem)"/>
   /// <seealso cref="ValidationErrorFactory.NewStructureError{T}(String, ValidatableItem, AbstractMemberInfoModel{T})"/>
   public interface StructureValidationError : AbstractValidationError
   {
      /// <summary>
      /// Gets the <see cref="ValidatableItem"/> related to this validation error.
      /// </summary>
      /// <value>The <see cref="ValidatableItem"/> related to this validation error.</value>
      /// <seealso cref="ValidatableItem"/>
      ValidatableItem ValidatableItem { get; }

      /// <summary>
      /// Gets the <see cref="AbstractMemberInfoModel{T}"/> related to this validation error.
      /// May be <c>null</c>.
      /// </summary>
      /// <value>The <see cref="AbstractMemberInfoModel{T}"/> related to this validation error or <c>null</c>.</value>
      /// <seealso cref="AbstractMemberInfoModel{T}"/>
      AbstractMemberInfoModel<Object> MemberModel { get; }
   }

   internal class StructureValidationErrorImpl : StructureValidationError
   {
      private readonly ValidatableItem _validatableItem;
      private readonly AbstractMemberInfoModel<Object> _memberInfoModel;
      private readonly String _message;

      internal StructureValidationErrorImpl( String message, ValidatableItem validatableItem, AbstractMemberInfoModel<Object> memberInfoModel )
      {
         ArgumentValidator.ValidateNotNull( "Validatable item model", validatableItem );

         this._validatableItem = validatableItem;
         this._memberInfoModel = memberInfoModel;
         this._message = message;
      }

      #region StructureValidationError Members

      public ValidatableItem ValidatableItem
      {
         get
         {
            return this._validatableItem;
         }
      }

      public AbstractMemberInfoModel<Object> MemberModel
      {
         get
         {
            return this._memberInfoModel;
         }
      }

      public String Message
      {
         get
         {
            return this._message;
         }
      }

      #endregion

      public override String ToString()
      {
         return this._message; // "Validatable item: " + this._validatableItem + ", member model: " + this._memberInfoModel + ", message: " + this._message;
      }

   }
}
