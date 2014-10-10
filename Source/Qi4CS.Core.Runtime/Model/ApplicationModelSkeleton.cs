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

#if QI4CS_SDK
using CILAssemblyManipulator.API;
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
                  .SelectMany( tModel => tModel.Key.PublicTypes
                     .Concat( tModel.Value.PrivateCompositeTypeInfos.Keys )
                     .Concat( tModel.Value.FragmentTypeInfos.Keys )
                     .Concat( tModel.Value.ConcernInvocationTypeInfos.Keys )
                     .Concat( tModel.Value.SideEffectInvocationTypeInfos.Keys ) )
                     .Select( type => type.GetAssembly() )
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
         var assDic = new Dictionary<Assembly, Assembly>();
         var dic = this._collectionsFactory.NewDictionaryProxy( this._models.CQ.Values.ToDictionary( model => model, model => this._compositeModelTypeSupport[model.ModelType].LoadTypes( model, ( (CompositeValidationResultImmutable) validationResult.CompositeValidationResults[model] ).TypeModel, assDic ) ) ).CQ;
         this.ApplicationCodeResolveEvent.InvokeEventIfNotNull( evt => evt( this, new ApplicationCodeResolveArgs( dic, this.CollectionsFactory.NewDictionaryProxy( assDic ).CQ ) ) );
         var result = this.CreateNew( validationResult, applicationName, mode, version, dic );
         this.ApplicationInstanceCreatedEvent.InvokeEventIfNotNull( evt => evt( this, new ApplicationCreationArgs( result ) ) );
         return result;
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

      //public event EventHandler<ApplicationValidationArgs> ApplicationValidationEvent;

      //public event EventHandler<CompositeValidationArgs> CompositeValidationEvent;

      public event EventHandler<ApplicationCreationArgs> ApplicationInstanceCreatedEvent;

      public event EventHandler<ApplicationCodeResolveArgs> ApplicationCodeResolveEvent;

      #endregion

      //protected internal EventHandler<CompositeValidationArgs> CompositeValidationEventProperty
      //{
      //   get
      //   {
      //      return this.CompositeValidationEvent;
      //   }
      //}

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

         //TypeUtil.InvokeEventIfNotNull( this.ApplicationValidationEvent, evt => evt( this, new ApplicationValidationArgs( result ) ) );

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

      public DictionaryQuery<Assembly, CILAssemblyManipulator.API.CILAssembly> GenerateCode( CILReflectionContext reflectionContext, Boolean isSilverlight )
      {
         var validationResult = this.ValidationResult;
         CheckValidation( validationResult, "Tried to emit code based on application model with validation errors." );

         IDictionary<CompositeModel, CompositeEmittingInfo> cResults;
         var assDic = this.PerformEmitting( isSilverlight, reflectionContext, out cResults );

         this.ApplicationCodeGenerationEvent.InvokeEventIfNotNull( evt => evt( this, new ApplicationCodeGenerationArgs(
            this.CollectionsFactory.NewDictionaryProxy( cResults.ToDictionary(
            kvp => kvp.Key,
            kvp => this.CollectionsFactory.NewDictionaryProxy(
               kvp.Value.GetAllPublicComposites( kvp.Key ).ToDictionary( tuple => tuple.Item1, tuple => tuple.Item2.Builder )
               ).CQ
            ) ).CQ
            ) ) );

         return this.CollectionsFactory.NewDictionaryProxy( assDic.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Assembly ) ).CQ;
      }

      #endregion

      //      private static readonly ConstructorInfo DEBUGGABLE_ATTRIBUTE_CTOR = TypeUtil.LoadConstructorOrThrow( typeof( DebuggableAttribute ), new Type[] { typeof( DebuggableAttribute.DebuggingModes ) } );
      private static readonly ConstructorInfo ASS_TITLE_ATTRIBUTE_CTOR = typeof( AssemblyTitleAttribute ).LoadConstructorOrThrow( new Type[] { typeof( String ) } );
      private static readonly ConstructorInfo ASS_DESCRIPTION_ATTRIBUTE_CTOR = typeof( AssemblyDescriptionAttribute ).LoadConstructorOrThrow( new Type[] { typeof( String ) } );
      //private static readonly ConstructorInfo ASS_DEFAULT_ALIAS_ATTRIBUTE_CTOR = typeof( AssemblyDefaultAliasAttribute ).LoadConstructorOrThrow( new Type[] { typeof( String ) } );
      private static readonly ConstructorInfo QI4CS_GENERATED_ATTRIBUTE_CTOR = typeof( Qi4CSGeneratedAssemblyAttribute ).LoadConstructorOrThrow( 0 );

      private IDictionary<Assembly, CILModule> PerformEmitting( Boolean isSilverlight, CILReflectionContext reflectionContext, out IDictionary<CompositeModel, CompositeEmittingInfo> cResultsOut )
      {
         var typeModelDic = this._typeModelDic.Value;
         var assembliesArray = this._affectedAssemblies.Value.ToArray();
         var models = this._models.CQ.Values.ToArray();
         var supports = this._compositeModelTypeSupport;
         var cResults = models.ToDictionary( muudel => muudel, muudel => new CompositeEmittingInfo( reflectionContext, models ) );
         cResultsOut = cResults;
         var codeGens = models
            .Select( muudel => supports[muudel.ModelType] )
            .Distinct()
            .ToDictionary( mt => mt.AssemblyScopeSupport.ModelType, mt => Tuple.Create( mt.NewCodeGenerator( isSilverlight, reflectionContext ), mt.CodeGenerationInfo ) );

         var assemblyDic = new Dictionary<Assembly, CILModule>();
         foreach ( var currentAssembly in assembliesArray )
         {
            if ( !Types.QI4CS_ASSEMBLY.Equals( currentAssembly ) )
            {
               var assemblyBareFileName = Qi4CSGeneratedAssemblyAttribute.GetGeneratedAssemblyName( currentAssembly );

               var ass = reflectionContext.NewBlankAssembly( assemblyBareFileName );
               var eAss = currentAssembly.NewWrapper( reflectionContext );
               ass.Name.MajorVersion = eAss.Name.MajorVersion;
               ass.Name.MinorVersion = eAss.Name.MinorVersion;
               ass.Name.BuildNumber = eAss.Name.BuildNumber;
               ass.Name.Revision = eAss.Name.Revision;
               ass.Name.Culture = eAss.Name.Culture;

               ass.AddNewCustomAttributeTypedParams( ASS_TITLE_ATTRIBUTE_CTOR.NewWrapper( reflectionContext ), CILCustomAttributeFactory.NewTypedArgument( assemblyBareFileName, reflectionContext ) );
               ass.AddNewCustomAttributeTypedParams( ASS_DESCRIPTION_ATTRIBUTE_CTOR.NewWrapper( reflectionContext ), CILCustomAttributeFactory.NewTypedArgument( ( assemblyBareFileName + " Enhanced by Qi4CS." ), reflectionContext ) );
               //ass.AddNewCustomAttributeTypedParams( ASS_DEFAULT_ALIAS_ATTRIBUTE_CTOR.NewWrapper( reflectionContext ), CILCustomAttributeFactory.NewTypedArgument( assemblyBareFileName, reflectionContext ) );
               ass.AddNewCustomAttributeTypedParams( QI4CS_GENERATED_ATTRIBUTE_CTOR.NewWrapper( reflectionContext ) );

               var mod = ass.AddModule( assemblyBareFileName + ".dll" );
               assemblyDic.Add( currentAssembly, mod );
            }
         }

         // Phase 1: Emit empty types
         System.Threading.Tasks.Parallel.ForEach( assembliesArray, currentAssembly =>
         {
            foreach ( var model in models )
            {
               var tuple1 = codeGens[model.ModelType];
               var tuple2 = cResults[model];
               tuple1.Item1.EmitTypesForModel( model, typeModelDic[model], currentAssembly, GetEmittingModule( model, assemblyDic, currentAssembly ), tuple1.Item2, tuple2 );
            }
         } );

         // Phase 2: Emit fragment methods
         System.Threading.Tasks.Parallel.ForEach( assembliesArray, currentAssembly =>
         {
            foreach ( var model in models )
            {
               var tuple1 = codeGens[model.ModelType];
               var tuple2 = cResults[model];
               tuple1.Item1.EmitFragmentMethods( model, typeModelDic[model], currentAssembly, tuple1.Item2, tuple2 );
            }
         } );

         // Phase 3: Emit composite methods and concern & side-effect invocation types
         System.Threading.Tasks.Parallel.ForEach( assembliesArray, currentAssembly =>
         {
            foreach ( var model in models )
            {
               var tuple1 = codeGens[model.ModelType];
               var tuple2 = cResults[model];
               tuple1.Item1.EmitCompositeMethosAndInvocationInfos( model, typeModelDic[model], currentAssembly, tuple1.Item2, tuple2 );
            }
         } );

         // Phase 4: Emit all composite extra methods (state check, pre-prototype, etc)
         System.Threading.Tasks.Parallel.ForEach( assembliesArray, currentAssembly =>
         {
            foreach ( var model in models )
            {
               var tuple1 = codeGens[model.ModelType];
               var tuple2 = cResults[model];
               tuple1.Item1.EmitCompositeExtraMethods( model, typeModelDic[model], currentAssembly, tuple1.Item2, tuple2 );
            }
         } );

         // Phase 5: Emit all composite constructors
         System.Threading.Tasks.Parallel.ForEach( assembliesArray, currentAssembly =>
         {
            foreach ( var model in models )
            {
               var tuple1 = codeGens[model.ModelType];
               var tuple2 = cResults[model];
               tuple1.Item1.EmitCompositeConstructors( model, typeModelDic[model], currentAssembly, tuple1.Item2, tuple2 );
            }
         } );

         // Phase 6: Emit all composite factory types
         System.Threading.Tasks.Parallel.ForEach( assembliesArray, currentAssembly =>
         {
            foreach ( var model in models )
            {
               var tuple1 = codeGens[model.ModelType];
               var tuple2 = cResults[model];
               tuple1.Item1.EmitCompositeFactory( model, currentAssembly, GetEmittingModule( model, assemblyDic, currentAssembly ), tuple1.Item2, tuple2 );
            }
         } );

         return assemblyDic;
      }

      private static CILModule GetEmittingModule( CompositeModel cModel, Dictionary<Assembly, CILModule> dic, Assembly assembly )
      {
         return Types.QI4CS_ASSEMBLY.Equals( assembly ) ? dic[cModel.MainCodeGenerationType.Assembly] : dic[assembly];
      }

#endif
   }
}
