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
using System.Reflection;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// A model of a single constructor that can be used by Qi4CS to instantiate fragments.
   /// </summary>
   public interface ConstructorModel : AbstractMemberInfoModel<ConstructorInfo>, AbstractModelWithParameters
   {
      /// <summary>
      /// Gets the index of this <see cref="ConstructorModel"/> in <see cref="CompositeModel.Constructors"/> property.
      /// </summary>
      /// <value>The index of this <see cref="ConstructorModel"/> in <see cref="CompositeModel.Constructors"/> property.</value>
      Int32 ConstructorIndex { get; }
   }
}
