/*
 * Copyright (c) 2008, Rickard Öberg.
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
   /// This is a generic mixin that acts as a "no-op" action.
   /// For methods with return type being value type, this returns the <c>default(&lt;that value type&gt;)</c>.
   /// This mixin is useful if the main functionality is provided by concerns and/or side-effects.
   /// </summary>
   public class NoOpMixin : GenericInvocator
   {
      #region GenericInvocator Members

      /// <inheritdoc/>
      public virtual Object Invoke( Object composite, System.Reflection.MethodInfo method, Object[] args )
      {
         return null;
      }

      #endregion

   }
}
