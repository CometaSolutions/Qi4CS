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
using CollectionsWithRoles.API;
using Qi4CS.Core.Architectures.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Common;

namespace Qi4CS.Core.Architectures.Assembling
{
   internal sealed class LayeredPlainCompositeModelTypeAssemblyScopeSupport : AbstractPlainCompositeModelTypeAssemblyScopeSupport
   {
      public LayeredPlainCompositeModelTypeAssemblyScopeSupport()
         : base()
      {
      }

      public override Runtime.Assembling.CompositeAssemblyInfo CreateCompositeInfo( Int32 id, UsesContainerQuery parentContainer )
      {
         return new LayeredCompositeAssemblyInfoImpl( id, this.ModelType, parentContainer );
      }

      protected override Bootstrap.Assembling.AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForNew( Bootstrap.Assembling.Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo assemblyInfo, CollectionsFactory collectionsFactory )
      {
         return new LayeredPlainCompositeAssemblyDeclarationForNewImpl( assembler, compositeAssemblyInfos, (LayeredCompositeAssemblyInfo) assemblyInfo, collectionsFactory );
      }

      protected override Bootstrap.Assembling.AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForExisting( Assembler assembler, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos )
      {
         return new LayeredPlainCompositeAssemblyDeclarationForExistingImpl( assembler, this.ModelType, compositeAssemblyInfos );
      }

      public override CompositeModelTypeModelScopeSupport CreateModelScopeSupport()
      {
         return new LayeredPlainCompositeModelTypeModelScopeSupport( this );
      }
   }

   internal sealed class LayeredServiceModelTypeAssemblyScopeSupport : AbstractServiceModelTypeAssemblyScopeSupport
   {
      public LayeredServiceModelTypeAssemblyScopeSupport()
         : base()
      {
      }

      public override Runtime.Assembling.CompositeAssemblyInfo CreateCompositeInfo( Int32 id, UsesContainerQuery parentContainer )
      {
         return new LayeredServiceCompositeAssemblyInfoImpl( id, parentContainer );
      }

      protected override Bootstrap.Assembling.AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForNew( Bootstrap.Assembling.Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo assemblyInfo, CollectionsFactory collectionsFactory )
      {
         return new LayeredServiceCompositeAssemblyDeclarationForNewImpl( assembler, compositeAssemblyInfos, (LayeredServiceCompositeAssemblyInfo) assemblyInfo, collectionsFactory );
      }

      protected override Bootstrap.Assembling.AbstractCompositeAssemblyDeclaration DoCreateAssemblyDeclarationForExisting( Assembler assembler, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos )
      {
         return new LayeredServiceCompositeAssemblyDeclarationForExistingImpl( assembler, this.ModelType, compositeAssemblyInfos );
      }

      public override CompositeModelTypeModelScopeSupport CreateModelScopeSupport()
      {
         return new LayeredServiceModelTypeModelScopeSupport( this );
      }
   }

   //internal sealed class LayeredValueModelTypeAssemblyScopeSupport : AbstractValueModelTypeAssemblyScopeSupport
   //{
   //   public LayeredValueModelTypeAssemblyScopeSupport()
   //      : base()
   //   {
   //   }

   //   public override Runtime.Assembling.CompositeAssemblyInfo CreateCompositeInfo( Int32 id, UsesContainerQuery parentContainer )
   //   {
   //      return new LayeredCompositeAssemblyInfoImpl( id, this.ModelType, parentContainer );
   //   }

   //   protected override Bootstrap.Assembling.CompositeAssemblyDeclaration DoCreateAssemblyDeclarationForNew( Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo assemblyInfo, CollectionsFactory collectionsFactory )
   //   {
   //      return new LayeredCompositeAssemblyDeclarationForNewImpl( assembler, compositeAssemblyInfos, (LayeredCompositeAssemblyInfo) assemblyInfo, collectionsFactory );
   //   }

   //   protected override Bootstrap.Assembling.CompositeAssemblyDeclaration DoCreateAssemblyDeclarationForExisting( Assembler assembler, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos )
   //   {
   //      return new LayeredCompositeAssemblyDeclarationForExistingImpl( assembler, this.ModelType, compositeAssemblyInfos );
   //   }

   //   public override CompositeModelTypeModelScopeSupport CreateModelScopeSupport()
   //   {
   //      return new LayeredValueModelTypeModelScopeSupport( this );
   //   }
   //}
}
