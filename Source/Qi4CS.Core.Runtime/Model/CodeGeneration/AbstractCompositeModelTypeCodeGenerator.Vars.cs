﻿/*
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
#if QI4CS_SDK
using System;
using System.Linq;
using CILAssemblyManipulator.Logical;
using CommonUtils;

namespace Qi4CS.Core.Runtime.Model
{
   public partial class AbstractCompositeModelTypeCodeGenerator
   {
      protected readonly CILType COMPOSITE_CTOR_PROPERTIES_PARAM_TYPE;
      protected readonly CILType COMPOSITE_CTOR_EVENTS_PARAM_TYPE;
      protected readonly CILType ACTION_REF_TYPE;
      protected readonly CILType[] ACTION_REF_TYPES_1;
      protected readonly CILType[] ACTION_REF_TYPES_2;
      protected readonly CILType CHECK_STATE_FUNC_TYPE;
      protected readonly CILType[] PUBLIC_COMPOSITE_CTOR_ADDITTIONAL_PARAM_TYPES;
      protected readonly CILType ACTION_TYPE;
      protected readonly CILType FRAGMENT_DEPENDANT_TYPE;
      protected readonly CILType FIELD_MODEL_TYPE;
      protected readonly CILType CONSTRUCTOR_MODEL_TYPE;
      protected readonly CILType PARAMETER_MODEL_TYPE;
      protected readonly CILType SPECIAL_METHOD_MODEL_TYPE;
      protected readonly CILType COMPOSITE_METHOD_MODEL_TYPE;
      protected readonly CILType CONSTRAINT_MODEL_TYPE;
      protected readonly CILType VOID_TYPE;
      protected readonly CILType OBJECT_TYPE;
      protected readonly CILType ATTRIBUTE_TYPE;
      protected readonly CILType TYPE_TYPE;
      protected readonly CILType ABSTRACT_INJECTABLE_MODEL_TYPE;
      protected readonly CILType CONSTRAINT_TYPE;
      protected readonly CILType INT32_TYPE;
      protected readonly CILType UINT32_TYPE;
      protected readonly CILType INT64_TYPE;
      protected readonly CILType BOOLEAN_TYPE;
      protected readonly CILType SINGLE_TYPE;
      protected readonly CILType DOUBLE_TYPE;
      protected readonly CILType INT_PTR_TYPE;
      protected readonly CILType STRING_TYPE;
      protected readonly CILType EXCEPTION_TYPE;
      //protected readonly CILType WEAK_EVENT_WRAPPER_TYPE;
      //protected readonly CILType STRONG_EVENT_WRAPPER_TYPE;
      protected readonly CILType IENUMERABLE_GDEF_TYPE;
      protected readonly CILType IENUMERABLE_NO_GDEF_TYPE;
      protected readonly CILType USE_DEFAULTS_ATTRIBUTE_TYPE;
      protected readonly CILType COMPOSITE_FACTORY_TYPE;
      protected readonly CILType REF_ACTION_TYPE;
      protected readonly CILType REF_FUNCTION_TYPE;

      protected readonly CILMethod APPLICATION_GETTER_METHOD;
      protected readonly CILMethod STRUCTURE_OWNER_GETTER_METHOD;
      protected readonly CILMethod APPLICATION_IS_PASSIVE_GETTER_METHOD;
      protected readonly CILMethod INJECTION_SERVICE_GETTER_METHOD;
      protected readonly CILMethod COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER;
      protected readonly CILMethod COMPOSITE_METHOD_RESULT_GETTER;
      //protected readonly CILMethod INJECTABLE_MODEL_INJECTION_SCOPES_GETTER;
      protected readonly CILMethod INJECTION_CONTEXT_PROVIDER_METHOD;
      protected readonly CILField OPTIONAL_ATTRIBUTE_FIELD;
      protected readonly CILMethod CONSTRAINT_MODELS_GETTER;
      protected readonly CILMethod CONSTRAINT_ATTRIBUTE_GETTER;
      protected readonly CILMethod VIOLATIONS_LIST_COUNT_GETTER;
      protected readonly CILMethod METHOD_INFO_NATIVE_GETTER;
      protected readonly CILMethod MODEL_INFO_GETTER;
      protected readonly CILMethod MODEL_GETTER;
      protected readonly CILMethod C_METHODS_GETTER;
      protected readonly CILMethod GET_FRAGMENT_INSTANCE_POOL_METHOD;
      protected readonly CILMethod GET_FRAGMENT_INSTANCE_METHOD;
      protected readonly CILMethod TAKE_FRAGMENT_INSTANCE_METHOD;
      protected readonly CILMethod RETURN_FRAGMENT_INSTANCE_METHOD;
      protected readonly CILMethod SPECIAL_METHODS_GETTER;
      protected readonly CILMethod FRAGMENT_GETTER;
      protected readonly CILMethod FRAGMENT_SETTER;
      protected readonly CILMethod COMPOSITES_GETTER;
      protected readonly CILMethod COMPOSITES_GETTER_INDEXER;
      protected readonly CILMethod TYPE_OBJECT_DICTIONARY_GET_METHOD;
      protected readonly CILMethod GENERIC_FRAGMENT_METHOD;
      protected readonly CILConstructor CONSTRAINT_VIOLATION_CONSTRUCTOR;
      protected readonly CILMethod ADD_CONSTRAINT_VIOLATION_METHOD;
      protected readonly CILMethod F_INSTANCE_SET_NEXT_INFO_METHOD;
      protected readonly CILMethod F_INSTANCE_SET_METHOD_RESULT_METHOD;
      protected readonly CILMethod F_INSTANCE_GET_NEXT_INFO_METHOD;
      protected readonly CILMethod F_INSTANCE_GET_METHOD_RESULT_METHOD;
      protected readonly CILMethod STRING_CONCAT_METHOD_3;
      protected readonly CILMethod STRING_CONCAT_METHOD_2;
      protected readonly CILMethod METHOD_INFO_GET_GARGS_METHOD;
      protected readonly CILMethod MAKE_GENERIC_METHOD_METHOD;
      protected readonly CILMethod INVOKE_METHOD_METHOD;
      //protected readonly CILMethod GET_METHDO_GDEF;
      protected readonly CILMethod GET_CTOR_INDEX_METHOD;
      protected readonly CILConstructor APPLICATION_NOT_ACTIVE_EXCEPTION_CONSTRUCTOR;
      protected readonly CILMethod GET_CONSTRAINT_INSTANCE_POOL_METHOD;
      protected readonly CILMethod TAKE_CONSTRAINT_INSTANCE_METHOD;
      protected readonly CILMethod RETURN_CONSTRAINT_INSTANCE_METHOD;
      protected readonly CILMethod IS_VALID_METHOD;
      protected readonly CILMethod TO_STRING_METHOD;
      protected readonly CILMethod RETURN_CONCERN_INVOCATION_METHOD;
      protected readonly CILMethod RETURN_SIDE_EFFECT_INVOCATION_METHOD;
      protected readonly CILField EMPTY_OBJECTS_FIELD;
      protected readonly CILMethod LIST_QUERY_ITEM_GETTER;
      protected readonly CILMethod FIELDS_GETTER;
      protected readonly CILMethod FIELD_SET_VALUE_METHOD;
      protected readonly CILConstructor PROTOTYPE_ACTION_CONSTRUCTOR;
      protected readonly CILMethod EQUALS_METHOD;
      protected readonly CILMethod HASH_CODE_METHOD;
      protected readonly CILMethod REFERENCE_EQUALS_METHOD;
      protected readonly CILMethod GET_TYPE_METHOD;
      protected readonly CILMethod DELEGATE_COMBINE_METHOD;
      protected readonly CILMethod DELEGATE_REMOVE_METHOD;
      protected readonly CILMethod INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF;
      protected readonly CILMethod GET_EVENT_INFO_METHOD;
      protected readonly CILConstructor COMPOSITE_EVENT_CTOR;
      protected readonly CILConstructor INVALID_OPERATION_EXCEPTION_CTOR_WITH_STRING;
      protected readonly CILMethod QNAME_FROM_TYPE_AND_NAME;
      protected readonly CILMethod IS_PROTOTYPE_GETTER;
      protected readonly CILMethod GET_PROPERTY_INFO_METHOD;
      protected readonly CILMethod COMPOSITE_METHODS_INDEXER;
      protected readonly CILMethod EVENT_MODEL_GETTER;
      protected readonly CILMethod PROPERTY_MODEL_GETTER;
      protected readonly CILConstructor COMPOSITE_PROPERTY_CTOR;
      protected readonly CILMethod INVOCATION_INFO_GETTER;
      protected readonly CILMethod INVOCATION_INFO_SETTER;
      protected readonly CILConstructor INVOCATION_INFO_CREATOR_CTOR;
      protected readonly CILMethod INVOCATION_INFO_METHOD_GETTER;
      protected readonly CILMethod INVOCATION_INFO_FRAGMENT_METHOD_MODEL_GETTER;
      protected readonly CILMethod INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER;
      protected readonly CILMethod CONCERN_MODELS_GETTER;
      protected readonly CILMethod CONCERN_MODELS_INDEXER;
      protected readonly CILMethod MIXIN_MODEL_GETTER;
      protected readonly CILMethod SIDE_EFFECT_MODELS_GETTER;
      protected readonly CILMethod SIDE_EFFECT_MODELS_INDEXER;
      protected readonly CILMethod COLLECTION_ADD_ONLY_ADD_METHOD;
      protected readonly CILConstructor ACTION_0_CTOR;
      protected readonly CILMethod INTERLOCKED_EXCHANGE_METHOD_GDEF;
      protected readonly CILMethod GET_INVOCATION_LIST_METHOD;
      protected readonly CILMethod ADD_LAST_METHOD;
      //protected readonly CILMethod WEAK_EVENT_ARRAY_WRAPPER_ARRAY_GETTER;
      //protected readonly CILMethod WEAK_EVENT_ARRAY_WRAPPER_COUNT_GETTER;
      //protected readonly CILMethod WEAK_EVENT_ARRAY_CLEANUP_METHOD;
      //protected readonly CILMethod WEAK_EVENT_ARRAY_COMBINE_METHOD;
      //protected readonly CILMethod WEAK_EVENT_ARRAY_REMOVE_METHOD;
      //protected readonly CILMethod IS_EVENT_INFO_DEAD_METHOD;
      //protected readonly CILMethod EVENT_INFO_TARGET_GETTER;
      //protected readonly CILMethod EVENT_INFO_METHOD_GETTER;
      //protected readonly CILConstructor EVENT_INFO_CTOR;
      protected readonly CILMethod Q_NAME_GET_BARE_TYPE_NAME_METHOD;
      protected readonly CILMethod Q_NAME_FROM_MEMBER_INFO_METHOD;
      protected readonly CILConstructor INJECTION_EXCEPTION_CTOR;
      protected readonly CILMethod CHECK_STATE_METHOD_SIG;
      protected readonly CILMethod CONSTRAINT_EXCEPTION_VIOLATIONS_GETTER;
      protected readonly CILMethod CONSTRAINT_VIOLATIONS_DIC_ADD_METHOD;
      protected readonly CILConstructor CONSTRAINT_VIOLATIONS_DIC_CTOR;
      protected readonly CILConstructor CHECK_ACTION_FUNC_CTOR;
      protected readonly CILMethod SET_DEFAULTS_METHOD_SIG;
      protected readonly CILConstructor CONSTRAINT_VIOLATIONS_LIST_CTOR;
      protected readonly CILConstructor EXCEPTION_LIST_CTOR;
      protected readonly CILConstructor FUNC_1_CTOR;
      protected readonly CILConstructor FUNC_2_CTOR;
      protected readonly CILConstructor FUNC_3_CTOR;
      protected readonly CILConstructor ACTION_1_CTOR;
      protected readonly CILConstructor LAZY_GDEF_CTOR;
      protected readonly CILConstructor FRAGMENT_INSTANCE_CTOR_NO_PARAMS;
      protected readonly CILConstructor FRAGMENT_INSTANCE_CTOR_WITH_PARAMS;
      protected readonly CILMethod MODEL_CTORS_GETTER;
      protected readonly CILMethod FRAGMENT_DEPENDANT_GETTER;
      protected readonly CILMethod FRAGMENT_DEPENDANT_SETTER;
      protected readonly CILProperty FRAGMENT_DEPENDANT_PROPERTY;
      protected readonly CILMethod CONCERN_INVOCATION_INFO_ITEM_1;
      protected readonly CILMethod CONCERN_INVOCATION_INFO_ITEM_2;
      protected readonly CILMethod SIDE_EFFECT_INVOCATION_INFO_ITEM_1;
      protected readonly CILMethod SIDE_EFFECT_INVOCATION_INFO_ITEM_2;
      protected readonly CILMethod SIDE_EFFECT_INVOCATION_INFO_ITEM_3;
      protected readonly CILConstructor OBJECT_CTOR;
      protected readonly CILConstructor NULLABLE_CTOR;
      protected readonly CILMethod DEFAULT_CREATOR_GETTER;
      protected readonly CILMethod DEFAULT_CREATOR_INVOKER;
      protected readonly CILConstructor NO_POOL_ATTRIBUTE_CTOR;
      protected readonly CILConstructor MAIN_PUBLIC_COMPOSITE_TYPE_ATTRIBUTE_CTOR;
      protected readonly CILConstructor CONSTRAINT_VIOLATION_LIST_CTOR;
      protected readonly CILConstructor CONSTRAINT_VIOLATION_EXCEPTION_CTOR;
      protected readonly CILConstructor INTERNAL_EXCEPTION_CTOR;
      protected readonly CILConstructor AGGREGATE_EXCEPTION_EXCEPTION_ENUMERABLE_CTOR;
      protected readonly CILConstructor DEBUGGER_DISPLAY_ATTRIBUTE_STRING_CTOR;
      protected readonly CILMethod INTERLOCKED_READ_I64_METHOD;
      protected readonly CILMethod INTERLOCKED_EXCHANGE_I32_METHOD;
      protected readonly CILMethod INTERLOCKED_EXCHANGE_I64_METHOD;
      protected readonly CILMethod INTERLOCKED_EXCHANGE_R32_METHOD;
      protected readonly CILMethod INTERLOCKED_EXCHANGE_R64_METHOD;
      protected readonly CILMethod INTERLOCKED_EXCHANGE_INT_PTR_METHOD;
      protected readonly CILMethod INTERLOCKED_EXCHANGE_OBJECT_METHOD;
      protected readonly CILMethod INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD;
      protected readonly CILMethod INTERLOCKED_COMPARE_EXCHANGE_I64_METHOD;
      protected readonly CILMethod INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD;
      protected readonly CILMethod INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD;
      protected readonly CILMethod INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD;
      protected readonly CILMethod INTERLOCKED_COMPARE_EXCHANGE_OBJECT_METHOD;
      protected readonly CILConstructor COMPOSITE_TYPE_ID_CTOR;
      protected readonly CILMethod COMPOSITE_FACTORY_METHOD;
      protected readonly CILMethod COMPOSITE_CALLBACK_GET_COMPOSITE_METHOD_METHOD;
      protected readonly CILConstructor ARGUMENT_EXCEPTION_STRING_CTOR;
      protected readonly CILMethod MAKE_GENERIC_TYPE_METHOD;
      protected readonly CILMethod GET_FIRST_INSTANCE_CTOR;
      protected readonly CILMethod CONSTRUCTOR_INVOKE_METHOD;
      protected readonly CILMethod INT_PTR_SIZE_GETTER;
      protected readonly CILMethod REF_ACTION_INVOKER;
      protected readonly CILMethod REF_FUNCTION_INVOKER;
      protected readonly CILConstructor REF_INVOKER_CALLBACK_CTOR;
      protected readonly CILConstructor COMPOSITE_METHOD_MODEL_INDEX_ATTRIBUTE;
      protected readonly CILConstructor SPECIAL_METHOD_MODEL_INDEX_ATTRIBUTE;
      protected readonly CILConstructor CONSTRUCTOR_MODEL_INDEX_ATTRIBUTE;
      protected readonly CILConstructor FIELD_MODEL_INDEX_ATTRIBUTE;
      protected readonly CILMethod DOUBLE_BITS_TO_INT64;
      protected readonly CILMethod INT64_BITS_TO_DOUBLE;
      protected readonly CILMethod GET_BYTES_INT32;
      protected readonly CILMethod GET_BYTES_SINGLE;
      protected readonly CILMethod BYTES_TO_INT32;
      protected readonly CILMethod BYTES_TO_SINGLE;
      protected readonly CILConstructor COMPOSITE_TYPES_ATTRIBUTE_CTOR;
      protected readonly CILProperty COMPOSITE_TYPES_ATTRIBUTE_PUBLIC_TYPES_PROPERTY;
      protected readonly CILProperty COMPOSITE_TYPES_ATTRIBUTE_PRIVATE_TYPES_PROPERTY;
      protected readonly CILProperty COMPOSITE_TYPES_ATTRIBUTE_FRAGMENT_TYPES_PROPERTY;
      protected readonly CILProperty COMPOSITE_TYPES_ATTRIBUTE_CONCERN_INVOCATION_HANDLER_TYPES_PROPERTY;
      protected readonly CILProperty COMPOSITE_TYPES_ATTRIBUTE_SIDE_EFFECT_INVOCATION_HANDLER_TYPES_PROPERTY;
      protected readonly CILProperty COMPOSITE_TYPES_ATTRIBUTE_COMPOSITE_FACTORY_TYPE_PROPERTY;
      protected readonly CILConstructor PUBLIC_COMPOSITE_GENERIC_BINDING_ATTRIBUTE_CTOR;
      protected readonly CILProperty PUBLIC_COMPOSITE_GENERIC_BINDING_ATTRIBUTE_PROPERTY;

      protected readonly CILReflectionContext ctx;
      protected readonly Boolean isSilverLight;
      protected readonly CompositeCodeGenerationInfo codeGenInfo;

      protected AbstractCompositeModelTypeCodeGenerator( CompositeCodeGenerationInfo codeGenInfo, Boolean isSilverlight, CILReflectionContext aCtx )
      {
         ArgumentValidator.ValidateNotNull( "Code generation constants", codeGenInfo );
         ArgumentValidator.ValidateNotNull( "CIL reflection context", aCtx );

         this.codeGenInfo = codeGenInfo;
         this.isSilverLight = isSilverlight;
         this.ctx = aCtx;

         this.COMPOSITE_CTOR_PROPERTIES_PARAM_TYPE = ctx.NewWrapperAsType( COMPOSITE_CTOR_PROPERTIES_PARAM_TYPE_NATIVE );
         this.COMPOSITE_CTOR_EVENTS_PARAM_TYPE = ctx.NewWrapperAsType( COMPOSITE_CTOR_EVENTS_PARAM_TYPE_NATIVE );
         this.ACTION_REF_TYPE = ctx.NewWrapperAsType( ACTION_REF_TYPE_NATIVE );
         this.ACTION_REF_TYPES_1 = ACTION_REF_TYPES_1_NATIVE.Select( t => ctx.NewWrapperAsType( t ) ).ToArray();
         this.ACTION_REF_TYPES_2 = ACTION_REF_TYPES_2_NATIVE.Select( t => ctx.NewWrapperAsType( t ) ).ToArray();
         this.CHECK_STATE_FUNC_TYPE = ctx.NewWrapperAsType( CHECK_STATE_FUNC_TYPE_NATIVE );
         this.PUBLIC_COMPOSITE_CTOR_ADDITTIONAL_PARAM_TYPES = PUBLIC_COMPOSITE_CTOR_ADDITTIONAL_PARAM_TYPES_NATIVE.Select( t => ctx.NewWrapperAsType( t ) ).ToArray();
         this.ACTION_TYPE = ctx.NewWrapperAsType( ACTION_TYPE_NATIVE );
         this.FRAGMENT_DEPENDANT_TYPE = ctx.NewWrapperAsType( FRAGMENT_DEPENDANT_TYPE_NATIVE );
         this.FIELD_MODEL_TYPE = ctx.NewWrapperAsType( FIELD_MODEL_TYPE_NATIVE );
         this.CONSTRUCTOR_MODEL_TYPE = ctx.NewWrapperAsType( CONSTRUCTOR_MODEL_TYPE_NATIVE );
         this.PARAMETER_MODEL_TYPE = ctx.NewWrapperAsType( PARAMETER_MODEL_TYPE_NATIVE );
         this.SPECIAL_METHOD_MODEL_TYPE = ctx.NewWrapperAsType( SPECIAL_METHOD_MODEL_TYPE_NATIVE );
         this.COMPOSITE_METHOD_MODEL_TYPE = ctx.NewWrapperAsType( COMPOSITE_METHOD_MODEL_TYPE_NATIVE );
         this.CONSTRAINT_MODEL_TYPE = ctx.NewWrapperAsType( CONSTRAINT_MODEL_TYPE_NATIVE );
         this.VOID_TYPE = ctx.NewWrapperAsType( VOID_TYPE_NATIVE );
         this.OBJECT_TYPE = ctx.NewWrapperAsType( OBJECT_TYPE_NATIVE );
         this.ATTRIBUTE_TYPE = ctx.NewWrapperAsType( ATTRIBUTE_TYPE_NATIVE );
         this.TYPE_TYPE = ctx.NewWrapperAsType( TYPE_TYPE_NATIVE );
         this.ABSTRACT_INJECTABLE_MODEL_TYPE = ctx.NewWrapperAsType( ABSTRACT_INJECTABLE_MODEL_TYPE_NATIVE );
         this.CONSTRAINT_TYPE = ctx.NewWrapperAsType( CONSTRAINT_TYPE_NATIVE );
         this.INT32_TYPE = ctx.NewWrapperAsType( INT32_TYPE_NATIVE );
         this.UINT32_TYPE = ctx.NewWrapperAsType( UINT32_TYPE_NATIVE );
         this.INT64_TYPE = ctx.NewWrapperAsType( INT64_TYPE_NATIVE );
         this.BOOLEAN_TYPE = ctx.NewWrapperAsType( BOOLEAN_TYPE_NATIVE );
         this.SINGLE_TYPE = ctx.NewWrapperAsType( SINGLE_TYPE_NATIVE );
         this.DOUBLE_TYPE = ctx.NewWrapperAsType( DOUBLE_TYPE_NATIVE );
         this.INT_PTR_TYPE = ctx.NewWrapperAsType( INT_PTR_TYPE_NATIVE );
         this.STRING_TYPE = ctx.NewWrapperAsType( STRING_TYPE_NATIVE );
         this.EXCEPTION_TYPE = ctx.NewWrapperAsType( EXCEPTION_TYPE_NATIVE );
         //this.WEAK_EVENT_WRAPPER_TYPE = ctx.NewWrapperAsType( WEAK_EVENT_WRAPPER_TYPE_NATIVE );
         //this.STRONG_EVENT_WRAPPER_TYPE = ctx.NewWrapperAsType( STRONG_EVENT_WRAPPER_TYPE_NATIVE );
         this.IENUMERABLE_GDEF_TYPE = ctx.NewWrapperAsType( IENUMERABLE_GDEF_TYPE_NATIVE );
         this.IENUMERABLE_NO_GDEF_TYPE = ctx.NewWrapperAsType( IENUMERABLE_NO_GDEF_TYPE_NATIVE );
         this.USE_DEFAULTS_ATTRIBUTE_TYPE = ctx.NewWrapperAsType( USE_DEFAULTS_ATTRIBUTE_TYPE_NATIVE );
         this.COMPOSITE_FACTORY_TYPE = ctx.NewWrapperAsType( COMPOSITE_FACTORY_TYPE_NATIVE );
         this.REF_ACTION_TYPE = ctx.NewWrapperAsType( REF_ACTION_TYPE_NATIVE );
         this.REF_FUNCTION_TYPE = ctx.NewWrapperAsType( REF_FUNCTION_TYPE_NATIVE );

         this.APPLICATION_GETTER_METHOD = ctx.NewWrapper( APPLICATION_GETTER_METHOD_NATIVE );
         this.STRUCTURE_OWNER_GETTER_METHOD = ctx.NewWrapper( STRUCTURE_OWNER_GETTER_METHOD_NATIVE );
         this.APPLICATION_IS_PASSIVE_GETTER_METHOD = ctx.NewWrapper( APPLICATION_IS_PASSIVE_GETTER_METHOD_NATIVE );
         this.INJECTION_SERVICE_GETTER_METHOD = ctx.NewWrapper( INJECTION_SERVICE_GETTER_METHOD_NATIVE );
         this.COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER = ctx.NewWrapper( COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER_NATIVE );
         this.COMPOSITE_METHOD_RESULT_GETTER = ctx.NewWrapper( COMPOSITE_METHOD_RESULT_GETTER_NATIVE );
         //this.INJECTABLE_MODEL_INJECTION_SCOPES_GETTER = ctx.NewWrapper( INJECTABLE_MODEL_INJECTION_SCOPES_GETTER_NATIVE );
         this.INJECTION_CONTEXT_PROVIDER_METHOD = ctx.NewWrapper( INJECTION_CONTEXT_PROVIDER_METHOD_NATIVE );
         this.OPTIONAL_ATTRIBUTE_FIELD = ctx.NewWrapper( OPTIONAL_ATTRIBUTE_FIELD_NATIVE );
         this.CONSTRAINT_MODELS_GETTER = ctx.NewWrapper( CONSTRAINT_MODELS_GETTER_NATIVE );
         this.CONSTRAINT_ATTRIBUTE_GETTER = ctx.NewWrapper( CONSTRAINT_ATTRIBUTE_GETTER_NATIVE );
         this.VIOLATIONS_LIST_COUNT_GETTER = ctx.NewWrapper( VIOLATIONS_LIST_COUNT_GETTER_NATIVE );
         this.METHOD_INFO_NATIVE_GETTER = ctx.NewWrapper( METHOD_INFO_NATIVE_GETTER_NATIVE );
         this.MODEL_INFO_GETTER = ctx.NewWrapper( MODEL_INFO_GETTER_NATIVE );
         this.MODEL_GETTER = ctx.NewWrapper( MODEL_GETTER_NATIVE );
         this.C_METHODS_GETTER = ctx.NewWrapper( C_METHODS_GETTER_NATIVE );
         this.GET_FRAGMENT_INSTANCE_POOL_METHOD = ctx.NewWrapper( GET_FRAGMENT_INSTANCE_POOL_METHOD_NATIVE );
         this.GET_FRAGMENT_INSTANCE_METHOD = ctx.NewWrapper( GET_FRAGMENT_INSTANCE_METHOD_NATIVE );
         this.TAKE_FRAGMENT_INSTANCE_METHOD = ctx.NewWrapper( TAKE_FRAGMENT_INSTANCE_METHOD_NATIVE );
         this.RETURN_FRAGMENT_INSTANCE_METHOD = ctx.NewWrapper( RETURN_FRAGMENT_INSTANCE_METHOD_NATIVE );
         this.SPECIAL_METHODS_GETTER = ctx.NewWrapper( SPECIAL_METHODS_GETTER_NATIVE );
         this.FRAGMENT_GETTER = ctx.NewWrapper( FRAGMENT_GETTER_NATIVE );
         this.FRAGMENT_SETTER = ctx.NewWrapper( FRAGMENT_SETTER_NATIVE );
         this.COMPOSITES_GETTER = ctx.NewWrapper( COMPOSITES_GETTER_NATIVE );
         this.COMPOSITES_GETTER_INDEXER = ctx.NewWrapper( COMPOSITES_GETTER_INDEXER_NATIVE );
         this.TYPE_OBJECT_DICTIONARY_GET_METHOD = ctx.NewWrapper( TYPE_OBJECT_DICTIONARY_GET_METHOD_NATIVE );
         this.GENERIC_FRAGMENT_METHOD = ctx.NewWrapper( GENERIC_FRAGMENT_METHOD_NATIVE );
         this.CONSTRAINT_VIOLATION_CONSTRUCTOR = ctx.NewWrapper( CONSTRAINT_VIOLATION_CONSTRUCTOR_NATIVE );
         this.ADD_CONSTRAINT_VIOLATION_METHOD = ctx.NewWrapper( ADD_CONSTRAINT_VIOLATION_METHOD_NATIVE );
         this.F_INSTANCE_SET_NEXT_INFO_METHOD = ctx.NewWrapper( F_INSTANCE_SET_NEXT_INFO_METHOD_NATIVE );
         this.F_INSTANCE_SET_METHOD_RESULT_METHOD = ctx.NewWrapper( F_INSTANCE_SET_METHOD_RESULT_METHOD_NATIVE );
         this.F_INSTANCE_GET_NEXT_INFO_METHOD = ctx.NewWrapper( F_INSTANCE_GET_NEXT_INFO_METHOD_NATIVE );
         this.F_INSTANCE_GET_METHOD_RESULT_METHOD = ctx.NewWrapper( F_INSTANCE_GET_METHOD_RESULT_METHOD_NATIVE );
         this.STRING_CONCAT_METHOD_3 = ctx.NewWrapper( STRING_CONCAT_METHOD_3_NATIVE );
         this.STRING_CONCAT_METHOD_2 = ctx.NewWrapper( STRING_CONCAT_METHOD_2_NATIVE );
         this.METHOD_INFO_GET_GARGS_METHOD = ctx.NewWrapper( METHOD_INFO_GET_GARGS_METHOD_NATIVE );
         this.MAKE_GENERIC_METHOD_METHOD = ctx.NewWrapper( MAKE_GENERIC_METHOD_METHOD_NATIVE );
         this.INVOKE_METHOD_METHOD = ctx.NewWrapper( INVOKE_METHOD_METHOD_NATIVE );
         //this.GET_METHDO_GDEF = ctx.NewWrapper( GET_METHDO_GDEF_NATIVE );
         this.GET_CTOR_INDEX_METHOD = ctx.NewWrapper( GET_CTOR_INDEX_METHOD_NATIVE );
         this.APPLICATION_NOT_ACTIVE_EXCEPTION_CONSTRUCTOR = ctx.NewWrapper( APPLICATION_NOT_ACTIVE_EXCEPTION_CONSTRUCTOR_NATIVE );
         this.GET_CONSTRAINT_INSTANCE_POOL_METHOD = ctx.NewWrapper( GET_CONSTRAINT_INSTANCE_POOL_METHOD_NATIVE );
         this.TAKE_CONSTRAINT_INSTANCE_METHOD = ctx.NewWrapper( TAKE_CONSTRAINT_INSTANCE_METHOD_NATIVE );
         this.RETURN_CONSTRAINT_INSTANCE_METHOD = ctx.NewWrapper( RETURN_CONSTRAINT_INSTANCE_METHOD_NATIVE );
         this.IS_VALID_METHOD = ctx.NewWrapper( IS_VALID_METHOD_NATIVE );
         this.TO_STRING_METHOD = ctx.NewWrapper( TO_STRING_METHOD_NATIVE );
         this.RETURN_CONCERN_INVOCATION_METHOD = ctx.NewWrapper( RETURN_CONCERN_INVOCATION_METHOD_NATIVE );
         this.RETURN_SIDE_EFFECT_INVOCATION_METHOD = ctx.NewWrapper( RETURN_SIDE_EFFECT_INVOCATION_METHOD_NATIVE );
         this.EMPTY_OBJECTS_FIELD = ctx.NewWrapper( EMPTY_OBJECTS_FIELD_NATIVE );
         this.LIST_QUERY_ITEM_GETTER = ctx.NewWrapper( LIST_QUERY_ITEM_GETTER_NATIVE );
         this.FIELDS_GETTER = ctx.NewWrapper( FIELDS_GETTER_NATIVE );
         this.FIELD_SET_VALUE_METHOD = ctx.NewWrapper( FIELD_SET_VALUE_METHOD_NATIVE );
         this.PROTOTYPE_ACTION_CONSTRUCTOR = ctx.NewWrapper( PROTOTYPE_ACTION_CONSTRUCTOR_NATIVE );
         this.EQUALS_METHOD = ctx.NewWrapper( EQUALS_METHOD_NATIVE );
         this.HASH_CODE_METHOD = ctx.NewWrapper( HASH_CODE_METHOD_NATIVE );
         this.REFERENCE_EQUALS_METHOD = ctx.NewWrapper( REFERENCE_EQUALS_METHOD_NATIVE );
         this.GET_TYPE_METHOD = ctx.NewWrapper( GET_TYPE_METHOD_NATIVE );
         this.DELEGATE_COMBINE_METHOD = ctx.NewWrapper( DELEGATE_COMBINE_METHOD_NATIVE );
         this.DELEGATE_REMOVE_METHOD = ctx.NewWrapper( DELEGATE_REMOVE_METHOD_NATIVE );
         this.INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF = ctx.NewWrapper( INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF_NATIVE );
         this.GET_EVENT_INFO_METHOD = ctx.NewWrapper( GET_EVENT_INFO_METHOD_NATIVE );
         this.COMPOSITE_EVENT_CTOR = ctx.NewWrapper( COMPOSITE_EVENT_CTOR_NATIVE );
         this.INVALID_OPERATION_EXCEPTION_CTOR_WITH_STRING = ctx.NewWrapper( INVALID_OPERATION_EXCEPTION_CTOR_WITH_STRING_NATIVE );
         this.QNAME_FROM_TYPE_AND_NAME = ctx.NewWrapper( QNAME_FROM_TYPE_AND_NAME_NATIVE );
         this.IS_PROTOTYPE_GETTER = ctx.NewWrapper( IS_PROTOTYPE_GETTER_NATIVE );
         this.GET_PROPERTY_INFO_METHOD = ctx.NewWrapper( GET_PROPERTY_INFO_METHOD_NATIVE );
         this.COMPOSITE_METHODS_INDEXER = ctx.NewWrapper( COMPOSITE_METHODS_INDEXER_NATIVE );
         this.EVENT_MODEL_GETTER = ctx.NewWrapper( EVENT_MODEL_GETTER_NATIVE );
         this.PROPERTY_MODEL_GETTER = ctx.NewWrapper( PROPERTY_MODEL_GETTER_NATIVE );
         this.COMPOSITE_PROPERTY_CTOR = ctx.NewWrapper( COMPOSITE_PROPERTY_CTOR_NATIVE );
         this.INVOCATION_INFO_GETTER = ctx.NewWrapper( INVOCATION_INFO_GETTER_NATIVE );
         this.INVOCATION_INFO_SETTER = ctx.NewWrapper( INVOCATION_INFO_SETTER_NATIVE );
         this.INVOCATION_INFO_CREATOR_CTOR = ctx.NewWrapper( INVOCATION_INFO_CREATOR_CTOR_NATIVE );
         this.INVOCATION_INFO_METHOD_GETTER = ctx.NewWrapper( INVOCATION_INFO_METHOD_GETTER_NATIVE );
         this.INVOCATION_INFO_FRAGMENT_METHOD_MODEL_GETTER = ctx.NewWrapper( INVOCATION_INFO_FRAGMENT_METHOD_MODEL_GETTER_NATIVE );
         this.INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER = ctx.NewWrapper( INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER_NATIVE );
         this.CONCERN_MODELS_GETTER = ctx.NewWrapper( CONCERN_MODELS_GETTER_NATIVE );
         this.CONCERN_MODELS_INDEXER = ctx.NewWrapper( CONCERN_MODELS_INDEXER_NATIVE );
         this.MIXIN_MODEL_GETTER = ctx.NewWrapper( MIXIN_MODEL_GETTER_NATIVE );
         this.SIDE_EFFECT_MODELS_GETTER = ctx.NewWrapper( SIDE_EFFECT_MODELS_GETTER_NATIVE );
         this.SIDE_EFFECT_MODELS_INDEXER = ctx.NewWrapper( SIDE_EFFECT_MODELS_INDEXER_NATIVE );
         this.COLLECTION_ADD_ONLY_ADD_METHOD = ctx.NewWrapper( COLLECTION_ADD_ONLY_ADD_METHOD_NATIVE );
         this.ACTION_0_CTOR = ctx.NewWrapper( ACTION_0_CTOR_NATIVE );
         this.INTERLOCKED_EXCHANGE_METHOD_GDEF = ctx.NewWrapper( INTERLOCKED_EXCHANGE_METHOD_GDEF_NATIVE );
         this.GET_INVOCATION_LIST_METHOD = ctx.NewWrapper( GET_INVOCATION_LIST_METHOD_NATIVE );
         this.ADD_LAST_METHOD = ctx.NewWrapper( ADD_LAST_METHOD_NATIVE );
         //this.WEAK_EVENT_ARRAY_WRAPPER_ARRAY_GETTER = ctx.NewWrapper( WEAK_EVENT_ARRAY_WRAPPER_ARRAY_GETTER_NATIVE );
         //this.WEAK_EVENT_ARRAY_WRAPPER_COUNT_GETTER = ctx.NewWrapper( WEAK_EVENT_ARRAY_WRAPPER_COUNT_GETTER_NATIVE );
         //this.WEAK_EVENT_ARRAY_CLEANUP_METHOD = ctx.NewWrapper( WEAK_EVENT_ARRAY_CLEANUP_METHOD_NATIVE );
         //this.WEAK_EVENT_ARRAY_COMBINE_METHOD = ctx.NewWrapper( WEAK_EVENT_ARRAY_COMBINE_METHOD_NATIVE );
         //this.WEAK_EVENT_ARRAY_REMOVE_METHOD = ctx.NewWrapper( WEAK_EVENT_ARRAY_REMOVE_METHOD_NATIVE );
         //this.IS_EVENT_INFO_DEAD_METHOD = ctx.NewWrapper( IS_EVENT_INFO_DEAD_METHOD_NATIVE );
         //this.EVENT_INFO_TARGET_GETTER = ctx.NewWrapper( EVENT_INFO_TARGET_GETTER_NATIVE );
         //this.EVENT_INFO_METHOD_GETTER = ctx.NewWrapper( EVENT_INFO_METHOD_GETTER_NATIVE );
         //this.EVENT_INFO_CTOR = ctx.NewWrapper( EVENT_INFO_CTOR_NATIVE );
         this.Q_NAME_GET_BARE_TYPE_NAME_METHOD = ctx.NewWrapper( Q_NAME_GET_BARE_TYPE_NAME_METHOD_NATIVE );
         this.Q_NAME_FROM_MEMBER_INFO_METHOD = ctx.NewWrapper( Q_NAME_FROM_MEMBER_INFO_METHOD_NATIVE );
         this.INJECTION_EXCEPTION_CTOR = ctx.NewWrapper( INJECTION_EXCEPTION_CTOR_NATIVE );
         this.CHECK_STATE_METHOD_SIG = ctx.NewWrapper( CHECK_STATE_METHOD_SIG_NATIVE );
         this.CONSTRAINT_EXCEPTION_VIOLATIONS_GETTER = ctx.NewWrapper( CONSTRAINT_EXCEPTION_VIOLATIONS_GETTER_NATIVE );
         this.CONSTRAINT_VIOLATIONS_DIC_ADD_METHOD = ctx.NewWrapper( CONSTRAINT_VIOLATIONS_DIC_ADD_METHOD_NATIVE );
         this.CONSTRAINT_VIOLATIONS_DIC_CTOR = ctx.NewWrapper( CONSTRAINT_VIOLATIONS_DIC_CTOR_NATIVE );
         this.CHECK_ACTION_FUNC_CTOR = ctx.NewWrapper( CHECK_ACTION_FUNC_CTOR_NATIVE );
         this.SET_DEFAULTS_METHOD_SIG = ctx.NewWrapper( SET_DEFAULTS_METHOD_SIG_NATIVE );
         this.CONSTRAINT_VIOLATIONS_LIST_CTOR = ctx.NewWrapper( CONSTRAINT_VIOLATIONS_LIST_CTOR_NATIVE );
         this.EXCEPTION_LIST_CTOR = ctx.NewWrapper( EXCEPTION_LIST_CTOR_NATIVE );
         this.FUNC_1_CTOR = ctx.NewWrapper( FUNC_1_CTOR_NATIVE );
         this.FUNC_2_CTOR = ctx.NewWrapper( FUNC_2_CTOR_NATIVE );
         this.FUNC_3_CTOR = ctx.NewWrapper( FUNC_3_CTOR_NATIVE );
         this.ACTION_1_CTOR = ctx.NewWrapper( ACTION_1_CTOR_NATIVE );
         this.LAZY_GDEF_CTOR = ctx.NewWrapper( LAZY_GDEF_CTOR_NATIVE );
         this.FRAGMENT_INSTANCE_CTOR_NO_PARAMS = ctx.NewWrapper( FRAGMENT_INSTANCE_CTOR_NO_PARAMS_NATIVE );
         this.FRAGMENT_INSTANCE_CTOR_WITH_PARAMS = ctx.NewWrapper( FRAGMENT_INSTANCE_CTOR_WITH_PARAMS_NATIVE );
         this.MODEL_CTORS_GETTER = ctx.NewWrapper( MODEL_CTORS_GETTER_NATIVE );
         this.FRAGMENT_DEPENDANT_GETTER = ctx.NewWrapper( FRAGMENT_DEPENDANT_GETTER_NATIVE );
         this.FRAGMENT_DEPENDANT_SETTER = ctx.NewWrapper( FRAGMENT_DEPENDANT_SETTER_NATIVE );
         this.FRAGMENT_DEPENDANT_PROPERTY = ctx.NewWrapper( FRAGMENT_DEPENDANT_PROPERTY_NATIVE );
         this.CONCERN_INVOCATION_INFO_ITEM_1 = ctx.NewWrapper( CONCERN_INVOCATION_INFO_ITEM_1_NATIVE );
         this.CONCERN_INVOCATION_INFO_ITEM_2 = ctx.NewWrapper( CONCERN_INVOCATION_INFO_ITEM_2_NATIVE );
         this.SIDE_EFFECT_INVOCATION_INFO_ITEM_1 = ctx.NewWrapper( SIDE_EFFECT_INVOCATION_INFO_ITEM_1_NATIVE );
         this.SIDE_EFFECT_INVOCATION_INFO_ITEM_2 = ctx.NewWrapper( SIDE_EFFECT_INVOCATION_INFO_ITEM_2_NATIVE );
         this.SIDE_EFFECT_INVOCATION_INFO_ITEM_3 = ctx.NewWrapper( SIDE_EFFECT_INVOCATION_INFO_ITEM_3_NATIVE );
         this.OBJECT_CTOR = ctx.NewWrapper( OBJECT_CTOR_NATIVE );
         this.NULLABLE_CTOR = ctx.NewWrapper( NULLABLE_CTOR_NATIVE );
         this.DEFAULT_CREATOR_GETTER = ctx.NewWrapper( DEFAULT_CREATOR_GETTER_NATIVE );
         this.DEFAULT_CREATOR_INVOKER = ctx.NewWrapper( DEFAULT_CREATOR_INVOKER_NATIVE );
         this.NO_POOL_ATTRIBUTE_CTOR = ctx.NewWrapper( NO_POOL_ATTRIBUTE_CTOR_NATIVE );
         this.MAIN_PUBLIC_COMPOSITE_TYPE_ATTRIBUTE_CTOR = ctx.NewWrapper( MAIN_PUBLIC_COMPOSITE_TYPE_ATTRIBUTE_CTOR_NATIVE );
         this.CONSTRAINT_VIOLATION_LIST_CTOR = ctx.NewWrapper( CONSTRAINT_VIOLATION_LIST_CTOR_NATIVE );
         this.CONSTRAINT_VIOLATION_EXCEPTION_CTOR = ctx.NewWrapper( CONSTRAINT_VIOLATION_EXCEPTION_CTOR_NATIVE );
         this.INTERNAL_EXCEPTION_CTOR = ctx.NewWrapper( INTERNAL_EXCEPTION_CTOR_NATIVE );
         this.AGGREGATE_EXCEPTION_EXCEPTION_ENUMERABLE_CTOR = ctx.NewWrapper( AGGREGATE_EXCEPTION_EXCEPTION_ENUMERABLE_CTOR_NATIVE );
         this.DEBUGGER_DISPLAY_ATTRIBUTE_STRING_CTOR = ctx.NewWrapper( DEBUGGER_DISPLAY_ATTRIBUTE_STRING_CTOR_NATIVE );
         this.INTERLOCKED_READ_I64_METHOD = INTERLOCKED_READ_I64_METHOD_NATIVE == null ? null : ctx.NewWrapper( INTERLOCKED_READ_I64_METHOD_NATIVE );
         this.INTERLOCKED_EXCHANGE_I32_METHOD = ctx.NewWrapper( INTERLOCKED_EXCHANGE_I32_METHOD_NATIVE );
         this.INTERLOCKED_EXCHANGE_I64_METHOD = ctx.NewWrapper( INTERLOCKED_EXCHANGE_I64_METHOD_NATIVE );
         this.INTERLOCKED_EXCHANGE_R32_METHOD = INTERLOCKED_EXCHANGE_R32_METHOD_NATIVE == null ? null : ctx.NewWrapper( INTERLOCKED_EXCHANGE_R32_METHOD_NATIVE );
         this.INTERLOCKED_EXCHANGE_R64_METHOD = INTERLOCKED_EXCHANGE_R64_METHOD_NATIVE == null ? null : ctx.NewWrapper( INTERLOCKED_EXCHANGE_R64_METHOD_NATIVE );
         this.INTERLOCKED_EXCHANGE_INT_PTR_METHOD = INTERLOCKED_EXCHANGE_INT_PTR_METHOD_NATIVE == null ? null : ctx.NewWrapper( INTERLOCKED_EXCHANGE_INT_PTR_METHOD_NATIVE );
         this.INTERLOCKED_EXCHANGE_OBJECT_METHOD = INTERLOCKED_EXCHANGE_OBJECT_METHOD_NATIVE == null ? null : ctx.NewWrapper( INTERLOCKED_EXCHANGE_OBJECT_METHOD_NATIVE );
         this.INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD = ctx.NewWrapper( INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD_NATIVE );
         this.INTERLOCKED_COMPARE_EXCHANGE_I64_METHOD = ctx.NewWrapper( INTERLOCKED_COMPARE_EXCHANGE_I64_METHOD_NATIVE );
         this.INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD = INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD_NATIVE == null ? null : ctx.NewWrapper( INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD_NATIVE );
         this.INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD = INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD_NATIVE == null ? null : ctx.NewWrapper( INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD_NATIVE );
         this.INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD = INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD_NATIVE == null ? null : ctx.NewWrapper( INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD_NATIVE );
         this.INTERLOCKED_COMPARE_EXCHANGE_OBJECT_METHOD = ctx.NewWrapper( INTERLOCKED_COMPARE_EXCHANGE_OBJECT_METHOD_NATIVE );
         this.COMPOSITE_TYPE_ID_CTOR = ctx.NewWrapper( COMPOSITE_TYPE_ID_CTOR_NATIVE );
         this.COMPOSITE_FACTORY_METHOD = ctx.NewWrapper( COMPOSITE_FACTORY_METHOD_NATIVE );
         this.COMPOSITE_CALLBACK_GET_COMPOSITE_METHOD_METHOD = ctx.NewWrapper( COMPOSITE_CALLBACK_GET_COMPOSITE_METHOD_METHOD_NATIVE );
         this.ARGUMENT_EXCEPTION_STRING_CTOR = ctx.NewWrapper( ARGUMENT_EXCEPTION_STRING_CTOR_NATIVE );
         this.MAKE_GENERIC_TYPE_METHOD = ctx.NewWrapper( MAKE_GENERIC_TYPE_METHOD_NATIVE );
         this.GET_FIRST_INSTANCE_CTOR = ctx.NewWrapper( GET_FIRST_INSTANCE_CTOR_NATIVE );
         this.CONSTRUCTOR_INVOKE_METHOD = ctx.NewWrapper( CONSTRUCTOR_INVOKE_METHOD_NATIVE );
         this.INT_PTR_SIZE_GETTER = ctx.NewWrapper( INT_PTR_SIZE_GETTER_NATIVE );
         this.REF_ACTION_INVOKER = ctx.NewWrapper( REF_ACTION_INVOKER_NATIVE );
         this.REF_FUNCTION_INVOKER = ctx.NewWrapper( REF_FUNCTION_INVOKER_NATIVE );
         this.REF_INVOKER_CALLBACK_CTOR = ctx.NewWrapper( REF_INVOKER_CALLBACK_CTOR_NATIVE );
         this.COMPOSITE_METHOD_MODEL_INDEX_ATTRIBUTE = ctx.NewWrapper( COMPOSITE_METHOD_MODEL_INDEX_ATTRIBUTE_NATIVE );
         this.SPECIAL_METHOD_MODEL_INDEX_ATTRIBUTE = ctx.NewWrapper( SPECIAL_METHOD_MODEL_INDEX_ATTRIBUTE_NATIVE );
         this.CONSTRUCTOR_MODEL_INDEX_ATTRIBUTE = ctx.NewWrapper( CONSTRUCTOR_MODEL_INDEX_ATTRIBUTE_NATIVE );
         this.DOUBLE_BITS_TO_INT64 = ctx.NewWrapper( DOUBLE_BITS_TO_INT64_NATIVE );
         this.INT64_BITS_TO_DOUBLE = ctx.NewWrapper( INT64_BITS_TO_DOUBLE_NATIVE );
         this.GET_BYTES_INT32 = ctx.NewWrapper( GET_BYTES_INT32_NATIVE );
         this.GET_BYTES_SINGLE = ctx.NewWrapper( GET_BYTES_INT32_NATIVE );
         this.BYTES_TO_INT32 = ctx.NewWrapper( GET_BYTES_INT32_NATIVE );
         this.BYTES_TO_SINGLE = ctx.NewWrapper( GET_BYTES_INT32_NATIVE );
         this.COMPOSITE_TYPES_ATTRIBUTE_CTOR = ctx.NewWrapper( COMPOSITE_TYPES_ATTRIBUTE_CTOR_NATIVE );
         this.COMPOSITE_TYPES_ATTRIBUTE_PUBLIC_TYPES_PROPERTY = ctx.NewWrapper( COMPOSITE_TYPES_ATTRIBUTE_PUBLIC_TYPES_PROPERTY_NATIVE );
         this.COMPOSITE_TYPES_ATTRIBUTE_PRIVATE_TYPES_PROPERTY = ctx.NewWrapper( COMPOSITE_TYPES_ATTRIBUTE_PRIVATE_TYPES_PROPERTY_NATIVE );
         this.COMPOSITE_TYPES_ATTRIBUTE_FRAGMENT_TYPES_PROPERTY = ctx.NewWrapper( COMPOSITE_TYPES_ATTRIBUTE_FRAGMENT_TYPES_PROPERTY_NATIVE );
         this.COMPOSITE_TYPES_ATTRIBUTE_CONCERN_INVOCATION_HANDLER_TYPES_PROPERTY = ctx.NewWrapper( COMPOSITE_TYPES_ATTRIBUTE_CONCERN_INVOCATION_HANDLER_TYPES_PROPERTY_NATIVE );
         this.COMPOSITE_TYPES_ATTRIBUTE_SIDE_EFFECT_INVOCATION_HANDLER_TYPES_PROPERTY = ctx.NewWrapper( COMPOSITE_TYPES_ATTRIBUTE_SIDE_EFFECT_INVOCATION_HANDLER_TYPES_PROPERTY_NATIVE );
         this.COMPOSITE_TYPES_ATTRIBUTE_COMPOSITE_FACTORY_TYPE_PROPERTY = ctx.NewWrapper( COMPOSITE_TYPES_ATTRIBUTE_COMPOSITE_FACTORY_TYPE_PROPERTY_NATIVE );
         this.PUBLIC_COMPOSITE_GENERIC_BINDING_ATTRIBUTE_CTOR = ctx.NewWrapper( PUBLIC_COMPOSITE_GENERIC_BINDING_ATTRIBUTE_CTOR_NATIVE );
         this.PUBLIC_COMPOSITE_GENERIC_BINDING_ATTRIBUTE_PROPERTY = ctx.NewWrapper( PUBLIC_COMPOSITE_GENERIC_BINDING_ATTRIBUTE_PROPERTY_NATIVE );
      }
   }
}
#endif