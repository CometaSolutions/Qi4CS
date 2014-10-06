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
using CommonUtils;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public abstract class AbstractMemberInfoModelMutable<TMember>
      where TMember : class
   {

      private readonly AbstractMemberInfoModelState<TMember> _state;
      private readonly AbstractMemberInfoModel<TMember> _immutable;

      public AbstractMemberInfoModelMutable( AbstractMemberInfoModelState<TMember> state, AbstractMemberInfoModel<TMember> immutable )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         ArgumentValidator.ValidateNotNull( "Immutable", immutable );

         this._state = state;
         this._immutable = immutable;
      }

      public AbstractMemberInfoModel<TMember> Immutable
      {
         get
         {
            return this._immutable;
         }
      }

      public TMember NativeInfo
      {
         set
         {
            this._state.NativeInfo = value;
         }
         get
         {
            return this._state.NativeInfo;
         }
      }

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) || ( obj is AbstractMemberInfoModelMutable<TMember> && this._immutable.Equals( ( (AbstractMemberInfoModelMutable<TMember>) obj ).Immutable ) );
      }

      public override Int32 GetHashCode()
      {
         return this._immutable.GetHashCode();
      }

      public override String ToString()
      {
         return this._state.ToString();
      }
   }

   public abstract class AbstractMemberInfoModelImmutable<TMember> : AbstractMemberInfoModel<TMember>
      where TMember : class
   {
      private readonly AbstractMemberInfoModelState<TMember> _state;

      public AbstractMemberInfoModelImmutable( AbstractMemberInfoModelState<TMember> state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      #region AbstractMemberInfoModel<MemberType> Members

      public TMember NativeInfo
      {
         get
         {
            return this._state.NativeInfo;
         }
      }

      #endregion

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) || ( obj is AbstractMemberInfoModel<TMember> && Object.Equals( this._state.NativeInfo, ( (AbstractMemberInfoModel<TMember>) obj ).NativeInfo ) );
      }

      public override Int32 GetHashCode()
      {
         return this._state.NativeInfo == null ? 0 : this._state.NativeInfo.GetHashCode();
      }

      public override String ToString()
      {
         return this._state.ToString();
      }
   }

   public class AbstractMemberInfoModelState<TMember>
      where TMember : class
   {
      private TMember _member;

      public AbstractMemberInfoModelState()
      {
      }

      public TMember NativeInfo
      {
         get
         {
            return this._member;
         }
         set
         {
            this._member = value;
         }
      }

      public override String ToString()
      {
         return this._member == null ? "" : this._member.ToString();
      }
   }
}
