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
using System.Linq;
using Qi4CS.Core.API.Common;

namespace Qi4CS.Core.Runtime.Model
{
   public static class CodeGenerationConstants
   {
      public static readonly Object[] EMPTY_OBJECTS = new Object[0];
   }

   public static class CodeGenerationUtils
   {
      public static Boolean IsPublicTypeMainCompositeType( this Type type )
      {
         return type.GetCustomAttributes( false ).OfType<MainPublicCompositeTypeAttribute>().Any();
      }

      public static Boolean FragmentTypeNeedsPool( this Type type )
      {
         return !type.GetCustomAttributes( false ).OfType<NoPoolNeededAttribute>().Any();
      }

      public static Int32 GetCompositeTypeID( this Type type )
      {
         var attr = type.GetCustomAttributes( false ).OfType<CompositeTypeIDAttribute>().FirstOrDefault();
         if ( attr == null )
         {
            throw new InternalException( "Could not find " + typeof( CompositeTypeIDAttribute ) + " attribute on generated type." );
         }
         return attr.CompositeTypeID;
      }
   }
}
