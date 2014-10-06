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
   /// This attribute is used to mark all attributes which are used for injections.
   /// </summary>
   [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
   public sealed class InjectionScopeAttribute : Attribute
   {
      //private readonly InjectionTime _injectionTime;
      // Each marked attribute describes a particular scope from which the injection value should be taken.
      ///// <summary>
      ///// Creates a new <see cref="InjectionScopeAttribute"/> with specified information on when to perform injection.
      ///// </summary>
      ///// <param name="injectionTime">Information on when to perform injection.</param>
      ///// <seealso cref="InjectionTime"/>
      //public InjectionScopeAttribute( InjectionTime injectionTime )
      //{
      //   this._injectionTime = injectionTime;
      //}

      ///// <summary>
      ///// Gets the information about when to perform injection.
      ///// </summary>
      ///// <value>The information about when to perform injection.</value>
      //public InjectionTime InjectionTime
      //{
      //   get
      //   {
      //      return this._injectionTime;
      //   }
      //}

      /// <summary>
      /// This is helper constant to tell default reflection elements which are injectable.
      /// </summary>
      public const AttributeTargets DEFAULT_INJECTION_TARGETS = AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue;
   }


}
