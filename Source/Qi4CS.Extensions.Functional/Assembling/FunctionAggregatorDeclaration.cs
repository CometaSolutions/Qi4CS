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

namespace Qi4CS.Extensions.Functional.Assembling
{
   /// <summary>
   /// This interface exposes methods for setting up Qi4CS services as aggregators for some functionality.
   /// Function aggregator uses some logic to delegate actual execution to other objects, contained within aggregator.
   /// By using methods of this interface, this logic can be preconfigured.
   /// </summary>
   /// <typeparam name="TKey">The type of the key by which objects contained by this aggregator are indexed.</typeparam>
   /// <typeparam name="TComposite">The type which all objects contained by this aggregator should implement.</typeparam>
   /// <example>
   /// <para>
   /// Assume we have the following setup, which gets a string related to some object, depending on type of object:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="FunctionAggregatorDeclarationCode1" language="C#" />
   /// </para>
   /// <para>
   /// Bootstrapping this scenario would happen like this:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="FunctionAggregatorDeclarationCode1" language="C#" />
   /// </para>
   /// <para>
   /// The usage scenario of the above would be something like this:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="FunctionAggregatorDeclarationCode1" language="C#" />
   /// As one can see, function aggregators can be used as a certain kind of API extension to the original API without modifying it.
   /// This example was rather simple and useless, but should demonstrate some basic principles and reasons as motivation for this Qi4CS extension.
   /// </para>
   /// <para>
   /// TODO - examples with more complex stuff
   /// </para>
   /// </example>
   public interface FunctionAggregatorDeclaration<TKey, TComposite>
      where TComposite : class
   {
      /// <summary>
      /// Specifies that both service and aggregated objects implement <typeparamref name="TComposite"/> type.
      /// The <paramref name="functionLookup"/> parameter will be used to extract the keys from method invocation arguments.
      /// All public instance methods of all types in inheritace hierarchy of <typeparamref name="TComposite"/> are considered to be delegator methods.
      /// </summary>
      /// <param name="functionLookup">
      /// Callback to extract keys from method invocation arguments.
      /// The parameter to the callback will be method invocation arguments as object array, and the callback should return the keys (usually only one) to be used to search for aggregated object.
      /// The callback may return <c>null</c>, which is considered to be same as empty enumerable.
      /// </param>
      /// <returns>This <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="functionLookup"/> is <c>null</c>.</exception>
      FunctionAggregatorDeclaration<TKey, TComposite> WithFunctionType(
         Func<Object[], IEnumerable<TKey>> functionLookup
         );

      /// <summary>
      /// Specifies that a specific method should be delegator method.
      /// </summary>
      /// <param name="method">
      /// The method that acts as delegator method.
      /// This method should be available from both service composite and aggregated objects.
      /// </param>
      /// <param name="functionLookup"><inheritdoc cref="FunctionAggregatorDeclaration{TKey, TComposite}.WithFunctionType(Func{Object[], IEnumerable{TKey}})" />
      /// </param>
      /// <param name="funcWhenDirectLookupFails">
      /// In case that the service did not contain aggregated object for any of the keys returned by <paramref name="functionLookup"/>, this callback will be used as special backup callback.
      /// First parameter for the callback will be Qi4CS <see cref="Application"/>.
      /// Second parameter will be result of <paramref name="functionLookup"/>.
      /// Third parameter will be the information about currently aggregated objects: the key of the object, and the <see cref="Lazy{T}"/> of the object.
      /// Fourth parameter will be method invocation arguments
      /// The callback should return result, which is a 3-tuple: the key of aggregated object, the aggregated object, and whether the service should store aggregated object indexed by the key.
      /// </param>
      /// <returns>This <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="method"/> or <paramref name="functionLookup"/> is <c>null</c>.</exception>
      FunctionAggregatorDeclaration<TKey, TComposite> WithFunctionMethod(
         MethodInfo method,
         Func<Object[], IEnumerable<TKey>> functionLookup,
         Func<Application, IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, Lazy<TComposite>>>, Object[], Tuple<TKey, TComposite, Boolean>> funcWhenDirectLookupFails
         //Func<TKey, TComposite, Object[], IEnumerable<Tuple<String, Object>>> additionalRolesFunc
         );

      /// <summary>
      /// Specifies that a specific method from service composite type should be delegator method.
      /// The delegate target is another method, which should be available from aggregated objects.
      /// </summary>
      /// <param name="serviceMethod">The method from service composite type, acting as delegator method.</param>
      /// <param name="functionMethod">The method from aggregated object type, will be called by invoking delegator method.</param>
      /// <param name="functionLookup"><inheritdoc cref="FunctionAggregatorDeclaration{TKey, TComposite}.WithFunctionType(Func{Object[], IEnumerable{TKey}})" /></param>
      /// <param name="preArgTransformer">
      /// Transformation callback to modify method invocation arguments before invoking <paramref name="functionMethod"/>.
      /// The parameter will be original method invocation argumetns, and the callback should return the modified invocation arguments.
      /// If this parameter is <c>null</c>, arguments won't be transformed.
      /// </param>
      /// <param name="postArgTransformer">
      /// A callback to modify the arguments given to <paramref name="serviceMethod"/>.
      /// It only makes sense when <paramref name="preArgTransformer"/> is not null, and when <paramref name="serviceMethod"/> has by-ref parameters.
      /// This callback receives the arguments given to <paramref name="serviceMethod"/> as first parameter and result of <paramref name="preArgTransformer"/> as second parameter.
      /// Typically the callback will assign some array element(s) from the second parameter to some array element(s) in the first parameter.
      /// If this parameter is <c>null</c>, no post-processing is done for arguments.
      /// </param>
      /// <param name="funcWhenDirectLookupFails"><inheritdoc cref="FunctionAggregatorDeclaration{TKey, TComposite}.WithFunctionType(Func{Object[], IEnumerable{TKey}})" /></param>
      /// <returns>This <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="serviceMethod"/>, <paramref name="functionMethod"/> or <paramref name="functionLookup"/> is <c>null</c>.</exception>
      FunctionAggregatorDeclaration<TKey, TComposite> MapServiceMethodToFunctionMethod(
         MethodInfo serviceMethod,
         MethodInfo functionMethod,
         Func<Object[], IEnumerable<TKey>> functionLookup,
         Func<Object[], Object[]> preArgTransformer,
         Action<Object[], Object[]> postArgTransformer,
         Func<Application, IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, Lazy<TComposite>>>, Object[], Tuple<TKey, TComposite, Boolean>> funcWhenDirectLookupFails
         //Func<TKey, TComposite, Object[], IEnumerable<Tuple<String, Object>>> additionalRolesFunc
         );

      /// <summary>
      /// Adds callbacks for creating pre-defined aggregated objects during service activation.
      /// </summary>
      /// <param name="keysAndComposites">
      /// Array of information about callbacks.
      /// The first item is array of keys of the aggregated object.
      /// The second item is callback which creates the aggregated object.
      /// The callback receives the <see cref="StructureServiceProvider"/> related to the <see cref="Assembler"/> of the <see cref="ServiceCompositeAssemblyDeclaration"/> this <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/> was created from.
      /// The callback should return the aggregated object.
      /// </param>
      /// <returns>This <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.</returns>
      /// <remarks>
      /// If <paramref name="keysAndComposites"/> is <c>null</c>, this method does nothing.
      /// If there are any <c>null</c> elements or elements with <c>null</c> for <see cref="Tuple{T,U}.Item2"/> value within <paramref name="keysAndComposites"/> array, they are ignored.
      /// </remarks>
      FunctionAggregatorDeclaration<TKey, TComposite> WithDefaultFunctions( params Tuple<TKey[], Func<StructureServiceProvider, TComposite>>[] keysAndComposites );

      /// <summary>
      /// Adds custom <see cref="IEqualityComparer{T}"/> for the keys used to map aggregated objects.
      /// </summary>
      /// <param name="equalityComparer">
      /// The custom <see cref="IEqualityComparer{T}"/> to use as equality comparer for keys.
      /// Set to <c>null</c> to make Qi4CS use default comparer for <typeparamref name="TKey"/>.
      /// </param>
      /// <returns>This <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.</returns>
      FunctionAggregatorDeclaration<TKey, TComposite> WithEqualityComparer( IEqualityComparer<TKey> equalityComparer );

      /// <summary>
      /// Returns the <see cref="ServiceCompositeAssemblyDeclaration"/> this <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/> was created from.
      /// </summary>
      /// <returns>The <see cref="ServiceCompositeAssemblyDeclaration"/> this <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/> was created from.</returns>
      ServiceCompositeAssemblyDeclaration Done();
   }
}
