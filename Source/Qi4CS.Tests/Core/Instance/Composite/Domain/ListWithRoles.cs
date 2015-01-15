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
using Qi4CS.Core.API.Model;

namespace Qi4CS.Tests.Core.Instance.Composite.Domain
{
   public interface ListWithRoles<TMutableQuery, TImmutableQuery> : Mutable<ListMutableQuery<TMutableQuery, TImmutableQuery>, ListImmutableQuery<TImmutableQuery>>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
      CollectionWithRoles<TMutableQuery, TImmutableQuery> AsCollectionWithRoles { get; }
      TMutableQuery this[int index] { set; }
      void Insert( int index, TMutableQuery item );
      void RemoveAt( int index );
   }

   public interface ListMutableQuery<TMutableQuery, TImmutableQuery> : MutableQuery<ListImmutableQuery<TImmutableQuery>>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
      CollectionMutableQuery<TMutableQuery, TImmutableQuery> AsCollectionMutableQuery { get; }
      TMutableQuery this[int index] { get; }
      int IndexOf( TMutableQuery item );
   }

   public interface ListImmutableQuery<TImmutable>
   {
      CollectionImmutableQuery<TImmutable> AsCollectionImmutableQuery { get; }
      TImmutable this[int index] { get; }
      int IndexOf( TImmutable item );
   }

   public abstract class ListWithRolesMixin<TMutableQuery, TImmutableQuery> : MutableMixin<ListMutableQuery<TMutableQuery, TImmutableQuery>, ListImmutableQuery<TImmutableQuery>>, ListWithRoles<TMutableQuery, TImmutableQuery>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
#pragma warning disable 649
      [This]
      private ListWithRolesState<TMutableQuery> _state;

      [This]
      private CollectionWithRoles<TMutableQuery, TImmutableQuery> _collection;

#pragma warning restore 649

      #region ListWithRoles<TMutableQuery,TImmutableQuery> Members

      public virtual CollectionWithRoles<TMutableQuery, TImmutableQuery> AsCollectionWithRoles
      {
         get
         {
            return this._collection;
         }
      }

      public virtual TMutableQuery this[int index]
      {
         set
         {
            this._state.List[index] = value;
         }
      }

      public virtual void Insert( int index, TMutableQuery item )
      {
         this._state.List.Insert( index, item );
      }

      public virtual void RemoveAt( int index )
      {
         this._state.List.RemoveAt( index );
      }

      #endregion
   }

   public abstract class ListMutableQueryMixin<TMutableQuery, TImmutableQuery> : MutableQueryMixin<ListImmutableQuery<TImmutableQuery>>, ListMutableQuery<TMutableQuery, TImmutableQuery>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
#pragma warning disable 649
      [This]
      private ListWithRolesState<TMutableQuery> _state;

      [This]
      private CollectionMutableQuery<TMutableQuery, TImmutableQuery> _collection;

#pragma warning restore 649

      #region ListMutableQuery<TMutableQuery,TImmutableQuery> Members

      public virtual CollectionMutableQuery<TMutableQuery, TImmutableQuery> AsCollectionMutableQuery
      {
         get
         {
            return this._collection;
         }
      }

      public virtual TMutableQuery this[int index]
      {
         get
         {
            return this._state.List[index];
         }
      }

      public virtual int IndexOf( TMutableQuery item )
      {
         return this._state.List.IndexOf( item );
      }

      #endregion
   }

   public class ListImmutableQueryMixin<TMutableQuery, TImmutableQuery> : ListImmutableQuery<TImmutableQuery>
      where TMutableQuery : MutableQuery<TImmutableQuery>
   {
#pragma warning disable 649
      [This]
      private ListWithRolesState<TMutableQuery> _state;

      [This]
      private CollectionImmutableQuery<TImmutableQuery> _collection;
#pragma warning restore 649

      #region ListImmutableQuery<TImmutableQuery> Members

      public virtual CollectionImmutableQuery<TImmutableQuery> AsCollectionImmutableQuery
      {
         get
         {
            return this._collection;
         }
      }

      public virtual TImmutableQuery this[int index]
      {
         get
         {
            return this._state.List[index].IQ;
         }
      }

      public virtual int IndexOf( TImmutableQuery item )
      {
         Int32 result = -1;
         Int32 idx = 0;
         while ( result == -1 && idx < this._state.List.Count )
         {
            TMutableQuery mq = this._state.List[idx];
            if ( ( mq == null && item == null ) || ( mq != null && Object.Equals( mq.IQ, item ) ) )
            {
               result = idx;
            }
            ++idx;
         }

         return result;
      }

      #endregion

   }

   public interface ListWithRolesState<MutableType>
   {
      [Immutable]
      IList<MutableType> List { get; set; }
   }
}
