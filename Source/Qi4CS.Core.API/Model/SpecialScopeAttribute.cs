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
using System;

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// Similarly to <see cref="API.Model.InjectionScopeAttribute"/>, this attribute is used to mark all attributes related to special methods.
   /// </summary>
   /// <remarks>
   /// Special methods in scope of Qi4CS mean any methods, which should be invoked by Qi4CS runtime in certain situation.
   /// If the special method has parameters, all parameters should be injectable.
   /// If the special method has return value, it will be ignored.
   /// Each situation is associated to a certain attribute found in this namespace.
   /// See documentation of each attribute for more information.
   /// </remarks>
   /// <example>
   /// The following example shows how special scope attributes should be used.
   /// <code>
   /// public interface MyComposite
   /// {
   ///   // Domain-specific methods
   ///   ...
   /// }
   /// 
   /// public class MyMixin : MyComposite
   /// {
   ///   // Methods implementing MyComposite
   ///   ...
   ///   
   ///   [Initialize]
   ///   protected virtual void Init([This] MyState state, [Uses] SomeObject obj)
   ///   {
   ///      // This method will be called every time a new instance of MyMixin is created by Qi4CS runtime
   ///      ...
   ///    }
   ///    
   ///   [Prototype]
   ///   protected virtual void CompositeCreated([Service] SomeService service)
   ///   {
   ///     // This method will be called by Qi4CS runtime during composite instantiation
   ///     ...
   ///   }
   /// </code>
   /// </example>
   /// <seealso cref="ActivateAttribute"/>
   /// <seealso cref="InitializeAttribute"/>
   /// <seealso cref="PassivateAttribute"/>
   /// <seealso cref="PrototypeAttribute"/>
   [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
   public sealed class SpecialScopeAttribute : Attribute
   {
      /// <summary>
      /// Helper field to get the attribute target for special method attributes.
      /// </summary>
      public const AttributeTargets SPECIAL_SCOPE_USAGE = AttributeTargets.Method;
   }
}
