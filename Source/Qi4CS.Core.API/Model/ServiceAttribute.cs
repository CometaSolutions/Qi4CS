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
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// This injection scope attribute can be used to inject instance(s) of service(s).
   /// </summary>
   /// <remarks>
   /// <para>
   /// Possible target types are the following:
   /// <list type="definition">
   /// <item><term>The <c>ServiceType</c></term><description>the first service, in unspecified order, castable to <c>ServiceType</c> is injected.</description></item>
   /// <item><term>The <see cref="System.Collections.Generic.IEnumerable{T}">IEnumerable&lt;<c>ServiceType</c>&gt;</see></term><description>all services castable to <c>ServiceType</c> are injected.</description></item>
   /// <item><term>The <see cref="API.Instance.ServiceReferenceInfo{T}">ServiceReferenceInfo&lt;<c>ServiceType</c>&gt;</see></term><description>then the first <see cref="API.Instance.ServiceReferenceInfo{T}"/>, in unspecified order, of services castable to <c>ServiceType</c> is injected.</description></item>
   /// <item><term>The <see cref="System.Collections.Generic.IEnumerable{T}">IEnumerable&lt;<c>ServiceType</c>&gt;</see></term><description>all <see cref="API.Instance.ServiceReferenceInfo{T}"/> of services castable to <c>ServiceType</c> are injected.</description></item>
   /// </list>
   /// </para>
   /// <para>
   /// The matching services may further be filtered by using qualifier attributes, e.g. <see cref="T:Qi4CS.Core.SPI.Model.IdentifiedByAttribute"/> or <see cref="T:Qi4CS.Core.SPI.Model.ActiveAttribute"/>.
   /// </para>
   /// </remarks>
   /// <seealso cref="API.Instance.StructureServiceProvider.FindServices(System.Collections.Generic.IEnumerable{Type})"/>
   [InjectionScope, AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false )]
   public class ServiceAttribute : Attribute
   {
   }

   /// <summary>
   /// This is attribute used to mark other attribues, which further filter the matching services for the <see cref="ServiceAttribute"/> injection.
   /// </summary>
   /// <seealso cref="T:Qi4CS.Core.SPI.Model.ServiceQualifier"/>
   [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
   public class ServiceQualifierAttribute : Attribute
   {
      private readonly Type _qualifierType;
      private readonly Boolean _changesDuringRuntime;

      /// <summary>
      /// Creates new instance of <see cref="ServiceQualifierAttribute"/> with given qualifier callback type, and whether it affects the injection time.
      /// </summary>
      /// <param name="qualifierType">The type of qualifier callback. Must not contain open generic parameters, and must implement <see cref="T:Qi4CS.Core.SPI.Model.ServiceQualifier"/> interface.</param>
      /// <param name="changesDuringRuntime">Whether requirements for the qualifier change during runtime of Qi4CS application. If this is <c>true</c>, then the value will be re-injected every time the fragment is used.</param>
      public ServiceQualifierAttribute( Type qualifierType, Boolean changesDuringRuntime )
      {
         this._qualifierType = qualifierType;
         this._changesDuringRuntime = changesDuringRuntime;
      }

      /// <summary>
      /// Gets the qualifier callback type. Must implement <see cref="T:Qi4CS.Core.SPI.Model.ServiceQualifier"/>.
      /// </summary>
      /// <value>The qualifier callback type.</value>
      public Type QualifierType
      {
         get
         {
            return this._qualifierType;
         }
      }

      /// <summary>
      /// Gets whether injection should be re-provided every time the fragment is used.
      /// </summary>
      /// <value>Whether injection should be re-provided every time the fragment is used.</value>
      public Boolean ChangesDuringRuntime
      {
         get
         {
            return this._changesDuringRuntime;
         }
      }
   }
}
