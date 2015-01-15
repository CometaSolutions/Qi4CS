/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
using System.Reflection;
using System.Text;

namespace Qi4CS.Core.Runtime.Instance
{
   public static class ReflectionHelper
   {
      public static PropertyInfo GetDeclaredInstanceProperty( Type type, String name )
      {
         return
#if WINDOWS_PHONE_APP
            type.GetTypeInfo().DeclaredProperties.FirstOrDefault( p => String.Equals( p.Name, name) && !p.GetSomeMethod().IsStatic )
#else
 type.GetProperty( name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
#endif
;
      }

      public static EventInfo GetDeclaredInstanceEvent( Type type, String name )
      {
         return
#if WINDOWS_PHONE_APP
            type.GetTypeInfo().DeclaredEvents.FirstOrDefault(e => String.Equals( e.Name, name) && !e.GetSomeMethod().IsStatic )
#else
 type.GetEvent( name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
#endif
;
      }

      public static ConstructorInfo GetFirstInstanceConstructor( Type type )
      {
         return
#if WINDOWS_PHONE_APP
            type.GetTypeInfo().DeclaredConstructors.First(c => !c.IsStatic)
#else
 type.GetConstructors( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )[0]
#endif
;
      }


      /// <summary>
      /// Helper field to get access of Qi4CS assembly.
      /// </summary>
      public static readonly Assembly QI4CS_ASSEMBLY = typeof(ReflectionHelper)
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
.Assembly;

      /// <summary>
      /// Finds a method from <paramref name="classType"/> and its base type hierarchy which could be seen as implementing the <paramref name="methodFromParent"/>.
      /// This means that the declaring type of <paramref name="methodFromParent"/> is <paramref name="classType"/> or its parent type (generic arguments, if any, not considered), and method signature matches.
      /// </summary>
      /// <param name="classType">The type containing implementation of method.</param>
      /// <param name="methodFromParent">The method of some parent type of <paramref name="classType"/>.</param>
      /// <returns>The suitable method of <paramref name="classType"/>, or <c>null</c> if no such method could be found. Also returns <c>null</c> if either of <paramref name="classType"/> or <paramref name="methodFromParent"/> is <c>null</c>.</returns>
      public static MethodInfo FindMethodImplicitlyImplementingMethod(Type classType, MethodInfo methodFromParent)
      {
         // TODO optimize: if classType not interface and methodFromInterface.DeclaringType is not interface, and methodFromInterface.DeclaringType is not generic, then it is enough to just call classType.GetMethod with suitable parameters.
         MethodInfo result = null;
         if (classType != null && methodFromParent != null)
         {
            foreach (var suitableType in classType.GetAllParentTypes().Where(t => AreStructurallySame(t, methodFromParent.DeclaringType, true)))
            {
#if WINDOWS_PHONE_APP
               var newMethodsFromInterface = suitableType.GetAllDeclaredInstanceMethods().Where(m => String.Equals(m.Name, methodFromParent.Name));
#else
               var newMethodFromInterface = (MethodInfo)MethodBase.GetMethodFromHandle(methodFromParent.MethodHandle, suitableType.TypeHandle);
#endif
               result = classType
                  .GetClassHierarchy()
                  .SelectMany(t => t.GetAllDeclaredInstanceMethods())
                  .FirstOrDefault(method =>
                  {
#if WINDOWS_PHONE_APP
                     return newMethodsFromInterface.Any(newMethodFromInterface =>
                  {
#endif
                     var p1 = newMethodFromInterface.GetParameters();
                     var p2 = method.GetParameters();
                     return
#if !WINDOWS_PHONE_APP
                        String.Equals(newMethodFromInterface.Name, method.Name) &&
#endif
                        newMethodFromInterface.GetGenericArguments().Length == method.GetGenericArguments().Length
                        && p1.Length == p2.Length
                        && AreStructurallySame(method.ReturnParameter.ParameterType, newMethodFromInterface.ReturnParameter.ParameterType, false)
                        && p1.TakeWhile((param, idx) => AreStructurallySame(p2[idx].ParameterType, param.ParameterType, false)).Count() == p1.Length;
#if WINDOWS_PHONE_APP
                  });
#endif
                  });

               if (result != null)
               {
                  break;
               }
            }
         }
         return result;
      }

      /// <summary>
      /// Checks whether two types are structurally same.
      /// </summary>
      /// <param name="x">First type.</param>
      /// <param name="y">Second type.</param>
      /// <param name="comparingBaseTypes">Whether generic parameter position matters.</param>
      /// <returns><c>true</c> if two types are structurally the same; <c>false</c> otherwise.</returns>
      /// <remarks>
      /// TODO explain better what 'structurally same' means.
      /// </remarks>
      public static Boolean AreStructurallySame(Type x, Type y, Boolean comparingBaseTypes)
      {
         var result = Object.ReferenceEquals(x, y);
         if (!result)
         {
            if (x.IsGenericParameter && y.IsGenericParameter)
            {
               result = comparingBaseTypes || (x
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
.DeclaringMethod != null && y
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
.DeclaringMethod != null && x.GenericParameterPosition == y.GenericParameterPosition);
            }
            else if (x.IsGenericType() && y.IsGenericType())
            {
               var cGArgs = x.GetGenericArguments();
               var iGArgs = y.GetGenericArguments();
               result = Object.ReferenceEquals(x.GetGenericTypeDefinition(), y.GetGenericTypeDefinition())
                  && iGArgs.TakeWhile((arg, index) => AreStructurallySame(cGArgs[index], arg, comparingBaseTypes)).Count() == iGArgs.Length;
            }
            else if (x.HasElementType && y.HasElementType)
            {
               if (x.IsArray && y.IsArray)
               {
                  result = x.GetArrayRank() == y.GetArrayRank();
               }
               if (comparingBaseTypes || result || !x.IsArray)
               {
                  result = (comparingBaseTypes || (x.IsArray == y.IsArray
                     && x.IsByRef == y.IsByRef
                     && x.IsPointer == y.IsPointer))
                     && AreStructurallySame(x.GetElementType(), y.GetElementType(), comparingBaseTypes);
               }
            }
         }
         return result;
      }
   }
}
