using CollectionsWithRoles.API;
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
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class SideEffectMethodModelMutable : AbstractFragmentMethodModelMutable, Mutable<SideEffectMethodModelMutable, SideEffectMethodModel>, MutableQuery<SideEffectMethodModel>
   {
#pragma warning disable 414
      private readonly SideEffectMethodModelState _state;
#pragma warning restore 414

      public SideEffectMethodModelMutable( SideEffectMethodModelState state, SideEffectMethodModel immutable )
         : base( state, immutable )
      {
         this._state = state;
      }

      #region Mutable<SideEffectMethodModelMutable,SideEffectMethodModel> Members

      public SideEffectMethodModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<SideEffectMethodModel> Members

      public SideEffectMethodModel IQ
      {
         get
         {
            return ( SideEffectMethodModel )base.Immutable;
         }
      }

      #endregion
   }

   public class SideEffectMethodModelImmutable : AbstractFragmentMethodModelImmutable, SideEffectMethodModel
   {
#pragma warning disable 414
      private readonly SideEffectMethodModelState _state;
#pragma warning restore 414

      public SideEffectMethodModelImmutable( SideEffectMethodModelState state )
         : base( state )
      {
         this._state = state;
      }
   }

   public class SideEffectMethodModelState : AbstractFragmentMethodModelState
   {

   }
}
