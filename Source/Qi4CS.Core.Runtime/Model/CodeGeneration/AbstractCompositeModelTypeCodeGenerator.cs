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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CILAssemblyManipulator.API;
using CollectionsWithRoles.API;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public abstract partial class AbstractCompositeModelTypeCodeGenerator : CompositeModelTypeCodeGenerator
   {
#region CompositeModelTypeCodeGenerator Members

      public void EmitTypesForModel( CompositeModel model, CompositeTypeModel typeModel, System.Reflection.Assembly assemblyBeingProcessed, CILModule mob, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo )
      {
         if ( model.PublicTypes.Any( pType => pType.Assembly.Equals( assemblyBeingProcessed ) )
            || typeModel.FragmentTypeInfos.Keys.Any( type => type.Assembly.Equals( assemblyBeingProcessed ) )
            || typeModel.PrivateCompositeTypeInfos.Keys.Any( type => type.Assembly.Equals( assemblyBeingProcessed ) )
            || typeModel.ConcernInvocationTypeInfos.Keys.Any( type => type.Assembly.Equals( assemblyBeingProcessed ) )
            || typeModel.SideEffectInvocationTypeInfos.Keys.Any( type => type.Assembly.Equals( assemblyBeingProcessed ) )
            )
         {
            var collectionsFactory = model.ApplicationModel.CollectionsFactory;

            // Define public composite type
            var publicCompositeTypeGenInfo = this.EmitPublicCompositeType( codeGenerationInfo, model, typeModel, assemblyBeingProcessed, mob, collectionsFactory, emittingInfo );
            // Define all private composite types.
            foreach ( var pcInfo in this.GetBindings( typeModel.PrivateCompositeTypeInfos.Values, assemblyBeingProcessed ) )
            {
               foreach ( var bindings in this.GetForEachBinding( pcInfo ) )
               {
                  this.EmitPrivateCompositeType( codeGenerationInfo, model, assemblyBeingProcessed, typeModel, pcInfo, bindings, publicCompositeTypeGenInfo.Builder, emittingInfo, collectionsFactory );
               }
            }

            foreach ( var fInfo in this.GetBindings( typeModel.FragmentTypeInfos.Values, assemblyBeingProcessed ) )
            {
               foreach ( var bindings in this.GetForEachBinding( fInfo ) )
               {
                  var fID = emittingInfo.NewFragmentID( model );
                  var fragmentGenerationInfo = this.EmitFragmentType( codeGenerationInfo, model, assemblyBeingProcessed, typeModel, fInfo, bindings, publicCompositeTypeGenInfo.Builder, fID, emittingInfo, collectionsFactory );
                  this.EmitCreateFragmentMethod( codeGenerationInfo, model, publicCompositeTypeGenInfo, fID, fragmentGenerationInfo, emittingInfo );
               }
            }
         }
      }

      public void EmitFragmentMethods( CompositeModel model, CompositeTypeModel typeModel, System.Reflection.Assembly assemblyBeingProcessed, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo )
      {
         var publicCompositeTypeGenInfo = emittingInfo.GetPublicComposite( model, assemblyBeingProcessed );
         if ( publicCompositeTypeGenInfo != null )
         {
            foreach ( var binding in this.GetBindings( typeModel.FragmentTypeInfos.Values, assemblyBeingProcessed ) )
            {
               foreach ( var fGenInfo in emittingInfo.FragmentTypeGenerationInfos[binding].Item1 )
               {
                  this.EmitFragmentMethods( codeGenerationInfo, model, publicCompositeTypeGenInfo, fGenInfo, emittingInfo.AllCompositeGenerationInfos );
               }
            }
         }
      }

      public void EmitCompositeMethosAndInvocationInfos( CompositeModel model, CompositeTypeModel typeModel, System.Reflection.Assembly assemblyBeingProcessed, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo )
      {
         var publicCompositeTypeGenInfo = emittingInfo.GetPublicComposite( model, assemblyBeingProcessed );
         if ( publicCompositeTypeGenInfo != null )
         {
            var collectionsFactory = model.ApplicationModel.CollectionsFactory;

            var compositeTypeGenerationInfos = emittingInfo.AllCompositeGenerationInfos.Where( info => info.Equals( publicCompositeTypeGenInfo ) || publicCompositeTypeGenInfo.Builder.Equals( info.Builder.DeclaringType ) );
            var fragmentTypeGenerationInfos = emittingInfo.FragmentTypeGenerationInfos.Values.SelectMany( info => info.Item1 );

            var genericEventMixinType = model.ApplicationModel.GenericEventMixinType;
            var genericPropertyMixinType = model.ApplicationModel.GenericPropertyMixinType;

            foreach ( var cInfo in compositeTypeGenerationInfos )
            {
               Boolean actuallyEmit = false, actuallyEmitSet = false;
               foreach ( var cMethod in model.Methods )
               {
                  if ( cInfo.Parents.ContainsKey( cMethod.NativeInfo.DeclaringType.NewWrapperAsType( this.ctx ) ) )
                  {
                     if ( !actuallyEmitSet )
                     {
                        actuallyEmit = emittingInfo.TryAddTypeWithCompositeMethods( cInfo );
                        actuallyEmitSet = true;
                     }
                     if ( actuallyEmit )
                     {
                        // Define a composite method, utilizing required constraints, concerns, mixins, and side-effects
                        var cMethodGen = this.EmitCompositeMethod(
                           codeGenerationInfo,
                           fragmentTypeGenerationInfos,
                           publicCompositeTypeGenInfo,
                           cInfo,
                           emittingInfo,
                           cMethod,
                           typeModel
                           );
                        if ( cMethod.EventModel != null )
                        {
                           this.EmitEventRelatedThings( cInfo, cMethodGen, cMethod.EventModel, genericEventMixinType );
                        }
                        if ( cMethod.PropertyModel != null )
                        {
                           this.EmitPropertyRelatedThings( codeGenerationInfo, publicCompositeTypeGenInfo, cInfo, cMethodGen, cMethod.PropertyModel, genericPropertyMixinType );
                        }
                     }
                  }
               }
            }

            foreach ( var bindingInfo in this.GetBindings( typeModel.ConcernInvocationTypeInfos.Values, assemblyBeingProcessed ) )
            {
               foreach ( var bindings in this.GetForEachBinding( bindingInfo ) )
               {
                  this.EmitConcernInvocationType( codeGenerationInfo, model, assemblyBeingProcessed, typeModel, bindingInfo, bindings, emittingInfo, publicCompositeTypeGenInfo, fragmentTypeGenerationInfos, collectionsFactory );
               }
            }

            foreach ( var bindingInfo in this.GetBindings( typeModel.SideEffectInvocationTypeInfos.Values, assemblyBeingProcessed ) )
            {
               foreach ( var bindings in this.GetForEachBinding( bindingInfo ) )
               {
                  this.EmitSideEffectInvocationType( codeGenerationInfo, model, assemblyBeingProcessed, typeModel, bindingInfo, publicCompositeTypeGenInfo.Builder, bindings, emittingInfo, collectionsFactory );
               }
            }
         }
      }

      public void EmitCompositeExtraMethods( CompositeModel model, CompositeTypeModel typeModel, System.Reflection.Assembly assemblyBeingProcessed, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo )
      {
         var publicCompositeTypeGenInfo = emittingInfo.GetPublicComposite( model, assemblyBeingProcessed );
         if ( publicCompositeTypeGenInfo != null )
         {
            var fragmentTypeGenerationInfos = emittingInfo.FragmentTypeGenerationInfos.Values.SelectMany( info => info.Item1 );
            foreach ( var cInfo in emittingInfo.AllCompositeGenerationInfos.Where( info => info.Equals( publicCompositeTypeGenInfo ) || publicCompositeTypeGenInfo.Builder.Equals( info.Builder.DeclaringType ) ) )
            {
               if ( emittingInfo.TryAddTypeWithExtraMethods( cInfo ) )
               {

                  this.EmitCheckStateMethod( codeGenerationInfo, model, cInfo, CHECK_STATE_METHOD_NAME );
                  this.EmitPrePrototypeMethod( codeGenerationInfo, model, cInfo, SET_DEFAULTS_METHOD_NAME );

                  // Emit equals and hashcode -implementations, if necessary
                  if ( Object.ReferenceEquals( cInfo, publicCompositeTypeGenInfo ) )
                  {
                     this.EmitPublicCompositeEquals( codeGenerationInfo, model, typeModel, publicCompositeTypeGenInfo, cInfo, emittingInfo, fragmentTypeGenerationInfos );
                     this.EmitPublicCompositeHashCode( codeGenerationInfo, model, typeModel, publicCompositeTypeGenInfo, cInfo, emittingInfo, fragmentTypeGenerationInfos );
                  }
                  else
                  {
                     this.EmitPrivateCompositeEquals( codeGenerationInfo, model, typeModel, publicCompositeTypeGenInfo, cInfo, emittingInfo, fragmentTypeGenerationInfos );
                     this.EmitPrivateCompositeHashCode( codeGenerationInfo, model, typeModel, publicCompositeTypeGenInfo, cInfo, emittingInfo, fragmentTypeGenerationInfos );
                  }
               }
            }
         }
      }

      public void EmitCompositeConstructors( CompositeModel model, CompositeTypeModel typeModel, System.Reflection.Assembly assemblyBeingProcessed, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo )
      {
         var publicCompositeTypeGenInfo = emittingInfo.GetPublicComposite( model, assemblyBeingProcessed );
         if ( publicCompositeTypeGenInfo != null )
         {
            foreach ( var cInfo in emittingInfo.AllCompositeGenerationInfos.Where( info => info.Equals( publicCompositeTypeGenInfo ) || publicCompositeTypeGenInfo.Builder.Equals( info.Builder.DeclaringType ) ) )
            {
               if ( emittingInfo.TryAddTypeWithCtor( cInfo ) )
               {
                  this.EmitCompositeConstructor( codeGenerationInfo, model, typeModel, emittingInfo, cInfo, emittingInfo.IsMainCompositeGenerationInfo( cInfo, MAIN_PUBLIC_COMPOSITE_TYPE_ATTRIBUTE_CTOR.DeclaringType ) );
               }
            }
         }
      }

      public void EmitCompositeFactory( CompositeModel model, System.Reflection.Assembly assemblyBeingProcessed, CILModule module, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo )
      {
         var publicCompositeTypeGenInfo = emittingInfo.GetPublicComposite( model, assemblyBeingProcessed );
         if ( publicCompositeTypeGenInfo != null && emittingInfo.IsMainCompositeGenerationInfo( publicCompositeTypeGenInfo, assemblyBeingProcessed ) )
         {
            var typeIDDic = emittingInfo.GetGenerationInfosByTypeID( model );

            var factory = module.AddType( publicCompositeTypeGenInfo.Builder.Name + codeGenerationInfo.CompositeFactorySuffix, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed );

            // Define default constructor
            factory.AddDefaultConstructor( MethodAttributes.Public );

            // Add interface to inheritance hierarchy
            factory.AddDeclaredInterfaces( COMPOSITE_FACTORY_TYPE );

            // Explicitly implement interface
            var method = TypeGenerationUtils.ImplementMethodForEmitting( factory, null, t => t, COMPOSITE_FACTORY_METHOD, COMPOSITE_FACTORY_TYPE.Name + "." + COMPOSITE_FACTORY_METHOD.Name, MethodAttributesUtils.EXPLICIT_IMPLEMENTATION_ATTRIBUTES ).Item1;
            var methodGenInfo = new MethodGenerationInfoImpl( method );
            // Generate code for it
            var il = method.MethodIL;

            var resultB = il.DeclareLocal( method.GetReturnType() );
            il.EmitLoadArg( 1 )
              .EmitLoadInt32( 1 )
              .EmitSubtract()
              .EmitSwitch(
                  typeIDDic.Count,
                  ( il2, labels, defaultLabel, switchEndLabel ) =>
                  {
                     for ( var i = 0; i < labels.Length; ++i )
                     {
                        var label = labels[i];
                        il2.MarkLabel( label );
                        var curTypeID = i + 1;
                        if ( publicCompositeTypeGenInfo.Builder.GenericArguments.Any() )
                        {
                           // typeof(<type>).MakeGenericType(gArgs).GetConstructors()[0].Invoke(args);
                           il2.EmitReflectionObjectOf( typeIDDic[curTypeID].Builder )
                              .EmitLoadArg( 2 )
                              .EmitCall( MAKE_GENERIC_TYPE_METHOD )
                              .EmitCall( GET_CONSTRUCTORS_METHOD )
                              .EmitLoadInt32( 0 )
                              .EmitLoadElement( GET_CONSTRUCTORS_METHOD.GetReturnType().GetElementType() )
                              .EmitLoadArg( 3 )
                              .EmitCall( CONSTRUCTOR_INVOKE_METHOD )
                              .EmitStoreLocal( resultB );
                        }
                        else
                        {
                           var ctor = typeIDDic[curTypeID].Builder.Constructors[0];

                           // Prepare by-ref parameters
                           foreach ( var param in ctor.Parameters )
                           {
                              var paramType = param.ParameterType;
                              var isByRef = paramType.IsByRef();
                              if ( isByRef && !paramType.Equals( OBJECT_TYPE ) )
                              {
                                 var elType = paramType.GetElementType();
                                 var local = methodGenInfo.GetOrCreateLocal( GenerateLocalInfoInCompositeFactory( curTypeID, param ), elType );
                                 il2.EmitLoadDefault( elType, elTypeInner => local )
                                    .EmitStoreLocal( local );
                              }
                           }

                           // Load and cast all required parameters
                           foreach ( var param in ctor.Parameters )
                           {
                              var paramType = param.ParameterType;
                              var isByRef = paramType.IsByRef();
                              if ( isByRef && !paramType.Equals( OBJECT_TYPE ) )
                              {
                                 il2.EmitLoadLocalAddress( methodGenInfo.GetLocalRaw( GenerateLocalInfoInCompositeFactory( curTypeID, param ) ) );
                              }
                              else
                              {
                                 il2.EmitLoadArg( 3 )
                                    .EmitLoadInt32( param.Position );
                                 if ( isByRef )
                                 {
                                    il2.EmitLoadElementAddress( OBJECT_TYPE );
                                 }
                                 else
                                 {
                                    il2.EmitLoadElement( OBJECT_TYPE );
                                 }
                                 il2.EmitCastToType( OBJECT_TYPE, paramType );
                              }
                           }
                           il2.EmitNewObject( ctor )
                              .EmitStoreLocal( resultB );

                           // Post-process by-ref parameters
                           foreach ( var param in ctor.Parameters.Skip( 1 ) )
                           {
                              var paramType = param.ParameterType;
                              var isByRef = paramType.IsByRef();
                              if ( isByRef && !paramType.Equals( OBJECT_TYPE ) )
                              {
                                 var local = methodGenInfo.GetLocalRaw( GenerateLocalInfoInCompositeFactory( curTypeID, param ) );
                                 il2.EmitLoadArg( 3 )
                                    .EmitLoadInt32( param.Position )
                                    .EmitLoadLocal( local )
                                    .EmitCastToType( local.LocalType, param.ParameterType )
                                    .EmitStoreElement( OBJECT_TYPE );
                              }
                           }
                        }
                        // Return result
                        il2.EmitLoadLocal( resultB )
                           .EmitReturn();
                     }
                  },
                  ( il2, defaultLabel ) =>
                  {
                     il2.EmitLoadString( "Invalid type id " )
                        .EmitLoadArg( 1 )
                        .EmitCastToType( INT32_TYPE, OBJECT_TYPE )
                        .EmitCall( STRING_CONCAT_METHOD_2 )
                        .EmitThrowNewException( ARGUMENT_EXCEPTION_STRING_CTOR );
                  } );

            // Make it explicit implementation
            method.AddOverriddenMethods( COMPOSITE_FACTORY_METHOD );
         }
      }

      #endregion

      protected static String GenerateLocalInfoInCompositeFactory( Int32 typeID, CILParameter param )
      {
         return "t" + typeID + "p" + param.Position;
      }

      protected IEnumerable<TypeBindingInformation> GetBindings( IEnumerable<TypeBindingInformation> all, System.Reflection.Assembly assemblyBeingProcessed )
      {
         return all.Where( binding => binding.NativeInfo.Assembly.Equals( assemblyBeingProcessed ) );
      }

      protected IEnumerable<ListQuery<AbstractGenericTypeBinding>> GetForEachBinding( TypeBindingInformation bindingInfo )
      {
         return bindingInfo.GenericBindings.Any() ? bindingInfo.GenericBindings : Enumerable.Repeat<ListQuery<AbstractGenericTypeBinding>>( null, 1 );
      }

      protected virtual String GetGeneratedClassName( String prefix, Int32 modelID )
      {
         return prefix + modelID;
      }

      protected virtual CompositeMethodGenerationInfo ImplementMethodForEmitting(
         AbstractTypeGenerationInfoForComposites thisGenInfo,
         CILMethod methodToCopy,
         String newName,
         MethodAttributes? newAttributes,
         Boolean retainSpecialName // Ignored if new attributes is null
         )
      {
         return this.ImplementMethodForEmitting(
            thisGenInfo,
            parentFromModel => thisGenInfo.Parents[parentFromModel],
            methodToCopy,
            newName,
            newAttributes,
            retainSpecialName
            );
      }

      protected virtual CompositeMethodGenerationInfo ImplementMethodForEmitting(
         AbstractTypeGenerationInfoForComposites thisGenInfo,
         Func<CILType, CILType> parentFunc,
         CILMethod methodToCopy,
         String newName,
         MethodAttributes? newAttributes,
         Boolean retainSpecialName // Ignored if new attributes is null
         )
      {
         var result = TypeGenerationUtils.ImplementMethodForEmitting(
            thisGenInfo.Builder,
            thisGenInfo.GenericArguments,
            parentFunc,
            methodToCopy,
            newName,
            newAttributes.HasValue ? ( retainSpecialName && methodToCopy.Attributes.IsSpecialName() ? ( newAttributes.Value | MethodAttributes.SpecialName ) : newAttributes.Value ) : (MethodAttributes) methodToCopy.Attributes
            );
         return new CompositeMethodGenerationInfoImpl( result.Item1, methodToCopy, result.Item2 );
      }

      protected virtual CompositeMethodGenerationInfo EmitCompositeMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentTypeGenerationInfos,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         CompositeMethodModel compositeMethod,
         CompositeTypeModel typeModel
         )
      {
         var method = compositeMethod.NativeInfo.NewWrapper( this.ctx );
         var useExplicitImplementation = thisGenInfo.Builder.BaseType.Equals( OBJECT_TYPE );
         var methodGenInfo = this.ImplementMethodForEmitting(
            thisGenInfo,
            method,
            useExplicitImplementation ? COMPOSITE_METHOD_PREFIX + compositeMethod.MethodIndex : method.Name,
            useExplicitImplementation ? MethodAttributesUtils.EXPLICIT_IMPLEMENTATION_ATTRIBUTES : NORMAL_IMPLEMENTATION_ATTRIBUTES,
            true
           );

         var actualMethod = methodGenInfo.OverriddenMethod;
         thisGenInfo.NormalMethodBuilders.Add( method, methodGenInfo );
         var mb = methodGenInfo.Builder;

#if DEBUG
         //mb.SetCustomAttribute( new CustomAttributeBuilder( type-of( DebuggableAttribute ).GetConstructor( Type.EmptyTypes ), EMPTY_OBJECTS ) );
#endif

         var il = methodGenInfo.IL;

         // Local variable instance
         var cInstanceB = methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );

         // Local variable result - only for non-void methods
         LocalBuilder resultB = null;
         if ( !VOID_TYPE.Equals( methodGenInfo.Builder.GetReturnType() ) )
         {
            resultB = methodGenInfo.GetOrCreateLocal( LB_RESULT, methodGenInfo.ReturnType );
         }

         // CompositeInstance instance = this._instance;
         il.EmitLoadThisField( thisGenInfo.CompositeField );
         il.EmitStoreLocal( cInstanceB );

         // Throw if application is not active
         this.EmitThrowIfApplicationNotActive( methodGenInfo );

         var parametersNeedFragmentSetup = compositeMethod.Parameters.Any( paramModel => paramModel.IsFragmentDependant() );
         var hasGenericFragmentMethods = this.HasGenericFragmentMethods( compositeMethod );
         var hasOnInvocationInjections = this.HasOnInvocationInjections( compositeMethod );

         il.EmitTryFinally(
            ( il2 ) => // <call composite method with the help of constraints, concerns, mixins, side-effects>
            {
               if ( hasGenericFragmentMethods || hasOnInvocationInjections )
               {
                  LocalBuilder compositeMethodModelB = null;
                  if ( hasOnInvocationInjections )
                  {
                     this.InitializeComplexMethodModelLocalIfNecessary( compositeMethod, methodGenInfo, out compositeMethodModelB );
                  }

                  // instance.InvocationInfo = new InvocationInfoImpl(<method-of-this-composite>, <next fragment model>);
                  il2
                     .EmitLoadLocal( cInstanceB )
                     .EmitReflectionObjectOf( actualMethod.MakeGenericMethod( methodGenInfo.GenericArguments.ToArray() ) );
                  if ( hasOnInvocationInjections )
                  {
                     il2.EmitLoadLocal( compositeMethodModelB );
                     if ( compositeMethod.Concerns.Any() )
                     {
                        il2
                           .EmitCall( CONCERN_MODELS_GETTER )
                           .EmitLoadInt32( 0 )
                           .EmitCall( CONCERN_MODELS_INDEXER );
                     }
                     else
                     {
                        il2.EmitCall( MIXIN_MODEL_GETTER );
                     }
                  }
                  else
                  {
                     il2.EmitLoadNull();
                  }
                  il2.EmitNewObject( INVOCATION_INFO_CREATOR_CTOR )
                    .EmitCall( INVOCATION_INFO_SETTER );
               }
               this.EmitCompositeMethodBody(
                  codeGenerationInfo,
                  compositeMethod,
                  typeModel,
                  fragmentTypeGenerationInfos,
                  publicCompositeGenInfo,
                  thisGenInfo,
                  emittingInfo,
                  methodGenInfo
               );
            },
            ( parametersNeedFragmentSetup || hasGenericFragmentMethods || hasOnInvocationInjections ) ? ( il2 ) =>
            {
               this.ForEachParameterWithInjection<ConcernForAttribute>(
                  compositeMethod,
                  paramModel =>
                  {
                     var position = paramModel.NativeInfo.Position;
                     var pType = methodGenInfo.Parameters[position].ParameterType;
                     // cInstance.ReturnConcernInvocation(<type>, <arg>);
                     il2
                        .EmitLoadLocal( cInstanceB )
                        .EmitReflectionObjectOf( pType )
                        .EmitLoadArgumentToPassAsParameter( methodGenInfo.Parameters[position] )
                        .EmitCastToType( pType, FRAGMENT_DEPENDANT_TYPE )
                        .EmitCall( RETURN_CONCERN_INVOCATION_METHOD );
                  }
                  );
               this.ForEachParameterWithInjection<SideEffectForAttribute>(
                  compositeMethod,
                  paramModel =>
                  {
                     var position = paramModel.NativeInfo.Position;
                     var pType = methodGenInfo.Parameters[position].ParameterType;
                     // cInstance.ReturnSideEffectInvocation(<type>, <arg>);
                     il2
                        .EmitLoadLocal( cInstanceB )
                        .EmitReflectionObjectOf( pType )
                        .EmitLoadArgumentToPassAsParameter( methodGenInfo.Parameters[position] )
                        .EmitCastToType( pType, FRAGMENT_DEPENDANT_TYPE )
                        .EmitCall( RETURN_SIDE_EFFECT_INVOCATION_METHOD );
                  }
                  );
               if ( hasGenericFragmentMethods || hasOnInvocationInjections )
               {
                  // instance.InvocationInfo = null;
                  il2
                     .EmitLoadLocal( cInstanceB )
                     .EmitLoadNull()
                     .EmitCall( INVOCATION_INFO_SETTER );
               }
            } : (Action<MethodIL>) null
            );

         if ( resultB != null )
         {
            il.EmitLoadLocal( resultB );
         }
         il.EmitReturn();
         if ( useExplicitImplementation )
         {
            methodGenInfo.Builder.AddOverriddenMethods( actualMethod );
         }

         return methodGenInfo;
      }

      protected virtual void EmitThrowIfApplicationNotActive(
         CompositeMethodGenerationInfo thisMethodGenInfo
         )
      {
         var il = thisMethodGenInfo.IL;
         var cInstanceB = thisMethodGenInfo.GetLocalOrThrow( LB_C_INSTANCE );

         // if (cInstance.Application.IsPassive)
         // then
         // throw not active exception
         // endif
         il.EmitIf(
            ( il2, endIfLabel ) =>
            {
               il2
                  .EmitLoadLocal( cInstanceB )
                  .EmitCall( STRUCTURE_OWNER_GETTER_METHOD )
                  .EmitCall( APPLICATION_GETTER_METHOD )
                  .EmitCall( APPLICATION_IS_PASSIVE_GETTER_METHOD )
                  .EmitBranch( BranchType.IF_FALSE, endIfLabel );
            },
            ( il2, endIfLabel ) =>
            {
               il2.EmitThrowNewException( APPLICATION_NOT_ACTIVE_EXCEPTION_CONSTRUCTOR );
            }
            );
      }

      protected virtual void EmitThrowIfApplicationNotActiveWithoutLocalVariable(
         CompositeTypeGenerationInfo thisGenerationInfo,
         MethodIL il
         )
      {
         // if (this._instance.Application.IsPassive)
         // then
         // throw not active exception
         // endif
         il.EmitIf(
            ( il2, endIfLabel ) =>
            {
               il2
                  .EmitLoadThisField( thisGenerationInfo.CompositeField )
                  .EmitCall( STRUCTURE_OWNER_GETTER_METHOD )
                  .EmitCall( APPLICATION_GETTER_METHOD )
                  .EmitCall( APPLICATION_IS_PASSIVE_GETTER_METHOD )
                  .EmitBranch( BranchType.IF_FALSE, endIfLabel );
            },
            ( il2, endIfLabel ) =>
            {
               il2.EmitThrowNewException( APPLICATION_NOT_ACTIVE_EXCEPTION_CONSTRUCTOR );
            }
            );
      }

      protected virtual void InitializeComplexMethodModelLocalIfNecessary(
         CompositeMethodModel compositeMethodModel,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         out LocalBuilder compositeMethodModelB
         )
      {
         if ( !thisMethodGenInfo.TryGetLocal( LB_COMPOSITE_METHOD_MODEL, out compositeMethodModelB ) )
         {
            compositeMethodModelB = thisMethodGenInfo.GetOrCreateLocal( LB_COMPOSITE_METHOD_MODEL );
            this.InitializeComplexMethodModelLocal( thisMethodGenInfo, compositeMethodModel, compositeMethodModelB );
         }
      }

      protected virtual void InitializeComplexMethodModelLocal(
         CompositeMethodGenerationInfo methodGenInfo,
         CompositeMethodModel compositeMethodModel,
         LocalBuilder compositeMethodModelB
         )
      {
         // CompositeMethodModel compositeMethodModel = instance.ModelInfo.Model.Methods[<idx>];
         methodGenInfo.IL
            .EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_C_INSTANCE ) )
            .EmitCall( MODEL_INFO_GETTER )
            .EmitCall( MODEL_GETTER )
            .EmitCall( C_METHODS_GETTER )
            .EmitLoadInt32( compositeMethodModel.MethodIndex )
            .EmitCall( ChangeQueryItemGetterDeclaringTypeGArgs( COMPOSITE_METHOD_MODEL_TYPE ) )
            .EmitStoreLocal( compositeMethodModelB );
      }

      protected CILMethod ChangeQueryItemGetterDeclaringTypeGArgs( CILType type )
      {
         return LIST_QUERY_ITEM_GETTER.ChangeDeclaringType( type );
      }

      protected virtual void EmitCompositeMethodBody(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeMethodModel compositeMethodModel,
         CompositeTypeModel typeModel,
         IEnumerable<FragmentTypeGenerationInfo> fragmentTypeGenerationInfos,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         CompositeMethodGenerationInfo methodGenInfo
         )
      {
         var il = methodGenInfo.IL;
         var hasInConstraints = compositeMethodModel.Parameters.Where( pModel => !pModel.NativeInfo.IsOut ).Any( param => this.HasConstraints( param ) );
         var hasOutConstraints = compositeMethodModel.Parameters.Where( pModel => pModel.NativeInfo.IsOut ).Any( ParamArrayAttribute => this.HasConstraints( ParamArrayAttribute ) ) || this.HasConstraints( compositeMethodModel.Result );
         var hasSideEffects = compositeMethodModel.SideEffects.Any();
         var hasResult = !VOID_TYPE.Equals( compositeMethodModel.Result.NativeInfo.ParameterType );

         // TODO REFACTOR are these two really necessary?
         methodGenInfo.GetOrCreateLocal( LB_F_INSTANCE_POOL );
         methodGenInfo.GetOrCreateLocal( LB_F_INSTANCE );

         this.EmitAfterCompositeMethodBodyBegan(
            fragmentTypeGenerationInfos,
            thisGenInfo,
            methodGenInfo,
            compositeMethodModel
            );

         LocalBuilder exceptionB = null;
         if ( hasSideEffects )
         {
            exceptionB = methodGenInfo.GetOrCreateLocal( LB_EXCEPTION );
            // exception = null;
            il
               .EmitLoadNull()
               .EmitStoreLocal( exceptionB );
         }

         il.EmitTryCatchFinally(
            EXCEPTION_TYPE,
            ( il2 ) =>
            {
               if ( compositeMethodModel.PropertyModel == null || !compositeMethodModel.PropertyModel.IsPartOfCompositeState() )
               {
                  // Pre-process in-parameters. Inject values and check constraints if necessary.
                  this.EmitProcessParameters( codeGenerationInfo, compositeMethodModel, true, publicCompositeGenInfo, thisGenInfo, methodGenInfo );

                  // Throw if constraint violations
                  if ( hasInConstraints )
                  {
                     this.EmitThrowIfViolations( thisGenInfo, methodGenInfo, compositeMethodModel );
                  }
               }

               // Save original parameters to array if we have generic side effects, or if we have any by-ref parameter types
               if ( methodGenInfo.HasByRefParameters || compositeMethodModel.SideEffects.Any( seModel => seModel.IsGeneric ) )
               {
                  this.EmitStoreArgumentsToObjectArray( methodGenInfo, methodGenInfo.GetOrCreateLocal( LB_ARGS_ARRAY_FOR_SIDE_EFFECTS ) );
               }

               // Call fragment (concern or mixin)
               AbstractFragmentMethodModel nextMethod = compositeMethodModel.Concerns.Any() ? (AbstractFragmentMethodModel) compositeMethodModel.Concerns.First() : compositeMethodModel.Mixin;
               this.EmitCallFragmentModel(
                  codeGenerationInfo,
                  fragmentTypeGenerationInfos,
                  publicCompositeGenInfo,
                  thisGenInfo,
                  methodGenInfo,
                  emittingInfo,
                  null,
                  null,
                  typeModel,
                  nextMethod,
                  null,
                  null,
                  false,
                  true
                  );

               if ( compositeMethodModel.PropertyModel == null || !compositeMethodModel.PropertyModel.IsPartOfCompositeState() )
               {
                  // Post-process out-parameters
                  this.EmitProcessParameters( codeGenerationInfo, compositeMethodModel, false, publicCompositeGenInfo, thisGenInfo, methodGenInfo );

                  // Post-process result
                  this.EmitProcessResult( codeGenerationInfo, compositeMethodModel, publicCompositeGenInfo, thisGenInfo, methodGenInfo );

                  if ( hasOutConstraints && hasResult )
                  {
                     this.EmitThrowIfViolations( thisGenInfo, methodGenInfo, compositeMethodModel );
                  }
               }
            },
            !hasSideEffects ? (Action<MethodIL>) null : ( il2 ) =>
            {
               // Store exception to local variable
               il2.EmitStoreLocal( exceptionB );
            },
            true,
            !hasSideEffects ? (Action<MethodIL>) null : ( il2 ) =>
            {
               // Finally action
               ILLabel? endIfLabel = null;
               if ( ( hasInConstraints || hasOutConstraints ) && methodGenInfo.HasLocal( LB_VIOLATIONS ) )
               {
                  // If there are violations, don't invoke side effects
                  endIfLabel = il2.DefineLabel();
                  il2
                     .EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_VIOLATIONS ) )
                     .EmitBranch( BranchType.IF_TRUE, endIfLabel.Value );
               }

               this.EmitSetupResultForSideEffectInvocations( compositeMethodModel, methodGenInfo );

               var seIdx = 0;
               foreach ( SideEffectMethodModel seMethodModel in compositeMethodModel.SideEffects )
               {
                  this.EmitRequiredActionIfHasOnInvocationInjections(
                     seMethodModel,
                     methodGenInfo,
                     compositeMethodModelB =>
                     {
                        il2
                           .EmitCall( SIDE_EFFECT_MODELS_GETTER )
                           .EmitLoadInt32( seIdx )
                           .EmitCall( SIDE_EFFECT_MODELS_INDEXER );
                     }
                     );

                  this.EmitCallFragmentModel(
                     codeGenerationInfo,
                     fragmentTypeGenerationInfos,
                     publicCompositeGenInfo,
                     thisGenInfo,
                     methodGenInfo,
                     emittingInfo,
                     methodGenInfo.GetLocalOrNull( LB_RESULT ),
                     exceptionB,
                     typeModel,
                     seMethodModel,
                     null,
                     null,
                     true,
                     true
                     );
                  seIdx++;
               }

               if ( endIfLabel.HasValue )
               {
                  il2.MarkLabel( endIfLabel.Value );
               }
            }
            );
      }

      protected virtual void EmitSetupResultForSideEffectInvocations(
         CompositeMethodModel compositeMethodModel,
         CompositeMethodGenerationInfo thisMethodGenInfo
         )
      {
         var il = thisMethodGenInfo.IL;
         var resultB = thisMethodGenInfo.GetLocalOrNull( LB_RESULT );

         if ( thisMethodGenInfo.HasByRefParameters && ( compositeMethodModel.SideEffects.Any() || this.HasParameterWithInjection<SideEffectForAttribute>( compositeMethodModel ) ) )
         {
            // <result> is array [result, arg-1, ..., arg-n]
            var argsArrB = thisMethodGenInfo.GetOrCreateLocal( LB_ARGS_ARRAY );
            var arrElemType = argsArrB.LocalType.GetElementType();
            il
               .EmitLoadInt32( thisMethodGenInfo.Parameters.Count + 1 )
               .EmitNewArray( arrElemType )
               .EmitStoreLocal( argsArrB )
               .EmitLoadLocal( argsArrB )
               .EmitLoadInt32( 0 );

            if ( resultB == null )
            {
               il.EmitLoadNull();
            }
            else
            {
               il
                  .EmitLoadLocal( resultB )
                  .EmitCastToType( resultB.LocalType, arrElemType );
            }

            il.EmitStoreElement( arrElemType );
            for ( Int32 idx = 0; idx < thisMethodGenInfo.Parameters.Count; ++idx )
            {
               var paramType = thisMethodGenInfo.Parameters[idx].ParameterType;
               il
                  .EmitLoadLocal( argsArrB )
                  .EmitLoadInt32( idx + 1 )
                  .EmitLoadArgumentForMethodCall( thisMethodGenInfo.Parameters[idx] )
                  .EmitCastToType( paramType, arrElemType )
                  .EmitStoreElement( arrElemType );
            }
         }
      }

      protected virtual void EmitStoreArgumentsToObjectArray(
         CompositeMethodGenerationInfo thisMethodGenInfo,
          LocalBuilder arrayB
         )
      {
         var il = thisMethodGenInfo.IL;
         if ( thisMethodGenInfo.Parameters.Any() )
         {
            var arrElemType = arrayB.LocalType.GetElementType();
            il.EmitLoadInt32( thisMethodGenInfo.Parameters.Count )
              .EmitNewArray( arrElemType )
              .EmitStoreLocal( arrayB );
            for ( Int32 idx = 0; idx < thisMethodGenInfo.Parameters.Count; ++idx )
            {
               var paramType = thisMethodGenInfo.Parameters[idx].ParameterType;
               il
                  .EmitLoadLocal( arrayB )
                  .EmitLoadInt32( idx )
                  .EmitLoadArgumentForMethodCall( thisMethodGenInfo.Parameters[idx] )
                  .EmitCastToType( paramType, arrElemType )
                  .EmitStoreElement( arrElemType );
            }
         }
      }

      protected virtual Boolean HasConstraints( ParameterModel pModel )
      {
         return pModel.Constraints.Any() || ( !VOID_TYPE.Equals( pModel.NativeInfo.ParameterType ) && !pModel.IsOptional && ( !pModel.NativeInfo.ParameterType.IsValueType || pModel.NativeInfo.ParameterType.IsNullable() ) );
      }

      protected virtual void EmitAfterCompositeMethodBodyBegan(
         IEnumerable<FragmentTypeGenerationInfo> fragmentTypeGenerationInfos,
         CompositeTypeGenerationInfo thisGenerationInfo,
         CompositeMethodGenerationInfo thisCompositeMethodGenerationInfo,
         CompositeMethodModel compositeMethodModel
         )
      {
         // By default, nothing to do.
      }

      protected virtual void EmitCallAllSpecialMethods<AttributeType>(
         CompositeCodeGenerationInfo codeGenerationInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentTypeGenerationInfos,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenerationInfo,
         CompositeMethodGenerationInfo thisCompositeMethodGenerationInfo,
         CompositeEmittingInfo emittingInfo,
         Int32 methodID,
         CompositeModel instanceableModel,
         CompositeTypeModel typeModel,
         Boolean catchExceptionsOfSingleInvoke
         )
         where AttributeType : Attribute
      {
         SpecialMethodModel[] activationMethodModels = this.GetSpecialMethods<AttributeType>( instanceableModel );
         if ( activationMethodModels.Any() )
         {
            var fTypes = activationMethodModels.Select( sMethod => sMethod.FragmentType ).GetBottomTypes();
            foreach ( var fType in fTypes )
            {
               this.EmitUseFragmentPool(
                  codeGenerationInfo,
                  fragmentTypeGenerationInfos,
                  publicCompositeGenInfo,
                  thisGenerationInfo,
                  thisCompositeMethodGenerationInfo,
                  emittingInfo,
                  null,
                  null,
                  instanceableModel,
                  null,
                  typeModel,
                  fType.NewWrapperAsType( this.ctx ),
                  false,
                  ( resolvedFragmentType, fragmentGenInfo ) =>
                  {
                     this.EmitCallFragmentSpecialMethods<AttributeType>( instanceableModel, resolvedFragmentType, fragmentGenInfo, thisCompositeMethodGenerationInfo, catchExceptionsOfSingleInvoke );
                  }
                  );
            }
         }
      }

      protected virtual SpecialMethodModel[] GetSpecialMethods<AttributeType>(
         CompositeModel compositeModel
         )
         where AttributeType : Attribute
      {
         return compositeModel.SpecialMethods.Where( sMethod => sMethod.AllAttributes.OfType<AttributeType>().Any() ).ToArray();
      }

      protected virtual void EmitProcessParameters(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeMethodModel compositeMethodModel,
         Boolean acceptIn,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo
         )
      {
         foreach ( ParameterModel paramModel in compositeMethodModel.Parameters )
         {
            if ( acceptIn != paramModel.NativeInfo.IsOut )
            {
               this.EmitProcessCompositeMethodParameter( codeGenerationInfo, compositeMethodModel, paramModel, publicCompositeGenInfo, thisGenInfo, thisMethodGenInfo, false );
            }
         }
      }

      protected virtual void EmitProcessCompositeMethodParameter(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeMethodModel compositeMethodModel,
         ParameterModel paramModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         Boolean paramIsResult
         )
      {
         this.EmitProcessParameter(
            codeGenerationInfo,
            paramModel,
            publicCompositeGenInfo,
            thisGenInfo,
            thisMethodGenInfo,
            paramIsResult ? thisMethodGenInfo.GetLocalOrThrow( LB_RESULT ) : null,
            il2 =>
            {
               LocalBuilder compositeMethodModelB;
               this.InitializeComplexMethodModelLocalIfNecessary( compositeMethodModel, thisMethodGenInfo, out compositeMethodModelB );
               il2.EmitLoadLocal( compositeMethodModelB );
               if ( paramIsResult )
               {
                  il2.EmitCall( COMPOSITE_METHOD_RESULT_GETTER );
               }
               else
               {
                  il2
                     .EmitCall( COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER )
                     .EmitLoadInt32( paramModel.NativeInfo.Position )
                     .EmitCall( ChangeQueryItemGetterDeclaringTypeGArgs( PARAMETER_MODEL_TYPE ) );
               }
            },
            false );
      }

      protected virtual void EmitProcessParameter(
         CompositeCodeGenerationInfo codeGenerationInfo,
         ParameterModel paramModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         AbstractTypeGenerationInfoForComposites thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         LocalBuilder paramAsLocalOuter,
         Action<MethodIL> loadParameterModel,
         Boolean forceConstraintCheck,
         CILTypeBase overrideParamType = null,
         Action<AbstractInjectableModel, MethodGenerationInfo, CILTypeBase, LocalBuilder, IList<CILField>, Action<MethodIL>> injectionProviderUsageAction = null,
         Func<CILType, IList<CILField>> additionalFieldsInLambaClass = null,
         Action<MethodGenerationInfo, LocalBuilder, IList<CILField>> useAdditionalFields = null
         )
      {
         Boolean hasConstraints = paramModel.Constraints.Any();

         Action<AbstractInjectableModel, MethodGenerationInfo, CILTypeBase, LocalBuilder, IList<CILField>, Action<MethodIL>> constraintCheckAction = ( injectableModel, methodGenInfo, actualType, paramAsLocal, lambdaAdditionalFields, loadInjModel ) =>
         {
            if ( injectionProviderUsageAction != null )
            {
               injectionProviderUsageAction( injectableModel, methodGenInfo, actualType, paramAsLocal, lambdaAdditionalFields, loadInjModel );
            }

            var il = methodGenInfo.IL;
            var instanceB = methodGenInfo.GetLocalOrThrow( LB_C_INSTANCE );
            var paramBuilder = paramAsLocal == null ? methodGenInfo.Parameters[paramModel.NativeInfo.Position] : null;

            ILLabel? endConstraintCheckL = null;
            // Skip constraint checking if instance is prototype
            if ( !forceConstraintCheck && ( hasConstraints || ( !paramModel.IsOptional && !actualType.IsValueType() ) ) )
            {
               endConstraintCheckL = il.DefineLabel();
               il.EmitLoadLocal( instanceB )
                 .EmitCall( IS_PROTOTYPE_GETTER )
                 .EmitBranch( BranchType.IF_TRUE, endConstraintCheckL.Value );
            }

            this.EmitAddToViolationsIfNotOptionalAndNull(
               paramModel,
               actualType,
               () => il.EmitLoadArgumentForMethodCall( paramBuilder ),
               () => il.EmitLoadArgAddress( paramBuilder.Position + 1 ),
               paramAsLocal,
               methodGenInfo,
               hasConstraints ? endConstraintCheckL : null
               );

            if ( hasConstraints )
            {
               var paramModelB = methodGenInfo.GetOrCreateLocal( LB_PARAM_MODEL );
               var constraintModelB = methodGenInfo.GetOrCreateLocal( LB_CONSTRAINT_MODEL );

               // parameterModel = compositeMethodModel.Parameters.ElementAt(<index>);
               loadParameterModel( il );
               il.EmitStoreLocal( paramModelB );

               for ( Int32 idx = 0; idx < paramModel.Constraints.Count(); ++idx )
               {
                  var constraintModel = paramModel.Constraints.ElementAt( idx );

                  // constraintModel = parameterModel.Constraints.ElementAt(<idx>);
                  il.EmitLoadLocal( paramModelB )
                    .EmitCall( CONSTRAINT_MODELS_GETTER )
                    .EmitLoadInt32( idx )
                    .EmitCall( ChangeQueryItemGetterDeclaringTypeGArgs( CONSTRAINT_MODEL_TYPE ) )
                    .EmitStoreLocal( constraintModelB );

                  // InstancePool<ConstraintInstance> constraintPool = instance.Application.GetConstraintInstancePool(<resolved constraint type>);
                  var resolvedConstraintType = this.ResolveConstraintType( constraintModel, actualType );
                  var constraintInstancePoolB = methodGenInfo.GetOrCreateLocal( LB_CONSTRAINT_INSTANCE_POOL );
                  il.EmitLoadLocal( instanceB )
                    .EmitCall( STRUCTURE_OWNER_GETTER_METHOD )
                    .EmitCall( APPLICATION_GETTER_METHOD )
                    .EmitReflectionObjectOf( resolvedConstraintType )
                    .EmitCall( GET_CONSTRAINT_INSTANCE_POOL_METHOD )
                    .EmitStoreLocal( constraintInstancePoolB );
                  // Object cInstance = constraintPool.TakeInstance();
                  var constraintInstanceB = methodGenInfo.GetOrCreateLocal( LB_CONSTRAINT_INSTANCE );
                  il.EmitLoadLocal( constraintInstancePoolB )
                    .EmitLoadLocalAddress( constraintInstanceB )
                    .EmitCall( TAKE_CONSTRAINT_INSTANCE_METHOD );
                  //.EmitPop();
                  // if (cInstance == null)
                  // then
                  // cInstance = new ConstraintInstanceImpl(new <constraint type>());
                  // end if
                  il.EmitIf(
                     ( il2, endIfLabel ) =>
                     {
                        il2//.EmitLoadLocal( constraintInstanceB )
                           .EmitBranch( BranchType.IF_TRUE, endIfLabel );
                     },
                     ( il2, endIfLabel ) =>
                     {
                        il2.EmitNewObject( resolvedConstraintType.Constructors.First( ctor => !ctor.Parameters.Any() ) )
                           .EmitStoreLocal( constraintInstanceB )
                           .EmitLoadLocal( constraintInstancePoolB )
                           .EmitLoadLocal( constraintInstanceB )
                           .EmitCall( RETURN_CONSTRAINT_INSTANCE_METHOD );
                     }
                     );
                  // constraintPool.ReturnInstance(cInstance);
                  //il.EmitLoadLocal( constraintInstancePoolB )
                  //  .EmitLoadLocal( constraintInstanceB )
                  //  .EmitCall( RETURN_CONSTRAINT_INSTANCE_METHOD );

                  // if (!((Constraint<<attrType>, <valueType>>)cInstance).IsValid(constraintModel.ResolvedConstraintAttribute, <param>))
                  // then
                  // (add to violations)
                  // endif
                  il.EmitIf(
                     ( il2, endIfLabel ) =>
                     {
                        var cGArgs = new CILTypeBase[] { constraintModel.ConstraintAttribute.GetType().NewWrapper( this.ctx ), ( actualType.IsByRef() ? actualType.GetElementType() : actualType ) };
                        //                        var iFaceType = IS_VALID_METHOD.DeclaringType.MakeGenericType( cGArgs );
                        var methodToCall = IS_VALID_METHOD.ChangeDeclaringType( cGArgs );
                        il2.EmitLoadLocal( constraintInstanceB )
                           .EmitCastToType( OBJECT_TYPE, resolvedConstraintType )
                           .EmitLoadLocal( constraintModelB )
                           .EmitCall( CONSTRAINT_ATTRIBUTE_GETTER )
                           .EmitCastToType( ATTRIBUTE_TYPE, cGArgs[0] );
                        if ( paramAsLocal == null )
                        {
                           il2.EmitLoadArgumentForMethodCall( paramBuilder );
                        }
                        else
                        {
                           il2.EmitLoadLocal( paramAsLocal );
                        }
                        il2.EmitCastToType( actualType, cGArgs[1] )
                           .EmitCall( methodToCall )
                           .EmitBranch( BranchType.IF_TRUE, endIfLabel );
                     },
                     ( il2, endIfLabel ) => this.EmitAddToViolationsOptional( paramModel, paramBuilder, methodGenInfo, paramAsLocal )
                     );
               }
            }
            if ( endConstraintCheckL.HasValue )
            {
               if ( methodGenInfo != thisMethodGenInfo )
               {
                  // We are in lambda - throw if any violations.
                  this.EmitThrowIfViolations( thisGenInfo, methodGenInfo, (AbstractMethodModel) paramModel.Owner );
               }
               il.MarkLabel( endConstraintCheckL.Value );
            }
         };

         var paramIsResult = paramAsLocalOuter != null;
         var paramRuntimeType = overrideParamType ?? ( paramIsResult ? paramAsLocalOuter.LocalType : thisMethodGenInfo.Parameters[paramModel.NativeInfo.Position].ParameterType );

         if ( paramModel.InjectionScope == null )
         {
            constraintCheckAction( paramModel, thisMethodGenInfo, paramRuntimeType, paramAsLocalOuter, null, loadParameterModel );
         }
         else
         {
            // injectionProviderContext = instance.Application.InjectionService.CreateProviderContext(compositeMethodModel.Parameters[<idx>].InjectionAttribute, <paramType>);
            Boolean shouldEmitConstraintCheckHere = false;
            Action<MethodIL> actiun = il2 =>
               shouldEmitConstraintCheckHere = this.EmitUseInjectionProvider(
                  codeGenerationInfo,
                  paramModel,
                  publicCompositeGenInfo,
                  thisGenInfo,
                  thisMethodGenInfo,
                  loadParameterModel,
                  paramRuntimeType,
                  paramAsLocalOuter == null || paramRuntimeType.Equals( paramAsLocalOuter.LocalType ),
                  constraintCheckAction,
                  additionalFieldsInLambaClass,
                  useAdditionalFields
               );

            if ( paramIsResult )
            {
               actiun( thisMethodGenInfo.IL );
               thisMethodGenInfo.IL.EmitStoreLocal( paramAsLocalOuter );
            }
            else
            {
               thisMethodGenInfo.IL.EmitStoreToArgument( paramIsResult ? thisMethodGenInfo.ReturnParameter : thisMethodGenInfo.Parameters[paramModel.NativeInfo.Position], actiun );
            }

            if ( shouldEmitConstraintCheckHere )
            {
               constraintCheckAction( paramModel, thisMethodGenInfo, paramRuntimeType, paramAsLocalOuter, null, loadParameterModel );
            }
         }
      }

      protected virtual void EmitAddToViolationsIfNotOptionalAndNull(
         ParameterModel paramModel,
         CILTypeBase paramRuntimeType,
         Action loadParameterAction,
         Action loadParameterAddressAction,
         LocalBuilder paramAsLocal,
         MethodGenerationInfo thisMethodGenInfo,
         ILLabel? branchIfNull
         )
      {
         var il = thisMethodGenInfo.IL;
         var branchIfNotNull = il.DefineLabel();
         if ( paramRuntimeType.IsByRef() )
         {
            paramRuntimeType = paramRuntimeType.GetElementType();
         }

         if ( !paramRuntimeType.IsValueType() && ( !paramModel.IsOptional || branchIfNull != null ) )
         {
            // if (<param> == null)
            if ( paramAsLocal != null )
            {
               il.EmitLoadLocal( paramAsLocal )
                 .EmitCastToType( paramAsLocal.LocalType, OBJECT_TYPE );
            }
            else
            {
               if ( paramRuntimeType.IsByRef() )
               {
                  loadParameterAddressAction();
               }
               else
               {
                  loadParameterAction();
                  il.EmitCastToType( paramRuntimeType, OBJECT_TYPE );
               }
            }

            if ( paramModel.IsOptional )
            {
               // Don't check constraints if optional
               il.EmitBranch( BranchType.IF_FALSE, branchIfNull.Value );
            }
            else
            {
               il.EmitBranch( BranchType.IF_TRUE, branchIfNotNull );

               // then
               // (add to violations)
               this.EmitAddToViolationsNotOptional( paramModel, thisMethodGenInfo );
               if ( branchIfNull.HasValue )
               {
                  // else
                  il.EmitBranch( BranchType.ALWAYS, branchIfNull.Value );
               }
            }
         }
         il.MarkLabel( branchIfNotNull );
      }

      protected virtual Boolean EmitUseInjectionProvider(
         CompositeCodeGenerationInfo codeGenerationInfo,
         AbstractInjectableModel injectableModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         AbstractTypeGenerationInfoForComposites thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         Action<MethodIL> loadInjectableModelAction,
         CILTypeBase typeToGive,
         Boolean castToTypeToGive,
         Action<AbstractInjectableModel, MethodGenerationInfo, CILTypeBase, LocalBuilder, IList<CILField>, Action<MethodIL>> injectionProviderUsageAction,
         Func<CILType, IList<CILField>> additionalFieldsInLambaClass,
         Action<MethodGenerationInfo, LocalBuilder, IList<CILField>> useAdditionalFields
         )
      {
         var il = thisMethodGenInfo.IL;
         LocalBuilder instanceB;
         CILTypeBase actualTypeToGive;
         var isLazy = typeToGive.IsLazy( out actualTypeToGive );
         if ( isLazy )
         {
            Tuple<CILType, CILConstructor, CILMethod, CILField, CILField, CILField, IList<CILField>> lambdaInfo;
            if ( !publicCompositeGenInfo.LazyInjectionLambdaClasses.TryGetValue( injectableModel, out lambdaInfo ) )
            {
               // Create lambda class
               var lambdaTBInfo = new TypeGenerationInfoImpl( thisGenInfo.Builder.AddType(
                  INJECTION_LAMBDA_CLASS_PREFIX + thisGenInfo.GetNextLambdaClassID(),
                  TypeAttributes.NestedPrivate | TypeAttributes.Sealed ),
                  publicCompositeGenInfo.GenericArguments.Count );
               var lambdaTB = lambdaTBInfo.Builder;

               var lambdaCtor = lambdaTB.AddDefaultConstructor( MethodAttributes.Public | MethodAttributes.HideBySig );
               var lambdaCInstanceField = lambdaTB.AddField( codeGenerationInfo.CompositeInstanceFieldName, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ), FieldAttributes.Public );
               var lambdaInjectableModelField = lambdaTB.AddField( LAZY_INJECTION_LAMBDA_CLASS_INJECTABLE_MODEL_FIELD_NAME, ABSTRACT_INJECTABLE_MODEL_TYPE, FieldAttributes.Public );
               var lambdaTypeField = lambdaTB.AddField( LAZY_INJECTION_LAMBDA_CLASS_TYPE_FIELD_NAME, TYPE_TYPE, FieldAttributes.Public );

               IList<CILField> lambdaAdditionalFields = null;
               if ( additionalFieldsInLambaClass != null )
               {
                  lambdaAdditionalFields = additionalFieldsInLambaClass( lambdaTB );
               }

               var lambdaMethod = lambdaTB.AddMethod( LAMBDA_METHOD_NAME, MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard );
               lambdaMethod.ReturnParameter.ParameterType = actualTypeToGive;
               var lambdaIL = lambdaMethod.MethodIL;
               var lambdaMethodGenInfo = new MethodGenerationInfoImpl( lambdaMethod );
               instanceB = lambdaMethodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );
               var resultB = lambdaMethodGenInfo.GetOrCreateLocal( LB_RESULT, INJECTION_CONTEXT_PROVIDER_METHOD.ReturnParameter.ParameterType );
               lambdaIL
                  .EmitLoadThisField( lambdaCInstanceField )
                  .EmitStoreLocal( instanceB )
                  .EmitLoadLocal( instanceB )
                  .EmitCall( STRUCTURE_OWNER_GETTER_METHOD )
                  .EmitCall( APPLICATION_GETTER_METHOD )
                  .EmitCall( INJECTION_SERVICE_GETTER_METHOD )
                  .EmitLoadLocal( instanceB )
                  .EmitLoadThisField( lambdaInjectableModelField )
                  //.EmitCall( INJECTABLE_MODEL_INJECTION_SCOPES_GETTER )
                  .EmitReflectionObjectOf( actualTypeToGive )
                  .EmitCall( INJECTION_CONTEXT_PROVIDER_METHOD )
                  .EmitStoreLocal( resultB );

               injectionProviderUsageAction( injectableModel, lambdaMethodGenInfo, actualTypeToGive, resultB, lambdaAdditionalFields, il3 => il3.EmitLoadThisField( lambdaInjectableModelField ) );
               lambdaIL
                  .EmitLoadLocal( resultB )
                  .EmitCastToType( resultB.LocalType, actualTypeToGive )
                  .EmitReturn();

               lambdaInfo = Tuple.Create( lambdaTB, lambdaCtor, lambdaMethod, lambdaCInstanceField, lambdaInjectableModelField, lambdaTypeField, lambdaAdditionalFields );
               publicCompositeGenInfo.LazyInjectionLambdaClasses.Add( injectableModel, lambdaInfo );
            }

            var lambdaB = thisMethodGenInfo.GetOrCreateLocalBasedOnType( lambdaInfo.Item1 );
            var thisInstanceB = thisMethodGenInfo.GetLocalOrThrow( LB_C_INSTANCE );
            var actualFuncCtor = FUNC_1_CTOR.ChangeDeclaringType( actualTypeToGive );
            var actualLazyCtor = LAZY_GDEF_CTOR.ChangeDeclaringType( actualTypeToGive );
            il
               .EmitNewObject( lambdaInfo.Item2 )
               .EmitStoreLocal( lambdaB )
               .EmitLoadLocal( lambdaB )
               .EmitStoreOtherField( lambdaInfo.Item4, il2 => il2.EmitLoadLocal( thisInstanceB ) )
               .EmitLoadLocal( lambdaB )
               .EmitStoreOtherField( lambdaInfo.Item5, loadInjectableModelAction )
               .EmitLoadLocal( lambdaB )
               .EmitStoreOtherField( lambdaInfo.Item6, il2 => il2.EmitReflectionObjectOf( actualTypeToGive ) );
            if ( useAdditionalFields != null )
            {
               useAdditionalFields( thisMethodGenInfo, lambdaB, lambdaInfo.Item7 );
            }
            il
               .EmitLoadLocal( lambdaB )
               .EmitLoadUnmanagedMethodToken( lambdaInfo.Item3 )
               .EmitNewObject( actualFuncCtor )
               .EmitLoadInt32( (Int32) System.Threading.LazyThreadSafetyMode.None )
               .EmitNewObject( actualLazyCtor );
         }
         else
         {

            instanceB = thisMethodGenInfo.GetLocalOrThrow( LB_C_INSTANCE );
            // instance.Application.InjectionService.ProvideInjectable(instance, <load injectable>.Scope, <paramType>);
            il.EmitLoadLocal( instanceB )
              .EmitCall( STRUCTURE_OWNER_GETTER_METHOD )
              .EmitCall( APPLICATION_GETTER_METHOD )
              .EmitCall( INJECTION_SERVICE_GETTER_METHOD )
              .EmitLoadLocal( instanceB );
            loadInjectableModelAction( il );
            il
               //.EmitCall( INJECTABLE_MODEL_INJECTION_SCOPES_GETTER )
              .EmitReflectionObjectOf( typeToGive )
              .EmitCall( INJECTION_CONTEXT_PROVIDER_METHOD );
            if ( castToTypeToGive )
            {
               il.EmitCastToType( INJECTION_CONTEXT_PROVIDER_METHOD.ReturnParameter.ParameterType, typeToGive );
            }
         }

         return !isLazy;
      }

      protected virtual CILType ResolveConstraintType( ConstraintModel constraintModel, CILTypeBase parameterType )
      {
         var constraintType = constraintModel.ConstraintType.NewWrapperAsType( this.ctx );
         if ( constraintType.ContainsGenericParameters() )
         {
            // TODO refactor this method
            var gArgs = constraintType.GenericArguments.ToArray();
            foreach ( var constraintIFace in constraintType.AllImplementedInterfaces( false ).Where( iFace => CONSTRAINT_TYPE.Equals( iFace.GenericDefinition ) ) )
            {
               // attribute
               var attribParam = constraintIFace.GenericArguments[0];
               // parameter
               var paramParam = constraintIFace.GenericArguments[1];
               if ( constraintIFace.ContainsGenericParameters() )
               {

                  if ( TypeKind.TypeParameter == attribParam.TypeKind )
                  {
                     gArgs[( (CILTypeParameter) attribParam ).GenericParameterPosition] = constraintModel.ConstraintAttribute.GetType().NewWrapper( this.ctx );
                  }

                  if ( paramParam.ContainsGenericParameters() )
                  {
                     var constraintBFS = new Queue<CILTypeBase>();
                     var paramBFS = new Queue<CILTypeBase>();
                     constraintBFS.Enqueue( paramParam );
                     paramBFS.Enqueue( parameterType );

                     while ( constraintBFS.Any() && paramBFS.Any() )
                     {
                        var currentFromConstraint = constraintBFS.Dequeue();
                        var currentFromParam = paramBFS.Dequeue();
                        if ( TypeKind.TypeParameter == currentFromConstraint.TypeKind )
                        {
                           gArgs[( (CILTypeParameter) currentFromConstraint ).GenericParameterPosition] = currentFromParam;
                        }
                        else if ( TypeKind.Type == currentFromParam.TypeKind )
                        {
                           currentFromParam = ( (CILType) currentFromParam ).FullInheritanceChain().FirstOrDefault( bType => bType.GenericDefinitionIfGenericType().Equals( ( (CILType) currentFromConstraint ).GenericDefinitionIfGenericType() ) );
                           if ( currentFromParam == null )
                           {
                              throw new InternalException( "Constraint type " + constraintType + " is not compatible with parameter type" + parameterType + "." );
                           }
                           foreach ( var nextFromConstraint in ( (CILType) currentFromConstraint ).GenericArguments )
                           {
                              constraintBFS.Enqueue( nextFromConstraint );
                           }
                           foreach ( var nextFromParam in ( (CILType) currentFromParam ).GenericArguments )
                           {
                              paramBFS.Enqueue( nextFromParam );
                           }
                        }
                     }
                  }
               }
            }
            if ( gArgs.Any( gArg => TypeKind.TypeParameter == gArg.TypeKind && ( (CILTypeParameter) gArg ).DeclaringType.Equals( constraintType ) ) )
            {
               throw new InternalException( "Could not resolve generic arguments for constraint" + constraintType );
            }
            constraintType = constraintType.MakeGenericType( gArgs );
         }
         return constraintType;
      }

      protected virtual void EmitAddToViolationsNotOptional( ParameterModel paramModel, MethodGenerationInfo thisMethodGenInfo )
      {
         this.EmitAddToViolationsCommon( thisMethodGenInfo );

         var il = thisMethodGenInfo.IL;
         var violationsB = thisMethodGenInfo.GetOrCreateLocal( LB_VIOLATIONS );
         // violations.Add(new ConstraintViolationInfoImpl(<index>, null, OptionalAttribute.VALUE, parameterModel.Name));
         il.EmitLoadLocal( violationsB )
           .EmitLoadInt32( paramModel.NativeInfo.Position )
           .EmitLoadNull()
           .EmitLoadThisField( OPTIONAL_ATTRIBUTE_FIELD )
           .EmitLoadString( paramModel.Name )
           .EmitNewObject( CONSTRAINT_VIOLATION_CONSTRUCTOR )
           .EmitCall( ADD_CONSTRAINT_VIOLATION_METHOD );
      }

      protected virtual void EmitAddToViolationsOptional( ParameterModel paramModel, CILParameter paramBuilder, MethodGenerationInfo thisMethodGenInfo, LocalBuilder paramAsLocal )
      {
         this.EmitAddToViolationsCommon( thisMethodGenInfo );

         var il = thisMethodGenInfo.IL;
         var violationsB = thisMethodGenInfo.GetLocalOrThrow( LB_VIOLATIONS );
         var constraintModelB = thisMethodGenInfo.GetLocalOrThrow( LB_CONSTRAINT_MODEL );

         // violations.Add(new ConstraintViolationInfoImpl(<index>, <parameter>, constraintModel.ConstraintAttribute, parameterModel.Name));
         il.EmitLoadLocal( violationsB )
           .EmitLoadInt32( paramModel.NativeInfo.Position );
         if ( paramAsLocal == null )
         {
            il.EmitLoadArgumentForMethodCall( paramBuilder )
               .EmitCastToType( paramBuilder.ParameterType, OBJECT_TYPE );
         }
         else
         {
            il.EmitLoadLocal( paramAsLocal )
              .EmitCastToType( paramAsLocal.LocalType, OBJECT_TYPE );
         }
         il.EmitLoadLocal( constraintModelB )
           .EmitCall( CONSTRAINT_ATTRIBUTE_GETTER )
           .EmitLoadString( paramModel.Name )
           .EmitNewObject( CONSTRAINT_VIOLATION_CONSTRUCTOR )
           .EmitCall( ADD_CONSTRAINT_VIOLATION_METHOD );
      }

      protected virtual void EmitAddToViolationsCommon( MethodGenerationInfo thisMethodGenInfo )
      {
         var il = thisMethodGenInfo.IL;
         var violationsB = thisMethodGenInfo.GetOrCreateLocal( LB_VIOLATIONS );
         // if (violations == null)
         // then
         // violations = new List<ConstraintViolationInfo>();
         // endif
         il.EmitIf(
            ( il2, endIfLabel ) =>
            {
               il2.EmitLoadLocal( violationsB )
                  .EmitBranch( BranchType.IF_TRUE, endIfLabel );
            },
            ( il2, endIfLabel ) =>
            {
               il2.EmitNewObject( CONSTRAINT_VIOLATION_LIST_CTOR )
                  .EmitStoreLocal( violationsB );
            }
            );
      }

      protected virtual void EmitThrowIfViolations( AbstractTypeGenerationInfoForComposites thisTypeGenInfo, MethodGenerationInfo thisMethodGenInfo, AbstractMethodModel methodModel )
      {
         var il = thisMethodGenInfo.IL;
         LocalBuilder violationsB = null;
         if ( thisMethodGenInfo.HasLocal( LB_VIOLATIONS ) )
         {
            violationsB = thisMethodGenInfo.GetLocalOrThrow( LB_VIOLATIONS );

            // if (violations != null && violations.Count > 0)
            // then
            // throw new ConstraintViolationException(<method of composite method>, violations);
            // endif
            il.EmitIf(
               ( il2, endIfLabel ) =>
               {
                  il2.EmitLoadLocal( violationsB )
                     .EmitBranch( BranchType.IF_FALSE, endIfLabel )
                     .EmitLoadLocal( violationsB )
                     .EmitCall( VIOLATIONS_LIST_COUNT_GETTER )
                     .EmitLoadInt32( 0 )
                     .EmitBranch( BranchType.IF_FIRST_LESSER_THAN_SECOND, endIfLabel );
               },
               ( il2, endIfLabel ) =>
               {
                  // 1st param is <method of current method>, 2nd param is violations-list
                  var eMethod = methodModel.NativeInfo.NewWrapper( this.ctx );
                  il2.EmitReflectionObjectOf( TypeGenerationUtils.GetMethodForEmitting( thisTypeGenInfo.Parents, eMethod ).MakeGenericMethod( thisMethodGenInfo.GenericArguments.ToArray() ) )
                     .EmitLoadLocal( violationsB )
                     .EmitThrowNewException( CONSTRAINT_VIOLATION_EXCEPTION_CTOR );
               }
               );
         }
      }

      protected virtual void EmitProcessResult(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeMethodModel compositeMethodModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo
         )
      {
         if ( thisMethodGenInfo.HasLocal( LB_RESULT ) )
         {
            this.EmitProcessCompositeMethodParameter( codeGenerationInfo, compositeMethodModel, compositeMethodModel.Result, publicCompositeGenInfo, thisGenInfo, thisMethodGenInfo, true );
         }
      }

      protected virtual FragmentTypeGenerationInfo GetTypeGenerationInfo(
         Type typeOrGenDef,
         IEnumerable<FragmentTypeGenerationInfo> typeGenerationInfos,
         CILType methodDeclaringType
         )
      {
         var typeFromModel = typeOrGenDef.GetGenericDefinitionIfGenericType().NewWrapper( this.ctx );
         var typeGenInfo = typeGenerationInfos
            .Where( genInfo => genInfo.DirectBaseFromModel.GenericDefinitionIfGenericType().Equals( typeFromModel ) )
            .SingleOrDefault( genInfo => genInfo.Parents.ContainsKey( methodDeclaringType ) );
         if ( typeGenInfo == null )
         {
            throw new InternalException( "Could not deduct what generic arguments to give for type " + typeOrGenDef + " in " + methodDeclaringType + "." );
         }
         return typeGenInfo;
      }

      protected virtual void EmitUseFragmentPool(
         CompositeCodeGenerationInfo codeGenerationInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentTypeGenerationInfos,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         CompositeEmittingInfo emittingInfo,
         LocalBuilder resultB,
         LocalBuilder exceptionB,
         CompositeModel compositeModel,
         AbstractFragmentMethodModel fragmentMethodToCall,
         CompositeTypeModel typeModel,
         CILType fragmentTypeFromModel,
         Boolean catchAndIgnoreExceptions,
         Action<CILType, FragmentTypeGenerationInfo> callMethodAction
         )
      {
         var cInstanceB = thisMethodGenInfo.GetLocalOrThrow( LB_C_INSTANCE );
         var fInstanceB = thisMethodGenInfo.GetOrCreateLocal( LB_F_INSTANCE );

         TypeBindingInformation fTypeInfo = null;
         if ( !TryFindInTypeDictionarySearchSubTypes(
             TypeGenerationUtils.GenericDefinitionIfGenericType( fragmentTypeFromModel ),
             emittingInfo.GetEmulatedFragmentTypeBindingInfos( typeModel ),
            out fTypeInfo ) )
         {
            throw new InternalException( "Could not find type binding information for " + fragmentTypeFromModel + "." );
         }
         var fragmentGenInfo = this.GetTypeGenerationInfo( fTypeInfo.NativeInfo, fragmentTypeGenerationInfos, fragmentTypeFromModel );
         var needPool = fragmentGenInfo.IsInstancePoolRequired;
         var poolB = needPool ? thisMethodGenInfo.GetOrCreateLocal( LB_F_INSTANCE_POOL ) : null;

         var fragmentType = ( thisGenInfo.GenericArguments.Any() ? fragmentGenInfo.Builder.MakeGenericType( thisGenInfo.GenericArguments.ToArray() ) : fragmentGenInfo.Builder );
         var il = thisMethodGenInfo.IL;

         if ( needPool )
         {
            // fragmentPool = instance.GetInstancePoolForFragment(<fragmentType>);
            il.EmitLoadLocal( cInstanceB )
               .EmitReflectionObjectOf( fragmentType )
               .EmitCall( GET_FRAGMENT_INSTANCE_POOL_METHOD )
               .EmitStoreLocal( poolB )

         // fragmentInstance = fragmentPool.TakeInstance();
           .EmitLoadLocal( poolB )
           .EmitLoadLocalAddress( fInstanceB )
           .EmitCall( TAKE_FRAGMENT_INSTANCE_METHOD )
           .EmitPop();
         }
         else
         {
            // fragmentInstance = instance.GetInstanceForFragment(<fragmentType>);
            il.EmitLoadLocal( cInstanceB )
              .EmitReflectionObjectOf( fragmentType )
              .EmitCall( GET_FRAGMENT_INSTANCE_METHOD )
              .EmitStoreLocal( fInstanceB );
         }

         il.EmitTryCatchFinally(
            EXCEPTION_TYPE,
            ( il2 ) =>
            {
               // if (fragmentInstance == null )
               // then
               // <create and initialize fragment>
               // end if
               // Provide per-invocation injections.
               // end if
               //               Boolean hasSpecialMethods = this.GetSpecialMethods<InitializeAttribute>( compositeModel, fragmentGenInfo ).Any();

               il2
                  .EmitIf(
                  ( il3, endIfLabel ) =>
                  {
                     il3.EmitLoadLocal( fInstanceB );
                     if ( !needPool )
                     {
                        il3.EmitCall( FRAGMENT_GETTER );
                     }
                     il3.EmitBranch( BranchType.IF_TRUE, endIfLabel );
                  },
                  ( il3, endIfLabel ) =>
                  {
                     if ( !needPool )
                     {
                        il3.EmitLoadLocal( fInstanceB );
                     }
                     il3.EmitLoadLocal( cInstanceB )
                        .EmitLoadInt32( fragmentMethodToCall == null ? DEFAULT_INVOCATION_METHOD_INDEX : fragmentMethodToCall.CompositeMethod.MethodIndex )
                        .EmitLoadInt32( fragmentMethodToCall is ConcernMethodModel ? fragmentMethodToCall.CompositeMethod.Concerns.IndexOf( (ConcernMethodModel) fragmentMethodToCall ) : DEFAULT_CONCERN_INVOCATION_INDEX );
                     if ( resultB == null )
                     {
                        il3.EmitLoadNull();
                     }
                     else
                     {
                        il3.EmitLoadLocal( resultB )
                           .EmitCastToType( resultB.LocalType, OBJECT_TYPE );
                     }
                     if ( exceptionB == null )
                     {
                        il3.EmitLoadNull();
                     }
                     else
                     {
                        il3.EmitLoadLocal( exceptionB );
                     }
                     il3.EmitCall( emittingInfo.FragmentCreationMethods[fragmentGenInfo].Builder );
                     if ( needPool )
                     {
                        il3.EmitStoreLocal( fInstanceB );
                     }
                     else
                     {
                        il3.EmitCall( FRAGMENT_GETTER );
                        il3.EmitCall( FRAGMENT_SETTER );
                     }
                     this.EmitCallFragmentSpecialMethods<InitializeAttribute>( compositeModel, fragmentType, fragmentGenInfo, thisMethodGenInfo, false );
                  }
                  );

               this.EmitInjectAllInvocationScopes( codeGenerationInfo, compositeModel, publicCompositeGenInfo, thisGenInfo, thisMethodGenInfo, fragmentGenInfo );

               // Use fragment
               callMethodAction( fragmentType, fragmentGenInfo );
            },
            !catchAndIgnoreExceptions ? (Action<MethodIL>) null : ( il2 ) =>
            {
               // Ignore
               il2.EmitPop();
            },
            false,
            !needPool ? (Action<MethodIL>) null : ( il2 ) =>
            {
               // if (fragmentInstance != null)
               // then
               // fragmentPool.ReturnInstance(fragmentInstance);
               // end if

               il.EmitIf(
                  ( il3, endIfLabel ) =>
                  {
                     il3.EmitLoadLocal( fInstanceB )
                        .EmitBranch( BranchType.IF_FALSE, endIfLabel );
                  },
                  ( il3, endIfLable ) =>
                  {
                     il3.EmitLoadLocal( poolB )
                        .EmitLoadLocal( fInstanceB )
                        .EmitCall( RETURN_FRAGMENT_INSTANCE_METHOD );
                  }
                  );
            }
            );
      }

      private static Boolean TryFindInTypeDictionarySearchSubTypes<TValue>( CILType type, IDictionary<CILType, TValue> dictionary, out TValue result )
      {
         // First try to find directly
         var found = dictionary.TryGetValue( type, out result );
         if ( !found )
         {
            // Then iterate all value until found
            foreach ( var kvp in dictionary )
            {
               found = TypeGenerationUtils.IsAssignableFrom( type, kvp.Key.GenericDefinitionIfGenericType() );
               if ( found )
               {
                  result = kvp.Value;
                  break;
               }
            }
         }

         return found;
      }

      protected virtual void EmitInjectAllInvocationScopes(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         FragmentTypeGenerationInfo fragmentGenInfo
         )
      {
         var il = thisMethodGenInfo.IL;

         // TODO REFACTOR are these two truly necessary?
         thisMethodGenInfo.GetLocalOrThrow( LB_C_INSTANCE );
         thisMethodGenInfo.GetOrCreateLocal( LB_F_INSTANCE );

         // Provide per-invocation injections.
         foreach ( FieldModel fieldModel in this.FindFieldsWithInjectionTime( compositeModel, fragmentGenInfo, InjectionTime.ON_METHOD_INVOKATION ) )
         {
            this.EmitInjectToField( codeGenerationInfo, fieldModel, publicCompositeGenInfo, thisGenInfo, thisMethodGenInfo, fragmentGenInfo, null, null );
         }
      }

      protected virtual void EmitInjectToField(
         CompositeCodeGenerationInfo codeGenerationInfo,
         FieldModel fieldModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         FragmentTypeGenerationInfo fragmentGenInfo,
         Func<CILType, IList<CILField>> additionalFieldsInLambaClass,
         Action<MethodGenerationInfo, LocalBuilder, IList<CILField>> useAdditionalFields
         )
      {
         var il = thisMethodGenInfo.IL;

         var fInstanceB = thisMethodGenInfo.GetOrCreateLocal( LB_F_INSTANCE );

         var nField = fieldModel.NativeInfo;
         var eField = nField.NewWrapper( this.ctx );
         var actualField = TypeGenerationUtils.GetFieldForEmitting( fragmentGenInfo.Parents, eField );

         var fAss = fieldModel.NativeInfo.DeclaringType.Assembly;
         var thisAssName = thisGenInfo.Builder.Module.Assembly.Name.Name;
         var thisAssemblyIsQi4CSForField = fAss.NewWrapper( thisGenInfo.Builder.ReflectionContext ).Name.Name + Qi4CSGeneratedAssemblyAttribute.ASSEMBLY_NAME_SUFFIX == thisAssName;
         var internalAccepted = thisAssemblyIsQi4CSForField && fAss.GetCustomAttributes( true ).OfType<System.Runtime.CompilerServices.InternalsVisibleToAttribute>().Any( attr => attr.AssemblyName.StartsWith( thisAssName ) );
         var dt = nField.DeclaringType;
         var canStoreDirectly = !nField.IsInitOnly
            && ( nField.IsPublic || ( internalAccepted && ( nField.IsAssembly || nField.IsFamilyOrAssembly ) ) )
            && ( dt.IsPublic || ( internalAccepted && !dt.IsVisible ) );

         Action<MethodIL> actiun = il2 =>
         {
            // instance.ModelInfo.Model.Fields[<idx>]
            Action<MethodIL> loadInjModel = il3 =>
                    il3.EmitLoadLocal( thisMethodGenInfo.GetLocalOrThrow( LB_C_INSTANCE ) )
                    .EmitCall( MODEL_INFO_GETTER )
                    .EmitCall( MODEL_GETTER )
                    .EmitCall( FIELDS_GETTER )
                    .EmitLoadInt32( fieldModel.FieldIndex )
                    .EmitCall( ChangeQueryItemGetterDeclaringTypeGArgs( FIELD_MODEL_TYPE ) );

            var shouldEmitNullCheckAction = this.EmitUseInjectionProvider(
               codeGenerationInfo,
               fieldModel,
               publicCompositeGenInfo,
               thisGenInfo,
               thisMethodGenInfo,
               loadInjModel,
               actualField.FieldType,
               false,
               CheckFieldOrCtorParamInjection,
               additionalFieldsInLambaClass,
               useAdditionalFields
            );
            if ( shouldEmitNullCheckAction )
            {
               CheckFieldOrCtorParamInjection( fieldModel, thisMethodGenInfo, actualField.FieldType, null, null, loadInjModel );
            }
         };

         if ( canStoreDirectly )
         {
            il.EmitLoadLocal( fInstanceB )
              .EmitCall( FRAGMENT_GETTER )
              .EmitCastToType( FRAGMENT_GETTER.ReturnParameter.ParameterType, actualField.DeclaringType )
              .EmitStoreOtherField( actualField, actiun );
         }
         else
         {
            il.EmitReflectionObjectOf( actualField )
              .EmitLoadLocal( fInstanceB )
              .EmitCall( FRAGMENT_GETTER );
            actiun( il );
            il.EmitCall( FIELD_SET_VALUE_METHOD );
         }
      }

      protected void CheckFieldOrCtorParamInjection( AbstractInjectableModel injectableModel, MethodGenerationInfo methodGenInfo, CILTypeBase actualType, LocalBuilder injectionResultLocal, IList<CILField> lambdaAdditionalFields, Action<MethodIL> loadInjectableModel )
      {
         var il3 = methodGenInfo.IL;
         if ( !injectableModel.IsOptional )
         {
            var throwExceptionLabel = il3.DefineLabel();
            var doneLabel = il3.DefineLabel();
            if ( injectionResultLocal != null )
            {
               il3.EmitLoadLocal( injectionResultLocal );
            }
            il3
               .EmitDup()
               .EmitBranch( BranchType.IF_FALSE, throwExceptionLabel )
               .EmitBranch( BranchType.ALWAYS, doneLabel )
               .MarkLabel( throwExceptionLabel )
               .EmitPop()
               .EmitLoadString( "Could not provide injection value for non-optional " );
            loadInjectableModel( il3 );
            il3
               .EmitLoadString( "." )
               .EmitCall( STRING_CONCAT_METHOD_3 )
               .EmitReflectionObjectOf( actualType )
               .EmitThrowNewException( INJECTION_EXCEPTION_CTOR )
               .MarkLabel( doneLabel );
         }
         if ( injectionResultLocal == null && injectableModel is FieldModel && !( (FieldModel) injectableModel ).NativeInfo.IsInitOnly && ( (FieldModel) injectableModel ).NativeInfo.IsPublic )
         {
            il3.EmitCastToType( INJECTION_CONTEXT_PROVIDER_METHOD.ReturnParameter.ParameterType, actualType );
         }
         else if ( injectionResultLocal != null )
         {
            il3.EmitPop();
         }
         EmitSetupFragmentDependentInjection( injectableModel, methodGenInfo, actualType, injectionResultLocal, lambdaAdditionalFields );
      }

      protected virtual void EmitCallFragmentModel(
         CompositeCodeGenerationInfo codeGenerationInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentTypeGenerationInfos,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         CompositeEmittingInfo emittingInfo,
         LocalBuilder resultB,
         LocalBuilder exceptionB,
         CompositeTypeModel typeModel,
         AbstractFragmentMethodModel fragmentMethodModel,
         LocalBuilder concernIndexB,
         Action<CILType, CILMethod, FragmentTypeGenerationInfo> actualCallAction,
         Boolean isCallingSideEffects,
         Boolean thisIsComposite
         )
      {
         this.EmitUseFragmentPool(
            codeGenerationInfo,
            fragmentTypeGenerationInfos,
            publicCompositeGenInfo,
            thisGenInfo,
            thisMethodGenInfo,
            emittingInfo,
            resultB,
            exceptionB,
            fragmentMethodModel.CompositeMethod.CompositeModel,
            fragmentMethodModel,
            typeModel,
            fragmentMethodModel.FragmentType.NewWrapperAsType( this.ctx ),
            isCallingSideEffects,
            ( resolvedFragmentType, fragmentGenInfo ) =>
            {
               if ( isCallingSideEffects )
               {
                  this.EmitSetupSideEffects(
                     fragmentMethodModel.CompositeMethod,
                     publicCompositeGenInfo,
                     thisMethodGenInfo
                     );
               }
               else
               {
                  this.EmitSetupConcerns(
                     fragmentMethodModel,
                     publicCompositeGenInfo,
                     thisMethodGenInfo,
                     concernIndexB
                     );
               }

               // Call fragment method
               this.EmitCallFragmentMethod(
                  thisGenInfo,
                  thisMethodGenInfo,
                  fragmentGenInfo,
                  resolvedFragmentType,
                  fragmentMethodModel,
                  fragmentMethodModel.CompositeMethod.NativeInfo,
                  !isCallingSideEffects,
                  actualCallAction,
                  thisIsComposite
                  );
            }
            );
      }

      protected virtual void EmitSetupConcerns(
         AbstractFragmentMethodModel fragmentMethodModel,
         CompositeTypeGenerationInfo publicCompositeTypeGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         LocalBuilder concernIndexB
         )
      {
         if ( fragmentMethodModel is ConcernMethodModel || fragmentMethodModel.CompositeMethod.Parameters.Any( paramModel => paramModel.InjectionScope is ConcernForAttribute ) )
         {
            var il = thisMethodGenInfo.IL;
            il
               .EmitLoadLocal( thisMethodGenInfo.GetLocalOrThrow( LB_F_INSTANCE ) )
               .EmitLoadInt32( fragmentMethodModel.CompositeMethod.MethodIndex );
            if ( concernIndexB == null )
            {
               il.EmitLoadInt32( 0 );
            }
            else
            {
               il
                  .EmitLoadLocal( concernIndexB )
                  .EmitLoadInt32( 1 )
                  .EmitAdd();
            }
            il.EmitCall( F_INSTANCE_SET_NEXT_INFO_METHOD );
         }

         this.ForEachParameterWithInjection<ConcernForAttribute>( fragmentMethodModel.CompositeMethod, pModel => this.EmitSetFragmentForFragmentDependantInjectableParameter( pModel, publicCompositeTypeGenInfo, thisMethodGenInfo ) );
      }

      protected virtual void ForEachParameterWithInjection<InjectionType>( CompositeMethodModel methodModel, Action<ParameterModel> action )
      {
         foreach ( ParameterModel paramModel in methodModel.Parameters.Where( paramModel => paramModel.InjectionScope is InjectionType ) )
         {
            action( paramModel );
         }
      }

      protected virtual Boolean HasParameterWithInjection<InjectionType>( CompositeMethodModel methodModel )
      {
         return methodModel.Parameters.Where( paramModel => paramModel.InjectionScope is InjectionType ).Any();
      }

      protected virtual void EmitSetupSideEffects(
         CompositeMethodModel compositeMethodModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo
         )
      {
         var hasInjectionParams = compositeMethodModel.Parameters.Any( paramModel => paramModel.InjectionScope is SideEffectForAttribute );
         var il = thisMethodGenInfo.IL;
         var resultB = thisMethodGenInfo.GetLocalOrNull( LB_RESULT );
         var exceptionB = thisMethodGenInfo.GetLocalOrThrow( LB_EXCEPTION );

         if ( compositeMethodModel.SideEffects.Any() || hasInjectionParams )
         {
            il
               .EmitLoadLocal( thisMethodGenInfo.GetLocalOrThrow( LB_F_INSTANCE ) )
               .EmitLoadInt32( compositeMethodModel.MethodIndex );
            if ( thisMethodGenInfo.HasByRefParameters )
            {
               il.EmitLoadLocal( thisMethodGenInfo.GetLocalOrThrow( LB_ARGS_ARRAY ) );
            }
            else
            {
               if ( resultB == null )
               {
                  il.EmitLoadNull();
               }
               else
               {
                  il.EmitLoadLocal( resultB )
                    .EmitCastToType( resultB.LocalType, OBJECT_TYPE );
               }
            }
            il.EmitLoadLocal( exceptionB )
              .EmitCall( F_INSTANCE_SET_METHOD_RESULT_METHOD );
         }

         this.ForEachParameterWithInjection<SideEffectForAttribute>( compositeMethodModel, pModel => this.EmitSetFragmentForFragmentDependantInjectableParameter( pModel, publicCompositeGenInfo, thisMethodGenInfo ) );
      }

      protected virtual void EmitSetFragmentForFragmentDependantInjectableParameter(
         ParameterModel pModel,
         CompositeTypeGenerationInfo publicCompositeTypeGen,
         MethodGenerationInfo methodGenInfo
         )
      {
         var il = methodGenInfo.IL;
         var position = pModel.NativeInfo.Position;
         var paramType = methodGenInfo.Parameters[position].ParameterType;
         CILTypeBase actualParamType;
         var isLazy = paramType.IsLazy( out actualParamType );
         il.EmitIf(
                  ( il2, endIfLabel ) =>
                  {
                     il2.EmitLoadArgumentForMethodCall( methodGenInfo.Parameters[position] )
                        .EmitCastToType( paramType, OBJECT_TYPE )
                        .EmitBranch( BranchType.IF_FALSE, endIfLabel );
                  },
                  ( il2, endIfLabel ) =>
                  {
                     il2.EmitLoadArgumentForMethodCall( methodGenInfo.Parameters[position] );
                     if ( isLazy )
                     {
                        il2.EmitCastToType( paramType, publicCompositeTypeGen.LazyInjectionLambdaClasses[pModel].Item1 )
                           .EmitStoreOtherField( publicCompositeTypeGen.LazyInjectionLambdaClasses[pModel].Item7[0], il3 => il3
                              .EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_F_INSTANCE ) )
                              );
                     }
                     else
                     {
                        il2.EmitCastToType( paramType, FRAGMENT_DEPENDANT_PROPERTY.DeclaringType )
                           .EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_F_INSTANCE ) )
                           .EmitCall( FRAGMENT_DEPENDANT_SETTER );
                     }
                  }
                  );
      }

      protected virtual void EmitCallFragmentMethod(
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         FragmentTypeGenerationInfo fragmentGenInfo,
         CILType fragmentType,
         AbstractFragmentMethodModel fragmentMethodModel,
         System.Reflection.MethodInfo compositeMethod,
         Boolean storeResult,
         Action<CILType, CILMethod, FragmentTypeGenerationInfo> actualCallAction,
         Boolean thisIsComposite
         )
      {
         CILMethod methodToCall = null;
         if ( !fragmentMethodModel.IsGeneric )
         {
            var fMB = fragmentGenInfo.NormalMethodBuilders[fragmentMethodModel.NativeInfo.NewWrapper( this.ctx )].Builder;
            methodToCall = TypeGenerationUtils.GetMethodForEmitting( fragmentGenInfo.Parents, fMB );
            if ( fragmentMethodModel.NativeInfo.IsGenericMethodDefinition && thisMethodGenInfo.GenericArguments.Any() )
            {
               methodToCall = methodToCall.MakeGenericMethod( thisMethodGenInfo.GenericArguments.ToArray() );
            }
         }

         if ( actualCallAction == null )
         {
            var fInstanceB = thisMethodGenInfo.GetLocalOrThrow( LB_F_INSTANCE );
            var resultB = storeResult ? thisMethodGenInfo.GetLocalOrNull( LB_RESULT ) : null;

            var methodGenInfo = thisGenInfo.NormalMethodBuilders[compositeMethod.NewWrapper( this.ctx )];
            var il = methodGenInfo.IL;
            var parameters = methodGenInfo.Parameters;
            if ( fragmentMethodModel.IsGeneric )
            {
               // Generic call
               LocalBuilder argsB = null;
               Boolean hasParameters = thisMethodGenInfo.Parameters.Any();
               if ( hasParameters )
               {
                  argsB = thisMethodGenInfo.GetOrCreateLocal( LB_ARGS_ARRAY );
                  if ( storeResult || !methodGenInfo.HasLocal( LB_ARGS_ARRAY_FOR_SIDE_EFFECTS ) )
                  {
                     // Object[] args = new Object[<amount of parameters>];
                     il.EmitLoadInt32( parameters.Count )
                       .EmitNewArray( argsB.LocalType.GetElementType() )
                       .EmitStoreLocal( argsB );

                     if ( parameters.Any() )
                     {
                        // Construct args
                        for ( Int32 idx = 0; idx < parameters.Count; ++idx )
                        {
                           var param = parameters[idx];
                           // args[<idx>] = <parameter>;
                           il.EmitLoadLocal( argsB )
                             .EmitLoadInt32( idx )
                             .EmitLoadArgumentForMethodCall( param )
                             .EmitCastToType( param.ParameterType, argsB.LocalType.GetElementType() )
                             .EmitStoreElement( argsB.LocalType.GetElementType() );
                        }
                     }
                  }
                  else
                  {
                     // Object[] args = argsForSideEffects;
                     il.EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_ARGS_ARRAY_FOR_SIDE_EFFECTS ) )
                       .EmitStoreLocal( argsB );
                  }
               }
               // Generic call
               // ((GenericInvocator)fragmentInstance.Fragment).Invoke( this, <compositeMethod>, args);
               il.EmitLoadLocal( fInstanceB )
                 .EmitCall( FRAGMENT_GETTER )
                 .EmitCastToType( FRAGMENT_GETTER.ReturnParameter.ParameterType, GENERIC_FRAGMENT_METHOD.DeclaringType );

               // The composite
               if ( thisIsComposite )
               {
                  il.EmitLoadArg( 0 );
               }
               else
               {
                  // this is concern / side effect invocation helper, ie inner class.
                  il.EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_C_INSTANCE ) )
                  .EmitCall( COMPOSITES_GETTER )
                  .EmitReflectionObjectOf( thisGenInfo.Builder.DeclaringType.MakeGenericType( thisGenInfo.GenericArguments.ToArray() ), false )
                  .EmitCall( COMPOSITES_GETTER_INDEXER );
               }

               // Instance method (instance.InvocationInfo.Item1)
               il.EmitLoadLocal( thisMethodGenInfo.GetLocalOrThrow( LB_C_INSTANCE ) )
                 .EmitCall( INVOCATION_INFO_GETTER )
                 .EmitCall( INVOCATION_INFO_METHOD_GETTER );

               // Args
               if ( hasParameters )
               {
                  il.EmitLoadLocal( argsB );
               }
               else
               {
                  il.EmitLoadThisField( EMPTY_OBJECTS_FIELD );
               }

               // Call method
               il.EmitCall( GENERIC_FRAGMENT_METHOD );

               // Pop if result of actual method is void
               if ( resultB == null )
               {
                  il.EmitPop();
               }
               else
               {
                  // Store the result of generic invocation to result local builder
                  // Note that if result local builder is value type or generic parameter, and if generic invocation returns null, emitting the default(...) instruction is required.
                  var extraCheckRequired = resultB.LocalType.IsValueType() || resultB.LocalType.IsGenericParameter();
                  il.EmitIfElse(
                     ( il2, elseLabel, endIfLabel ) =>
                     {
                        if ( extraCheckRequired )
                        {
                           il2
                              .EmitDup()
                              .EmitBranch( BranchType.IF_TRUE, elseLabel );
                        }
                     },
                     ( il2, elseLabel, endIfLabel ) =>
                     {
                        if ( extraCheckRequired )
                        {
                           var storeRequired = true;
                           il2
                              .EmitPop()
                              .EmitLoadDefault( resultB.LocalType, tt =>
                              {
                                 storeRequired = false;
                                 return resultB;
                              }, false );
                           if ( storeRequired )
                           {
                              il2.EmitStoreLocal( resultB );
                           }
                        }
                     },
                     ( il2, endIfLabel ) =>
                        il2
                           .EmitCastToType( GENERIC_FRAGMENT_METHOD.GetReturnType(), resultB.LocalType )
                           .EmitStoreLocal( resultB )
                           );
               }

               if ( storeResult )
               {
                  // Need to post-process the arguments array for out-parameters
                  this.EmitAfterGenericCall(
                     methodGenInfo,
                     pBuilder =>
                     {
                        // <arg> = args[idx];
                        il.EmitStoreToArgument( pBuilder, ( il2 ) =>
                        {
                           il2.EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_ARGS_ARRAY ) )
                              .EmitLoadInt32( pBuilder.Position )
                              .EmitLoadElement( argsB.LocalType.GetElementType() )
                              .EmitCastToType( argsB.LocalType.GetElementType(), pBuilder.ParameterType );
                        } );
                     }
                     );
               }
            }
            else
            {
               // Normal call
               // ((<fragment type>)fragmentInstance.Fragment).<fragment generated method>(<args>);
               //               LocalBuilder[] dummies = new LocalBuilder[parameters.Count];
               foreach ( var pInfo in parameters )
               {
                  if ( !storeResult && pInfo.ParameterType.IsByRef() )
                  {
                     var pName = GetDummyParameterName( fragmentMethodModel.CompositeMethod.MethodIndex, pInfo );
                     LocalBuilder dummyB;
                     if ( !methodGenInfo.HasLocalRaw( pName ) )
                     {
                        dummyB = il.DeclareLocal( pInfo.ParameterType.GetElementType() );
                        methodGenInfo.AddLocalRaw( pName, dummyB );
                     }
                     else
                     {
                        dummyB = methodGenInfo.GetLocalRaw( pName );
                     }
                     // dummy = argsForSideEffects[<idx>];
                     var argsArrayB = thisMethodGenInfo.GetLocalOrThrow( LB_ARGS_ARRAY_FOR_SIDE_EFFECTS );
                     il.EmitLoadLocal( argsArrayB )
                       .EmitLoadInt32( pInfo.Position )
                       .EmitLoadElement( argsArrayB.LocalType.GetElementType() )
                       .EmitCastToType( argsArrayB.LocalType.GetElementType(), dummyB.LocalType )
                        //this.EmitLoadArgumentForMethodCall( il, pInfo, methodGenInfo.ParamTypes[pInfo.Position - 1] );
                       .EmitStoreLocal( dummyB );
                  }
               }

               il.EmitLoadLocal( fInstanceB )
                 .EmitCall( FRAGMENT_GETTER )
                 .EmitCastToType( FRAGMENT_GETTER.ReturnParameter.ParameterType, fragmentType );
               foreach ( var pInfo in methodGenInfo.Parameters )
               {
                  // Either load local or arg
                  var pName = GetDummyParameterName( fragmentMethodModel.CompositeMethod.MethodIndex, pInfo );
                  if ( methodGenInfo.HasLocalRaw( pName ) )
                  {
                     il.EmitLoadLocalAddress( methodGenInfo.GetLocalRaw( pName ) );
                  }
                  else
                  {
                     il.EmitLoadArg( pInfo );
                  }
               }
               il.EmitCall( methodToCall );
               if ( resultB != null )
               {
                  il.EmitStoreLocal( resultB );
               }
            }
         }
         else
         {
            actualCallAction( fragmentType, methodToCall, fragmentGenInfo );
         }
      }

      protected virtual void EmitCreateFragmentMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeGenerationInfo publicCompositeGenerationInfo,
         Int32 methodID,
         FragmentTypeGenerationInfo fragmentGenerationInfo,
         CompositeEmittingInfo emittingInfo
         )
      {
         var mb = publicCompositeGenerationInfo.Builder.AddMethod(
            CREATE_FRAGMENT_METHOD_PREFIX + fragmentGenerationInfo.Builder.Name,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, // TODO MethodAttributes.Assembly when [InternalsVisibleTo] attribute will be applied to all generated assemblies.
            CallingConventions.Standard );
         var thisMethodGenInfo = new CompositeMethodGenerationInfoImpl(
            mb,
            null,
            null );
         var cit = codeGenerationInfo.CompositeInstanceFieldType.NewWrapperAsType( this.ctx );
         mb.AddParameter( CREATE_FRAGMENT_METHOD_INSTANCE_PARAM_NAME, ParameterAttributes.None, cit );
         mb.AddParameter( CREATE_FRAGMENT_METHOD_METHOD_INDEX_PARAM_NAME, ParameterAttributes.None, INT32_TYPE );
         mb.AddParameter( CREATE_FRAGMENT_METHOD_CONCERN_INDEX_PARAM_NAME, ParameterAttributes.None, INT32_TYPE );
         mb.AddParameter( CREATE_FRAGMENT_METHOD_METHOD_RESULT_PARAM_NAME, ParameterAttributes.None, OBJECT_TYPE );
         mb.AddParameter( CREATE_FRAGMENT_METHOD_METHOD_EXCEPTION_PARAM_NAME, ParameterAttributes.None, EXCEPTION_TYPE );
         mb.ReturnParameter.ParameterType = LB_F_INSTANCE.Type.NewWrapper( this.ctx );

         var instanceB = thisMethodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, cit );
         var fInstanceB = thisMethodGenInfo.GetOrCreateLocal( LB_F_INSTANCE );

         var il = thisMethodGenInfo.IL;
         il
            .EmitLoadArg( 0 )
            .EmitStoreLocal( instanceB );

         this.EmitCreateFragment(
            codeGenerationInfo,
            compositeModel,
            ( publicCompositeGenerationInfo.GenericArguments.Any() ? fragmentGenerationInfo.Builder.MakeGenericType( publicCompositeGenerationInfo.GenericArguments.ToArray() ) : fragmentGenerationInfo.Builder ),
            publicCompositeGenerationInfo,
            publicCompositeGenerationInfo,
            thisMethodGenInfo,
            methodID,
            fragmentGenerationInfo );

         il
            .EmitLoadLocal( fInstanceB )
            .EmitReturn();

         if ( !emittingInfo.FragmentCreationMethods.TryAdd( fragmentGenerationInfo, thisMethodGenInfo ) )
         {
            throw new InternalException( "Method for creating fragment generation info " + fragmentGenerationInfo + " already emitted?" );
         }
      }

      protected virtual void EmitCreateFragment(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel instanceableModel,
         CILType resolvedFragmentType,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenerationInfo,
         CompositeMethodGenerationInfo thisCompositeMethodGenerationInfo,
         Int32 methodID,
         FragmentTypeGenerationInfo fragmentGenerationInfo
         )
      {
         var il = thisCompositeMethodGenerationInfo.IL;
         var instanceB = thisCompositeMethodGenerationInfo.GetLocalOrThrow( LB_C_INSTANCE );
         var fInstanceB = thisCompositeMethodGenerationInfo.GetLocalOrThrow( LB_F_INSTANCE );
         var ctorIndexB = thisCompositeMethodGenerationInfo.GetOrCreateLocal( LB_INDEX );

         // Int32 ctorIndex = instance.GetConstructorIndex(<resolvedFragmentType>);
         il.EmitLoadLocal( instanceB )
           .EmitReflectionObjectOf( resolvedFragmentType )
           .EmitCall( GET_CTOR_INDEX_METHOD )
           .EmitStoreLocal( ctorIndexB );

         // switch constructor model index
         var labelsForDefaultCase = new LinkedList<ILLabel>();
         il.EmitLoadLocal( ctorIndexB )
           .EmitSwitch(
            instanceableModel.Constructors.Count,
            ( il2, jumpTable, defaultCaseLabel, switchEndLabel ) =>
            {
               foreach ( var ctorModel in instanceableModel.Constructors )
               {
                  var ctorArgsB = thisCompositeMethodGenerationInfo.GetOrCreateLocal( LB_ARGS_ARRAY );
                  var idx = ctorModel.ConstructorIndex;
                  var ctorDeclaringType = ctorModel.NativeInfo.DeclaringType.NewWrapperAsType( this.ctx );
                  var thisCaseLabel = jumpTable[idx];
                  if ( fragmentGenerationInfo.DirectBaseFromModel.Equals( ctorDeclaringType )
                      || ( fragmentGenerationInfo.DirectBaseFromModel.ContainsGenericParameters() && ctorDeclaringType.ContainsGenericParameters() && Object.Equals( fragmentGenerationInfo.DirectBaseFromModel.GenericDefinition, ctorDeclaringType.GenericDefinition ) ) )
                  {
                     // Mark this label as with case: -statement
                     il2.MarkLabel( thisCaseLabel );
                     var ctorGenInfo = fragmentGenerationInfo.ConstructorBuilders[idx];
                     var ctor = ctorGenInfo.Builder.ChangeDeclaringType( resolvedFragmentType.GenericArguments.ToArray() );

                     // Create FragmentInstance
                     var ctorParamModels = ctorModel.Parameters.ToArray();
                     CILConstructor fInstanceCtor;
                     if ( ctorParamModels.Any( ctorParamModel => ctorParamModel.IsFragmentDependant() ) )
                     {
                        fInstanceCtor = FRAGMENT_INSTANCE_CTOR_WITH_PARAMS;
                        il2.EmitLoadArg( 1 )
                           .EmitLoadArg( 2 )
                           .EmitLoadArg( 3 )
                           .EmitLoadArg( 4 );
                     }
                     else
                     {
                        fInstanceCtor = FRAGMENT_INSTANCE_CTOR_NO_PARAMS;
                     }
                     il2.EmitNewObject( fInstanceCtor )
                        .EmitStoreLocal( fInstanceB );

                     // Process parameters
                     if ( ctorParamModels.Any() )
                     {
                        // Object[] args = new Object[];
                        il2.EmitLoadInt32( ctorParamModels.Length )
                           .EmitNewArray( ctorArgsB.LocalType.GetElementType() )
                           .EmitStoreLocal( ctorArgsB );
                        var tempStorageB = thisCompositeMethodGenerationInfo.GetOrCreateLocal( LB_TEMP_STORAGE );

                        foreach ( var pModel in ctorParamModels )
                        {
                           // inject and process constraints for temp local
                           var paramIdx = pModel.NativeInfo.Position;

                           this.EmitProcessParameter(
                              codeGenerationInfo,
                              pModel,
                              publicCompositeGenInfo,
                              thisGenerationInfo,
                              thisCompositeMethodGenerationInfo,
                              tempStorageB,
                              il3 => il3
                                 .EmitLoadLocal( instanceB )
                                 .EmitCall( MODEL_INFO_GETTER )
                                 .EmitCall( MODEL_GETTER )
                                 .EmitCall( MODEL_CTORS_GETTER )
                                 .EmitLoadInt32( ctorModel.ConstructorIndex )
                                 .EmitCall( ChangeQueryItemGetterDeclaringTypeGArgs( CONSTRUCTOR_MODEL_TYPE ) )
                                 .EmitCall( COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER )
                                 .EmitLoadInt32( paramIdx )
                                 .EmitCall( ChangeQueryItemGetterDeclaringTypeGArgs( PARAMETER_MODEL_TYPE ) ),
                              true,
                              fragmentGenerationInfo.ConstructorBuilders[idx].Parameters[paramIdx + 1].ParameterType,
                              CheckFieldOrCtorParamInjection,
                              EmitInjectionLambdaClassFragmentInstanceField,
                              EmitSetupInjectionLambdaForFragmentInstance
                              );

                           // args[<idx>] = temp;
                           il2.EmitLoadLocal( ctorArgsB )
                              .EmitLoadInt32( paramIdx )
                              .EmitLoadLocal( tempStorageB )
                              .EmitStoreElement( ctorArgsB.LocalType.GetElementType() );
                        }
                     }

                     // fragmentInstance.Fragment = new <fragment>;
                     // First argument is always composite instance
                     il2.EmitLoadLocal( fInstanceB )
                        .EmitLoadLocal( instanceB );

                     // Then load arguments from arg-array
                     this.EmitLoadParametersFromObjectArray( thisGenerationInfo, thisCompositeMethodGenerationInfo, ctorGenInfo, methodID, 1, il, () => il.EmitLoadLocal( ctorArgsB ) );

                     // Then call the constructor
                     il2.EmitNewObject( ctor )

                     // Call the constructor of fragment instance
                        .EmitCall( FRAGMENT_SETTER );

                     foreach ( var fieldModel in this.FindFieldsWithInjectionTime( instanceableModel, fragmentGenerationInfo, InjectionTime.ON_CREATION ) )
                     {
                        this.EmitInjectToField(
                           codeGenerationInfo,
                           fieldModel,
                           publicCompositeGenInfo,
                           thisGenerationInfo,
                           thisCompositeMethodGenerationInfo,
                           fragmentGenerationInfo,
                           EmitInjectionLambdaClassFragmentInstanceField,
                           EmitSetupInjectionLambdaForFragmentInstance
                           );
                     }

                     // Branch to end switch
                     il2.EmitBranch( BranchType.ALWAYS, switchEndLabel );
                  }
                  else
                  {
                     // Mark this label as default case
                     labelsForDefaultCase.AddLast( thisCaseLabel );
                  }
               }
            },
            ( il2, switchEndLabel ) => il2
               .MarkLabels( labelsForDefaultCase )
               .EmitLoadString( "The constructor index " )
               .EmitLoadLocal( ctorIndexB )
               .EmitCastToType( ctorIndexB.LocalType, OBJECT_TYPE )
               .EmitLoadString( " was invalid for fragment type " )
               .EmitCall( STRING_CONCAT_METHOD_3 )
               .EmitReflectionObjectOf( resolvedFragmentType )
               .EmitCall( BASE_TYPE_GETTER )
               .EmitLoadString( "." )
               .EmitCall( STRING_CONCAT_METHOD_3 )
               .EmitThrowNewException( INTERNAL_EXCEPTION_CTOR )
            );
      }

      protected IList<CILField> EmitInjectionLambdaClassFragmentInstanceField( CILType tb )
      {
         return new List<CILField>( Enumerable.Repeat( tb.AddField( LB_F_INSTANCE.Name, FRAGMENT_GETTER.DeclaringType, FieldAttributes.Public ), 1 ) );
      }

      protected static void EmitSetupInjectionLambdaForFragmentInstance( MethodGenerationInfo methodGenInfo, LocalBuilder lambdaB, IList<CILField> fields )
      {
         methodGenInfo.IL
            .EmitLoadLocal( lambdaB )
            .EmitStoreOtherField( fields[0], il2 => il2.EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_F_INSTANCE ) ) );
      }

      protected void EmitSetupFragmentDependentInjection(
         AbstractInjectableModel injectableModel,
         MethodGenerationInfo methodGenInfo,
         CILTypeBase actualType,
         LocalBuilder injectionResultLocal,
         IList<CILField> lambdaAdditionalFields
         )
      {
         if ( injectableModel.IsFragmentDependant() )
         {
            var il3 = methodGenInfo.IL;
            if ( lambdaAdditionalFields != null )
            {
               il3
                  .EmitLoadLocal( injectionResultLocal )
                  .EmitCastToType( injectionResultLocal.LocalType, FRAGMENT_DEPENDANT_SETTER.DeclaringType )
                  .EmitLoadThisField( lambdaAdditionalFields[0] )
                  .EmitCall( FRAGMENT_DEPENDANT_SETTER );
            }
            else
            {
               il3.EmitDup()
                  .EmitCastToType( actualType, FRAGMENT_DEPENDANT_PROPERTY.DeclaringType )
                  .EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_F_INSTANCE ) )
                  .EmitCall( FRAGMENT_DEPENDANT_SETTER );
            }
         }
      }

      protected virtual void EmitCallFragmentSpecialMethods<AttributeType>(
         CompositeModel instanceableModel,
         CILType resolvedFragmentType,
         FragmentTypeGenerationInfo fragmentGenerationInfo,
         CompositeMethodGenerationInfo thisCompositeMethodGenerationInfo,
         Boolean emitCatch
         )
         where AttributeType : Attribute
      {
         var il = thisCompositeMethodGenerationInfo.IL;
         var fInstanceB = thisCompositeMethodGenerationInfo.GetLocalOrThrow( LB_F_INSTANCE );
         foreach ( var sMethodModel in this.GetSpecialMethods<AttributeType>( instanceableModel, fragmentGenerationInfo ).OrderBy( model => model.NativeInfo.DeclaringType, BASE_TYPE_COMPARER ) )
         {
            var sMethod = TypeGenerationUtils.GetMethodForEmitting( fragmentGenerationInfo.Parents, fragmentGenerationInfo.SpecialMethodBuilders[sMethodModel.NativeInfo].Builder );
            // try
            // {
            //   ((<fragmentType>)fragmentInstance.Fragment).<SpecialMethod>();
            // } catch(Exception exc)
            // {
            //   <create exception list if needed, and add exception>
            // }
            il.EmitTryCatch(
               EXCEPTION_TYPE,
               il2 => il2
                   .EmitLoadLocal( fInstanceB )
                  .EmitCall( FRAGMENT_GETTER )
                  .EmitCastToType( FRAGMENT_GETTER.ReturnParameter.ParameterType, resolvedFragmentType )
                  .EmitCall( sMethod ),
                  emitCatch ? il2 => this.EmitStoreExceptionListWithinCatch( thisCompositeMethodGenerationInfo ) : (Action<MethodIL>) null,
               false
               );
         }
      }

      protected virtual CompositeMethodGenerationInfo EmitFragmentSpecialMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         SpecialMethodModel specialMethodModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         FragmentTypeGenerationInfo thisGenInfo
         )
      {
         var specialMethod = TypeGenerationUtils.GetMethodForEmitting( thisGenInfo.Parents, specialMethodModel.NativeInfo.NewWrapper( this.ctx ) );

         // Create a new method with different name, calling base.OriginalName(args) with injected parameters.
         var mb = new CompositeMethodGenerationInfoImpl(
            thisGenInfo.Builder.AddMethod(
             SPECIAL_METHOD_PREFIX + specialMethodModel.SpecialMethodIndex,
             MethodAttributes.Public | MethodAttributes.HideBySig,
             specialMethod.CallingConvention ),
             null,
             null ).WithReturnType( VOID_TYPE ) as CompositeMethodGenerationInfo;

         var parameters = specialMethod.Parameters;
         var il = mb.IL;

         var argBuilders = new LocalBuilder[parameters.Count];

         // Inject the parameters
         if ( parameters.Count > 0 )
         {
            // CompositeInstance instance = this._instance;
            var instanceB = mb.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );
            il.EmitLoadThisField( thisGenInfo.CompositeField )
              .EmitStoreLocal( instanceB );

            // SpecialMethodModel specialMethodModel = instance.ModelInfo.Model.SpecialMethods[<idx>];
            var sMethodB = mb.GetOrCreateLocal( LB_SPECIAL_METHOD_MODEL );
            il.EmitLoadLocal( instanceB )
              .EmitCall( MODEL_INFO_GETTER )
              .EmitCall( MODEL_GETTER )
              .EmitCall( SPECIAL_METHODS_GETTER )
              .EmitLoadInt32( specialMethodModel.SpecialMethodIndex )
              .EmitCall( ChangeQueryItemGetterDeclaringTypeGArgs( SPECIAL_METHOD_MODEL_TYPE ) )
              .EmitStoreLocal( sMethodB );

            for ( Int32 paramIdx = 0; paramIdx < specialMethodModel.Parameters.Count(); ++paramIdx )
            {
               var paramModel = specialMethodModel.Parameters.ElementAt( paramIdx );
               var paramType = TypeGenerationUtils.CreateTypeForEmitting( parameters[paramIdx].ParameterType, thisGenInfo.GenericArguments, mb.GenericArguments );
               var argBuilder = il.DeclareLocal( paramType );
               argBuilders[paramIdx] = argBuilder;

               // injectionProviderContext = instance.Application.InjectionService.CreateProviderContext(specialMethodModel.Parameters[idx].Injections[0], type-of(<paramType>));
               this.EmitProcessParameter(
                  codeGenerationInfo,
                  paramModel,
                  publicCompositeGenInfo,
                  thisGenInfo,
                  mb,
                  argBuilder,
                  il2 => il2
                     .EmitLoadLocal( sMethodB )
                     .EmitCall( COMPOSITE_METHOD_MODEL_PARAMETERS_GETTER )
                     .EmitLoadInt32( paramIdx )
                     .EmitCall( ChangeQueryItemGetterDeclaringTypeGArgs( PARAMETER_MODEL_TYPE ) ),
                  true );
            }
            // Throw if violations
            this.EmitThrowIfViolations( thisGenInfo, mb, specialMethodModel );
         }

         // Call the actual method
         // base.<methodName>(<parameters>);
         il.EmitLoadArg( 0 );

         // Load args for method call
         for ( Int32 paramIdx = 0; paramIdx < parameters.Count(); ++paramIdx )
         {
            if ( parameters[paramIdx].Attributes.IsOut() )
            {
               il.EmitLoadLocalAddress( argBuilders[paramIdx] );
            }
            else
            {
               il.EmitLoadLocal( argBuilders[paramIdx] );
            }
         }

         il.EmitCall( specialMethod );
         if ( !VOID_TYPE.Equals( specialMethod.GetReturnType() ) )
         {
            il.EmitPop();
         }
         il.EmitReturn();

         return mb;
      }

      protected virtual CILTypeBase[] ResolveGArgsForParent(
         CompositeTypeModel tModel,
         CILTypeParameter[] gBuilders,
         ListQuery<AbstractGenericTypeBinding> bindings,
         Boolean returnNullIfAnyIndirects = false
         )
      {
         var result = bindings.Select( binding => this.ResolveGArgForParent( tModel, gBuilders, binding, returnNullIfAnyIndirects ) ).ToArray();
         return result.Any( arg => arg == null ) ? null : result;
      }

      protected virtual CILTypeBase ResolveGArgForParent(
         CompositeTypeModel tModel,
         CILTypeParameter[] gBuilders,
         AbstractGenericTypeBinding binding,
         Boolean returnNullIfAnyIndirects
         )
      {
         CILTypeBase result = null;
         if ( binding is DirectGenericTypeBinding )
         {
            DirectGenericTypeBinding dBinding = (DirectGenericTypeBinding) binding;
            result = dBinding.TypeOrGenericDefinition.NewWrapper( this.ctx );
            if ( result.ContainsGenericParameters() )
            {
               result = ( (CILType) result ).MakeGenericType( dBinding.InnerBindings.Select( inner => this.ResolveGArgForParent( tModel, gBuilders, inner, returnNullIfAnyIndirects ) ).ToArray() );
            }
         }
         else if ( !returnNullIfAnyIndirects )
         {
            IndirectGenericTypeBinding iBinding = (IndirectGenericTypeBinding) binding;
            result = gBuilders[tModel.PublicCompositeGenericArguments.Select( ( arg, i ) => Tuple.Create( arg, i ) ).First( tuple => tuple.Item1.DeclaringType == iBinding.GenericDefinition && tuple.Item1.GenericParameterPosition == iBinding.GenericIndex ).Item2];
         }
         return result;
      }

      protected virtual CILField EmitInstanceField( CompositeCodeGenerationInfo codeGenerationInfo, CompositeModel model, CILType tb )
      {
         return tb.AddField( codeGenerationInfo.CompositeInstanceFieldName, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ), FieldAttributes.InitOnly | FieldAttributes.Private );
      }

      protected virtual void EmitCompositeConstructor(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeEmittingInfo emittingInfo,
         CompositeTypeGenerationInfo thisGenerationInfo,
         Boolean isPublicCompositeCtor
         )
      {
         var fragmentGenerationInfos = emittingInfo.FragmentTypeGenerationInfos.Values.SelectMany( info => info.Item1 );
         var ctorParamTypes = new CILType[] { codeGenerationInfo.CompositeInstanceFieldType.NewWrapperAsType( this.ctx ), COMPOSITE_CTOR_PROPERTIES_PARAM_TYPE, COMPOSITE_CTOR_EVENTS_PARAM_TYPE };
         Int32 firstAdditionalParamIndex = ctorParamTypes.Length;
         if ( isPublicCompositeCtor )
         {
            ctorParamTypes = ctorParamTypes.Concat( this.GetPublicCompositeAdditionalArguments( thisGenerationInfo ) ).ToArray();
         }

         var baseCtor = thisGenerationInfo.Builder.BaseType.Constructors.Single( ctor => !ctor.Attributes.IsStatic() && ctor.Parameters.Count == 0 );
         //var baseCtor = TypeGenerationUtils.GetMethodForEmitting( thisGenerationInfo.Parents, ctorFromModel );

         CompositeConstructorGenerationInfo cInfo = new CompositeConstructorGenerationInfoImpl(
            MethodAttributes.Public,
            CallingConventions.Standard | CallingConventions.HasThis,
            ctorParamTypes.Select( ct => Tuple.Create( (CILTypeBase) ct, ParameterAttributes.None, "" ) ),
            thisGenerationInfo,
            null,//ctorFromModel,
            baseCtor );

         var propertiesB = cInfo.Parameters[1];
         var eventsB = cInfo.Parameters[2];

         var il = cInfo.IL;

         // Load 'this' and call base constructor (Object)
         il.EmitLoadArg( 0 );
         il.EmitCall( baseCtor );

         // this._instance = <first parameter>
         il.EmitStoreThisField( thisGenerationInfo.CompositeField,
            il2 => il2.EmitLoadArg( 1 )
            );

         // Emit composite property creation code
         this.EmitCompositePropertyCreation(
            thisGenerationInfo,
            propertiesB,
            COMPOSITE_CTOR_PROPERTIES_PARAM_TYPE,
            il
            );

         // Emit composite event addition code
         this.EmitCompositeEventCreation(
            thisGenerationInfo,
            eventsB,
            COMPOSITE_CTOR_EVENTS_PARAM_TYPE,
            il
            );

         if ( isPublicCompositeCtor )
         {
            this.EmitTheRestOfPublicCompositeConstructor( codeGenerationInfo, compositeModel, typeModel, emittingInfo, fragmentGenerationInfos, thisGenerationInfo, cInfo, firstAdditionalParamIndex );
         }

         // Constructor ends
         il.EmitReturn();
      }

      protected virtual void EmitTheRestOfPublicCompositeConstructor(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel model,
         CompositeTypeModel typeModel,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos,
         CompositeTypeGenerationInfo thisGenerationInfo,
         ConstructorGenerationInfo ctorGenerationInfo,
         Int32 firstAdditionalParamIndex
         )
      {

         this.EmitSetPrePrototypeMethod(
            codeGenerationInfo,
            thisGenerationInfo,
            ctorGenerationInfo,
            emittingInfo.AllCompositeGenerationInfos,
            SET_DEFAULTS_METHOD_NAME,
            firstAdditionalParamIndex );

         this.EmitSetActionMethod<PrototypeAttribute>(
            codeGenerationInfo,
            model,
            typeModel,
            fragmentGenerationInfos,
            thisGenerationInfo,
            thisGenerationInfo,
            emittingInfo,
            ctorGenerationInfo,
            firstAdditionalParamIndex + 1,
            PROTOTYPE_METHOD_NAME,
            false
            );

         this.EmitSetCheckStateMethod(
            codeGenerationInfo,
            thisGenerationInfo,
            ctorGenerationInfo,
            emittingInfo.AllCompositeGenerationInfos,
            CHECK_STATE_METHOD_NAME,
            firstAdditionalParamIndex + 2 );
      }

      protected virtual void EmitSetPrePrototypeMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         ConstructorGenerationInfo ctorGenerationInfo,
         IEnumerable<CompositeTypeGenerationInfo> compositeGenInfos,
         String methodName,
         Int32 paramIndex
         )
      {
         var suitableGenInfos = compositeGenInfos.Where( genInfo => genInfo.PrePrototypeMethod != null );
         if ( suitableGenInfos.Any() )
         {
            var genParams = TypeGenerationUtils.ImplementMethodForEmitting(
               thisGenInfo,
               SET_DEFAULTS_METHOD_SIG,
               methodName + PUBLIC_COMPOSITE_METHOD_INVOKER_SUFFIX,
               MethodAttributes.Public | MethodAttributes.HideBySig // TODO MethodAttributes.Assembly when [InternalsVisibleTo] applied to all generated assemblies
            );

            var methodGenInfo = new CompositeMethodGenerationInfoImpl( genParams.Item1, SET_DEFAULTS_METHOD_SIG, genParams.Item2 );
            var il = methodGenInfo.IL;

            var cInstanceB = methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );

            // instance = this._instance;
            il.EmitLoadThisField( thisGenInfo.CompositeField )
              .EmitStoreLocal( cInstanceB );

            var thisGArgs = thisGenInfo.GenericArguments.ToArray();
            if ( !thisGArgs.Any() )
            {
               thisGArgs = null;
            }

            foreach ( var genInfo in suitableGenInfos )
            {
               il
                  .EmitLoadLocal( cInstanceB )
                  .EmitCall( COMPOSITES_GETTER )
                  .EmitReflectionObjectOf( thisGArgs == null ? genInfo.Builder : genInfo.Builder.MakeGenericType( thisGArgs ), false )
                  .EmitCall( COMPOSITES_GETTER_INDEXER )
                  .EmitCastToType( COMPOSITES_GETTER_INDEXER.GetReturnType(), genInfo.Builder )
                  .EmitCall( genInfo.PrePrototypeMethod.Builder );
            }

            il.EmitReturn();

            ctorGenerationInfo.IL.EmitStoreToArgument( ctorGenerationInfo.Parameters[paramIndex], il2 =>
            {
               il2.EmitLoadArg( 0 )
                  .EmitLoadUnmanagedMethodToken( methodGenInfo.Builder )
                  .EmitNewObject( ACTION_0_CTOR );
            } );
         }
      }

      protected virtual void EmitPrePrototypeMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel model,
         CompositeTypeGenerationInfo thisGenerationInfo,
         String methodName
         )
      {
         var requiredGenProps = thisGenerationInfo.AutoGeneratedPropertyInfos.Values
            .Where( genProp => !this.CanEmitDefaultValueForPropertyModel( genProp.PropertyModel ) && !genProp.PropertyModel.NativeInfo.PropertyType.IsEnum && ( genProp.PropertyModel.NativeInfo.PropertyType.IsValueType || genProp.PropertyModel.NativeInfo.PropertyType.IsGenericParameter ) && !genProp.PropertyField.FieldType.Equals( genProp.PropertyType ) )
            .ToArray();
         var modelsWithDefaults = model.Methods
            .Where( method => method.PropertyModel != null && method.PropertyModel.IsPartOfCompositeState() )
            .Select( method => method.PropertyModel )
            .Distinct()
            .Where( propModel => thisGenerationInfo.AutoGeneratedPropertyInfos.ContainsKey( propModel.NativeInfo ) && this.CanEmitDefaultValueForPropertyModel( propModel ) )
            .ToArray();

         if ( requiredGenProps.Any() || modelsWithDefaults.Any() )
         {
            var genParams = TypeGenerationUtils.ImplementMethodForEmitting(
               thisGenerationInfo,
               SET_DEFAULTS_METHOD_SIG,
               methodName,
               MethodAttributes.Public | MethodAttributes.HideBySig // TODO MethodAttributes.Assembly when [InternalsVisibleTo] applied to all generated assemblies
            );

            var methodGenInfo = new CompositeMethodGenerationInfoImpl( genParams.Item1, SET_DEFAULTS_METHOD_SIG, genParams.Item2 );
            var il = methodGenInfo.IL;

            var cInstanceB = methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );

            // instance = this._instance;
            il.EmitLoadThisField( thisGenerationInfo.CompositeField )
              .EmitStoreLocal( cInstanceB );

            foreach ( var genProp in requiredGenProps )
            {
               //               var propertyDeclaringType = TypeGenerationUtils.CreateTypeForEmitting( thisGenerationInfo.Builder, thisGenerationInfo.GenericArguments, null );
               var genPropType = genProp.PropertyType;

               il.EmitLoadArg( 0 )
                  //.EmitLoadLocal( cInstanceB )
                  //.EmitCall( COMPOSITES_GETTER )
                  //.EmitReflectionObjectOf( thisGenerationInfo.Parents[genProp.PropertyModel.NativeInfo.DeclaringType] )
                  //.EmitCall( COMPOSITES_GETTER_INDEXER )
                  //.EmitCastToType( COMPOSITES_GETTER_INDEXER.ReturnType, propertyDeclaringType )
                    .EmitLoadDefault( genPropType, aType => methodGenInfo.GetOrCreateLocalBasedOnType( aType ) )
                    .EmitCall( genProp.SetMethod );
            }

            foreach ( var modelWithDefault in modelsWithDefaults )
            {
               //               var propertyDeclaringType = TypeGenerationUtils.CreateTypeForEmitting( thisGenerationInfo.Builder, thisGenerationInfo.GenericArguments, null );
               il.EmitLoadArg( 0 );
               //.EmitLoadLocal( cInstanceB )
               //.EmitCall( COMPOSITES_GETTER )
               //.EmitReflectionObjectOf( thisGenerationInfo.Parents[modelWithDefault.NativeInfo.DeclaringType] )
               //.EmitCall( COMPOSITES_GETTER_INDEXER )
               //.EmitCastToType( COMPOSITES_GETTER_INDEXER.ReturnType, propertyDeclaringType );
               this.EmitLoadDefaultValueForPropertyModel( modelWithDefault, thisGenerationInfo, methodGenInfo );
               il.EmitCall( thisGenerationInfo.AutoGeneratedPropertyInfos[modelWithDefault.NativeInfo].SetMethod );
            }
            il.EmitReturn();

            thisGenerationInfo.PrePrototypeMethod = methodGenInfo;
         }
      }

      protected virtual void EmitSetCheckStateMethod(
         CompositeCodeGenerationInfo codeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         ConstructorGenerationInfo ctorGenerationInfo,
         IEnumerable<CompositeTypeGenerationInfo> compositeGenInfos,
         String methodName,
         Int32 paramIndex
         )
      {
         var suitableGenInfos = compositeGenInfos.Where( genInfo => genInfo.CheckStateMethod != null );
         if ( suitableGenInfos.Any() )
         {
            var genParams = TypeGenerationUtils.ImplementMethodForEmitting(
               thisGenInfo,
               CHECK_STATE_METHOD_SIG,
               methodName + PUBLIC_COMPOSITE_METHOD_INVOKER_SUFFIX,
               MethodAttributes.Private | MethodAttributes.HideBySig
            );
            var methodGenInfo = new CompositeMethodGenerationInfoImpl( genParams.Item1, CHECK_STATE_METHOD_SIG, genParams.Item2 );
            var il = methodGenInfo.IL;

            var cInstanceB = methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );

            il.EmitLoadThisField( thisGenInfo.CompositeField )
               .EmitStoreLocal( cInstanceB );

            var thisGArgs = thisGenInfo.GenericArguments.ToArray();
            if ( !thisGArgs.Any() )
            {
               thisGArgs = null;
            }

            foreach ( var genInfo in suitableGenInfos )
            {
               il
                  .EmitLoadLocal( cInstanceB )
                  .EmitCall( COMPOSITES_GETTER )
                  .EmitReflectionObjectOf( thisGArgs == null ? genInfo.Builder : genInfo.Builder.MakeGenericType( thisGArgs ), false )
                  .EmitCall( COMPOSITES_GETTER_INDEXER )
                  .EmitCastToType( COMPOSITES_GETTER_INDEXER.GetReturnType(), genInfo.Builder )
                  .EmitLoadArg( 1 )
                  .EmitCall( genInfo.CheckStateMethod.Builder );
            }
            il.EmitReturn();

            ctorGenerationInfo.IL.EmitStoreToArgument( ctorGenerationInfo.Parameters[paramIndex], il2 =>
            {
               il2.EmitLoadArg( 0 )
                  .EmitLoadUnmanagedMethodToken( methodGenInfo.Builder )
                  .EmitNewObject( CHECK_ACTION_FUNC_CTOR );
            } );
         }
      }

      protected virtual void EmitCheckStateMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel model,
         CompositeTypeGenerationInfo thisGenerationInfo,
         String methodName
         )
      {
         if ( thisGenerationInfo.AutoGeneratedPropertyInfos.Any() )
         {
            var genParams = TypeGenerationUtils.ImplementMethodForEmitting(
               thisGenerationInfo,
               CHECK_STATE_METHOD_SIG,
               methodName,
               MethodAttributes.Public | MethodAttributes.HideBySig // TODO MethodAttributes.Assembly when [InternalsVisibleTo] is applied to all generated assemblies.
               );
            var methodGenInfo = new CompositeMethodGenerationInfoImpl( genParams.Item1, CHECK_STATE_METHOD_SIG, genParams.Item2 );
            var il = methodGenInfo.IL;

            var cInstanceB = methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );

            // instance = this._instance;
            il.EmitLoadThisField( thisGenerationInfo.CompositeField )
              .EmitStoreLocal( cInstanceB );

            var constraintExceptionB = methodGenInfo.GetOrCreateLocal( LB_CONSTRAINT_VIOLATION_EXCEPTION );

            foreach ( var item in thisGenerationInfo.AutoGeneratedPropertyInfos )
            {
               // try
               // {
               //    this.<getter>();
               // } catch (ConstraintViolationException exc)
               // {
               //    if ( constrationViolations == null )
               //    {
               //       constraintViolations = new Dictionary<QualifiedName, IList<ConstraintViolationInfo>>();
               //    }
               //    constraintViolations.Add(QualifiedName.FromMemberInfo(<info-of>), exc.Violations);
               // }
               var propertyDeclaringType = thisGenerationInfo.Parents[item.Key.DeclaringType.NewWrapperAsType( this.ctx )];
               il.EmitTryCatch(
               CONSTRAINT_VIOLATION_EXCEPTION_CTOR.DeclaringType,
               il2 => il2
                  .EmitLoadArg( 0 )
                  //.EmitLoadLocal( cInstanceB )
                  //.EmitCall( COMPOSITES_GETTER )
                  //.EmitReflectionObjectOf( propertyDeclaringType )
                  //.EmitCall( COMPOSITES_GETTER_INDEXER )
                  //.EmitCastToType( COMPOSITES_GETTER_INDEXER.ReturnType, propertyDeclaringType )
                  .EmitCall( TypeGenerationUtils.GetMethodForEmitting( thisGenerationInfo.Parents, item.Value.PropertyModel.GetterMethod.NativeInfo.NewWrapper( this.ctx ) ) )
                  .EmitPop(),
               il2 =>
               {
                  il2.EmitStoreLocal( constraintExceptionB )
                     //il2.EmitIf(
                     //   ( il3, endIfLabel ) => il3
                     //      .EmitLoadLocal( violationsDicB )
                     //      .EmitBranch( BranchType.IF_TRUE, endIfLabel ),
                     //   ( il3, endIfLabel ) => il3
                     //      .EmitNewObject( CONSTRAINT_VIOLATIONS_DIC_CTOR )
                     //      .EmitStoreLocal( violationsDicB )
                     //   );
                     .EmitLoadArgumentForMethodCall( methodGenInfo.Parameters[0] )
                     .EmitReflectionObjectOf( propertyDeclaringType )
                     .EmitLoadString( item.Key.Name )
                     .EmitLoadInt32( (Int32) DEFAULT_SEARCH_BINDING_FLAGS )
                     .EmitCall( GET_PROPERTY_INFO_METHOD )
                     .EmitCall( Q_NAME_FROM_MEMBER_INFO_METHOD )
                     .EmitLoadLocal( constraintExceptionB )
                     .EmitCall( CONSTRAINT_EXCEPTION_VIOLATIONS_GETTER )
                     .EmitCall( CONSTRAINT_VIOLATIONS_DIC_ADD_METHOD );
               },
               false );
            }

            il.EmitReturn();

            thisGenerationInfo.CheckStateMethod = methodGenInfo;

            //il = ctorGenerationInfo.IL;
            //il.EmitStoreToArgument( ctorGenerationInfo.ParamBuilders[paramIndex], ctorGenerationInfo.ParamTypes[paramIndex], il2 => il2
            //   .EmitLoadArg( 0 )
            //   .EmitLoadUnmanagedMethodToken( methodGenInfo.Builder )
            //   .EmitNewObject( CHECK_ACTION_FUNC_CTOR )
            //);
         }

      }

      protected virtual IEnumerable<CILType> GetPublicCompositeAdditionalArguments( CompositeTypeGenerationInfo thisTypeGenInfo )
      {
         return PUBLIC_COMPOSITE_CTOR_ADDITTIONAL_PARAM_TYPES;
      }

      protected virtual Int32 GetAmountOfAdditionalArgumentsForPublicCompositeConstructor()
      {
         return PUBLIC_COMPOSITE_CTOR_ADDITTIONAL_PARAM_TYPES.Length;
      }

      protected virtual void EmitCompositePropertyCreation(
         CompositeTypeGenerationInfo thisGenerationInfo,
         CILParameter propertiesB,
         CILType propertiesBType,
         MethodIL il
         )
      {
         foreach ( var kvp in thisGenerationInfo.AutoGeneratedPropertyInfos )
         {
            // properties.Add( new CompositeProperty(
            //  instance.ModelInfo.Model.CompositeMethods[<idx>].PropertyModel,
            //  type-of(<declaringType>).GetProperty(<name>),
            //  this.<propertyGetterMethod>,
            //  this.<propertySetterMethod>,
            //  this.<propertyExchangeMethod>,
            //  this.<propertyCompareExchangeMethod>
            //  ));

            // Load argument for storing
            il.EmitLoadArgumentForMethodCall( propertiesB )

            // 1st param
              .EmitLoadArg( 1 )
              .EmitCall( MODEL_INFO_GETTER )
              .EmitCall( MODEL_GETTER )
              .EmitCall( C_METHODS_GETTER )
              .EmitLoadInt32( kvp.Value.PropertyModel.GetterMethod.MethodIndex )
              .EmitCall( COMPOSITE_METHODS_INDEXER )
              .EmitCall( PROPERTY_MODEL_GETTER )

            // 2nd param
              .EmitReflectionObjectOf( thisGenerationInfo.Parents[kvp.Key.DeclaringType.NewWrapperAsType( this.ctx )] )
              .EmitLoadString( kvp.Key.Name )
              .EmitLoadInt32( (Int32) DEFAULT_SEARCH_BINDING_FLAGS )
              .EmitCall( GET_PROPERTY_INFO_METHOD )

            // 3rd param
               .EmitLoadArg( 0 )
               .EmitLoadUnmanagedMethodToken( kvp.Value.RefInvokerMethod )
               .EmitNewObject( REF_INVOKER_CALLBACK_CTOR );

            var propertyTypeGen = kvp.Value.PropertyType;

            // 4th param
            // If getter method for 32-bit systems is not null, we must give that if we are in 32bit process
            var needIf = kvp.Value.Get32Method != null;
            var delCtor = FUNC_1_CTOR.ChangeDeclaringType( propertyTypeGen );
            il.EmitIfElse( needIf ? new Action<MethodIL, ILLabel, ILLabel>(
               ( il2, elseLbl, endIfLbl ) =>
                  il2.EmitCall( INT_PTR_SIZE_GETTER )
                     .EmitLoadInt32( 4 )
                     .EmitCeq()
                     .EmitBranch( BranchType.IF_TRUE, elseLbl ) ) :
               null,
               ( il2, elseLbl, endIfLbl ) => il2.EmitLoadArg( 0 ).EmitLoadUnmanagedMethodToken( kvp.Value.GetMethod ).EmitNewObject( delCtor ),
               needIf ? new Action<MethodIL, ILLabel>( ( il2, endIfLbl ) => il2.EmitLoadArg( 0 ).EmitLoadUnmanagedMethodToken( kvp.Value.Get32Method ).EmitNewObject( delCtor ) ) : null
               );

            // 5th param
            il.EmitLoadArg( 0 )
              .EmitLoadUnmanagedMethodToken( kvp.Value.SetMethod )
              .EmitNewObject( ACTION_1_CTOR.ChangeDeclaringType( propertyTypeGen ) )

            // 6th param
              .EmitLoadArg( 0 )
              .EmitLoadUnmanagedMethodToken( kvp.Value.ExchangeMethod )
              .EmitNewObject( FUNC_2_CTOR.ChangeDeclaringType( propertyTypeGen, propertyTypeGen ) )

            // 7th param
              .EmitLoadArg( 0 )
              .EmitLoadUnmanagedMethodToken( kvp.Value.CompareExchangeMethod )
              .EmitNewObject( FUNC_3_CTOR.ChangeDeclaringType( propertyTypeGen, propertyTypeGen, propertyTypeGen ) )

            // Constructor
              .EmitNewObject( COMPOSITE_PROPERTY_CTOR.ChangeDeclaringType( propertyTypeGen ) )

            // Store composite property
              .EmitCall( COLLECTION_ADD_ONLY_ADD_METHOD.ChangeDeclaringType( propertiesBType.GetGenericArgumentsArray() ) );
         }
      }

      protected virtual void EmitCompositeEventCreation(
         CompositeTypeGenerationInfo thisGenerationInfo,
         CILParameter eventsB,
         CILType eventsBType,
         MethodIL il
         )
      {
         foreach ( var kvp in thisGenerationInfo.AutoGeneratedEventInfos )
         {
            // events[<idx>] = new CompositeEvent(
            //  instance.ModelInfo.Model.CompositeMethods[<idx>].EventModel,
            //  type-of(<declaringType>).GetEvent(<name>),
            //  this.<eventInvocationMethod>,
            //  this.<eventAdditionMethod>,
            //  this.<eventRemovalMethod>,
            //  this.<eventClearMethod>,
            //  this.<eventCheckerMethod>
            //  );

            // Load argument for storing
            il.EmitLoadArgumentForMethodCall( eventsB )

            // 1st param
              .EmitLoadArg( 1 )
              .EmitCall( MODEL_INFO_GETTER )
              .EmitCall( MODEL_GETTER )
              .EmitCall( C_METHODS_GETTER )
              .EmitLoadInt32( kvp.Value.EventModel.AddMethod.MethodIndex )
              .EmitCall( COMPOSITE_METHODS_INDEXER )
              .EmitCall( EVENT_MODEL_GETTER )

            // 2nd param
              .EmitReflectionObjectOf( thisGenerationInfo.Parents[kvp.Key.DeclaringType.NewWrapperAsType( this.ctx )] )
              .EmitLoadString( kvp.Key.Name )
              .EmitLoadInt32( (Int32) DEFAULT_SEARCH_BINDING_FLAGS )
              .EmitCall( GET_EVENT_INFO_METHOD )

            // 3rd param
               .EmitLoadArg( 0 )
               .EmitLoadUnmanagedMethodToken( kvp.Value.RefInvokerMethod )
               .EmitNewObject( REF_INVOKER_CALLBACK_CTOR )

            // 4th param
              .EmitLoadArg( 0 )
              .EmitLoadUnmanagedMethodToken( kvp.Value.InvocationMethod )
              .EmitNewObject( TypeGenerationUtils.GetMethodForEmitting( decType => TypeGenerationUtils.CreateTypeForEmittingCILType( decType, thisGenerationInfo.GenericArguments, null ), kvp.Key.EventHandlerType.LoadConstructorOrThrow( 2 ).NewWrapper( this.ctx ) ) );

            var eventTypeGen = kvp.Value.EventHandlerType;

            // 5th param
            il.EmitLoadArg( 0 )
              .EmitLoadUnmanagedMethodToken( kvp.Value.AdditionMethod )
              .EmitNewObject( ACTION_1_CTOR.ChangeDeclaringType( eventTypeGen ) )

            // 6th param
              .EmitLoadArg( 0 )
              .EmitLoadUnmanagedMethodToken( kvp.Value.RemovalMethod )
              .EmitNewObject( ACTION_1_CTOR.ChangeDeclaringType( eventTypeGen ) )

            // 7th param
              .EmitLoadArg( 0 )
              .EmitLoadUnmanagedMethodToken( kvp.Value.EventClearMethod )
              .EmitNewObject( ACTION_0_CTOR )

            // 8th param
              .EmitLoadArg( 0 )
              .EmitLoadUnmanagedMethodToken( kvp.Value.EventCheckerMethod )
              .EmitNewObject( FUNC_1_CTOR.ChangeDeclaringType( BOOLEAN_TYPE ) )

            // Constructor
              .EmitNewObject( COMPOSITE_EVENT_CTOR.ChangeDeclaringType( eventTypeGen ) )

            // Store composite event
              .EmitCall( COLLECTION_ADD_ONLY_ADD_METHOD.ChangeDeclaringType( eventsBType.GetGenericArgumentsArray() ) );
         }
      }


      protected virtual void EmitSetActionMethod<AttributeType>(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenerationInfo,
         CompositeEmittingInfo emittingInfo,
         ConstructorGenerationInfo ctorGenerationInfo,
         Int32 paramIndex,
         String actionMethodName,
         Boolean invokeAllRegardlessOfExceptions
         )
         where AttributeType : Attribute
      {
         SpecialMethodModel[] sMethods = this.GetSpecialMethods<AttributeType>( compositeModel );
         if ( sMethods.Any() )
         {
            // Emit method to call all needed special methods
            var actionGenInfo = new CompositeMethodGenerationInfoImpl(
               thisGenerationInfo.Builder.AddMethod( actionMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, CallingConventions.HasThis ), null, null
               ).WithReturnType( VOID_TYPE ) as CompositeMethodGenerationInfo;
            var cInstanceB = actionGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );

            var il = actionGenInfo.IL;
            if ( sMethods.Any() )
            {
               // instance = this._instance;
               il.EmitLoadThisField( thisGenerationInfo.CompositeField )
                 .EmitStoreLocal( cInstanceB );

               // Call all needed special methods
               this.EmitCallAllSpecialMethods<AttributeType>(
                  codeGenerationInfo,
                  fragmentGenerationInfos,
                  publicCompositeGenInfo,
                  thisGenerationInfo,
                  actionGenInfo,
                  emittingInfo,
                  -1,
                  compositeModel,
                  typeModel,
                  invokeAllRegardlessOfExceptions
                  );

               if ( invokeAllRegardlessOfExceptions )
               {
                  // Throw aggregate exception if any exceptions occurred during invocation
                  this.EmitThrowIfExceptionListHasAny( actionGenInfo, AGGREGATE_EXCEPTION_EXCEPTION_ENUMERABLE_CTOR );
               }
            }

            il.EmitReturn();

            // Now set the action
            il = ctorGenerationInfo.IL;
            il.EmitStoreToArgument( ctorGenerationInfo.Parameters[paramIndex], il2 =>
            {
               il2.EmitLoadArg( 0 )
                  .EmitLoadUnmanagedMethodToken( actionGenInfo.Builder )
                  .EmitNewObject( ACTION_0_CTOR );
            } );
         }
      }

      protected virtual ConstructorGenerationInfo EmitFragmentConstructor(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel instanceableModel,
         FragmentTypeGenerationInfo thisGenInfo,
         CILConstructor parentConstructor
         )
      {
         var ctorToCall = TypeGenerationUtils.GetMethodForEmitting( thisGenInfo.Parents, parentConstructor );
         var parameters = ctorToCall.Parameters;
         var ctorParamTypes = Enumerable.Repeat( codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ), 1 )
               .Concat( parameters.Select( param => TypeGenerationUtils.CreateTypeForEmitting( param.ParameterType, ctorToCall.DeclaringType.GenericArguments, null ) ) )
               .ToArray();

         var result = new CompositeConstructorGenerationInfoImpl(
            MethodAttributes.Public,
            ctorToCall.CallingConvention,
            ctorParamTypes.Select( ct => Tuple.Create( ct, ParameterAttributes.None, "" ) ),
            thisGenInfo,
            parentConstructor,
            ctorToCall );
         var il = result.IL;

         // Load `this` and call base constructor with all arguments except first one
         il.EmitLoadArg( 0 );
         foreach ( var pInfo in parameters )
         {
            il.EmitLoadArgumentToPassAsParameter( pInfo.ParameterType, pInfo.Position + 2 );
         }
         il.EmitCall( ctorToCall );

         // this._instance = <first parameter>
         il.EmitStoreThisField( thisGenInfo.CompositeField, il2 => il2.EmitLoadArg( 1 ) );

         // Constructor ends
         il.EmitReturn();

         return result;
      }

      protected virtual void EmitToStringMethod( AbstractTypeGenerationInfoForComposites thisTypeGenInfo, String prefix )
      {
         // TODO need to decide something better here
         //CompositeMethodGenerationInfo mGenInfo = this.CreateCopy(
         //   thisTypeGenInfo,
         //   TO_STRING_METHOD,
         //   null,
         //   null
         //   );
         //ILWrapper il = mGenInfo.IL;
         //// return "Instance of type " + this._instance.PublicCompositeRuntimeType
         //EmitString( il, prefix );
         //EmitLoadArg( il, 0 );
         //il.Emit( OpCodes.Ldfld, thisTypeGenInfo.CompositeField );
         //il.Emit( OpCodes.Callvirt, PUBLIC_RUNTIME_TYPE_GETTER );
         //il.Emit( OpCodes.Call, STRING_CONCAT_METHOD_2 );
         //il.Emit( OpCodes.Ret );
      }

      protected virtual CompositeTypeGenerationInfo EmitPublicCompositeType(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         System.Reflection.Assembly assemblyBeingProcessed,
         CILModule mob,
         CollectionsFactory collectionsFactory,
         CompositeEmittingInfo emittingInfo
         )
      {
         var generatedName = this.GetGeneratedClassName( codeGenerationInfo.PublicCompositePrefix, compositeModel.CompositeModelID );
         CompositeTypeGenerationInfo info = null;
         lock ( mob.DefinedTypesLock )
         {
            if ( mob.DefinedTypes.Any( type => type.Name.Equals( generatedName ) ) )
            {
               info = emittingInfo.PublicCompositeGenerationInfos.First( gInfo => gInfo.Builder.Module.Equals( mob ) );
               this.ProcessTypeGenerationInfo<CompositeTypeGenerationInfo>( info, null, null, true, compositeModel, assemblyBeingProcessed, typeModel, emittingInfo, null );
            }
            else
            {
               info = this.CreateTypeGenerationInfo(
                  codeGenerationInfo,
                  compositeModel,
                  assemblyBeingProcessed,
                  mob,
                  null,
                  generatedName,
                  typeModel,
                  null,
                  null,
                  emittingInfo,
                  null,
                  collectionsFactory,
                  ( tb, amountOfGArgs, compositeField ) => new CompositeTypeGenerationInfoImpl( tb, amountOfGArgs, compositeField, compositeModel )
               );

               // Make public composite more friendly in debug view
               this.EmitToStringMethod( info, "Composite of type " );
            }
            if ( emittingInfo.IsMainCompositeGenerationInfo( info, assemblyBeingProcessed ) )
            {
               info.Builder.AddNewCustomAttributeTypedParams( MAIN_PUBLIC_COMPOSITE_TYPE_ATTRIBUTE_CTOR );
            }
         }
         return info;
      }

      protected virtual void EmitPrivateCompositeType(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         System.Reflection.Assembly assemblyBeingProcessed,
         CompositeTypeModel typeModel,
         TypeBindingInformation typeInfo,
         ListQuery<AbstractGenericTypeBinding> bindings,
         CILType publicCompositeBuilder,
         CompositeEmittingInfo emittingInfo,
         CollectionsFactory collectionsFactory
         )
      {
         var generatedName = this.GetGeneratedClassName( codeGenerationInfo.PrivateCompositePrefix, emittingInfo.NewPrivateCompositeID( compositeModel ) );
         var info = this.CreateTypeGenerationInfo(
            codeGenerationInfo,
            compositeModel,
            assemblyBeingProcessed,
            null,
            publicCompositeBuilder,
            generatedName,
            typeModel,
            typeInfo,
            bindings,
            emittingInfo,
            emittingInfo.CompositeTypeGenerationInfos,
            collectionsFactory,
            ( tb, amountOfGArgs, compositeField ) => new CompositeTypeGenerationInfoImpl( tb, amountOfGArgs, compositeField, compositeModel )
            );
         this.EmitToStringMethod( info, "Private composite, public composite is of type " );
      }

      protected virtual TTypeGenerationInfo CreateTypeGenerationInfo<TTypeGenerationInfo>(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         System.Reflection.Assembly assemblyToProcess,
         CILModule mob,
         CILType publicCompositeBuilder,
         String typeName,
         CompositeTypeModel typeModel,
         TypeBindingInformation bindingInfo,
         ListQuery<AbstractGenericTypeBinding> bindings,
         CompositeEmittingInfo emittingInfo,
         ConcurrentDictionary<TypeBindingInformation, Tuple<IList<TTypeGenerationInfo>, Object>> typeGenerationInfos,
         CollectionsFactory collectionsFactory,
         Func<CILType, Int32, CILField, TTypeGenerationInfo> creatorFunc
         )
         where TTypeGenerationInfo : class, AbstractTypeGenerationInfoForComposites
      {
         var isPublicComposite = publicCompositeBuilder == null;
         var tb = isPublicComposite ? mob.AddType( typeName, TypeAttributes.Class | TypeAttributes.Public ) : publicCompositeBuilder.AddType( typeName, TypeAttributes.Class | TypeAttributes.NestedPublic );

         var typeID = emittingInfo.NewCompositeTypeID( compositeModel );
         tb.AddNewCustomAttributeTypedParams( COMPOSITE_TYPE_ID_CTOR, CILCustomAttributeFactory.NewTypedArgument( INT32_TYPE, typeID ) );

         var typeGArgs = typeModel.PublicCompositeGenericArguments.ToArray();
         var max = typeGArgs.Length;
         var result = creatorFunc( tb, max, this.EmitInstanceField( codeGenerationInfo, compositeModel, tb ) );

         emittingInfo.RegisterGenerationInfo( compositeModel, result, typeID );
         this.SetDebuggerDisplayAttribute( result );

         this.ProcessTypeGenerationInfo( result, bindingInfo, bindings, isPublicComposite, compositeModel, assemblyToProcess, typeModel, emittingInfo, typeGenerationInfos );

         return result;

      }

      protected void ProcessTypeGenerationInfo<TTypeGenerationInfo>(
         TTypeGenerationInfo result,
         TypeBindingInformation bindingInfo,
         ListQuery<AbstractGenericTypeBinding> bindings,
         Boolean isPublicComposite,
         CompositeModel compositeModel,
         System.Reflection.Assembly assemblyToProcess,
         CompositeTypeModel typeModel,
         CompositeEmittingInfo emittingInfo,
         ConcurrentDictionary<TypeBindingInformation, Tuple<IList<TTypeGenerationInfo>, Object>> typeGenerationInfos
         )
         where TTypeGenerationInfo : class, AbstractTypeGenerationInfoForComposites
      {
         var typeGArgs = typeModel.PublicCompositeGenericArguments.Select( arg => arg.NewWrapperAsTypeParameter( this.ctx ) ).ToArray();
         var publicTypesToProcess = compositeModel.PublicTypes
            .Where( pType => pType.Assembly.Equals( assemblyToProcess ) )
            .Select( pType => pType.NewWrapperAsType( this.ctx ) )
            .ToArray();
         var thisTypeFromModel = bindingInfo == null ? null : bindingInfo.NativeInfo.NewWrapperAsType( this.ctx );
         var gBuilders = result.GenericArguments.ToArray();
         var parent = thisTypeFromModel;
         CILTypeBase[] parentGArgs = null;
         if ( bindings != null )
         {
            parentGArgs = this.ResolveGArgsForParent( typeModel, gBuilders, bindings );
         }
         else if ( parent != null && parent.ContainsGenericParameters() )
         {
            parentGArgs = gBuilders;
         }

         if ( parentGArgs != null )
         {
            parent = parent.MakeGenericType( parentGArgs );
         }

         foreach ( var pcGArg in typeGArgs )
         {
            TypeGenerationUtils.CopyGenericArgumentConstraintsExceptVariance( gBuilders, null, pcGArg, gBuilders[pcGArg.GenericParameterPosition] );
         }

         if ( parent != null && !parent.ContainsGenericParameters() )
         {
            thisTypeFromModel = parent;
         }

         foreach ( var pType in isPublicComposite ? publicTypesToProcess : Enumerable.Repeat( thisTypeFromModel.GenericDefinitionIfGArgsHaveGenericParams(), 1 ) )
         {
            var pTypeGen = pType;
            if ( gBuilders.Length > 0 && pType.IsGenericTypeDefinition() )
            {
               pTypeGen = pType.MakeGenericType( isPublicComposite ?
                  gBuilders
                     .Where( gBuilder => typeGArgs[gBuilder.GenericParameterPosition].DeclaringType.Equals( pType ) )
                     .OrderBy( gBuilder => typeGArgs[gBuilder.GenericParameterPosition].GenericParameterPosition )
                     .ToArray() :
                  parentGArgs );
            }
            result.AddParent( pType, pTypeGen );
         }

         if ( isPublicComposite )
         {
            emittingInfo.AddPublicCompositeGenerationInfo( assemblyToProcess, compositeModel, (CompositeTypeGenerationInfo) result );
         }
         else
         {
            var tuple = typeGenerationInfos.GetOrAdd( bindingInfo, existing => Tuple.Create( (IList<TTypeGenerationInfo>) new List<TTypeGenerationInfo>(), new Object() ) );
            lock ( tuple.Item2 )
            {
               tuple.Item1.Add( result );
            }
         }
      }

      protected virtual void SetDebuggerDisplayAttribute( AbstractTypeGenerationInfoForComposites thisGenInfo )
      {
         // TODO don't do this...
         //var types = thisGenInfo.Parents.Keys.Where( type => !OBJECT_TYPE.Equals( type ) && !thisGenInfo.Builder.Equals( type ) && !FRAGMENT_DEPENDANT_PROPERTY.DeclaringType.Equals( type ) )
         //   .Select( key => thisGenInfo.Parents[key] )
         //   .OnlyBottomTypes()
         //   .ToArray();
         //thisGenInfo.Builder.AddNewCustomAttributeTypedParams(
         //   DEBUGGER_DISPLAY_ATTRIBUTE_STRING_CTOR,
         //   CILCustomAttributeFactory.NewTypedArgument(
         //   String.Join( ", ", types.Select( type =>
         //      "{" + Q_NAME_GET_BARE_TYPE_NAME_METHOD.DeclaringType + "." + Q_NAME_GET_BARE_TYPE_NAME_METHOD.Name + "(typeof(" + TypeGenerationUtils.GetBareTypeName( type, '.' ) + ")),nq}"
         //      ) ), this.ctx )
         //   );
      }

      protected virtual FragmentTypeGenerationInfo EmitFragmentType(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         System.Reflection.Assembly assemblyBeingProcessed,
         CompositeTypeModel typeModel,
         TypeBindingInformation typeInfo,
         ListQuery<AbstractGenericTypeBinding> bindings,
         CILType compositeBuilder,
         Int32 typeID,
         CompositeEmittingInfo emittingInfo,
         CollectionsFactory collectionsFactory
         )
      {
         var fragmentType = typeInfo.NativeInfo.NewWrapperAsType( this.ctx );
         if ( bindings != null )
         {
            var resolvedGArgs = this.ResolveGArgsForParent( typeModel, fragmentType.GenericArguments.Cast<CILTypeParameter>().ToArray(), bindings, true );
            if ( resolvedGArgs != null )
            {
               fragmentType = fragmentType.MakeGenericType( resolvedGArgs );
            }
         }

         var generatedName = this.GetGeneratedClassName( codeGenerationInfo.FragmentPrefix, typeID );
         var instancePoolRequired = compositeModel.GetAllInjectableModels()
            .Where( iModel => iModel.IsRelatedToFragment( typeInfo.NativeInfo ) )
            .Any( iModel => InjectionTime.ON_METHOD_INVOKATION.Equals( iModel.GetInjectionTime() ) || iModel.IsFragmentDependant() );

         FragmentTypeGenerationInfo info = this.CreateTypeGenerationInfo(
            codeGenerationInfo,
            compositeModel,
            assemblyBeingProcessed,
            null,
            compositeBuilder,
            generatedName,
            typeModel,
            typeInfo,
            bindings,
            emittingInfo,
            emittingInfo.FragmentTypeGenerationInfos,
            collectionsFactory,
            ( tb, amountOfGArgs, compositeField ) => new FragmentTypeGenerationInfoImpl( tb, amountOfGArgs, compositeField, instancePoolRequired )
            );

         if ( !instancePoolRequired )
         {
            info.Builder.AddNewCustomAttribute( NO_POOL_ATTRIBUTE_CTOR.DeclaringType );
         }

         foreach ( ConstructorModel constructorModel in compositeModel.Constructors )
         {
            if ( constructorModel.NativeInfo.DeclaringType.NewWrapper( this.ctx ).Equals( info.DirectBaseFromModel ) )
            {
               var ctor = this.EmitFragmentConstructor( codeGenerationInfo, compositeModel, info, constructorModel.NativeInfo.NewWrapper( this.ctx ) );
               info.ConstructorBuilders.Add( constructorModel.ConstructorIndex, ctor );
            }
         }

         this.EmitToStringMethod( info, "Fragment for composite of type " );
         return info;
      }

      protected virtual IEnumerable<FieldModel> FindFieldsWithInjectionTime( CompositeModel compositeModel, FragmentTypeGenerationInfo fragmentGenInfo, InjectionTime injectionTime )
      {
         return compositeModel.Fields.Where( fieldModel => fragmentGenInfo.Parents.ContainsKey( fieldModel.NativeInfo.DeclaringType.NewWrapperAsType( this.ctx ) ) && injectionTime.Equals( fieldModel.GetInjectionTime() ) );
      }

      protected virtual void EmitFragmentMethods(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         FragmentTypeGenerationInfo thisGenInfo,
         IEnumerable<CompositeTypeGenerationInfo> compositeTypeGenerationInfos
         )
      {
         var fragmentTypeFromModel = thisGenInfo.DirectBaseFromModel;
         foreach ( var compositeMethod in compositeModel.Methods.Where( cMethod => TypeGenerationUtils.IsAssignableFrom( cMethod.NativeInfo.DeclaringType.NewWrapperAsType( this.ctx ), fragmentTypeFromModel ) ) )
         {
            var existingFragmentMethod = TypeGenerationUtils.FindMethodImplicitlyImplementingMethod( fragmentTypeFromModel, compositeMethod.NativeInfo.NewWrapper( this.ctx ) );
            if ( existingFragmentMethod != null && !thisGenInfo.NormalMethodBuilders.ContainsKey( existingFragmentMethod ) )
            {
               var generatedMethod = this.EmitFragmentMethod(
                  existingFragmentMethod,
                  compositeMethod,
                  thisGenInfo,
                  compositeTypeGenerationInfos
               );

               if ( !existingFragmentMethod.Attributes.IsAbstract() )
               {
                  thisGenInfo.NormalMethodBuilders.Add( existingFragmentMethod, generatedMethod );
               }
            }
         }

         // Iterate all special methods. If the declaring type of special method is assignable of fragment type, we need to emit special method.
         foreach ( var sMethod in this.GetSpecialMethods<Attribute>( compositeModel, thisGenInfo ) )
         {
            CompositeMethodGenerationInfo generatedMethod = this.EmitFragmentSpecialMethod(
               codeGenerationInfo,
               sMethod,
               publicCompositeGenInfo,
               thisGenInfo
               );
            thisGenInfo.SpecialMethodBuilders.Add( sMethod.NativeInfo, generatedMethod );
         }
      }

      protected virtual IEnumerable<SpecialMethodModel> GetSpecialMethods<AttributeType>(
         CompositeModel instanceableModel,
         FragmentTypeGenerationInfo fragmentGenerationInfo
         )
         where AttributeType : Attribute
      {
         return instanceableModel.SpecialMethods.Where( sMethod => fragmentGenerationInfo.Parents.ContainsKey( sMethod.NativeInfo.DeclaringType.NewWrapperAsType( this.ctx ) ) && sMethod.AllAttributes.OfType<AttributeType>().Any() );
      }

      protected virtual CompositeMethodGenerationInfo EmitFragmentMethod(
         CILMethod fragmentMethod,
         CompositeMethodModel compositeMethodModel,
         FragmentTypeGenerationInfo thisGenInfo,
         IEnumerable<CompositeTypeGenerationInfo> compositeTypeGenerationInfos
         )
      {
         var compositeTypeGenInfo = compositeTypeGenerationInfos.Where( genInfo => genInfo.Parents.ContainsKey( compositeMethodModel.NativeInfo.DeclaringType.NewWrapperAsType( this.ctx ) ) ).First();

         // Override the base.OriginalName(args) to call the composite's method.
         var mbGenInfo = this.ImplementMethodForEmitting(
            thisGenInfo,
            parent => compositeTypeGenInfo.Parents[parent],
            compositeMethodModel.NativeInfo.NewWrapper( this.ctx ),
            null,
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
            true
            );

         var canEmitTailCall = !mbGenInfo.OverriddenMethod.Parameters.Any( param => param.ParameterType.IsByRef() );
         this.EmitCallSameCompositeMethod( mbGenInfo.OverriddenMethod, mbGenInfo, thisGenInfo.CompositeField, canEmitTailCall );
         mbGenInfo.IL.EmitReturn();

         CompositeMethodGenerationInfo result = null;
         if ( !fragmentMethod.Attributes.IsAbstract() )
         {
            // Create a new method with different name, calling base.OriginalName(args).
            result = this.ImplementMethodForEmitting(
               thisGenInfo,
               fragmentMethod,
               FRAGMENT_METHOD_PREFIX + compositeMethodModel.MethodIndex,
               MethodAttributes.Public | MethodAttributes.HideBySig,
               false
               );
#if DEBUG
            //result.MethodBuilder.SetCustomAttribute( new CustomAttributeBuilder( type-of( DebuggerStepperBoundaryAttribute ).GetConstructor( Type.EmptyTypes ), EMPTY_OBJECTS ) );
#endif
            var il = result.IL;

            // Load 'this'
            il.EmitLoadArg( 0 );

            // Load parameters
            foreach ( var pInfo in result.Parameters )
            {
               il.EmitLoadArg( pInfo );
            }

            if ( canEmitTailCall )
            {
               il.EmitTailcall();
            }
            il.EmitCallBase( result.OverriddenMethod.MakeGenericMethod( result.GenericArguments.ToArray() ) );
            il.EmitReturn();
         }

         return result;
      }

      protected virtual void EmitCallSameCompositeMethod(
         CILMethod actualCompositeMethod,
         CompositeMethodGenerationInfo methodGenInfo,
         CILField compositeField,
         Boolean emitTail
         )
      {
         var il = methodGenInfo.IL;
         var mgBuilders = methodGenInfo.GenericArguments.ToArray();
         var compositeMethodDeclaringType = actualCompositeMethod.DeclaringType;
         // ((<composite runtime type>)this._composite.Composites[<composite runtime bottom type>]).<composite method>(<args>);
         il.EmitLoadThisField( compositeField )
           .EmitCall( COMPOSITES_GETTER )
           .EmitReflectionObjectOf( compositeMethodDeclaringType, false )
           .EmitCall( COMPOSITES_GETTER_INDEXER )
           .EmitCastToType( COMPOSITES_GETTER_INDEXER.GetReturnType(), compositeMethodDeclaringType );
         // Load parameters
         foreach ( var pInfo in methodGenInfo.Parameters )
         {
            il.EmitLoadArg( pInfo );
         }
         if ( emitTail )
         {
            il.EmitTailcall();
         }
         il.EmitCall( actualCompositeMethod.MakeGenericMethod( mgBuilders ) );
      }

      protected virtual void EmitConcernInvocationType(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel instanceableModel,
         System.Reflection.Assembly assemblyBeingProcessed,
         CompositeTypeModel typeModel,
         TypeBindingInformation typeInfo,
         ListQuery<AbstractGenericTypeBinding> bindings,
         CompositeEmittingInfo emittingInfo,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentTypeGenerationInfos,
         CollectionsFactory collectionsFactory
         )
      {
         this.EmitInvocationType(
            codeGenerationInfo,
            instanceableModel,
            assemblyBeingProcessed,
            typeModel,
            typeInfo,
            publicCompositeGenInfo.Builder,
            bindings,
            emittingInfo,
            emittingInfo.ConcernTypeGenerationInfos,
            codeGenerationInfo.ConcernInvocationPrefix,
            emittingInfo.NewConcernInvocationID( instanceableModel ),
            ( genInfo, fInstanceB ) =>
            {
               this.EmitGenericConcernInvocationMethod(
                  codeGenerationInfo,
                  fragmentTypeGenerationInfos,
                  publicCompositeGenInfo,
                  genInfo,
                  emittingInfo,
                  instanceableModel,
                  typeModel,
                  fInstanceB
               );
            },
            ( genInfo, fInstanceB, compositeMethodModel ) =>
            {
               this.EmitNormalConcernInvocationMethod(
                  codeGenerationInfo,
                  compositeMethodModel,
                  typeModel,
                  fragmentTypeGenerationInfos,
                  publicCompositeGenInfo,
                  genInfo,
                  emittingInfo,
                  fInstanceB
               );
            },
            collectionsFactory
            );
      }

      protected virtual void EmitInvocationType(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel instanceableModel,
         System.Reflection.Assembly assemblyBeingProcessed,
         CompositeTypeModel typeModel,
         TypeBindingInformation typeInfo,
         CILType compositeBuilder,
         ListQuery<AbstractGenericTypeBinding> bindings,
         CompositeEmittingInfo emittingInfo,
         ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> typeGenerationInfos,
         String prefix,
         Int32 typeID,
         Action<CompositeTypeGenerationInfo, CILField> emitGenericMethod,
         Action<CompositeTypeGenerationInfo, CILField, CompositeMethodModel> emitNormalMethod,
         CollectionsFactory collectionsFactory
         )
      {
         CompositeTypeGenerationInfo genInfo = this.CreateTypeGenerationInfo(
            codeGenerationInfo,
            instanceableModel,
            assemblyBeingProcessed,
            null,
            compositeBuilder,
            this.GetGeneratedClassName( prefix, typeID ),
            typeModel,
            typeInfo,
            bindings,
            emittingInfo,
            typeGenerationInfos,
            collectionsFactory,
            ( tb, amountOfGArgs, compositeField ) => new CompositeTypeGenerationInfoImpl( tb, amountOfGArgs, compositeField, instanceableModel )
            );
         if ( genInfo != null )
         {
            var tb = genInfo.Builder;

            var ctorGenInfo = new CompositeConstructorGenerationInfoImpl(
               MethodAttributes.Public,
               CallingConventions.HasThis,
               Enumerable.Repeat( Tuple.Create( codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ), ParameterAttributes.None, "instance" ), 1 ),
               genInfo,
               null,
               null );
            var il = ctorGenInfo.IL;
            il.EmitLoadArg( 0 )
              .EmitCall( genInfo.Builder.BaseType.Constructors.First() )
              .EmitStoreThisField( genInfo.CompositeField, il2 => il2.EmitLoadArg( 1 ) )
              .EmitReturn();

            // Make it more friendly in debugger
            this.EmitToStringMethod( genInfo, "Invocation handler (" + prefix + ") for composite of type " );

            var invocationBaseType = FRAGMENT_DEPENDANT_PROPERTY.DeclaringType;
            // Implement concern invocation base interface, and set parent
            tb.AddDeclaredInterfaces( invocationBaseType );

            // Provide Instanceable<InvocationType> implementation using normal read/ Interlocked.Exchange write
            //var propBuilder = tb.AddProperty(
            //   INSTANCEABLE_NEXT_PROPERTY.Name,
            //   INSTANCEABLE_NEXT_PROPERTY.Attributes
            //   );

            //var nextB = tb.AddField( "_next", invocationBaseType, FieldAttributes.Private );
            //var getterFromInterface = INSTANCEABLE_NEXT_GETTER.ChangeDeclaringType( invocationBaseType );
            //var nextGetter = this.ImplementMethodForEmitting(
            //   genInfo,
            //   getterFromInterface,
            //   null,
            //   MethodImplAttributesExtensions.EXPLICIT_IMPLEMENTATION_ATTRIBUTES,
            //   true
            //   );
            //il = nextGetter.IL;
            //il.EmitLoadThisField( nextB )
            //  .EmitReturn();
            //nextGetter.Builder.AddOverriddenMethods( getterFromInterface );
            //propBuilder.GetMethod = nextGetter.Builder;

            //var setterFromInterface = INSTANCEABLE_NEXT_SETTER.ChangeDeclaringType( invocationBaseType );
            //var nextSetter = this.ImplementMethodForEmitting(
            //   genInfo,
            //   setterFromInterface,
            //   null,
            //   MethodImplAttributesExtensions.EXPLICIT_IMPLEMENTATION_ATTRIBUTES,
            //   true
            //   );
            //il = nextSetter.IL;
            //il.EmitLoadThisFieldAddress( nextB )
            //  .EmitLoadArg( 1 )
            //  .EmitCall( INTERLOCKED_EXCHANGE_METHOD_GDEF.MakeGenericMethod( invocationBaseType ) )
            //  .EmitPop()
            //  .EmitReturn();
            //nextSetter.Builder.AddOverriddenMethods( setterFromInterface );
            //propBuilder.SetMethod = nextSetter.Builder;

            // Provide implementation for invocation handler
            var fragmentInstanceB = tb.AddField( "_fInstance", FRAGMENT_DEPENDANT_GETTER.GetReturnType(), FieldAttributes.Private );

            // Implement the property of the FragmentDependant
            var propBuilder = tb.AddProperty(
               FRAGMENT_DEPENDANT_PROPERTY.Name,
               FRAGMENT_DEPENDANT_PROPERTY.Attributes
            );

            var fragmentGetter = this.ImplementMethodForEmitting(
               genInfo,
               FRAGMENT_DEPENDANT_GETTER,
               null,
               MethodAttributesUtils.EXPLICIT_IMPLEMENTATION_ATTRIBUTES,
               true );
            il = fragmentGetter.IL;
            il
               .EmitLoadThisField( fragmentInstanceB )
               .EmitReturn();
            fragmentGetter.Builder.AddOverriddenMethods( FRAGMENT_DEPENDANT_GETTER );
            propBuilder.GetMethod = fragmentGetter.Builder;

            var fragmentSetter = this.ImplementMethodForEmitting(
               genInfo,
               FRAGMENT_DEPENDANT_SETTER,
               null,
               MethodAttributesUtils.EXPLICIT_IMPLEMENTATION_ATTRIBUTES,
               true );
            il = fragmentSetter.IL;
            il
               .EmitStoreThisField( fragmentInstanceB, il2 => il2.EmitLoadArg( 1 ) )
               .EmitReturn();
            fragmentSetter.Builder.AddOverriddenMethods( FRAGMENT_DEPENDANT_SETTER );
            propBuilder.SetMethod = fragmentSetter.Builder;

            if ( GENERIC_FRAGMENT_METHOD.DeclaringType.Equals( typeInfo.NativeInfo.NewWrapperAsType( this.ctx ) ) )
            {
               emitGenericMethod( genInfo, fragmentInstanceB );
            }
            else
            {
               foreach ( CompositeMethodModel cMethodModel in instanceableModel.Methods )
               {
                  if ( genInfo.Parents.ContainsKey( cMethodModel.NativeInfo.DeclaringType.NewWrapperAsType( this.ctx ) ) )
                  {
                     emitNormalMethod( genInfo, fragmentInstanceB, cMethodModel );
                  }
               }
            }
         }
      }

      protected virtual void EmitNormalConcernInvocationMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeMethodModel methodModel,
         CompositeTypeModel typeModel,
         IEnumerable<FragmentTypeGenerationInfo> fragmentTypeGenerationInfos,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisInfo,
         CompositeEmittingInfo emittingInfo,
         CILField fInstanceB
         )
      {
         this.EmitNormalInvocationMethod(
            methodModel,
            thisInfo,
            CONCERN_INVOCATION_METHOD_PREFIX,
            fInstanceB,
            F_INSTANCE_GET_NEXT_INFO_METHOD,
            CONCERN_INVOCATION_INFO_ITEM_1,
            ( methodGenInfo, resultB, invocationInfoB ) =>
            {
               var il = methodGenInfo.IL;
               var instanceLB = methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );

               // instance = this._instance;
               il.EmitLoadThisField( thisInfo.CompositeField )
                 .EmitStoreLocal( instanceLB );

               // Call next concern or mixin
               this.EmitCallNextConcern(
                  methodModel,
                  methodGenInfo,
                  invocationInfoB,
                  ( nextModel, concernIndexB ) =>
                  this.EmitCallFragmentModel(
                     codeGenerationInfo,
                     fragmentTypeGenerationInfos,
                     publicCompositeGenInfo,
                     thisInfo,
                     methodGenInfo,
                     emittingInfo,
                     null,
                     null,
                     typeModel,
                     nextModel,
                     concernIndexB,
                     null,
                     false,
                     false
                     )
                  );
            }
            );
      }

      protected virtual void EmitNormalInvocationMethod(
         CompositeMethodModel methodModel,
         CompositeTypeGenerationInfo thisInfo,
         String methodPrefix,
         CILField fInstanceB,
         CILMethod fInstanceMethodToCall,
         CILMethod compositeMethodIndexGetter,
         Action<CompositeMethodGenerationInfo, LocalBuilder, LocalBuilder> emitWhenMethodIndexMatches
         )
      {
         var methodIndex = methodModel.MethodIndex;
         var methodFromModel = methodModel.NativeInfo.NewWrapper( this.ctx );

         // Implement method from the model
         var useExplicitImplementation = thisInfo.Builder.BaseType.Equals( OBJECT_TYPE );
         var methodGenInfo = this.ImplementMethodForEmitting(
            thisInfo,
            methodFromModel,
            useExplicitImplementation ? methodPrefix + methodIndex : methodFromModel.Name,
            useExplicitImplementation ? MethodAttributesUtils.EXPLICIT_IMPLEMENTATION_ATTRIBUTES : NORMAL_IMPLEMENTATION_ATTRIBUTES,
            true
         );
         thisInfo.NormalMethodBuilders.Add( methodFromModel, methodGenInfo );
         var il = methodGenInfo.IL;

         // if (this._methodIndex == <method index>)
         // then
         // emit index matching action
         // else if this._methodIndex == -1
         // Then throw exception to avoid possible infinite recursion
         // else
         // Call the composite method.
         // end if
         LocalBuilder resultB = null;
         if ( !VOID_TYPE.Equals( methodGenInfo.ReturnType ) )
         {
            resultB = methodGenInfo.GetOrCreateLocal( LB_RESULT, methodGenInfo.ReturnType );
         }
         var invocationInfoB = methodGenInfo.GetOrCreateLocal( LB_INVOCATION_INFO, fInstanceMethodToCall.GetReturnType() );
         il.EmitLoadThisField( fInstanceB )
           .EmitCall( fInstanceMethodToCall )
           .EmitStoreLocal( invocationInfoB );

         il.EmitIfElse(
            ( il2, elseLabel, endIfLabel ) =>
            {
               il2.EmitLoadLocal( invocationInfoB )
                  .EmitCall( compositeMethodIndexGetter )
                  .EmitLoadInt32( methodIndex )
                  .EmitCeq()
                  .EmitBranch( BranchType.IF_FALSE, elseLabel );
            },
            ( il2, elseLabel, endIfLabel ) =>
            {
               emitWhenMethodIndexMatches( methodGenInfo, resultB, invocationInfoB );
            },
            ( il2, endIfLabel ) =>
            {
               il2.EmitIfElse(
                  ( il3, elseLabel, endIfLabel2 ) =>
                  {
                     il3.EmitLoadLocal( invocationInfoB )
                        .EmitCall( compositeMethodIndexGetter )
                        .EmitLoadInt32( DEFAULT_INVOCATION_METHOD_INDEX )
                        .EmitCeq()
                        .EmitBranch( BranchType.IF_FALSE, elseLabel );
                  },
                  ( il3, elseLabel, endIfLabel2 ) => this.EmitThrowInternalExceptionInInvocationHandler( il, "Invalid method index ", invocationInfoB, compositeMethodIndexGetter ),
                  ( il3, endIfLabel2 ) =>
                  {
                     this.EmitCallSameCompositeMethod( methodGenInfo.OverriddenMethod, methodGenInfo, thisInfo.CompositeField, false );
                     if ( resultB != null )
                     {
                        il3.EmitStoreLocal( resultB );
                     }
                  }
                  );
            }
            );

         // return (result);
         if ( resultB != null )
         {
            il.EmitLoadLocal( resultB );
         }
         il.EmitReturn();

         if ( useExplicitImplementation )
         {
            methodGenInfo.Builder.AddOverriddenMethods( methodGenInfo.OverriddenMethod );
         }
      }

      protected virtual void EmitGenericConcernInvocationMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisInfo,
         CompositeEmittingInfo emittingInfo,
         CompositeModel instanceableModel,
         CompositeTypeModel typeModel,
         CILField fInstanceB
         )
      {
         this.EmitGenericInvocationMethod(
            codeGenerationInfo,
            thisInfo,
            instanceableModel,
            fInstanceB,
            F_INSTANCE_GET_NEXT_INFO_METHOD,
            CONCERN_INVOCATION_INFO_ITEM_1,
           ( compositeMethodModel, methodGenInfo, invocationInfoB, instanceLB, resultB ) =>
           {
              var il = methodGenInfo.IL;

              // Save the method info object to second argument, in case the invoker gives wrong one by mistake or purpose
              il.EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_C_INSTANCE ) )
                .EmitCall( INVOCATION_INFO_GETTER )
                .EmitCall( INVOCATION_INFO_METHOD_GETTER )
                .EmitStoreArg( 2 );

              this.EmitCallNextConcern(
              compositeMethodModel,
              methodGenInfo,
              invocationInfoB,
              ( nextModel, concernIndexB ) =>
              {
                 this.EmitCallFragmentModel(
                    codeGenerationInfo,
                    fragmentGenerationInfos,
                    publicCompositeGenInfo,
                    thisInfo,
                    methodGenInfo,
                    emittingInfo,
                    null,
                    null,
                    typeModel,
                    nextModel,
                    concernIndexB,
                    ( actualFragmentType, methodToCall, fragmentTypeGenInfo ) =>
                    {
                       var fInstanceLB = methodGenInfo.GetLocalOrThrow( LB_F_INSTANCE );
                       if ( nextModel.IsGeneric )
                       {
                          // Generic fragment call
                          // result = ((GenericInvocator)fragmentInstance.Fragment).Invoke( <first arg>, <second arg>, <third arg>);
                          il.EmitLoadLocal( fInstanceLB )
                            .EmitCall( FRAGMENT_GETTER )
                            .EmitCastToType( FRAGMENT_GETTER.GetReturnType(), GENERIC_FRAGMENT_METHOD.DeclaringType )

                          // The arguments
                            .EmitLoadArg( 1 )
                            .EmitLoadArg( 2 )
                            .EmitLoadArg( 3 )

                          // Call method
                            .EmitCall( GENERIC_FRAGMENT_METHOD )

                          // Store result
                            .EmitStoreLocal( resultB );
                       }
                       else if ( nextModel.NativeInfo.IsGenericMethodDefinition )
                       {
                          var fragmentMethodGenInfo = fragmentTypeGenInfo.NormalMethodBuilders[nextModel.NativeInfo.NewWrapper( this.ctx )];
                          // Generic method call, need to reflect, because next fragment is generic type and/or the method is generic.
                          // result = type-of(<fragment generated type>).GetMethod(<name>, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).MakeGenericMethod(<arg-2>.GetGenericArguments()).Invoke(fragmentInstance.Fragment, <arg-3>);
                          //il.EmitMethodOf( GetActualMethod<MethodInfo>( fragmentTypeGenInfo.Parents, fragmentMethodGenInfo.MethodBuilder ).MakeGenericMethod, fragmentMethodGenInfo.GBuilders );
                          il.EmitReflectionObjectOf( actualFragmentType )
                            .EmitLoadString( fragmentMethodGenInfo.Builder.Name )
                            .EmitLoadInt32( (Int32) ( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly ) )
                            .EmitCall( GET_METHOD_METHOD )
                            .EmitLoadArg( 2 )
                            .EmitCall( METHOD_INFO_GET_GARGS_METHOD )
                            .EmitCall( MAKE_GENERIC_METHOD_METHOD )
                            .EmitLoadLocal( fInstanceLB )
                            .EmitCall( FRAGMENT_GETTER )
                            .EmitLoadArg( 3 )
                            .EmitCall( INVOKE_METHOD_METHOD )
                            .EmitStoreLocal( resultB );
                       }
                       else
                       {
                          // Non-generic method call to non-generic fragment, load arguments from arg-array
                          // ((<fragment type>)fragmentInstance.Fragment).<fragment generated method>(<args>);
                          il.EmitLoadLocal( fInstanceLB )
                            .EmitCall( FRAGMENT_GETTER )
                            .EmitCastToType( FRAGMENT_GETTER.GetReturnType(), methodToCall.DeclaringType );

                          var fragmentMethodGenInfo = fragmentTypeGenInfo.NormalMethodBuilders[nextModel.NativeInfo.NewWrapper( this.ctx )];
                          this.EmitLoadParametersFromObjectArray( thisInfo, methodGenInfo, fragmentMethodGenInfo, compositeMethodModel.MethodIndex, 0, il, () => il.EmitLoadArg( 3 ) );
                          il.EmitCall( methodToCall );
                          var returnType = methodToCall.GetReturnType();
                          if ( !VOID_TYPE.Equals( returnType ) )
                          {
                             il.EmitCastToType( returnType, resultB.LocalType )
                               .EmitStoreLocal( resultB );
                          }

                          // Need to post-process the arguments array for out-parameters
                          this.EmitAfterGenericCall(
                             fragmentMethodGenInfo,
                             pBuilder =>
                             {
                                // args[<idx>] = <local>;
                                il.EmitLoadArg( 3 )
                                  .EmitLoadInt32( pBuilder.Position );
                                var argLB = methodGenInfo.GetLocalRaw( GetDummyParameterName( compositeMethodModel.MethodIndex, pBuilder ) );
                                il.EmitLoadLocal( argLB )
                                  .EmitCastToType( argLB.LocalType, OBJECT_TYPE )
                                  .EmitStoreElement( OBJECT_TYPE );
                             }
                             );
                       }
                    },
                    false,
                    false
                    );
              }
              );
           }
            );
      }

      protected virtual void EmitAfterGenericCall(
         CompositeMethodGenerationInfo compositeMethodGenerationInfoToUse,
         Action<CILParameter> outParamAction
         )
      {
         for ( Int32 pIdx = 0; pIdx < compositeMethodGenerationInfoToUse.Parameters.Count; ++pIdx )
         {
            var pBuilder = compositeMethodGenerationInfoToUse.Parameters[pIdx];
            if ( pBuilder.Attributes.IsOut() || pBuilder.ParameterType.IsByRef() )
            {
               outParamAction( pBuilder );
            }
         }
      }

      protected static String GetDummyParameterName( Int32 methodID, CILParameter param )
      {
         return ARG_ARRAY_DUMMY_LOCAL_PREFIX + "_" + methodID + "_" + param.Position;
      }

      protected virtual void EmitLoadParametersFromObjectArray<BuilderType>(
         CompositeTypeGenerationInfo thisGenerationInfo,
         CompositeMethodGenerationInfo thisCompositeMethodGenerationInfo,
         MethodBaseGenerationInfo<BuilderType> targetCompositeMethodGenerationInfo,
         Int32 methodID,
         Int32 amountOfParamsToSkip,
         MethodIL il,
         Action emitLoadObjectArray
         )
         where BuilderType : CILMethodBase
      {
         var dummies = new LocalBuilder[targetCompositeMethodGenerationInfo.Parameters.Count - amountOfParamsToSkip];
         var paramTypes = targetCompositeMethodGenerationInfo.Parameters.Skip( amountOfParamsToSkip ).Select( pType => TypeGenerationUtils.CreateTypeForEmitting( pType.ParameterType, thisGenerationInfo.GenericArguments, thisCompositeMethodGenerationInfo.GenericArguments ) ).ToArray();
         var pBuilders = targetCompositeMethodGenerationInfo.Parameters.Skip( amountOfParamsToSkip ).ToArray();
         foreach ( var pInfo in pBuilders )
         {
            var position = pInfo.Position - amountOfParamsToSkip;
            var paramType = paramTypes[position];
            if ( pInfo.Attributes.IsOut() || paramType.IsByRef() )
            {
               var dummy = il.DeclareLocal( paramType.GetElementType() );
               thisCompositeMethodGenerationInfo.AddLocalRaw( GetDummyParameterName( methodID, pInfo ), dummy );
               dummies[position] = dummy;
               emitLoadObjectArray();
               il.EmitLoadInt32( position )
                 .EmitLoadElement( OBJECT_TYPE )
                 .EmitCastToType( OBJECT_TYPE, dummy.LocalType )
                 .EmitStoreLocal( dummy );
            }
         }
         // Load parameters
         foreach ( var pInfo in pBuilders )
         {
            Int32 position = pInfo.Position - amountOfParamsToSkip;
            if ( dummies[position] != null )
            {
               il.EmitLoadLocalAddress( dummies[position] );
            }
            else
            {
               emitLoadObjectArray();
               il.EmitLoadInt32( position )
                 .EmitLoadElement( OBJECT_TYPE );
               var paramType = paramTypes[position];
               il.EmitCastToType( OBJECT_TYPE, paramType );
            }
         }
      }

      protected virtual void EmitGenericInvocationMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeTypeGenerationInfo thisInfo,
         CompositeModel instanceableModel,
         CILField fInstanceB,
         CILMethod fInstanceMethodToCall,
         CILMethod compositeMethodIndexGetter,
         Action<CompositeMethodModel, CompositeMethodGenerationInfo, LocalBuilder, LocalBuilder, LocalBuilder> emitMethodHandling
         )
      {
         var methodGenInfo = this.ImplementMethodForEmitting(
            thisInfo,
            GENERIC_FRAGMENT_METHOD,
            null,
            MethodAttributesUtils.EXPLICIT_IMPLEMENTATION_ATTRIBUTES,
            true
            );
         thisInfo.NormalMethodBuilders.Add( GENERIC_FRAGMENT_METHOD, methodGenInfo );
         var il = methodGenInfo.IL;
         var instanceLB = methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );
         var resultB = methodGenInfo.GetOrCreateLocal( LB_RESULT, OBJECT_TYPE );
         var invocationInfoB = methodGenInfo.GetOrCreateLocal( LB_INVOCATION_INFO, fInstanceMethodToCall.GetReturnType() );

         il.EmitLoadThisField( fInstanceB )
           .EmitCall( fInstanceMethodToCall )
           .EmitStoreLocal( invocationInfoB );
         // instance = this._instance;
         il.EmitLoadThisField( thisInfo.CompositeField )
           .EmitStoreLocal( instanceLB )

         // result = null;
           .EmitLoadNull()
           .EmitStoreLocal( resultB )

         // Switch method index, and invoke the corresponding next fragment
           .EmitLoadThisField( fInstanceB )
           .EmitCall( fInstanceMethodToCall )
           .EmitStoreLocal( invocationInfoB )
           .EmitLoadLocal( invocationInfoB )
           .EmitCall( compositeMethodIndexGetter )
           .EmitSwitch( instanceableModel.Methods.Count,
              ( il2, jumpTable, defaultCaseLabel, endSwitchLabel ) =>
              {
                 for ( Int32 methodIndex = 0; methodIndex < jumpTable.Length; ++methodIndex )
                 {
                    CompositeMethodModel compositeMethodModel = instanceableModel.Methods[methodIndex];
                    il2.MarkLabel( jumpTable[methodIndex] );
                    emitMethodHandling( compositeMethodModel, methodGenInfo, invocationInfoB, instanceLB, resultB );
                    il2.EmitBranch( BranchType.ALWAYS, endSwitchLabel );
                 }
              },
              ( il2, endSwitchLabel ) => this.EmitThrowInternalExceptionInInvocationHandler( il2, "Invalid method index: ", invocationInfoB, compositeMethodIndexGetter )
            )

           .EmitLoadLocal( resultB )
           .EmitReturn();

         methodGenInfo.Builder.AddOverriddenMethods( GENERIC_FRAGMENT_METHOD );
      }

      protected virtual void EmitCallNextConcern(
         CompositeMethodModel methodModel,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         LocalBuilder invocationInfoB,
         Action<AbstractFragmentMethodModel, LocalBuilder> callFragmentAction
         )
      {
         var il = thisMethodGenInfo.IL;
         //         Int32 amountOfConcerns = methodModel.Concerns.Count();

         // TODO localBuilder for concern index.
         var concernIndexB = thisMethodGenInfo.GetOrCreateLocal( LB_CONCERN_INDEX );

         // Switch concern index
         il.EmitLoadLocal( invocationInfoB )
           .EmitCall( CONCERN_INVOCATION_INFO_ITEM_2 )
           .EmitStoreLocal( concernIndexB )
           .EmitLoadLocal( concernIndexB )
           .EmitSwitch( methodModel.Concerns.Count,
           ( il2, jumpTable, defaultCaseLabel, endSwitchLabel ) =>
           {
              // Invoke appropriate fragments for each concern index
              for ( Int32 idx = 0; idx < jumpTable.Length; ++idx )
              {
                 il.MarkLabel( jumpTable[idx] );
                 var nextConcernIndex = idx + 1;
                 AbstractFragmentMethodModel nextModel = null;
                 if ( nextConcernIndex == jumpTable.Length )
                 {
                    nextModel = methodModel.Mixin;
                 }
                 else
                 {
                    nextModel = methodModel.Concerns[nextConcernIndex];
                 }
                 // Setup invocation info if required
                 var instanceLB = thisMethodGenInfo.GetLocalOrThrow( LB_C_INSTANCE );
                 // if (instance.InvocationInfo != null && instance.InvocationInfo.FragmentMethodModel != null)
                 var endIfLabel = il.DefineLabel();
                 il.EmitLoadLocal( instanceLB )
                   .EmitCall( INVOCATION_INFO_GETTER )
                   .EmitBranch( BranchType.IF_FALSE, endIfLabel )
                   .EmitLoadLocal( instanceLB )
                   .EmitCall( INVOCATION_INFO_GETTER )
                   .EmitCall( INVOCATION_INFO_FRAGMENT_METHOD_MODEL_GETTER )
                   .EmitBranch( BranchType.IF_FALSE, endIfLabel )
                    // then
                    // instance.InvocationInfo.FragmentMethodModel = this._concernIndex >= <max value> ? instance.ModelInfo.Model.Methods[this._methodIndex].Mixin : instance.ModelInfo.Model.Methods[this._methodIndex].Concerns[this._concernIndex + 1];
                   .EmitLoadLocal( instanceLB )
                   .EmitCall( INVOCATION_INFO_GETTER )
                   .EmitCastToType( INVOCATION_INFO_GETTER.GetReturnType(), INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER.DeclaringType )
                   .EmitLoadLocal( instanceLB )
                   .EmitCall( MODEL_INFO_GETTER )
                   .EmitCall( MODEL_GETTER )
                   .EmitCall( C_METHODS_GETTER )
                   .EmitLoadLocal( invocationInfoB )
                   .EmitCall( CONCERN_INVOCATION_INFO_ITEM_1 )
                   .EmitCall( COMPOSITE_METHODS_INDEXER );

                 Int32 maxValue = methodModel.Concerns.Count - 1;
                 if ( maxValue > 0 )
                 {
                    var loadMixinLabel = il.DefineLabel();
                    il.EmitLoadLocal( concernIndexB )
                      .EmitLoadInt32( maxValue )
                      .EmitBranch( BranchType.IF_FIRST_GREATER_THAN_OR_EQUAL_TO_SECOND, loadMixinLabel )

                      .EmitCall( CONCERN_MODELS_GETTER )
                      .EmitLoadLocal( concernIndexB )
                      .EmitLoadInt32( 1 )
                      .EmitAdd()
                      .EmitCall( CONCERN_MODELS_INDEXER )
                      .EmitCall( INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER )
                      .EmitBranch( BranchType.ALWAYS, endIfLabel )

                      .MarkLabel( loadMixinLabel );
                 }
                 il.EmitCall( MIXIN_MODEL_GETTER )
                   .EmitCall( INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER )

                 // end if
                   .MarkLabel( endIfLabel );

                 // Emit custom code for calling next concern
                 callFragmentAction( nextModel, concernIndexB );
                 // Branch to point after switch
                 il.EmitBranch( BranchType.ALWAYS, endSwitchLabel );
              }
           },
           ( il2, endSwitchLabel ) => this.EmitThrowInternalExceptionInInvocationHandler( il, "Invalid concern index: ", concernIndexB, null )
           );
      }

      protected virtual void EmitThrowInternalExceptionInInvocationHandler(
         MethodIL il,
         String prefix,
         LocalBuilder invocationInfoB,
         CILMethod methodIndexGetter
         )
      {
         // throw new InternalException("<prefix>" + <field> + ".");
         il.EmitLoadString( prefix )
           .EmitLoadLocal( invocationInfoB );
         if ( methodIndexGetter != null )
         {
            il.EmitCall( methodIndexGetter )
              .EmitCastToType( methodIndexGetter.GetReturnType(), OBJECT_TYPE );
         }
         else
         {
            il.EmitCastToType( invocationInfoB.LocalType, OBJECT_TYPE );
         }
         il
           .EmitLoadString( "." )
           .EmitCall( STRING_CONCAT_METHOD_3 )
           .EmitThrowNewException( INTERNAL_EXCEPTION_CTOR );
      }

      protected virtual void EmitSideEffectInvocationType(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         System.Reflection.Assembly assemblyBeingProcessed,
         CompositeTypeModel typeModel,
         TypeBindingInformation typeInfo,
         CILType compositeBuilder,
         ListQuery<AbstractGenericTypeBinding> bindings,
         CompositeEmittingInfo emittingInfo,
         CollectionsFactory collectionsFactory
         )
      {
         this.EmitInvocationType(
            codeGenerationInfo,
            compositeModel,
            assemblyBeingProcessed,
            typeModel,
            typeInfo,
            compositeBuilder,
            bindings,
            emittingInfo,
            emittingInfo.SideEffectTypeGenerationInfos,
            codeGenerationInfo.SideEffectInvocationPrefix,
            emittingInfo.NewSideEffectInvocationID( compositeModel ),
            ( genInfo, fInstanceB ) =>
            {
               this.EmitGenericSideEffectMethod(
                  codeGenerationInfo,
                  genInfo,
                  compositeModel,
                  fInstanceB
                  );
            },
            ( genInfo, fInstanceB, compositeMethodModel ) =>
            {
               this.EmitNormalSideEffectMethod(
                  compositeMethodModel,
                  genInfo,
                  fInstanceB
                  );
            },
            collectionsFactory
            );
      }

      protected virtual void EmitNormalSideEffectMethod(
         CompositeMethodModel methodModel,
         CompositeTypeGenerationInfo thisInfo,
         CILField fInstanceB
         )
      {
         this.EmitNormalInvocationMethod(
            methodModel,
            thisInfo,
            SIDE_EFFECT_INVOCATION_METHOD_PREFIX,
            fInstanceB,
            F_INSTANCE_GET_METHOD_RESULT_METHOD,
            SIDE_EFFECT_INVOCATION_INFO_ITEM_1,
            ( methodGenInfo, resultLB, invocationInfoB ) =>
            {
               this.EmitSideEffectBody(
                  invocationInfoB,
                  methodGenInfo,
                  il =>
                  {
                     var isVoidMethod = resultLB == null;
                     if ( !isVoidMethod || methodGenInfo.HasByRefParameters )
                     {
                        var argsArrayType = LB_ARGS_ARRAY_FOR_SIDE_EFFECTS.Type.NewWrapperAsType( this.ctx );
                        if ( isVoidMethod )
                        {
                           resultLB = methodGenInfo.GetOrCreateLocal( LB_RESULT, methodGenInfo.HasByRefParameters ? argsArrayType : SIDE_EFFECT_INVOCATION_INFO_ITEM_2.GetReturnType() );
                        }

                        il.EmitLoadLocal( invocationInfoB )
                          .EmitCall( SIDE_EFFECT_INVOCATION_INFO_ITEM_2 );
                        if ( methodGenInfo.HasByRefParameters )
                        {
                           il.EmitCastToType( SIDE_EFFECT_INVOCATION_INFO_ITEM_2.GetReturnType(), argsArrayType );
                        }
                        if ( !isVoidMethod )
                        {
                           // result = (<return-type>)this._result;
                           CILTypeBase fromType;
                           if ( methodGenInfo.HasByRefParameters )
                           {
                              // this._result is array, with first item as return value
                              fromType = argsArrayType.ElementType;
                              il.EmitLoadInt32( 0 )
                                .EmitLoadElement( fromType );
                           }
                           else
                           {
                              fromType = SIDE_EFFECT_INVOCATION_INFO_ITEM_2.GetReturnType();
                           }
                           il.EmitCastToType( fromType, resultLB.LocalType );
                        }
                        il.EmitStoreLocal( resultLB );

                        if ( methodGenInfo.HasByRefParameters )
                        {
                           // for all by-ref parameters, set this method parameters to be them
                           for ( var idx = 0; idx < methodGenInfo.Parameters.Count; ++idx )
                           {
                              var paramType = methodGenInfo.Parameters[idx].ParameterType;
                              if ( paramType.IsByRef() )
                              {
                                 // <arg> = (<arg-type>)((Object[])invocationInfo.Item2)[<idx + 1>];
                                 il.EmitStoreToArgument( methodGenInfo.Parameters[idx], ( il2 ) =>
                                 {
                                    if ( isVoidMethod )
                                    {
                                       il2.EmitLoadLocal( resultLB );
                                    }
                                    else
                                    {
                                       il2.EmitLoadLocal( invocationInfoB )
                                          .EmitCall( SIDE_EFFECT_INVOCATION_INFO_ITEM_2 )
                                          .EmitCastToType( SIDE_EFFECT_INVOCATION_INFO_ITEM_2.GetReturnType(), argsArrayType );
                                    }
                                    il2.EmitLoadInt32( idx + 1 )
                                       .EmitLoadElement( argsArrayType.ElementType )
                                       .EmitCastToType( argsArrayType.ElementType, ( (CILType) paramType ).ElementType );
                                 } );
                              }
                           }
                        }
                     }
                  } );
            } );
      }

      protected virtual void EmitGenericSideEffectMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeTypeGenerationInfo thisInfo,
         CompositeModel compositeModel,
         CILField fInstanceB
         )
      {
         this.EmitGenericInvocationMethod(
            codeGenerationInfo,
            thisInfo,
            compositeModel,
            fInstanceB,
            F_INSTANCE_GET_METHOD_RESULT_METHOD,
            SIDE_EFFECT_INVOCATION_INFO_ITEM_1,
            ( compositeMethodModel, methodGenInfo, invocationInfoB, instanceLB, resultLB ) =>
            {
               Boolean hasByRefParams = compositeMethodModel.Parameters.Any( param => param.NativeInfo.ParameterType.IsByRef );
               this.EmitSideEffectBody(
                  invocationInfoB,
                  methodGenInfo,
                  il =>
                  {
                     var argsArrayType = LB_ARGS_ARRAY_FOR_SIDE_EFFECTS.Type.NewWrapperAsType( this.ctx );
                     // result = this._result;
                     il.EmitLoadLocal( invocationInfoB )
                       .EmitCall( SIDE_EFFECT_INVOCATION_INFO_ITEM_2 );
                     if ( hasByRefParams )
                     {
                        // this._result is array, with first item as return value
                        il.EmitCastToType( SIDE_EFFECT_INVOCATION_INFO_ITEM_2.GetReturnType(), argsArrayType )
                          .EmitLoadInt32( 0 )
                          .EmitLoadElement( OBJECT_TYPE );
                     }
                     il.EmitStoreLocal( resultLB );

                     if ( hasByRefParams )
                     {
                        for ( Int32 idx = 0; idx < compositeMethodModel.Parameters.Count; ++idx )
                        {
                           Type paramType = compositeMethodModel.Parameters[idx].NativeInfo.ParameterType;
                           if ( paramType.IsByRef )
                           {
                              // <arg-3>[idx] = ((Object[])this._result)[<idx + 1>];
                              il.EmitLoadArg( 3 )
                                .EmitLoadInt32( idx )
                                .EmitLoadLocal( invocationInfoB )
                                .EmitCall( SIDE_EFFECT_INVOCATION_INFO_ITEM_2 )
                                .EmitCastToType( SIDE_EFFECT_INVOCATION_INFO_ITEM_2.GetReturnType(), argsArrayType )
                                .EmitLoadInt32( idx + 1 )
                                .EmitLoadElement( OBJECT_TYPE )
                                .EmitStoreElement( OBJECT_TYPE );
                           }
                        }
                     }

                  }
                  );
            }
            );
      }

      protected virtual void EmitSideEffectBody(
         LocalBuilder invocationInfoB,
         CompositeMethodGenerationInfo methodGenInfo,
         Action<MethodIL> resultHandling
         )
      {
         var il = methodGenInfo.IL;
         // if ( this._exception != null )
         // then
         // throw this._exception;
         // else
         // handle result
         // end if
         il.EmitIfElse(
            ( il2, elseLabel, endIfLabel ) =>
            {
               il2.EmitLoadLocal( invocationInfoB )
                  .EmitCall( SIDE_EFFECT_INVOCATION_INFO_ITEM_3 )
                  .EmitBranch( BranchType.IF_FALSE, elseLabel );
            },
            ( il2, elseLabel, endIfLabel ) =>
            {
               il2.EmitLoadLocal( invocationInfoB )
                  .EmitCall( SIDE_EFFECT_INVOCATION_INFO_ITEM_3 )
                  .EmitThrow();
            },
            ( il2, endIfLabel ) => resultHandling( il2 )
            );
      }

      protected virtual void EmitPrivateCompositeEquals(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         // By default, do nothing.
      }

      protected virtual void EmitPrivateCompositeHashCode(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         // By default, do nothing.
      }

      protected virtual void EmitPublicCompositeEquals(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         // By default, do nothing.
      }

      protected virtual void EmitPublicCompositeHashCode(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         // By default, do nothing.
      }

      protected virtual IEnumerable<FragmentTypeGenerationInfo> FindFragmentsImplementingMethod(
         CILMethod method,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos
         )
      {
         return fragmentGenerationInfos
            .Where( genInfo => genInfo.DirectBaseFromModel.FullInheritanceChain().Any( t => t.DeclaredMethods.Contains( method ) ) );
         //{
         //   var implMethod = genInfo.DirectBaseFromModel.BaseTypeChain().SelectMany( t => t.DeclaredMethods ).FirstOrDefault( m => method.Name.Equals( m.Name ), method.GetParameters().Select( pInfo => pInfo.ParameterType ).ToArray() );
         //   return implMethod != null && !implMethod.DeclaringType.Equals( method.DeclaringType ) && implMethod.GetBaseDefinition().Equals( method );
         //}
         //);
      }

      protected virtual void EmitCallNonCompositeMethod(
         CompositeCodeGenerationInfo codeGenerationInfo,
         CompositeModel compositeModel,
         CompositeTypeModel typeModel,
         CompositeTypeGenerationInfo publicCompositeGenInfo,
         CompositeTypeGenerationInfo thisGenInfo,
         CompositeEmittingInfo emittingInfo,
         IEnumerable<FragmentTypeGenerationInfo> fragmentGenerationInfos,
         CILMethod methodToCall,
         Action<MethodIL> beforeUsingFragment,
         Action<MethodIL> afterUsingFragment,
         Func<IEnumerable<FragmentTypeGenerationInfo>, FragmentTypeGenerationInfo> genSelector
         )
      {
         var suitableFragment = genSelector( TypeGenerationUtils.OnlyBottomTypes( this.FindFragmentsImplementingMethod( methodToCall, fragmentGenerationInfos ), genInfo => genInfo.DirectBaseFromModel ) );
         if ( suitableFragment != null )
         {
            var methodGenInfo = this.ImplementMethodForEmitting(
               thisGenInfo,
               parent => suitableFragment.Parents[parent],
               methodToCall,
               null,
               MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
               true
            );
            var il = methodGenInfo.IL;

            if ( beforeUsingFragment != null )
            {
               beforeUsingFragment( il );
            }

            var actualMethod = methodGenInfo.OverriddenMethod;
            var cInstanceB = methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, codeGenerationInfo.CompositeInstanceFieldType.NewWrapper( this.ctx ) );
            var resultB = methodGenInfo.GetOrCreateLocal( LB_RESULT, actualMethod.GetReturnType() );

            // cInstance = this._instance;
            il.EmitLoadThisField( thisGenInfo.CompositeField )
              .EmitStoreLocal( cInstanceB );

            this.EmitUseFragmentPool(
               codeGenerationInfo,
               fragmentGenerationInfos,
               publicCompositeGenInfo,
               thisGenInfo,
               methodGenInfo,
               emittingInfo,
               null,
               null,
               compositeModel,
               null,
               typeModel,
               suitableFragment.DirectBaseFromModel,
               false,
               ( fType, fInfo ) =>
               {
                  //((<fragment-type>)fInstance.Fragment).<method>(<args>);
                  il.EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_F_INSTANCE ) )
                    .EmitCall( FRAGMENT_GETTER );
                  foreach ( var pInfo in methodGenInfo.Parameters )
                  {
                     il.EmitLoadArg( pInfo.Position + 1 );
                  }
                  il.EmitCall( actualMethod )
                    .EmitStoreLocal( resultB );
               }
               );
            il.EmitLoadLocal( resultB )
              .EmitReturn();
            if ( afterUsingFragment != null )
            {
               afterUsingFragment( il );
            }
         }
      }

      protected virtual void EmitConvertAndReturnMethod(
         CILTypeBase propertyType,
         CILMethod actualGetMethod,
         MethodIL il
         )
      {
         il.EmitLoadArg( 0 )
           .EmitCall( actualGetMethod )
           .EmitCastToType( propertyType, OBJECT_TYPE )
           .EmitReturn();
      }

      protected virtual Boolean HasGenericFragmentMethods( CompositeMethodModel cMethod )
      {
         return cMethod.GetAllMethodModels()
            .OfType<AbstractFragmentMethodModel>()
            .Any( fMethod => fMethod.IsGeneric );
      }

      protected virtual Boolean HasOnInvocationInjections( IEnumerable<SpecialMethodModel> sMethods )
      {
         return sMethods.Any( sMethod => sMethod.Parameters.Any( param => this.HasOnInvocationInjections( param ) ) || this.HasOnInvocationInjections( sMethod.CompositeModel, sMethod ) );
      }

      protected virtual Boolean HasOnInvocationInjections( CompositeMethodModel cMethod )
      {
         AbstractFragmentMethodModel[] fragmentModels = cMethod.GetAllMethodModels()
            .OfType<AbstractFragmentMethodModel>()
            .ToArray();
         return Enumerable.Repeat<AbstractInjectableModel>( cMethod.Result, 1 )
            .Concat( cMethod.Parameters )
            .Concat( cMethod.CompositeModel.Fields.Where( fieldModel =>
               fragmentModels.Any( fragmentModel => fieldModel.NativeInfo.DeclaringType.GetGenericDefinitionIfGenericType().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( fragmentModel.NativeInfo.DeclaringType ) ) )
            )
            .Any( iModel => this.HasOnInvocationInjections( iModel ) );
      }

      protected virtual Boolean HasOnInvocationInjections( AbstractFragmentMethodModel fModel )
      {
         return this.HasOnInvocationInjections( fModel.CompositeMethod.CompositeModel, fModel );
      }

      protected virtual Boolean HasOnInvocationInjections( CompositeModel owner, AbstractMethodModel mModel )
      {
         return owner.Fields.Where( fieldModel =>
                fieldModel.NativeInfo.DeclaringType.GetGenericDefinitionIfGenericType().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( mModel.NativeInfo.DeclaringType )
            )
            .Any( iModel => this.HasOnInvocationInjections( iModel ) );
      }

      protected virtual Boolean HasOnInvocationInjections( AbstractInjectableModel iModel )
      {
         return iModel.InjectionScope != null && InjectionTime.ON_METHOD_INVOKATION.Equals( iModel.GetInjectionTime() );
      }

      //protected virtual Boolean HasOnInvocationInjections( CompositeModel owner, CompositeTypeGenerationInfo fragmentGenInfo )
      //{
      //   return owner.Fields.Where( fieldModel =>
      //      TypeUtil.IsAssignableFrom( TypeUtil.GenericDefinitionIfGenericType( fieldModel.NativeInfo.DeclaringType ), fragmentGenInfo.DirectBaseFromModel )
      //      )
      //      .Any( iModel => this.HasOnInvocationInjections( iModel ) );
      //}

      protected virtual void EmitRequiredActionIfHasOnInvocationInjections(
         AbstractFragmentMethodModel fModel,
         CompositeMethodGenerationInfo thisMethodGenInfo,
         Action<LocalBuilder> fragmentMethodModelLoader
         )
      {
         LocalBuilder compositeMethodModelB;
         this.InitializeComplexMethodModelLocalIfNecessary( fModel.CompositeMethod, thisMethodGenInfo, out compositeMethodModelB );
         if ( this.HasOnInvocationInjections( fModel ) )
         {
            var il = thisMethodGenInfo.IL;

            // instance.InvocationInfo.FragmentMethodModel = compositeMethod.<load fragment method model>;
            il.EmitLoadLocal( thisMethodGenInfo.GetLocalOrThrow( LB_C_INSTANCE ) )
              .EmitCall( INVOCATION_INFO_GETTER )
              .EmitCastToType( INVOCATION_INFO_GETTER.GetReturnType(), INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER.DeclaringType )
              .EmitLoadLocal( compositeMethodModelB );
            fragmentMethodModelLoader( compositeMethodModelB );
            il.EmitCall( INVOCATION_INFO_FRAGMENT_METHOD_MODEL_SETTER );
         }
      }

      protected virtual Boolean NeedToEmitAdditionalMemberInfo(
         CompositeTypeGenerationInfo thisGenerationInfo,
         String memberName,
         Func<CILType, String, Object> memberGetter
         )
      {
         return !thisGenerationInfo.Parents.Keys.Any( parent => !parent.Attributes.IsInterface() && !thisGenerationInfo.Builder.Equals( parent ) && memberGetter( parent, memberName ) != null );
      }

      protected virtual CILMethod EmitRefMethodForPropertyOrEvent(
         CILField field,
         String name
         )
      {
         // private Boolean <name>(Object delegate, out Object returnValue)
         // {
         //    if (delegate is FunctionWithRef<<field type>>)
         //    {
         //       returnValue = ((FunctionWithRef<<field type>>)delegate)(ref this._field);
         //       return true;
         //    } else if (delegate is ActionWithRef<<field type>>)
         //    {
         //       ((ActionWithRef<<field type>>)delegate(ref this._field);
         //       returnValue = null;
         //       return true;
         //    } else
         //    {
         //       returnValue = null;
         //       return false;
         //    }
         //}
         var method = field.DeclaringType.AddMethod( name, MethodAttributes.Private | MethodAttributes.HideBySig, CallingConventions.HasThis );
         method.ReturnParameter.ParameterType = BOOLEAN_TYPE;
         var delegateParam = method.AddParameter( "delegate", ParameterAttributes.None, OBJECT_TYPE );
         var returnValueParam = method.AddParameter( "returnValue", ParameterAttributes.Out, OBJECT_TYPE.MakeByRefType() );

         var il = method.MethodIL;
         var refFuncType = REF_FUNCTION_TYPE.MakeGenericType( field.FieldType );
         il.EmitIfElse(
            ( il2, elseLabel, endIfLabel ) => il2
               .EmitLoadArg( delegateParam )
               .EmitIsInst( refFuncType )
               .EmitBranch( BranchType.IF_FALSE, elseLabel ),
            ( il2, elseLabel, endIfLabel ) => il2
               .EmitStoreToArgument( returnValueParam, il3 => il3
                  .EmitLoadArg( delegateParam )
                  .EmitCastToType( delegateParam.ParameterType, refFuncType )
                  .EmitLoadThisFieldAddress( field )
                  .EmitCall( REF_FUNCTION_INVOKER.ChangeDeclaringType( field.FieldType ) )
                  .EmitCastToType( field.FieldType, OBJECT_TYPE )
                  )
               .EmitLoadBoolean( true ),
            ( il2, endIfLabel ) =>
            {
               var refActionType = REF_ACTION_TYPE.MakeGenericType( field.FieldType );
               il2
               .EmitStoreToArgument( returnValueParam, il4 => il4.EmitLoadNull() )
               .EmitIfElse(
               ( il3, elseLabel, endIfLabel2 ) => il3
                  .EmitLoadArg( delegateParam )
                  .EmitIsInst( refActionType )
                  .EmitBranch( BranchType.IF_FALSE, elseLabel ),
               ( il3, elseLabel, endIfLabel2 ) => il3
                  .EmitLoadArg( delegateParam )
                  .EmitCastToType( delegateParam.ParameterType, refActionType )
                  .EmitLoadThisFieldAddress( field )
                  .EmitCall( REF_ACTION_INVOKER.ChangeDeclaringType( field.FieldType ) )
                  .EmitLoadBoolean( true ),
               ( il3, endIfLabel2 ) => il3
                  .EmitLoadBoolean( false )
               );
            } )
            .EmitReturn();

         return method;

      }
   }
}
#endif
