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
using System.Reflection;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This is common interface for elements composing compostie state, namely <see cref="CompositeProperty"/> and <see cref="CompositeEvent"/>.
   /// </summary>
   /// <typeparam name="TReflectionInfo">The type of the native element, will be <see cref="System.Reflection.PropertyInfo"/> for <see cref="CompositeProperty"/>s, and <see cref="System.Reflection.EventInfo"/> for <see cref="CompositeEvent"/>s.</typeparam>
   public interface CompositeStateParticipant<TReflectionInfo>
      where TReflectionInfo : MemberInfo
   {
      /// <summary>
      /// Gets the <see cref="System.Reflection.PropertyInfo"/> or <see cref="System.Reflection.EventInfo"/> of this property or event, respectively.
      /// </summary>
      /// <value>The <see cref="System.Reflection.PropertyInfo"/> or <see cref="System.Reflection.EventInfo"/> of this property or event, respectively.</value>
      TReflectionInfo ReflectionInfo { get; }

      /// <summary>
      /// Gets the <see cref="QualifiedName"/> of this property or event.
      /// </summary>
      /// <value>The <see cref="QualifiedName"/> of this property of event.</value>
      QualifiedName QualifiedName { get; }

      /// <summary>
      /// This method will try to invoke an action, which will receive the composite property or event field as its <c>ref</c> parameter.
      /// </summary>
      /// <typeparam name="TField">The presumed type of the field containing composite property or event.</typeparam>
      /// <param name="action">The delegate to invoke.</param>
      /// <returns><c>true</c> if composite property of event field was of type <typeparamref name="TField"/> (this implies that the delegate was invoked); <c>false</c> otherwise.</returns>
      /// <remarks>
      /// TODO link to documentation about how field type is deduced (it is not always the same as type of property or event).
      /// </remarks>
      Boolean TryInvokeActionWithRef<TField>( ActionWithRef<TField> action );

      /// <summary>
      /// This method will try to invoke a function, which will receive the composite property or event field as its <c>ref</c> parameter.
      /// </summary>
      /// <typeparam name="TField">The presumed type of the field containing composite property or event.</typeparam>
      /// <param name="function">The delegate to invoke.</param>
      /// <param name="result">If this method returns <c>true</c>, this parameter will contain the return value of <paramref name="function"/>.</param>
      /// <returns><c>true</c> if composite property of event field was of type <typeparamref name="TField"/> (this implies that the delegate was invoked); <c>false</c> otherwise.</returns>
      /// <remarks>
      /// TODO link to documentation about how field type is deduced (it is not always the same as type of property or event).
      /// </remarks>
      Boolean TryInvokeFunctionWithRef<TField>( FunctionWithRef<TField> function, out TField result );
   }

   /// <summary>
   /// This delegate is used in <see cref="CompositeStateParticipant{T}"/> to obtain a <c>ref</c> reference to the field containing composite state participant (property or event).
   /// </summary>
   /// <typeparam name="T">The type of parameter.</typeparam>
   /// <param name="parameter">The parameter.</param>
   public delegate void ActionWithRef<T>( ref T parameter );

   /// <summary>
   /// This delegate is used in <see cref="CompositeStateParticipant{T}"/> to obtain a <c>ref</c> reference to the field containing composite state participant (property or event).
   /// </summary>
   /// <typeparam name="T">The type of the parameter.</typeparam>
   /// <param name="parameter">The parameter.</param>
   /// <returns>The result of function.</returns>
   public delegate T FunctionWithRef<T>( ref T parameter );

}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Helper method to invoke <see cref="CompositeStateParticipant{T}.TryInvokeActionWithRef"/> and throw an exception if it returns <c>false</c>.
   /// </summary>
   /// <typeparam name="TReflectionInfo">The kind of composite state participant, <see cref="PropertyInfo"/> or <see cref="EventInfo"/>.</typeparam>
   /// <typeparam name="TField">The presumed type of the field containing composite property or event.</typeparam>
   /// <param name="stateParticipant">The <see cref="CompositeStateParticipant{T}"/>.</param>
   /// <param name="action">The delegate to invoke.</param>
   /// <exception cref="InvalidOperationException">If <see cref="CompositeStateParticipant{T}.TryInvokeActionWithRef"/> returns <c>false</c>, that is, when the field type does not match <typeparamref name="TField"/>.</exception>
   /// <remarks>
   /// TODO link to documentation about how field type is deduced (it is not always the same as type of property or event).
   /// </remarks>
   /// <seealso cref="CompositeStateParticipant{T}.TryInvokeActionWithRef"/>
   public static void InvokeActionWithRef<TReflectionInfo, TField>( this CompositeStateParticipant<TReflectionInfo> stateParticipant, ActionWithRef<TField> action )
      where TReflectionInfo : MemberInfo
   {
      if ( !stateParticipant.TryInvokeActionWithRef( action ) )
      {
         ThrowUnmatchedStateParticipantFieldType<TReflectionInfo, TField>( stateParticipant );
      }
   }

   /// <summary>
   /// Helper method to invoke <see cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/> and throw an exception if it returns <c>false</c>.
   /// </summary>
   /// <typeparam name="TReflectionInfo">The kind of composite state participant, <see cref="PropertyInfo"/> or <see cref="EventInfo"/>.</typeparam>
   /// <typeparam name="TField">The presumed type of the field containing composite property or event.</typeparam>
   /// <param name="stateParticipant">The <see cref="CompositeStateParticipant{T}"/>.</param>
   /// <param name="function">The delegate to invoke.</param>
   /// <returns>The return value of of <paramref name="function"/>.</returns>
   /// <exception cref="InvalidOperationException">If <see cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/> returns <c>false</c>, that is, when the field type does not match <typeparamref name="TField"/>.</exception>
   /// <remarks>
   /// TODO link to documentation about how field type is deduced (it is not always the same as type of property or event).
   /// </remarks>
   /// <seealso cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/>
   public static TField InvokeFunctionWithRef<TReflectionInfo, TField>( this CompositeStateParticipant<TReflectionInfo> stateParticipant, FunctionWithRef<TField> function )
      where TReflectionInfo : MemberInfo
   {
      TField result;
      if ( !stateParticipant.TryInvokeFunctionWithRef( function, out result ) )
      {
         ThrowUnmatchedStateParticipantFieldType<TReflectionInfo, TField>( stateParticipant );
      }
      return result;
   }

   /// <summary>
   /// Invokes the <see cref="CompositeStateParticipant{T}.TryInvokeActionWithRef"/> method with given delegate, assuming that field type is same as property type.
   /// </summary>
   /// <typeparam name="TProperty">The type of the property.</typeparam>
   /// <param name="property">The <see cref="CompositeProperty{T}"/>.</param>
   /// <param name="action">The delegate to invoke.</param>
   /// <exception cref="InvalidOperationException">If <see cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/> returns <c>false</c>, that is, when the field type does not match <typeparamref name="TProperty"/>.</exception>
   /// <remarks>
   /// TODO link to documentation about how field type is deduced (it is not always the same as type of property or event).
   /// </remarks>
   /// <seealso cref="CompositeStateParticipant{T}.TryInvokeActionWithRef"/>
   public static void InvokeActionWithRefSameType<TProperty>( this CompositeProperty<TProperty> property, ActionWithRef<TProperty> action )
   {
      property.InvokeActionWithRef( action );
   }

   /// <summary>
   /// Invokes the <see cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/> method with given delegate, assuming that field type is same as property type.
   /// </summary>
   /// <typeparam name="TProperty">The type of the property.</typeparam>
   /// <param name="property">The <see cref="CompositeProperty{T}"/>.</param>
   /// <param name="function">The delegate to invoke.</param>
   /// <returns>The return value of <paramref name="function"/>.</returns>
   /// <exception cref="InvalidOperationException">If <see cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/> returns <c>false</c>, that is, when the field type does not match <typeparamref name="TProperty"/>.</exception>
   /// <remarks>
   /// TODO link to documentation about how field type is deduced (it is not always the same as type of property or event).
   /// </remarks>
   /// <seealso cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/>
   public static TProperty InvokeFunctionWithRefSameType<TProperty>( this CompositeProperty<TProperty> property, FunctionWithRef<TProperty> function )
   {
      return property.InvokeFunctionWithRef( function );
   }

   /// <summary>
   /// Invokes the <see cref="CompositeStateParticipant{T}.TryInvokeActionWithRef"/> method with given delegate, assuming that field type is same as event type.
   /// </summary>
   /// <typeparam name="TEvent">The type of the property.</typeparam>
   /// <param name="evt">The <see cref="CompositeEvent{T}"/>.</param>
   /// <param name="action">The delegate to invoke.</param>
   /// <exception cref="InvalidOperationException">If <see cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/> returns <c>false</c>, that is, when the field type does not match <typeparamref name="TEvent"/>.</exception>
   /// <remarks>
   /// TODO link to documentation about how field type is deduced (it is not always the same as type of property or event).
   /// </remarks>
   /// <seealso cref="CompositeStateParticipant{T}.TryInvokeActionWithRef"/>
   public static void InvokeActionWithRefSameType<TEvent>( this CompositeEvent<TEvent> evt, ActionWithRef<TEvent> action )
      where TEvent : class
   {
      evt.InvokeActionWithRef( action );
   }

   /// <summary>
   /// Invokes the <see cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/> method with given delegate, assuming that field type is same as event type.
   /// </summary>
   /// <typeparam name="TEvent">The type of the property.</typeparam>
   /// <param name="evt">The <see cref="CompositeEvent{T}"/>.</param>
   /// <param name="function">The delegate to invoke.</param>
   /// <exception cref="InvalidOperationException">If <see cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/> returns <c>false</c>, that is, when the field type does not match <typeparamref name="TEvent"/>.</exception>
   /// <remarks>
   /// TODO link to documentation about how field type is deduced (it is not always the same as type of property or event).
   /// </remarks>
   /// <seealso cref="CompositeStateParticipant{T}.TryInvokeFunctionWithRef"/>
   public static TEvent InvokeFunctionWithRefSameType<TEvent>( this CompositeEvent<TEvent> evt, FunctionWithRef<TEvent> function )
      where TEvent : class
   {
      return evt.InvokeFunctionWithRef( function );
   }

   private static void ThrowUnmatchedStateParticipantFieldType<TReflectionInfo, TField>( CompositeStateParticipant<TReflectionInfo> stateParticipant )
      where TReflectionInfo : MemberInfo
   {
      throw new InvalidOperationException( "The composite state participant " + stateParticipant + " does not use field of type " + typeof( TField ) + "." );
   }
}
