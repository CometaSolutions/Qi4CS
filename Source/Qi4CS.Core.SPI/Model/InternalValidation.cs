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
   /// This validation error is related to situations when something goes wrong internally in Qi4CS runtime.
   /// </summary>
   /// <seealso cref="ValidationErrorFactory.NewInternalError(String, Object)"/>
   public interface InternalValidationError : AbstractValidationError
   {
      /// <summary>
      /// Gets the object related to this error.
      /// </summary>
      /// <value>The object related to this error.</value>
      Object RelatedObject { get; }
   }

   internal class InternalValidationErrorImpl : InternalValidationError
   {
      private readonly String _message;
      private readonly Object _relatedObject;

      internal InternalValidationErrorImpl( String message, Object relatedObject )
      {
         this._message = message;
         this._relatedObject = relatedObject;
      }

      #region InternalValidationError Members

      public String Message
      {
         get
         {
            return this._message;
         }
      }

      public Object RelatedObject
      {
         get
         {
            return this._relatedObject;
         }
      }

      #endregion

      public override String ToString()
      {
         return "Message: " + this._message + ", related object: " + this._relatedObject;
      }

   }
}
