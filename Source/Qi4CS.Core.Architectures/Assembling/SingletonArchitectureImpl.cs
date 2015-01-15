using System;
using System.Collections.Generic;
using CollectionsWithRoles.API;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
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
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Architectures.Common;
using Qi4CS.Core.Architectures.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Architectures.Assembling
{
   internal class SingletonArchitectureImpl : ApplicationArchitectureSkeleton<SingletonApplicationModel>, SingletonArchitecture
   {
      private readonly SingletonAssemblerImpl _compositeAssembler;
      private readonly DomainSpecificAssemblerAggregatorImpl<Assembler> _domainSpecificAssemblers;

      internal SingletonArchitectureImpl( IEnumerable<CompositeModelTypeAssemblyScopeSupport> modelTypeSupport )
         : base( modelTypeSupport )
      {
         // Default composite assembler
         this._compositeAssembler = new SingletonAssemblerImpl( this, new CompositeIDGenerator().IDGeneratorFunction, this.ModelTypeSupport, this.UsesContainer, this.CollectionsFactory );

         // Domain specific assembler holder
         this._domainSpecificAssemblers = new DomainSpecificAssemblerAggregatorImpl<Assembler>( this.CollectionsFactory );
      }

      protected override SingletonApplicationModel DoCreateModel()
      {
         DictionaryQuery<CompositeModelType, CompositeModelTypeModelScopeSupport> compositeModelTypeSupport;
         DictionaryProxy<Int32, CompositeModel> models;
         SetProxy<CompositeModel> modelsForContainer;
         var resultImmutable = new SingletonApplicationModelImmutable( this, ArchitectureDefaults.GENERIC_COMPOSITE_PROPERTY_MIXIN_TYPE, ArchitectureDefaults.GENERIC_COMPOSITE_EVENT_MIXIN_TYPE, ArchitectureDefaults.GENERIC_FRAGMENT_TYPE, this.ModelTypeSupport, out compositeModelTypeSupport, out models, out modelsForContainer, this.CompositeAssembler );

         foreach ( var modelType in this.ModelTypeSupport.Keys )
         {
            foreach ( var info in this._compositeAssembler.GetInfos( modelType ) )
            {
               var cModel = this.NewCompositeModel( compositeModelTypeSupport, resultImmutable, info, null );
               models.Add( cModel.CompositeModelID, cModel );
               modelsForContainer.Add( cModel );
            }
         }

         return resultImmutable;
      }

      #region SingletonArchitecture Members

      public Assembler CompositeAssembler
      {
         get
         {
            return this._compositeAssembler;
         }
      }

      #endregion

      #region DomainSpecificAssemblerAggregator<Assembler> Members

      public void AddDomainSpecificAssemblers( params DomainSpecificAssembler<Assembler>[] assemblers )
      {
         this._domainSpecificAssemblers.AddDomainSpecificAssemblers( assemblers );
      }

      #endregion

      public override IEnumerable<Assembler> AllAssemblers
      {
         get
         {
            return this._compositeAssembler.Singleton();
         }
      }

      protected override void AssembleDomainSpecificAssemblers()
      {
         foreach ( DomainSpecificAssembler<Assembler> assembler in this._domainSpecificAssemblers.Assemblers )
         {
            assembler.AddComposites( this._compositeAssembler );
         }
      }

      protected override Tuple<Type, InjectionFunctionality>[] GetInjectionFunctionality()
      {
         return new[] 
         {
            Tuple.Create<Type, InjectionFunctionality>( typeof( ThisAttribute ), new ThisInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( ConcernForAttribute ), new ConcernForInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( SideEffectForAttribute ), new SideEffectForInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( StateAttribute ), new StateInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( UsesAttribute ), new UsesInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( InvocationAttribute ), new InvocationInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( ServiceAttribute ), new SingletonApplicationServiceInjectionFunctionality() ),
            Tuple.Create<Type, InjectionFunctionality>( typeof( StructureAttribute ), new SingletonApplicationStructureInjectionFunctionality() )
         };
      }
   }
}
