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
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Tests.Core.Instance.Composite.Domain
{
   public interface ObjectsFactory
   {
      Int32Object NewInt32Object();
      Int32Object NewInt32Object( [Optional]Int32? value );
   }

   public class ObjectsFactoryMixin : ObjectsFactory
   {
#pragma warning disable 649
      [Structure]
      private StructureServiceProvider _structureServices;
#pragma warning restore 649

      #region ObjectsFactory Members

      public virtual Int32Object NewInt32Object()
      {
         return this.NewInt32Object( null );
      }

      public virtual Int32Object NewInt32Object( Int32? value )
      {
         var builder = this._structureServices.NewPlainCompositeBuilder<Int32Object>();
         builder.Builder.Prototype<LeafObjectState<Int32?>>().Value = value;
         return builder.Instantiate();
      }

      #endregion
   }
}
