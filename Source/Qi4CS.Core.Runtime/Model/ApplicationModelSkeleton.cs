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
using System.Linq;
using System.Collections.Generic;
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.SPI.Instance;
using System.Reflection;
using Qi4CS.Core.API.Model;
using System.Diagnostics;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.Runtime.Model;

#if QI4CS_SDK
using CILAssemblyManipulator.Logical;
using System.Threading.Tasks;
#if !SILVERLIGHT
using System.Collections.Concurrent;
#endif
#endif

namespace Qi4CS.Core.Runtime.Model
{

   public abstract class ApplicationModelSkeletonImmutable<TApplicationInstance> : ApplicationModel<TApplicationInstance>
      where TApplicationInstance : ApplicationSPI
   {

      private readonly InjectionService _injectionService;

      private readonly Type _genericCompositePropertyMixin;

      private readonly Type _genericCompositeEventMixin;

      private readonly Type _genericFragmentBaseType;

      private readonly DictionaryQuery<CompositeModelType, CompositeModelTypeModelScopeSupport> _compositeModelTypeSupport;

      private readonly DictionaryProxy<Int32, CompositeModel> _models;

      private readonly CollectionsFactory _collectionsFactory;

      private readonly Lazy<ApplicationValidationResultIQ> _validationResult;

      private readonly Lazy<DictionaryQuery<CompositeModel, CompositeTypeModel>> _typeModelDic;

      private readonly Lazy<SetQuery<Assembly>> _affectedAssemblies;

      protected ApplicationModelSkeletonImmutable(
         ApplicationArchitecture<ApplicationModel<ApplicationSPI>> architecture,
         Type genericCompositePropertyMixin,
         Type genericCompositeEventMixin,
         Type genericFragmentBaseType,
         DictionaryQuery<CompositeModelType, CompositeModelTypeAssemblyScopeSupport> modelTypeAssemblyScopeSupport,
         out DictionaryQuery<CompositeModelType, CompositeModelTypeModelScopeSupport> compositeModelTypeSupport,
         out DictionaryProxy<Int32, CompositeModel> models
         )
      {
         ArgumentValidator.ValidateNotNull( "Application architecture", architecture );
         ArgumentValidator.ValidateNotNull( "Generic composite property mixin", genericCompositePropertyMixin );
         ArgumentValidator.ValidateNotNull( "Generic composite event mixin", genericCompositeEventMixin );
         ArgumentValidator.ValidateNotNull( "Generic fragment base type", genericFragmentBaseType );

         this._injectionService = new InjectionServiceImpl();
         this._genericCompositePropertyMixin = genericCompositePropertyMixin;
         this._genericCompositeEventMixin = genericCompositeEventMixin;
         this._genericFragmentBaseType = genericFragmentBaseType;
         this._collectionsFactory = architecture.CollectionsFactory;
         this._compositeModelTypeSupport = this._collectionsFactory.NewDictionaryProxy<CompositeModelType, CompositeModelTypeModelScopeSupport>( modelTypeAssemblyScopeSupport.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.CreateModelScopeSupport() ) ).CQ;
         this._models = this._collectionsFactory.NewDictionaryProxy<Int32, CompositeModel>();
         this._validationResult = new Lazy<ApplicationValidationResultIQ>( this.DoValidate, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
         compositeModelTypeSupport = this._compositeModelTypeSupport;
         models = this._models;
         this._typeModelDic = new Lazy<DictionaryQuery<CompositeModel, CompositeTypeModel>>( () =>
            this._collectionsFactory.NewDictionaryProxy( this.CompositeModels.Values
            .Select( cModel => Tuple.Create( cModel, ( (CompositeValidationResultImmutable) this._validationResult.Value.CompositeValidationResults[cModel] ).TypeModel ) )
            .ToDictionary( tuple => tuple.Item1, tuple => tuple.Item2, ReferenceEqualityComparer<CompositeModel>.ReferenceBasedComparer ) ).CQ
         , System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
         this._affectedAssemblies = new Lazy<SetQuery<Assembly>>( () =>
            this._collectionsFactory.NewSetProxy( new HashSet<Assembly>(
               this._typeModelDic.Value
                  .SelectMany( tModel => tModel.Value.GetAllCodeGenerationRelatedAssemblies( tModel.Key ) )
            ) ).CQ
         , System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      #region ApplicationModel<ApplicationValidationResultType,ApplicationInstanceType> Members

      public ApplicationValidationResultIQ ValidationResult
      {
         get
         {
            return this._validationResult.Value;
         }
      }

      public TApplicationInstance NewInstance( String applicationName, String mode, String version )
      {
         var validationResult = this.ValidationResult;

         CheckValidation( validationResult, "Tried to create new application instance from model with validation errors." );
         var assDic = this.AffectedAssemblies
            .Where( a => !ReflectionHelper.IsQi4CSAssembly( a ) )
            .ToDictionary( a => a, a => this.LoadQi4CSGeneratedAssembly( a ) );
         var attrDic = assDic.Values
            .SelectMany( a => a.GetCustomAttributes().OfType<CompositeTypesAttribute>() )
            .GroupBy( a => this.CompositeModels[a.CompositeID] )
            .ToDictionary( g => g.Key, g => g.ToList() );

         var dic = this._collectionsFactory.NewDictionaryProxy( this._models.CQ.Values.ToDictionary(
            model => model,
            model => this._compositeModelTypeSupport[model.ModelType].LoadTypes(
               model,
               attrDic[model]
               ) )
            ).CQ;
         this.ApplicationCodeResolveEvent.InvokeEventIfNotNull( evt => evt( this, new ApplicationCodeResolveArgs( dic, this.CollectionsFactory.NewDictionaryProxy( assDic ).CQ ) ) );
         var result = this.CreateNew( validationResult, applicationName, mode, version, dic );
         this.ApplicationInstanceCreatedEvent.InvokeEventIfNotNull( evt => evt( this, new ApplicationCreationArgs( result ) ) );
         return result;
      }

      private Assembly LoadQi4CSGeneratedAssembly( Assembly originalAssembly )
      {
         var args = new AssemblyLoadingArgs( originalAssembly.FullName, Qi4CSGeneratedAssemblyAttribute.GetGeneratedAssemblyName( originalAssembly ) );
         this.GeneratedAssemblyLoadingEvent.InvokeEventIfNotNull( evt => evt( this, args ) );

         var an = args.Qi4CSGeneratedAssemblyName;
         if ( args.Version != null )
         {
            an += ", Version=" + args.Version;
         }

         if ( !String.IsNullOrEmpty( args.Culture ) )
         {
            an += ", Culture=" + args.Culture;
         }

         if ( !args.PublicKey.IsNullOrEmpty() )
         {
            an += ", PublicKey=" + args.PublicKey.CreateHexString();
         }
         else if ( !args.PublicKeyToken.IsNullOrEmpty() )
         {
            an += ", PublicKeyToken=" + args.PublicKeyToken.CreateHexString();
         }

         return Assembly.Load(
#if WINDOWS_PHONE_APP
               new AssemblyName(
#endif
 an
#if WINDOWS_PHONE_APP
               )
#endif
 );
      }

      public InjectionService InjectionService
      {
         get
         {
            return this._injectionService;
         }
      }

      public Type GenericPropertyMixinType
      {
         get
         {
            return this._genericCompositePropertyMixin;
         }
      }

      public Type GenericEventMixinType
      {
         get
         {
            return this._genericCompositeEventMixin;
         }
      }

      public Type GenericFragmentBaseType
      {
         get
         {
            return this._genericFragmentBaseType;
         }
      }

      public CollectionsFactory CollectionsFactory
      {
         get
         {
            return this._collectionsFactory;
         }
      }

      public DictionaryQuery<Int32, CompositeModel> CompositeModels
      {
         get
         {
            return this._models.CQ;
         }
      }

      public event EventHandler<ApplicationCreationArgs> ApplicationInstanceCreatedEvent;

      public event EventHandler<ApplicationCodeResolveArgs> ApplicationCodeResolveEvent;

      public event EventHandler<AssemblyLoadingArgs> GeneratedAssemblyLoadingEvent;

      #endregion

      protected virtual ApplicationValidationResultMutable CreateEmptyValidationResult()
      {
         ApplicationValidationResultState state = new ApplicationValidationResultState( (ApplicationModel<ApplicationSPI>) this );
         return new ApplicationValidationResultMutable( state, new ApplicationValidationResultImmutable( state ) );
      }

      protected static void CheckValidation( ApplicationValidationResultIQ validationResult, String msg )
      {
         if ( validationResult.HasAnyErrors() )
         {
            throw new InvalidApplicationModelException( validationResult, msg );
         }
      }

      protected abstract TApplicationInstance CreateNew( ApplicationValidationResultIQ validationResult, String name, String mode, String version, DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> loadingResults );

      protected abstract void ValidateApplicationModel( ApplicationValidationResultMutable result );

      protected DictionaryQuery<CompositeModelType, CompositeModelTypeModelScopeSupport> CompositeModelTypeModelScopeSupports
      {
         get
         {
            return this._compositeModelTypeSupport;
         }
      }

      private ApplicationValidationResultIQ DoValidate()
      {
         var result = this.CreateEmptyValidationResult();

         this.ValidateApplicationModel( result );

         var supports = new HashSet<CompositeModelTypeModelScopeSupport>();
         if ( !result.IQ.HasAnyErrors() )
         {
            foreach ( var cModel in this.CompositeModels.Values )
            {
               var support = this._compositeModelTypeSupport[cModel.ModelType];
               result.CompositeValidationResults.Add( cModel, support.ValidateModel( cModel ) );
               supports.Add( support );
            }
         }

         if ( !result.IQ.HasAnyErrors() )
         {
            foreach ( var support in supports )
            {
               support.ValidateModelsInApplication( (ApplicationModel<ApplicationSPI>) this, result );
            }
         }

         return result.IQ;
      }

      public SetQuery<Assembly> AffectedAssemblies
      {
         get
         {
            return this._affectedAssemblies.Value;
         }
      }

#if QI4CS_SDK

      #region ApplicationModel<TApplicationInstance> Members

      public event EventHandler<ApplicationCodeGenerationArgs> ApplicationCodeGenerationEvent;

      public DictionaryQuery<Assembly, CILAssemblyManipulator.Logical.CILAssembly> GenerateCode(
         CILReflectionContext reflectionContext,
         Boolean parallelize,
         Boolean isSilverlight
         )
      {
         var validationResult = this.ValidationResult;
         CheckValidation( validationResult, "Tried to emit code based on application model with validation errors." );

         IDictionary<CompositeModel, IDictionary<Assembly, CILType[]>> cResults;
         var assDic = this.PerformEmitting( reflectionContext, parallelize, isSilverlight, out cResults );

         this.ApplicationCodeGenerationEvent.InvokeEventIfNotNull( evt => evt( this, new ApplicationCodeGenerationArgs(
            this.CollectionsFactory.NewDictionaryProxy( cResults.ToDictionary(
            kvp => kvp.Key,
            kvp => this.CollectionsFactory.NewDictionaryProxy( kvp.Value.ToDictionary( kvp2 => kvp2.Key, kvp2 => this.CollectionsFactory.NewListProxy( kvp2.Value.ToList() ).CQ ) ).CQ
            ) ).CQ
            ) ) );

         return this.CollectionsFactory.NewDictionaryProxy( assDic.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Assembly ) ).CQ;
      }

      #endregion

      private static readonly ConstructorInfo ASS_TITLE_ATTRIBUTE_CTOR = typeof( AssemblyTitleAttribute ).LoadConstructorOrThrow( new Type[] { typeof( String ) } );
      private static readonly ConstructorInfo ASS_DESCRIPTION_ATTRIBUTE_CTOR = typeof( AssemblyDescriptionAttribute ).LoadConstructorOrThrow( new Type[] { typeof( String ) } );
      private static readonly ConstructorInfo QI4CS_GENERATED_ATTRIBUTE_CTOR = typeof( Qi4CSGeneratedAssemblyAttribute ).LoadConstructorOrThrow( 0 );

      private IDictionary<Assembly, CILModule> PerformEmitting(
         CILReflectionContext reflectionContext,
         Boolean parallelize,
         Boolean isSilverlight,
         out IDictionary<CompositeModel, IDictionary<Assembly, CILType[]>> cResultsOut
         )
      {
         var typeModelDic = this._typeModelDic.Value;
         var assembliesArray = this._affectedAssemblies.Value.ToArray();
         var models = this._models.CQ.Values.ToArray();
         var supports = this._compositeModelTypeSupport;
         var cResults = new
#if SILVERLIGHT
         Dictionary<CompositeModel, IDictionary<Assembly, CILType[]>>()
#else
 ConcurrentDictionary<CompositeModel, IDictionary<Assembly, CILType[]>>()
#endif
;
         cResultsOut = cResults;
         var codeGens = models
            .Select( muudel => supports[muudel.ModelType] )
            .Distinct()
            .ToDictionary( mt => mt.AssemblyScopeSupport.ModelType, mt => mt.NewCodeGenerator( isSilverlight, reflectionContext ) );

         var assemblyDic = new Dictionary<Assembly, CILModule>();
         foreach ( var currentAssembly in assembliesArray )
         {
            if ( !ReflectionHelper.IsQi4CSAssembly( currentAssembly ) )
            {
               var assemblyBareFileName = Qi4CSGeneratedAssemblyAttribute.GetGeneratedAssemblyName( currentAssembly );

               var ass = reflectionContext.NewBlankAssembly( assemblyBareFileName );
               var eAss = reflectionContext.NewWrapper( currentAssembly );
               ass.Name.MajorVersion = eAss.Name.MajorVersion;
               ass.Name.MinorVersion = eAss.Name.MinorVersion;
               ass.Name.BuildNumber = eAss.Name.BuildNumber;
               ass.Name.Revision = eAss.Name.Revision;
               ass.Name.Culture = eAss.Name.Culture;

               ass.AddNewCustomAttributeTypedParams( reflectionContext.NewWrapper( ASS_TITLE_ATTRIBUTE_CTOR ), CILCustomAttributeFactory.NewTypedArgument( assemblyBareFileName, reflectionContext ) );
               ass.AddNewCustomAttributeTypedParams( reflectionContext.NewWrapper( ASS_DESCRIPTION_ATTRIBUTE_CTOR ), CILCustomAttributeFactory.NewTypedArgument( ( assemblyBareFileName + " Enhanced by Qi4CS." ), reflectionContext ) );
               ass.AddNewCustomAttributeTypedParams( reflectionContext.NewWrapper( QI4CS_GENERATED_ATTRIBUTE_CTOR ) );

               var mod = ass.AddModule( assemblyBareFileName + ".dll" );
               assemblyDic.Add( currentAssembly, mod );
            }
         }

         CodeGenUtils.DoPotentiallyInParallel(
            parallelize,
            models,
            model =>
            {
               var typeModel = typeModelDic[model];

               // Assemblies dictionary will get modified, so create a local copy of it
               // Also, assemblies not part of this model will not be visible
               var thisAssemblyDicCopy = typeModel.GetAllCodeGenerationRelatedAssemblies( model )
                 .Distinct()
                 .Where( a => !ReflectionHelper.IsQi4CSAssembly( a ) )
                 .ToDictionary( a => a, a => assemblyDic[a] );

               // Perform emitting
#if SILVERLIGHT
               lock( cResults )
               {
#endif
               cResults[model] = codeGens[model.ModelType].EmitCodeForCompositeModel( new CompositeModelEmittingArgs( model, typeModel, thisAssemblyDicCopy ) );
#if SILVERLIGHT
               }
#endif
            } );

         return assemblyDic;
      }

#endif


   }

#if QI4CS_SDK

   // TODO this class is temporary.
   public static class CodeGenUtils
   {
      // TODO move this method to UtilPack, with multiple variations something like this
      // ParallelHelper.ForEach
      // ParallelHelper.ForEachWithPartitioner
      // ParallelHelper.ForEachWithThreadLocal
      // ParallelHelper.ForEachWithThreadLocalAndPartitioner
      // ParallelHelper.ForEachGeneric <- all parameters can be specified
      public static Boolean DoPotentiallyInParallel<T>( Boolean parallelize, IEnumerable<T> enumerable, Action<T> action
#if !SILVERLIGHT
, Func<Partitioner<T>> partitionerCreator = null
#endif
 )
      {
         if ( parallelize )
         {
#if SILVERLIGHT
            // No Parallel in SL
            Task.WaitAll( enumerable.Select( item => Task.Factory.StartNew( () => action( item ) ) ).ToArray() );
#else
            Partitioner<T> partitioner;
            if ( partitionerCreator == null )
            {
               partitioner = Partitioner.Create( enumerable );
            }
            else
            {
               partitioner = partitionerCreator();
            }

            Parallel.ForEach( partitioner, action );
#endif
         }
         else
         {
            foreach ( var item in enumerable )
            {
               action( item );
            }
         }
         return parallelize;
      }

      public static Boolean DoPotentiallyInParallel( Boolean parallelize, Int32 fromInclusive, Int32 toExclusive, Action<Int32> action )
      {
         if ( parallelize )
         {

#if SILVERLIGHT
            // No Parallel in SL
            Task.WaitAll( Enumerable.Range( fromInclusive, toExclusive - fromInclusive ).Select( idx => Task.Factory.StartNew( () => action( idx ) ) ).ToArray() );
#else
            Parallel.For( fromInclusive, toExclusive, action );
#endif
         }
         else
         {
            for ( var i = fromInclusive; i < toExclusive; ++i )
            {
               action( i );
            }
         }

         return parallelize;
      }
   }

#endif
}

public static partial class E_Qi4CS
{
   public static IEnumerable<Type> GetAllCodeGenerationRelatedTypes( this CompositeTypeModel tModel, CompositeModel model )
   {
      return model.PublicTypes
         .Concat( tModel.FragmentTypeInfos.Keys )
         .Concat( tModel.PrivateCompositeTypeInfos.Keys )
         .Concat( tModel.ConcernInvocationTypeInfos.Keys )
         .Concat( tModel.SideEffectInvocationTypeInfos.Keys );
   }

   public static IEnumerable<Assembly> GetAllCodeGenerationRelatedAssemblies( this CompositeTypeModel tModel, CompositeModel model )
   {
      return tModel.GetAllCodeGenerationRelatedTypes( model ).Select( t => t.GetAssembly() );
   }
}