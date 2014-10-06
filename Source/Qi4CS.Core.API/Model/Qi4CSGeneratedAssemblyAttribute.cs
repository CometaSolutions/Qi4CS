﻿/*
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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// This attribute is applied to all assemblies generated by Qi4CS.
   /// </summary>
   [AttributeUsage( AttributeTargets.Assembly )]
   public sealed class Qi4CSGeneratedAssemblyAttribute : Attribute
   {
      /// <summary>
      /// When generating an assembly with Qi4CS types and methods, it's name will be <c>"X.Qi4CSAssembly"</c>, where <c>X</c> is the name of the original assembly.
      /// This constant string has value of <c>".Qi4CSAssembly"</c>.
      /// </summary>
      public const String ASSEMBLY_NAME_SUFFIX = ".Qi4CSAssembly";

      /// <summary>
      /// This is helper constant to separate assembly name and start of public key. Use in tandem with <see cref="Qi4CSGeneratedAssemblyAttribute.ASSEMBLY_NAME_SUFFIX"/>.
      /// </summary>
      /// <remarks>The value is <c>", PublicKey="</c>.</remarks>
      public const String ASSEMBLY_PUBLIC_KEY_SUFFIX = ", PublicKey=";

      private const Char SEPARATOR = ',';
      private const Char ESCAPE_CHAR = '\\';
      /// <summary>
      /// Helper method to get the name of the Qi4CS generated assembly which contains types and methods related to original <paramref name="assembly"/>.
      /// </summary>
      /// <param name="assembly">The original assembly.</param>
      /// <returns>The name of the generated assembly, which will be <c>"X.Qi4CSAssembly"</c>, where <c>X</c> is the name of the <paramref name="assembly"/>.</returns>
      public static String GetGeneratedAssemblyName( Assembly assembly )
      {
         var result = assembly.FullName;
         // The ',' might be escaped
         var idx = 0;
         while ( idx < result.Length && result[idx] != SEPARATOR )
         {
            if ( result[idx] == ESCAPE_CHAR )
            {
               ++idx;
            }
            ++idx;
         }
         if ( idx < result.Length )
         {
            result = result.Substring( 0, idx );
         }
         return result + ASSEMBLY_NAME_SUFFIX;
      }
   }
}
