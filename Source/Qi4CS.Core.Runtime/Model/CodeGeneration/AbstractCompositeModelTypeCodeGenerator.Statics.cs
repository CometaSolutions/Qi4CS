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
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using CommonUtils;

namespace Qi4CS.Core.Runtime.Model
{
   public abstract partial class AbstractCompositeModelTypeCodeGenerator
   {
      private class BaseTypeComparer : IComparer<Type>
      {
         #region IComparer<Type> Members

         public Int32 Compare( Type x, Type y )
         {
            x = x.GetGenericDefinitionIfGenericType();
            y = y.GetGenericDefinitionIfGenericType();
            Int32 result = x.Equals( y ) ? 0 : 1;
            if ( result > 0 )
            {
               Type currentY = y.BaseType;
               while ( currentY != null )
               {
                  currentY = currentY.GetGenericDefinitionIfGenericType();
                  if ( currentY.Equals( x ) )
                  {
                     result = -1;
                     break;
                  }
                  else
                  {
                     currentY = currentY.BaseType;
                  }
               }
            }
            return result;
         }

         #endregion
      }

      protected static readonly IComparer<Type> BASE_TYPE_COMPARER = new BaseTypeComparer();

      protected const String COMPOSITE_METHOD_PREFIX = "CompositeMethod";
      protected const String SPECIAL_METHOD_PREFIX = "SpecialMethod";
      protected const String FRAGMENT_METHOD_PREFIX = "FragmentMethod";
      protected const String PROTOTYPE_METHOD_NAME = "PrototypeMethod";
      protected const String PROTOTYPE_FIELD_NAME = "PrototypeField";
      protected const String PROTOTYPE_METHOD_PARAMETER_NAME = "instance";
      protected const String ARG_ARRAY_DUMMY_LOCAL_PREFIX = "dummy";
      protected const String DELEGATE_INVOKE_METHOD_NAME = "Invoke";
      protected const String CHECK_STATE_METHOD_NAME = "CheckState";
      protected const String SET_DEFAULTS_METHOD_NAME = "SetDefaults";
      protected const String INJECTION_LAMBDA_CLASS_PREFIX = "LazyInjector";
      protected const String LAMBDA_METHOD_NAME = "LambdaMethod";
      protected const String CREATE_FRAGMENT_METHOD_PREFIX = "Create";
      protected const String CREATE_FRAGMENT_METHOD_INSTANCE_PARAM_NAME = "instance";
      protected const String CREATE_FRAGMENT_METHOD_METHOD_INDEX_PARAM_NAME = "methodIndex";
      protected const String CREATE_FRAGMENT_METHOD_CONCERN_INDEX_PARAM_NAME = "concernIndex";
      protected const String CREATE_FRAGMENT_METHOD_METHOD_RESULT_PARAM_NAME = "methodResult";
      protected const String CREATE_FRAGMENT_METHOD_METHOD_EXCEPTION_PARAM_NAME = "exception";
      protected const String PUBLIC_COMPOSITE_METHOD_INVOKER_SUFFIX = "PublicInvoker";

      protected const String REF_INVOKER_METHOD_SUFFIX = "RefInvoker";

      protected const String CONCERN_INVOCATION_METHOD_PREFIX = "ConcernInvocationMethod";
      protected const String SIDE_EFFECT_INVOCATION_METHOD_PREFIX = "SideEffectInvocationMethod";

      protected const String LAZY_INJECTION_LAMBDA_CLASS_INJECTABLE_MODEL_FIELD_NAME = "_injectableModel";
      //protected const String LAZY_INJECTION_LAMBDA_CLASS_ATTRIBUTE_FIELD_NAME = "_attribute";
      protected const String LAZY_INJECTION_LAMBDA_CLASS_TYPE_FIELD_NAME = "_type";

      protected static readonly DictionaryQuery<Type, MethodBase> DEFAULT_CREATORS;
      protected const String DEFAULT_STRING = "";

      protected const Int32 DEFAULT_INVOCATION_METHOD_INDEX = -1;
      protected const Int32 DEFAULT_CONCERN_INVOCATION_INDEX = -1;
      protected const BindingFlags DEFAULT_SEARCH_BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

      protected static readonly LocalBuilderInfo LB_RESULT;
      protected static readonly LocalBuilderInfo LB_C_INSTANCE;
      protected static readonly LocalBuilderInfo LB_F_INSTANCE;
      protected static readonly LocalBuilderInfo LB_F_INSTANCE_POOL;
      protected static readonly LocalBuilderInfo LB_EXCEPTION;
      protected static readonly LocalBuilderInfo LB_PARAM_MODEL;
      protected static readonly LocalBuilderInfo LB_CONSTRAINT_MODEL;
      protected static readonly LocalBuilderInfo LB_INDEX;
      protected static readonly LocalBuilderInfo LB_MAX;
      protected static readonly LocalBuilderInfo LB_ARGS_ARRAY;
      protected static readonly LocalBuilderInfo LB_ARGS_ARRAY_FOR_SIDE_EFFECTS;
      protected static readonly LocalBuilderInfo LB_SPECIAL_METHOD_MODEL;
      protected static readonly LocalBuilderInfo LB_COMPOSITE_METHOD_MODEL;
      protected static readonly LocalBuilderInfo LB_VIOLATIONS;
      protected static readonly LocalBuilderInfo LB_CONSTRAINT_INSTANCE_POOL;
      protected static readonly LocalBuilderInfo LB_CONSTRAINT_INSTANCE;
      protected static readonly LocalBuilderInfo LB_EXCEPTION_LIST;
      protected static readonly LocalBuilderInfo LB_EVENT_HANDLERS;
      //protected static readonly LocalBuilderInfo LB_EVENT_HANDLERS_WEAK;
      //protected static readonly LocalBuilderInfo LB_AMOUNT_OF_DEAD_EVENT_INFOS;
      protected static readonly LocalBuilderInfo LB_CONSTRAINT_VIOLATION_EXCEPTION;
      protected static readonly LocalBuilderInfo LB_TEMP_STORAGE;
      protected static readonly LocalBuilderInfo LB_INVOCATION_INFO;
      protected static readonly LocalBuilderInfo LB_CONCERN_INDEX;

      protected const CILAssemblyManipulator.API.MethodAttributes NORMAL_IMPLEMENTATION_ATTRIBUTES = CILAssemblyManipulator.API.MethodAttributes.Public | CILAssemblyManipulator.API.MethodAttributes.HideBySig | CILAssemblyManipulator.API.MethodAttributes.Virtual;

      protected static readonly Type COMPOSITE_CTOR_PROPERTIES_PARAM_TYPE_NATIVE = typeof( CollectionAdditionOnly<CompositeProperty> );
      protected static readonly Type COMPOSITE_CTOR_EVENTS_PARAM_TYPE_NATIVE = typeof( CollectionAdditionOnly<CompositeEvent> );
      protected static readonly Type ACTION_REF_TYPE_NATIVE = typeof( Action ).MakeByRefType();
      protected static readonly Type[] ACTION_REF_TYPES_1_NATIVE = new Type[] { ACTION_REF_TYPE_NATIVE };
      protected static readonly Type[] ACTION_REF_TYPES_2_NATIVE = new Type[] { ACTION_REF_TYPE_NATIVE, ACTION_REF_TYPE_NATIVE };
      protected static readonly Type CHECK_STATE_FUNC_TYPE_NATIVE = typeof( Action<IDictionary<QualifiedName, IList<ConstraintViolationInfo>>> );
      protected static readonly Type[] PUBLIC_COMPOSITE_CTOR_ADDITTIONAL_PARAM_TYPES_NATIVE = new Type[] { ACTION_REF_TYPE_NATIVE, ACTION_REF_TYPE_NATIVE, CHECK_STATE_FUNC_TYPE_NATIVE.MakeByRefType() };
      protected static readonly Type ACTION_TYPE_NATIVE = typeof( Action );
      protected static readonly Type FRAGMENT_DEPENDANT_TYPE_NATIVE = typeof( FragmentDependant );
      protected static readonly Type FIELD_MODEL_TYPE_NATIVE = typeof( FieldModel );
      protected static readonly Type CONSTRUCTOR_MODEL_TYPE_NATIVE = typeof( ConstructorModel );
      protected static readonly Type PARAMETER_MODEL_TYPE_NATIVE = typeof( ParameterModel );
      protected static readonly Type SPECIAL_METHOD_MODEL_TYPE_NATIVE = typeof( SpecialMethodModel );
      protected static readonly Type COMPOSITE_METHOD_MODEL_TYPE_NATIVE = typeof( CompositeMethodModel );
      protected static readonly Type CONSTRAINT_MODEL_TYPE_NATIVE = typeof( ConstraintModel );
      protected static readonly Type VOID_TYPE_NATIVE = typeof( void );
      protected static readonly Type OBJECT_TYPE_NATIVE = typeof( Object );
      protected static readonly Type ATTRIBUTE_TYPE_NATIVE = typeof( Attribute );
      protected static readonly Type TYPE_TYPE_NATIVE = typeof( Type );
      protected static readonly Type ABSTRACT_INJECTABLE_MODEL_TYPE_NATIVE = typeof( AbstractInjectableModel );
      protected static readonly Type CONSTRAINT_TYPE_NATIVE = typeof( Constraint<,> );
      protected static readonly Type INT32_TYPE_NATIVE = typeof( Int32 );
      protected static readonly Type UINT32_TYPE_NATIVE = typeof( UInt32 );
      protected static readonly Type INT64_TYPE_NATIVE = typeof( Int64 );
      protected static readonly Type BOOLEAN_TYPE_NATIVE = typeof( Boolean );
      protected static readonly Type SINGLE_TYPE_NATIVE = typeof( Single );
      protected static readonly Type DOUBLE_TYPE_NATIVE = typeof( Double );
      protected static readonly Type INT_PTR_TYPE_NATIVE = typeof( IntPtr );
      protected static readonly Type STRING_TYPE_NATIVE = typeof( String );
      protected static readonly Type EXCEPTION_TYPE_NATIVE = typeof( Exception );
      //protected static readonly Type WEAK_EVENT_WRAPPER_TYPE_NATIVE = typeof( WeakEventHandlerWrapperForCodeGeneration );
      //protected static readonly Type STRONG_EVENT_WRAPPER_TYPE_NATIVE = typeof( EventHandlerInfoForCodeGeneration );
      protected static readonly Type IENUMERABLE_GDEF_TYPE_NATIVE = typeof( IEnumerable<> );
      protected static readonly Type IENUMERABLE_NO_GDEF_TYPE_NATIVE = typeof( System.Collections.IEnumerable );
      protected static readonly Type USE_DEFAULTS_ATTRIBUTE_TYPE_NATIVE = typeof( UseDefaultsAttribute );
      protected static readonly Type COMPOSITE_FACTORY_TYPE_NATIVE = typeof( CompositeFactory );
      protected static readonly Type REF_ACTION_TYPE_NATIVE = typeof( ActionWithRef<> );
      protected static readonly Type REF_FUNCTION_TYPE_NATIVE = typeof( FunctionWithRef<> );

      protected static readonly Type[] EXCEPTION_ENUMERABLE_ARRAY = new Type[] { typeof( IEnumerable<Exception> ) };

      //protected static readonly Type[] VOLATILE_FIELD_REQ_MODS = new Type[] { typeof( System.Runtime.CompilerServices.IsVolatile ) };

      protected static readonly MethodInfo APPLICATION_GETTER_METHOD_NATIVE;
      protected static readonly MethodInfo STRUCTURE_OWNER_GETTER_METHOD_NATIVE;
      protected static readonly MethodInfo APPLICATION_IS_PASSIVE_GETTER_METHOD_NATIVE;
      protected static readonly MethodInfo INJECTION_SERVICE_GETTER_METHOD_NATIVE;
      protected static readonly MethodInfo COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER_NATIVE;
      protected static readonly MethodInfo COMPOSITE_METHOD_RESULT_GETTER_NATIVE;
      //protected static readonly MethodInfo INJECTABLE_MODEL_INJECTION_SCOPES_GETTER_NATIVE;
      protected static readonly MethodInfo INJECTION_CONTEXT_PROVIDER_METHOD_NATIVE;
      protected static readonly FieldInfo OPTIONAL_ATTRIBUTE_FIELD_NATIVE;
      protected static readonly MethodInfo CONSTRAINT_MODELS_GETTER_NATIVE;
      protected static readonly MethodInfo CONSTRAINT_ATTRIBUTE_GETTER_NATIVE;
      protected static readonly MethodInfo VIOLATIONS_LIST_COUNT_GETTER_NATIVE;
      protected static readonly MethodInfo METHOD_INFO_NATIVE_GETTER_NATIVE;
      protected static readonly MethodInfo MODEL_INFO_GETTER_NATIVE;
      protected static readonly MethodInfo MODEL_GETTER_NATIVE;
      protected static readonly MethodInfo C_METHODS_GETTER_NATIVE;
      protected static readonly MethodInfo GET_FRAGMENT_INSTANCE_POOL_METHOD_NATIVE;
      protected static readonly MethodInfo GET_FRAGMENT_INSTANCE_METHOD_NATIVE;
      protected static readonly MethodInfo TAKE_FRAGMENT_INSTANCE_METHOD_NATIVE;
      protected static readonly MethodInfo RETURN_FRAGMENT_INSTANCE_METHOD_NATIVE;
      protected static readonly MethodInfo SPECIAL_METHODS_GETTER_NATIVE;
      protected static readonly MethodInfo FRAGMENT_GETTER_NATIVE;
      protected static readonly MethodInfo FRAGMENT_SETTER_NATIVE;
      protected static readonly MethodInfo COMPOSITES_GETTER_NATIVE;
      protected static readonly MethodInfo COMPOSITES_GETTER_INDEXER_NATIVE;
      protected static readonly MethodInfo TYPE_OBJECT_DICTIONARY_GET_METHOD_NATIVE;
      protected static readonly MethodInfo GENERIC_FRAGMENT_METHOD_NATIVE;
      protected static readonly ConstructorInfo CONSTRAINT_VIOLATION_CONSTRUCTOR_NATIVE;
      protected static readonly MethodInfo ADD_CONSTRAINT_VIOLATION_METHOD_NATIVE;
      protected static readonly MethodInfo F_INSTANCE_SET_NEXT_INFO_METHOD_NATIVE;
      protected static readonly MethodInfo F_INSTANCE_SET_METHOD_RESULT_METHOD_NATIVE;
      protected static readonly MethodInfo F_INSTANCE_GET_NEXT_INFO_METHOD_NATIVE;
      protected static readonly MethodInfo F_INSTANCE_GET_METHOD_RESULT_METHOD_NATIVE;
      protected static readonly MethodInfo STRING_CONCAT_METHOD_3_NATIVE;
      protected static readonly MethodInfo STRING_CONCAT_METHOD_2_NATIVE;
      protected static readonly MethodInfo METHOD_INFO_GET_GARGS_METHOD_NATIVE;
      protected static readonly MethodInfo MAKE_GENERIC_METHOD_METHOD_NATIVE;
      protected static readonly MethodInfo INVOKE_METHOD_METHOD_NATIVE;
      protected static readonly MethodInfo GET_METHOD_METHOD_NATIVE;
      protected static readonly MethodInfo GET_CTOR_INDEX_METHOD_NATIVE;
      protected static readonly MethodInfo BASE_TYPE_GETTER_NATIVE;
      protected static readonly ConstructorInfo APPLICATION_NOT_ACTIVE_EXCEPTION_CONSTRUCTOR_NATIVE;
      protected static readonly MethodInfo GET_CONSTRAINT_INSTANCE_POOL_METHOD_NATIVE;
      protected static readonly MethodInfo TAKE_CONSTRAINT_INSTANCE_METHOD_NATIVE;
      protected static readonly MethodInfo RETURN_CONSTRAINT_INSTANCE_METHOD_NATIVE;
      protected static readonly MethodInfo IS_VALID_METHOD_NATIVE;
      protected static readonly MethodInfo TO_STRING_METHOD_NATIVE;
      protected static readonly MethodInfo RETURN_CONCERN_INVOCATION_METHOD_NATIVE;
      protected static readonly MethodInfo RETURN_SIDE_EFFECT_INVOCATION_METHOD_NATIVE;
      protected static readonly FieldInfo EMPTY_OBJECTS_FIELD_NATIVE;
      protected static readonly MethodInfo LIST_QUERY_ITEM_GETTER_NATIVE;
      protected static readonly MethodInfo FIELDS_GETTER_NATIVE;
      protected static readonly MethodInfo FIELD_SET_VALUE_METHOD_NATIVE;
      protected static readonly ConstructorInfo PROTOTYPE_ACTION_CONSTRUCTOR_NATIVE;
      protected static readonly MethodInfo EQUALS_METHOD_NATIVE;
      protected static readonly MethodInfo HASH_CODE_METHOD_NATIVE;
      protected static readonly MethodInfo REFERENCE_EQUALS_METHOD_NATIVE;
      protected static readonly MethodInfo GET_TYPE_METHOD_NATIVE;
      protected static readonly MethodInfo ASSEMBLY_GETTER_NATIVE;
      protected static readonly MethodInfo DELEGATE_COMBINE_METHOD_NATIVE;
      protected static readonly MethodInfo DELEGATE_REMOVE_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF_NATIVE;
      protected static readonly MethodInfo GET_EVENT_INFO_METHOD_NATIVE;
      protected static readonly ConstructorInfo COMPOSITE_EVENT_CTOR_NATIVE;
      protected static readonly ConstructorInfo INVALID_OPERATION_EXCEPTION_CTOR_WITH_STRING_NATIVE;
      protected static readonly MethodInfo QNAME_FROM_TYPE_AND_NAME_NATIVE;
      protected static readonly MethodInfo IS_PROTOTYPE_GETTER_NATIVE;
      protected static readonly MethodInfo GET_PROPERTY_INFO_METHOD_NATIVE;
      protected static readonly MethodInfo COMPOSITE_METHODS_INDEXER_NATIVE;
      protected static readonly MethodInfo EVENT_MODEL_GETTER_NATIVE;
      protected static readonly MethodInfo PROPERTY_MODEL_GETTER_NATIVE;
      protected static readonly ConstructorInfo COMPOSITE_PROPERTY_CTOR_NATIVE;
      protected static readonly MethodInfo INVOCATION_INFO_GETTER_NATIVE;
      protected static readonly MethodInfo INVOCATION_INFO_SETTER_NATIVE;
      protected static readonly ConstructorInfo INVOCATION_INFO_CREATOR_CTOR_NATIVE;
      protected static readonly MethodInfo INVOCATION_INFO_METHOD_GETTER_NATIVE;
      protected static readonly MethodInfo INVOCATION_INFO_FRAGMENT_METHOD_MODEL_GETTER_NATIVE;
      protected static readonly MethodInfo INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER_NATIVE;
      protected static readonly MethodInfo CONCERN_MODELS_GETTER_NATIVE;
      protected static readonly MethodInfo CONCERN_MODELS_INDEXER_NATIVE;
      protected static readonly MethodInfo MIXIN_MODEL_GETTER_NATIVE;
      protected static readonly MethodInfo SIDE_EFFECT_MODELS_GETTER_NATIVE;
      protected static readonly MethodInfo SIDE_EFFECT_MODELS_INDEXER_NATIVE;
      protected static readonly MethodInfo COLLECTION_ADD_ONLY_ADD_METHOD_NATIVE;
      protected static readonly ConstructorInfo ACTION_0_CTOR_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_EXCHANGE_METHOD_GDEF_NATIVE;
      protected static readonly MethodInfo GET_INVOCATION_LIST_METHOD_NATIVE;
      protected static readonly MethodInfo ADD_LAST_METHOD_NATIVE;
      //protected static readonly MethodInfo WEAK_EVENT_ARRAY_WRAPPER_ARRAY_GETTER_NATIVE;
      //protected static readonly MethodInfo WEAK_EVENT_ARRAY_WRAPPER_COUNT_GETTER_NATIVE;
      //protected static readonly MethodInfo WEAK_EVENT_ARRAY_CLEANUP_METHOD_NATIVE;
      //protected static readonly MethodInfo WEAK_EVENT_ARRAY_COMBINE_METHOD_NATIVE;
      //protected static readonly MethodInfo WEAK_EVENT_ARRAY_REMOVE_METHOD_NATIVE;
      //protected static readonly MethodInfo IS_EVENT_INFO_DEAD_METHOD_NATIVE;
      //protected static readonly MethodInfo EVENT_INFO_TARGET_GETTER_NATIVE;
      //protected static readonly MethodInfo EVENT_INFO_METHOD_GETTER_NATIVE;
      //protected static readonly ConstructorInfo EVENT_INFO_CTOR_NATIVE;
      protected static readonly MethodInfo Q_NAME_GET_BARE_TYPE_NAME_METHOD_NATIVE;
      protected static readonly MethodInfo Q_NAME_FROM_MEMBER_INFO_METHOD_NATIVE;
      protected static readonly ConstructorInfo INJECTION_EXCEPTION_CTOR_NATIVE;
      protected static readonly MethodInfo CHECK_STATE_METHOD_SIG_NATIVE;
      protected static readonly MethodInfo CONSTRAINT_EXCEPTION_VIOLATIONS_GETTER_NATIVE;
      protected static readonly MethodInfo CONSTRAINT_VIOLATIONS_DIC_ADD_METHOD_NATIVE;
      protected static readonly ConstructorInfo CONSTRAINT_VIOLATIONS_DIC_CTOR_NATIVE;
      protected static readonly ConstructorInfo CHECK_ACTION_FUNC_CTOR_NATIVE;
      protected static readonly MethodInfo SET_DEFAULTS_METHOD_SIG_NATIVE;
      protected static readonly ConstructorInfo CONSTRAINT_VIOLATIONS_LIST_CTOR_NATIVE;
      protected static readonly ConstructorInfo EXCEPTION_LIST_CTOR_NATIVE;
      protected static readonly ConstructorInfo FUNC_1_CTOR_NATIVE;
      protected static readonly ConstructorInfo FUNC_2_CTOR_NATIVE;
      protected static readonly ConstructorInfo FUNC_3_CTOR_NATIVE;
      protected static readonly ConstructorInfo ACTION_1_CTOR_NATIVE;
      protected static readonly ConstructorInfo LAZY_GDEF_CTOR_NATIVE;
      protected static readonly ConstructorInfo FRAGMENT_INSTANCE_CTOR_NO_PARAMS_NATIVE;
      protected static readonly ConstructorInfo FRAGMENT_INSTANCE_CTOR_WITH_PARAMS_NATIVE;
      protected static readonly MethodInfo MODEL_CTORS_GETTER_NATIVE;
      protected static readonly MethodInfo FRAGMENT_DEPENDANT_GETTER_NATIVE;
      protected static readonly MethodInfo FRAGMENT_DEPENDANT_SETTER_NATIVE;
      protected static readonly PropertyInfo FRAGMENT_DEPENDANT_PROPERTY_NATIVE;
      protected static readonly MethodInfo CONCERN_INVOCATION_INFO_ITEM_1_NATIVE;
      protected static readonly MethodInfo CONCERN_INVOCATION_INFO_ITEM_2_NATIVE;
      protected static readonly MethodInfo SIDE_EFFECT_INVOCATION_INFO_ITEM_1_NATIVE;
      protected static readonly MethodInfo SIDE_EFFECT_INVOCATION_INFO_ITEM_2_NATIVE;
      protected static readonly MethodInfo SIDE_EFFECT_INVOCATION_INFO_ITEM_3_NATIVE;
      protected static readonly ConstructorInfo OBJECT_CTOR_NATIVE;
      protected static readonly ConstructorInfo NULLABLE_CTOR_NATIVE;
      protected static readonly MethodInfo DEFAULT_CREATOR_GETTER_NATIVE;
      protected static readonly MethodInfo DEFAULT_CREATOR_INVOKER_NATIVE;
      protected static readonly ConstructorInfo NO_POOL_ATTRIBUTE_CTOR_NATIVE;
      protected static readonly ConstructorInfo MAIN_PUBLIC_COMPOSITE_TYPE_ATTRIBUTE_CTOR_NATIVE;
      protected static readonly ConstructorInfo CONSTRAINT_VIOLATION_LIST_CTOR_NATIVE;
      protected static readonly ConstructorInfo CONSTRAINT_VIOLATION_EXCEPTION_CTOR_NATIVE;
      protected static readonly ConstructorInfo INTERNAL_EXCEPTION_CTOR_NATIVE;
      protected static readonly ConstructorInfo AGGREGATE_EXCEPTION_EXCEPTION_ENUMERABLE_CTOR_NATIVE;
      protected static readonly ConstructorInfo DEBUGGER_DISPLAY_ATTRIBUTE_STRING_CTOR_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_READ_I64_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_EXCHANGE_I32_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_EXCHANGE_I64_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_EXCHANGE_R32_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_EXCHANGE_R64_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_EXCHANGE_INT_PTR_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_EXCHANGE_OBJECT_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_COMPARE_EXCHANGE_I64_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD_NATIVE;
      protected static readonly MethodInfo INTERLOCKED_COMPARE_EXCHANGE_OBJECT_METHOD_NATIVE;
      protected static readonly ConstructorInfo COMPOSITE_TYPE_ID_CTOR_NATIVE;
      protected static readonly MethodInfo COMPOSITE_FACTORY_METHOD_NATIVE;
      protected static readonly ConstructorInfo ARGUMENT_EXCEPTION_STRING_CTOR_NATIVE;
      protected static readonly MethodInfo MAKE_GENERIC_TYPE_METHOD_NATIVE;
      protected static readonly MethodInfo GET_CONSTRUCTORS_METHOD_NATIVE;
      protected static readonly MethodInfo CONSTRUCTOR_INVOKE_METHOD_NATIVE;
      protected static readonly MethodInfo INT_PTR_SIZE_GETTER_NATIVE;
      protected static readonly MethodInfo REF_ACTION_INVOKER_NATIVE;
      protected static readonly MethodInfo REF_FUNCTION_INVOKER_NATIVE;
      protected static readonly ConstructorInfo REF_INVOKER_CALLBACK_CTOR_NATIVE;
      protected static readonly ConstructorInfo COMPOSITE_METHOD_MODEL_INDEX_ATTRIBUTE_NATIVE;
      protected static readonly ConstructorInfo SPECIAL_METHOD_MODEL_INDEX_ATTRIBUTE_NATIVE;
      protected static readonly ConstructorInfo CONSTRUCTOR_MODEL_INDEX_ATTRIBUTE_NATIVE;
      protected static readonly MethodInfo DOUBLE_BITS_TO_INT64_NATIVE;
      protected static readonly MethodInfo INT64_BITS_TO_DOUBLE_NATIVE;
      protected static readonly MethodInfo GET_BYTES_INT32_NATIVE;
      protected static readonly MethodInfo GET_BYTES_SINGLE_NATIVE;
      protected static readonly MethodInfo BYTES_TO_INT32_NATIVE;
      protected static readonly MethodInfo BYTES_TO_SINGLE_NATIVE;
      //protected static readonly MethodInfo CONVERT_I64_DOUBLE_NATIVE;
      //protected static readonly MethodInfo CONVERT_DOUBLE_I64_NATIVE;
      //protected static readonly MethodInfo CONVERT_I32_SINGLE_NATIVE;
      //protected static readonly MethodInfo CONVERT_SINGLE_I32_NATIVE;

      static AbstractCompositeModelTypeCodeGenerator()
      {
         STRUCTURE_OWNER_GETTER_METHOD_NATIVE = typeof( CompositeInstanceImpl ).LoadGetterOrThrow( "StructureOwner" );
         APPLICATION_GETTER_METHOD_NATIVE = typeof( CompositeInstanceStructureOwner ).LoadGetterOrThrow( "Application" );
         APPLICATION_IS_PASSIVE_GETTER_METHOD_NATIVE = typeof( Application ).LoadGetterOrThrow( "Passive" );
         INJECTION_SERVICE_GETTER_METHOD_NATIVE = typeof( ApplicationSPI ).LoadGetterOrThrow( "InjectionService" );
         COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER_NATIVE = typeof( AbstractModelWithParameters ).LoadGetterOrThrow( "Parameters" );
         COMPOSITE_METHOD_RESULT_GETTER_NATIVE = typeof( CompositeMethodModel ).LoadGetterOrThrow( "Result" );
         //INJECTABLE_MODEL_INJECTION_SCOPES_GETTER_NATIVE = TypeUtil.LoadGetterOrThrow( typeof( AbstractInjectableModel ), "InjectionScope" );
         INJECTION_CONTEXT_PROVIDER_METHOD_NATIVE = typeof( InjectionFunctionality ).LoadMethodOrThrow( "ProvideInjection", null );
         OPTIONAL_ATTRIBUTE_FIELD_NATIVE = typeof( OptionalAttribute ).LoadFieldOrThrow( "VALUE" );
         CONSTRAINT_MODELS_GETTER_NATIVE = typeof( ParameterModel ).LoadGetterOrThrow( "Constraints" );
         CONSTRAINT_ATTRIBUTE_GETTER_NATIVE = typeof( ConstraintModel ).LoadGetterOrThrow( "ConstraintAttribute" );
         VIOLATIONS_LIST_COUNT_GETTER_NATIVE = typeof( ICollection<ConstraintViolationInfo> ).LoadGetterOrThrow( "Count" );
         METHOD_INFO_NATIVE_GETTER_NATIVE = typeof( AbstractMemberInfoModel<MethodInfo> ).LoadGetterOrThrow( "NativeInfo" );
         MODEL_INFO_GETTER_NATIVE = typeof( CompositeInstanceImpl ).LoadGetterOrThrow( "ModelInfo" );
         MODEL_GETTER_NATIVE = typeof( CompositeModelInfo ).LoadGetterOrThrow( "Model" );
         C_METHODS_GETTER_NATIVE = typeof( CompositeModel ).LoadGetterOrThrow( "Methods" );
         GET_FRAGMENT_INSTANCE_POOL_METHOD_NATIVE = typeof( CompositeInstanceImpl ).LoadMethodOrThrow( "GetInstancePoolForFragment", null );
         GET_FRAGMENT_INSTANCE_METHOD_NATIVE = typeof( CompositeInstanceImpl ).LoadMethodOrThrow( "GetInstanceForFragment", null );
         TAKE_FRAGMENT_INSTANCE_METHOD_NATIVE = typeof( InstancePool<FragmentInstance> ).LoadMethodOrThrow( "TryTake", null );
         RETURN_FRAGMENT_INSTANCE_METHOD_NATIVE = typeof( InstancePool<FragmentInstance> ).LoadMethodOrThrow( "Return", null );
         SPECIAL_METHODS_GETTER_NATIVE = typeof( CompositeModel ).LoadGetterOrThrow( "SpecialMethods" );
         FRAGMENT_GETTER_NATIVE = typeof( FragmentInstance ).LoadGetterOrThrow( "Fragment" );
         FRAGMENT_SETTER_NATIVE = typeof( FragmentInstance ).LoadSetterOrThrow( "Fragment" );
         COMPOSITES_GETTER_NATIVE = typeof( CompositeInstanceImpl ).LoadGetterOrThrow( "Composites" );
         COMPOSITES_GETTER_INDEXER_NATIVE = COMPOSITES_GETTER_NATIVE.ReturnType.LoadGetterOrThrow( "Item" );
         TYPE_OBJECT_DICTIONARY_GET_METHOD_NATIVE = typeof( IDictionary<Type, Object> ).LoadGetterOrThrow( "Item" );
         GENERIC_FRAGMENT_METHOD_NATIVE = typeof( GenericInvocator ).LoadMethodOrThrow( "Invoke", null );
         CONSTRAINT_VIOLATION_CONSTRUCTOR_NATIVE = typeof( ConstraintViolationInfo ).LoadConstructorOrThrow( (Int32?) null );
         ADD_CONSTRAINT_VIOLATION_METHOD_NATIVE = typeof( ICollection<ConstraintViolationInfo> ).LoadMethodOrThrow( "Add", 1 );
         F_INSTANCE_SET_NEXT_INFO_METHOD_NATIVE = typeof( FragmentInstance ).LoadMethodOrThrow( "SetNextInfo", null );
         F_INSTANCE_SET_METHOD_RESULT_METHOD_NATIVE = typeof( FragmentInstance ).LoadMethodOrThrow( "SetMethodResult", null );
         F_INSTANCE_GET_NEXT_INFO_METHOD_NATIVE = typeof( FragmentInstance ).LoadMethodOrThrow( "GetNextInfo", null );
         F_INSTANCE_GET_METHOD_RESULT_METHOD_NATIVE = typeof( FragmentInstance ).LoadMethodOrThrow( "GetMethodResult", null );
         STRING_CONCAT_METHOD_3_NATIVE = typeof( String ).LoadMethodWithParamTypesOrThrow( "Concat", new Type[] { typeof( Object ), typeof( Object ), typeof( Object ) } );
         STRING_CONCAT_METHOD_2_NATIVE = typeof( String ).LoadMethodWithParamTypesOrThrow( "Concat", new Type[] { typeof( Object ), typeof( Object ) } );
         METHOD_INFO_GET_GARGS_METHOD_NATIVE = typeof( MethodBase ).LoadMethodOrThrow( "GetGenericArguments", null );
         MAKE_GENERIC_METHOD_METHOD_NATIVE = typeof( MethodInfo ).LoadMethodOrThrow( "MakeGenericMethod", null );
         INVOKE_METHOD_METHOD_NATIVE = typeof( MethodBase ).LoadMethodOrThrow( "Invoke", 2 );
         GET_METHOD_METHOD_NATIVE = typeof( Type ).LoadMethodWithParamTypesOrThrow( "GetMethod", new Type[] { typeof( String ), typeof( BindingFlags ) } );
         GET_CTOR_INDEX_METHOD_NATIVE = typeof( CompositeInstanceImpl ).LoadMethodOrThrow( "GetConstructorIndex", null );
         BASE_TYPE_GETTER_NATIVE = typeof( Type ).LoadGetterOrThrow( "BaseType" );
         APPLICATION_NOT_ACTIVE_EXCEPTION_CONSTRUCTOR_NATIVE = typeof( ApplicationNotActiveException ).LoadConstructorOrThrow( 0 );
         GET_CONSTRAINT_INSTANCE_POOL_METHOD_NATIVE = typeof( ApplicationSPI ).LoadMethodOrThrow( "GetConstraintInstancePool", null );
         TAKE_CONSTRAINT_INSTANCE_METHOD_NATIVE = typeof( InstancePool<Object> ).LoadMethodOrThrow( "TryPeek", null );
         RETURN_CONSTRAINT_INSTANCE_METHOD_NATIVE = typeof( InstancePool<Object> ).LoadMethodOrThrow( "Return", null );
         IS_VALID_METHOD_NATIVE = typeof( Constraint<,> ).LoadMethodOrThrow( "IsValid", 2 );
         TO_STRING_METHOD_NATIVE = typeof( Object ).LoadMethodOrThrow( "ToString", 0 );
         RETURN_CONCERN_INVOCATION_METHOD_NATIVE = typeof( CompositeInstanceImpl ).LoadMethodOrThrow( "ReturnConcernInvocation", null );
         RETURN_SIDE_EFFECT_INVOCATION_METHOD_NATIVE = typeof( CompositeInstanceImpl ).LoadMethodOrThrow( "ReturnSideEffectInvocation", null );
         EMPTY_OBJECTS_FIELD_NATIVE = typeof( CodeGenerationConstants ).LoadFieldOrThrow( "EMPTY_OBJECTS" );
         LIST_QUERY_ITEM_GETTER_NATIVE = typeof( CollectionQueryWithIndexer<> ).LoadGetterOrThrow( "Item" );
         FIELDS_GETTER_NATIVE = typeof( CompositeModel ).LoadGetterOrThrow( "Fields" );
         FIELD_SET_VALUE_METHOD_NATIVE = typeof( FieldInfo ).LoadMethodOrThrow( "SetValue", 2 );
         PROTOTYPE_ACTION_CONSTRUCTOR_NATIVE = typeof( Action<CompositeInstanceImpl> ).LoadConstructorOrThrow( 2 );
         EQUALS_METHOD_NATIVE = typeof( Object ).LoadMethodOrThrow( "Equals", 1 );
         HASH_CODE_METHOD_NATIVE = typeof( Object ).LoadMethodOrThrow( "GetHashCode", null );
         REFERENCE_EQUALS_METHOD_NATIVE = typeof( Object ).LoadMethodOrThrow( "ReferenceEquals", null );
         GET_TYPE_METHOD_NATIVE = typeof( Object ).LoadMethodOrThrow( "GetType", null );
         ASSEMBLY_GETTER_NATIVE = typeof( Type ).LoadGetterOrThrow( "Assembly" );
         DELEGATE_COMBINE_METHOD_NATIVE = typeof( Delegate ).LoadMethodOrThrow( "Combine", 2 );
         DELEGATE_REMOVE_METHOD_NATIVE = typeof( Delegate ).LoadMethodOrThrow( "Remove", 2 );
         INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF_NATIVE = typeof( Interlocked ).LoadMethodGDefinitionOrThrow( "CompareExchange" );
         GET_EVENT_INFO_METHOD_NATIVE = typeof( Type ).LoadMethodWithParamTypesOrThrow( "GetEvent", new[] { typeof( String ), typeof( System.Reflection.BindingFlags ) } );
         COMPOSITE_EVENT_CTOR_NATIVE = typeof( CompositeEventImpl<> ).LoadConstructorOrThrow( (Int32?) null );
         INVALID_OPERATION_EXCEPTION_CTOR_WITH_STRING_NATIVE = typeof( InvalidOperationException ).LoadConstructorOrThrow( new Type[] { typeof( String ) } );
         QNAME_FROM_TYPE_AND_NAME_NATIVE = typeof( QualifiedName ).LoadMethodOrThrow( "FromTypeAndName", null );
         IS_PROTOTYPE_GETTER_NATIVE = typeof( CompositeInstanceImpl ).LoadGetterOrThrow( "IsPrototype" );
         GET_PROPERTY_INFO_METHOD_NATIVE = typeof( Type ).LoadMethodWithParamTypesOrThrow( "GetProperty", new[] { typeof( String ), typeof( System.Reflection.BindingFlags ) } );
         COMPOSITE_METHODS_INDEXER_NATIVE = typeof( CollectionQueryWithIndexer<CompositeMethodModel> ).LoadGetterOrThrow( "Item" );
         EVENT_MODEL_GETTER_NATIVE = typeof( CompositeMethodModel ).LoadGetterOrThrow( "EventModel" );
         PROPERTY_MODEL_GETTER_NATIVE = typeof( CompositeMethodModel ).LoadGetterOrThrow( "PropertyModel" );
         COMPOSITE_PROPERTY_CTOR_NATIVE = typeof( CompositePropertyImpl<> ).LoadConstructorOrThrow( (Int32?) null );
         INVOCATION_INFO_GETTER_NATIVE = typeof( CompositeInstanceImpl ).LoadGetterOrThrow( "InvocationInfo" );
         INVOCATION_INFO_SETTER_NATIVE = typeof( CompositeInstanceImpl ).LoadSetterOrThrow( "InvocationInfo" );
         INVOCATION_INFO_CREATOR_CTOR_NATIVE = typeof( InvocationInfoImpl ).LoadConstructorOrThrow( (Int32?) null );
         INVOCATION_INFO_METHOD_GETTER_NATIVE = typeof( InvocationInfo ).LoadGetterOrThrow( "CompositeMethod" );
         INVOCATION_INFO_FRAGMENT_METHOD_MODEL_GETTER_NATIVE = typeof( InvocationInfo ).LoadGetterOrThrow( "FragmentMethodModel" );
         INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER_NATIVE = typeof( InvocationInfoImpl ).LoadSetterOrThrow( "FragmentMethodModel" );
         CONCERN_MODELS_GETTER_NATIVE = typeof( CompositeMethodModel ).LoadGetterOrThrow( "Concerns" );
         CONCERN_MODELS_INDEXER_NATIVE = typeof( CollectionQueryWithIndexer<ConcernMethodModel> ).LoadMethodOrThrow( "get_Item", null );
         MIXIN_MODEL_GETTER_NATIVE = typeof( CompositeMethodModel ).LoadGetterOrThrow( "Mixin" );
         SIDE_EFFECT_MODELS_GETTER_NATIVE = typeof( CompositeMethodModel ).LoadGetterOrThrow( "SideEffects" );
         SIDE_EFFECT_MODELS_INDEXER_NATIVE = typeof( CollectionQueryWithIndexer<SideEffectMethodModel> ).LoadMethodOrThrow( "get_Item", null );
         COLLECTION_ADD_ONLY_ADD_METHOD_NATIVE = typeof( CollectionAdditionOnly<> ).LoadMethodOrThrow( "Add", null );
         ACTION_0_CTOR_NATIVE = typeof( Action ).LoadConstructorOrThrow( 2 );
         INTERLOCKED_EXCHANGE_METHOD_GDEF_NATIVE = typeof( Interlocked ).LoadMethodGDefinitionOrThrow( "Exchange" );
         GET_INVOCATION_LIST_METHOD_NATIVE = typeof( MulticastDelegate ).LoadMethodOrThrow( "GetInvocationList", null );
         ADD_LAST_METHOD_NATIVE = typeof( LinkedList<> ).LoadMethodWithParamTypesOrThrow( "AddLast", typeof( LinkedList<> ).GetGenericArguments() );
         //WEAK_EVENT_ARRAY_WRAPPER_ARRAY_GETTER_NATIVE = typeof( WeakEventHandlerWrapperForCodeGeneration ).LoadGetterOrThrow( "Array" );
         //WEAK_EVENT_ARRAY_WRAPPER_COUNT_GETTER_NATIVE = typeof( WeakEventHandlerWrapperForCodeGeneration ).LoadGetterOrThrow( "ElementCount" );
         //WEAK_EVENT_ARRAY_CLEANUP_METHOD_NATIVE = typeof( WeakEventHandlerWrapperForCodeGeneration ).LoadMethodOrThrow( "CleanUp", null );
         //WEAK_EVENT_ARRAY_COMBINE_METHOD_NATIVE = typeof( WeakEventHandlerWrapperForCodeGeneration ).LoadMethodOrThrow( "Combine", null );
         //WEAK_EVENT_ARRAY_REMOVE_METHOD_NATIVE = typeof( WeakEventHandlerWrapperForCodeGeneration ).LoadMethodOrThrow( "Remove", null );
         //IS_EVENT_INFO_DEAD_METHOD_NATIVE = typeof( WeakEventHandlerWrapperForCodeGeneration ).LoadMethodOrThrow( "IsDead", null );
         //EVENT_INFO_TARGET_GETTER_NATIVE = typeof( EventHandlerInfoForCodeGeneration ).LoadGetterOrThrow( "Target" );
         //EVENT_INFO_METHOD_GETTER_NATIVE = typeof( EventHandlerInfoForCodeGeneration ).LoadGetterOrThrow( "Method" );
         //EVENT_INFO_CTOR_NATIVE = typeof( EventHandlerInfoForCodeGeneration ).LoadConstructorOrThrow( (Int32?) null );
         Q_NAME_GET_BARE_TYPE_NAME_METHOD_NATIVE = typeof( QualifiedName ).LoadMethodOrThrow( "GetBareTypeName", null );
         Q_NAME_FROM_MEMBER_INFO_METHOD_NATIVE = typeof( QualifiedName ).LoadMethodOrThrow( "FromMemberInfo", null );
         INJECTION_EXCEPTION_CTOR_NATIVE = typeof( InjectionException ).LoadConstructorOrThrow( (Int32?) null );
         CHECK_STATE_METHOD_SIG_NATIVE = CHECK_STATE_FUNC_TYPE_NATIVE.LoadMethodOrThrow( "Invoke", null );
         CONSTRAINT_EXCEPTION_VIOLATIONS_GETTER_NATIVE = typeof( ConstraintViolationException ).LoadGetterOrThrow( "Violations" );
         CONSTRAINT_VIOLATIONS_DIC_ADD_METHOD_NATIVE = typeof( IDictionary<QualifiedName, IList<ConstraintViolationInfo>> ).LoadMethodOrThrow( "Add", 2 );
         CONSTRAINT_VIOLATIONS_DIC_CTOR_NATIVE = typeof( Dictionary<,> ).MakeGenericType( CONSTRAINT_VIOLATIONS_DIC_ADD_METHOD_NATIVE.DeclaringType.GetGenericArguments() ).LoadConstructorOrThrow( 0 );
         CHECK_ACTION_FUNC_CTOR_NATIVE = CHECK_STATE_FUNC_TYPE_NATIVE.LoadConstructorOrThrow( 2 );
         SET_DEFAULTS_METHOD_SIG_NATIVE = ACTION_TYPE_NATIVE.LoadMethodOrThrow( "Invoke", null );
         CONSTRAINT_VIOLATIONS_LIST_CTOR_NATIVE = typeof( List<ConstraintViolationInfo> ).LoadConstructorOrThrow( 0 );
         EXCEPTION_LIST_CTOR_NATIVE = typeof( LinkedList<Exception> ).LoadConstructorOrThrow( 0 );
         FUNC_1_CTOR_NATIVE = typeof( Func<> ).LoadConstructorOrThrow( 2 );
         FUNC_2_CTOR_NATIVE = typeof( Func<,> ).LoadConstructorOrThrow( 2 );
         FUNC_3_CTOR_NATIVE = typeof( Func<,,> ).LoadConstructorOrThrow( 2 );
         ACTION_1_CTOR_NATIVE = typeof( Action<> ).LoadConstructorOrThrow( 2 );
         LAZY_GDEF_CTOR_NATIVE = typeof( Lazy<> ).LoadConstructorOrThrow( new Type[] { FUNC_1_CTOR_NATIVE.DeclaringType.MakeGenericType( typeof( Lazy<> ).GetGenericArguments()[0] ), typeof( System.Threading.LazyThreadSafetyMode ) } );
         FRAGMENT_INSTANCE_CTOR_NO_PARAMS_NATIVE = typeof( FragmentInstanceImpl ).LoadConstructorOrThrow( 0 );
         FRAGMENT_INSTANCE_CTOR_WITH_PARAMS_NATIVE = typeof( FragmentInstanceImpl ).LoadConstructorOrThrow( 4 );
         MODEL_CTORS_GETTER_NATIVE = typeof( CompositeModel ).LoadGetterOrThrow( "Constructors" );
         FRAGMENT_DEPENDANT_PROPERTY_NATIVE = typeof( FragmentDependant ).LoadPropertyOrThrow( "Fragment" );
         FRAGMENT_DEPENDANT_GETTER_NATIVE = FRAGMENT_DEPENDANT_PROPERTY_NATIVE.GetGetMethod();
         FRAGMENT_DEPENDANT_SETTER_NATIVE = FRAGMENT_DEPENDANT_PROPERTY_NATIVE.GetSetMethod();
         CONCERN_INVOCATION_INFO_ITEM_1_NATIVE = F_INSTANCE_GET_NEXT_INFO_METHOD_NATIVE.ReturnType.LoadGetterOrThrow( "Item1" );
         CONCERN_INVOCATION_INFO_ITEM_2_NATIVE = F_INSTANCE_GET_NEXT_INFO_METHOD_NATIVE.ReturnType.LoadGetterOrThrow( "Item2" );
         SIDE_EFFECT_INVOCATION_INFO_ITEM_1_NATIVE = F_INSTANCE_GET_METHOD_RESULT_METHOD_NATIVE.ReturnType.LoadGetterOrThrow( "Item1" );
         SIDE_EFFECT_INVOCATION_INFO_ITEM_2_NATIVE = F_INSTANCE_GET_METHOD_RESULT_METHOD_NATIVE.ReturnType.LoadGetterOrThrow( "Item2" );
         SIDE_EFFECT_INVOCATION_INFO_ITEM_3_NATIVE = F_INSTANCE_GET_METHOD_RESULT_METHOD_NATIVE.ReturnType.LoadGetterOrThrow( "Item3" );
         OBJECT_CTOR_NATIVE = typeof( Object ).LoadConstructorOrThrow( 0 );
         NULLABLE_CTOR_NATIVE = typeof( Nullable<> ).LoadConstructorOrThrow( 1 );
         DEFAULT_CREATOR_GETTER_NATIVE = typeof( PropertyModel ).LoadGetterOrThrow( "DefaultValueCreator" );
         DEFAULT_CREATOR_INVOKER_NATIVE = DEFAULT_CREATOR_GETTER_NATIVE.ReturnType.LoadMethodOrThrow( "Invoke", null );
         NO_POOL_ATTRIBUTE_CTOR_NATIVE = typeof( NoPoolNeededAttribute ).LoadConstructorOrThrow( (Int32?) null );
         MAIN_PUBLIC_COMPOSITE_TYPE_ATTRIBUTE_CTOR_NATIVE = typeof( MainPublicCompositeTypeAttribute ).LoadConstructorOrThrow( (Int32?) null );
         CONSTRAINT_VIOLATION_LIST_CTOR_NATIVE = typeof( List<ConstraintViolationInfo> ).LoadConstructorOrThrow( 0 );
         CONSTRAINT_VIOLATION_EXCEPTION_CTOR_NATIVE = typeof( ConstraintViolationException ).LoadConstructorOrThrow( 2 );
         INTERNAL_EXCEPTION_CTOR_NATIVE = typeof( InternalException ).LoadConstructorOrThrow( 1 );
         AGGREGATE_EXCEPTION_EXCEPTION_ENUMERABLE_CTOR_NATIVE = typeof( AggregateException ).LoadConstructorOrThrow( new Type[] { typeof( IEnumerable<Exception> ) } );
         DEBUGGER_DISPLAY_ATTRIBUTE_STRING_CTOR_NATIVE = typeof( DebuggerDisplayAttribute ).LoadConstructorOrThrow( new Type[] { typeof( String ) } );
         // Use normal reflection for methods not present in WP8
         INTERLOCKED_READ_I64_METHOD_NATIVE = typeof( Interlocked ).GetMethod( "Read" );//  TypeUtil.TryLoadMethod( typeof( Interlocked ), "Read", 1 );
         INTERLOCKED_EXCHANGE_I32_METHOD_NATIVE = typeof( Interlocked ).LoadMethodWithParamTypesOrThrow( "Exchange", new Type[] { typeof( Int32 ).MakeByRefType(), typeof( Int32 ) } );
         INTERLOCKED_EXCHANGE_I64_METHOD_NATIVE = typeof( Interlocked ).LoadMethodWithParamTypesOrThrow( "Exchange", new Type[] { typeof( Int64 ).MakeByRefType(), typeof( Int64 ) } );
         INTERLOCKED_EXCHANGE_R32_METHOD_NATIVE = typeof( Interlocked ).GetMethod( "Exchange", new[] { typeof( Single ).MakeByRefType(), typeof( Single ) } ); // TypeUtil.TryLoadMethodWithParamTypes( typeof( Interlocked ), "Exchange", new Type[] { typeof( Single ).MakeByRefType(), typeof( Single ) } );
         INTERLOCKED_EXCHANGE_R64_METHOD_NATIVE = typeof( Interlocked ).GetMethod( "Exchange", new[] { typeof( Double ).MakeByRefType(), typeof( Double ) } ); // TypeUtil.TryLoadMethodWithParamTypes( typeof( Interlocked ), "Exchange", new Type[] { typeof( Double ).MakeByRefType(), typeof( Double ) } );
         INTERLOCKED_EXCHANGE_INT_PTR_METHOD_NATIVE = typeof( Interlocked ).GetMethod( "Exchange", new[] { typeof( IntPtr ).MakeByRefType(), typeof( IntPtr ) } ); // TypeUtil.TryLoadMethodWithParamTypes( typeof( Interlocked ), "Exchange", new Type[] { typeof( IntPtr ).MakeByRefType(), typeof( IntPtr ) } );
         INTERLOCKED_EXCHANGE_OBJECT_METHOD_NATIVE = typeof( Interlocked ).GetMethod( "Exchange", new[] { typeof( Object ).MakeByRefType(), typeof( Object ) } );// TypeUtil.TryLoadMethodWithParamTypes( typeof( Interlocked ), "Exchange", new Type[] { typeof( Object ).MakeByRefType(), typeof( Object ) } );
         INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD_NATIVE = typeof( Interlocked ).LoadMethodWithParamTypesOrThrow( "CompareExchange", new Type[] { typeof( Int32 ).MakeByRefType(), typeof( Int32 ), typeof( Int32 ) } );
         INTERLOCKED_COMPARE_EXCHANGE_I64_METHOD_NATIVE = typeof( Interlocked ).LoadMethodWithParamTypesOrThrow( "CompareExchange", new Type[] { typeof( Int64 ).MakeByRefType(), typeof( Int64 ), typeof( Int64 ) } );
         INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD_NATIVE = typeof( Interlocked ).GetMethod( "CompareExchange", new[] { typeof( Single ).MakeByRefType(), typeof( Single ), typeof( Single ) } );// TypeUtil.TryLoadMethodWithParamTypes( typeof( Interlocked ), "CompareExchange", new Type[] { typeof( Single ).MakeByRefType(), typeof( Single ), typeof( Single ) } );
         INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD_NATIVE = typeof( Interlocked ).GetMethod( "CompareExchange", new[] { typeof( Double ).MakeByRefType(), typeof( Double ), typeof( Double ) } );  //TypeUtil.TryLoadMethodWithParamTypes( typeof( Interlocked ), "CompareExchange", new Type[] { typeof( Double ).MakeByRefType(), typeof( Double ), typeof( Double ) } );
         INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD_NATIVE = typeof( Interlocked ).GetMethod( "CompareExchange", new[] { typeof( IntPtr ).MakeByRefType(), typeof( IntPtr ), typeof( IntPtr ) } ); // TypeUtil.TryLoadMethodWithParamTypes( typeof( Interlocked ), "CompareExchange", new Type[] { typeof( IntPtr ).MakeByRefType(), typeof( IntPtr ), typeof( IntPtr ) } );
         INTERLOCKED_COMPARE_EXCHANGE_OBJECT_METHOD_NATIVE = typeof( Interlocked ).LoadMethodWithParamTypesOrThrow( "CompareExchange", new Type[] { typeof( Object ).MakeByRefType(), typeof( Object ), typeof( Object ) } );
         COMPOSITE_TYPE_ID_CTOR_NATIVE = typeof( CompositeTypeIDAttribute ).LoadConstructorOrThrow( new Type[] { typeof( Int32 ) } );
         COMPOSITE_FACTORY_METHOD_NATIVE = COMPOSITE_FACTORY_TYPE_NATIVE.LoadMethodOrThrow( "CreateInstance", null );
         ARGUMENT_EXCEPTION_STRING_CTOR_NATIVE = typeof( ArgumentException ).LoadConstructorOrThrow( new Type[] { typeof( String ) } );
         MAKE_GENERIC_TYPE_METHOD_NATIVE = typeof( Type ).LoadMethodOrThrow( "MakeGenericType", null );
         GET_CONSTRUCTORS_METHOD_NATIVE = typeof( Type ).LoadMethodOrThrow( "GetConstructors", 0 );
         CONSTRUCTOR_INVOKE_METHOD_NATIVE = typeof( ConstructorInfo ).LoadMethodOrThrow( "Invoke", 1 );
         INT_PTR_SIZE_GETTER_NATIVE = typeof( IntPtr ).LoadGetterOrThrow( "Size" );
         REF_ACTION_INVOKER_NATIVE = REF_ACTION_TYPE_NATIVE.LoadMethodOrThrow( "Invoke", 1 );
         REF_FUNCTION_INVOKER_NATIVE = REF_FUNCTION_TYPE_NATIVE.LoadMethodOrThrow( "Invoke", 1 );
         REF_INVOKER_CALLBACK_CTOR_NATIVE = typeof( RefInvokerCallback ).LoadConstructorOrThrow( (Int32?) null );
         COMPOSITE_METHOD_MODEL_INDEX_ATTRIBUTE_NATIVE = typeof( CompositeMethodModelIndexAttribute ).LoadConstructorOrThrow( new[] { typeof( Int32 ) } );
         SPECIAL_METHOD_MODEL_INDEX_ATTRIBUTE_NATIVE = typeof( SpecialMethodModelIndexAttribute ).LoadConstructorOrThrow( new[] { typeof( Int32 ) } ); ;
         CONSTRUCTOR_MODEL_INDEX_ATTRIBUTE_NATIVE = typeof( ConstructorModelIndexAttribute ).LoadConstructorOrThrow( new[] { typeof( Int32 ) } ); ;
         DOUBLE_BITS_TO_INT64_NATIVE = typeof( BitConverter ).LoadMethodOrThrow( "DoubleToInt64Bits", null );
         INT64_BITS_TO_DOUBLE_NATIVE = typeof( BitConverter ).LoadMethodOrThrow( "Int64BitsToDouble", null );
         GET_BYTES_INT32_NATIVE = typeof( BitConverter ).LoadMethodWithParamTypesOrThrow( "GetBytes", new[] { typeof( Int32 ) } );
         GET_BYTES_SINGLE_NATIVE = typeof( BitConverter ).LoadMethodWithParamTypesOrThrow( "GetBytes", new[] { typeof( Single ) } );
         BYTES_TO_INT32_NATIVE = typeof( BitConverter ).LoadMethodWithParamTypesOrThrow( "ToInt32", new[] { typeof( Byte[] ), typeof( Int32 ) } );
         BYTES_TO_SINGLE_NATIVE = typeof( BitConverter ).LoadMethodWithParamTypesOrThrow( "ToSingle", new[] { typeof( Byte[] ), typeof( Int32 ) } );
         //CONVERT_I64_DOUBLE_NATIVE = TypeUtil.TryLoadMethodWithParamTypes( typeof( Convert ), "ToDouble", new[] { typeof( Int64 ) } );
         //CONVERT_DOUBLE_I64_NATIVE = TypeUtil.TryLoadMethodWithParamTypes( typeof( Convert ), "ToInt64", new[] { typeof( Double ) } );
         //CONVERT_I32_SINGLE_NATIVE = TypeUtil.TryLoadMethodWithParamTypes( typeof( Convert ), "ToSingle", new[] { typeof( Int32 ) } );
         //CONVERT_SINGLE_I32_NATIVE = TypeUtil.TryLoadMethodWithParamTypes( typeof( Convert ), "ToInt32", new[] { typeof( Single ) } );

         LB_RESULT = new LocalBuilderInfo( "result" );
         LB_C_INSTANCE = new LocalBuilderInfo( "compositeInstance" );
         LB_F_INSTANCE = new LocalBuilderInfo( "fragmentInstance", typeof( FragmentInstance ) );
         LB_F_INSTANCE_POOL = new LocalBuilderInfo( "fragmentInstancePool", typeof( InstancePool<FragmentInstance> ) );
         LB_EXCEPTION = new LocalBuilderInfo( "exception", typeof( Exception ) );
         LB_PARAM_MODEL = new LocalBuilderInfo( "parameterModel", typeof( ParameterModel ) );
         LB_CONSTRAINT_MODEL = new LocalBuilderInfo( "constraintModel", typeof( ConstraintModel ) );
         LB_INDEX = new LocalBuilderInfo( "index", typeof( Int32 ) );
         LB_MAX = new LocalBuilderInfo( "max", typeof( Int32 ) );
         LB_ARGS_ARRAY = new LocalBuilderInfo( "args", typeof( Object[] ) );
         LB_ARGS_ARRAY_FOR_SIDE_EFFECTS = new LocalBuilderInfo( "argsForSideEffects", typeof( Object[] ) );
         LB_SPECIAL_METHOD_MODEL = new LocalBuilderInfo( "specialMethodModel", typeof( SpecialMethodModel ) );
         LB_COMPOSITE_METHOD_MODEL = new LocalBuilderInfo( "compositeMethodModel", typeof( CompositeMethodModel ) );
         LB_VIOLATIONS = new LocalBuilderInfo( "violations", typeof( IList<ConstraintViolationInfo> ) );
         LB_CONSTRAINT_INSTANCE_POOL = new LocalBuilderInfo( "constraintInstancePool", typeof( InstancePool<Object> ) );
         LB_CONSTRAINT_INSTANCE = new LocalBuilderInfo( "constraintInstance", typeof( Object ) );
         LB_EXCEPTION_LIST = new LocalBuilderInfo( "exceptions", EXCEPTION_LIST_CTOR_NATIVE.DeclaringType );
         LB_EVENT_HANDLERS = new LocalBuilderInfo( "handlers", typeof( Delegate[] ) );
         //LB_EVENT_HANDLERS_WEAK = new LocalBuilderInfo( "handlers", typeof( WeakEventHandlerWrapperForCodeGeneration ) );
         //LB_AMOUNT_OF_DEAD_EVENT_INFOS = new LocalBuilderInfo( "amountOfDead", WEAK_EVENT_ARRAY_WRAPPER_COUNT_GETTER_NATIVE.ReturnType );
         LB_CONSTRAINT_VIOLATION_EXCEPTION = new LocalBuilderInfo( "constraintViolationException", CONSTRAINT_EXCEPTION_VIOLATIONS_GETTER_NATIVE.DeclaringType );
         LB_TEMP_STORAGE = new LocalBuilderInfo( "tempStorage", typeof( Object ) );
         LB_INVOCATION_INFO = new LocalBuilderInfo( "invocationInfo" );
         LB_CONCERN_INDEX = new LocalBuilderInfo( "concernIndex", typeof( Int32 ) );

         var dic = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewDictionaryProxy<Type, MethodBase>();
         //dic.Add( typeof( IList ), TypeUtil.TryLoadConstructor( typeof( ArrayList ), Type.EmptyTypes ) );
         dic.Add( typeof( IList<> ), typeof( List<> ).LoadConstructorOrThrow( Empty<Type>.Array ) );
         dic.Add( typeof( ISet<> ), typeof( HashSet<> ).LoadConstructorOrThrow( Empty<Type>.Array ) );
         //dic.Add( typeof( ICollection ), TypeUtil.TryLoadConstructor( typeof( ArrayList ), Type.EmptyTypes ) );
         dic.Add( typeof( ICollection<> ), typeof( List<> ).LoadConstructorOrThrow( Empty<Type>.Array ) );
         //dic.Add( typeof( IDictionary ), TypeUtil.TryLoadConstructor( typeof( Hashtable ), Type.EmptyTypes ) );
         dic.Add( typeof( IDictionary<,> ), typeof( Dictionary<,> ).LoadConstructorOrThrow( Empty<Type>.Array ) );
         DEFAULT_CREATORS = dic.CQ;
      }
   }
}
#endif