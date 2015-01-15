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
   /// This is helper base class for typed side-effects.
   /// It has the <see cref="SideEffectOf{T}.result" /> field which may be used to get the result of the original invocation.
   /// </summary>
   /// <typeparam name="T">The type of the next side-effect or mixin.</typeparam>
   /// <remarks>
   /// Generic side-effects should extend the <see cref="GenericSideEffect"/> class.
   /// If the method contains <c>out</c> or <c>ref</c> parameters, modifying them will not be visible to parameters of the original invocation.
   /// </remarks>
   /// <seealso cref="SideEffectForAttribute"/>
   public abstract class SideEffectOf<T>
   {
      /// <summary>
      /// This field will contain wrapper, which will either return the result of the original invocation, if the invoked method is same as invoker method.
      /// Otherwise, a full composite method chain is performed.
      /// </summary>
      [SideEffectFor]
      protected readonly T result;
   }

   /// <summary>
   /// This is helper base class for all generic side-effects.
   /// Use the <see cref="GenericInvocator.Invoke(Object, MethodInfo, Object[])"/> method of the <c>next</c> field to get the result of the composite method if any.
   /// If an exception occurred in composite method invocation, the <see cref="GenericInvocator.Invoke(Object, MethodInfo, Object[])"/> method will throw that exception.
   /// </summary>
   /// <remarks>
   /// Please note that word 'generic' doesn't mean 'generic type' when used in conjunction with fragments.
   /// Instead, a 'generic fragment' is the one that doesn't implement domain interface but instead implements <see cref="GenericInvocator"/> which handles all related method invocation.
   /// </remarks>
   public abstract class GenericSideEffect : SideEffectOf<GenericInvocator>, GenericInvocator
   {
      #region GenericInvocator Members

      /// <inheritdoc/>
      public Object Invoke( Object composite, MethodInfo method, Object[] args )
      {
         this.DoInvoke( composite, method, args );
         return null;
      }

      #endregion

      /// <summary>
      /// This method should be overridden by subclasses to implement the side-effect logic.
      /// It differs from <see cref="GenericInvocator.Invoke(Object, MethodInfo, Object[])"/> method in such way that this method does not have return type, since the return value of side-effects is ignored by Qi4CS.
      /// </summary>
      /// <param name="composite">The composite for which the method is being invoked.</param>
      /// <param name="method">The <see cref="MethodInfo"/> of the method being invoked.</param>
      /// <param name="args">The arguments for the <paramref name="method"/>.</param>
      protected abstract void DoInvoke( Object composite, MethodInfo method, Object[] args );
   }
}
