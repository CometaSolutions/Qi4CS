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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// This enum represents the various fragments in Qi4CS.
   /// </summary>
   public enum FragmentModelType
   {
      /// <summary>
      /// The mixin type provides the actual 'implementation' of composite methods.
      /// </summary>
      Mixin,
      /// <summary>
      /// The concern type provides one step in chain of concerns, usually complex boundary checking or e.g. session handling.
      /// Concerns may modify composite state and may affect the end-result of the method invocation.
      /// </summary>
      Concern,
      /// <summary>
      /// The side effect type provides one item in list of side-effects, usually logging.
      /// Side effects should not modify composite state and can not affect the end-result of the method invocation.
      /// </summary>
      SideEffect,
      /// <summary>
      /// Constraints are special fragments which perform simple boundary checking on method parameters.
      /// Constraints can not access composite state nor modify the method parameters.
      /// </summary>
      Constraint,
   }
}
