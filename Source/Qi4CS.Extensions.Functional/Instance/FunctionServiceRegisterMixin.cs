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
using System.Threading;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Extensions.Functional.Assembling;

namespace Qi4CS.Extensions.Functional.Instance
{
   internal class FunctionServiceRegisterMixin<TKey, TComposite> : FunctionServiceLookup<TKey, TComposite>
      where TComposite : class
   {

#pragma warning disable 649

      [This]
      private FunctionServiceState<TKey, TComposite> _state;

      [Uses]
      private FunctionInfo<TKey, TComposite> _info;

      [Structure]
      private StructureServiceProvider _ssp;

#pragma warning restore 649

      #region FunctionServiceLookup<T> Members

      public virtual void RegisterFunction( TKey key, Lazy<TComposite> composite )
      {
#if WP8_BUILD
         lock ( this._state.Functions )
         {
#endif
         this._state.Functions[key] = composite;
#if WP8_BUILD
         }
#endif
      }

      public virtual void UnregisterFunction( TKey key )
      {
#if WP8_BUILD
         lock ( this._state.Functions )
         {
            this._state.Functions.Remove( key );
         }
#else
         Lazy<TComposite> composite;
         this._state.Functions.TryRemove( key, out composite );
#endif
      }

      public virtual Boolean HasFunctionFor( TKey key )
      {
         return this._state.Functions.ContainsKey( key );
      }

      #endregion

      [Prototype]
      protected void InitState()
      {
         foreach ( var creator in this._info.DefaultFunctions )
         {
            var creatorForLambda = creator; // Can't use foreach variable in lambdas since then we will get same composite x many times
            var lazeh = new Lazy<TComposite>( () => creatorForLambda.Item2( this._ssp ), LazyThreadSafetyMode.ExecutionAndPublication );
            foreach ( var key in creator.Item1 )
            {
               this.RegisterFunction( key, lazeh );
            }
         }
      }
   }
}
