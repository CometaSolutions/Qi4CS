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
using Qi4CS.Core.Bootstrap.Model;

namespace Qi4CS.Core.Bootstrap.Instance
{
   /// <summary>
   /// This is instance-level structural construct of <see cref="Model.LayerModel"/>.
   /// </summary>
   public interface Layer
   {
      /// <summary>
      /// Gets the <see cref="LayeredApplication"/> this <see cref="Layer"/> belongs to.
      /// </summary>
      /// <value>The <see cref="LayeredApplication"/> this <see cref="Layer"/> belongs to.</value>
      LayeredApplication Application { get; }

      /// <summary>
      /// Gets the <see cref="LayerModel"/> of this <see cref="Layer"/>.
      /// </summary>
      /// <value>The <see cref="LayerModel"/> of this <see cref="Layer"/>.</value>
      LayerModel LayerModel { get; }
   }
}
