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
using System.Reflection;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Common;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Assembling
{
   public abstract class ApplicationArchitectureSkeleton<ApplicationModelType> : ApplicationArchitecture<ApplicationModelType>
      where ApplicationModelType : ApplicationModel<ApplicationSPI>
   {
      private readonly DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> _modelTypeSupport;
      private readonly UsesContainerMutable _metaInfoContainer;
      private readonly CollectionsFactory _collectionsFactory;
      private readonly DictionaryProxy<Type, InjectionFunctionality> _additionalInjectionFunctionalities;

      protected ApplicationArchitectureSkeleton( IEnumerable<CompositeModelTypeAssemblyScopeSupport> modelTypeSupport )
      {
         ArgumentValidator.ValidateNotNull( "Composite model type supports", modelTypeSupport );

         this._collectionsFactory = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY;
         this._modelTypeSupport = this._collectionsFactory.NewDictionaryProxy( modelTypeSupport.ToDictionary( support => support.ModelType, support => support ) ).CQ;
         this._metaInfoContainer = UsesContainerMutableImpl.CreateEmpty();
         this._additionalInjectionFunctionalities = this._collectionsFactory.NewDictionaryProxy<Type, InjectionFunctionality>();
      }

      #region ApplicationArchitecture Members

      public ApplicationModelType CreateModel()
      {
         this.AssembleDomainSpecificAssemblers();
         ApplicationModelType result = this.DoCreateModel();
         var inj = ( (InjectionServiceImpl) result.InjectionService );
         inj.RegisterInjectionFunctionality( this.GetInjectionFunctionality() );
         inj.RegisterInjectionFunctionality( this._additionalInjectionFunctionalities.CQ.Select( kvp => Tuple.Create( kvp.Key, kvp.Value ) ).ToArray() );
         EventHandler<ApplicationModelCreatedArgs> evt = this.ApplicationModelCreatedEvent;
         if ( evt != null )
         {
            evt( this, new ApplicationModelCreatedArgs( (ApplicationArchitecture<ApplicationModel<ApplicationSPI>>) this, result ) );
         }
         return result;
      }

      public DictionaryProxy<Type, InjectionFunctionality> AdditionalInjectionFunctionalities
      {
         get
         {
            return this._additionalInjectionFunctionalities;
         }
      }

      public CollectionsFactory CollectionsFactory
      {
         get
         {
            return this._collectionsFactory;
         }
      }

      public abstract IEnumerable<Assembler> AllAssemblers { get; }

      public event EventHandler<AttributeProcessingArgs> AttributeProcessingEvent;

      public event EventHandler<ApplicationModelCreatedArgs> ApplicationModelCreatedEvent;

      public event EventHandler<CompositeModelCreatedArgs> CompositeModelCreatedEvent;

      #endregion

      private Attribute LaunchAttributeProcessingEvent( Int32 compositeID, Object reflectionElement, Attribute attribute )
      {
         Attribute result = attribute;
         EventHandler<AttributeProcessingArgs> evt = this.AttributeProcessingEvent;
         if ( evt != null )
         {
            AttributeProcessingArgs args = new AttributeProcessingArgs( compositeID, reflectionElement, attribute );
            evt( this, args );
            result = args.NewAttribute;
         }
         return result;
      }

      protected abstract ApplicationModelType DoCreateModel();

      protected abstract void AssembleDomainSpecificAssemblers();

      protected abstract Tuple<Type, InjectionFunctionality>[] GetInjectionFunctionality();

      protected DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> ModelTypeSupport
      {
         get
         {
            return this._modelTypeSupport;
         }
      }

      protected CompositeModel NewCompositeModel( DictionaryQuery<CompositeModelType, CompositeModelTypeModelScopeSupport> compositeModelSupports, ApplicationModel<ApplicationSPI> appModel, CompositeAssemblyInfo compositeInfo, String architectureContainerID )
      {
         var result = compositeModelSupports[compositeInfo.CompositeModelType].NewCompositeModel( appModel, compositeInfo, this.LaunchAttributeProcessingEvent, architectureContainerID );
         this.CompositeModelCreatedEvent.InvokeEventIfNotNull( evt => evt( this, new CompositeModelCreatedArgs( result ) ) );
         return result;
      }

      #region UsesProvider Members

      public ApplicationArchitecture<ApplicationModelType> Use( params Object[] objects )
      {
         this._metaInfoContainer.Use( objects );
         return this;
      }

      public ApplicationArchitecture<ApplicationModelType> UseWithName( String name, Object value )
      {
         this._metaInfoContainer.UseWithName( name, value );
         return this;
      }

      #endregion

      protected UsesContainerMutable UsesContainer
      {
         get
         {
            return this._metaInfoContainer;
         }
      }

   }
}
