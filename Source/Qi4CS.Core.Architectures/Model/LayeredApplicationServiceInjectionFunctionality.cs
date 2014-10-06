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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Model;
using System.Collections.Generic;

namespace Qi4CS.Core.Architectures.Model
{
   internal class LayeredApplicationServiceInjectionFunctionality : AbstractServiceInjectionFunctionality
   {
      protected override IEnumerable<ServiceCompositeModel> FindSuitableModels( SPI.Model.CompositeModel compositeModel, SPI.Model.AbstractInjectableModel model, Attribute scope, Type targetType, Type serviceType )
      {
         return LayeredApplicationModelUtils.SearchVisibleModels(
             sModel => sModel.ModelType.Equals( CompositeModelType.SERVICE ) && sModel.PublicTypes.Any( pType => serviceType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( pType ) ),
             ( moduleModel, cModel ) => cModel,
             ( (LayeredApplicationModel) compositeModel.ApplicationModel ).FindModuleModel( compositeModel )
            ).Cast<ServiceCompositeModel>();
         //var matchingModel = LayeredApplicationModelUtils.FindFirstVisibleCompositeModel( ( (LayeredApplicationModel) compositeModel.ApplicationModel ).FindModuleModel( compositeModel ), CompositeModelType.SERVICE, serviceType );
         //return new ValidationResult( matchingModel != null, "The service of type " + serviceType + " is either not present or not visible." );
      }
   }
}
