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
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Core.Runtime.Instance
{
   public class ServiceReferenceImpl : ServiceReferenceSPI
   {
      private readonly ServiceCompositeModel _model;
      private readonly Lazy<ServiceCompositeInstanceImpl> _instance;
      //      private readonly Type[] _actualServiceTypes;
      private readonly Application _application;

      public ServiceReferenceImpl( ServiceCompositeModel model, Func<ServiceCompositeInstanceImpl> instanceCreator, Application application /*, Type[] actualServiceTypes */ )
      {
         ArgumentValidator.ValidateNotNull( "Service composite model", model );
         ArgumentValidator.ValidateNotNull( "Service composite instance creator", instanceCreator );
         ArgumentValidator.ValidateNotNull( "Application", application );
         //         ArgumentValidator.ValidateNotEmpty( "Service types", actualServiceTypes );

         this._application = application;
         this._model = model;
         this._instance = new Lazy<ServiceCompositeInstanceImpl>( instanceCreator, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
         //         this._actualServiceTypes = actualServiceTypes;
      }

      #region ServiceReference<ServiceType> Members

      public Object GetServiceWithType( Type otherType )
      {
         var typeToSearch = otherType;
         if ( typeToSearch.IsGenericType() )
         {
            typeToSearch = typeToSearch.GetGenericTypeDefinition();
         }
         if ( this._instance.Value.ModelInfo.Model.PublicTypes
            .Any( t => typeToSearch.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( t ) ) )// ( isGeneric ? TypeUtil.GenericDefinitionIfGenericType( t ) : t ).Equals( typeToSearch ) ) )
         {
            return this._instance.Value.GetCompositeForType( otherType );
         }
         else
         {
            throw new ArgumentException( "The type must be one of the public types." );
         }
      }

      public Boolean Active
      {
         get
         {
            return !this._application.Passive && this._instance.Value.Active;
         }
      }

      public String ServiceID
      {
         get
         {
            return this._instance.Value.ServiceID;
         }
      }

      public void Activate()
      {
         this._instance.Value.ActivateIfNeeded( null, null );
      }

      public ServiceCompositeModel Model
      {
         get
         {
            return this._model;
         }
      }

      #endregion
   }
}
