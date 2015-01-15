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
   /// This injection scope attribute may be used to inject references to objects provided via methods of <see cref="API.Common.UsesProvider{T}"/>.
   /// </summary>
   /// <remarks>
   /// Specifically, the search order for suitable object is this:
   /// <list type="number">
   /// <item><description>the objects provided via <see cref="E_Qi4CS.Use{T}(Common.UsesProvider{T}, Object[])"/> and <see cref="Common.UsesProvider{T}.UseWithName(String, Object)"/> to <see cref="API.Instance.CompositeBuilder"/>,</description></item>
   /// <item><description>the objects provided via <see cref="E_Qi4CS.Use{T}(Common.UsesProvider{T}, Object[])"/> and <see cref="Common.UsesProvider{T}.UseWithName(String, Object)"/> to <see cref="API.Instance.StructureServiceProvider"/>,</description></item>
   /// <item><description>the objects provided via <see cref="E_Qi4CS.Use{T}(Common.UsesProvider{T}, Object[])"/> and <see cref="Common.UsesProvider{T}.UseWithName(String, Object)"/> to composite assembly declaration, and</description></item>
   /// <item><description>the rest of the sequence depends on architecture.</description></item>
   /// </list>
   /// </remarks>
   [InjectionScope, AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false )]
   public class UsesAttribute : Attribute
   {
      private readonly String _name;

      /// <summary>
      /// Creates a new instance of <see cref="UsesAttribute"/>, which will make Qi4CS search for objects provided via <see cref="E_Qi4CS.Use{T}(Common.UsesProvider{T}, Object[])"/> method.
      /// </summary>
      public UsesAttribute()
         : this( null )
      {

      }

      /// <summary>
      /// Creates a new instance of <see cref="UsesAttribute"/>, which will make Qi4CS search for objects provided via <see cref="Common.UsesProvider{T}.UseWithName(String, Object)"/> method if <paramref name="name"/> is non-<c>null</c>.
      /// Otherwise, the objects provided via <see cref="E_Qi4CS.Use{T}(Common.UsesProvider{T}, Object[])"/> method will be searched.
      /// </summary>
      /// <param name="name">The name of the object that was given to <see cref="Common.UsesProvider{T}.UseWithName(String, Object)"/> method.
      /// May be <c>null</c> for unnamed search.</param>
      public UsesAttribute( String name )
      {
         this._name = name;
      }

      /// <summary>
      /// Gets the name of the object to search. May be <c>null</c> for unnamed search.
      /// </summary>
      /// <value>The name of the object to search. May be <c>null</c> for unnamed search.</value>
      public String Name
      {
         get
         {
            return this._name;
         }
      }
   }
}
