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
#if QI4CS_SDK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CILAssemblyManipulator.API;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using CommonUtils;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public interface EmittableWithGenericArguments
   {
      ListQuery<CILTypeParameter> GenericArguments { get; }
   }

   public interface EmittableWithBuilder<TBuilder>
   //where TBuilder : MemberInfo
   {
      TBuilder Builder { get; }
   }

   public interface TypeGenerationInfo : EmittableWithGenericArguments, EmittableWithBuilder<CILType>
   {
      DictionaryQuery<CILType, CILType> Parents { get; }
      void AddParent( CILType typeWithoutThisGArgs, CILType parent );
      Int32 GetNextLambdaClassID();
   }

   public interface AbstractTypeGenerationInfoForComposites : TypeGenerationInfo
   {
      CILField CompositeField { get; }
      IDictionary<CILMethod, CompositeMethodGenerationInfo> NormalMethodBuilders { get; }
      IDictionary<Int32, ConstructorGenerationInfo> ConstructorBuilders { get; }
      CompositeTypeGenerationInfo PublicCompositeTypeGen { get; }
   }

   public interface MethodBaseGenerationInfo<TBuilder> : EmittableWithGenericArguments, EmittableWithBuilder<TBuilder>
      where TBuilder : CILMethodBase
   {
      MethodIL IL { get; }
      ListQuery<CILParameter> Parameters { get; }
      LocalBuilder GetOrCreateLocal( LocalBuilderInfo info );
      LocalBuilder GetOrCreateLocal( LocalBuilderInfo info, CILTypeBase type );
      LocalBuilder GetLocalOrNull( LocalBuilderInfo info );
      LocalBuilder GetLocalOrThrow( LocalBuilderInfo info );
      LocalBuilder GetLocalRaw( String name );
      Boolean HasLocalRaw( String name );
      void AddLocalRaw( String name, LocalBuilder builder );
      Boolean TryGetLocal( LocalBuilderInfo info, out LocalBuilder result );
      Boolean HasLocal( LocalBuilderInfo info );
      LocalBuilder GetOrCreateLocalBasedOnType( CILTypeBase type );
   }

   public interface MethodGenerationInfo : MethodBaseGenerationInfo<CILMethod>
   {
      CILParameter ReturnParameter { get; }
      CILTypeBase ReturnType { get; }
   }

   public interface ConstructorGenerationInfo : MethodBaseGenerationInfo<CILConstructor>
   {

   }

   public abstract class GenerationInfoBase<TBuilder> : EmittableWithGenericArguments, EmittableWithBuilder<TBuilder>
      where TBuilder : class
   {
      private static readonly CILTypeParameter[] EMPTY_PARAMS = new CILTypeParameter[0];

      private readonly TBuilder _builder;
      private readonly ListQuery<CILTypeParameter> _gBuilders;

      protected GenerationInfoBase( TBuilder builder, Int32 amountOfGArgs, CollectionsFactory collectionsFactory, String genericArgPrefix, Func<TBuilder, String[], CILTypeParameter[]> gArgDef )
      {
         this._builder = builder;

         CILTypeParameter[] gArgs;
         if ( gArgDef != null && amountOfGArgs > 0 )
         {
            gArgs = gArgDef( builder, Enumerable.Repeat( genericArgPrefix, amountOfGArgs ).Select( ( name, idx ) => name + idx ).ToArray() );
         }
         else
         {
            gArgs = EMPTY_PARAMS;
         }
         this._gBuilders = collectionsFactory.NewListProxyFromParams( gArgs ).CQ;
      }

      protected GenerationInfoBase( TBuilder builder, CollectionsFactory cf, ListQuery<CILTypeBase> curGArgs )
      {
         this._builder = builder;
         this._gBuilders = cf.NewListProxy( curGArgs == null ? new List<CILTypeParameter>( EMPTY_PARAMS ) : curGArgs.Cast<CILTypeParameter>().ToList() ).CQ;
      }

      #region EmittableWithGenericArguments Members

      public ListQuery<CILTypeParameter> GenericArguments
      {
         get
         {
            return this._gBuilders;
         }
      }

      #endregion

      #region EmittableWithBuilder<TBuilder> Members

      public TBuilder Builder
      {
         get
         {
            return this._builder;
         }
      }

      #endregion
   }

   public class TypeGenerationInfoImpl : GenerationInfoBase<CILType>, TypeGenerationInfo
   {
      public const String GENERIC_ARGUMENT_NAME_PREFIX = "G";
      public const String METHOD_GENERIC_ARGUMENT_NAME_PREFIX = "MG";

      private readonly DictionaryProxy<CILType, CILType> _parents;
      private Int32 _currentLambdaClassCount;

      public TypeGenerationInfoImpl(
         CILType tb
         )
         : this( tb, 0 )
      {
      }

      public TypeGenerationInfoImpl(
         CILType tb,
         Int32 amountOfGArgs
         )
         : base( tb, amountOfGArgs, CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY, GENERIC_ARGUMENT_NAME_PREFIX, ( b, a ) => b.DefineGenericParameters( a ) )
      {
         this._parents = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewDictionaryProxy( new Dictionary<CILType, CILType>() );
         this._parents[(CILType) typeof( Object ).NewWrapper( tb.ReflectionContext )] = (CILType) typeof( Object ).NewWrapper( tb.ReflectionContext );
         this._parents[this.Builder] = this.Builder;
         this._currentLambdaClassCount = 0;
      }

      #region TypeGenerationInfo Members

      public DictionaryQuery<CILType, CILType> Parents
      {
         get
         {
            return this._parents.CQ;
         }
      }

      public virtual void AddParent( CILType typeWithoutThisGArgs, CILType parent )
      {
         if ( parent.Attributes.IsInterface() )
         {
            ResolveInterfaces( this.GenericArguments, parent, this._parents );
            this.Builder.AddDeclaredInterfaces( parent );
         }
         else if ( parent.Attributes.IsClass() )
         {
            CreateParentsDictionaryUsingBaseTypes( typeWithoutThisGArgs, this.Builder, parent, this.GenericArguments, this._parents );
            this.Builder.BaseType = parent;
         }
         else
         {
            throw new ArgumentException( "Only interfaces or classes allowed as parents." );
         }
      }

      public Int32 GetNextLambdaClassID()
      {
         return Interlocked.Increment( ref this._currentLambdaClassCount );
      }

      #endregion

      protected static CILType ConvertModelInterfaceToBuildingInterface( CILType iFaceFromModel, IEnumerable<CILTypeBase> baseTypeGArgs )
      {
         var result = iFaceFromModel;
         if ( iFaceFromModel.ContainsGenericParameters() )
         {
            var genDef = iFaceFromModel.GenericDefinition;
            result = genDef.MakeGenericType( iFaceFromModel.GenericArguments.Select( ( gArg, idx ) => TypeGenerationUtils.CreateTypeForEmitting( gArg, baseTypeGArgs, null ) ).ToArray() );
         }
         return result;
      }

      // The purpose of this method is that Type.GetInterfaces() throws exception when invoked on interface containing generic arguments of type builder.
      // Additionally, TypeBuilder's GetInterfaces() method returns only bottom-level interface instead of flattening hierarchy.
      protected static void ResolveInterfaces( IEnumerable<CILTypeParameter> gBuilders, CILType iFace, DictionaryProxy<CILType, CILType> result )
      {
         result.Add( iFace.GenericDefinitionIfGArgsHaveGenericParams(), iFace );
         if ( iFace.GArgsHaveGenericParams() )
         {
            foreach ( var parentIFace in iFace.GenericDefinition.GetAllImplementedInterfaces( false ) )
            {
               var next = parentIFace;
               if ( parentIFace.IsGenericType() )
               {
                  next = parentIFace.MakeGenericType( parentIFace.GenericArguments.Select( gArg => TypeGenerationUtils.CreateTypeForEmitting( gArg, gBuilders, null ) ).ToArray() );
               }

               if ( !result.CQ.ContainsKey( parentIFace ) )
               {
                  result.Add( parentIFace, next );
               }
            }
         }
         else
         {
            foreach ( var parentIFace in iFace.GetAllImplementedInterfaces( false ) )
            {
               if ( !result.CQ.ContainsKey( parentIFace ) )
               {
                  result.Add( parentIFace, parentIFace );
               }
            }
         }
      }

      protected static void CreateParentsDictionaryUsingBaseTypes( CILType typeInModel, CILType tb, CILType typeBuilderBaseType, IEnumerable<CILTypeParameter> gBuilders, DictionaryProxy<CILType, CILType> result )
      {
         var baseType = typeBuilderBaseType;
         var currentTypeInModel = typeInModel;
         while ( currentTypeInModel != null )
         {
            result[currentTypeInModel] = baseType;
            currentTypeInModel = currentTypeInModel.BaseType;
            var curGArgs = baseType.GenericArguments;
            baseType = baseType.BaseType;
            if ( baseType != null && baseType.ContainsGenericParameters() )
            {
               baseType = TypeGenerationUtils.CreateTypeForEmittingCILType( baseType, curGArgs, null );
            }
         }

         //         var genIFacesSet = new HashSet<CILType>();
         var parentGArgs = typeBuilderBaseType.GenericArguments;
         foreach ( var iFace in typeInModel.AsSingleBranchEnumerable( t => t.BaseType ).SelectMany( t => t.DeclaredInterfaces ) )
         {
            var tehFace = iFace;
            if ( tehFace.ContainsGenericParameters() )
            {
               tehFace = iFace.GenericDefinition;
            }
            foreach ( var tehFace2 in tehFace.GetAllImplementedInterfaces( true ) )
            {
               if ( !result.CQ.ContainsKey( tehFace2 ) )
               {
                  result.Add( tehFace2, ConvertModelInterfaceToBuildingInterface( tehFace2, parentGArgs ) );
               }
            }
         }
      }
   }


   public abstract class AbstractTypeGenerationInfoForCompositesImpl : TypeGenerationInfoImpl, AbstractTypeGenerationInfoForComposites
   {
      private readonly IDictionary<CILMethod, CompositeMethodGenerationInfo> _methodBuilders;
      private readonly IDictionary<Int32, ConstructorGenerationInfo> _constructorBuilders;
      private readonly CILField _compositeField;
      private readonly CompositeTypeGenerationInfo _publicCompositeTypeGen;

      public AbstractTypeGenerationInfoForCompositesImpl(
         CILType tb,
         Int32 amountOfGArgs,
         CILField compositeField,
         CompositeTypeGenerationInfo publicCompositeTypeGen
         )
         : base( tb, amountOfGArgs )
      {
         this._methodBuilders = new Dictionary<CILMethod, CompositeMethodGenerationInfo>();
         this._constructorBuilders = new Dictionary<Int32, ConstructorGenerationInfo>();
         this._compositeField = compositeField;
         this._publicCompositeTypeGen = publicCompositeTypeGen ?? this as CompositeTypeGenerationInfo;
      }

      #region AbstractTypeGenerationInfoForComposites Members

      public CILField CompositeField
      {
         get
         {
            return this._compositeField;
         }
      }

      public IDictionary<CILMethod, CompositeMethodGenerationInfo> NormalMethodBuilders
      {
         get
         {
            return this._methodBuilders;
         }
      }

      public IDictionary<Int32, ConstructorGenerationInfo> ConstructorBuilders
      {
         get
         {
            return this._constructorBuilders;
         }
      }

      public CompositeTypeGenerationInfo PublicCompositeTypeGen
      {
         get
         {
            return this._publicCompositeTypeGen;
         }
      }

      #endregion
   }

   public interface CompositeTypeGenerationInfo : AbstractTypeGenerationInfoForComposites
   {
      CompositeModel CompositeModel { get; }
      IDictionary<EventInfo, EventGenerationInfo> AutoGeneratedEventInfos { get; }
      IDictionary<PropertyInfo, PropertyGenerationInfo> AutoGeneratedPropertyInfos { get; }
      IDictionary<EventInfo, CILEvent> RawEventInfos { get; }
      IDictionary<PropertyInfo, CILProperty> RawPropertyInfos { get; }
      IDictionary<AbstractInjectableModel, Tuple<CILType, CILConstructor, CILMethod, CILField, CILField, CILField, IList<CILField>>> LazyInjectionLambdaClasses { get; }
      Boolean IsMainCompositeType { get; }
      MethodGenerationInfo CheckStateMethod { get; set; }
      MethodGenerationInfo PrePrototypeMethod { get; set; }
   }

   public class CompositeTypeGenerationInfoImpl : AbstractTypeGenerationInfoForCompositesImpl, CompositeTypeGenerationInfo
   {
      private readonly CompositeModel _model;
      private readonly IDictionary<EventInfo, CILEvent> _rawEventInfos;
      private readonly IDictionary<EventInfo, EventGenerationInfo> _autoGeneratedEventInfos;
      private readonly IDictionary<PropertyInfo, CILProperty> _rawPropertyInfos;
      private readonly IDictionary<PropertyInfo, PropertyGenerationInfo> _autoGeneratedPropertyInfos;
      private readonly Lazy<IDictionary<AbstractInjectableModel, Tuple<CILType, CILConstructor, CILMethod, CILField, CILField, CILField, IList<CILField>>>> _lazyInjectionLambdaClasses;

      private readonly Boolean _isMainCompositeType;
      private MethodGenerationInfo _checkStateMethod;
      private MethodGenerationInfo _prePrototypeMethod;

      public CompositeTypeGenerationInfoImpl(
         CILType tb,
         Int32 amountOfGArgs,
         CILField compositeField,
         CompositeModel model,
         CompositeTypeGenerationInfo publicCompositeTypeGen,
         Boolean isMainCompositeType
         )
         : base( tb, amountOfGArgs, compositeField, publicCompositeTypeGen )
      {
         ArgumentValidator.ValidateNotNull( "Composite model", model );

         this._model = model;
         this._autoGeneratedEventInfos = new Dictionary<EventInfo, EventGenerationInfo>();
         this._rawEventInfos = new Dictionary<EventInfo, CILEvent>();
         this._autoGeneratedPropertyInfos = new Dictionary<PropertyInfo, PropertyGenerationInfo>();
         this._rawPropertyInfos = new Dictionary<PropertyInfo, CILProperty>();
         this._lazyInjectionLambdaClasses = new Lazy<IDictionary<AbstractInjectableModel, Tuple<CILType, CILConstructor, CILMethod, CILField, CILField, CILField, IList<CILField>>>>( () => new Dictionary<AbstractInjectableModel, Tuple<CILType, CILConstructor, CILMethod, CILField, CILField, CILField, IList<CILField>>>(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );

         this._isMainCompositeType = publicCompositeTypeGen == null && isMainCompositeType;
      }

      public IDictionary<EventInfo, EventGenerationInfo> AutoGeneratedEventInfos
      {
         get
         {
            return this._autoGeneratedEventInfos;
         }
      }

      public IDictionary<EventInfo, CILEvent> RawEventInfos
      {
         get
         {
            return this._rawEventInfos;
         }
      }

      public IDictionary<PropertyInfo, PropertyGenerationInfo> AutoGeneratedPropertyInfos
      {
         get
         {
            return this._autoGeneratedPropertyInfos;
         }
      }

      public IDictionary<PropertyInfo, CILProperty> RawPropertyInfos
      {
         get
         {
            return this._rawPropertyInfos;
         }
      }

      public IDictionary<AbstractInjectableModel, Tuple<CILType, CILConstructor, CILMethod, CILField, CILField, CILField, IList<CILField>>> LazyInjectionLambdaClasses
      {
         get
         {
            return this._lazyInjectionLambdaClasses.Value;
         }
      }



      public Boolean IsMainCompositeType
      {
         get
         {
            return this._isMainCompositeType;
         }
      }

      public CompositeModel CompositeModel
      {
         get
         {
            return this._model;
         }
      }

      public MethodGenerationInfo CheckStateMethod
      {
         get
         {
            return this._checkStateMethod;
         }
         set
         {
            this._checkStateMethod = value;
         }
      }

      public MethodGenerationInfo PrePrototypeMethod
      {
         get
         {
            return this._prePrototypeMethod;
         }
         set
         {
            this._prePrototypeMethod = value;
         }
      }
   }

   public interface FragmentTypeGenerationInfo : AbstractTypeGenerationInfoForComposites
   {
      Boolean IsInstancePoolRequired { get; }
      IDictionary<MethodInfo, CompositeMethodGenerationInfo> SpecialMethodBuilders { get; }
      CILType DirectBaseFromModel { get; }
   }

   public class FragmentTypeGenerationInfoImpl : AbstractTypeGenerationInfoForCompositesImpl, FragmentTypeGenerationInfo
   {
      private readonly IDictionary<MethodInfo, CompositeMethodGenerationInfo> _specialMethodBuilders;
      private readonly Boolean _instancePoolRequired;
      private CILType _typeFromModel;

      public FragmentTypeGenerationInfoImpl(
         CILType tb,
         Int32 amountOfGArgs,
         CILField compositeField,
         Boolean instancePoolRequired,
         CompositeTypeGenerationInfo publicCompositeTypeGen
         )
         : base( tb, amountOfGArgs, compositeField, publicCompositeTypeGen )
      {
         ArgumentValidator.ValidateNotNull( "Public composite type generation info", publicCompositeTypeGen );

         this._specialMethodBuilders = new Dictionary<MethodInfo, CompositeMethodGenerationInfo>();

         this._instancePoolRequired = instancePoolRequired;
      }

      #region FragmentTypeGenerationInfo Members

      public Boolean IsInstancePoolRequired
      {
         get
         {
            return this._instancePoolRequired;
         }
      }

      public IDictionary<MethodInfo, CompositeMethodGenerationInfo> SpecialMethodBuilders
      {
         get
         {
            return this._specialMethodBuilders;
         }
      }

      public CILType DirectBaseFromModel
      {
         get
         {
            return this._typeFromModel;
         }
      }

      #endregion

      public override void AddParent( CILType typeWithoutThisGArgs, CILType parent )
      {
         base.AddParent( typeWithoutThisGArgs, parent );
         this._typeFromModel = typeWithoutThisGArgs;
      }
   }

   public class TypeCreationArgs : EventArgs
   {
      private readonly AbstractTypeGenerationInfoForComposites _typeGenerationInfo;

      public TypeCreationArgs( AbstractTypeGenerationInfoForComposites genInfo )
      {
         ArgumentValidator.ValidateNotNull( "Type generation info", genInfo );

         this._typeGenerationInfo = genInfo;
      }

      public AbstractTypeGenerationInfoForComposites TypeGenerationInfo
      {
         get
         {
            return this._typeGenerationInfo;
         }
      }
   }

   public interface CompositeMethodBaseGenerationInfo<TBuilder> : MethodBaseGenerationInfo<TBuilder>
      where TBuilder : CILMethodBase
   {
      TBuilder OverriddenMethod { get; }
      TBuilder MethodFromModel { get; }
      Boolean HasByRefParameters { get; }
   }

   public interface CompositeMethodGenerationInfo : CompositeMethodBaseGenerationInfo<CILMethod>, MethodGenerationInfo
   {
      AbstractTypeGenerationInfoForComposites DeclaringTypeGenInfo { get; }
   }

   public static class CompositeTypeGenerationUtils
   {
      public static void DoCompositeMethodOrConstructorGenerationInfoCtor<TEInfo, TInfo>(
         ref TEInfo methodToOverride,
         ref TInfo methodFromModel,
         ref Boolean hasByRefParams,
         TEInfo methodToOverrideParam,
         TInfo methodFromModelParam
         )
         where TEInfo : CILMethodBase
      {
         methodToOverride = methodToOverrideParam;
         methodFromModel = methodFromModelParam;
         hasByRefParams = methodToOverrideParam != null && methodToOverrideParam.Parameters.Any( param => param.ParameterType.IsByRef() );
      }
   }

   public abstract class MethodBaseGenerationInfoImpl<TBuilder> : GenerationInfoBase<TBuilder>, MethodBaseGenerationInfo<TBuilder>
      where TBuilder : class, CILMethodBase
   {

      private const String AUTOGENERATED_PREFIX = "AutoGen";

      private readonly IDictionary<String, LocalBuilder> _locals;
      //      private readonly MethodIL _il;

      protected MethodBaseGenerationInfoImpl(
         TBuilder mb,
         Int32 amountOfGArgs,
         Func<TBuilder, String[], CILTypeParameter[]> gArgDef
         )
         : base( mb, amountOfGArgs, CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY, TypeGenerationInfoImpl.METHOD_GENERIC_ARGUMENT_NAME_PREFIX, gArgDef )
      {
         this._locals = new Dictionary<String, LocalBuilder>();
         //         this._il = mb.MethodIL;
      }

      protected MethodBaseGenerationInfoImpl( TBuilder mb, ListQuery<CILTypeBase> curGArgs )
         : base( mb, CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY, curGArgs )
      {
         this._locals = new Dictionary<String, LocalBuilder>();
         //         this._il = mb.MethodIL;
      }

      public MethodIL IL
      {
         get
         {
            return this.Builder.MethodIL;
         }
      }

      public ListQuery<CILParameter> Parameters
      {
         get
         {
            return this.Builder.Parameters;
         }
      }

      public LocalBuilder GetOrCreateLocal( LocalBuilderInfo info )
      {
         return this.GetOrCreateLocal( info.Name, info.Type.NewWrapper( this.Builder.ReflectionContext ) );
      }

      public LocalBuilder GetOrCreateLocal( LocalBuilderInfo info, CILTypeBase type )
      {
         return this.GetOrCreateLocal( info.Name, type );
      }

      public LocalBuilder GetOrCreateLocal( String name, CILTypeBase type )
      {
         ArgumentValidator.ValidateNotNull( "Local variable type", type );
         LocalBuilder result = null;
         if ( !this._locals.TryGetValue( name, out result ) )
         {
            result = this.IL.DeclareLocal( type );
            this._locals.Add( name, result );
         }

         return result;
      }

      public LocalBuilder GetLocalOrNull( LocalBuilderInfo info )
      {
         LocalBuilder result;
         if ( !this._locals.TryGetValue( info.Name, out result ) )
         {
            result = null;
         }
         return result;
      }

      public LocalBuilder GetLocalOrThrow( LocalBuilderInfo info )
      {
         LocalBuilder result = GetLocalOrNull( info );
         if ( result == null )
         {
            throw new ArgumentException( "Could not find local builder for " + info.Name + "." );
         }
         return result;
      }

      public LocalBuilder GetLocalRaw( String name )
      {
         return this._locals[name];
      }

      public Boolean HasLocalRaw( String name )
      {
         return this._locals.ContainsKey( name );
      }

      public void AddLocalRaw( String name, LocalBuilder builder )
      {
         this._locals.Add( name, builder );
      }

      public Boolean TryGetLocal( LocalBuilderInfo info, out LocalBuilder result )
      {
         return this._locals.TryGetValue( info.Name, out result );
      }

      public Boolean HasLocal( LocalBuilderInfo info )
      {
         return this._locals.ContainsKey( info.Name );
      }

      public LocalBuilder GetOrCreateLocalBasedOnType( CILTypeBase type )
      {
         LocalBuilder result = this._locals.Values.FirstOrDefault( value => value.LocalType.Equals( type ) );
         if ( result == null )
         {
            result = this.IL.DeclareLocal( type );
            this._locals.Add( AUTOGENERATED_PREFIX + this._locals.Count, result );
         }
         return result;
      }

      public MethodBaseGenerationInfoImpl<TBuilder> WithParameters( IEnumerable<Tuple<CILTypeBase, CILAssemblyManipulator.API.ParameterAttributes>> paramTypes )
      {
         foreach ( var pt in paramTypes )
         {
            this.Builder.AddParameter( null, pt.Item2, pt.Item1 );
         }
         return this;
      }
   }

   public class MethodGenerationInfoImpl : MethodBaseGenerationInfoImpl<CILMethod>, MethodGenerationInfo
   {
      public MethodGenerationInfoImpl(
         CILMethod mb,
         Int32 amountOfGArgs )
         : base( mb, amountOfGArgs, ( b, a ) => b.DefineGenericParameters( a ) )
      {

      }

      public MethodGenerationInfoImpl( CILMethod mb )
         : base( mb, mb.GenericArguments )
      {

      }


      #region MethodGenerationInfo Members

      public CILTypeBase ReturnType
      {
         get
         {
            return this.Builder.ReturnParameter.ParameterType;
         }
      }

      #endregion

      #region MethodGenerationInfo Members

      public CILParameter ReturnParameter
      {
         get
         {
            return this.Builder.ReturnParameter;
         }
      }

      #endregion

      public MethodGenerationInfoImpl WithReturnType( CILTypeBase type )
      {
         this.Builder.ReturnParameter.ParameterType = type;
         return this;
      }
   }

   public class CompositeMethodGenerationInfoImpl : MethodGenerationInfoImpl, CompositeMethodGenerationInfo
   {
      private readonly CILMethod _methodToOverride;
      private readonly CILMethod _methodFromModel;
      private readonly Boolean _hasByRefParams;
      private readonly AbstractTypeGenerationInfoForComposites _declaringTypeGenInfo;

      public CompositeMethodGenerationInfoImpl(
         CILMethod mb,
         Int32 amountOfGArgs,
         CILMethod methodFromModel,
         CILMethod methodToOverride
         )
         : base( mb, amountOfGArgs )
      {
         CompositeTypeGenerationUtils.DoCompositeMethodOrConstructorGenerationInfoCtor(
            ref this._methodToOverride,
            ref this._methodFromModel,
            ref this._hasByRefParams,
            methodToOverride,
            methodFromModel
            );
      }

      public CompositeMethodGenerationInfoImpl(
         CILMethod mb,
         CILMethod methodFromModel,
         CILMethod methodToOverride,
         AbstractTypeGenerationInfoForComposites declaringTypeGenInfo = null )
         : base( mb )
      {
         CompositeTypeGenerationUtils.DoCompositeMethodOrConstructorGenerationInfoCtor(
            ref this._methodToOverride,
            ref this._methodFromModel,
            ref this._hasByRefParams,
            methodToOverride,
            methodFromModel
         );
         this._declaringTypeGenInfo = declaringTypeGenInfo;
      }

      public CILMethod OverriddenMethod
      {
         get
         {
            return this._methodToOverride;
         }
      }

      public CILMethod MethodFromModel
      {
         get
         {
            return this._methodFromModel;
         }
      }

      public Boolean HasByRefParameters
      {
         get
         {
            return this._hasByRefParams;
         }
      }

      public AbstractTypeGenerationInfoForComposites DeclaringTypeGenInfo
      {
         get
         {
            return this._declaringTypeGenInfo;
         }
      }
   }

   public interface CompositeConstructorGenerationInfo : CompositeMethodBaseGenerationInfo<CILConstructor>, ConstructorGenerationInfo
   {
      Int32 FirstAdditionalParamIndex { get; }
   }

   public class ConstructorGenerationInfoImpl : MethodBaseGenerationInfoImpl<CILConstructor>, ConstructorGenerationInfo
   {
      public ConstructorGenerationInfoImpl(
         CILType tb,
         CILAssemblyManipulator.API.MethodAttributes ctorAttributes,
         CILAssemblyManipulator.API.CallingConventions callingConventions,
         IEnumerable<Tuple<CILTypeBase, CILAssemblyManipulator.API.ParameterAttributes, String>> parameters
         )
         : base( tb.AddConstructor( ctorAttributes, callingConventions ), 0, null )
      {
         foreach ( var tuple in parameters )
         {
            this.Builder.AddParameter( tuple.Item3, tuple.Item2, tuple.Item1 );
         }
      }
   }

   public class CompositeConstructorGenerationInfoImpl : ConstructorGenerationInfoImpl, CompositeConstructorGenerationInfo
   {
      private readonly CILConstructor _methodToOverride;
      private readonly CILConstructor _methodFromModel;
      private readonly Boolean _hasByRefParams;
      private readonly Int32 _firstAdditionalParamIndex;

      public CompositeConstructorGenerationInfoImpl(
         CILAssemblyManipulator.API.MethodAttributes ctorAttributes,
         CILAssemblyManipulator.API.CallingConventions callingConventions,
         IEnumerable<Tuple<CILTypeBase, CILAssemblyManipulator.API.ParameterAttributes, String>> parameters,
         AbstractTypeGenerationInfoForComposites typeGenerationInfo,
         CILConstructor ctorFromModel,
         CILConstructor ctorToOverride,
         Int32 firstAdditionalParamIndex = 0
         )
         : base( typeGenerationInfo.Builder, ctorAttributes, callingConventions, parameters )
      {
         CompositeTypeGenerationUtils.DoCompositeMethodOrConstructorGenerationInfoCtor(
            ref this._methodToOverride,
            ref this._methodFromModel,
            ref this._hasByRefParams,
            ctorToOverride,
            ctorFromModel
            );
         this._firstAdditionalParamIndex = firstAdditionalParamIndex;
      }

      public CILConstructor OverriddenMethod
      {
         get
         {
            return this._methodToOverride;
         }
      }

      public CILConstructor MethodFromModel
      {
         get
         {
            return this._methodFromModel;
         }
      }

      public Boolean HasByRefParameters
      {
         get
         {
            return this._hasByRefParams;
         }
      }

      public Int32 FirstAdditionalParamIndex
      {
         get
         {
            return this._firstAdditionalParamIndex;
         }
      }
   }

   public class EventGenerationInfo : StateParticipantGenerationInfo
   {
      private readonly EventModel _eventModel;
      private readonly CILField _eventField;
      private readonly CILMethod _addMethod;
      private readonly CILMethod _removeMethod;
      private readonly CILMethod _invokeMethod;
      private readonly CILMethod _eventClearMethod;
      private readonly CILMethod _eventCheckerMethod;
      private readonly CILTypeBase _eventHandlerType;

      public EventGenerationInfo(
         CILMethod refInvoker,
         EventModel eventModel,
         CILTypeBase eventHandlerType,
         CILField eventField,
         CILMethod addMethod,
         CILMethod removeMethod,
         CILMethod invokeMethod,
         CILMethod eventClearMethod,
         CILMethod eventCheckerMethod
         )
         : base( refInvoker )
      {
         ArgumentValidator.ValidateNotNull( "Event model", eventModel );
         ArgumentValidator.ValidateNotNull( "Event handler type", eventHandlerType );
         ArgumentValidator.ValidateNotNull( "Event field", eventField );
         ArgumentValidator.ValidateNotNull( "Event addition method", addMethod );
         ArgumentValidator.ValidateNotNull( "Event removal method", removeMethod );
         ArgumentValidator.ValidateNotNull( "Method for event invocation", invokeMethod );
         ArgumentValidator.ValidateNotNull( "Method for clearing event field", eventClearMethod );
         ArgumentValidator.ValidateNotNull( "Method for checking event field", eventCheckerMethod );

         this._eventModel = eventModel;
         this._eventHandlerType = eventHandlerType;
         this._eventField = eventField;
         this._addMethod = addMethod;
         this._removeMethod = removeMethod;
         this._invokeMethod = invokeMethod;
         this._eventClearMethod = eventClearMethod;
         this._eventCheckerMethod = eventCheckerMethod;
      }

      public EventModel EventModel
      {
         get
         {
            return this._eventModel;
         }
      }

      public CILTypeBase EventHandlerType
      {
         get
         {
            return this._eventHandlerType;
         }
      }

      public CILField EventField
      {
         get
         {
            return this._eventField;
         }
      }

      public CILMethod AdditionMethod
      {
         get
         {
            return this._addMethod;
         }
      }

      public CILMethod RemovalMethod
      {
         get
         {
            return this._removeMethod;
         }
      }

      public CILMethod InvocationMethod
      {
         get
         {
            return this._invokeMethod;
         }
      }

      public CILMethod EventClearMethod
      {
         get
         {
            return this._eventClearMethod;
         }
      }

      public CILMethod EventCheckerMethod
      {
         get
         {
            return this._eventCheckerMethod;
         }
      }
   }

   public abstract class StateParticipantGenerationInfo
   {

      private readonly CILMethod _refInvokerMethod;

      public StateParticipantGenerationInfo( CILMethod refInvokerMethod )
      {
         ArgumentValidator.ValidateNotNull( "Ref invoker method", refInvokerMethod );

         this._refInvokerMethod = refInvokerMethod;
      }

      public CILMethod RefInvokerMethod
      {
         get
         {
            return this._refInvokerMethod;
         }
      }
   }

   public class PropertyGenerationInfo : StateParticipantGenerationInfo
   {
      private readonly PropertyModel _propertyModel;
      private readonly CILField _propertyField;
      private readonly CILMethod _getMethod;
      private readonly CILMethod _get32Method;
      private readonly CILMethod _setMethod;
      private readonly CILMethod _exchangeMethod;
      private readonly CILMethod _compareExchangeMethod;
      private readonly CILTypeBase _propertyType;

      public PropertyGenerationInfo(
         CILMethod refInvokerMethod,
         PropertyModel propertyModel,
         CILField propertyField,
         CILMethod getMethod,
         CILMethod get32Method,
         CILMethod setMethod,
         CILMethod exchangeMethod,
         CILMethod compareExchangeMethod,
         CILTypeBase propertyType
         )
         : base( refInvokerMethod )
      {
         ArgumentValidator.ValidateNotNull( "Property model", propertyModel );
         ArgumentValidator.ValidateNotNull( "Property field", propertyField );
         ArgumentValidator.ValidateNotNull( "Property getter method", getMethod );
         ArgumentValidator.ValidateNotNull( "Property setter method", setMethod );
         ArgumentValidator.ValidateNotNull( "Property type", propertyType );
         ArgumentValidator.ValidateNotNull( "Property exchange method", exchangeMethod );
         ArgumentValidator.ValidateNotNull( "Property compare exchange method", compareExchangeMethod );

         this._propertyModel = propertyModel;
         this._propertyField = propertyField;
         this._getMethod = getMethod;
         this._get32Method = get32Method;
         this._setMethod = setMethod;
         this._exchangeMethod = exchangeMethod;
         this._compareExchangeMethod = compareExchangeMethod;
         this._propertyType = propertyType;
      }

      public PropertyModel PropertyModel
      {
         get
         {
            return this._propertyModel;
         }
      }

      public CILField PropertyField
      {
         get
         {
            return this._propertyField;
         }
      }

      public CILMethod GetMethod
      {
         get
         {
            return this._getMethod;
         }
      }

      public CILMethod Get32Method
      {
         get
         {
            return this._get32Method;
         }
      }

      public CILMethod SetMethod
      {
         get
         {
            return this._setMethod;
         }
      }

      public CILMethod ExchangeMethod
      {
         get
         {
            return this._exchangeMethod;
         }
      }

      public CILMethod CompareExchangeMethod
      {
         get
         {
            return this._compareExchangeMethod;
         }
      }



      public CILTypeBase PropertyType
      {
         get
         {
            return this._propertyType;
         }
      }
   }

   public static class TypeGenerationUtils
   {
      private static readonly CILTypeBase[] EMPTY_TYPE_BASES = new CILTypeBase[0];

      public static CILTypeBase[] GetGenericArgumentsArray( this CILTypeBase type )
      {
         return TypeKind.TypeParameter == type.TypeKind || ( (CILType) type ).GenericArguments.Count == 0 ? EMPTY_TYPE_BASES : ( (CILType) type ).GenericArguments.ToArray();
      }

      public static Boolean IsPublicCompositeType( this CompositeTypeGenerationInfo typeGen )
      {
         return Object.ReferenceEquals( typeGen.PublicCompositeTypeGen, typeGen );
      }

      public static Boolean TryAdd_NotThreadSafe<TKey, TValue>( this IDictionary<TKey, TValue> dic, TKey key, TValue value )
      {
         var retVal = !dic.ContainsKey( key );
         if ( retVal )
         {
            dic.Add( key, value );
         }
         return retVal;
      }

      public static CILType CreateTypeForEmittingCILType( CILType paramType, IEnumerable<CILTypeBase> gBuilders, IEnumerable<CILTypeBase> mgBuilders )
      {
         return (CILType) CreateTypeForEmitting( paramType, gBuilders, mgBuilders );
      }

      public static CILTypeBase CreateTypeForEmitting( CILTypeBase paramType, IEnumerable<CILTypeBase> gBuilders, IEnumerable<CILTypeBase> mgBuilders )
      {
         var returnType = paramType;
         if ( returnType.TypeKind == TypeKind.TypeParameter )
         {
            var rp = (CILTypeParameter) returnType;
            var pos = rp.GenericParameterPosition;
            if ( rp.DeclaringMethod == null )
            {
               returnType = gBuilders is CollectionQueryWithIndexer<CILTypeBase> ? ( (CollectionQueryWithIndexer<CILTypeBase>) gBuilders )[pos] : ( (CILTypeBase[]) gBuilders )[pos];
            }
            else
            {
               returnType = mgBuilders is CollectionQueryWithIndexer<CILTypeBase> ? ( (CollectionQueryWithIndexer<CILTypeBase>) mgBuilders )[pos] : ( (CILTypeBase[]) mgBuilders )[pos];
            }
         }
         else
         {
            var rt = (CILType) returnType;
            if ( rt.IsGenericType() )
            {
               returnType = rt.MakeGenericType( rt.GenericArguments.Select( gArg => CreateTypeForEmitting( gArg, gBuilders, mgBuilders ) ).ToArray() );
            }
            else if ( rt.ContainsGenericParameters() )
            {
               var elements = new Stack<Tuple<ElementKind, GeneralArrayInfo>>();
               ElementKind? eKind;
               while ( ( eKind = returnType.GetElementKind() ).HasValue )
               {
                  elements.Push( Tuple.Create( eKind.Value, ( (CILType) returnType ).ArrayInformation ) );
                  returnType = ( (CILType) returnType ).ElementType;
               }
               returnType = CreateTypeForEmitting( returnType, gBuilders, mgBuilders );
               while ( elements.Any() )
               {
                  var element = elements.Pop();
                  returnType = returnType.MakeElementType( element.Item1, element.Item2 );
               }
            }
         }
         return returnType;
      }

      public static TMethod GetMethodForEmitting<TMethod>( DictionaryQuery<CILType, CILType> parents, TMethod method )
         where TMethod : CILMethodBase
      {
         return (TMethod) method.ChangeDeclaringTypeUT( parents[method.DeclaringType].GenericArguments.ToArray() );
      }

      public static TMethod GetMethodForEmitting<TMethod>( Func<CILType, CILType> decTypeTransformation, TMethod method )
         where TMethod : CILMethodBase
      {
         return (TMethod) method.ChangeDeclaringTypeUT( decTypeTransformation( method.DeclaringType ).GenericArguments.ToArray() );
      }

      public static CILField GetFieldForEmitting( DictionaryQuery<CILType, CILType> parents, CILField field )
      {
         return field.ChangeDeclaringType( parents[field.DeclaringType].GenericArguments.ToArray() );
      }

      public static CILType GenericDefinitionIfGArgsHaveGenericParams( this CILType type )
      {
         return type != null && GArgsHaveGenericParams( type ) ? type.GenericDefinition : type;
      }

      public static CILType GenericDefinitionIfGenericType( this CILType type )
      {
         return type != null && type.GenericArguments.Any() ? type.GenericDefinition : type;
      }

      public static Boolean GArgsHaveGenericParams( this CILType type )
      {
         return type.GenericArguments.Any( arg => TypeKind.TypeParameter == arg.TypeKind || GArgsHaveGenericParams( (CILType) arg ) );
      }

      public static void CopyGenericArgumentConstraintsExceptVariance( IEnumerable<CILTypeParameter> gBuilders, CILTypeParameter[] mgBuilders, CILTypeParameter gArg, CILTypeParameter gBuilder )
      {
         gBuilder.AddGenericParameterConstraints( gArg.GenericParameterConstraints.Select( constraint => CreateTypeForEmitting( constraint, gBuilders, mgBuilders ) ).ToArray() );
         gBuilder.Attributes = gArg.Attributes & CILAssemblyManipulator.API.GenericParameterAttributes.SpecialConstraintMask;
      }

      public static void CopyGenericArgumentConstraintsFromAll( IEnumerable<CILTypeParameter> allGBuilders, CILTypeParameter[] allMGBuilders, ListQuery<CILTypeBase> gArgs, CILTypeParameter[] gBuilders )
      {
         for ( Int32 idx = 0; idx < gArgs.Count; ++idx )
         {
            var gArg = gArgs[idx];
            CopyGenericArgumentConstraintsExceptVariance( allGBuilders, allMGBuilders, (CILTypeParameter) gArg, gBuilders[gBuilders.Length - gArgs.Count + idx] );
         }
      }

      public static Tuple<CILMethod, CILMethod> ImplementMethodForEmitting(
         TypeGenerationInfo typeGenInfo,
         CILMethod methodToCopy,
         String newName,
         CILAssemblyManipulator.API.MethodAttributes newAttributes
         )
      {
         return ImplementMethodForEmitting( typeGenInfo.Builder, typeGenInfo.GenericArguments, parent => typeGenInfo.Parents[parent], methodToCopy, newName, newAttributes );
      }

      public static Tuple<CILMethod, CILMethod> ImplementMethodForEmitting(
         CILType tb,
         IEnumerable<CILTypeParameter> tbGArgs,
         Func<CILType, CILType> parentFunc,
         CILMethod eMethodToCopy,
         String newName,
         CILAssemblyManipulator.API.MethodAttributes newAttributes
         )
      {
         CILTypeBase[] gArgs = null;
         var actualMethodToCopy = eMethodToCopy;

         if ( eMethodToCopy.DeclaringType.ContainsGenericParameters() )
         {
            gArgs = parentFunc( eMethodToCopy.DeclaringType ).GenericArguments.ToArray();
            actualMethodToCopy = eMethodToCopy.ChangeDeclaringType( gArgs );
         }

         var mb = tb.AddMethod(
            newName == null ? eMethodToCopy.Name : newName,
            newAttributes,
            (CILAssemblyManipulator.API.CallingConventions) eMethodToCopy.CallingConvention
         );

         if ( actualMethodToCopy.HasGenericArguments() )
         {
            actualMethodToCopy = actualMethodToCopy.GenericDefinition;
            var mgArgs = actualMethodToCopy.GenericArguments;
            var mgBuilders = mb.DefineGenericParameters( Enumerable.Repeat( TypeGenerationInfoImpl.METHOD_GENERIC_ARGUMENT_NAME_PREFIX, mgArgs.Count ).Select( ( name, idx ) => name + idx ).ToArray() );
            CopyGenericArgumentConstraintsFromAll( tbGArgs, mgBuilders, mgArgs, mgBuilders );
         }

         foreach ( var param in actualMethodToCopy.Parameters )
         {
            // Don't set HasDefault & optional flags
            mb.AddParameter( param.Name, param.Attributes & ~( CILAssemblyManipulator.API.ParameterAttributes.HasDefault | CILAssemblyManipulator.API.ParameterAttributes.Optional ), TypeGenerationUtils.CreateTypeForEmitting( param.ParameterType, tbGArgs, mb.GenericArguments ) );
         }
         var rp = mb.ReturnParameter;
         rp.Name = actualMethodToCopy.ReturnParameter.Name;
         rp.Attributes = actualMethodToCopy.ReturnParameter.Attributes;
         rp.ParameterType = TypeGenerationUtils.CreateTypeForEmitting( actualMethodToCopy.ReturnParameter.ParameterType, tbGArgs, mb.GenericArguments );

         return Tuple.Create( mb, actualMethodToCopy );
      }

      public static MethodGenerationInfo ImplementMethodForEmittingAndCreateMethodGenerationInfo(
         TypeGenerationInfo typeGenInfo,
         CILMethod methodToCopy,
         String newName,
         CILAssemblyManipulator.API.MethodAttributes newAttributes
         )
      {
         return new MethodGenerationInfoImpl( ImplementMethodForEmitting( typeGenInfo.Builder, typeGenInfo.GenericArguments, parent => typeGenInfo.Parents[parent], methodToCopy, newName, newAttributes ).Item1 );
      }

      //public static String GetBareTypeName( CILType type, Char nestedSeparator = '+' )
      //{
      //   StringBuilder builder = new StringBuilder();
      //   GetTypeString( type, false, true, builder, nestedSeparator );
      //   return builder.ToString();
      //}

      //private static void GetTypeString( CILTypeBase type, Boolean appendAssembly, Boolean appendTypeName, StringBuilder builder, Char nestedSeparator )
      //{
      //   if ( appendAssembly )
      //   {
      //      builder.Append( "[" ).Append( type.Module.Assembly.Name ).Append( "]" );
      //   }

      //   var ns = TypeKind.MethodSignature == type.TypeKind ? null : ( (CILTypeOrTypeParameter) type ).Namespace;
      //   if ( ns != null )
      //   {
      //      builder.Append( ns ).Append( "." );
      //   }

      //   if ( appendTypeName )
      //   {
      //      ProcessDeclaringType( type, builder, nestedSeparator );
      //      if ( type.IsGenericType() )
      //      {
      //         var typeCasted = (CILType) type;
      //         var gArgs = typeCasted.GenericArguments.ToArray();
      //         AppendBareName( typeCasted, gArgs, builder );
      //         builder.Append( "<" );
      //         for ( Int32 idx = 0; idx < gArgs.Length; ++idx )
      //         {
      //            var gArg = gArgs[idx];
      //            GetTypeString( gArg, appendAssembly, appendTypeName, builder, nestedSeparator );
      //            if ( idx < gArgs.Length - 1 )
      //            {
      //               builder.Append( ", " );
      //            }
      //         }
      //         builder.Append( ">" );
      //      }
      //      else
      //      {
      //         builder.Append( type.Name );
      //      }
      //   }
      //}

      //private static void ProcessDeclaringType( CILTypeBase type, StringBuilder builder, Char nestedSeparator )
      //{
      //   if ( !type.IsGenericParameter() )
      //   {
      //      var declType = type.DeclaringType;
      //      if ( declType != null )
      //      {
      //         ProcessDeclaringType( declType, builder, nestedSeparator );
      //         AppendBareName( declType, null, builder );
      //         builder.Append( nestedSeparator );
      //      }
      //   }
      //}

      private static void AppendBareName( CILType type, CILTypeBase[] gArgs, StringBuilder builder )
      {
         if ( gArgs != null || type.IsGenericType() )
         {
            if ( gArgs == null )
            {
               gArgs = type.GenericArguments.ToArray();
            }
            builder.Append( type.Name.Substring( 0, type.Name.Length - 1 - gArgs.Length.ToString().Length ) );
         }
         else
         {
            builder.Append( type.Name );
         }
      }

      public static Boolean IsAssignableFrom( CILType baseType, CILType subType )
      {
         var gDef = baseType.GenericDefinitionIfGArgsHaveGenericParams();
         return subType.GetFullInheritanceChain().FirstOrDefault( t => Object.Equals( t.GenericDefinitionIfGArgsHaveGenericParams(), gDef ) ) != null;
      }

      public static T[] OnlyBottomTypes<T>( IEnumerable<T> items, Func<T, CILType> typeSelector )
      {
         return items.Where( item => !items.Select( typeSelector ).Any( anotherType => !typeSelector( item ).Equals( anotherType ) && IsAssignableFrom( typeSelector( item ), anotherType ) ) ).ToArray();
      }

      public static Boolean TryFindInTypeDictionarySearchSubTypes<TargetType>( CILType type, IDictionary<CILType, TargetType> dic, out TargetType result )
      {
         // First try to find directly
         var found = dic.TryGetValue( type, out result );
         if ( !found )
         {
            // Then iterate all value until found
            foreach ( var kvp in dic )
            {
               found = IsAssignableFrom( type, kvp.Key );
               if ( found )
               {
                  result = kvp.Value;
                  break;
               }
            }
         }

         return found;
      }

      public static CILMethod FindMethodImplicitlyImplementingMethod( CILType classType, CILMethod methodFromInterface )
      {
         CILMethod result = null;
         if ( methodFromInterface != null )
         {
            foreach ( var suitableType in classType.GetFullInheritanceChain().Where( t => AreSame( t, methodFromInterface.DeclaringType, true ) ) )
            {
               var newMethodFromInterface = methodFromInterface.ChangeDeclaringType( suitableType.GenericArguments.ToArray() );
               result = classType
                  .AsSingleBranchEnumerable( t => t.BaseType )
                  .SelectMany( t => t.DeclaredMethods.Where( m => !m.Attributes.IsStatic() ) )
                  .FirstOrDefault( method =>
                  {
                     var p1 = newMethodFromInterface.Parameters;
                     var p2 = method.Parameters;
                     return Object.Equals( newMethodFromInterface.Name, method.Name )
                        && newMethodFromInterface.GenericArguments.Count == method.GenericArguments.Count
                        && p1.Count == p2.Count
                        && AreSame( method.ReturnParameter.ParameterType, newMethodFromInterface.ReturnParameter.ParameterType, false )
                        && p1.TakeWhile( ( param, idx ) => /*param.Attributes == p2[idx].Attributes &&*/ AreSame( p2[idx].ParameterType, param.ParameterType, false ) ).Count() == p1.Count;
                  } );
               if ( result != null )
               {
                  break;
               }
            }
         }
         return result;
      }

      private static Boolean AreSame( CILTypeBase x, CILTypeBase y, Boolean comparingBaseTypes )
      {
         var result = Object.ReferenceEquals( x, y );
         if ( !result )
         {
            if ( x.IsGenericParameter() && y.IsGenericParameter() )
            {
               var xt = (CILTypeParameter) x;
               var yt = (CILTypeParameter) y;
               result = comparingBaseTypes || ( xt.DeclaringMethod != null && yt.DeclaringMethod != null && xt.GenericParameterPosition == yt.GenericParameterPosition );
            }
            else
            {
               var xc = x as CILType;
               var yc = y as CILType;
               if ( xc != null && yc != null )
               {
                  if ( xc.IsGenericType() && yc.IsGenericType() )
                  {
                     var cGArgs = xc.GenericArguments;
                     var iGArgs = yc.GenericArguments;
                     result = Object.ReferenceEquals( xc.GenericDefinition, yc.GenericDefinition )
                        && iGArgs.TakeWhile( ( arg, index ) => AreSame( cGArgs[index], arg, comparingBaseTypes ) ).Count() == iGArgs.Count;
                  }
                  else if ( xc.IsAnyElementType() && yc.IsAnyElementType() )
                  {
                     if ( xc.IsArray() && yc.IsArray() )
                     {
                        result = Object.Equals( xc.ArrayInformation, yc.ArrayInformation );
                     }
                     if ( comparingBaseTypes || result || !xc.IsArray() )
                     {
                        result = ( comparingBaseTypes || xc.ElementKind == yc.ElementKind )
                           && AreSame( xc.ElementType, yc.ElementType, comparingBaseTypes );
                     }
                  }
               }
            }
         }
         return result;
      }
   }

   public class LocalBuilderInfo
   {
      private readonly Type _localBuilderType;
      private readonly String _localName;

      public LocalBuilderInfo( String name, Type type )
      {
         ArgumentValidator.ValidateNotNull( "Name", name );
         this._localName = name;
         this._localBuilderType = type;
      }

      public LocalBuilderInfo( String name )
         : this( name, null )
      {

      }

      public String Name
      {
         get
         {
            return this._localName;
         }
      }

      public Type Type
      {
         get
         {
            return this._localBuilderType;
         }
      }
   }
}
#endif
