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

   public class FragmentTypeGenerationResultImpl : GeneratedTypeInfoImpl, FragmentTypeGenerationResult
   {
      private readonly Boolean _instancePoolRequired;

      public FragmentTypeGenerationResultImpl( Type generatedType )
         : base( generatedType )
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
      private readonly ListQuery<GeneratedTypeInfo> _concernInvocationGenerationResults;
      private readonly ListQuery<GeneratedTypeInfo> _sideEffectInvocationGenerationResults;
      private readonly ListQuery<GeneratedTypeInfo> _privateCompositeGenerationResults;
      private readonly Int32 _maxParamCountForCtors;

      public PublicCompositeTypeGenerationResultImpl(
         CompositeModel cModel,
         CompositeTypeModel tModel,
         IList<CompositeTypesAttribute> typeAttributes
         )
      {
         //var orderer = new TypeLoadOrderer( cModel );

         var collectionsFactory = cModel.ApplicationModel.CollectionsFactory;
         var publicTypes = typeAttributes
            .SelectMany( a => a.PublicCompositeTypes )
            .ToArray();

         var mainTypes = publicTypes.Where( iResult => iResult.IsPublicTypeMainCompositeType() );
         if ( !mainTypes.Any() || mainTypes.Skip( 1 ).Any() )
         {
            throw new ArgumentException( ( mainTypes.Any() ? "Too many" : "Too little" ) + " generated main types (" + String.Join( ", ", mainTypes ) + "), exactly one allowed." );
         }
         var mainType = mainTypes.First();
         this._compositeFactory = (CompositeFactory) typeAttributes.First( a => a.CompositeFactoryType != null ).CompositeFactoryType
#if WINDOWS_PHONE_APP
            .GetAllInstanceConstructors().First()
#else
.GetConstructors()[0]
#endif
.Invoke( null );

         var fragmentTypeGenerationResults = typeAttributes
            .SelectMany( a => a.FragmentTypes.EmptyIfNull() )
            .Select( t => (FragmentTypeGenerationResult) new FragmentTypeGenerationResultImpl( t ) )
            .ToArray();
         var concernInvocationGenerationResults = typeAttributes
            .SelectMany( a => a.ConcernInvokationHandlerTypes.EmptyIfNull() )
            .Select( t => (GeneratedTypeInfo) new GeneratedTypeInfoImpl( t ) )
            .ToArray();
         var sideEffectInvocationGenerationResults = typeAttributes
            .SelectMany( a => a.SideEffectInvocationHandlerTypes.EmptyIfNull() )
            .Select( t => (GeneratedTypeInfo) new GeneratedTypeInfoImpl( t ) )
            .ToArray();
         var privateCompositeGenerationresults = typeAttributes
            .SelectMany( a => a.PrivateCompositeTypes.EmptyIfNull() )
            .Select( t => (GeneratedTypeInfo) new GeneratedTypeInfoImpl( t ) )
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

      public ListQuery<GeneratedTypeInfo> ConcernInvocationGenerationResults
      {
         get
         {
            return this._concernInvocationGenerationResults;
         }
      }

      public ListQuery<GeneratedTypeInfo> SideEffectGenerationResults
      {
         get
         {
            return this._sideEffectInvocationGenerationResults;
         }
      }

      public ListQuery<GeneratedTypeInfo> PrivateCompositeGenerationResults
      {
         get
         {
            return this._privateCompositeGenerationResults;
         }
      }

      #endregion
   }
}
