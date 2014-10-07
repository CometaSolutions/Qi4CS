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
using CILAssemblyManipulator.API;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class ServiceModelTypeCodeGenerator : AbstractCompositeModelTypeCodeGenerator
   {

      protected const String PASSIVATION_METHOD_NAME = "PassivationMethod";
      protected const String ACTIVATION_METHOD_NAME = "ActivationMethod";

      protected static readonly System.Reflection.MethodInfo SERVICE_COMPOSITE_ACTIVATE_IF_NEEDED_METHOD_NATIVE;

      protected readonly CILMethod SERVICE_COMPOSITE_ACTIVATE_IF_NEEDED_METHOD;

      static ServiceModelTypeCodeGenerator()
      {
         SERVICE_COMPOSITE_ACTIVATE_IF_NEEDED_METHOD_NATIVE = typeof( ServiceCompositeInstanceImpl ).LoadMethodOrThrow( "ActivateIfNeeded", null );
         //ACTION_CONSTRUCTOR_NATIVE = TypeUtil.TryLoadConstructor( typeof( Action<ServiceCompositeInstanceImpl> ), 2 );
      }

      public ServiceModelTypeCodeGenerator( Boolean isSilverlight, CILReflectionContext ctx )
         : base( isSilverlight, ctx )
      {
         this.SERVICE_COMPOSITE_ACTIVATE_IF_NEEDED_METHOD = SERVICE_COMPOSITE_ACTIVATE_IF_NEEDED_METHOD_NATIVE.NewWrapper( this.ctx );
      }

      protected override void EmitAfterCompositeMethodBodyBegan(
         IEnumerable<FragmentTypeGenerationInfo> fragmentTypeGenerationInfos,
         CompositeTypeGenerationInfo thisGenerationInfo,
         CompositeMethodGenerationInfo thisMethodGenerationInfo,
         SPI.Model.CompositeMethodModel compositeMethodModel
         )
      {
         var instanceableModel = compositeMethodModel.CompositeModel;

         // Need to emit code related to activation
         var il = thisMethodGenerationInfo.IL;
         var cInstanceB = thisMethodGenerationInfo.GetLocalOrThrow( LB_C_INSTANCE );

         LocalBuilder compositeMethodModelB = null;
         Boolean hasOnInvocationInjections = this.HasOnInvocationInjections( compositeMethodModel.CompositeModel.SpecialMethods.Where( sMethod => sMethod.AllAttributes.OfType<ActivateAttribute>().Any() || sMethod.AllAttributes.OfType<PrototypeAttribute>().Any() ) );
         if ( hasOnInvocationInjections )
         {
            this.InitializeComplexMethodModelLocalIfNecessary( compositeMethodModel, thisMethodGenerationInfo, out compositeMethodModelB );
         }

         // cInstance.ActivateIfNeeded(<method>, <next fragment>);
         il.EmitLoadLocal( cInstanceB )
           .EmitReflectionObjectOf( thisMethodGenerationInfo.OverriddenMethod.MakeGenericMethod( thisMethodGenerationInfo.GenericArguments.ToArray() ) );
         if ( hasOnInvocationInjections )
         {
            il.EmitLoadLocal( compositeMethodModelB );
            if ( compositeMethodModel.Concerns.Any() )
            {
               il.EmitCall( CONCERN_MODELS_GETTER )
                 .EmitLoadInt32( 0 )
                 .EmitCall( CONCERN_MODELS_INDEXER );
            }
            else
            {
               il.EmitCall( MIXIN_MODEL_GETTER );
            }
         }
         else
         {
            il.EmitLoadNull();
         }
         il.EmitCall( SERVICE_COMPOSITE_ACTIVATE_IF_NEEDED_METHOD );
      }

      protected override IEnumerable<CILType> GetPublicCompositeAdditionalArguments( CompositeTypeGenerationInfo typeGenInfo )
      {
         return base.GetPublicCompositeAdditionalArguments( typeGenInfo ).Concat( ACTION_REF_TYPES_2 );
      }

      protected override void EmitTheRestOfPublicCompositeConstructor(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel model,
         CompositeTypeModel typeModel,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos,
         CompositeTypeGenerationInfo thisGenerationInfo,
         ConstructorGenerationInfo ctorGenerationInfo,
         Int32 firstAdditionalParameterIndex
         )
      {
         base.EmitTheRestOfPublicCompositeConstructor( codeGenerationInfo, model, typeModel, emittingInfo, fragmentGenerationInfos, thisGenerationInfo, ctorGenerationInfo, firstAdditionalParameterIndex );

         Int32 baseAdditionalParamsCount = base.GetAmountOfAdditionalArgumentsForPublicCompositeConstructor();

         this.EmitSetActionMethod<ActivateAttribute>(
            codeGenerationInfo,
            model,
            typeModel,
            fragmentGenerationInfos,
            thisGenerationInfo,
            thisGenerationInfo,
            emittingInfo,
            ctorGenerationInfo,
            firstAdditionalParameterIndex + baseAdditionalParamsCount,
            ACTIVATION_METHOD_NAME,
            false
            );

         this.EmitSetActionMethod<PassivateAttribute>(
            codeGenerationInfo,
            model,
            typeModel,
            fragmentGenerationInfos,
            thisGenerationInfo,
            thisGenerationInfo,
            emittingInfo,
            ctorGenerationInfo,
            firstAdditionalParameterIndex + baseAdditionalParamsCount + 1,
            PASSIVATION_METHOD_NAME,
            true
            );
      }

      protected override Int32 GetAmountOfAdditionalArgumentsForPublicCompositeConstructor()
      {
         return base.GetAmountOfAdditionalArgumentsForPublicCompositeConstructor() + ACTION_REF_TYPES_2.Length;
      }
   }
}
#endif