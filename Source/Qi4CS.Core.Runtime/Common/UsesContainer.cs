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
using CommonUtils;
using Qi4CS.Core.SPI.Common;

namespace Qi4CS.Core.Runtime.Common
{

   public class UsesContainerMutableImpl : UsesContainerMutable
   {
      private readonly UsesContainerState _state;
      private readonly UsesContainerQuery _query;

      public UsesContainerMutableImpl( UsesContainerState state, UsesContainerQuery query )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         ArgumentValidator.ValidateNotNull( "Query role", query );

         this._state = state;
         this._query = query;
      }

      #region UsesContainerMutable Members

      public UsesContainerQuery Query
      {
         get
         {
            return this._query;
         }
      }

      #endregion

      #region UsesProvider Members

      public UsesContainerMutable UseWithName( String name, Object value )
      {
         if ( value != null )
         {
            foreach ( var type in value.GetType().GetAllParentTypes() )
            {
               if ( name == null )
               {
                  this._state.UnnamedObjects[type] = value;
               }
               else
               {
                  IDictionary<String, Object> dic = null;
                  if ( !this._state.NamedObjects.TryGetValue( type, out dic ) )
                  {
                     dic = new Dictionary<String, Object>();
                     this._state.NamedObjects.Add( type, dic );
                  }
                  dic[name] = value;
               }
            }
         }
         return this;
      }

      #endregion

      public static UsesContainerMutable CreateEmpty()
      {
         return CreateWithParent( null );
      }

      public static UsesContainerMutable CreateWithParent( UsesContainerQuery parent )
      {
         UsesContainerState state = new UsesContainerState( parent );
         return new UsesContainerMutableImpl( state, new UsesContainerQueryImpl( state ) );
      }

      public static UsesContainerMutable CopyOf( UsesContainerQuery other )
      {
         UsesContainerState state = new UsesContainerState( other.Parent );
         UsesContainerMutable result = new UsesContainerMutableImpl( state, new UsesContainerQueryImpl( state ) );
         foreach ( KeyValuePair<String, Object> kvp in other.ThisNamedObjects )
         {
            result.UseWithName( kvp.Key, kvp.Value );
         }
         result.Use( other.ThisUnnamedObjects.ToArray() );
         return result;
      }
   }

   public class UsesContainerQueryImpl : UsesContainerQuery
   {
      private readonly UsesContainerState _state;

      public UsesContainerQueryImpl( UsesContainerState state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      #region UsesContainerQuery Members

      public Boolean HasValue( Type type, String name )
      {
         Boolean result;
         if ( type != null )
         {
            if ( name == null )
            {
               result = this._state.UnnamedObjects.ContainsKey( type );
            }
            else
            {
               result = this._state.NamedObjects.ContainsKey( type ) && this._state.NamedObjects[type].ContainsKey( name );
            }

            if ( !result && this._state.Parent != null )
            {
               result = this._state.Parent.HasValue( type, name );
            }
         }
         else
         {
            result = false;
         }
         return result;
      }

      public IEnumerable<Type> ContainedTypes
      {
         get
         {
            return this._state.UnnamedObjects.Keys.Concat( this._state.NamedObjects.Keys ).Concat( this._state.Parent == null ? Empty<Type>.Enumerable : this._state.Parent.ContainedTypes );
         }
      }

      #endregion

      #region UsesProviderQuery Members

      public Object GetObjectForName( Type type, String name )
      {
         Object result = null;
         if ( type != null )
         {
            if ( name == null && this._state.UnnamedObjects.ContainsKey( type ) )
            {
               result = this._state.UnnamedObjects[type];
            }
            else if ( name != null && this._state.NamedObjects.ContainsKey( type ) && this._state.NamedObjects[type].ContainsKey( name ) )
            {
               result = this._state.NamedObjects[type][name];
            }
            if ( result == null && this._state.Parent != null )
            {
               result = this._state.Parent.GetObjectForName( type, name );
            }
         }
         return result;
      }

      public UsesContainerQuery Parent
      {
         get
         {
            return this._state.Parent;
         }
      }

      public IEnumerable<KeyValuePair<String, Object>> ThisNamedObjects
      {
         get
         {
            return this._state.NamedObjects.SelectMany( kvp => kvp.Value ).ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
         }
      }

      public IEnumerable<Object> ThisUnnamedObjects
      {
         get
         {
            return this._state.UnnamedObjects.Values;
         }
      }

      #endregion
   }

   public class UsesContainerState
   {
      private readonly IDictionary<Type, Object> _unnamedObjects;
      private readonly IDictionary<Type, IDictionary<String, Object>> _namedObjects;
      private readonly UsesContainerQuery _parent;

      public UsesContainerState( UsesContainerQuery parent )
      {
         this._unnamedObjects = new Dictionary<Type, Object>();
         this._namedObjects = new Dictionary<Type, IDictionary<String, Object>>();
         this._parent = parent;
      }

      public IDictionary<Type, Object> UnnamedObjects
      {
         get
         {
            return this._unnamedObjects;
         }
      }

      public IDictionary<Type, IDictionary<String, Object>> NamedObjects
      {
         get
         {
            return this._namedObjects;
         }
      }

      public UsesContainerQuery Parent
      {
         get
         {
            return this._parent;
         }
      }
   }
}
