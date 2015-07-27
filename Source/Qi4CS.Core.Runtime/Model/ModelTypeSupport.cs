/*
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.Runtime.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Core.Runtime.Model
{
   public interface CompositeModelTypeModelScopeSupport
   {
      CompositeModelTypeAssemblyScopeSupport AssemblyScopeSupport { get; }

      CompositeCodeGenerationInfo CodeGenerationInfo { get; }

      CompositeValidationResultMutable ValidateModel( CompositeModel compositeModel );

      void ValidateModelsInApplication( ApplicationModel<ApplicationSPI> appModel, ApplicationValidationResultMutable appValidation );

      PublicCompositeTypeGenerationResult LoadTypes(
         CompositeModel compositeModel,
         IList<CompositeTypesAttribute> typeAttributes
         );

      CompositeModelTypeInstanceScopeSupport CreateInstanceScopeSupport();

      CompositeModel NewCompositeModel( ApplicationModel<ApplicationSPI> appModel, CompositeAssemblyInfo compositeInfo, Func<Int32, Object, Attribute, Attribute> attributeTransformer, String architectureContainerID );

#if QI4CS_SDK
      CompositeModelTypeCodeGenerator NewCodeGenerator( Boolean isWP8Emit, CILAssemblyManipulator.Logical.CILReflectionContext reflectionContext );
#endif

   }

   public abstract partial class AbstractModelTypeModelScopeSupportBase : CompositeModelTypeModelScopeSupport
   {
      private readonly CompositeModelTypeAssemblyScopeSupport _assemblyScopeSupport;
      private readonly CompositeCodeGenerationInfo _codeGenerationInfo;

      public AbstractModelTypeModelScopeSupportBase( CompositeModelTypeAssemblyScopeSupport assemblyScopeSupport, CompositeCodeGenerationInfo codeGenerationInfo )
      {
         ArgumentValidator.ValidateNotNull( "Assembly-scope support", assemblyScopeSupport );
         ArgumentValidator.ValidateNotNull( "Composite code generation info", codeGenerationInfo );

         this._assemblyScopeSupport = assemblyScopeSupport;
         this._codeGenerationInfo = codeGenerationInfo;
      }

      #region CompositeModelTypeModelScopeSupport Members

      public CompositeModelTypeAssemblyScopeSupport AssemblyScopeSupport
      {
         get
         {
            return this._assemblyScopeSupport;
         }
      }

      public abstract CompositeModelTypeInstanceScopeSupport CreateInstanceScopeSupport();

      public CompositeCodeGenerationInfo CodeGenerationInfo
      {
         get
         {
            return this._codeGenerationInfo;
         }
      }

      public CompositeValidationResultMutable ValidateModel( CompositeModel compositeModel )
      {
         return this.PerformValidationWithoutTypeGeneration( compositeModel );
      }

      public virtual PublicCompositeTypeGenerationResult LoadTypes(
         CompositeModel compositeModel,
         IList<CompositeTypesAttribute> typeAttributes
         )
      {
         return new PublicCompositeTypeGenerationResultImpl(
            compositeModel.ApplicationModel.CollectionsFactory,
            typeAttributes
            );
      }

      public virtual void ValidateModelsInApplication( ApplicationModel<ApplicationSPI> appModel, ApplicationValidationResultMutable appValidation )
      {
         // Do nothing by default
      }

      #endregion

      protected abstract void PostValidateModel( CompositeModel compositeModel, CompositeValidationResult validationResult );

      protected virtual CompositeModelValidator CreateValidator()
      {
         return new CompositeModelValidator();
      }

      protected virtual CompositeValidationResultMutable PerformValidationWithoutTypeGeneration( CompositeModel compositeModel )
      {
         var state = new CompositeValidationResultState( compositeModel );
         var result = new CompositeValidationResultMutable( state, new CompositeValidationResultImmutable( state ) );

         this.CreateValidator().ValidateComposite( result, compositeModel );
         this.PostValidateModel( compositeModel, result );
         return result;
      }

#if QI4CS_SDK

      public abstract CompositeModelTypeCodeGenerator NewCodeGenerator( Boolean isSilverlight, CILAssemblyManipulator.Logical.CILReflectionContext reflectionContext );

#endif
   }

   public abstract class AbstractPlainCompositeModelTypeModelScopeSupport : AbstractModelTypeModelScopeSupportBase
   {
      protected AbstractPlainCompositeModelTypeModelScopeSupport( AbstractPlainCompositeModelTypeAssemblyScopeSupport assemblyScopeSupport )
         : base( assemblyScopeSupport, new DefaultCompositeCodeGenerationInfo( typeof( CompositeInstanceImpl ) ) )
      {

      }

      protected override void PostProcessModel( CompositeModelMutable model, Assembling.CompositeAssemblyInfo info, String architectureContainerID )
      {
         // TODO Search for overridden Equals-method?
      }

      protected override void PostValidateModel( SPI.Model.CompositeModel compositeModel, CompositeValidationResult validationResult )
      {
         // Nothing to do.
      }

#if QI4CS_SDK
      public override CompositeModelTypeCodeGenerator NewCodeGenerator( Boolean isSilverlight, CILAssemblyManipulator.Logical.CILReflectionContext reflectionContext )
      {
         return new PlainCompositeModelTypeCodeGenerator( this.CodeGenerationInfo, isSilverlight, reflectionContext );
      }
#endif
   }

   public abstract class AbstractServiceModelTypeModelScopeSupport : AbstractModelTypeModelScopeSupportBase
   {
      protected AbstractServiceModelTypeModelScopeSupport( AbstractServiceModelTypeAssemblyScopeSupport assemblyScopeSupport )
         : base( assemblyScopeSupport, new DefaultCompositeCodeGenerationInfo( typeof( ServiceCompositeInstanceImpl ) ) )
      {

      }

      public override void ValidateModelsInApplication( ApplicationModel<ApplicationSPI> appModel, ApplicationValidationResultMutable appValidation )
      {
         var sModels = appModel.CompositeModels.Values.OfType<ServiceCompositeModel>().ToArray();
         if ( sModels.Any( sModel => sModel.ServiceID == null ) )
         {
            appValidation.InternalValidationErrors.Add( ValidationErrorFactory.NewInternalError( "The following service composite models had their service ID as null: " + String.Join( ", ", sModels.Where( sModel => sModel.ServiceID == null ) ), null ) );
         }
         else
         {
            var grouping = sModels.GroupBy( sModel => sModel.ServiceID ).Where( g => g.Count() > 1 ).ToArray();
            if ( grouping.Length > 0 )
            {
               appValidation.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The following services have same service ID: " + String.Join( "; ", grouping.Select( g => String.Join( ", ", g ) ) ), appModel ) );
            }
         }
      }

      protected override void CreateCompositeModelObjects( CompositeAssemblyInfo info, CollectionsFactory factory, out CompositeModelState state, out CompositeModelImmutable resultImmutable, out CompositeModelMutable result )
      {
         state = new ServiceCompositeModelState( factory );
         resultImmutable = new ServiceCompositeModelImmutable( (ServiceCompositeModelState) state );
         result = new ServiceCompositeModelMutable( (ServiceCompositeModelState) state, (ServiceCompositeModel) resultImmutable );
      }

      protected override void PostProcessModel( CompositeModelMutable model, CompositeAssemblyInfo info, String architectureContainerID )
      {
         ServiceCompositeAssemblyInfo sInfo = (ServiceCompositeAssemblyInfo) info;
         ServiceCompositeModelMutable sModel = (ServiceCompositeModelMutable) model;
         sModel.ServiceID = sInfo.ServiceID ?? ( "[" + architectureContainerID + String.Join( ", ", model.IQ.PublicTypes.Select( pType => QualifiedName.GetTypeName( pType ) ) ) + "]" );
         sModel.ActivateWithApplication = sInfo.ActivateWithApplication;
      }

      protected override void PostValidateModel( CompositeModel compositeModel, CompositeValidationResult validationResult )
      {
         ServiceCompositeModel sModel = (ServiceCompositeModel) compositeModel;
         if ( sModel.PublicTypes.Any( pType => pType.ContainsGenericParameters() ) )
         {
            validationResult.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "All the generic arguments of service composite's all public types must be closed.", compositeModel ) );
         }
      }

#if QI4CS_SDK
      public override CompositeModelTypeCodeGenerator NewCodeGenerator( Boolean isSilverlight, CILAssemblyManipulator.Logical.CILReflectionContext reflectionContext )
      {
         return new ServiceModelTypeCodeGenerator( this.CodeGenerationInfo, isSilverlight, reflectionContext );
      }
#endif
   }

   //   public abstract class AbstractValueModelTypeModelScopeSupport : AbstractModelTypeModelScopeSupportBase
   //   {
   //      protected AbstractValueModelTypeModelScopeSupport( AbstractValueModelTypeAssemblyScopeSupport assemblyScopeSupport )
   //         : base( assemblyScopeSupport, new DefaultCompositeCodeGenerationInfo( typeof( CompositeInstanceImpl ) ) )
   //      {

   //      }

   //      protected override void PostValidateModel( CompositeModel compositeModel, CompositeValidationResult validationResult )
   //      {
   //         foreach ( var violatingMethod in compositeModel.Methods.Where( method => method.PropertyModel == null || !method.Mixin.NativeInfo.DeclaringType.Equals( compositeModel.ApplicationModel.GenericPropertyMixinType ) ) )
   //         {
   //            validationResult.AddStructureError( new StructureValidationErrorImpl( compositeModel, violatingMethod, "Value composites must have only auto-generated property setters and getters." ) );
   //         }
   //      }

   //      public override void PostProcessModel( CompositeModelMutable model, Assembling.CompositeAssemblyInfo info )
   //      {
   //         foreach ( var pModel in model.Methods.CQ.Select( cMethod => cMethod.PropertyModel ).Distinct() )
   //         {
   //            pModel.PropertyIsImmutable = true;
   //         }

   //         // TODO override .Equals -method to call Object.ReferenceEquals(...), once value composite cache is working.
   //      }

   //#if QI4CS_SDK

   //      public override CompositeModelTypeCodeGenerator NewCodeGenerator( CILAssemblyManipulator.API.CILReflectionContext reflectionContext )
   //      {
   //         return new ValueModelTypeCodeGenerator( reflectionContext );
   //      }

   //#endif
   //   }
}
