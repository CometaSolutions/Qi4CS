/*
 * Copyright (c) 2007, Rickard Öberg.
 * Copyright (c) 2007, Niclas Hedhman.
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
   /// This attribute may be used by composites and fragments to declare default concerns in addition to those specified during bootstrap process.
   /// </summary>
   /// <seealso cref="DefaultFragmentsAttribute"/>
   public sealed class DefaultConcernsAttribute : DefaultFragmentsAttribute
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultConcernsAttribute"/> with a single concern type.
      /// </summary>
      /// <param name="concern">The type of the concern.</param>
      public DefaultConcernsAttribute( Type concern )
         : this( new[] { concern } )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="DefaultConcernsAttribute"/> with multiple concern types.
      /// </summary>
      /// <param name="concerns">The types of the concerns.</param>
      public DefaultConcernsAttribute( params Type[] concerns )
         : base( FragmentModelType.Concern, concerns )
      {

      }
   }
}
