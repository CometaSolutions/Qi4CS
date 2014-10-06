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


namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This exception is thrown whenever one tries to create new <see cref="SPI.Instance.ApplicationSPI"/> or generate code from a <see cref="ApplicationModel{T}"/> which has validation errors.
   /// </summary>
   public class InvalidApplicationModelException : Exception
   {
      private readonly ApplicationValidationResultIQ _validationResult;

      /// <summary>
      /// Creates new instance of <see cref="InvalidApplicationModelException"/>.
      /// </summary>
      /// <param name="validationResult">The <see cref="ApplicationValidationResultIQ"/> which contains validation errors prohibiting some functionality.</param>
      /// <param name="errorMsg">The additional error message.</param>
      public InvalidApplicationModelException( ApplicationValidationResultIQ validationResult, String errorMsg )
         : base( errorMsg + "\n" + validationResult )
      {
         this._validationResult = validationResult;
      }

      /// <summary>
      /// Gets the <see cref="ApplicationValidationResultIQ"/> that was passed to this exception.
      /// </summary>
      /// <value>The <see cref="ApplicationValidationResultIQ"/> that was passed to this exception.</value>
      public ApplicationValidationResultIQ ValidationResult
      {
         get
         {
            return this._validationResult;
         }
      }
   }
}
