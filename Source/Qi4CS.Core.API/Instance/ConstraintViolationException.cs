/*
 * Copyright (c) 2008, Niclas Hedhman.
 * See NOTICE file.
 * 
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
using System.Reflection;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This exception is thrown by Qi4CS runtime if one or more constraint violation occurs in a composite method call.
   /// </summary>
   /// <remarks>
   /// For each parameter, and for each constraint violation of the parameter, all violations are aggregated.
   /// This exception will contain all such violations.
   /// The <see cref="ConstraintViolationException"/> does not extend <see cref="ArgumentException"/> as constraints may be applied to <c>out</c> and return parameters.
   /// </remarks>
   public class ConstraintViolationException : Exception
   {
      private readonly ConstraintViolationInfo[] _violations;

      /// <summary>
      /// Creates a new <see cref="ConstraintViolationException"/> with given violation information.
      /// </summary>
      /// <param name="compositeMethod">The invoked composite method.</param>
      /// <param name="violations">The occurred <see cref="ConstraintViolationInfo">violations</see>.</param>
      public ConstraintViolationException( MethodInfo compositeMethod, IList<ConstraintViolationInfo> violations )
         : base( "Constraint violation in " + compositeMethod + ", violations:\n" + String.Join( "\n", violations ) )
      {
         this._violations = violations.ToArray();
      }

      /// <summary>
      /// Gets all the violations that occurred in a composite method call.
      /// </summary>
      /// <value>All the violations that occurred in a composite method call.</value>
      public ConstraintViolationInfo[] Violations
      {
         get
         {
            return this._violations;
         }
      }
   }
}
