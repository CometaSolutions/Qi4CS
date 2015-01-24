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
using System.Reflection;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Extensions.Functional.Instance;
using Qi4CS.Extensions.Functional.Model;

namespace Qi4CS.Extensions.Functional.Assembling
{
   internal class FunctionInfo<TKey, TComposite>
   {
      private readonly IDictionary<MethodInfo, MethodInfo> _functionMethods;
      private readonly IDictionary<MethodInfo, Func<Object[], Object[]>> _preArgTransformers;
      private readonly IDictionary<MethodInfo, Action<Object[], Object[]>> _postArgTransformers;
      private readonly IDictionary<MethodInfo, FunctionInvocationHelper> _invocationHelpers;
      private readonly IDictionary<Type, IEnumerable<MethodInfo>> _compositeInfoMethods;
      //private readonly IDictionary<MethodInfo, Func<TKey, TComposite, Object[], IEnumerable<Tuple<String, Object>>>> _additionalRoles;

      private readonly IDictionary<MethodInfo, Func<Object[], IEnumerable<TKey>>> _lookupFuncs;
      private readonly IDictionary<MethodInfo, Func<Application, IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, Lazy<TComposite>>>, Object[], Tuple<TKey, TComposite, Boolean>>> _directLookupFailFuncs;

      private readonly IList<Tuple<TKey[], Func<StructureServiceProvider, TComposite>>> _defaultFunctions;

      internal FunctionInfo( ServiceCompositeAssemblyDeclaration assDecl )
      {
         this._functionMethods = new Dictionary<MethodInfo, MethodInfo>();
         this._preArgTransformers = new Dictionary<MethodInfo, Func<Object[], Object[]>>();
         this._postArgTransformers = new Dictionary<MethodInfo, Action<Object[], Object[]>>();
         this._invocationHelpers = new Dictionary<MethodInfo, FunctionInvocationHelper>();
         this._compositeInfoMethods = new Dictionary<Type, IEnumerable<MethodInfo>>();
         //this._additionalRoles = new Dictionary<MethodInfo, Func<TKey, TComposite, Object[], IEnumerable<Tuple<String, Object>>>>();

         this._lookupFuncs = new Dictionary<MethodInfo, Func<Object[], IEnumerable<TKey>>>();
         this._directLookupFailFuncs = new Dictionary<MethodInfo, Func<Application, IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, Lazy<TComposite>>>, Object[], Tuple<TKey, TComposite, Boolean>>>();

         this._defaultFunctions = new List<Tuple<TKey[], Func<StructureServiceProvider, TComposite>>>();
      }

      internal IDictionary<MethodInfo, MethodInfo> FunctionMethods
      {
         get
         {
            return this._functionMethods;
         }
      }

      internal IDictionary<MethodInfo, Func<Object[], Object[]>> ArgsPreTransformers
      {
         get
         {
            return this._preArgTransformers;
         }
      }

      internal IDictionary<MethodInfo, Action<Object[], Object[]>> ArgsPostTransformers
      {
         get
         {
            return this._postArgTransformers;
         }
      }

      internal IDictionary<MethodInfo, FunctionInvocationHelper> InvocationHelpers
      {
         get
         {
            return this._invocationHelpers;
         }
      }

      internal IDictionary<Type, IEnumerable<MethodInfo>> MethodForCompositeInfos
      {
         get
         {
            return this._compositeInfoMethods;
         }
      }

      //internal IDictionary<MethodInfo, Func<TKey, TComposite, Object[], IEnumerable<Tuple<String, Object>>>> AdditionalRoles
      //{
      //   get
      //   {
      //      return this._additionalRoles;
      //   }
      //}

      internal IEnumerable<TKey> LookUp( MethodInfo serviceMethod, Object[] args )
      {
         return this._lookupFuncs[serviceMethod]( args );
      }


      internal IDictionary<MethodInfo, Func<Object[], IEnumerable<TKey>>> LookupFuncs
      {
         get
         {
            return this._lookupFuncs;
         }
      }

      internal IDictionary<MethodInfo, Func<Application, IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, Lazy<TComposite>>>, Object[], Tuple<TKey, TComposite, Boolean>>> DirectLookupFailFuncs
      {
         get
         {
            return this._directLookupFailFuncs;
         }
      }

      internal IList<Tuple<TKey[], Func<StructureServiceProvider, TComposite>>> DefaultFunctions
      {
         get
         {
            return this._defaultFunctions;
         }
      }
   }
}
