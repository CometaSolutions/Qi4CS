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
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Runtime.Instance;

namespace Qi4CS.Core.Runtime.Model
{
   public class GeneratedTypeInfoImpl : GeneratedTypeInfo
   {
      private readonly Type _generatedType;
      private readonly Int32 _typeID;

      public GeneratedTypeInfoImpl( Type generatedType )
      {
         ArgumentValidator.ValidateNotNull( "Generated type", generatedType );

         this._generatedType = generatedType;
         this._typeID = generatedType.GetCompositeTypeID();
      }

      #region GeneratedTypeInfo Members

      public Type GeneratedType
      {
         get
         {
            return this._generatedType;
         }
      }

      public Int32 GeneratedTypeID
      {
         get
         {
            return this._typeID;
         }
      }

      #endregion
   }

   public class TypeGenerationResultImpl : GeneratedTypeInfoImpl, TypeGenerationResult
   {
      private readonly Type _declaredType;

      public TypeGenerationResultImpl( Type generatedType, Type declaredType )
         : base( generatedType )
      {
         ArgumentValidator.ValidateNotNull( "Declared type", declaredType );

         this._declaredType = declaredType;
      }

      #region TypeGenerationResult Members

      public Type DeclaredType
      {
         get
         {
            return this._declaredType;
         }
      }

      #endregion
   }

   public class FragmentTypeGenerationResultImpl : TypeGenerationResultImpl, FragmentTypeGenerationResult
   {
      private readonly Boolean _instancePoolRequired;

      public FragmentTypeGenerationResultImpl( Type declaredType, Type generatedType )
         : base( generatedType, declaredType )
      {
         this._instancePoolRequired = generatedType.FragmentTypeNeedsPool();
      }

      #region FragmentTypeGenerationResult Members

      public Boolean InstancePoolRequired
      {
         get
         {
            return this._instancePoolRequired;
         }
      }

      #endregion
   }

   public class PublicCompositeTypeGenerationResultImpl : PublicCompositeTypeGenerationResult
   {
      private readonly CompositeFactory _compositeFactory;
      private readonly Type _generatedPublicMainType;
      private readonly ListQuery<GeneratedTypeInfo> _generatedPublicTypes;
      private readonly DictionaryQuery<Type, ListQuery<Int32>> _publicCompositeGenericArguments;
      private readonly ListQuery<FragmentTypeGenerationResult> _fragmentGenerationResults;
      private readonly ListQuery<TypeGenerationResult> _concernInvocationGenerationResults;
      private readonly ListQuery<TypeGenerationResult> _sideEffectInvocationGenerationResults;
      private readonly ListQuery<TypeGenerationResult> _privateCompositeGenerationResults;
      private readonly Int32 _maxParamCountForCtors;

      public PublicCompositeTypeGenerationResultImpl(
         CompositeModel cModel,
         CompositeTypeModel tModel,
         CompositeCodeGenerationInfo codeGenerationInfo,
         IDictionary<Assembly, Assembly> assDic
         )
      {
         //var orderer = new TypeLoadOrderer( cModel );

         var collectionsFactory = cModel.ApplicationModel.CollectionsFactory;
         var publicTypes = cModel
            .GetAllCompositeTypes()
            .Concat( cModel.GetAllFragmentTypes() )
            //.OrderBy( t => t, orderer )
            .Where( t => cModel.ApplicationModel.AffectedAssemblies.Contains( t.GetAssembly() ) )
            .OrderBy( t => t.Equals( cModel.MainCodeGenerationType ) ? 0 : 1 ) // Populate main code generation type first
            .Select( t => GetGeneratedPublicType( t, cModel, codeGenerationInfo, assDic ) )
            .Distinct()
            .ToArray();

         var mainTypes = publicTypes.Where( iResult => iResult.IsPublicTypeMainCompositeType() );
         if ( !mainTypes.Any() || mainTypes.Skip( 1 ).Any() )
         {
            throw new ArgumentException( ( mainTypes.Any() ? "Too many" : "Too little" ) + " generated main types (" + String.Join( ", ", mainTypes ) + "), exactly one allowed." );
         }
         var mainType = mainTypes.First();
         this._compositeFactory = (CompositeFactory) mainType.GetAssembly().GetType( mainType.Name + codeGenerationInfo.CompositeFactorySuffix, true )
#if WINDOWS_PHONE_APP
            .GetAllInstanceConstructors().First()
#else
            .GetConstructors()[0]
#endif
            .Invoke(null);

         var fragmentTypeGenerationResults = tModel.FragmentTypeInfos.Keys
            //.OrderBy( t => t, orderer )
            .Select( t => Tuple.Create( t, GetParticipantType( t, cModel, codeGenerationInfo, assDic, codeGenerationInfo.FragmentPrefix, true ) ) )
            .Where( t => t.Item2 != null )
            .Select( t => (FragmentTypeGenerationResult) new FragmentTypeGenerationResultImpl( t.Item1, t.Item2 ) )
            .ToArray();
         var concernInvocationGenerationResults = tModel.ConcernInvocationTypeInfos.Keys
            //.OrderBy( t => t, orderer )
            .Select( t => Tuple.Create( t, GetParticipantType( t, cModel, codeGenerationInfo, assDic, codeGenerationInfo.ConcernInvocationPrefix ) ) )
            .Where( t => t.Item2 != null )
            .Select( t => (TypeGenerationResult) new TypeGenerationResultImpl( t.Item2, t.Item1 ) )
            .ToArray();
         var sideEffectInvocationGenerationResults = tModel.SideEffectInvocationTypeInfos.Keys
            //.OrderBy( t => t, orderer )
            .Select( t => Tuple.Create( t, GetParticipantType( t, cModel, codeGenerationInfo, assDic, codeGenerationInfo.SideEffectInvocationPrefix ) ) )
            .Where( t => t.Item2 != null )
            .Select( t => (TypeGenerationResult) new TypeGenerationResultImpl( t.Item2, t.Item1 ) )
            .ToArray();
         var privateCompositeGenerationresults = tModel.PrivateCompositeTypeInfos.Keys
            //.OrderBy( t => t, orderer )
            .Select( t => Tuple.Create( t, GetParticipantType( t, cModel, codeGenerationInfo, assDic, codeGenerationInfo.PrivateCompositePrefix ) ) )
            .Where( t => t.Item2 != null )
            .Select( t => (TypeGenerationResult) new TypeGenerationResultImpl( t.Item2, t.Item1 ) )
            .ToArray();

         var pGArgs = collectionsFactory.NewDictionaryProxy(
            tModel.PublicCompositeGenericArguments
            .Select( ( gArg, idx ) => Tuple.Create( gArg, idx ) )
            .GroupBy( tuple => tuple.Item1.DeclaringType )
            .ToDictionary( grouping => grouping.Key, grouping => collectionsFactory.NewListProxy( grouping.Select( tuple => tuple.Item2 ).ToList() ).CQ ) );

         this._generatedPublicMainType = mainType;
         this._maxParamCountForCtors = mainType
#if WINDOWS_PHONE_APP
            .GetAllInstanceConstructors().First()
#else
            .GetConstructors( BindingFlags.Instance | BindingFlags.Public )[0]
#endif
            .GetParameters().Length;

         this._generatedPublicTypes = collectionsFactory.NewListProxy( publicTypes.Select(
            pt => (GeneratedTypeInfo) new GeneratedTypeInfoImpl( pt ) )
            .ToList() ).CQ;
         this._publicCompositeGenericArguments = pGArgs.CQ;
         this._privateCompositeGenerationResults = collectionsFactory.NewListProxyFromParams( privateCompositeGenerationresults ).CQ;
         this._fragmentGenerationResults = collectionsFactory.NewListProxyFromParams( fragmentTypeGenerationResults ).CQ;
         this._concernInvocationGenerationResults = collectionsFactory.NewListProxyFromParams( concernInvocationGenerationResults ).CQ;
         this._sideEffectInvocationGenerationResults = collectionsFactory.NewListProxyFromParams( sideEffectInvocationGenerationResults ).CQ;

         // Remember to remove Qi4CS assembly if present
         assDic.Remove(ReflectionHelper.QI4CS_ASSEMBLY);
      }

      protected static Type GetGeneratedPublicType( Type type, CompositeModel model, CompositeCodeGenerationInfo codeGenerationInfo, IDictionary<Assembly, Assembly> assDic )
      {
         return assDic.GetOrAdd_NotThreadSafe(
            type.GetAssembly(),
            a => ReflectionHelper.QI4CS_ASSEMBLY.Equals(a) ? assDic[model.MainCodeGenerationType.GetAssembly()] : Assembly.Load(
#if WINDOWS_PHONE_APP
               new AssemblyName(
#endif
               Qi4CSGeneratedAssemblyAttribute.GetGeneratedAssemblyName( a )
#if WINDOWS_PHONE_APP
               )
#endif
               )
            ).GetType(
            codeGenerationInfo.PublicCompositePrefix + model.CompositeModelID,
            true
            );
      }

      protected static Type GetParticipantType( Type type, CompositeModel model, CompositeCodeGenerationInfo codeGenerationInfo, IDictionary<Assembly, Assembly> assDic, String prefix, Boolean useBaseType = false )
      {
         return GetGeneratedPublicType( type, model, codeGenerationInfo, assDic )
#if WINDOWS_PHONE_APP
            .GetTypeInfo().DeclaredNestedTypes.Select(t => t.AsType())
#else
            .GetNestedTypes( BindingFlags.Public | BindingFlags.NonPublic )
#endif
            .FirstOrDefault( tt => tt.Name.StartsWith( prefix )
               && ( useBaseType ? tt.GetBaseType().GetGenericDefinitionIfContainsGenericParameters().Equals( type ) : tt.GetAllParentTypes().Any( ttt => ttt.GetGenericDefinitionIfContainsGenericParameters().Equals( type ) ) )
            //&& !TypeUtil.TypesOf( tt ).Except( new Type[] { tt, type } ).Any( ttt => TypeUtil.IsAssignableFrom( TypeUtil.GenericDefinitionIfContainsGenericParams( type ), ttt ) )
               );
      }

      #region PublicCompositeTypeGenerationResult Members

      public CompositeFactory CompositeFactory
      {
         get
         {
            return this._compositeFactory;
         }
      }

      public DictionaryQuery<Type, ListQuery<Int32>> PublicCompositeGenericArguments
      {
         get
         {
            return this._publicCompositeGenericArguments;
         }
      }

      public Type GeneratedMainPublicType
      {
         get
         {
            return this._generatedPublicMainType;
         }
      }

      public Int32 MaxParamCountForCtors
      {
         get
         {
            return this._maxParamCountForCtors;
         }
      }

      public ListQuery<GeneratedTypeInfo> GeneratedPublicTypes
      {
         get
         {
            return this._generatedPublicTypes;
         }
      }

      public ListQuery<FragmentTypeGenerationResult> FragmentGenerationResults
      {
         get
         {
            return this._fragmentGenerationResults;
         }
      }

      public ListQuery<TypeGenerationResult> ConcernInvocationGenerationResults
      {
         get
         {
            return this._concernInvocationGenerationResults;
         }
      }

      public ListQuery<TypeGenerationResult> SideEffectGenerationResults
      {
         get
         {
            return this._sideEffectInvocationGenerationResults;
         }
      }

      public ListQuery<TypeGenerationResult> PrivateCompositeGenerationResults
      {
         get
         {
            return this._privateCompositeGenerationResults;
         }
      }

      #endregion
   }
}
