/*
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
using System.Linq;
using CollectionsWithRoles.API;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public interface AbstractInjectableModelMutable
   {
      Attribute InjectionScope { get; }

      Type TargetType { get; }
   }

   public abstract class AbstractInjectableModelMutableImpl<TMember> : ModelWithAttributesMutable<TMember>, AbstractInjectableModelMutable
      where TMember : class
   {
#pragma warning disable 414
      private readonly AbstractInjectableModelState<TMember> _state;
#pragma warning restore 414
      private readonly AbstractInjectableModelImmutable<TMember> _immutable;

      public AbstractInjectableModelMutableImpl( AbstractInjectableModelState<TMember> state, AbstractInjectableModelImmutable<TMember> immutable )
         : base( state, immutable )
      {
         this._state = state;
         this._immutable = immutable;
      }


      #region AbstractInjectableModelMutable Members

      public Attribute InjectionScope
      {
         get
         {
            return this._immutable.InjectionScope;
         }
      }

      public abstract Type TargetType { get; }

      #endregion
   }

   public abstract class AbstractInjectableModelImmutable<TMember> : ModelWithAttributesImmutable<TMember>, AbstractInjectableModel
      where TMember : class
   {
      private readonly AbstractInjectableModelState<TMember> _state;

      public AbstractInjectableModelImmutable( AbstractInjectableModelState<TMember> state )
         : base( state )
      {
         this._state = state;
      }

      #region AbstractInjectableModel Members

      public Attribute InjectionScope
      {
         get
         {
            return this.GetAttributesMarkedWith( typeof( InjectionScopeAttribute ) ).FirstOrDefault();
         }
      }

      public Type TargetType
      {
         get
         {
            return this._state.TargetType;
         }
      }

      public abstract Type DeclaringType { get; }

      public abstract CompositeModel CompositeModel { get; }

      #endregion

      #region OptionalInfo Members

      public Boolean IsOptional
      {
         get
         {
            return this._state.IsOptional;
         }
      }

      #endregion
   }

   public class AbstractInjectableModelState<TMember> : ModelWithAttributesState<TMember>
      where TMember : class
   {
      private Boolean _isOptional;
      private Type _targetType;

      public AbstractInjectableModelState( CollectionsFactory factory )
         : base( factory )
      {

      }

      public Boolean IsOptional
      {
         get
         {
            return this._isOptional;
         }
         set
         {
            this._isOptional = value;
         }
      }

      public Type TargetType
      {
         get
         {
            return this._targetType;
         }
         set
         {
            value.IsLazy( out this._targetType );
         }
      }
   }
}
