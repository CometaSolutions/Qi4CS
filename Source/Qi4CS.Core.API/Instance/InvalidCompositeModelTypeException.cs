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

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This exception is thrown by <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, System.Collections.Generic.IEnumerable{Type})"/> method when <see cref="CompositeBuilder"/> for specified <see cref="CompositeModelType"/> could not be created.
   /// </summary>
   public class InvalidCompositeModelTypeException : Exception
   {
      /// <summary>
      /// Creates a new instance of <see cref="InvalidCompositeModelTypeException"/>.
      /// This constructor is invoked by Qi4CS runtime, user code does not need to invoke it.
      /// </summary>
      /// <param name="modelType">The composite model type.</param>
      /// <param name="msg">The case-specific error message.</param>
      public InvalidCompositeModelTypeException( CompositeModelType modelType, String msg )
         : base( "Invalid composite model type: " + modelType + "." + ( ( msg == null || msg.Trim().Length == 0 ) ? "" : ( " " + msg ) ) )
      {

      }
   }
}
