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
using System.Linq;
using CommonUtils;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This validation error is related to a <see cref="AbstractInjectableModel"/>.
   /// </summary>
   /// <seealso cref="ParameterModel"/>
   /// <seealso cref="FieldModel"/>
   /// <seealso cref="ValidationErrorFactory.NewInjectionError(String, AbstractInjectableModel)"/>
   public interface InjectionValidationError : AbstractValidationError
   {
      /// <summary>
      /// Gets the <see cref="AbstractInjectableModel"/> causing this error.
      /// </summary>
      /// <value>The <see cref="AbstractInjectableModel"/> causing this error.</value>
      /// <seealso cref="ParameterModel"/>
      /// <seealso cref="FieldModel"/>
      AbstractInjectableModel InjectableModel { get; }
   }

   internal class InjectionValidationErrorImpl : InjectionValidationError
   {
      private readonly AbstractInjectableModel _injectableModel;
      private readonly String _message;

      internal InjectionValidationErrorImpl( String message, AbstractInjectableModel injectableModel )
      {
         ArgumentValidator.ValidateNotNull( "Injectable model", injectableModel );

         this._injectableModel = injectableModel;
         this._message = message;
      }

      #region InjectionValidationError Members

      public AbstractInjectableModel InjectableModel
      {
         get
         {
            return this._injectableModel;
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
         return "Model: [" + String.Join( ", ", this._injectableModel.CompositeModel.PublicTypes.Select( pType => pType.FullName ) ) + "], injectable model: " + this._injectableModel + ", message: " + this._message;
      }
   }
}
