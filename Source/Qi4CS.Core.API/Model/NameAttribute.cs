/*
 * Copyright (c) 2007, Rickard Öberg.
 * Copyright (c) 2007, Niclas Hedhman.
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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// If the parameter name available from reflection should not be used, this attribute should be applied to parameter.
   /// The value of <see cref="NameAttribute.Name"/> will be used as parameter name in <see cref="Instance.ConstraintViolationInfo"/>.
   /// </summary>
   /// <seealso cref="Instance.ConstraintViolationInfo.ParameterName"/>
   [AttributeUsage( AttributeTargets.Parameter )]
   public class NameAttribute : Attribute
   {
      private readonly String _name;

      /// <summary>
      /// Creates new instance of <see cref="NameAttribute"/> with given parameter name.
      /// </summary>
      /// <param name="name">The name of the parameter, that should be used in <see cref="Instance.ConstraintViolationInfo"/>.</param>
      public NameAttribute( String name )
      {
         this._name = name;
      }

      /// <summary>
      /// Gets the name of the parameter, that should be used in <see cref="Instance.ConstraintViolationInfo"/>.
      /// </summary>
      /// <value>The name of the parameter, that should be used in <see cref="Instance.ConstraintViolationInfo"/>.</value>
      public String Name
      {
         get
         {
            return this._name;
         }
      }
   }
}
