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
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Extensions.Functional.Model;
using Qi4CS.Extensions.Functional.Instance;

namespace Qi4CS.Extensions.Functional.Assembling
{


   internal sealed class FunctionAggregatorDeclarationImpl<TKey, TComposite> : FunctionAggregatorDeclaration<TKey, TComposite>
      where TComposite : class
   {
      //      private static readonly Object[] EMPTY_OBJECTS = new Object[] { };
      private static readonly Func<Object[], Object[]> NO_ARGS_TRANSFORM = args => args;
      private static readonly Action<Object[], Object[]> NO_POST_ARG_TRANSFORMER_ACTION = ( mArgs, fArgs ) => { };

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
                   Equals( typeof( RoleAttribute ), args.OldAttribute.GetType() )
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
                     var invocationHandlerAttributes = args2.Assemblies.Values.SelectMany( a => a.GetCustomAttributes().OfType<InvocationHandlerTypeAttribute>() ).ToArray();

                     foreach ( var kvp in info.FunctionMethods )
                     {
                        var methodIdx = FunctionAssemblerUtils.GetCompositeMethodIndex( args.Model, kvp.Key );
                        var attribute = invocationHandlerAttributes.FirstOrDefault( a => a.CompositeID == args.Model.CompositeModelID && a.CompositeMethodIndex == methodIdx );
                        if ( attribute != null )
                        {
                           info.InvocationHelpers.Add( kvp.Key, (FunctionInvocationHelper) attribute.InvocationHandlerType.GetConstructors().First().Invoke( null ) );
                        }
                     }
                  } );

#if QI4CS_SDK
                  args.Model.ApplicationModel.ApplicationCodeGenerationEvent += new EventHandler<ApplicationCodeGenerationArgs>( ( sender2, args2 ) =>
                  {
                     Qi4CSFunctionalExtensionCodeGeneration.GenerateType( args.Model, info, args2 );
                  } );
#endif
               }
            } );
         }
         return info;
      }
   }
}
