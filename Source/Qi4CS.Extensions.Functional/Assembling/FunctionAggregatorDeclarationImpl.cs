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
using System;
using System.Collections.Generic;
using System.Linq;
using Qi4CS.Core.Bootstrap.Assembling;
using CommonUtils;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Extensions.Functional.Model;
using Qi4CS.Extensions.Functional.Instance;

#if QI4CS_SDK
using CILAssemblyManipulator.API;
#endif

namespace Qi4CS.Extensions.Functional.Assembling
{
#if QI4CS_SDK
   internal static class FunctionAggregatorDeclarationCodeGenStatics
   {
      internal static readonly System.Reflection.MethodInfo INVOCATION_HANDLER_METHOD = typeof( FunctionInvocationHelper ).LoadMethodOrThrow( "InvokeMethod", null );
   }
#endif

   internal sealed class FunctionAggregatorDeclarationImpl<TKey, TComposite> : FunctionAggregatorDeclaration<TKey, TComposite>
      where TComposite : class
   {
      //      private static readonly Object[] EMPTY_OBJECTS = new Object[] { };
      private static readonly Func<Object[], Object[]> NO_ARGS_TRANSFORM = args => args;
      private static readonly Action<Object[], Object[]> NO_POST_ARG_TRANSFORMER_ACTION = ( mArgs, fArgs ) => { };

      private const String INVOCATION_HANDLER_PREFIX = "FunctionInvocationHandler_";

      private readonly ServiceCompositeAssemblyDeclaration _assemblyDeclaration;

      internal FunctionAggregatorDeclarationImpl( ServiceCompositeAssemblyDeclaration assemblyDeclaration )
      {
         ArgumentValidator.ValidateNotNull( "Service assembly declaration", assemblyDeclaration );

         this._assemblyDeclaration = assemblyDeclaration;
      }

      #region FunctionAggregatorDeclaration Members

      public FunctionAggregatorDeclaration<TKey, TComposite> WithFunctionType(
         Func<Object[], IEnumerable<TKey>> functionLookup
         )
      {
         var assemblyDeclaration = this._assemblyDeclaration;
         assemblyDeclaration.OfTypes( typeof( TComposite ) );
         foreach ( var type in typeof( TComposite ).GetAllParentTypes() )
         {
            foreach ( var method in type.GetMethods( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly ) )
            {
               this.WithFunctionMethod(
                  method,
                  functionLookup,
                  null
                  //null
                  );
            }
         }
         return this;
      }

      // TODO multiple key types for same function

      public FunctionAggregatorDeclaration<TKey, TComposite> WithFunctionMethod(
         System.Reflection.MethodInfo method,
         Func<Object[], IEnumerable<TKey>> functionLookup,
         Func<Application, IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, Lazy<TComposite>>>, Object[], Tuple<TKey, TComposite, Boolean>> funcWhenDirectLookupFails
         //Func<TKey, TComposite, Object[], IEnumerable<Tuple<String, Object>>> additionalRolesFunc
         )
      {
         var assemblyDeclaration = this._assemblyDeclaration;
         assemblyDeclaration.Assembler.ApplicationArchitecture.AttributeProcessingEvent += new EventHandler<AttributeProcessingArgs>(
            ( sender, args ) =>
            {
               if ( assemblyDeclaration.AffectedCompositeIDs.Contains( args.CompositeID ) &&
                   args.ReflectionElement is System.Reflection.ParameterInfo &&
                   ( (System.Reflection.ParameterInfo) args.ReflectionElement ).Member.Equals( method ) &&
                   typeof( RoleAttribute ).Equals( args.OldAttribute.GetType() )
                  )
               {
                  args.NewAttribute = new RoleParameterAttribute( ( (RoleAttribute) args.OldAttribute ).Name );
               }
            }
            );
         return this.MapServiceMethodToFunctionMethod(
            method,
            method,
            functionLookup,
            args => args,
            null,
            funcWhenDirectLookupFails
            //additionalRolesFunc
            );
      }

      public FunctionAggregatorDeclaration<TKey, TComposite> MapServiceMethodToFunctionMethod(
         System.Reflection.MethodInfo serviceMethod,
         System.Reflection.MethodInfo functionMethod,
         Func<Object[], IEnumerable<TKey>> functionLookup,
         Func<Object[], Object[]> preArgTransformer,
         Action<Object[], Object[]> postArgProcessor,
         Func<Application, IEnumerable<TKey>, IEnumerable<KeyValuePair<TKey, Lazy<TComposite>>>, Object[], Tuple<TKey, TComposite, Boolean>> funcWhenDirectLookupFails
         //Func<TKey, TComposite, Object[], IEnumerable<Tuple<String, Object>>> additionalRolesFunc
         )
      {
         ArgumentValidator.ValidateNotNull( "Service method", serviceMethod );
         ArgumentValidator.ValidateNotNull( "Function method", functionMethod );
         ArgumentValidator.ValidateNotNull( "Function lookup", functionLookup );

         var assemblyDeclaration = this._assemblyDeclaration;

         if ( preArgTransformer == null )
         {
            preArgTransformer = NO_ARGS_TRANSFORM;
         }
         if ( postArgProcessor == null )
         {
            postArgProcessor = NO_POST_ARG_TRANSFORMER_ACTION;
         }

         if ( serviceMethod.IsGenericMethod && !serviceMethod.IsGenericMethodDefinition )
         {
            serviceMethod = serviceMethod.GetGenericMethodDefinition();
         }

         //         var keyType = typeof( TKey );
         //         var functionType = typeof( TComposite );

         // TODO check that method generic args match between service and function method
         assemblyDeclaration.WithMixins( typeof( FunctionServiceRegisterMixin<TKey, TComposite> ) );
         assemblyDeclaration
            .WithMixins( typeof( FunctionServiceMixin<TKey, TComposite> ) )
            .ApplyWith( new AppliesToFilterFromFunction( ( cMethod, fMethod ) => serviceMethod.Equals( cMethod ) ) ); // Add mixin for function method

         var info = this.GetInfo();

         info.FunctionMethods[serviceMethod] = functionMethod;
         info.ArgsPreTransformers[serviceMethod] = preArgTransformer;
         info.ArgsPostTransformers[serviceMethod] = postArgProcessor;
         info.LookupFuncs[serviceMethod] = functionLookup;
         //if ( additionalRolesFunc != null )
         //{
         //   info.AdditionalRoles[serviceMethod] = additionalRolesFunc;
         //}
         if ( funcWhenDirectLookupFails != null )
         {
            info.DirectLookupFailFuncs[serviceMethod] = funcWhenDirectLookupFails;
         }

         return this;
      }

      public FunctionAggregatorDeclaration<TKey, TComposite> WithDefaultFunctions( params Tuple<TKey[], Func<StructureServiceProvider, TComposite>>[] keysAndComposite )
      {
         if ( keysAndComposite != null )
         {
            var info = this.GetInfo();
            foreach ( var creator in keysAndComposite.Where( t => t != null && t.Item2 != null ) )
            {
               info.DefaultFunctions.Add( creator );
            }
         }
         return this;
      }

      public FunctionAggregatorDeclaration<TKey, TComposite> WithEqualityComparer( IEqualityComparer<TKey> equalityComparer )
      {
         this._assemblyDeclaration.WithDefaultFor(
            typeof( FunctionServiceState<TKey, TComposite> ).LoadPropertyOrThrow( "Functions" ),
            ( pInfo, app ) =>
#if SILVERLIGHT
 new Dictionary<TKey, Lazy<TComposite>>( equalityComparer )
#else
 new System.Collections.Concurrent.ConcurrentDictionary<TKey, Lazy<TComposite>>( equalityComparer )
#endif
 );
         return this;
      }

      public ServiceCompositeAssemblyDeclaration Done()
      {
         return this._assemblyDeclaration;
      }

      #endregion

      private FunctionInfo<TKey, TComposite> GetInfo()
      {
         ServiceCompositeAssemblyDeclaration assemblyDeclaration = this._assemblyDeclaration;
         FunctionInfo<TKey, TComposite> info = (FunctionInfo<TKey, TComposite>) assemblyDeclaration.Get<FunctionInfo<TKey, TComposite>>();
         if ( info == null )
         {
            info = new FunctionInfo<TKey, TComposite>( this._assemblyDeclaration );
            assemblyDeclaration.Use( info );
            assemblyDeclaration.Assembler.ApplicationArchitecture.CompositeModelCreatedEvent += new EventHandler<CompositeModelCreatedArgs>( ( sender, args ) =>
            {
               // Subscribe to validation event - in order to generate or load code for invocation helpers.
               if ( assemblyDeclaration.AffectedCompositeIDs.Contains( args.Model.CompositeModelID ) )
               {
                  args.Model.ApplicationModel.ApplicationCodeResolveEvent += new EventHandler<ApplicationCodeResolveArgs>( ( sender2, args2 ) =>
                  {
                     foreach ( var kvp in info.FunctionMethods )
                     {
                        var typeName = this.GetInvocationHandlerName( args.Model, kvp.Key );
                        var t = args2.Assemblies[kvp.Value.DeclaringType.Assembly].GetType( typeName, false );
                        if ( t != null )
                        {
                           info.InvocationHelpers.Add( kvp.Key, (FunctionInvocationHelper) t.GetConstructors().First().Invoke( null ) );
                        }
                     }
                  } );

#if QI4CS_SDK
                  args.Model.ApplicationModel.ApplicationCodeGenerationEvent += new EventHandler<ApplicationCodeGenerationArgs>( ( sender2, args2 ) =>
                  {
                     this.GenerateType( args.Model, info, args2 );
                  } );
#endif
               }
            } );
         }
         return info;
      }

      private String GetInvocationHandlerName( CompositeModel model, System.Reflection.MethodInfo method )
      {
         return INVOCATION_HANDLER_PREFIX + model.CompositeModelID + "_" + model.Methods.Single( cMethod => cMethod.NativeInfo.Equals( method ) ).MethodIndex;
      }

#if QI4CS_SDK
      private void GenerateType( CompositeModel cModel, FunctionInfo<TKey, TComposite> info, ApplicationCodeGenerationArgs args )
      {
         foreach ( var kvp in info.FunctionMethods )
         {
            var sMethod = kvp.Value;
            if ( !sMethod.DeclaringType.ContainsGenericParameters && !sMethod.ContainsGenericParameters )
            {
               var typeName = this.GetInvocationHandlerName( cModel, kvp.Key );
               var tb = args.TypeGenerationInformation[cModel][sMethod.DeclaringType.Assembly].Module.AddType( typeName, TypeAttributes.Public | TypeAttributes.Class );

               tb.AddDefaultConstructor( MethodAttributes.Public | MethodAttributes.HideBySig );

               var invocationHandlerMethod = FunctionAggregatorDeclarationCodeGenStatics.INVOCATION_HANDLER_METHOD.NewWrapper( tb.ReflectionContext );
               tb.SetParentType( invocationHandlerMethod.DeclaringType );
               var mb = tb.AddMethod( invocationHandlerMethod.Name, MethodAttributesUtils.EXPLICIT_IMPLEMENTATION_ATTRIBUTES, CallingConventions.Standard );
               mb.ReturnParameter.ParameterType = invocationHandlerMethod.GetReturnType();
               mb.AddOverriddenMethods( invocationHandlerMethod );
               foreach ( var p in invocationHandlerMethod.Parameters )
               {
                  mb.AddParameter( p.Name, p.Attributes, p.ParameterType );
               }

               var il = mb.MethodIL;
               // return ((<composite type>)composite).<method>((<first param type>)args[0], (<second param type>)args[1], ...);
               var arrayElementType = mb.Parameters[1].ParameterType.GetElementType();
               var eSMethod = sMethod.NewWrapper( tb.ReflectionContext );
               var localsArray = new LocalBuilder[eSMethod.Parameters.Count];
               for ( var i = 0; i < localsArray.Length; ++i )
               {
                  if ( eSMethod.Parameters[i].ParameterType.IsByRef() )
                  {
                     localsArray[i] = il.DeclareLocal( eSMethod.Parameters[i].ParameterType.GetElementType() );
                     il.EmitLoadArg( 2 )
                        .EmitLoadInt32( i )
                        .EmitLoadElement( arrayElementType )
                        .EmitCastToType( arrayElementType, localsArray[i].LocalType )
                        .EmitStoreLocal( localsArray[i] );
                  }
               }

               il.EmitLoadArg( 1 );
               il.EmitCastToType( mb.Parameters[0].ParameterType, sMethod.DeclaringType.NewWrapper( tb.ReflectionContext ) );
               foreach ( var param in eSMethod.Parameters )
               {
                  if ( localsArray[param.Position] == null )
                  {
                     var pType = param.ParameterType;
                     il.EmitLoadArg( 2 )
                       .EmitLoadInt32( param.Position )
                       .EmitLoadElement( arrayElementType )
                       .EmitCastToType( arrayElementType, pType );
                  }
                  else
                  {
                     il.EmitLoadLocalAddress( localsArray[param.Position] );
                  }
               }
               il.EmitCall( sMethod.NewWrapper( tb.ReflectionContext ) );

               for ( var i = 0; i < localsArray.Length; ++i )
               {
                  if ( localsArray[i] != null )
                  {
                     il.EmitLoadArg( 2 )
                        .EmitLoadInt32( i )
                        .EmitLoadLocal( localsArray[i] )
                        .EmitStoreElement( arrayElementType );
                  }
               }

               if ( typeof( void ).Equals( sMethod.ReturnType ) )
               {
                  il.EmitLoadNull();
               }
               else
               {
                  il.EmitCastToType( sMethod.ReturnType.NewWrapper( tb.ReflectionContext ), mb.GetReturnType() );
               }
               il.EmitReturn();
            }
         }
      }
#endif
   }
}
