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
   /// A model for "special methods" of the <see cref="CompositeModel"/>, which are methods belonging to fragments.
   /// These methods will be then invoked at certain times in composite lifecycle.
   /// </summary>
   public interface SpecialMethodModel : AbstractMethodModel, AbstractModelWithParameters, ModelWithAttributes, AbstractMethodModelWithFragment
   {
      /// <summary>
      /// Gets the <see cref="ParameterModel"/> of the result parameter of this <see cref="SpecialMethodModel"/>.
      /// </summary>
      /// <value>The <see cref="ParameterModel"/> of the result parameter of this <see cref="SpecialMethodModel"/>.</value>
      /// <remarks>
      /// Return values of special methods are ignored by Qi4CS runtime.
      /// This property is still present for information sake.
      /// </remarks>
      ParameterModel ResultModel { get; }

      /// <summary>
      /// Gets the index of this <see cref="SpecialMethodModel"/> in <see cref="CompositeModel.SpecialMethods"/> property.
      /// </summary>
      /// <value>The index of this <see cref="SpecialMethodModel"/> in <see cref="CompositeModel.SpecialMethods"/> property.</value>
      Int32 SpecialMethodIndex { get; }
   }
}
