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
#if !LOAD_ONLY
using System;
using System.Linq;
using CILAssemblyManipulator.API;

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
      protected readonly CILType WEAK_EVENT_WRAPPER_TYPE;
      protected readonly CILType STRONG_EVENT_WRAPPER_TYPE;
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
      protected readonly CILMethod GET_METHOD_METHOD;
      protected readonly CILMethod GET_CTOR_INDEX_METHOD;
      protected readonly CILMethod BASE_TYPE_GETTER;
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
      protected readonly CILMethod ASSEMBLY_GETTER;
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
      protected readonly CILMethod WEAK_EVENT_ARRAY_WRAPPER_ARRAY_GETTER;
      protected readonly CILMethod WEAK_EVENT_ARRAY_WRAPPER_COUNT_GETTER;
      protected readonly CILMethod WEAK_EVENT_ARRAY_CLEANUP_METHOD;
      protected readonly CILMethod WEAK_EVENT_ARRAY_COMBINE_METHOD;
      protected readonly CILMethod WEAK_EVENT_ARRAY_REMOVE_METHOD;
      protected readonly CILMethod IS_EVENT_INFO_DEAD_METHOD;
      protected readonly CILMethod EVENT_INFO_TARGET_GETTER;
      protected readonly CILMethod EVENT_INFO_METHOD_GETTER;
      protected readonly CILConstructor EVENT_INFO_CTOR;
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
      protected readonly CILConstructor ARGUMENT_EXCEPTION_STRING_CTOR;
      protected readonly CILMethod MAKE_GENERIC_TYPE_METHOD;
      protected readonly CILMethod GET_CONSTRUCTORS_METHOD;
      protected readonly CILMethod CONSTRUCTOR_INVOKE_METHOD;
      protected readonly CILMethod INT_PTR_SIZE_GETTER;
      protected readonly CILMethod REF_ACTION_INVOKER;
      protected readonly CILMethod REF_FUNCTION_INVOKER;
      protected readonly CILConstructor REF_INVOKER_CALLBACK_CTOR;

      protected readonly CILReflectionContext ctx;
      protected readonly Boolean wp8Emit;

      protected AbstractCompositeModelTypeCodeGenerator( Boolean isWP8Emit, CILReflectionContext aCtx )
      {
         this.wp8Emit = isWP8Emit;
         this.ctx = aCtx;

         this.COMPOSITE_CTOR_PROPERTIES_PARAM_TYPE = COMPOSITE_CTOR_PROPERTIES_PARAM_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.COMPOSITE_CTOR_EVENTS_PARAM_TYPE = COMPOSITE_CTOR_EVENTS_PARAM_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.ACTION_REF_TYPE = ACTION_REF_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.ACTION_REF_TYPES_1 = ACTION_REF_TYPES_1_NATIVE.Select( t => t.NewWrapperAsType( ctx ) ).ToArray();
         this.ACTION_REF_TYPES_2 = ACTION_REF_TYPES_2_NATIVE.Select( t => t.NewWrapperAsType( ctx ) ).ToArray();
         this.CHECK_STATE_FUNC_TYPE = CHECK_STATE_FUNC_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.PUBLIC_COMPOSITE_CTOR_ADDITTIONAL_PARAM_TYPES = PUBLIC_COMPOSITE_CTOR_ADDITTIONAL_PARAM_TYPES_NATIVE.Select( t => t.NewWrapperAsType( ctx ) ).ToArray();
         this.ACTION_TYPE = ACTION_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.FRAGMENT_DEPENDANT_TYPE = FRAGMENT_DEPENDANT_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.FIELD_MODEL_TYPE = FIELD_MODEL_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.CONSTRUCTOR_MODEL_TYPE = CONSTRUCTOR_MODEL_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.PARAMETER_MODEL_TYPE = PARAMETER_MODEL_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.SPECIAL_METHOD_MODEL_TYPE = SPECIAL_METHOD_MODEL_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.COMPOSITE_METHOD_MODEL_TYPE = COMPOSITE_METHOD_MODEL_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.CONSTRAINT_MODEL_TYPE = CONSTRAINT_MODEL_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.VOID_TYPE = VOID_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.OBJECT_TYPE = OBJECT_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.ATTRIBUTE_TYPE = ATTRIBUTE_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.TYPE_TYPE = TYPE_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.ABSTRACT_INJECTABLE_MODEL_TYPE = ABSTRACT_INJECTABLE_MODEL_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.CONSTRAINT_TYPE = CONSTRAINT_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.INT32_TYPE = INT32_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.UINT32_TYPE = UINT32_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.INT64_TYPE = INT64_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.BOOLEAN_TYPE = BOOLEAN_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.SINGLE_TYPE = SINGLE_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.DOUBLE_TYPE = DOUBLE_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.INT_PTR_TYPE = INT_PTR_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.STRING_TYPE = STRING_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.EXCEPTION_TYPE = EXCEPTION_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.WEAK_EVENT_WRAPPER_TYPE = WEAK_EVENT_WRAPPER_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.STRONG_EVENT_WRAPPER_TYPE = STRONG_EVENT_WRAPPER_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.IENUMERABLE_GDEF_TYPE = IENUMERABLE_GDEF_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.IENUMERABLE_NO_GDEF_TYPE = IENUMERABLE_NO_GDEF_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.USE_DEFAULTS_ATTRIBUTE_TYPE = USE_DEFAULTS_ATTRIBUTE_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.COMPOSITE_FACTORY_TYPE = COMPOSITE_FACTORY_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.REF_ACTION_TYPE = REF_ACTION_TYPE_NATIVE.NewWrapperAsType( ctx );
         this.REF_FUNCTION_TYPE = REF_FUNCTION_TYPE_NATIVE.NewWrapperAsType( ctx );

         this.APPLICATION_GETTER_METHOD = APPLICATION_GETTER_METHOD_NATIVE.NewWrapper( ctx );
         this.STRUCTURE_OWNER_GETTER_METHOD = STRUCTURE_OWNER_GETTER_METHOD_NATIVE.NewWrapper( ctx );
         this.APPLICATION_IS_PASSIVE_GETTER_METHOD = APPLICATION_IS_PASSIVE_GETTER_METHOD_NATIVE.NewWrapper( ctx );
         this.INJECTION_SERVICE_GETTER_METHOD = INJECTION_SERVICE_GETTER_METHOD_NATIVE.NewWrapper( ctx );
         this.COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER = COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER_NATIVE.NewWrapper( ctx );
         this.COMPOSITE_METHOD_RESULT_GETTER = COMPOSITE_METHOD_RESULT_GETTER_NATIVE.NewWrapper( ctx );
         //this.INJECTABLE_MODEL_INJECTION_SCOPES_GETTER = INJECTABLE_MODEL_INJECTION_SCOPES_GETTER_NATIVE.NewWrapper( ctx );
         this.INJECTION_CONTEXT_PROVIDER_METHOD = INJECTION_CONTEXT_PROVIDER_METHOD_NATIVE.NewWrapper( ctx );
         this.OPTIONAL_ATTRIBUTE_FIELD = OPTIONAL_ATTRIBUTE_FIELD_NATIVE.NewWrapper( ctx );
         this.CONSTRAINT_MODELS_GETTER = CONSTRAINT_MODELS_GETTER_NATIVE.NewWrapper( ctx );
         this.CONSTRAINT_ATTRIBUTE_GETTER = CONSTRAINT_ATTRIBUTE_GETTER_NATIVE.NewWrapper( ctx );
         this.VIOLATIONS_LIST_COUNT_GETTER = VIOLATIONS_LIST_COUNT_GETTER_NATIVE.NewWrapper( ctx );
         this.METHOD_INFO_NATIVE_GETTER = METHOD_INFO_NATIVE_GETTER_NATIVE.NewWrapper( ctx );
         this.MODEL_INFO_GETTER = MODEL_INFO_GETTER_NATIVE.NewWrapper( ctx );
         this.MODEL_GETTER = MODEL_GETTER_NATIVE.NewWrapper( ctx );
         this.C_METHODS_GETTER = C_METHODS_GETTER_NATIVE.NewWrapper( ctx );
         this.GET_FRAGMENT_INSTANCE_POOL_METHOD = GET_FRAGMENT_INSTANCE_POOL_METHOD_NATIVE.NewWrapper( ctx );
         this.GET_FRAGMENT_INSTANCE_METHOD = GET_FRAGMENT_INSTANCE_METHOD_NATIVE.NewWrapper( ctx );
         this.TAKE_FRAGMENT_INSTANCE_METHOD = TAKE_FRAGMENT_INSTANCE_METHOD_NATIVE.NewWrapper( ctx );
         this.RETURN_FRAGMENT_INSTANCE_METHOD = RETURN_FRAGMENT_INSTANCE_METHOD_NATIVE.NewWrapper( ctx );
         this.SPECIAL_METHODS_GETTER = SPECIAL_METHODS_GETTER_NATIVE.NewWrapper( ctx );
         this.FRAGMENT_GETTER = FRAGMENT_GETTER_NATIVE.NewWrapper( ctx );
         this.FRAGMENT_SETTER = FRAGMENT_SETTER_NATIVE.NewWrapper( ctx );
         this.COMPOSITES_GETTER = COMPOSITES_GETTER_NATIVE.NewWrapper( ctx );
         this.COMPOSITES_GETTER_INDEXER = COMPOSITES_GETTER_INDEXER_NATIVE.NewWrapper( ctx );
         this.TYPE_OBJECT_DICTIONARY_GET_METHOD = TYPE_OBJECT_DICTIONARY_GET_METHOD_NATIVE.NewWrapper( ctx );
         this.GENERIC_FRAGMENT_METHOD = GENERIC_FRAGMENT_METHOD_NATIVE.NewWrapper( ctx );
         this.CONSTRAINT_VIOLATION_CONSTRUCTOR = CONSTRAINT_VIOLATION_CONSTRUCTOR_NATIVE.NewWrapper( ctx );
         this.ADD_CONSTRAINT_VIOLATION_METHOD = ADD_CONSTRAINT_VIOLATION_METHOD_NATIVE.NewWrapper( ctx );
         this.F_INSTANCE_SET_NEXT_INFO_METHOD = F_INSTANCE_SET_NEXT_INFO_METHOD_NATIVE.NewWrapper( ctx );
         this.F_INSTANCE_SET_METHOD_RESULT_METHOD = F_INSTANCE_SET_METHOD_RESULT_METHOD_NATIVE.NewWrapper( ctx );
         this.F_INSTANCE_GET_NEXT_INFO_METHOD = F_INSTANCE_GET_NEXT_INFO_METHOD_NATIVE.NewWrapper( ctx );
         this.F_INSTANCE_GET_METHOD_RESULT_METHOD = F_INSTANCE_GET_METHOD_RESULT_METHOD_NATIVE.NewWrapper( ctx );
         this.STRING_CONCAT_METHOD_3 = STRING_CONCAT_METHOD_3_NATIVE.NewWrapper( ctx );
         this.STRING_CONCAT_METHOD_2 = STRING_CONCAT_METHOD_2_NATIVE.NewWrapper( ctx );
         this.METHOD_INFO_GET_GARGS_METHOD = METHOD_INFO_GET_GARGS_METHOD_NATIVE.NewWrapper( ctx );
         this.MAKE_GENERIC_METHOD_METHOD = MAKE_GENERIC_METHOD_METHOD_NATIVE.NewWrapper( ctx );
         this.INVOKE_METHOD_METHOD = INVOKE_METHOD_METHOD_NATIVE.NewWrapper( ctx );
         this.GET_METHOD_METHOD = GET_METHOD_METHOD_NATIVE.NewWrapper( ctx );
         this.GET_CTOR_INDEX_METHOD = GET_CTOR_INDEX_METHOD_NATIVE.NewWrapper( ctx );
         this.BASE_TYPE_GETTER = BASE_TYPE_GETTER_NATIVE.NewWrapper( ctx );
         this.APPLICATION_NOT_ACTIVE_EXCEPTION_CONSTRUCTOR = APPLICATION_NOT_ACTIVE_EXCEPTION_CONSTRUCTOR_NATIVE.NewWrapper( ctx );
         this.GET_CONSTRAINT_INSTANCE_POOL_METHOD = GET_CONSTRAINT_INSTANCE_POOL_METHOD_NATIVE.NewWrapper( ctx );
         this.TAKE_CONSTRAINT_INSTANCE_METHOD = TAKE_CONSTRAINT_INSTANCE_METHOD_NATIVE.NewWrapper( ctx );
         this.RETURN_CONSTRAINT_INSTANCE_METHOD = RETURN_CONSTRAINT_INSTANCE_METHOD_NATIVE.NewWrapper( ctx );
         this.IS_VALID_METHOD = IS_VALID_METHOD_NATIVE.NewWrapper( ctx );
         this.TO_STRING_METHOD = TO_STRING_METHOD_NATIVE.NewWrapper( ctx );
         this.RETURN_CONCERN_INVOCATION_METHOD = RETURN_CONCERN_INVOCATION_METHOD_NATIVE.NewWrapper( ctx );
         this.RETURN_SIDE_EFFECT_INVOCATION_METHOD = RETURN_SIDE_EFFECT_INVOCATION_METHOD_NATIVE.NewWrapper( ctx );
         this.EMPTY_OBJECTS_FIELD = EMPTY_OBJECTS_FIELD_NATIVE.NewWrapper( ctx );
         this.LIST_QUERY_ITEM_GETTER = LIST_QUERY_ITEM_GETTER_NATIVE.NewWrapper( ctx );
         this.FIELDS_GETTER = FIELDS_GETTER_NATIVE.NewWrapper( ctx );
         this.FIELD_SET_VALUE_METHOD = FIELD_SET_VALUE_METHOD_NATIVE.NewWrapper( ctx );
         this.PROTOTYPE_ACTION_CONSTRUCTOR = PROTOTYPE_ACTION_CONSTRUCTOR_NATIVE.NewWrapper( ctx );
         this.EQUALS_METHOD = EQUALS_METHOD_NATIVE.NewWrapper( ctx );
         this.HASH_CODE_METHOD = HASH_CODE_METHOD_NATIVE.NewWrapper( ctx );
         this.REFERENCE_EQUALS_METHOD = REFERENCE_EQUALS_METHOD_NATIVE.NewWrapper( ctx );
         this.GET_TYPE_METHOD = GET_TYPE_METHOD_NATIVE.NewWrapper( ctx );
         this.ASSEMBLY_GETTER = ASSEMBLY_GETTER_NATIVE.NewWrapper( ctx );
         this.DELEGATE_COMBINE_METHOD = DELEGATE_COMBINE_METHOD_NATIVE.NewWrapper( ctx );
         this.DELEGATE_REMOVE_METHOD = DELEGATE_REMOVE_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF = INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF_NATIVE.NewWrapper( ctx );
         this.GET_EVENT_INFO_METHOD = GET_EVENT_INFO_METHOD_NATIVE.NewWrapper( ctx );
         this.COMPOSITE_EVENT_CTOR = COMPOSITE_EVENT_CTOR_NATIVE.NewWrapper( ctx );
         this.INVALID_OPERATION_EXCEPTION_CTOR_WITH_STRING = INVALID_OPERATION_EXCEPTION_CTOR_WITH_STRING_NATIVE.NewWrapper( ctx );
         this.QNAME_FROM_TYPE_AND_NAME = QNAME_FROM_TYPE_AND_NAME_NATIVE.NewWrapper( ctx );
         this.IS_PROTOTYPE_GETTER = IS_PROTOTYPE_GETTER_NATIVE.NewWrapper( ctx );
         this.GET_PROPERTY_INFO_METHOD = GET_PROPERTY_INFO_METHOD_NATIVE.NewWrapper( ctx );
         this.COMPOSITE_METHODS_INDEXER = COMPOSITE_METHODS_INDEXER_NATIVE.NewWrapper( ctx );
         this.EVENT_MODEL_GETTER = EVENT_MODEL_GETTER_NATIVE.NewWrapper( ctx );
         this.PROPERTY_MODEL_GETTER = PROPERTY_MODEL_GETTER_NATIVE.NewWrapper( ctx );
         this.COMPOSITE_PROPERTY_CTOR = COMPOSITE_PROPERTY_CTOR_NATIVE.NewWrapper( ctx );
         this.INVOCATION_INFO_GETTER = INVOCATION_INFO_GETTER_NATIVE.NewWrapper( ctx );
         this.INVOCATION_INFO_SETTER = INVOCATION_INFO_SETTER_NATIVE.NewWrapper( ctx );
         this.INVOCATION_INFO_CREATOR_CTOR = INVOCATION_INFO_CREATOR_CTOR_NATIVE.NewWrapper( ctx );
         this.INVOCATION_INFO_METHOD_GETTER = INVOCATION_INFO_METHOD_GETTER_NATIVE.NewWrapper( ctx );
         this.INVOCATION_INFO_FRAGMENT_METHOD_MODEL_GETTER = INVOCATION_INFO_FRAGMENT_METHOD_MODEL_GETTER_NATIVE.NewWrapper( ctx );
         this.INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER = INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER_NATIVE.NewWrapper( ctx );
         this.CONCERN_MODELS_GETTER = CONCERN_MODELS_GETTER_NATIVE.NewWrapper( ctx );
         this.CONCERN_MODELS_INDEXER = CONCERN_MODELS_INDEXER_NATIVE.NewWrapper( ctx );
         this.MIXIN_MODEL_GETTER = MIXIN_MODEL_GETTER_NATIVE.NewWrapper( ctx );
         this.SIDE_EFFECT_MODELS_GETTER = SIDE_EFFECT_MODELS_GETTER_NATIVE.NewWrapper( ctx );
         this.SIDE_EFFECT_MODELS_INDEXER = SIDE_EFFECT_MODELS_INDEXER_NATIVE.NewWrapper( ctx );
         this.COLLECTION_ADD_ONLY_ADD_METHOD = COLLECTION_ADD_ONLY_ADD_METHOD_NATIVE.NewWrapper( ctx );
         this.ACTION_0_CTOR = ACTION_0_CTOR_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_EXCHANGE_METHOD_GDEF = INTERLOCKED_EXCHANGE_METHOD_GDEF_NATIVE.NewWrapper( ctx );
         this.GET_INVOCATION_LIST_METHOD = GET_INVOCATION_LIST_METHOD_NATIVE.NewWrapper( ctx );
         this.ADD_LAST_METHOD = ADD_LAST_METHOD_NATIVE.NewWrapper( ctx );
         this.WEAK_EVENT_ARRAY_WRAPPER_ARRAY_GETTER = WEAK_EVENT_ARRAY_WRAPPER_ARRAY_GETTER_NATIVE.NewWrapper( ctx );
         this.WEAK_EVENT_ARRAY_WRAPPER_COUNT_GETTER = WEAK_EVENT_ARRAY_WRAPPER_COUNT_GETTER_NATIVE.NewWrapper( ctx );
         this.WEAK_EVENT_ARRAY_CLEANUP_METHOD = WEAK_EVENT_ARRAY_CLEANUP_METHOD_NATIVE.NewWrapper( ctx );
         this.WEAK_EVENT_ARRAY_COMBINE_METHOD = WEAK_EVENT_ARRAY_COMBINE_METHOD_NATIVE.NewWrapper( ctx );
         this.WEAK_EVENT_ARRAY_REMOVE_METHOD = WEAK_EVENT_ARRAY_REMOVE_METHOD_NATIVE.NewWrapper( ctx );
         this.IS_EVENT_INFO_DEAD_METHOD = IS_EVENT_INFO_DEAD_METHOD_NATIVE.NewWrapper( ctx );
         this.EVENT_INFO_TARGET_GETTER = EVENT_INFO_TARGET_GETTER_NATIVE.NewWrapper( ctx );
         this.EVENT_INFO_METHOD_GETTER = EVENT_INFO_METHOD_GETTER_NATIVE.NewWrapper( ctx );
         this.EVENT_INFO_CTOR = EVENT_INFO_CTOR_NATIVE.NewWrapper( ctx );
         this.Q_NAME_GET_BARE_TYPE_NAME_METHOD = Q_NAME_GET_BARE_TYPE_NAME_METHOD_NATIVE.NewWrapper( ctx );
         this.Q_NAME_FROM_MEMBER_INFO_METHOD = Q_NAME_FROM_MEMBER_INFO_METHOD_NATIVE.NewWrapper( ctx );
         this.INJECTION_EXCEPTION_CTOR = INJECTION_EXCEPTION_CTOR_NATIVE.NewWrapper( ctx );
         this.CHECK_STATE_METHOD_SIG = CHECK_STATE_METHOD_SIG_NATIVE.NewWrapper( ctx );
         this.CONSTRAINT_EXCEPTION_VIOLATIONS_GETTER = CONSTRAINT_EXCEPTION_VIOLATIONS_GETTER_NATIVE.NewWrapper( ctx );
         this.CONSTRAINT_VIOLATIONS_DIC_ADD_METHOD = CONSTRAINT_VIOLATIONS_DIC_ADD_METHOD_NATIVE.NewWrapper( ctx );
         this.CONSTRAINT_VIOLATIONS_DIC_CTOR = CONSTRAINT_VIOLATIONS_DIC_CTOR_NATIVE.NewWrapper( ctx );
         this.CHECK_ACTION_FUNC_CTOR = CHECK_ACTION_FUNC_CTOR_NATIVE.NewWrapper( ctx );
         this.SET_DEFAULTS_METHOD_SIG = SET_DEFAULTS_METHOD_SIG_NATIVE.NewWrapper( ctx );
         this.CONSTRAINT_VIOLATIONS_LIST_CTOR = CONSTRAINT_VIOLATIONS_LIST_CTOR_NATIVE.NewWrapper( ctx );
         this.EXCEPTION_LIST_CTOR = EXCEPTION_LIST_CTOR_NATIVE.NewWrapper( ctx );
         this.FUNC_1_CTOR = FUNC_1_CTOR_NATIVE.NewWrapper( ctx );
         this.FUNC_2_CTOR = FUNC_2_CTOR_NATIVE.NewWrapper( ctx );
         this.FUNC_3_CTOR = FUNC_3_CTOR_NATIVE.NewWrapper( ctx );
         this.ACTION_1_CTOR = ACTION_1_CTOR_NATIVE.NewWrapper( ctx );
         this.LAZY_GDEF_CTOR = LAZY_GDEF_CTOR_NATIVE.NewWrapper( ctx );
         this.FRAGMENT_INSTANCE_CTOR_NO_PARAMS = FRAGMENT_INSTANCE_CTOR_NO_PARAMS_NATIVE.NewWrapper( ctx );
         this.FRAGMENT_INSTANCE_CTOR_WITH_PARAMS = FRAGMENT_INSTANCE_CTOR_WITH_PARAMS_NATIVE.NewWrapper( ctx );
         this.MODEL_CTORS_GETTER = MODEL_CTORS_GETTER_NATIVE.NewWrapper( ctx );
         this.FRAGMENT_DEPENDANT_GETTER = FRAGMENT_DEPENDANT_GETTER_NATIVE.NewWrapper( ctx );
         this.FRAGMENT_DEPENDANT_SETTER = FRAGMENT_DEPENDANT_SETTER_NATIVE.NewWrapper( ctx );
         this.FRAGMENT_DEPENDANT_PROPERTY = FRAGMENT_DEPENDANT_PROPERTY_NATIVE.NewWrapper( ctx );
         this.CONCERN_INVOCATION_INFO_ITEM_1 = CONCERN_INVOCATION_INFO_ITEM_1_NATIVE.NewWrapper( ctx );
         this.CONCERN_INVOCATION_INFO_ITEM_2 = CONCERN_INVOCATION_INFO_ITEM_2_NATIVE.NewWrapper( ctx );
         this.SIDE_EFFECT_INVOCATION_INFO_ITEM_1 = SIDE_EFFECT_INVOCATION_INFO_ITEM_1_NATIVE.NewWrapper( ctx );
         this.SIDE_EFFECT_INVOCATION_INFO_ITEM_2 = SIDE_EFFECT_INVOCATION_INFO_ITEM_2_NATIVE.NewWrapper( ctx );
         this.SIDE_EFFECT_INVOCATION_INFO_ITEM_3 = SIDE_EFFECT_INVOCATION_INFO_ITEM_3_NATIVE.NewWrapper( ctx );
         this.OBJECT_CTOR = OBJECT_CTOR_NATIVE.NewWrapper( ctx );
         this.NULLABLE_CTOR = NULLABLE_CTOR_NATIVE.NewWrapper( ctx );
         this.DEFAULT_CREATOR_GETTER = DEFAULT_CREATOR_GETTER_NATIVE.NewWrapper( ctx );
         this.DEFAULT_CREATOR_INVOKER = DEFAULT_CREATOR_INVOKER_NATIVE.NewWrapper( ctx );
         this.NO_POOL_ATTRIBUTE_CTOR = NO_POOL_ATTRIBUTE_CTOR_NATIVE.NewWrapper( ctx );
         this.MAIN_PUBLIC_COMPOSITE_TYPE_ATTRIBUTE_CTOR = MAIN_PUBLIC_COMPOSITE_TYPE_ATTRIBUTE_CTOR_NATIVE.NewWrapper( ctx );
         this.CONSTRAINT_VIOLATION_LIST_CTOR = CONSTRAINT_VIOLATION_LIST_CTOR_NATIVE.NewWrapper( ctx );
         this.CONSTRAINT_VIOLATION_EXCEPTION_CTOR = CONSTRAINT_VIOLATION_EXCEPTION_CTOR_NATIVE.NewWrapper( ctx );
         this.INTERNAL_EXCEPTION_CTOR = INTERNAL_EXCEPTION_CTOR_NATIVE.NewWrapper( ctx );
         this.AGGREGATE_EXCEPTION_EXCEPTION_ENUMERABLE_CTOR = AGGREGATE_EXCEPTION_EXCEPTION_ENUMERABLE_CTOR_NATIVE.NewWrapper( ctx );
         this.DEBUGGER_DISPLAY_ATTRIBUTE_STRING_CTOR = DEBUGGER_DISPLAY_ATTRIBUTE_STRING_CTOR_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_READ_I64_METHOD = INTERLOCKED_READ_I64_METHOD_NATIVE == null ? null : INTERLOCKED_READ_I64_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_EXCHANGE_I32_METHOD = INTERLOCKED_EXCHANGE_I32_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_EXCHANGE_I64_METHOD = INTERLOCKED_EXCHANGE_I64_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_EXCHANGE_R32_METHOD = INTERLOCKED_EXCHANGE_R32_METHOD_NATIVE == null ? null : INTERLOCKED_EXCHANGE_R32_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_EXCHANGE_R64_METHOD = INTERLOCKED_EXCHANGE_R64_METHOD_NATIVE == null ? null : INTERLOCKED_EXCHANGE_R64_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_EXCHANGE_INT_PTR_METHOD = INTERLOCKED_EXCHANGE_INT_PTR_METHOD_NATIVE == null ? null : INTERLOCKED_EXCHANGE_INT_PTR_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_EXCHANGE_OBJECT_METHOD = INTERLOCKED_EXCHANGE_OBJECT_METHOD_NATIVE == null ? null : INTERLOCKED_EXCHANGE_OBJECT_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD = INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_COMPARE_EXCHANGE_I64_METHOD = INTERLOCKED_COMPARE_EXCHANGE_I64_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD = INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD_NATIVE == null ? null : INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD = INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD_NATIVE == null ? null : INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD = INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD_NATIVE == null ? null : INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD_NATIVE.NewWrapper( ctx );
         this.INTERLOCKED_COMPARE_EXCHANGE_OBJECT_METHOD = INTERLOCKED_COMPARE_EXCHANGE_OBJECT_METHOD_NATIVE.NewWrapper( ctx );
         this.COMPOSITE_TYPE_ID_CTOR = COMPOSITE_TYPE_ID_CTOR_NATIVE.NewWrapper( ctx );
         this.COMPOSITE_FACTORY_METHOD = COMPOSITE_FACTORY_METHOD_NATIVE.NewWrapper( ctx );
         this.ARGUMENT_EXCEPTION_STRING_CTOR = ARGUMENT_EXCEPTION_STRING_CTOR_NATIVE.NewWrapper( ctx );
         this.MAKE_GENERIC_TYPE_METHOD = MAKE_GENERIC_TYPE_METHOD_NATIVE.NewWrapper( ctx );
         this.GET_CONSTRUCTORS_METHOD = GET_CONSTRUCTORS_METHOD_NATIVE.NewWrapper( ctx );
         this.CONSTRUCTOR_INVOKE_METHOD = CONSTRUCTOR_INVOKE_METHOD_NATIVE.NewWrapper( ctx );
         this.INT_PTR_SIZE_GETTER = INT_PTR_SIZE_GETTER_NATIVE.NewWrapper( ctx );
         this.REF_ACTION_INVOKER = REF_ACTION_INVOKER_NATIVE.NewWrapper( ctx );
         this.REF_FUNCTION_INVOKER = REF_FUNCTION_INVOKER_NATIVE.NewWrapper( ctx );
         this.REF_INVOKER_CALLBACK_CTOR = REF_INVOKER_CALLBACK_CTOR_NATIVE.NewWrapper( ctx );
      }
   }
}
#endif