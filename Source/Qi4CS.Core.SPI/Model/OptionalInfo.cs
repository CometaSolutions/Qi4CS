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
   /// This is common interface for all models which can have their target to be optional.
   /// </summary>
   /// <seealso cref="FieldModel"/>
   /// <seealso cref="ParameterModel"/>
   /// <seealso cref="PropertyModel"/>
   public interface OptionalInfo
   {
      /// <summary>
      /// Gets value indicating whether the target of this model may be optional.
      /// </summary>
      /// <value><c>true</c> if the target of this model may be optional; <c>false</c> otherwise.</value>
      Boolean IsOptional { get; }
   }
}
