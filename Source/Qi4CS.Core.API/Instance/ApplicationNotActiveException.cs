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
   /// This exception is thrown by Qi4CS runtime whenever there is attempt to access a Qi4CS composite belonging to application which is passive.
   /// </summary>
   public sealed class ApplicationNotActiveException : Exception
   {
      /// <summary>
      /// Creates a new instance of <see cref="ApplicationNotActiveException"/>.
      /// </summary>
      /// <remarks>
      /// The Qi4CS will create instances of this class, user code does not need to use this.
      /// </remarks>
      public ApplicationNotActiveException()
         : base( "Using composites is not supported when application is passive." )
      {

      }
   }
}
