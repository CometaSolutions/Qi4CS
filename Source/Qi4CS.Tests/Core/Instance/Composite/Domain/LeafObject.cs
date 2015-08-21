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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Tests.Core.Instance.Composite.Domain
{
   public interface LeafObject<out MutableQueryRole, out ImmutableQueryRole, in MutableValueType, out ImmutableValueType> : AbstractObject<MutableQueryRole, ImmutableQueryRole>
      where MutableQueryRole : LeafObjectQ<ImmutableQueryRole, MutableValueType, ImmutableValueType>
      where ImmutableQueryRole : LeafObjectIQ<ImmutableValueType>
   {
      MutableValueType AssignableValue { set; }
   }

   public interface LeafObjectQ<out ImmutableQueryRole, out MutableValueType, out ImmutableValueType> : AbstractObjectQ<ImmutableQueryRole>
      where ImmutableQueryRole : LeafObjectIQ<ImmutableValueType>
   {
      MutableValueType MutableValue { get; }
   }

   public interface LeafObjectIQ<out ImmutableValueType> : AbstractObjectIQ
   {
      ImmutableValueType ImmutableValue { get; }
   }

   public class LeafObjectMixin<MutableQueryRole, ImmutableQueryRole, MutableValueType, ImmutableValueType> : AbstractObjectMixin<MutableQueryRole, ImmutableQueryRole>, LeafObject<MutableQueryRole, ImmutableQueryRole, MutableValueType, ImmutableValueType>
      where MutableQueryRole : LeafObjectQ<ImmutableQueryRole, MutableValueType, ImmutableValueType>
      where ImmutableQueryRole : LeafObjectIQ<ImmutableValueType>
   {
#pragma warning disable 649
      [This]
      private readonly LeafObjectState<MutableValueType> _state;
#pragma warning restore 649

      #region LeafObject<MutableQueryRole,ImmutableQueryRole,MutableValueType,ImmutableValueType> Members

      public virtual MutableValueType AssignableValue
      {
         set
         {
            this._state.Value = value;
         }
      }

      #endregion

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) ||
            ( obj is LeafObject<MutableQueryRole, ImmutableQueryRole, MutableValueType, ImmutableValueType> &&
            Object.Equals( this._state.Value, ( (LeafObject<MutableQueryRole, ImmutableQueryRole, MutableValueType, ImmutableValueType>) obj ).MQ.MutableValue )
            );
      }

      public override Int32 GetHashCode()
      {
         return this._state.Value.GetHashCode();
      }
   }

   public class LeafObjectQMixin<ImmutableQueryRole, MutableValueType, ImmutableValueType> : AbstractObjectQMixin<ImmutableQueryRole>, LeafObjectQ<ImmutableQueryRole, MutableValueType, ImmutableValueType>
      where ImmutableQueryRole : LeafObjectIQ<ImmutableValueType>
   {
#pragma warning disable 649
      [This]
      private LeafObjectState<MutableValueType> _state;

#pragma warning restore 649

      #region LeafObjectQ<ImmutableQueryRole,MutableValueType,ImmutableValueType> Members

      public virtual MutableValueType MutableValue
      {
         get
         {
            return this._state.Value;
         }
      }

      #endregion

      protected LeafObjectState<MutableValueType> State
      {
         get
         {
            return this._state;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) ||
            ( obj is LeafObjectQ<ImmutableQueryRole, MutableValueType, ImmutableValueType> &&
            Object.Equals( this._state.Value, ( (LeafObjectQ<ImmutableQueryRole, MutableValueType, ImmutableValueType>) obj ).MutableValue )
            );
      }

      public override Int32 GetHashCode()
      {
         return this._state.Value.GetHashCode();
      }
   }

   public abstract class LeafObjectIQMixin<MutableValueType, ImmutableValueType> : AbstractObjectIQMixin, LeafObjectIQ<ImmutableValueType>
   {
#pragma warning disable 649
      [This]
      private LeafObjectState<MutableValueType> _state;

#pragma warning restore 649

      #region LeafObjectIQ<ImmutableValueType> Members

      public virtual ImmutableValueType ImmutableValue
      {
         get
         {
            return this.Transform( this._state.Value );
         }
      }

      #endregion

      protected LeafObjectState<MutableValueType> State
      {
         get
         {
            return this._state;
         }
      }

      protected abstract ImmutableValueType Transform( MutableValueType mutableValue );
   }

   public interface LeafObjectState<MutableValueType>
   {
      [Optional]
      MutableValueType Value { get; set; }
   }

   public abstract class CommonValueTypeLeafObjectMixin<MutableQueryRole, ImmutableQueryRole, CommonValueType> : LeafObjectMixin<MutableQueryRole, ImmutableQueryRole, CommonValueType, CommonValueType>, LeafObjectQ<ImmutableQueryRole, CommonValueType, CommonValueType>
      where MutableQueryRole : LeafObjectQ<ImmutableQueryRole, CommonValueType, CommonValueType>
      where ImmutableQueryRole : LeafObjectIQ<CommonValueType>
   {
#pragma warning disable 649
      [This]
      private LeafObjectState<CommonValueType> _state;

      [This]
      private ImmutableQueryRole _iq;

#pragma warning restore 649

      #region LeafObjectQ<ImmutableQueryRole,CommonValueType,CommonValueType> Members

      public virtual CommonValueType MutableValue
      {
         get
         {
            return this._state.Value;
         }
      }

      #endregion

      #region MutableQuery<ImmutableQueryRole> Members

      public virtual ImmutableQueryRole IQ
      {
         get
         {
            return this._iq;
         }
      }

      #endregion
   }

   public class CommonValueTypeLeafObjectIQMixin<CommonValueType> : LeafObjectIQMixin<CommonValueType, CommonValueType>
   {

      protected override CommonValueType Transform( CommonValueType mutableValue )
      {
         return mutableValue;
      }
   }
}
