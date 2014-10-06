/*
 * Copyright 2013 Stanislav Muhametsin. All rights Reserved.
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
using Qi4CS.Core.Architectures.Assembling;
using Qi4CS.Core.Architectures.Instance;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.Runtime.Model;

namespace Qi4CS.Core.Architectures.Model
{
   internal sealed class LayeredPlainCompositeModelTypeModelScopeSupport : AbstractPlainCompositeModelTypeModelScopeSupport
   {
      public LayeredPlainCompositeModelTypeModelScopeSupport( LayeredPlainCompositeModelTypeAssemblyScopeSupport assemblyScopeSupport )
         : base( assemblyScopeSupport )
      {
      }

      public override CompositeModelTypeInstanceScopeSupport CreateInstanceScopeSupport()
      {
         return new LayeredPlainCompositeModelTypeInstanceScopeSupport( this );
      }
   }

   internal sealed class LayeredServiceModelTypeModelScopeSupport : AbstractServiceModelTypeModelScopeSupport
   {
      public LayeredServiceModelTypeModelScopeSupport( LayeredServiceModelTypeAssemblyScopeSupport assemblyScopeSupport )
         : base( assemblyScopeSupport )
      {

      }

      public override CompositeModelTypeInstanceScopeSupport CreateInstanceScopeSupport()
      {
         return new LayeredServiceModelTypeInstanceScopeSupport( this );
      }
   }

   //internal sealed class LayeredValueModelTypeModelScopeSupport : AbstractValueModelTypeModelScopeSupport
   //{
   //   public LayeredValueModelTypeModelScopeSupport( LayeredValueModelTypeAssemblyScopeSupport assemblyScopeSupport )
   //      : base( assemblyScopeSupport )
   //   {

   //   }

   //   public override CompositeModelTypeInstanceScopeSupport CreateInstanceScopeSupport()
   //   {
   //      return new LayeredValueModelTypeInstanceScopeSupport( this );
   //   }
   //}
}
