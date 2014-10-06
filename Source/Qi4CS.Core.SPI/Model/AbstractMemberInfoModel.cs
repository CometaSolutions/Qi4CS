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

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This is common interface for all Qi4CS models representing some reflection element.
   /// </summary>
   /// <typeparam name="TMemberInfo">The type of native reflection element.</typeparam>
   /// <seealso cref="CompositeMethodModel"/>
   /// <seealso cref="ConcernMethodModel"/>
   /// <seealso cref="MixinMethodModel"/>
   /// <seealso cref="SideEffectMethodModel"/>
   /// <seealso cref="SpecialMethodModel"/>
   /// <seealso cref="ConstructorModel"/>
   /// <seealso cref="ParameterModel"/>
   /// <seealso cref="FieldModel"/>
   public interface AbstractMemberInfoModel<out TMemberInfo>
      where TMemberInfo : class
   {
      /// <summary>
      /// Gets the native reflection element of this Qi4CS model.
      /// </summary>
      /// <value>The native reflection element of this Qi4CS model.</value>
      TMemberInfo NativeInfo { get; }
   }
}
