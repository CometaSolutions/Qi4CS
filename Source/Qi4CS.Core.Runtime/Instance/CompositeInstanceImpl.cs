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
using System.Threading;
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Instance
{

   public class InvocationInfoImpl : InvocationInfo
   {
      private readonly MethodInfo _compositeMethod;
      private AbstractFragmentMethodModel _fragmentMethodModel;

      public InvocationInfoImpl( MethodInfo compositeMethod, AbstractFragmentMethodModel fragmentMethodModel )
      {
         ArgumentValidator.ValidateNotNull( "Composite method", compositeMethod );

         this._compositeMethod = compositeMethod;
         this._fragmentMethodModel = fragmentMethodModel;
      }

      #region InvocationInfo Members

      public MethodInfo CompositeMethod
      {
         get
         {
            return this._compositeMethod;
         }
      }

      public AbstractFragmentMethodModel FragmentMethodModel
      {
         get
         {
            return this._fragmentMethodModel;
         }
         set
         {
            this._fragmentMethodModel = value;
         }
      }

      #endregion
   }


   public class CompositeInstanceImpl : CompositeInstance
   {

      private class FragmentConstructorInfo
      {
         private readonly ConstructorModel _model;
         private readonly ConstructorInfo _runtimeInfo;
         private readonly ParameterInfo[] _parameters;

         public FragmentConstructorInfo( ConstructorModel model, ConstructorInfo runtimeInfo )
         {
            this._model = model;
            this._runtimeInfo = runtimeInfo;
            this._parameters = runtimeInfo.GetParameters();
         }

         public ConstructorModel Model
         {
            get
            {
               return this._model;
            }
         }

         public ConstructorInfo RuntimeInfo
         {
            get
            {
               return this._runtimeInfo;
            }
         }

         public ParameterInfo[] Parameters
         {
            get
            {
               return this._parameters;
            }
         }

         public override Boolean Equals( Object obj )
         {
            return Object.ReferenceEquals( this, obj ) || ( obj is FragmentConstructorInfo && this._runtimeInfo.Equals( ( (FragmentConstructorInfo) obj )._runtimeInfo ) );
         }

         public override Int32 GetHashCode()
         {
            return this._runtimeInfo.GetHashCode();
         }
      }

      public interface InstancePoolInfo<TInstance>
      {
         GeneratedTypeInfo TypeGenerationResult { get; }
         Type GeneratedType { get; }
         InstancePool<TInstance> Pool { get; }
      }

      public class InstancePoolInfoImpl<TInstance> : InstancePoolInfo<TInstance>
      {
         private readonly InstancePool<TInstance> _pool;
         private readonly Type _generatedType;
         private readonly GeneratedTypeInfo _typeGenerationResult;

         public InstancePoolInfoImpl( InstancePool<TInstance> pool, Type generatedType, GeneratedTypeInfo typeGenerationResult )
         {
            this._pool = pool;
            this._generatedType = generatedType;
            this._typeGenerationResult = typeGenerationResult;
         }

         #region InstancePoolInfo<InstanceType> Members

         public GeneratedTypeInfo TypeGenerationResult
         {
            get
            {
               return this._typeGenerationResult;
            }
         }

         public InstancePool<TInstance> Pool
         {
            get
            {
               return this._pool;
            }
         }

         public Type GeneratedType
         {
            get
            {
               return this._generatedType;
            }
         }

         #endregion
      }

      private enum PrototypeState { PROTOTYPE, IN_TRANSITION_RUNNING_ACTION, IN_TRANSITION_CHECKING_STATE, NOT_PROTOTYPE }

      protected const Int32 COMPOSITE_CTOR_FIRST_ADDITIONAL_PARAM_IDX = 3;
      protected const Int32 COMPOSITE_CTOR_ADDITIONAL_PARAMS_COUNT = 3;

      // TODO some things here probably could be put under Lazy<...>
      private readonly CompositeInstanceStructureOwner _structureOwner;
      private readonly CompositeModelInfo _modelInfo;
      private readonly DictionaryQuery<Type, Object> _composites;
      private readonly DictionaryQuery<Type, InstancePoolInfo<FragmentInstance>> _fragmentInstancePools;
      private readonly DictionaryQuery<Type, FragmentInstance> _fragmentInstances;
      private readonly DictionaryQuery<Type, InstancePoolInfo<FragmentDependant>> _concernInvocationInstances;
      private readonly DictionaryQuery<Type, InstancePoolInfo<FragmentDependant>> _sideEffectInvocationInstances;
      private readonly DictionaryQuery<Type, ListQuery<FragmentConstructorInfo>> _constructorsForFragments;
      private readonly CompositeState _state;
      private readonly UsesContainerQuery _usesContainer;
#if SILVERLIGHT
      [ThreadStatic]
      private static 
#else
      private readonly Lazy<ThreadLocal<
#endif
Stack<InvocationInfo>
#if !SILVERLIGHT
>>
#endif
 _invocationInfos;
      private readonly Action<IDictionary<QualifiedName, IList<ConstraintViolationInfo>>> _checkStateFunc;
      private readonly Type[] _gArgs;
      private readonly Lazy<MethodInfo[]> _compositeMethods;
      private readonly Lazy<DictionaryQuery<MethodInfo, CompositeMethodModel>> _methodsToModels;


      private Int32 _isPrototype;

      private InProgressTracker _isPrototypeTransitionInProgress;
      private readonly Action _prototypeAction;

      public CompositeInstanceImpl( CompositeInstanceStructureOwner structureOwner, CompositeModel model, IEnumerable<Type> publicCompositeTypes, UsesContainerQuery usesContainer )
         : this( structureOwner, model, publicCompositeTypes, usesContainer, null )
      {

      }

      protected CompositeInstanceImpl(
         CompositeInstanceStructureOwner structureOwner,
         CompositeModel model,
         IEnumerable<Type> publicCompositeTypes,
         UsesContainerQuery usesContainer,
         MainCompositeConstructorArguments publicCtorArgsObject
         )
      {
         ArgumentValidator.ValidateNotNull( "Structure owner", structureOwner );
         ArgumentValidator.ValidateNotNull( "Composite model", model );
         ArgumentValidator.ValidateNotEmpty( "Composite type", publicCompositeTypes );
         ArgumentValidator.ValidateNotNull( "Container for objects to be used in fragment creation", usesContainer );

         this._structureOwner = structureOwner;
         var application = this._structureOwner.Application;
         this._modelInfo = this._structureOwner.ModelInfoContainer.GetCompositeModelInfo( model );
         if ( publicCompositeTypes.Any( pcType => pcType.ContainsGenericParameters() ) )
         {
            throw new InternalException( "With given public composite types {" + String.Join( ", ", publicCompositeTypes ) + "} and public composite type in model being [" + String.Join( ", ", model.PublicTypes ) + "], the public composite types contained non-closed generic parameters." );
         }
         this._usesContainer = usesContainer;

         this._isPrototype = (Int32) PrototypeState.PROTOTYPE;

         this._invocationInfos =
#if !SILVERLIGHT
 new Lazy<ThreadLocal<Stack<InvocationInfo>>>( () => new ThreadLocal<Stack<InvocationInfo>>( () =>
#endif
 new Stack<InvocationInfo>()
#if !SILVERLIGHT
 ), LazyThreadSafetyMode.PublicationOnly )
#endif
;

         var composites = application.CollectionsFactory.NewDictionaryProxy<Type, Object>();
         var cProps = application.CollectionsFactory.NewListProxy( new List<CompositeProperty>( model.Methods.Count * 2 ) );
         var cEvents = application.CollectionsFactory.NewListProxy( new List<CompositeEvent>( model.Methods.Count * 2 ) );

         var publicTypeGenResult = this._modelInfo.Types;
         var factory = publicTypeGenResult.CompositeFactory;

         var gArgs = publicTypeGenResult.PublicCompositeGenericArguments.Count == 0 ? null : new Type[publicTypeGenResult.GeneratedMainPublicType.GetGenericArguments().Length];
         this._gArgs = gArgs;

         foreach ( var pType in publicCompositeTypes )
         {
            ListQuery<Int32> gArgInfo = null;
            if ( publicTypeGenResult.PublicCompositeGenericArguments.TryGetValue( pType.GetGenericDefinitionIfGenericType(), out gArgInfo ) )
            {
               var declaredGArgs = pType.GetGenericArguments();
               for ( Int32 i = 0; i < declaredGArgs.Length; ++i )
               {
                  gArgs[gArgInfo[i]] = declaredGArgs[i];
               }
            }
         }

         if ( gArgs != null && gArgs.Any( gArg => gArg == null ) )
         {
            throw new InvalidCompositeTypeException( publicCompositeTypes, "Could not find suitable generic argument for all public types of composite " + this._modelInfo.Model + "." );
         }

         Action prePrototypeAction = null;
         var publicCtorArgs = new Object[publicTypeGenResult.MaxParamCountForCtors];
         Object[] compositeCtorParams = null;
         foreach ( var genType in publicTypeGenResult.GeneratedPublicTypes )
         {
            var isMainType = genType.GeneratedType.Equals( this._modelInfo.Types.GeneratedMainPublicType );
            var curCtorArgs = isMainType ? publicCtorArgs : compositeCtorParams;
            this.SetCompositeCtorArgs( ref curCtorArgs, cProps.AO, cEvents.AO );
            var publicComposite = factory.CreateInstance( genType.GeneratedTypeID, gArgs, curCtorArgs );
            foreach ( var cType in this.GetTypeKeysForGeneratedType( publicComposite.GetType(), true, isMainType ) )
            {
               composites[cType] = publicComposite;
            }

            if ( isMainType )
            {
               if ( publicCtorArgsObject != null )
               {
                  publicCtorArgsObject.Arguments = curCtorArgs;
               }
               prePrototypeAction = (Action) publicCtorArgs[COMPOSITE_CTOR_FIRST_ADDITIONAL_PARAM_IDX];
               this._prototypeAction = (Action) publicCtorArgs[COMPOSITE_CTOR_FIRST_ADDITIONAL_PARAM_IDX + 1];
               this._checkStateFunc = (Action<IDictionary<QualifiedName, IList<ConstraintViolationInfo>>>) publicCtorArgs[COMPOSITE_CTOR_FIRST_ADDITIONAL_PARAM_IDX + 2];
               this._compositeMethods = new Lazy<MethodInfo[]>( () => ( (CompositeCallbacks) publicComposite ).GetCompositeMethods(), LazyThreadSafetyMode.ExecutionAndPublication );
            }
         }

         this._isPrototypeTransitionInProgress = null;

         this.SetCompositeCtorArgs( ref compositeCtorParams, cProps.AO, cEvents.AO );

         foreach ( var typeGenResult in publicTypeGenResult.PrivateCompositeGenerationResults )
         {
            var privateComposite = factory.CreateInstance( typeGenResult.GeneratedTypeID, gArgs, compositeCtorParams );
            foreach ( var cTypeOrParent in this.GetTypeKeysForGeneratedType( privateComposite.GetType(), false, false ) )
            {
               composites.Add( cTypeOrParent, privateComposite );
            }
         }

         this._composites = composites.CQ;

         this._methodsToModels = new Lazy<DictionaryQuery<MethodInfo, CompositeMethodModel>>( () =>
         {
            var retVal = new Dictionary<MethodInfo, CompositeMethodModel>();
            var cMethods = this._compositeMethods.Value;
            for ( var i = 0; i < cMethods.Length; ++i )
            {
               var cm = cMethods[i];
               retVal.Add( cm, this._modelInfo.Model.Methods[i] );
            }
            return application.CollectionsFactory.NewDictionaryProxy( retVal ).CQ;
         }, LazyThreadSafetyMode.ExecutionAndPublication );

         this._state = new CompositeStateImpl( this._structureOwner.Application.CollectionsFactory, cProps.CQ, cEvents.CQ );

         this._fragmentInstancePools = this.CreatePoolDictionary<FragmentTypeGenerationResult, FragmentInstance>(
            gArgs,
            this._modelInfo.Types.FragmentGenerationResults.Where( fGenResult => fGenResult.InstancePoolRequired ),
            application.CollectionsFactory );
         var fInstances = application.CollectionsFactory.NewDictionaryProxy( new Dictionary<Type, FragmentInstance>() );
         foreach ( var genResult in publicTypeGenResult.FragmentGenerationResults.Where( val => !val.InstancePoolRequired ) )
         {
            var genType = genResult.GeneratedType;
            if ( gArgs != null )
            {
               genType = genType.MakeGenericType( gArgs );
            }
            else if ( genType.ContainsGenericParameters() )
            {
               throw new InternalException( "Could not find generic arguments for fragment type " + genType.GetBaseType() + "." );
            }
            fInstances.Add( genType, new FragmentInstanceImpl() );
         }
         this._fragmentInstances = fInstances.CQ;
         this._concernInvocationInstances = this.CreatePoolDictionary<GeneratedTypeInfo, FragmentDependant>( gArgs, publicTypeGenResult.ConcernInvocationGenerationResults, application.CollectionsFactory );
         this._sideEffectInvocationInstances = this.CreatePoolDictionary<GeneratedTypeInfo, FragmentDependant>( gArgs, publicTypeGenResult.SideEffectGenerationResults, application.CollectionsFactory );
         this._constructorsForFragments = application.CollectionsFactory.NewDictionaryProxy<Type, ListQuery<FragmentConstructorInfo>>( this._fragmentInstancePools.Keys.Concat( this._fragmentInstances.Keys )
            .ToDictionary(
            fType => fType,
            fType => application.CollectionsFactory.NewListProxy( fType
               .GetAllInstanceConstructors()
               .Select( fCtor =>
               {
                  Int32 idx;
                  return fCtor.TryGetConstructorModelIndex( out idx ) ?
                     new FragmentConstructorInfo( model.Constructors[idx], fCtor ) :
                     null;
               } )
               .Where( i => i != null )
               .ToList()
               ).CQ
            ) ).CQ;

         if ( prePrototypeAction != null )
         {
            prePrototypeAction();
         }
      }

      private IEnumerable<Type> GetTypeKeysForGeneratedType( Type generatedCompositeType, Boolean includeObject, Boolean mainCompositeType )
      {
         var result = generatedCompositeType.GetAllParentTypes();
         if ( !includeObject )
         {
            result = result.Where( t => !t.Equals( typeof( Object ) ) );
         }
         if ( mainCompositeType )
         {
            result = result.Where( t => !t.Equals( typeof( CompositeCallbacks ) ) );
         }
         return result;
      }

      protected void SetCompositeCtorArgs( ref Object[] args, CollectionAdditionOnly<CompositeProperty> properties, CollectionAdditionOnly<CompositeEvent> events )
      {
         if ( args == null )
         {
            args = new Object[COMPOSITE_CTOR_FIRST_ADDITIONAL_PARAM_IDX];
         }
         args[0] = this;
         args[1] = properties;
         args[2] = events;
         for ( Int32 idx = COMPOSITE_CTOR_FIRST_ADDITIONAL_PARAM_IDX; idx < args.Length; ++idx )
         {
            args[idx] = null;
         }
      }

      #region CompositeInstance Members

      public FragmentDependant CreateOrGetConcernInvocationBase( Type type )
      {
         return this.CreateInvocationBase<FragmentDependant>( this._concernInvocationInstances, type );
      }

      public FragmentDependant CreateOrGetSideEffectInvocationBase( Type type )
      {
         return this.CreateInvocationBase<FragmentDependant>( this._sideEffectInvocationInstances, type );
      }

      protected TInvocation CreateInvocationBase<TInvocation>( DictionaryQuery<Type, InstancePoolInfo<TInvocation>> dic, Type key )
      {
         InstancePoolInfo<TInvocation> pool = this.FindPool<TInvocation>( key, dic );
         TInvocation result;
         if ( !pool.Pool.TryTake( out result ) )
         {
            result = (TInvocation) this._modelInfo.Types.CompositeFactory.CreateInstance( pool.TypeGenerationResult.GeneratedTypeID, this._gArgs, new Object[] { this } );
         }
         return result;
      }

      public void ReturnConcernInvocation( Type type, FragmentDependant concernInvocation )
      {
         if ( concernInvocation != null )
         {
            this.FindPool<FragmentDependant>( type, this._concernInvocationInstances ).Pool.Return( concernInvocation );
         }
      }

      public void ReturnSideEffectInvocation( Type type, FragmentDependant sideEffectInvocation )
      {
         if ( sideEffectInvocation != null )
         {
            this.FindPool<FragmentDependant>( type, this._sideEffectInvocationInstances ).Pool.Return( sideEffectInvocation );
         }
      }

      public CompositeModelInfo ModelInfo
      {
         get
         {
            return this._modelInfo;
         }
      }

      public CompositeInstanceStructureOwner StructureOwner
      {
         get
         {
            return this._structureOwner;
         }
      }

      public InstancePool<FragmentInstance> GetInstancePoolForFragment( Type fragment )
      {
         return this._fragmentInstancePools[fragment].Pool;
      }

      public FragmentInstance GetInstanceForFragment( Type fragment )
      {
         return this._fragmentInstances[fragment];
      }

      public DictionaryQuery<Type, Object> Composites
      {
         get
         {
            return this._composites;
         }
      }

      public Int32 GetConstructorIndex( Type fragmentType )
      {
         ListQuery<FragmentConstructorInfo> infos = null;
         Int32 result = -1;
         if ( this._constructorsForFragments.TryGetValue( fragmentType, out infos ) && infos.Count > 0 )
         {
            FragmentConstructorInfo ctorWithoutParams = infos.FirstOrDefault( ctor => !ctor.Model.Parameters.Any() && ctor.RuntimeInfo.DeclaringType.Equals( fragmentType.GetBaseType() ) );
            Type[] types = this._usesContainer.ContainedTypes.ToArray();
            if ( types.Any() )
            {
               // Need to find constructor with all [Uses] parameter types assignable
               foreach ( FragmentConstructorInfo info in infos )
               {
                  ParameterInfo[] pInfos = info.Parameters;
                  Boolean match = info.Model.Parameters.All( paramModel => !( paramModel.InjectionScope is UsesAttribute ) || this._usesContainer.HasValue( pInfos[paramModel.NativeInfo.Position].ParameterType, ( (UsesAttribute) paramModel.InjectionScope ).Name ) );
                  if ( match )
                  {
                     result = info.Model.ConstructorIndex;
                     break;
                  }
               }
            }
            if ( result == -1 )
            {
               if ( ctorWithoutParams == null )
               {
                  result = infos.First( ctor => ctor.RuntimeInfo.DeclaringType.Equals( fragmentType ) ).Model.ConstructorIndex;
               }
               else
               {
                  result = ctorWithoutParams.Model.ConstructorIndex;
               }
            }
         }
         return result;
      }

      public CompositeState State
      {
         get
         {
            return this._state;
         }
      }

      public Boolean IsPrototype
      {
         get
         {
            return this._isPrototype != (Int32) PrototypeState.NOT_PROTOTYPE && this._isPrototype != (Int32) PrototypeState.IN_TRANSITION_CHECKING_STATE;
         }
      }

      public void DisablePrototype( Int32 compositeMethodIndex, MethodGenericArgumentsInfo gArgsInfo, AbstractFragmentMethodModel nextMethod )
      {
         ApplicationSkeleton.ThreadsafeStateTransition( ref this._isPrototype, (Int32) PrototypeState.PROTOTYPE, (Int32) PrototypeState.IN_TRANSITION_RUNNING_ACTION, (Int32) PrototypeState.NOT_PROTOTYPE, true, ref this._isPrototypeTransitionInProgress, ApplicationSkeleton.WAIT_TIME, () =>
         {
            this.RunPrototypeAction( compositeMethodIndex, gArgsInfo, nextMethod );
            Interlocked.Exchange( ref this._isPrototype, (Int32) PrototypeState.IN_TRANSITION_CHECKING_STATE );
            this.CheckCompositeState( compositeMethodIndex, gArgsInfo, nextMethod );
            ( (CompositeModelImmutable) this.ModelInfo.Model ).InvokeCompositeInstantiated( this );
         } );
      }

      public DictionaryQuery<MethodInfo, CompositeMethodModel> MethodToModelMapping
      {
         get
         {
            return this._methodsToModels.Value;
         }
      }

      private void RunPrototypeAction( Int32 compositeMethodIndex, MethodGenericArgumentsInfo gArgsInfo, AbstractFragmentMethodModel nextMethod )
      {
         if ( this._prototypeAction != null )
         {
            var compositeMethod = this.GetCompositeMethodInfo( compositeMethodIndex, gArgsInfo );

            if ( compositeMethod != null )
            {
               _invocationInfos
#if !SILVERLIGHT
.Value.Value
#endif
.Push( new InvocationInfoImpl( compositeMethod, nextMethod ) );
            }
            try
            {
               this._prototypeAction();
            }
            finally
            {
               if ( compositeMethod != null )
               {
                  _invocationInfos
#if !SILVERLIGHT
.Value.Value
#endif
.Pop();
               }
            }
         }
      }

      private void CheckCompositeState( Int32 compositeMethodIndex, MethodGenericArgumentsInfo gArgsInfo, AbstractFragmentMethodModel nextMethod )
      {
         if ( this._checkStateFunc != null )
         {
            var violations = new Dictionary<QualifiedName, IList<ConstraintViolationInfo>>();
            this._checkStateFunc( violations );
            if ( violations.Any() )
            {
               throw new CompositeInstantiationException( violations );
            }
         }
      }

      public UsesContainerQuery UsesContainer
      {
         get
         {
            return this._usesContainer;
         }
      }

      //public virtual CompositeInstanceImpl Clone( IEnumerable<Type> publicCompositeTypes, out UsesContainerMutable newUses )
      //{
      //   newUses = UsesContainerMutableImpl.CopyOf( this._usesContainer );
      //   CompositeInstanceImpl result = this.CreateNewCopy( publicCompositeTypes, newUses.Query );
      //   this.CopyState( this._state, result.State );
      //   return result;
      //}

      public InvocationInfo InvocationInfo
      {
         get
         {
            var stk = _invocationInfos
#if !SILVERLIGHT
.Value.Value
#endif
;
            return stk.Count == 0 ? null : stk.Peek();
         }
         set
         {
            var stk = _invocationInfos
#if !SILVERLIGHT
.Value.Value
#endif
;
            if ( value == null )
            {
               stk.Pop();
            }
            else
            {
               stk.Push( value );
            }
         }
      }

      #endregion

      //protected virtual CompositeInstanceImpl CreateNewCopy( IEnumerable<Type> publicCompositeTypes, UsesContainerQuery uses )
      //{
      //   return new CompositeInstanceImpl( this._structureOwner, this._modelInfo.Model, publicCompositeTypes, uses );
      //}

      protected InstancePoolInfo<TInvocation> FindPool<TInvocation>( Type key, DictionaryQuery<Type, InstancePoolInfo<TInvocation>> dic )
      {
         InstancePoolInfo<TInvocation> pool = null;
         if ( !dic.TryFindInTypeDictionarySearchSubTypes( key, out pool ) )
         {
            throw new InternalException( "Could not find invocation base type " + key + "." );
         }
         return pool;
      }

      protected DictionaryQuery<Type, InstancePoolInfo<TInvocation>> CreatePoolDictionary<TTypeGen, TInvocation>(
         Type[] gArgs,
         IEnumerable<TTypeGen> typeGenResults,
         CollectionsFactory collectionsFactory
         )
         where TTypeGen : GeneratedTypeInfo
      {
         DictionaryProxy<Type, InstancePoolInfo<TInvocation>> result = collectionsFactory.NewDictionaryProxy<Type, InstancePoolInfo<TInvocation>>();

         foreach ( var genResult in typeGenResults )
         {
            var genType = genResult.GeneratedType;
            if ( gArgs != null )
            {
               genType = genType.MakeGenericType( gArgs );
            }
            else if ( genType.ContainsGenericParameters() )
            {
               var typeToStringify = genType;
               if ( typeof( TTypeGen ).Equals( typeof( FragmentTypeGenerationResult ) ) )
               {
                  typeToStringify = typeToStringify.GetBaseType();
               }
               throw new InternalException( "Could not find generic arguments for " + typeToStringify + "." );
            }
            result.Add( genType, new InstancePoolInfoImpl<TInvocation>( new InstancePool<TInvocation>(), genType, genResult ) );
         }
         return result.CQ;
      }

      protected MethodInfo GetCompositeMethodInfo( Int32 compositeMethodIndex, MethodGenericArgumentsInfo gArgsInfo )
      {
         var retVal = compositeMethodIndex >= 0 ?
            this._compositeMethods.Value[compositeMethodIndex] :
            null;

         if ( retVal != null && gArgsInfo != null )
         {
            retVal = retVal.MakeGenericMethod( gArgsInfo.GetGenericArguments() );
         }
         return retVal;
      }

      //private void CopyState( CompositeState from, CompositeState to )
      //{
      //   foreach ( KeyValuePair<QualifiedName, CompositeProperty> kvp in from.Properties )
      //   {
      //      to.Properties[kvp.Key].PropertyValueAsObject = kvp.Value.PropertyValueAsObject;
      //   }
      //}
   }

   public class MainCompositeConstructorArguments
   {
      public Object[] Arguments { get; set; }
   }


   public sealed class MethodGenericArgumentsInfo
   {
      private readonly RuntimeTypeHandle _typeHandle;
      private readonly RuntimeMethodHandle _methodHandle;

      public MethodGenericArgumentsInfo( RuntimeMethodHandle methodHandle, RuntimeTypeHandle typeHandle )
      {
         this._typeHandle = typeHandle;
         this._methodHandle = methodHandle;
      }

      internal Type[] GetGenericArguments()
      {
         return MethodBase.GetMethodFromHandle( this._methodHandle, this._typeHandle ).GetGenericArguments();
      }
   }

   public interface CompositeCallbacks
   {
      System.Reflection.MethodInfo[] GetCompositeMethods();
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Tries to find a value from <paramref name="dictionary"/> with types as keys. If direct lookup fails, this method will accept value of bottom-most type of <paramref name="type"/>'s inheritance hierarchy found in <paramref name="dictionary"/>.
   /// </summary>
   /// <typeparam name="TValue">The type of the values of the <paramref name="dictionary"/>.</typeparam>
   /// <param name="type">The type to search value for.</param>
   /// <param name="dictionary">The dictionary to search from.</param>
   /// <param name="result">This will contain result if return value is <c>true</c>; otherwise <c>default(TValue)</c>.</param>
   /// <returns>If the dictionary contains key <paramref name="type"/> or any of the keys has <paramref name="type"/> as its parent type, <c>true</c>; otherwise, <c>false</c>.</returns>
   internal static Boolean TryFindInTypeDictionarySearchBottommostType<TValue>( this DictionaryQuery<Type, TValue> dictionary, Type type, out TValue result )
   {
      var found = dictionary.TryGetValue( type, out result );
      if ( !found )
      {
         // Search for bottom-most type
         var current = type;
         var currentOK = false;
         foreach ( var kvp in dictionary )
         {
            currentOK = current.IsAssignableFrom( kvp.Key );
            found = currentOK || found;
            if ( currentOK )
            {
               result = kvp.Value;
               current = kvp.Key;
            }
         }
      }
      return found;
   }

   /// <summary>
   /// Tries to find a value from <paramref name="dictionary"/> with types as keys. If direct lookup fails, this method will accept value of the key which will return <c>true</c> for <see cref="E_CommonUtils.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes"/> with <paramref name="type"/> as first argument and current key as second argument.
   /// </summary>
   /// <typeparam name="TValue">The type of the values of the <paramref name="dictionary"/>.</typeparam>
   /// <param name="type">The type to search value for.</param>
   /// <param name="dictionary">The dictionary to search from.</param>
   /// <param name="result">This will contain result if return value is <c>true</c>; otherwise <c>default(TValue)</c>.</param>
   /// <returns>If the dictionary contains key <paramref name="type"/> or any of its parent types, <c>true</c>; otherwise, <c>false</c>.</returns>
   internal static Boolean TryFindInTypeDictionarySearchSubTypes<TValue>( this DictionaryQuery<Type, TValue> dictionary, Type type, out TValue result )
   {
      // First try to find directly
      var found = dictionary.TryGetValue( type, out result );
      if ( !found )
      {
         // Then iterate all value until found
         foreach ( var kvp in dictionary )
         {
            found = type.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( kvp.Key );
            if ( found )
            {
               result = kvp.Value;
               break;
            }
         }
      }

      return found;
   }
}