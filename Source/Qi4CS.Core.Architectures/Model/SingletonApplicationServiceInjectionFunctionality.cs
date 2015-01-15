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
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Model;
using System.Collections.Generic;

namespace Qi4CS.Core.Architectures.Model
{
   internal class SingletonApplicationServiceInjectionFunctionality : AbstractServiceInjectionFunctionality
   {
      protected override IEnumerable<ServiceCompositeModel> FindSuitableModels( CompositeModel compositeModel, AbstractInjectableModel model, Attribute scope, Type targetType, Type serviceType )
      {
         return ( (ModelContainer) compositeModel.ApplicationModel ).CompositeModelsInThisContainer.Where( cModel => cModel.PublicTypes.Any( pType => serviceType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( pType ) ) )
            .Cast<ServiceCompositeModel>();
         //return new ValidationResult(
         //   ( (ModelContainer) compositeModel.ApplicationModel ).CompositeModelsInThisContainer.Any( cModel => cModel.PublicTypes.Any( pType => TypeUtil.IsAssignableFrom( serviceType, pType ) ) ),
         //   "The service of type " + serviceType + " is not added to application."
         //   );
      }
   }
}
