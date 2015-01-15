/*
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
using System.Reflection;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.Runtime.Instance
{
   public class GenericPropertyMixin : GenericInvocator
   {
#pragma warning disable 649
      [State]
      private CompositeState _state;
#pragma warning restore 649

      #region GenericInvocator Members

      public virtual Object Invoke( Object composite, MethodInfo method, Object[] args )
      {
         Object result = null;
         CompositeProperty cProp = this._state.Properties[this._state.QualifiedNamesForMethods[method]];
         if ( args.Length == 0 )
         {
            result = cProp.PropertyValueAsObject;
         }
         else
         {
            cProp.PropertyValueAsObject = args[0];
         }
         return result;
      }

      #endregion
   }
}
