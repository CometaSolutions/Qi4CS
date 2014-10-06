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
   /// This attribute may be used by composites and fragments to declare default mixins in addition to those specified during bootstrap process.
   /// </summary>
   /// <example>
   /// <para>
   /// The following example shows the use of the <see cref="DefaultMixinsAttribute"/>.
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="DefaultMixinsAttributeCode1" language="C#" />
   /// Use <see cref="DefaultMixinsAttribute(Type[])"/> constructor to supply multiple mixin types at once.
   /// </para>
   /// </example>
   /// <seealso cref="DefaultFragmentsAttribute"/>
   public class DefaultMixinsAttribute : DefaultFragmentsAttribute
   {
      /// <summary>
      /// Creates a new instance of <see cref="DefaultMixinsAttribute"/> with a single mixin type.
      /// </summary>
      /// <param name="mixin">The type of the mixin.</param>
      public DefaultMixinsAttribute( Type mixin )
         : this( new[] { mixin } )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="DefaultMixinsAttribute"/> with multiple mixin types.
      /// </summary>
      /// <param name="mixins">The types of the mixins.</param>
      public DefaultMixinsAttribute( params Type[] mixins )
         : base( FragmentModelType.Mixin, mixins )
      {

      }
   }
}
