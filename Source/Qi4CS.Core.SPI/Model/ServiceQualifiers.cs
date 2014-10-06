/*
 * Copyright (c) 2009, Rickard Öberg.
 * See NOTICE file.
 * 
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
using System.Text;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// The interface that all qualifier callbacks must implement, which is used to further filter service references.
   /// </summary>
   public interface ServiceQualifier
   {
      /// <summary>
      /// This method will be called at model validation time for each matching service composite model to specify whether it satisfies the qualifier.
      /// </summary>
      /// <param name="model">The <see cref="ServiceCompositeModel"/> of the service.</param>
      /// <param name="attribute">The qualifier attribute.</param>
      /// <returns><c>true</c> if <paramref name="model"/> satisfies the conditions of qualifier <paramref name="attribute"/>; <c>false</c> otherwise.</returns>
      Boolean Qualifies( ServiceCompositeModel model, Attribute attribute );

      /// <summary>
      /// This method will be called by Qi4CS runtime at injection time for each matching service reference to specify whether it satisfies the qualifier.
      /// </summary>
      /// <param name="reference">The service reference.</param>
      /// <param name="attribute">The qualifier attribute.</param>
      /// <returns><c>true</c> if <paramref name="reference"/> satisfies the conditions of qualifier <paramref name="attribute"/>; <c>false</c> otherwise.</returns>
      /// <seealso cref="ServiceReference"/>
      Boolean Qualifies( ServiceReference reference, Attribute attribute );
   }

   /// <summary>
   /// This is abstract skeleton implementation class for service qualifier callbacks.
   /// </summary>
   /// <typeparam name="TAttribute">The type of service qualifier attribute that this callback is aimed for.</typeparam>
   public abstract class AbstractServiceQualifier<TAttribute> : ServiceQualifier
      where TAttribute : Attribute
   {
      /// <inheritdoc />
      public Boolean Qualifies( ServiceCompositeModel model, Attribute attribute )
      {
         return this.Qualifies( model, (TAttribute) attribute );
      }

      /// <inheritdoc />
      public Boolean Qualifies( ServiceReference reference, Attribute attribute )
      {
         return this.Qualifies( reference, (TAttribute) attribute );
      }

      /// <summary>
      /// This method will be called by <see cref="Qualifies(ServiceCompositeModel, Attribute)"/>. The <paramref name="attribute"/> will be casted to <typeparamref name="TAttribute"/>.
      /// </summary>
      /// <param name="model">The <see cref="ServiceCompositeModel"/> of the service.</param>
      /// <param name="attribute">The qualifier attribute.</param>
      /// <returns><c>true</c> if <paramref name="model"/> satisfies the conditions of qualifier <paramref name="attribute"/>; <c>false</c> otherwise.</returns>
      protected abstract Boolean Qualifies( ServiceCompositeModel model, TAttribute attribute );


      /// <summary>
      /// This method will be called by <see cref="Qualifies(ServiceReference, Attribute)"/>. The <paramref name="attribute"/> will be casted to <typeparamref name="TAttribute"/>.
      /// </summary>
      /// <param name="reference">The <see cref="ServiceReference"/>.</param>
      /// <param name="attribute">The qualifier attribute.</param>
      /// <returns><c>true</c> if <paramref name="reference"/> satisfies the condition of qualifier <paramref name="attribute"/>; <c>false</c> otherwise.</returns>
      protected abstract Boolean Qualifies( ServiceReference reference, TAttribute attribute );

   }

   /// <summary>
   /// This service qualifier attribute narrows matching services to the ones that have given ID.
   /// </summary>
   [AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = true ), ServiceQualifier( typeof( IdentifiedByQualifier ), false )]
   public class IdentifiedByAttribute : Attribute
   {
      /// <summary>
      /// This is qualifier callback type for <see cref="IdentifiedByAttribute"/>.
      /// </summary>
      public sealed class IdentifiedByQualifier : AbstractServiceQualifier<IdentifiedByAttribute>
      {
         /// <summary>
         /// Checks whether given service composite model has same ID as the one specified in service qualifier attribute.
         /// </summary>
         /// <param name="model">The <see cref="ServiceCompositeModel"/>.</param>
         /// <param name="attribute">The <see cref="IdentifiedByAttribute"/>.</param>
         /// <returns><c>true</c> if <see cref="ServiceCompositeModel.ServiceID"/> property of <paramref name="model"/> equals to <see cref="IdentifiedByAttribute.ServiceID"/> property; <c>false</c> otherwise.</returns>
         protected override Boolean Qualifies( ServiceCompositeModel model, IdentifiedByAttribute attribute )
         {
            return String.Equals( model.ServiceID, attribute.ServiceID );
         }
         /// <summary>
         /// Checks whether given service reference has same ID as the one specified in service qualifier attribute.
         /// </summary>
         /// <param name="reference">The <see cref="ServiceReference"/>.</param>
         /// <param name="attribute">The <see cref="IdentifiedByAttribute"/>.</param>
         /// <returns><c>true</c> if <see cref="ServiceReference.ServiceID"/> property of <paramref name="reference"/> equals to <see cref="IdentifiedByAttribute.ServiceID"/> property; <c>false</c> otherwise.</returns>
         protected override Boolean Qualifies( ServiceReference reference, IdentifiedByAttribute attribute )
         {
            return String.Equals( reference.ServiceID, attribute.ServiceID );
         }
      }

      private readonly String _identifier;

      /// <summary>
      /// Creates new instance of <see cref="IdentifiedByAttribute"/> with given service identifier.
      /// </summary>
      /// <param name="serviceID">The service identifier.</param>
      public IdentifiedByAttribute( String serviceID )
      {
         this._identifier = serviceID;
      }

      /// <summary>
      /// Gets the ID service must have.
      /// </summary>
      /// <value>The ID service must have.</value>
      public String ServiceID
      {
         get
         {
            return this._identifier;
         }
      }
   }

   /// <summary>
   /// This service qualifier attribute narrows matching services to the ones that are currently active.
   /// </summary>
   /// <remarks>
   /// Using this qualifier makes Qi4CS runtime re-inject services every time fragment is used, which may have a negative impact on performance.
   /// </remarks>
   [AttributeUsage( InjectionScopeAttribute.DEFAULT_INJECTION_TARGETS, AllowMultiple = false ), ServiceQualifier( typeof( ActiveQualifier ), true )]
   public class ActiveAttribute : Attribute
   {
      /// <summary>
      /// This is qualifier callback type for <see cref="ActiveAttribute"/>.
      /// </summary>
      public sealed class ActiveQualifier : AbstractServiceQualifier<ActiveAttribute>
      {
         /// <summary>
         /// Returns always <c>true</c> since active check is done at runtime.
         /// </summary>
         /// <param name="model">The <see cref="ServiceCompositeModel"/>.</param>
         /// <param name="attribute">The <see cref="ActiveAttribute"/>.</param>
         /// <returns><c>true</c>.</returns>
         protected override Boolean Qualifies( ServiceCompositeModel model, ActiveAttribute attribute )
         {
            return true;
         }

         /// <summary>
         /// Checks whether given service reference has same activation status as the one specified in service qualifier attribute.
         /// </summary>
         /// <param name="reference">The <see cref="ServiceReference"/>.</param>
         /// <param name="attribute">The <see cref="ActiveAttribute"/>.</param>
         /// <returns><c>true</c> if <see cref="ServiceReference.Active"/> property for <paramref name="reference"/> has same value as <see cref="ActiveAttribute.ActiveStatus"/> property for <paramref name="attribute"/>; <c>false</c> otherwise.</returns>
         protected override Boolean Qualifies( ServiceReference reference, ActiveAttribute attribute )
         {
            return reference.Active == attribute.ActiveStatus;
         }
      }

      private readonly Boolean _active;

      /// <summary>
      /// Creates new instance of <see cref="ActiveAttribute"/> with given active status that services must have.
      /// </summary>
      /// <param name="active">Whether the service is required to be active.</param>
      public ActiveAttribute( Boolean active = true )
      {
         this._active = active;
      }

      /// <summary>
      /// Gets the value whether the service is required to be active.
      /// </summary>
      /// <value>The value whether the service is required to be active.</value>
      public Boolean ActiveStatus
      {
         get
         {
            return this._active;
         }
      }
   }
}
