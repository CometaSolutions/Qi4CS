/*
 * Copyright (c) 2007, Rickard Öberg.
 * See NOTICE file.
 * 
 * Copyright 2012 Stanislav Muhametsin. All rights Reserved.
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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// This attribute may be used by composites and fragments to declare default side-effects in addition to those specified during bootstrap process.
   /// </summary>
   /// <seealso cref="DefaultFragmentsAttribute"/>
   public class DefaultSideEffectsAttribute : DefaultFragmentsAttribute
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultSideEffectsAttribute"/> with a single side-effect type.
      /// </summary>
      /// <param name="sideEffect">The type of the side-effect.</param>
      public DefaultSideEffectsAttribute( Type sideEffect )
         : this( new[] { sideEffect } )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="DefaultSideEffectsAttribute"/> with multiple side-effect types.
      /// </summary>
      /// <param name="sideEffects">The types of the side-effects.</param>
      public DefaultSideEffectsAttribute( params Type[] sideEffects )
         : base( FragmentModelType.SideEffect, sideEffects )
      {

      }
   }
}
