/*
 * Copyright 2015 Stanislav Muhametsin. All rights Reserved.
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
using System.Text;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Extensions.Functional.Assembling;

#if QI4CS_SDK
using CILAssemblyManipulator.API;
#endif

namespace Qi4CS.Extensions.Functional.Model
{
   /// <summary>
   /// The Qi4CS generated assemblies containing invocation handler helper types will be marked with this attribute.
   /// </summary>
   public class InvocationHandlerTypeAttribute : AbstractCompositeRelatedCodeGenerationAttribute
   {
      private readonly Int32 _compositeMethodIdx;
      private readonly Type _invocationHandlerType;

      /// <summary>
      /// Creates a new instance of <see cref="InvocationHandlerTypeAttribute"/>, affecting given composite ID and referencing given generated invocation handler helper type.
      /// </summary>
      /// <param name="compositeID">The ID of the composite in question.</param>
      /// <param name="compositeMethodIndex">The index of composite method in question.</param>
      /// <param name="invocationHandlerType">The type implementing <see cref="FunctionInvocationHelper"/>.</param>
      public InvocationHandlerTypeAttribute( Int32 compositeID, Int32 compositeMethodIndex, Type invocationHandlerType )
         : base( compositeID )
      {
         this._compositeMethodIdx = compositeMethodIndex;
         this._invocationHandlerType = invocationHandlerType;
      }

      /// <summary>
      /// Gets the composite method index of the affected composite.
      /// </summary>
      /// <value>The composite method index of the affected composite.</value>
      public Int32 CompositeMethodIndex
      {
         get
         {
            return this._compositeMethodIdx;
         }
      }

      /// <summary>
      /// Gets the generated type implementing <see cref="FunctionInvocationHelper"/>.
      /// </summary>
      /// <value>The generated type implementing <see cref="FunctionInvocationHelper"/>.</value>
      public Type InvocationHandlerType
      {
         get
         {
            return this._invocationHandlerType;
         }
      }
   }

   /// <summary>
   /// This interface is used in code generation for Qi4CS Functional extensions.
   /// It performs invoking of method of the aggregated object without reflection, if possible.
   /// </summary>
   public interface FunctionInvocationHelper
   {
      /// <summary>
      /// Invokes method on target aggregated object.
      /// </summary>
      /// <param name="composite">The aggregated object.</param>
      /// <param name="args">Arguments for the method.</param>
      /// <returns>Result of the method.</returns>
      Object InvokeMethod( Object composite, Object[] args );
   }

#if QI4CS_SDK
   internal static class Qi4CSFunctionalExtensionCodeGeneration
   {
      private static readonly System.Reflection.MethodInfo INVOCATION_HANDLER_METHOD = typeof( FunctionInvocationHelper ).LoadMethodOrThrow( "InvokeMethod", null );
      private static readonly System.Reflection.ConstructorInfo TYPE_INFO_ATTRIBUTE_CTOR = typeof( InvocationHandlerTypeAttribute ).LoadConstructorOrThrow( (Int32?) null );

      internal static void GenerateType<TKey, TComposite>( CompositeModel cModel, FunctionInfo<TKey, TComposite> info, ApplicationCodeGenerationArgs args )
      {
         foreach ( var kvp in info.FunctionMethods )
         {
            var sMethod = kvp.Value;
            if ( !sMethod.DeclaringType.ContainsGenericParameters && !sMethod.ContainsGenericParameters )
            {
               var compositeMethodIdx = FunctionAssemblerUtils.GetCompositeMethodIndex( cModel, kvp.Key );
               var typeName = "FunctionInvocationHandler_" + cModel.CompositeModelID + "_" + compositeMethodIdx;
               var curModule = args.TypeGenerationInformation[cModel][sMethod.DeclaringType.Assembly][0].Module;
               var tb = curModule.AddType( typeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed );

               tb.AddDefaultConstructor( MethodAttributes.Public | MethodAttributes.HideBySig );
               var ctx = tb.ReflectionContext;
               var invocationHandlerMethod = INVOCATION_HANDLER_METHOD.NewWrapper( ctx );
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
               var eSMethod = sMethod.NewWrapper( ctx );
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
               il.EmitCastToType( mb.Parameters[0].ParameterType, sMethod.DeclaringType.NewWrapper( ctx ) );
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
               il.EmitCall( sMethod.NewWrapper( ctx ) );

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
                  il.EmitCastToType( sMethod.ReturnType.NewWrapper( ctx ), mb.GetReturnType() );
               }
               il.EmitReturn();

               // Add attribute to the assembly
               var attrCtor = TYPE_INFO_ATTRIBUTE_CTOR.NewWrapper( ctx );
               curModule.Assembly.AddNewCustomAttributeTypedParams(
                  attrCtor,
                  CILCustomAttributeFactory.NewTypedArgument( cModel.CompositeModelID, ctx ),
                  CILCustomAttributeFactory.NewTypedArgument( compositeMethodIdx, ctx ),
                  CILCustomAttributeFactory.NewTypedArgument( (CILType) attrCtor.Parameters[2].ParameterType, tb )
                  );
            }
         }
      }
   }
#endif
}
