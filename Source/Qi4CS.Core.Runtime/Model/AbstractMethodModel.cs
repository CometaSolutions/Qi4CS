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
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public abstract class AbstractMethodModelMutable : AbstractMemberInfoModelMutable<MethodInfo>
   {
#pragma warning disable 414
      private readonly AbstractMethodModelState _state;
#pragma warning restore 414

      public AbstractMethodModelMutable( AbstractMethodModelState state, AbstractMethodModel immutable )
         : base( state, immutable )
      {
         this._state = state;
      }
   }

   public abstract class AbstractMethodModelImmutable : AbstractMemberInfoModelImmutable<MethodInfo>, AbstractMethodModel
   {
#pragma warning disable 414
      private readonly AbstractMethodModelState _state;
#pragma warning restore 414

      public AbstractMethodModelImmutable( AbstractMethodModelState state )
         : base( state )
      {
         this._state = state;
      }
   }

   public class AbstractMethodModelState : AbstractMemberInfoModelState<MethodInfo>
   {
      public override String ToString()
      {
         String result = null;
         if ( this.NativeInfo != null )
         {
            result = this.NativeInfo.DeclaringType + "." + this.NativeInfo.Name;
         }
         else
         {
            result = base.ToString();
         }
         return result;
      }
   }
}
