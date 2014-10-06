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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// This attribute denotes that the initial value of the property part of the composite state will be the default value for the type if none are specified during construction.
   /// </summary>
   /// <remarks>
   /// <para>
   /// These are the default values used for various types:
   /// <list type="bullet">
   /// <item><description><see cref="System.String"/>: "" (empty string)</description></item>
   /// <item><description><see cref="Nullable{T}"/>: an instance of <see cref="Nullable{T}"/> with the default value of the generic argument type.</description></item>
   /// <item><description><see cref="System.Collections.Generic.IList{T}"/>: empty <see cref="System.Collections.Generic.List{T}"/></description></item>
   /// <item><description><see cref="System.Collections.Generic.ISet{T}"/>: empty <see cref="System.Collections.Generic.HashSet{T}"/></description></item>
   /// <item><description><see cref="System.Collections.Generic.ICollection{T}"/>: empty <see cref="System.Collections.Generic.List{T}"/></description></item>
   /// <item><description><see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>: empty <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/></description></item>
   /// <item><description>Any array: empty array</description></item>
   /// <item><description><see cref="System.Collections.Generic.IEnumerable{T}"/>: empty array</description></item>
   /// <item><description>any class: return value of the parameterless constructor of the class</description></item>
   /// <item><description>any interface: return value of the parameterless constructor of the <see cref="UseDefaultsAttribute.ActualType"/></description></item>
   /// </list>
   /// </para>
   /// <para>
   /// Note that all struct types (including nullable structs) will have their default values even without <see cref="UseDefaultsAttribute"/>.
   /// This means that <see cref="UseDefaultsAttribute"/> is ignored for struct type properties.
   /// </para>
   /// <para>
   /// If this attribute is applied on a property which is not part of the composite state, it will be ignored by Qi4CS.
   /// If the target type does not match any of the above and the type does not define default constructor, validation will fail.
   /// </para>
   /// </remarks>
   [AttributeUsage( AttributeTargets.Property )]
   public sealed class UseDefaultsAttribute : Attribute
   {
      private readonly Type _actualType;

      /// <summary>
      /// Creates a new instance of <see cref="UseDefaultsAttribute"/>.
      /// </summary>
      public UseDefaultsAttribute()
         : this( null )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="UseDefaultsAttribute"/> with the hint of the actual type to use (when property type is interface, for example).
      /// </summary>
      /// <param name="actualType">The actual type to use for default property value.</param>
      public UseDefaultsAttribute( Type actualType )
      {
         this._actualType = actualType;
      }

      /// <summary>
      /// Gets the actual type supplied to this attribute. May be <c>null</c>.
      /// </summary>
      /// <value>The actual type supplied to this attribute. May be <c>null</c>.</value>
      public Type ActualType
      {
         get
         {
            return this._actualType;
         }
      }
   }
}
