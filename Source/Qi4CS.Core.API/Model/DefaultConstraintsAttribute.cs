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
   /// This attribute may be used on constraint attribute to declare the implementation type of the constraint.
   /// </summary>
   /// <remarks>The implementation type must implement <see cref="Instance.Constraint{T,U}"/> interface.</remarks>
   [AttributeUsage( AttributeTargets.Class )]
   public class DefaultConstraintsAttribute : DefaultFragmentsAttribute
   {
      /// <summary>
      /// Creates new instance of <see cref="DefaultConstraintsAttribute"/> with a single constraint implementation type.
      /// </summary>
      /// <param name="constraint">The constraint implementation type.</param>
      /// <remarks>The implementation type must implement <see cref="Instance.Constraint{T,U}"/> interface.</remarks>
      public DefaultConstraintsAttribute( Type constraint )
         : this( new Type[] { constraint } )
      {
      }

      /// <summary>
      /// Creates new instance of <see cref="DefaultConstraintsAttribute"/> with multiple constraint implementation types.
      /// All types will be used to check validity of the parameter value.
      /// </summary>
      /// <param name="constraints">The constraint implementation types.</param>
      /// <remarks>All implementation types must implement <see cref="Instance.Constraint{T,U}"/> interface.</remarks>
      public DefaultConstraintsAttribute( params Type[] constraints )
         : base( FragmentModelType.Constraint, constraints )
      {
      }

   }
}
