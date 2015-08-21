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

namespace Qi4CS.Extensions.Functional.Model
{
   internal class KeyedRoleMap
   {
      private sealed class Nothing
      {
         public static readonly Nothing VALUE = new Nothing();
         private Nothing() { }

         public override string ToString()
         {
            return "null";
         }
      }

      private readonly IDictionary<Type, IDictionary<Object, Object>> _rolemap;
      private KeyedRoleMap _parent;


      internal KeyedRoleMap()
         : this( null )
      {
      }

      internal KeyedRoleMap( KeyedRoleMap parent )
      {
         this._parent = (KeyedRoleMap) parent;
         this._rolemap = new Dictionary<Type, IDictionary<Object, Object>>();
      }

      #region KeyedRoleMap Members

      internal void Set( Object obj )
      {
         this.Set( obj, null );
      }

      internal void Set( Object obj, params Type[] roles )
      {
         this.SetWithKey( null, obj, roles );
      }

      internal void SetWithKey( Object key, Object obj, params Type[] roles )
      {
         if ( obj == null )
         {
            foreach ( Type role in roles )
            {
               this._rolemap.Remove( role );
            }
         }
         else
         {
            if ( roles != null && roles.Length > 0 )
            {
               foreach ( Type role in roles )
               {
                  this.SetRole( role, key, obj );
               }
            }
            else
            {
               this.SetRole( obj.GetType(), key, obj );
            }
         }
      }

      public T Get<T>()
      {
         return this.GetWithKey<T>( null, typeof( T ) );
      }

      public T GetWithKey<T>( Object key )
      {
         return this.GetWithKey<T>( key, typeof( T ) );
      }

      public T Get<T>( Type role )
      {
         return this.GetWithKey<T>( null, role );
      }

      public T GetWithKey<T>( Object key, Type role )
      {
         T result;
         if ( !this.TryGetRole<T>( key, role, out result ) )
         {
            throw new ArgumentException( "No role found for " + ( key == null ? "unnamed" : ( "named (\"" + key + "\"" ) ) + " role with type " + role + "." );
         }
         return result;
      }

      public Boolean ThisHasRole<T>()
      {
         T result;
         return this.TryGetRole<T>( null, typeof( T ), out result, false );
      }

      public Boolean ThisHasRoleWithKey<T>( Object key )
      {
         T result;
         return this.TryGetRole<T>( key, typeof( T ), out result, false );
      }

      public Boolean ThisHasRole( Type role )
      {
         Object sukka;
         return this.TryGetRole<Object>( null, role, out sukka, false );
      }

      public Boolean ThisHasRoleWithKey( Object key, Type role )
      {
         Object sukka;
         return this.TryGetRole<Object>( key, role, out sukka, false );
      }

      internal void Clear( KeyedRoleMap newParent )
      {
         this._parent = newParent;
         this._rolemap.Clear();
      }

      #endregion


      private void SetRole( Type role, Object key, Object obj )
      {
         foreach ( var type in role.GetAllParentTypes() )
         {
            this.SetType( type, key, obj );
         }
         if ( role.IsEnum )
         {
            this.SetType( Enum.GetUnderlyingType( role ), key, obj );
         }
      }

      private void SetType( Type role, Object key, Object obj )
      {
         if ( role != null && !Equals( typeof( Object ), role ) )
         {
            IDictionary<Object, Object> map = null;
            if ( !this._rolemap.TryGetValue( role, out map ) )
            {
               map = new Dictionary<Object, Object>();
               this._rolemap.Add( role, map );
            }
            if ( key == null )
            {
               key = Nothing.VALUE;
            }
            map[key] = obj;
         }
      }

      public Boolean TryGetRole<T>( Object key, Type role, out T resultT )
      {
         return this.TryGetRole<T>( key, role, out resultT, true );
      }

      private Boolean TryGetRole<T>( Object key, Type role, out T resultT, Boolean searchParent )
      {
         if ( key == null )
         {
            key = Nothing.VALUE;
         }
         IDictionary<Object, Object> map;
         Boolean found = TryFindInTypeDictionarySearchBottommostType( this._rolemap, role, out map );
         Object result = null;
         if ( found )
         {
            found = map.TryGetValue( key, out result );
            resultT = (T) result;
         }
         else if ( searchParent && this._parent != null )
         {
            found = this._parent.TryGetRole<T>( key, role, out resultT );
         }
         else
         {
            resultT = default( T );
         }

         return found;
      }

      /// <summary>
      /// Tries to find a value from <paramref name="dictionary"/> with types as keys. If direct lookup fails, this method will accept value of bottom-most type of <paramref name="type"/>'s inheritance hierarchy found in <paramref name="dictionary"/>.
      /// </summary>
      /// <typeparam name="TValue">The type of the values of the <paramref name="dictionary"/>.</typeparam>
      /// <param name="type">The type to search value for.</param>
      /// <param name="dictionary">The dictionary to search from.</param>
      /// <param name="result">This will contain result if return value is <c>true</c>; otherwise <c>default(TValue)</c>.</param>
      /// <returns>If the dictionary contains key <paramref name="type"/> or any of the keys has <paramref name="type"/> as its parent type, <c>true</c>; otherwise, <c>false</c>.</returns>
      private static Boolean TryFindInTypeDictionarySearchBottommostType<TValue>( IDictionary<Type, TValue> dictionary, Type type, out TValue result )
      {
         var found = dictionary.TryGetValue( type, out result );
         if ( !found )
         {
            // Search for bottom-most type
            var current = type;
            var currentOK = false;
            foreach ( var kvp in dictionary )
            {
               currentOK = current.IsAssignableFrom( kvp.Key );
               found = currentOK || found;
               if ( currentOK )
               {
                  result = kvp.Value;
                  current = kvp.Key;
               }
            }
         }
         return found;
      }
   }
}
