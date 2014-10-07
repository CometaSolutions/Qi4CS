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
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class PlainCompositeModelTypeCodeGenerator : AbstractCompositeModelTypeCodeGenerator
   {

      public PlainCompositeModelTypeCodeGenerator( Boolean isSilverlight, CILReflectionContext ctx )
         : base( isSilverlight, ctx )
      {
      }

      protected override void EmitPrivateCompositeEquals(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
      )
      {
         this.EmitPlainCompositeEquals( codeGenerationInfo, compositeModel, typeModel, publicCompositeGenInfo, thisGenInfo, emittingInfo, fragmentGenerationInfos );
      }

      protected override void EmitPrivateCompositeHashCode(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         this.EmitPlainCompositeHashCode( codeGenerationInfo, compositeModel, typeModel, publicCompositeGenInfo, thisGenInfo, emittingInfo, fragmentGenerationInfos );
      }

      protected override void EmitPublicCompositeEquals(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         this.EmitPlainCompositeEquals( codeGenerationInfo, compositeModel, typeModel, publicCompositeGenInfo, thisGenInfo, emittingInfo, fragmentGenerationInfos );
      }

      protected override void EmitPublicCompositeHashCode(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         this.EmitPlainCompositeHashCode( codeGenerationInfo, compositeModel, typeModel, publicCompositeGenInfo, thisGenInfo, emittingInfo, fragmentGenerationInfos );
      }

      protected virtual void EmitPlainCompositeEquals(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         // return Object.ReferenceEquals(this, obj) || (obj is <composite type> && <equals-implementation>);
         ILLabel? returnTrueL = null;
         ILLabel? returnFalseL = null;
         this.EmitCallNonCompositeMethod(
            codeGenerationInfo,
            compositeModel,
            typeModel,
            publicCompositeGenInfo,
            thisGenInfo,
            emittingInfo,
            fragmentGenerationInfos,
            EQUALS_METHOD,
            il =>
            {
               // Make labels for true and false values
               returnTrueL = il.DefineLabel();
               returnFalseL = il.DefineLabel();

               il.EmitLoadArg( 0 )
                 .EmitLoadArg( 1 )
                 .EmitCall( REFERENCE_EQUALS_METHOD )
                 .EmitBranch( BranchType.IF_TRUE, returnTrueL.Value );

               var continueLabel = il.DefineLabel();
               foreach ( var parent in thisGenInfo.Parents.Values.OnlyBottomTypes() )
               {
                  il.EmitLoadArg( 1 )
                    .EmitIsInst( parent )
                    .EmitBranch( BranchType.IF_TRUE, continueLabel );
               }
               il.EmitBranch( BranchType.ALWAYS, returnFalseL.Value )
                 .MarkLabel( continueLabel );
            },
            il =>
            {
               // return true
               il.MarkLabel( returnTrueL.Value )
                 .EmitLoadBoolean( true )
                 .EmitReturn()

               // return false
                 .MarkLabel( returnFalseL.Value )
                 .EmitLoadBoolean( false )
                 .EmitReturn();
            },
            genInfos => genInfos.FirstOrDefault( genInfo =>
            {
               var result = thisGenInfo.Parents.Keys.Where( t => !thisGenInfo.Builder.Equals( t ) ).OnlyBottomTypes().Where( t => !OBJECT_TYPE.Equals( t ) ).Any( p => genInfo.Parents.ContainsKey( p.GenericDefinitionIfGArgsHaveGenericParams() ) )
                  || genInfo.DirectBaseFromModel.FullInheritanceChain().Any( t => compositeModel.ApplicationModel.GenericFragmentBaseType.NewWrapperAsType( this.ctx ).Equals( t ) );
               if ( result )
               {
                  var m = TypeGenerationUtils.FindMethodImplicitlyImplementingMethod( genInfo.DirectBaseFromModel, EQUALS_METHOD );
                  result = !EQUALS_METHOD.Equals( m );
               }
               return result;
            } )
            );
      }

      protected virtual void EmitPlainCompositeHashCode(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         this.EmitCallNonCompositeMethod(
            codeGenerationInfo,
            compositeModel,
            typeModel,
            publicCompositeGenInfo,
            thisGenInfo,
            emittingInfo,
            fragmentGenerationInfos,
            HASH_CODE_METHOD,
            null,
            null,
            genInfos => genInfos.FirstOrDefault( genInfo =>
            {
               var result = thisGenInfo.Parents.Keys.Where( t => !thisGenInfo.Builder.Equals( t ) ).OnlyBottomTypes().Where( t => !OBJECT_TYPE.Equals( t ) ).Any( p => genInfo.Parents.ContainsKey( p.GenericDefinitionIfGArgsHaveGenericParams() ) )
                  || genInfo.DirectBaseFromModel.FullInheritanceChain().Any( t => compositeModel.ApplicationModel.GenericFragmentBaseType.NewWrapperAsType( this.ctx ).Equals( t ) );
               if ( result )
               {
                  var m = TypeGenerationUtils.FindMethodImplicitlyImplementingMethod( genInfo.DirectBaseFromModel, HASH_CODE_METHOD );
                  result = !HASH_CODE_METHOD.Equals( m );
               }
               return result;
            } )
            );
      }

   }
}
#endif