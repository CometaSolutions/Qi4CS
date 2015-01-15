/*
 * Copyright (c) 2007, Rickard Öberg.
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
using CommonUtils;

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// Sometimes it is required to fine-grain how fragments implementing <see cref="Qi4CS.Core.API.Instance.GenericInvocator"/> are applied onto various composite methods. This interface plays key role in such mechanism.
   /// </summary>
   /// <remarks>TODO what about order of concerns/side-effects? Should ordering be done by this interface too, or use separate one? Or maybe be done exclusively in Bootstrap part?</remarks>
   /// <seealso cref="DefaultAppliesToAttribute"/>
   /// <seealso cref="AppliesToFilterFromFunction"/>
   public interface AppliesToFilter
   {
      /// <summary>
      /// The Qi4CS will invoke this method and use the result when deciding whether apply a generic invocation fragment onto some specific composite method.
      /// </summary>
      /// <param name="compositeMethod">The <see cref="MethodInfo"/> of composite method. Use <see cref="MemberInfo.ReflectedType"/> to get composite type.</param>
      /// <param name="fragmentMethod">The <see cref="MethodInfo"/> of fragment method. Use <see cref="MemberInfo.ReflectedType"/> to get fragment type.</param>
      /// <returns><c>true</c>if <paramref name="fragmentMethod"/> should be applied to <paramref name="compositeMethod"/>; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// TODO Consider adding some way to invoke the attribute mapping event as argument to this method. TODO maybe add some way of letting know what kind of fragment is in question? (mixin/concern/side-effect/constraint)
      /// </remarks>
      /// <seealso cref="AppliesToFilterFromFunction"/>
      Boolean AppliesTo( MethodInfo compositeMethod, MethodInfo fragmentMethod );
   }

   /// <summary>
   /// Convenience class to easily create <see cref="AppliesToFilter"/>s from lambdas.
   /// </summary>
   public sealed class AppliesToFilterFromFunction : AppliesToFilter
   {
      private readonly Func<MethodInfo, MethodInfo, Boolean> _function;

      /// <summary>
      /// Creates a new <see cref="AppliesToFilterFromFunction"/> which will have its <see cref="AppliesToFilter.AppliesTo(MethodInfo, MethodInfo)"/> behave as given <paramref name="function"/>.
      /// </summary>
      /// <param name="function">The lambda to perform logic of <see cref="AppliesToFilter.AppliesTo(MethodInfo, MethodInfo)"/>.</param>
      /// <exception cref="ArgumentValidator">If <paramref name="function"/> is <c>null</c>.</exception>
      public AppliesToFilterFromFunction( Func<MethodInfo, MethodInfo, Boolean> function )
      {
         ArgumentValidator.ValidateNotNull( "Function", function );

         this._function = function;
      }

      #region AppliesToFilter Members

      /// <inheritdoc/>
      public Boolean AppliesTo( MethodInfo compositeMethod, MethodInfo fragmentMethod )
      {
         return this._function( compositeMethod, fragmentMethod );
      }

      #endregion
   }
}
