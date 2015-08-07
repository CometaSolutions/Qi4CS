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
using System.Reflection;
using System.Threading;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Extensions.Functional.Assembling;
using Qi4CS.Extensions.Functional.Model;
#if SILVERLIGHT
using CommonUtils;
#endif


namespace Qi4CS.Extensions.Functional.Instance
{


   internal static class RoleMapHolder
   {
      internal static readonly ThreadLocal<Stack<KeyedRoleMap>> ROLE_MAPS = new ThreadLocal<Stack<KeyedRoleMap>>( () => new Stack<KeyedRoleMap>() );
      internal static readonly InstancePool<KeyedRoleMap> ROLE_MAPS_POOL = new InstancePool<KeyedRoleMap>();
      internal static readonly ThreadLocal<InvocationDataHolder> INVOCATION_DATA = new ThreadLocal<InvocationDataHolder>( () => new InvocationDataHolder() );
   }

   internal class FunctionServiceMixin<TKey, TComposite> : GenericInvocator
      where TComposite : class
   {

#pragma warning disable 649

      [This]
      private FunctionServiceState<TKey, TComposite> _state;

      [Uses]
      private FunctionInfo<TKey, TComposite> _info;

      [Structure]
      private Application _application;

      [Structure]
      private CompositeInstance _cInstance;

#pragma warning restore 649

      #region GenericInvocator Members

      /// <inheritdoc />
      public Object Invoke( Object composite, MethodInfo method, Object[] args )
      {
         MethodInfo methodKey = method;
         if ( methodKey.IsGenericMethod && !methodKey.IsGenericMethodDefinition )
         {
            methodKey = methodKey.GetGenericMethodDefinition();
         }
         var keys = this._info.LookUp( methodKey, args );

         var map = this.BeforeFunction( method, args );
         var funcs = this._state.Functions;
#if SILVERLIGHT
         // Create temp copy of functions
         lock ( this._state.Functions )
         {
            funcs = funcs.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
         }
#endif
         try
         {
            TComposite functionComposite = null;
            if ( keys != null )
            {
               foreach ( var theKey in keys )
               {
                  Lazy<TComposite> lazeh;
                  if ( funcs.TryGetValue( theKey, out lazeh ) )
                  {
                     functionComposite = lazeh.Value;
                     //this.AddCustomRoles( methodKey, theKey, args, map, functionComposite );
                     break;
                  }
               }
            }
            if ( functionComposite == null )
            {
               Func<Application, IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, Lazy<TComposite>>>, Object[], Tuple<TKey, TComposite, Boolean>> indirectLookup;
               if ( this._info.DirectLookupFailFuncs.TryGetValue( methodKey, out indirectLookup ) )
               {
                  var lookupResult = indirectLookup( this._application, keys, funcs.ToArray(), args );
                  functionComposite = lookupResult.Item2;
                  var key = lookupResult.Item1;
                  if ( lookupResult.Item3 )
                  {
                     var lazeh = new Lazy<TComposite>( () => functionComposite, LazyThreadSafetyMode.ExecutionAndPublication );
                     funcs[key] = lazeh;
#if SILVERLIGHT
                     lock ( this._state.Functions )
                     {
                        this._state.Functions[key] = lazeh;
                     }
#endif
                  }
                  //this.AddCustomRoles( methodKey, key, args, map, functionComposite );
               }
               else
               {
                  throw new ArgumentException( "Could not find function for key " + String.Join( ", ", keys ) + "." );
               }
            }

            var argsForMethod = this._info.ArgsPreTransformers[methodKey]( args );
            if ( argsForMethod == null )
            {
               argsForMethod = args;
            }
            FunctionInvocationHelper helper;
            Object result;
            if ( this._info.InvocationHelpers.TryGetValue( methodKey, out helper ) )
            {
               result = helper.InvokeMethod( functionComposite, argsForMethod );
            }
            else
            {
               MethodInfo functionMethod = this._info.FunctionMethods[methodKey];
               Type functionType = functionMethod.DeclaringType;
               if ( functionMethod.IsGenericMethodDefinition )
               {
                  functionMethod.MakeGenericMethod( method.GetGenericArguments() );
               }
               functionMethod = (MethodInfo) MethodBase.GetMethodFromHandle( functionMethod.MethodHandle, functionComposite.GetType().TypeHandle );
               result = functionMethod.Invoke( functionComposite, argsForMethod );
            }
            this._info.ArgsPostTransformers[methodKey]( args, argsForMethod );
            return result;
         }
         finally
         {
            this.AfterFunction( map );
         }
      }

      #endregion

      private KeyedRoleMap BeforeFunction( MethodInfo cMethod, Object[] args )
      {
         KeyedRoleMap parent = RoleMapHolder.ROLE_MAPS.Value.Any() ? RoleMapHolder.ROLE_MAPS.Value.Peek() : null;
         KeyedRoleMap map;
         if ( !RoleMapHolder.ROLE_MAPS_POOL.TryTake( out map ) )
         {
            map = new KeyedRoleMap( parent );
         }
         else
         {
            map.Clear( parent );
         }
         Int32 idx = 0;
         foreach ( ParameterModel pModel in this._cInstance.MethodToModelMapping[cMethod].Parameters )
         {
            RoleParameterAttribute attr = pModel.AllAttributes.OfType<RoleParameterAttribute>().FirstOrDefault();
            if ( attr != null )
            {
               map.SetWithKey( attr.Name, args[idx] );
            }
            ++idx;
         }
         RoleMapHolder.ROLE_MAPS.Value.Push( map );

         RoleMapHolder.INVOCATION_DATA.Value.FunctionStarted();
         //CompositeHolder.COMPOSITES.Value.AddLast( composite );

         return map;
      }

      //private void AddCustomRoles( MethodInfo methodKey, TKey key, Object[] args, KeyedRoleMap map, TComposite functionComposite )
      //{
      //   Func<TKey, TComposite, Object[], IEnumerable<Tuple<String, Object>>> additionalRoles;
      //   if ( this._info.AdditionalRoles.TryGetValue( methodKey, out additionalRoles ) && additionalRoles != null )
      //   {
      //      foreach ( var role in additionalRoles( key, functionComposite, args ) )
      //      {
      //         map.SetWithKey( role.Item1, role.Item2 );
      //      }
      //   }
      //}

      private void AfterFunction( KeyedRoleMap instance )
      {
         //CompositeHolder.COMPOSITES.Value.RemoveLast();
         RoleMapHolder.ROLE_MAPS.Value.Pop();
         RoleMapHolder.ROLE_MAPS_POOL.Return( instance );

         RoleMapHolder.INVOCATION_DATA.Value.FunctionEnded();
      }
   }

   internal interface FunctionServiceState<TKey, TComposite>
      where TComposite : class
   {
      [Immutable, UseDefaults]
#if SILVERLIGHT
      IDictionary<TKey, Lazy<TComposite>> Functions { get; set; }
#else
      System.Collections.Concurrent.ConcurrentDictionary<TKey, Lazy<TComposite>> Functions { get; set; }
#endif
   }

   internal class InvocationDataHolder
   {
      private readonly KeyedRoleMap _rolemap;
      private Int32 _depth;

      internal InvocationDataHolder()
      {
         this._rolemap = new KeyedRoleMap();
         this._depth = 0;
      }

      internal KeyedRoleMap RoleMap
      {
         get
         {
            return this._rolemap;
         }
      }

      internal void FunctionStarted()
      {
         ++this._depth;
      }

      internal void FunctionEnded()
      {
         --this._depth;
         if ( this._depth == 0 )
         {
            this._rolemap.Clear( null );
         }
      }
   }
}
