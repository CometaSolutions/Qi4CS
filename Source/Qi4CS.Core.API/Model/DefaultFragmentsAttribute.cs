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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// This class serves as base class for <see cref="Qi4CS.Core.API.Model.DefaultConcernsAttribute"/>, <see cref="Qi4CS.Core.API.Model.DefaultMixinsAttribute"/> and <see cref="Qi4CS.Core.API.Model.DefaultSideEffectsAttribute"/>.
   /// All of these attributes give a way for API writers to specify default implementation details for the types of their API.
   /// However, the bootstrapping process may override these defaults.
   /// </summary>
   /// <seealso cref="Qi4CS.Core.API.Model.DefaultConcernsAttribute"/>
   /// <seealso cref="Qi4CS.Core.API.Model.DefaultMixinsAttribute"/>
   /// <seealso cref="Qi4CS.Core.API.Model.DefaultSideEffectsAttribute"/>
   [AttributeUsage( AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true )]
   public class DefaultFragmentsAttribute : Attribute
   {
      private readonly FragmentModelType _fragmentModel;
      private readonly Type[] _fragments;

      /// <summary>
      /// This constructor is provided only for CLS compliance. Do not use it.
      /// </summary>
      /// <exception cref="NotSupportedException">Always.</exception>
      public DefaultFragmentsAttribute()
      {
         throw new NotSupportedException( "This constructor is provided only for CLS compliance." );
      }

      /// <summary>
      /// This constructor should be used by sub-types.
      /// </summary>
      /// <param name="fragmentModelType">The <see cref="FragmentModelType"/>.</param>
      /// <param name="fragments">The types of the fragments.</param>
      protected DefaultFragmentsAttribute( FragmentModelType fragmentModelType, Type[] fragments )
      {
         this._fragmentModel = fragmentModelType;
         this._fragments = fragments;
      }

      /// <summary>
      /// Gets the <see cref="FragmentModelType"/> of this attribute.
      /// </summary>
      /// <value>The <see cref="FragmentModelType"/> of this attribute.</value>
      public FragmentModelType FragmentModelType
      {
         get
         {
            return this._fragmentModel;
         }
      }

      /// <summary>
      /// Gets the fragment types of this attribute.
      /// </summary>
      /// <value>The fragment types of this attribute.</value>
      public IEnumerable<Type> Fragments
      {
         get
         {
            return this._fragments.Skip( 0 );
         }
      }
   }
}
