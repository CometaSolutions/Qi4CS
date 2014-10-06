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
   /// This is common interface for all models of fragment methods part of the composite method invocation chain (mixin, concern, and side-effect methods).
   /// </summary>
   /// <seealso cref="MixinMethodModel"/>
   /// <seealso cref="ConcernMethodModel"/>
   /// <seealso cref="SideEffectMethodModel"/>
   public interface AbstractFragmentMethodModel : AbstractMethodModel, AbstractMethodModelWithFragment
   {
      /// <summary>
      /// Gets the <see cref="CompositeMethodModel"/> this fragment method model belongs to.
      /// </summary>
      /// <value>The <see cref="CompositeMethodModel"/> this fragment method model belongs to.</value>
      /// <seealso cref="CompositeMethodModel"/>
      CompositeMethodModel CompositeMethod { get; }

      /// <summary>
      /// Gets the value indicating whether the method invocation chain should invoke this method through <see cref="API.Instance.GenericInvocator"/> interface.
      /// </summary>
      /// <value><c>true</c> if this fragment method should be invoked through <see cref="API.Instance.GenericInvocator"/> interface; <c>false</c> otherwise.</value>
      Boolean IsGeneric { get; }
   }

   /// <summary>
   /// This is common interface for all models of fragment methods.
   /// </summary>
   /// <seealso cref="MixinMethodModel"/>
   /// <seealso cref="ConcernMethodModel"/>
   /// <seealso cref="SideEffectMethodModel"/>
   /// <seealso cref="SpecialMethodModel"/>
   public interface AbstractMethodModelWithFragment
   {
      /// <summary>
      /// Gets the type of the fragment the method of this model describes.
      /// </summary>
      /// <value>The type of the fragment the method of this model describes.</value>
      Type FragmentType { get; }
   }
}
