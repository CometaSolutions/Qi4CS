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
using System.Collections.Generic;
using System.Linq;
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Instance
{
   public class StructureServiceProviderImpl : StructureServiceProviderSPI
   {
      private readonly CompositeInstanceStructureOwner _structure;
      private readonly DictionaryQuery<CompositeModelType, CompositeModelTypeInstanceScopeSupport> _modelSupport;
      private readonly Func<Func<CompositeModel, Boolean>, IEnumerable<Tuple<CompositeInstanceStructureOwner, CompositeModel>>> _modelFinder;

      public StructureServiceProviderImpl(
         CompositeInstanceStructureOwner structure,
         DictionaryQuery<CompositeModelType, CompositeModelTypeInstanceScopeSupport> modelSupport,
         Func<Func<CompositeModel, Boolean>, IEnumerable<Tuple<CompositeInstanceStructureOwner, CompositeModel>>> serviceFinderFunction
         )
      {
         ArgumentValidator.ValidateNotNull( "Structure", structure );
         ArgumentValidator.ValidateNotNull( "Model type support", modelSupport );
         ArgumentValidator.ValidateNotNull( "Service finder function", serviceFinderFunction );

         this._structure = structure;
         this._modelSupport = modelSupport;
         this._modelFinder = serviceFinderFunction;
      }

      #region StructureServiceProvider Members

      public Application Application
      {
         get
         {
            return this._structure.Application;
         }
      }

      public CompositeBuilder NewCompositeBuilder( CompositeModelType compositeModelType, IEnumerable<Type> compositeTypes )
      {
         ArgumentValidator.ValidateNotNull( "Composite model type", compositeModelType );
         ArgumentValidator.ValidateNotNull( "Composite types", compositeTypes );

         CompositeModelTypeInstanceScopeSupport support;
         if ( this._modelSupport.TryGetValue( compositeModelType, out support ) )
         {
            return support.CreateBuilder( this, compositeTypes.ToArray() );
         }
         else
         {
            throw new InvalidCompositeModelTypeException( compositeModelType, "This application does not support this composite model type." );
         }
      }

      public IEnumerable<ServiceReference> FindServices( IEnumerable<Type> serviceTypes )
      {
         ArgumentValidator.ValidateNotNull( "Service types", serviceTypes );
         var serviceTypesArr = serviceTypes.ToArray();
         AbstractModelTypeInstanceScopeSupport.ThrowIfGenericParams( serviceTypesArr );
         return this.FindServicesImpl( serviceTypesArr );
      }

      #endregion


      #region StructureServiceProviderSPI Members

      public CompositeInstanceStructureOwner Structure
      {
         get
         {
            return this._structure;
         }
      }

      #endregion

      protected IEnumerable<ServiceReference> FindServicesImpl( Type[] serviceTypes )
      {
         CompositeModelTypeInstanceScopeSupport support = this._modelSupport[CompositeModelType.SERVICE];
         return this._modelFinder( model => CompositeModelType.SERVICE.Equals( model.ModelType ) && serviceTypes.All( sType => model.PublicTypes.Any( pType => sType.IsAssignableFrom( pType ) ) ) )
            .Select( tuple => new ServiceReferenceImpl( (ServiceCompositeModel) tuple.Item2, () => (ServiceCompositeInstanceImpl) support.CreateInstance( tuple.Item1, tuple.Item2, serviceTypes, null ), this._structure.Application/*, serviceTypes */ )
            );
      }
   }
}
