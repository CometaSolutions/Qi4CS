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
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Extensions.Functional.Assembling;
using Qi4CS.Extensions.Functional.Model;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Common;

namespace Qi4CS.Extensions.Functional.Assembling
{
   /// <summary>
   /// Utility class containing some commonly used things related to assembling function aggregator services. 
   /// </summary>
   public static class FunctionAssemblerUtils
   {
      private static Object[] EMPTY = new Object[0];

      /// <summary>
      /// Returns a callback to extract types from method invocation argument at given zero-based index.
      /// The type extraction logic will use <see cref="GetTypesOfTypeableOrComposite{TTypeable}(Object, Func{TTypeable, Type})"/> method to extract types.
      /// </summary>
      /// <typeparam name="TTypeable">The type of interface which may provide custom type extraction way.</typeparam>
      /// <param name="argIndex">The zero-based index of method parameter to use to extract types.</param>
      /// <param name="typeExtractor">
      /// The custom type extractor callback for parameters which are of type <typeparamref name="TTypeable"/>.
      /// It will receive the method invocation parameter at given index and correctly typed, and should return the <see cref="Type"/> associcated with that parameter.
      /// </param>
      /// <returns>
      /// A callback to extract types from method invocation argument at given index.
      /// This callback may be used in methods of <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.
      /// </returns>
      /// <seealso cref="GetTypesOfTypeableOrComposite{TTypeable}(Object, Func{TTypeable, Type})"/>
      public static Func<Object[], IEnumerable<Type>> TypeBasedFunctionLookUp<TTypeable>( Int32 argIndex, Func<TTypeable, Type> typeExtractor )
         where TTypeable : class
      {
         return args => GetTypesOfTypeableOrComposite<TTypeable>( args[argIndex], typeExtractor );
      }

      /// <summary>
      /// Returns a callback to extract types from method invocation argument at given zero-based index.
      /// The type extraction logic will use <see cref="GetTypesOfObjectOrComposite(Object)"/> method to extract types.
      /// </summary>
      /// <param name="argIndex">The zero-based index of method parameter to use to extract types.</param>
      /// <returns>
      /// A callback to extract types from method invocation argument at given index.
      /// This callback may be used in methods of <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.
      /// </returns>
      /// <seealso cref="GetTypesOfObjectOrComposite(Object)"/>
      public static Func<Object[], IEnumerable<Type>> TypeBasedFunctionLookUp( Int32 argIndex )
      {
         return args => GetTypesOfObjectOrComposite( args[argIndex] );
      }

      /// <summary>
      /// Returns a callback to extract types and all types in the inheritance hierarchy of those types from method invocation argument at given zero-based index.
      /// The type extraction logic will use <see cref="GetTypesOfTypeableOrComposite{TTypeable}(Object, Func{TTypeable, Type})"/> method to extract types, and then for each type, invoke <see cref="E_CommonUtils.GetAllParentTypes"/> method and return all resulting types.
      /// </summary>
      /// <typeparam name="TTypeable"><inheritdoc cref="TypeBasedFunctionLookUp{T}(Int32, Func{T,Type})"/></typeparam>
      /// <param name="argIndex"><inheritdoc cref="TypeBasedFunctionLookUp{T}(Int32, Func{T,Type})"/></param>
      /// <param name="typeExtractor"><inheritdoc cref="TypeBasedFunctionLookUp{T}(Int32, Func{T,Type})"/></param>
      /// <returns>
      /// A callback to extract types and all types in the inheritance hierarchy of those type from method from method invocation argument at given index.
      /// This callback may be used in methods of <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.
      /// </returns>
      /// <seealso cref="GetTypesOfTypeableOrComposite{TTypeable}(Object, Func{TTypeable, Type})"/>
      public static Func<Object[], IEnumerable<Type>> TypeBasedFunctionLookUpAllTypes<TTypeable>( Int32 argIndex, Func<TTypeable, Type> typeExtractor )
         where TTypeable : class
      {
         return args =>
         {
            IEnumerable<Type> result = null;
            Object arg = args[argIndex];
            if ( arg != null )
            {
               result = GetTypesOfTypeableOrComposite<TTypeable>( arg, typeExtractor ).SelectMany( t => t.GetAllParentTypes() );
            }
            return result;
         };
      }

      /// <summary>
      /// Returns a callback to extract types and all types in the inheritance hierarchy of those types from method invocation argument at given zero-based index.
      /// The type extraction logic will use <see cref="GetTypesOfObjectOrComposite(Object)"/> method to extract types, and then for each type, invoke <see cref="E_CommonUtils.GetAllParentTypes"/> method and return all resulting types.
      /// </summary>
      /// <param name="argIndex"><inheritdoc cref="TypeBasedFunctionLookUp(Int32)" /></param>
      /// <returns>
      /// A callback to extract types and all types in the inheritance hierarchy of those type from method from method invocation argument at given index.
      /// This callback may be used in methods of <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.
      /// </returns>
      /// <seealso cref="GetTypesOfObjectOrComposite(Object)"/>
      public static Func<Object[], IEnumerable<Type>> TypeBasedFunctionLookUpAllTypes( Int32 argIndex )
      {
         return args =>
         {
            IEnumerable<Type> result = null;
            Object arg = args[argIndex];
            if ( arg != null )
            {
               result = GetTypesOfObjectOrComposite( arg ).SelectMany( t => t.GetAllParentTypes() );
            }
            return result;
         };
      }

      /// <summary>
      /// This method will extract types of <paramref name="obj"/> either through <typeparamref name="TTypeable"/> interface, if <paramref name="obj"/> is instance of <typeparamref name="TTypeable"/>, or by using <see cref="GetTypesOfObjectOrComposite(Object)"/> method if it is not.
      /// </summary>
      /// <typeparam name="TTypeable">The interface providing some way to query the type of itself.</typeparam>
      /// <param name="obj">The object to extract types from. If <c>null</c>, empty enumerable will be returned.</param>
      /// <param name="typeableExtractor">
      /// The callback to extract type if <paramref name="obj"/> is not <c>null</c> and instance of <typeparamref name="TTypeable"/>.
      /// It will receive <paramref name="obj"/> as parameter and should return <see cref="Type"/> associated with it.
      /// </param>
      /// <returns>Types of <paramref name="obj"/>, either by using <typeparamref name="TTypeable"/> and <paramref name="typeableExtractor"/>, or by <see cref="GetTypesOfObjectOrComposite"/> method.</returns>
      public static IEnumerable<Type> GetTypesOfTypeableOrComposite<TTypeable>( Object obj, Func<TTypeable, Type> typeableExtractor )
         where TTypeable : class
      {
         IEnumerable<Type> result;
         var typeable = obj as TTypeable;
         if ( typeable != null )
         {
            result = typeableExtractor( typeable ).Singleton();
         }
         else
         {
            result = GetTypesOfObjectOrComposite( obj );
         }
         return result;
      }

      /// <summary>
      /// This method will extract types of <paramref name="obj"/> either by using logic to extract types of Qi4CS composite or fragment, if <paramref name="obj"/> is a Qi4CS composite or fragment, or via <see cref="Object.GetType"/> method if <paramref name="obj"/> is not Qi4CS composite or fragment.
      /// </summary>
      /// <param name="obj">
      /// The object to extract types from.
      /// If <c>null</c>, empty enumerable will be returned.
      /// </param>
      /// <returns>
      /// Types of <paramref name="obj"/>, either by using logic to extract types of Qi4CS composite or fragment, or by using <see cref="Object.GetType"/> method.
      /// </returns>
      public static IEnumerable<Type> GetTypesOfObjectOrComposite( Object obj )
      {
         IEnumerable<Type> result;
         if ( obj == null )
         {
            result = Enumerable.Empty<Type>();
         }
         else
         {
            var type = obj.GetType();
            if ( type.Assembly.GetCustomAttributes( true ).OfType<Qi4CSGeneratedAssemblyAttribute>().Any() )
            {
               if ( typeof( Object ).Equals( type.BaseType ) )
               {
                  result = type.GetImplementedInterfaces().GetBottomTypes();
               }
               else
               {
                  result = type.BaseType.Singleton();
               }
            }
            else
            {
               result = type.Singleton();
            }
         }
         return result;
      }

      /// <summary>
      /// Helper field of callback, which will return empty array from any given array argument.
      /// Can be used as parameter for certain methods of <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>.
      /// </summary>
      public static Func<Object[], Object[]> ARGS_TO_EMPTY = args => EMPTY;
   }
}

/// <summary>
/// This class contains extension method to obtain <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/> from <see cref="ServiceCompositeAssemblyDeclaration"/>.
/// </summary>
public static class E_Qi4CS_Functional
{
   /// <summary>
   /// Specifies that this <see cref="ServiceCompositeAssemblyDeclaration"/> should act as aggregator service.
   /// </summary>
   /// <typeparam name="TKey">The type of the keys by which aggregated objects are indexed.</typeparam>
   /// <typeparam name="TComposite">The type that aggregated objects should implement.</typeparam>
   /// <param name="assemblyDeclaration">The <see cref="ServiceCompositeAssemblyDeclaration"/>.</param>
   /// <returns>A <see cref="FunctionAggregatorDeclaration{TKey, TComposite}"/> which may be used to further configure aggregator service.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="assemblyDeclaration"/> is <c>null</c>.</exception>
   /// <seealso cref="FunctionAggregatorDeclaration{TKey, TComposite}"/>
   public static FunctionAggregatorDeclaration<TKey, TComposite> AsFunctionAggregator<TKey, TComposite>( this ServiceCompositeAssemblyDeclaration assemblyDeclaration )
      where TComposite : class
   {
      var dic = assemblyDeclaration.Assembler.ApplicationArchitecture.AdditionalInjectionFunctionalities;
      dic[RoleInjectionFunctionality.SCOPE] = RoleInjectionFunctionality.INSTANCE;
      dic[FunctionInvocationDataInjectionFunctionality.SCOPE] = FunctionInvocationDataInjectionFunctionality.INSTANCE;
      return new FunctionAggregatorDeclarationImpl<TKey, TComposite>( assemblyDeclaration );
   }
}
