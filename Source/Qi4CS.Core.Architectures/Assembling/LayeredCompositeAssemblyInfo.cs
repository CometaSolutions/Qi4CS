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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.SPI.Common;

namespace Qi4CS.Core.Architectures.Assembling
{
   internal interface LayeredCompositeAssemblyInfo : CompositeAssemblyInfo
   {
      Visibility Visibility { get; set; }
   }

   internal class LayeredCompositeAssemblyInfoImpl : CompositeAssemblyInfoImpl, LayeredCompositeAssemblyInfo
   {
      public const Visibility DEFAULT_VISIBILITY = Visibility.MODULE;

      private Visibility _visibility;

      internal LayeredCompositeAssemblyInfoImpl( Int32 id, CompositeModelType modelType, UsesContainerQuery parentContainer )
         : base( id, modelType, parentContainer )
      {
         this._visibility = DEFAULT_VISIBILITY;
      }

      #region LayeredCompositeAssemblyInfo Members

      public Visibility Visibility
      {
         get
         {
            return this._visibility;
         }
         set
         {
            this._visibility = value;
         }
      }

      #endregion
   }
}
