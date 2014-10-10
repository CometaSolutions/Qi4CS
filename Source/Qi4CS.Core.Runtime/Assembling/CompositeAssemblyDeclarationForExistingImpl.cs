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
   public abstract class AbstractCompositeAssemblyDeclarationForExistingImpl : AbstractCompositeAssemblyDeclaration
   {
      private readonly Assembler _assembler;
      private readonly DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> _composites;
      private readonly ISet<Type> _affectedTypes;
      private readonly CompositeModelType _compositeModelType;

      public AbstractCompositeAssemblyDeclarationForExistingImpl( Assembler assembler, CompositeModelType compositeModelType, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos )
      {
         ArgumentValidator.ValidateNotNull( "Assembler", assembler );
         ArgumentValidator.ValidateNotNull( "Assembly infos", compositeAssemblyInfos );
         ArgumentValidator.ValidateNotNull( "Composite model type", compositeModelType );

         this._assembler = assembler;
         this._compositeModelType = compositeModelType;
         this._composites = compositeAssemblyInfos;
         this._affectedTypes = new HashSet<Type>();
      }

      #region CompositeAssemblyDeclaration Members

      public FragmentAssemblyDeclaration WithMixins( params Type[] mixinTypes )
      {
         mixinTypes = mixinTypes.FilterNulls();
         this.ForEachInfo( info => info.GetFragmentAssemblyInfo( FragmentModelType.Mixin ).AddFragmentTypes( mixinTypes ) );
         return new FragmentAssemblyDeclarationImpl( this, mixinTypes, this.GetFragmentAssemblies( FragmentModelType.Mixin ) );
      }

      public FragmentAssemblyDeclaration WithConcerns( params Type[] concernTypes )
      {
         concernTypes = concernTypes.FilterNulls();
         this.ForEachInfo( info => info.GetFragmentAssemblyInfo( FragmentModelType.Concern ).AddFragmentTypes( concernTypes ) );
         return new FragmentAssemblyDeclarationImpl( this, concernTypes, this.GetFragmentAssemblies( FragmentModelType.Concern ) );
      }

      public FragmentAssemblyDeclaration WithSideEffects( params Type[] sideEffectTypes )
      {
         sideEffectTypes = sideEffectTypes.FilterNulls();
         this.ForEachInfo( info => info.GetFragmentAssemblyInfo( FragmentModelType.SideEffect ).AddFragmentTypes( sideEffectTypes ) );
         return new FragmentAssemblyDeclarationImpl( this, sideEffectTypes, this.GetFragmentAssemblies( FragmentModelType.SideEffect ) );
      }

      public AbstractCompositeAssemblyDeclaration WithConstraints( params Type[] constraints )
      {
         constraints = constraints.FilterNulls();
         this.ForEachInfo( info => info.GetFragmentAssemblyInfo( FragmentModelType.Constraint ).AddFragmentTypes( constraints ) );
         return this;
      }

      public AbstractCompositeAssemblyDeclaration OfTypes( params Type[] types )
      {
         types = types.FilterNulls();
         this._affectedTypes.UnionWith( types );
         var mainType = types.FirstOrDefault( type => !Types.QI4CS_ASSEMBLY.Equals( type.GetAssembly() ) );
         if ( mainType != null )
         {
            this.ForEachInfo( info =>
            {
               if ( info.MainCodeGenerationType == null )
               {
                  info.MainCodeGenerationType = mainType;
               }
            } );
         }
         return this;
      }

      public IEnumerable<Int32> AffectedCompositeIDs
      {
         get
         {
            return this._composites
               .Where( kvp => this._affectedTypes.Any( aType => aType.GetGenericDefinitionIfContainsGenericParameters().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( kvp.Key ) ) )
               .SelectMany( kvp => kvp.Value )
               .Select( info => info.CompositeID )
               .Distinct();
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
            return this._compositeModelType;
         }
      }

      public AbstractCompositeAssemblyDeclaration WithDefaultFor( System.Reflection.PropertyInfo property, Func<PropertyInfo, ApplicationSPI, Object> defaultProvider )
      {
         if ( property != null && defaultProvider != null )
         {
            this.ForEachInfo( info => info.DefaultProviders[AbstractCompositeAssemblyDeclarationForNewImpl.ProcessPropertyInfoForDefaultProvider( property )] = defaultProvider );
         }
         return this;
      }

      #endregion

      #region UsesProvider Members

      public AbstractCompositeAssemblyDeclaration Use( params Object[] objects )
      {
         this.ForEachInfo( info => info.Use( objects ) );
         return this;
      }

      public AbstractCompositeAssemblyDeclaration UseWithName( String name, Object value )
      {
         this.ForEachInfo( info => info.UseWithName( name, value ) );
         return this;
      }

      #endregion

      #region UsesProviderQuery Members

      public Object GetObjectForName( Type type, String name )
      {
         Object result = null;
         this.ForEachInfo( info => result = result == null ? info.MetaInfoContainer.GetObjectForName( type, name ) : result );
         return result;
      }

      #endregion

      protected FragmentAssemblyInfo[] GetFragmentAssemblies( FragmentModelType fragmentModelType )
      {
         return this._affectedTypes.Where( type => this._composites.ContainsKey( type ) ).SelectMany( type => this._composites[type] ).Select( info => info.GetFragmentAssemblyInfo( fragmentModelType ) ).ToArray();
      }

      protected void ForEachInfo( Action<CompositeAssemblyInfo> action )
      {
         ListQuery<CompositeAssemblyInfo> list = this._composites
            .Where( kvp => this._affectedTypes.Contains( kvp.Key ) )
            .Select( kvp => kvp.Value )
            .FirstOrDefault();
         if ( list != null )
         {
            list
               .Where( info => info.Types.IsSupersetOf( this._affectedTypes ) )
               .Distinct( ReferenceEqualityComparer<CompositeAssemblyInfo>.ReferenceBasedComparer )
               .All( info =>
               {
                  action( info );
                  return true;
               } );
         }
      }
   }

   public class PlainCompositeAssemblyDeclarationForExistingImpl : AbstractCompositeAssemblyDeclarationForExistingImpl, PlainCompositeAssemblyDeclaration
   {
      public PlainCompositeAssemblyDeclarationForExistingImpl( Assembler assembler, CompositeModelType compositeModelType, DictionaryQuery<Type, ListQuery<CompositeAssemblyInfo>> compositeAssemblyInfos )
         : base( assembler, compositeModelType, compositeAssemblyInfos )
      {
      }
   }
}
