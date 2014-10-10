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
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Core.Runtime.Assembling
{

   public abstract class AbstractCompositeAssemblyDeclarationForNewImpl : AbstractCompositeAssemblyDeclaration
   {
      private readonly Assembler _assembler;
      private readonly CollectionsFactory _collectionsFactory;
      private readonly DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> _composites;
      private readonly CompositeAssemblyInfo _info;

      public AbstractCompositeAssemblyDeclarationForNewImpl( Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo thisInfo, CollectionsFactory collectionsFactory )
      {
         ArgumentValidator.ValidateNotNull( "Assembler", assembler );
         ArgumentValidator.ValidateNotNull( "Assembly infos", compositeAssemblyInfos );
         ArgumentValidator.ValidateNotNull( "This composite info", thisInfo );
         ArgumentValidator.ValidateNotNull( "Collections factory", collectionsFactory );

         this._assembler = assembler;
         this._composites = compositeAssemblyInfos;
         this._info = thisInfo;
         this._collectionsFactory = collectionsFactory;
      }

      #region CompositeAssemblyDeclaration Members

      public FragmentAssemblyDeclaration WithMixins( params Type[] mixinTypes )
      {
         mixinTypes = mixinTypes.FilterNulls();
         var fInfo = this._info.GetFragmentAssemblyInfo( FragmentModelType.Mixin );
         fInfo.AddFragmentTypes( mixinTypes );
         return new FragmentAssemblyDeclarationImpl( this, mixinTypes, new FragmentAssemblyInfo[] { fInfo } );
      }

      public FragmentAssemblyDeclaration WithConcerns( params Type[] concernTypes )
      {
         concernTypes = concernTypes.FilterNulls();
         var fInfo = this._info.GetFragmentAssemblyInfo( FragmentModelType.Concern );
         fInfo.AddFragmentTypes( concernTypes );
         return new FragmentAssemblyDeclarationImpl( this, concernTypes, new FragmentAssemblyInfo[] { fInfo } );
      }

      public FragmentAssemblyDeclaration WithSideEffects( params Type[] sideEffectTypes )
      {
         sideEffectTypes = sideEffectTypes.FilterNulls();
         var fInfo = this._info.GetFragmentAssemblyInfo( FragmentModelType.SideEffect );
         fInfo.AddFragmentTypes( sideEffectTypes );
         return new FragmentAssemblyDeclarationImpl( this, sideEffectTypes, new FragmentAssemblyInfo[] { fInfo } );
      }

      public AbstractCompositeAssemblyDeclaration WithConstraints( params Type[] constraints )
      {
         constraints = constraints.FilterNulls();
         this._info.GetFragmentAssemblyInfo( FragmentModelType.Constraint ).AddFragmentTypes( constraints );
         return this;
      }

      public AbstractCompositeAssemblyDeclaration OfTypes( params Type[] types )
      {
         types = types.FilterNulls();
         foreach ( var type in types )
         {
            ListProxy<CompositeAssemblyInfo> list;
            if ( !this._composites.CQ.TryGetValue( type, out list ) )
            {
               list = this._collectionsFactory.NewListProxy<CompositeAssemblyInfo>();
               this._composites.Add( type, list );
            }
            if ( !list.CQ.Contains( this._info ) )
            {
               list.Add( this._info );
               this._info.Types.Add( type );
            }
         }
         var mainType = types.FirstOrDefault( type => !Types.QI4CS_ASSEMBLY.Equals( type
#if WINDOWS_PHONE_APP
            .GetTypeInfo()
#endif
.Assembly ) );
         if ( mainType != null )
         {
            this._info.MainCodeGenerationType = mainType;
         }
         return this;
      }

      public IEnumerable<Int32> AffectedCompositeIDs
      {
         get
         {
            return this._info.CompositeID.Singleton();
         }
      }

      public Assembler Assembler
      {
         get
         {
            return this._assembler;
         }
      }

      public CompositeModelType CompositeModelType
      {
         get
         {
            return this._info.CompositeModelType;
         }
      }

      public AbstractCompositeAssemblyDeclaration WithDefaultFor( PropertyInfo property, Func<PropertyInfo, ApplicationSPI, Object> defaultProvider )
      {
         if ( property != null && defaultProvider != null )
         {
            this._info.DefaultProviders[ProcessPropertyInfoForDefaultProvider( property )] = defaultProvider;
         }
         return this;
      }

      #endregion

      #region UsesProvider Members

      public AbstractCompositeAssemblyDeclaration Use( params Object[] objects )
      {
         this._info.Use( objects );
         return this;
      }

      public AbstractCompositeAssemblyDeclaration UseWithName( String name, Object value )
      {
         this._info.UseWithName( name, value );
         return this;
      }

      #endregion

      #region UsesProviderQuery Members

      public Object GetObjectForName( Type type, String name )
      {
         return this._info.MetaInfoContainer.GetObjectForName( type, name );
      }

      #endregion

      protected CompositeAssemblyInfo Info
      {
         get
         {
            return this._info;
         }
      }

      public static PropertyInfo ProcessPropertyInfoForDefaultProvider( PropertyInfo property )
      {
         if ( property.DeclaringType.ContainsGenericParameters() )
         {
            property = property.DeclaringType.GetGenericTypeDefinition().LoadPropertyOrThrow( property.Name );
         }
         return property;
      }
   }

   public class PlainCompositeAssemblyDeclarationForNewImpl : AbstractCompositeAssemblyDeclarationForNewImpl, PlainCompositeAssemblyDeclaration
   {
      public PlainCompositeAssemblyDeclarationForNewImpl( Assembler assembler, DictionaryWithRoles<Type, ListProxy<CompositeAssemblyInfo>, ListProxyQuery<CompositeAssemblyInfo>, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos, CompositeAssemblyInfo thisInfo, CollectionsFactory collectionsFactory )
         : base( assembler, compositeAssemblyInfos, thisInfo, collectionsFactory )
      {
      }
   }
}
