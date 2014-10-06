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
   public class ConcernMethodModelMutable : AbstractFragmentMethodModelMutable, Mutable<ConcernMethodModelMutable, ConcernMethodModel>, MutableQuery<ConcernMethodModel>
   {
#pragma warning disable 414
      private readonly ConcernMethodModelState _state;
#pragma warning restore 414

      public ConcernMethodModelMutable( ConcernMethodModelState state, ConcernMethodModel immutable )
         : base( state, immutable )
      {
         this._state = state;
      }

      #region Mutable<ConcernMethodModelMutable,ConcernMethodModel> Members

      public ConcernMethodModelMutable MQ
      {
         get
         {
            return this;
         }
      }

      #endregion

      #region MutableQuery<ConcernMethodModel> Members

      public ConcernMethodModel IQ
      {
         get
         {
            return ( ConcernMethodModel )base.Immutable;
         }
      }

      #endregion
   }

   public class ConcernMethodModelImmutable : AbstractFragmentMethodModelImmutable, ConcernMethodModel
   {
#pragma warning disable 414
      private readonly ConcernMethodModelState _state;
#pragma warning restore 414

      public ConcernMethodModelImmutable( ConcernMethodModelState state )
         : base( state )
      {
         this._state = state;
      }
   }

   public class ConcernMethodModelState : AbstractFragmentMethodModelState
   {

   }
}
