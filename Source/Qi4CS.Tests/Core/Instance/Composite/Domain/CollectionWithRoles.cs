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
using System.Collections.Generic;
using System.Linq;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Tests.Core.Instance.Composite.Domain
{
   public interface CollectionWithRoles<TMutableQuery, TImmutableQuery> : Mutable<CollectionMutableQuery<TMutableQuery, TImmutableQuery>, CollectionImmutableQuery<TImmutableQuery>>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
      void Add( TMutableQuery item );
      Boolean Remove( TMutableQuery item );
      void Clear();
   }

   public interface CollectionMutableQuery<TMutableQuery, TImmutableQuery> : MutableQuery<CollectionImmutableQuery<TImmutableQuery>>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
      Boolean Contains( TMutableQuery item );
      IEnumerable<TMutableQuery> EnumerableMutables { get; }
   }

   public interface CollectionImmutableQuery<TImmutable>
   {
      Int32 Count { get; }
      Boolean Contains( TImmutable item );
      IEnumerable<TImmutable> EnumerableImmutables { get; }
   }

   public class CollectionWithRolesMixin<TMutableQuery, TImmutableQuery> : MutableMixin<CollectionMutableQuery<TMutableQuery, TImmutableQuery>, CollectionImmutableQuery<TImmutableQuery>>, CollectionWithRoles<TMutableQuery, TImmutableQuery>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
#pragma warning disable 649
      [This]
      private CollectionWithRolesState<TMutableQuery> _state;
#pragma warning restore 649

      #region CollectionWithRoles<TMutableQuery,TImmutableQuery> Members

      public virtual void Add( TMutableQuery item )
      {
         this._state.MutableCollection.Add( item );
      }

      public virtual Boolean Remove( TMutableQuery item )
      {
         return this._state.MutableCollection.Remove( item );
      }

      public virtual void Clear()
      {
         this._state.MutableCollection.Clear();
      }

      #endregion
   }

   public abstract class CollectionMutableQueryMixin<TMutableQuery, TImmutableQuery> : MutableQueryMixin<CollectionImmutableQuery<TImmutableQuery>>, CollectionMutableQuery<TMutableQuery, TImmutableQuery>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
#pragma warning disable 649
      [This]
      private readonly CollectionWithRolesState<TMutableQuery> _state;

#pragma warning restore 649

      #region CollectionMutableQuery<TMutableQuery,TImmutableQuery> Members

      public virtual Boolean Contains( TMutableQuery item )
      {
         return this._state.MutableCollection.Contains( item );
      }

      public virtual IEnumerable<TMutableQuery> EnumerableMutables
      {
         get
         {
            return this._state.MutableCollection.Skip( 0 );
         }
      }

      #endregion
   }

   public abstract class CollectionImmutableQueryMixin<TMutableQuery, TImmutable> : CollectionImmutableQuery<TImmutable>
      where TMutableQuery : MutableQuery<TImmutable>
   {
#pragma warning disable 649
      [This]
      private CollectionWithRolesState<TMutableQuery> _state;
#pragma warning restore 649

      #region CollectionImmutableQuery<TImmutable> Members

      public virtual Int32 Count
      {
         get
         {
            return this._state.MutableCollection.Count;
         }
      }

      public virtual Boolean Contains( TImmutable item )
      {
         return this._state.MutableCollection.Any( mq => ( mq == null && item == null ) || ( mq != null && Object.Equals( mq.IQ, item ) ) );
      }

      public virtual IEnumerable<TImmutable> EnumerableImmutables
      {
         get
         {
            return this._state.MutableCollection.Select( mutable => mutable.IQ );
         }
      }

      #endregion
   }

   public interface CollectionWithRolesState<TMutable>
   {
      [Immutable]
      ICollection<TMutable> MutableCollection { get; set; }
   }
}
