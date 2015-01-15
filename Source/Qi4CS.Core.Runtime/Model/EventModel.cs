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
using System.Reflection;
using CollectionsWithRoles.API;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class EventModelMutable : ModelWithAttributesMutable<EventInfo>, Mutable<EventModelMutable, EventModel>, MutableQuery<EventModel>
   {
      private readonly EventModelState _state;
      private readonly EventModelImmutable _immutable;

      public EventModelMutable( EventModelState state, EventModelImmutable immutable )
         : base( state, immutable )
      {
         this._state = state;
         this._immutable = immutable;
      }

      #region Mutable<EventModelMutable,EventModel> Members

      public EventModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<EventModel> Members

      public EventModel IQ
      {
         get
         {
            return this._immutable;
         }
      }

      #endregion

      public CompositeMethodModelMutable AddMethod
      {
         get
         {
            return this._state.AddMethod;
         }
         set
         {
            this._state.AddMethod = value;
         }
      }

      public CompositeMethodModelMutable RemoveMethod
      {
         get
         {
            return this._state.RemoveMethod;
         }
         set
         {
            this._state.RemoveMethod = value;
         }
      }
   }

   public class EventModelImmutable : ModelWithAttributesImmutable<EventInfo>, EventModel
   {
      private readonly EventModelState _state;

      public EventModelImmutable( EventModelState state )
         : base( state )
      {
         this._state = state;
      }

      #region EventModel Members

      public CompositeMethodModel AddMethod
      {
         get
         {
            return this._state.AddMethod.IQ;
         }
      }

      public CompositeMethodModel RemoveMethod
      {
         get
         {
            return this._state.RemoveMethod.IQ;
         }
      }

      #endregion
   }

   public class EventModelState : ModelWithAttributesState<EventInfo>
   {
      private CompositeMethodModelMutable _addMethod;
      private CompositeMethodModelMutable _removeMethod;

      public EventModelState( CollectionsFactory collectionsFactory )
         : base( collectionsFactory )
      {

      }

      public CompositeMethodModelMutable AddMethod
      {
         get
         {
            return this._addMethod;
         }
         set
         {
            this._addMethod = value;
         }
      }

      public CompositeMethodModelMutable RemoveMethod
      {
         get
         {
            return this._removeMethod;
         }
         set
         {
            this._removeMethod = value;
         }
      }
   }
}
