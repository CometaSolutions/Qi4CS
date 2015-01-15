/*
 * Copyright (c) 2009, Rickard Öberg.
 * See NOTICE file.
 * 
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
   /// This exception is thrown by <see cref="Application.Passivate()"/> method if any exceptions occur in event handlers during passivation.
   /// </summary>
   public sealed class ApplicationPassivationException : AggregateException
   {
      /// <summary>
      /// Creates new instance of <see cref="ApplicationPassivationException"/> with given errors.
      /// </summary>
      /// <param name="exceptions">The errors that occurred.</param>
      /// <remarks>
      /// The Qi4CS will create instances of this class, user code does not need to use this.
      /// </remarks>
      public ApplicationPassivationException( Exception[] exceptions )
         : base( "Exceptions occurred during application passivation.", exceptions )
      {
      }
   }
}
