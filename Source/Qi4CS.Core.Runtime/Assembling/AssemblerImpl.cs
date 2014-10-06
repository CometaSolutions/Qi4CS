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
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Assembling
{
   public abstract class AssemblerImpl<TApplication> : Assembler
      where TApplication : Application
   {
      private static readonly IEnumerable<CompositeAssemblyInfo> EMPTY_INFOS = Enumerable.Repeat<CompositeAssemblyInfo>( null, 0 );

      private readonly ApplicationArchitecture<ApplicationModel<ApplicationSPI>> _applicationArchitecture;
      private readonly Func<Int32> _newCompositeIDRequestor;
      private readonly CollectionsFactory _collectionsFactory;
      private readonly DictionaryProxy<CompositeModelType, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>>> _compositeInfos;
      private readonly DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> _assemblyScopeSupport;
      private readonly UsesContainerMutable _parentContainer;

      public AssemblerImpl( ApplicationArchitecture<ApplicationModel<ApplicationSPI>> applicationArchitecture, Func<Int32> newCompositeIDRequestor, DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> modelTypeSupport, UsesContainerMutable parentContainer, CollectionsFactory collectionsFactory )
      {
         ArgumentValidator.ValidateNotNull( "Application architecture", applicationArchitecture );
         ArgumentValidator.ValidateNotNull( "ID requestor function", newCompositeIDRequestor );
         ArgumentValidator.ValidateNotNull( "Model type support", modelTypeSupport );
         ArgumentValidator.ValidateNotNull( "Parent uses container", parentContainer );
         ArgumentValidator.ValidateNotNull( "Collections factory", collectionsFactory );

         this._applicationArchitecture = applicationArchitecture;
         this._newCompositeIDRequestor = newCompositeIDRequestor;
         this._collectionsFactory = collectionsFactory;
         this._compositeInfos = this._collectionsFactory.NewDictionaryProxy<CompositeModelType, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>>>();
         this._assemblyScopeSupport = modelTypeSupport;
         this._parentContainer = parentContainer;
      }

      #region Assembler Members

      public TCompositeDeclaration NewComposite<TCompositeDeclaration>( CompositeModelType compositeType )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration
      {
         var infos = this.GetInfosForAssemblingDeclaration( compositeType );
         return this._assemblyScopeSupport[compositeType].CreateAssemblyDeclarationForNew<TCompositeDeclaration>( this, infos, this._assemblyScopeSupport[compositeType].CreateCompositeInfo( this._newCompositeIDRequestor(), this._parentContainer.Query ), this._collectionsFactory );
      }

      public TCompositeDeclaration ForExistingComposite<TCompositeDeclaration>( CompositeModelType compositeType )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration
      {
         var infos = this.GetInfosForAssemblingDeclaration( compositeType );
         return this._assemblyScopeSupport[compositeType].CreateAssemblyDeclarationForExisting<TCompositeDeclaration>( this, infos.MQ.IQ );
      }

      public Boolean ForNewOrExistingComposite<TCompositeDeclaration>( CompositeModelType compositeType, IEnumerable<Type> types, out TCompositeDeclaration result )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration
      {
         var infos = this.GetInfosForAssemblingDeclaration( compositeType );
         var givenExceptExisting = types.Except( types.Where( typeInner => infos.CQ.Keys.Any( key => typeInner.GetGenericDefinitionIfContainsGenericParameters().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( key ) ) ) );
         var returnValue = givenExceptExisting.Any();
         if ( returnValue )
         {
            // Completely new composite assembly declaration
            result = this.NewComposite<TCompositeDeclaration>( compositeType );
         }
         else
         {
            // There is already one or more existing infos
            result = this.ForExistingComposite<TCompositeDeclaration>( compositeType );
         }
         result.OfTypes( types.ToArray() );
         return returnValue;
      }

      public ApplicationArchitecture<ApplicationModel<ApplicationSPI>> ApplicationArchitecture
      {
         get
         {
            return this._applicationArchitecture;
         }
      }

      public StructureServiceProviderSPI GetStructureServiceProvider( Application application )
      {
         ArgumentValidator.ValidateNotNull( "Application", application );

         if ( application is TApplication )
         {
            return this.DoGetStructureServiceProvider( (TApplication) application );
         }
         else
         {
            throw new ArgumentException( "The given Qi4CS application was of wrong type. Expected " + typeof( TApplication ) + " but was " + application.GetType() + "." );
         }
      }

      #endregion

      public IEnumerable<CompositeAssemblyInfo> GetInfos( CompositeModelType compositeType )
      {
         IEnumerable<CompositeAssemblyInfo> result = EMPTY_INFOS;
         DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> dic;
         if ( this._compositeInfos.CQ.TryGetValue( compositeType, out dic ) )
         {
            result = dic.CQ.Values.SelectMany( list => list.CQ );
         }
         return result.Distinct( ReferenceEqualityComparer<CompositeAssemblyInfo>.ReferenceBasedComparer );
      }

      protected DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> AssemblyScopeSupport
      {
         get
         {
            return this._assemblyScopeSupport;
         }
      }

      #region UsesProvider<Assembler>> Members

      public Assembler Use( params Object[] objects )
      {
         this._parentContainer.Use( objects );
         return this;
      }

      public Assembler UseWithName( String name, Object value )
      {
         this._parentContainer.UseWithName( name, value );
         return this;
      }

      #endregion

      protected DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> GetInfosForAssemblingDeclaration( CompositeModelType compositeModelType )
      {
         ArgumentValidator.ValidateNotNull( "Composite model type", compositeModelType );

         DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> infos;
         if ( !this._compositeInfos.CQ.TryGetValue( compositeModelType, out infos ) )
         {
            infos = this._collectionsFactory.NewDictionary<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>>();
            this._compositeInfos.Add( compositeModelType, infos );
         }
         return infos;
      }

      protected abstract StructureServiceProviderSPI DoGetStructureServiceProvider( TApplication application );

   }
}
