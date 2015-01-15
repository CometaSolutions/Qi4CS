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
using System.Collections.Generic;
using System.Linq;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This exception is thrown by <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/> method when no suitable <see cref="CompositeBuilder"/> is found with the composite types provided for the method.
   /// </summary>
   public class NoSuchCompositeTypeException : Exception
   {
      private readonly Type[] _compositeTypes;

      /// <summary>
      /// Creates a new instance of <see cref="NoSuchCompositeTypeException"/>.
      /// This constructor is invoked by Qi4CS runtime, user code does not need to invoke it.
      /// </summary>
      /// <param name="compositeTypes">The composite types that caused this exception.</param>
      public NoSuchCompositeTypeException( IEnumerable<Type> compositeTypes )
         : base( compositeTypes == null || !compositeTypes.Any() ? "No composite type supplied" : ( "No such composite type: " + String.Join( ", ", compositeTypes ) ) )
      {
         this._compositeTypes = compositeTypes.ToArray();
      }

      /// <summary>
      /// Gets the composite types that caused this exception.
      /// </summary>
      /// <value>The composite types that caused this exception.</value>
      public Type[] CompositeTypes
      {
         get
         {
            return this._compositeTypes;
         }
      }
   }
}
