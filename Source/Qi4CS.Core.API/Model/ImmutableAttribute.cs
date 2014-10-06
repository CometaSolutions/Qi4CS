/*
 * Copyright (c) 2008, Rickard Öberg.
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
   /// This attribute makes property immutable, if applied to property.
   /// If applied to type, it makes all the properties of the type (and its sub-types) immutable.
   /// </summary>
   [AttributeUsage( AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false )]
   public class ImmutableAttribute : Attribute
   {
      /// <summary>
      /// All instances of <see cref="ImmutableAttribute"/> are considered to be equal.
      /// Therefore this returns true if <paramref name="obj"/> is of type <see cref="ImmutableAttribute"/>.
      /// </summary>
      /// <param name="obj">The other object to check equality against.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is non-<c>null</c> and is of type <see cref="ImmutableAttribute"/>; <c>false</c> otherwise.</returns>
      public override Boolean Equals( Object obj )
      {
         return obj is ImmutableAttribute;
      }

      /// <summary>
      /// All instances of <see cref="ImmutableAttribute"/> are considered to be equal.
      /// Therefore this returns the hash code of <see cref="ImmutableAttribute"/> type.
      /// </summary>
      /// <returns>The hash code of <see cref="ImmutableAttribute"/> type.</returns>
      public override Int32 GetHashCode()
      {
         return typeof( ImmutableAttribute ).GetHashCode();
      }
   }
}
