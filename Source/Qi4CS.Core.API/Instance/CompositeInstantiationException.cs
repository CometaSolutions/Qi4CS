/*
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
using System.Collections.Generic;
using System.Linq;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This exception is thrown by <see cref="CompositeBuilder.InstantiateWithType(Type)"/> method if any of the properties part of the composite state are invalid.
   /// </summary>
   public class CompositeInstantiationException : Exception
   {
      private readonly IDictionary<QualifiedName, IList<ConstraintViolationInfo>> _dic;

      /// <summary>
      /// Creates a new instance of the <see cref="CompositeInstantiationException"/>.
      /// Qi4CS runtime will call this so user code does not need to use this.
      /// </summary>
      /// <param name="dic">Information about invalid properties.</param>
      public CompositeInstantiationException( IDictionary<QualifiedName, IList<ConstraintViolationInfo>> dic )
         : base( "The following violations occurred:\n" + String.Join( "\n", dic.Select( kvp => kvp.Key + " - [" + String.Join( ", ", kvp.Value ) ) ) )
      {
         this._dic = dic;
      }

      /// <summary>
      /// Gets the information about the properties with violations.
      /// </summary>
      /// <value>The information about the properties with violations.</value>
      public IDictionary<QualifiedName, IList<ConstraintViolationInfo>> Violations
      {
         get
         {
            return this._dic;
         }
      }
   }
}
