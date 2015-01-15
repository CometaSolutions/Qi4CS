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
using System.Linq;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Tests.Core.Instance.Composite.Domain
{
   public interface AbstractObject<out MutableQueryRole, out ImmutableQueryRole> : Mutable<MutableQueryRole, ImmutableQueryRole>
      where MutableQueryRole : AbstractObjectQ<ImmutableQueryRole>
      where ImmutableQueryRole : AbstractObjectIQ
   {
   }

   public interface AbstractObjectQ<out ImmutableQueryRole> : MutableQuery<ImmutableQueryRole>
      where ImmutableQueryRole : AbstractObjectIQ
   {
   }

   public interface Typeable
   {
      Type ImplementedType { get; }
   }

   public interface AbstractObjectIQ : Typeable
   {

   }

   public class AbstractObjectMixin<MutableQueryRole, ImmutableQueryRole> : MutableMixin<MutableQueryRole, ImmutableQueryRole>, AbstractObject<MutableQueryRole, ImmutableQueryRole>
      where MutableQueryRole : AbstractObjectQ<ImmutableQueryRole>
      where ImmutableQueryRole : AbstractObjectIQ
   {
   }

   public class AbstractObjectQMixin<ImmutableQueryRole> : MutableQueryMixin<ImmutableQueryRole>, AbstractObjectQ<ImmutableQueryRole>
      where ImmutableQueryRole : AbstractObjectIQ
   {
   }

   public abstract class AbstractObjectIQMixin : AbstractObjectIQ
   {
#pragma warning disable 649
      [Structure]
      private ApplicationSPI _application;

#pragma warning restore 649

      #region Typeable Members

      public virtual Type ImplementedType
      {
         get
         {
            return this._application.GetCompositeInstance( this ).ModelInfo.Model.PublicTypes.First();
         }
      }

      #endregion
   }
}
