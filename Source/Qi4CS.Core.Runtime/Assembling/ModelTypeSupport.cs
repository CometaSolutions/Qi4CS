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
using System;
using System.Collections.Generic;
using System.Linq;
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Common;

namespace Qi4CS.Core.Runtime.Assembling
{
   public interface CompositeModelTypeAssemblyScopeSupport
   {
      CompositeModelType ModelType { get; }

      CompositeAssemblyInfo CreateCompositeInfo( Int32 id, UsesContainerQuery parentContainer );

      TCompositeDeclaration CreateAssemblyDeclarationForNew<TCompositeDeclaration>( Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo assemblyInfo, CollectionsFactory collectionsFactory )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration;

      TCompositeDeclaration CreateAssemblyDeclarationForExisting<TCompositeDeclaration>( Assembler assembler, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration;

      IEnumerable<Type> MandatoryRoles { get; }

      CompositeModelTypeModelScopeSupport CreateModelScopeSupport();
   }

   public abstract class AbstractModelTypeAssemblyScopeSupport : CompositeModelTypeAssemblyScopeSupport
   {
      private readonly API.Instance.CompositeModelType _modelType;
      private readonly Type[] _mandatoryRoles;

      protected AbstractModelTypeAssemblyScopeSupport( API.Instance.CompositeModelType modelType, params Type[] mandatoryRoles )
      {
         ArgumentValidator.ValidateNotNull( "Composite model type", modelType );

         this._modelType = modelType;
         this._mandatoryRoles = mandatoryRoles.Skip( 0 ).ToArray();
      }

      #region CompositeModelTypeAssemblyScopeSupport Members

      public API.Instance.CompositeModelType ModelType
      {
         get
         {
            return this._modelType;
         }
      }

      public TCompositeDeclaration CreateAssemblyDeclarationForNew<TCompositeDeclaration>( Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo assemblyInfo, CollectionsFactory collectionsFactory )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration
      {
         var result = this.DoCreateAssemblyDeclarationForNew( assembler, compositeAssemblyInfos, assemblyInfo, collectionsFactory );
         if ( this._mandatoryRoles.Any() )
         {
            result.OfTypes( this._mandatoryRoles.ToArray() );
         }
         return (TCompositeDeclaration) result;
      }

      public virtual TCompositeDeclaration CreateAssemblyDeclarationForExisting<TCompositeDeclaration>( Assembler assembler, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos )
         where TCompositeDeclaration : AbstractCompositeAssemblyDeclaration
      {
         var result = this.DoCreateAssemblyDeclarationForExisting( assembler, compositeAssemblyInfos );
         //if ( this._mandatoryRoles.Any() )
         //{
         //   result.OfTypes( this._mandatoryRoles.ToArray() );
         //}
         return (TCompositeDeclaration) result;
      }

      public virtual CompositeAssemblyInfo CreateCompositeInfo( Int32 id, UsesContainerQuery parentContainer )
      {
         return new CompositeAssemblyInfoImpl( id, this._modelType, parentContainer );
      }

      public IEnumerable<Type> MandatoryRoles
      {
         get
         {
            return this._mandatoryRoles.Skip( 0 );
         }
      }

      public abstract CompositeModelTypeModelScopeSupport CreateModelScopeSupport();

      #endregion

      protected abstract AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForNew( Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo assemblyInfo, CollectionsFactory collectionsFactory );

      protected abstract AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForExisting( Assembler assembler, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos );
   }

   public abstract class AbstractPlainCompositeModelTypeAssemblyScopeSupport : AbstractModelTypeAssemblyScopeSupport
   {
      protected AbstractPlainCompositeModelTypeAssemblyScopeSupport()
         : base( Qi4CS.Core.API.Instance.CompositeModelType.PLAIN )
      {

      }

      protected override Bootstrap.Assembling.AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForExisting( Bootstrap.Assembling.Assembler assembler, CollectionsWithRoles.API.DictionaryQuery<Type, CollectionsWithRoles.API.ListQuery<Assembling.CompositeAssemblyInfo>> compositeAssemblyInfos )
      {
         return new PlainCompositeAssemblyDeclarationForExistingImpl( assembler, Qi4CS.Core.API.Instance.CompositeModelType.PLAIN, compositeAssemblyInfos );
      }

      protected override Bootstrap.Assembling.AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForNew( Bootstrap.Assembling.Assembler assembler, CollectionsWithRoles.API.DictionaryWithRoles<Type, CollectionsWithRoles.API.ListProxy<Assembling.CompositeAssemblyInfo>, CollectionsWithRoles.API.ListProxyQuery<Assembling.CompositeAssemblyInfo>, CollectionsWithRoles.API.ListQuery<Assembling.CompositeAssemblyInfo>> compositeAssemblyInfos, Assembling.CompositeAssemblyInfo assemblyInfo, CollectionsWithRoles.API.CollectionsFactory collectionsFactory )
      {
         return new PlainCompositeAssemblyDeclarationForNewImpl( assembler, compositeAssemblyInfos, assemblyInfo, collectionsFactory );
      }
   }

   public abstract class AbstractServiceModelTypeAssemblyScopeSupport : AbstractModelTypeAssemblyScopeSupport
   {
      protected AbstractServiceModelTypeAssemblyScopeSupport()
         : base( Qi4CS.Core.API.Instance.CompositeModelType.SERVICE, typeof( Identity ) )
      {

      }

      public override Assembling.CompositeAssemblyInfo CreateCompositeInfo( Int32 id, UsesContainerQuery parentContainer )
      {
         return new ServiceCompositeAssemblyInfoImpl( id, this.ModelType, parentContainer );
      }

      protected override AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForNew( Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo assemblyInfo, CollectionsFactory collectionsFactory )
      {
         return new ServiceCompositeAssemblyDeclarationForNewImpl( assembler, compositeAssemblyInfos, assemblyInfo, collectionsFactory );
      }

      protected override AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForExisting( Assembler assembler, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos )
      {
         return new ServiceCompositeAssemblyDeclarationForExistingImpl( assembler, this.ModelType, compositeAssemblyInfos );
      }

   }

   //   public abstract class AbstractValueModelTypeAssemblyScopeSupport : AbstractModelTypeAssemblyScopeSupport
   //   {
   //      protected AbstractValueModelTypeAssemblyScopeSupport()
   //         : base( Qi4CS.Core.API.Instance.CompositeModelType.VALUE )
   //      {

   //      }
   //   }
}
