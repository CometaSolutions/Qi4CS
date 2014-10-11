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
   }
}
