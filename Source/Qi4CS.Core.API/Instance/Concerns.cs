/*
 * Copyright (c) 2008, Niclas Hedhman.
 * See NOTICE file.
 * 
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
using System;
using System.Reflection;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This is helper base class for typed concerns.
   /// It has the <see cref="ConcernOf{T}.next">"next"</see> field which may be used to invoke the next concern or mixin in composite method invocation chain.
   /// </summary>
   /// <typeparam name="T">The type of the next concern or mixin.</typeparam>
   /// <remarks>
   /// Generic concerns should extend the <see cref="GenericConcern"/> class.
   /// </remarks>
   /// <seealso cref="ConcernForAttribute"/>
   public abstract class ConcernOf<T>
   {
      /// <summary>
      /// This field will be injected with the wrapper which will either delegate the call to the next fragment (concern or mixin) in method invocation chain, if same method as the caller is invoked.
      /// Otherwise, a full composite method chain is performed.
      /// </summary>
      [ConcernFor]
      protected readonly T next;
   }

   /// <summary>
   /// This is helper base class for all generic concerns.
   /// Use the <c>next</c> field to continue the composite method invocation chain.
   /// </summary>
   /// <remarks>
   /// Please note that word 'generic' doesn't mean 'generic type' when used in conjunction with fragments.
   /// Instead, a 'generic fragment' is the one that doesn't implement domain interface but instead implements <see cref="GenericInvocator"/> which handles all related method invocation.
   /// </remarks>
   public abstract class GenericConcern : ConcernOf<GenericInvocator>, GenericInvocator
   {
      #region GenericInvocator Members

      /// <inheritdoc/>
      public abstract Object Invoke( Object composite, MethodInfo method, Object[] args );

      #endregion
   }
}
