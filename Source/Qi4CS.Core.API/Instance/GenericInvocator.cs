using System;
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
using System.Reflection;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// The interface that must be implemented by all generic fragments. Provides a single method to handle invocation of any composite method where this fragment is part of method invocation chain.
   /// </summary>
   /// <remarks>
   /// Please note that word 'generic' doesn't mean 'generic type' when used in conjunction with fragments.
   /// Instead, a 'generic fragment' is the one that doesn't implement domain interface but instead implements this interface which handles all related method invocation.
   /// </remarks>
   public interface GenericInvocator
   {
      /// <summary>
      /// This method is invoked by Qi4CS whenever it is generic fragment's turn in composite method call chain.
      /// </summary>
      /// <param name="composite">The composite for which the method is being invoked.</param>
      /// <param name="method">The <see cref="MethodInfo"/> of the method being invoked.</param>
      /// <param name="args">The arguments for the <paramref name="method"/>.</param>
      /// <returns>Should return object castable to return type of <paramref name="method"/>. If return value is <c>null</c> and return type is value type or generic parameter, Qi4CS runtime will load the <c>default</c> value for such type.</returns>
      Object Invoke( Object composite, MethodInfo method, Object[] args );
   }
}
