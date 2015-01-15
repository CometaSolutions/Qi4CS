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
using Qi4CS.Core.API.Model;

namespace Qi4CS.Tests.Core.Instance.Composite.Domain
{
   public interface Mutable<out MutableQueryType, out ImmutableQueryType>
      where MutableQueryType : MutableQuery<ImmutableQueryType>
   {
      MutableQueryType MQ { get; }
   }

   public interface MutableQuery<out ImmutableQueryType>
   {
      ImmutableQueryType IQ { get; }
   }

   public class MutableMixin<MutableQueryRole, ImmutableQueryRole> : Mutable<MutableQueryRole, ImmutableQueryRole>
   where MutableQueryRole : MutableQuery<ImmutableQueryRole>
   {
#pragma warning disable 649

      [This]
      private MutableQueryRole _mutableQuery;

#pragma warning restore 649

      #region Mutable<MutableQueryRole,ImmutableQueryRole> Members

      public virtual MutableQueryRole MQ
      {
         get
         {
            return this._mutableQuery;
         }
      }

      #endregion
   }

   public class MutableQueryMixin<ImmutableQueryRole> : MutableQuery<ImmutableQueryRole>
   {
#pragma warning disable 649

      [This]
      private ImmutableQueryRole _immutableQuery;

#pragma warning restore 649

      #region MutableQuery<ImmutableQueryRole> Members

      public virtual ImmutableQueryRole IQ
      {
         get
         {
            return this._immutableQuery;
         }
      }

      #endregion
   }
}
