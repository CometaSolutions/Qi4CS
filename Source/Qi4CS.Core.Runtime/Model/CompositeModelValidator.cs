/*
 * Copyright 2011 Stanislav Muhametsin. All rights Reserved.
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
using System.Runtime.CompilerServices;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Model;
using CommonUtils;

namespace Qi4CS.Core.Runtime.Model
{

   public class CompositeModelValidator
   {
      public virtual void ValidateComposite( CompositeValidationResultMutable result, CompositeModel compositeModel )
      {
         // TODO check that all keys of the type model CompositeTypeInformations and FragmentTypeInformations are assignable from all methods of the composite model.
         this.ValidateCompositeTypeModel( result, compositeModel );

         if ( compositeModel.MainCodeGenerationType == null )
         {
            result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Composite model main code generation type was null", compositeModel ) );
         }
         else if ( !compositeModel.PublicTypes.Contains( compositeModel.MainCodeGenerationType ) )
         {
            result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Composite model main code generation type " + compositeModel.MainCodeGenerationType + " was not contained in its public types (" + String.Join( ", ", compositeModel.PublicTypes ) + ").", compositeModel ) );
         }
         else
         {

            CompositeMethodModel[] unimplementedMethods = compositeModel.Methods
               .Where( methodModel => methodModel.Mixin == null )
               .ToArray();
            if ( unimplementedMethods.Any() )
            {
               foreach ( CompositeMethodModel unimplementedMethod in unimplementedMethods )
               {
                  result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "No implementation found for " + unimplementedMethod.NativeInfo + ".", compositeModel, unimplementedMethod ) );
               }
            }
            else
            {
               foreach ( FieldModel fieldModel in compositeModel.Fields )
               {
                  this.ValidateField( result, compositeModel, fieldModel );
               }

               foreach ( ConstructorModel constructorModel in compositeModel.Constructors )
               {
                  foreach ( ParameterModel parameterModel in constructorModel.Parameters )
                  {
                     this.ValidateParameter( result, compositeModel, parameterModel );
                  }
               }

               foreach ( CompositeMethodModel compositeMethod in compositeModel.Methods )
               {
                  if ( compositeMethod == null )
                  {
                     result.InternalValidationErrors.Add( ValidationErrorFactory.NewInternalError( "Composite method model may not be null.", compositeModel ) );
                  }
                  else
                  {
                     this.ValidateParameter( result, compositeModel, compositeMethod.Result );
                     foreach ( ParameterModel parameterModel in compositeMethod.Parameters )
                     {
                        this.ValidateParameter( result, compositeModel, parameterModel );
                     }
                     foreach ( AbstractFragmentMethodModel fragmentMethod in compositeMethod.GetAllMethodModels().OfType<AbstractFragmentMethodModel>() )
                     {
                        this.ValidateFragmentMethod( result, compositeModel, compositeMethod, fragmentMethod );
                     }
                     foreach ( ConstraintModel constraintModel in compositeMethod.Parameters.SelectMany( param => param.Constraints ).Concat( compositeMethod.Result.Constraints ) )
                     {
                        if ( constraintModel.ConstraintType == null )
                        {
                           result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Could not bind parameter constraint " + constraintModel.ConstraintAttribute + ".", compositeModel, compositeMethod ) );
                        }
                        else if ( constraintModel.ConstraintType.GetConstructor( Empty<Type>.Array ) == null )
                        {
                           result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Constraints must have constructor without parameters, but constraint of type " + constraintModel.ConstraintType + " does not.", compositeModel, compositeMethod ) );
                        }
                        else if ( constraintModel.ConstraintType.ContainsGenericParameters() && !constraintModel.ConstraintType.IsGenericTypeDefinition() )
                        {
                           result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Constraint type " + constraintModel.ConstraintType + " contains non-closed generic parameters but is not generic type definition.", compositeModel, compositeMethod ) );
                        }
                     }
                  }
               }
               foreach ( var pModel in compositeModel.GetAllPropertyModels() )
               {
                  this.ValidatePropertyModel( result, compositeModel, pModel );
               }
               foreach ( SpecialMethodModel specialMethod in compositeModel.SpecialMethods )
               {
                  this.ValidateSpecialMethodModel( result, compositeModel, specialMethod );
               }

               foreach ( var constraintModel in compositeModel.GetAllConstraints() )
               {
                  this.ValidateConstraintModel( result, compositeModel, constraintModel );
               }

               if ( !result.IQ.HasAnyErrors() )
               {
                  result.TypeModel = new CompositeTypeModelImpl( compositeModel, result );
               }
            }
         }
      }

      protected virtual void ValidatePropertyModel( CompositeValidationResult result, CompositeModel model, PropertyModel property )
      {
         if ( property.IsPartOfCompositeState() )
         {
            UseDefaultsAttribute udAttr;
            if ( property.IsUseDefaults( out udAttr ) )
            {
               if ( udAttr.ActualType != null && !property.NativeInfo.PropertyType.IsAssignableFrom( udAttr.ActualType ) )
               {
                  result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The type " + property.NativeInfo.PropertyType + " of " + property.NativeInfo + " is not assignable from type given to " + typeof( UseDefaultsAttribute ) + " constructor ( " + udAttr.ActualType + ").", model, property ) );
               }
               else if ( property.NativeInfo.PropertyType.IsGenericParameter )
               {
                  result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The type " + property.NativeInfo.PropertyType + " of " + property.NativeInfo + " is marked with " + typeof( UseDefaultsAttribute ) + " but it is generic type parameter.", model, property ) );
               }
            }
         }
      }

      protected virtual void ValidateConstraintModel( CompositeValidationResult result, CompositeModel model, ConstraintModel constraint )
      {
         if ( constraint.ConstraintType == null )
         {
            result.InternalValidationErrors.Add( ValidationErrorFactory.NewInternalError( "Constraint model had null as constraint type", constraint ) );
         }
      }
      protected virtual void ValidateFragmentMethod( CompositeValidationResult result, CompositeModel compositeModel, CompositeMethodModel compositeMethod, AbstractFragmentMethodModel methodModel )
      {
         if ( compositeMethod == null )
         {
            result.InternalValidationErrors.Add( ValidationErrorFactory.NewInternalError( "Composite method model may not be null.", compositeModel ) );
         }
         if ( methodModel != null )
         {
            var declType = methodModel.NativeInfo.DeclaringType;
            if ( !declType
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
               .IsClass )
            {
               result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The declaring type of fragment method " + methodModel.NativeInfo + " must be a class, however " + declType + " is not a class.", compositeModel, compositeMethod ) );
            }
            if ( !methodModel.IsGeneric && ( !methodModel.NativeInfo.IsVirtual || methodModel.NativeInfo.IsFinal ) )
            {
               result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The method " + methodModel.NativeInfo + " in " + declType + " is not virtual, however, all composite methods must be virtual.", compositeModel, methodModel ) );
            }
            if ( declType
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
               .IsSealed )
            {
               result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The type " + declType + " is sealed, however, all fragment types must be non-sealed.", compositeModel, methodModel ) );
            }
            //if ( !declType.IsPublic && !declType.IsNestedPublic )
            //{
            //   String msg = null;
            //   if ( methodModel.NativeInfo.IsAssembly || methodModel.NativeInfo.IsFamilyOrAssembly )
            //   {
            //      if ( !declType.Assembly.GetCustomAttributes( true ).OfType<InternalsVisibleToAttribute>().Any( attr => Qi4CSGeneratedAssemblyAttribute.ASSEMBLY_NAME.Equals( attr.AssemblyName ) ) )
            //      {
            //         msg = "The type " + declType + " is marked as internal, however, the " + typeof( InternalsVisibleToAttribute ) + " with argument " + typeof( Qi4CSGeneratedAssemblyAttribute ) + ".ASSEMBLY_NAME is not applied to the assembly";
            //      }
            //   }
            //   else
            //   {
            //      msg = "The type " + declType + " is not visible to the generated assembly.";
            //   }
            //   if ( msg != null )
            //   {
            //      result.AddStructureError( new StructureValidationErrorImpl( compositeModel, methodModel, msg ) );
            //   }
            //}
            var genName = Qi4CSGeneratedAssemblyAttribute.GetGeneratedAssemblyName( declType.GetAssembly() );
            if ( !IsTypeVisible( declType, genName ) )
            {
               result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The type " + declType + " is not visible. Consider either making it public, or internal with combination of applying " + typeof( InternalsVisibleToAttribute ) + " with argument " + typeof( Qi4CSGeneratedAssemblyAttribute ) + ".ASSEMBLY_NAME to the assembly.", compositeModel, methodModel ) );
            }

         }
      }

      private static Boolean IsTypeVisible( Type type, String generatedAssemblyName )
      {
         return type
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
               .IsVisible || ( !type
#if WINDOWS_PHONE_APP
               .GetTypeInfo()
#endif
               .IsNestedPrivate && type.GetAssembly().GetCustomAttributes( ).OfType<InternalsVisibleToAttribute>().Any( attr =>
         {
            var assName = attr.AssemblyName;
            var idx = assName.IndexOf( Qi4CSGeneratedAssemblyAttribute.ASSEMBLY_PUBLIC_KEY_SUFFIX );
            if ( idx != -1 )
            {
               assName = assName.Substring( 0, idx );
            }
            return String.Equals( assName, generatedAssemblyName );
         } ) );
      }

      protected virtual void ValidateField( CompositeValidationResult result, CompositeModel compositeModel, FieldModel fieldModel )
      {
         if ( fieldModel == null )
         {
            result.InternalValidationErrors.Add( ValidationErrorFactory.NewInternalError( "Field model may not be null.", compositeModel ) );
         }
         else
         {
            this.ValidateInjectableModel( result, compositeModel, fieldModel );
         }
      }

      protected virtual void ValidateParameter( CompositeValidationResult result, CompositeModel compositeModel, ParameterModel parameterModel )
      {
         this.ValidateParameterType( result, compositeModel, (AbstractMemberInfoModel<MemberInfo>) parameterModel.Owner, parameterModel, parameterModel.NativeInfo.ParameterType );
         this.ValidateInjectableModel( result, compositeModel, parameterModel );
      }

      protected virtual void ValidateParameterType( CompositeValidationResult result, CompositeModel compositeModel, AbstractMemberInfoModel<MemberInfo> memberModel, AbstractInjectableModel injectableModel, Type type )
      {
         if ( this.IsCompositeTypeAffectingInjection( injectableModel.InjectionScope ) )
         {
            Stack<Type> stk = new Stack<Type>();
            stk.Push( type );
            while ( stk.Any() )
            {
               Type current = stk.Pop();
               if ( current.IsGenericParameter && current
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
                  .DeclaringMethod != null )
               {
                  result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Injection scopes affecting composite type (like " + typeof( ThisAttribute ) + ", " + typeof( ConcernForAttribute ) + ", or " + typeof( SideEffectForAttribute ) + " are not allowed on method argument, which is referencing a generic method parameter.", compositeModel, memberModel ) );
               }
               else if ( !current.IsGenericParameter && current.ContainsGenericParameters() )
               {
                  foreach ( Type gArg in current.GetGenericArguments() )
                  {
                     stk.Push( gArg );
                  }
               }
            }
         }
      }

      protected virtual Boolean IsCompositeTypeAffectingInjection( Attribute attribute )
      {
         return attribute is ThisAttribute || attribute is ConcernForAttribute || attribute is SideEffectForAttribute;
      }

      protected virtual void ValidateCompositeType( CompositeValidationResult result, CompositeModel compositeModel, Type compositeType )
      {
         if ( !compositeType
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
            .IsInterface && !compositeType
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
            .IsClass )
         {
            result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Composites must be interfaces or class; the composite " + compositeType + " however is neither.", compositeModel ) );
         }
      }

      protected virtual void ValidateFragmentType( CompositeValidationResult result, CompositeModel compositeModel, Type fragmentType )
      {
         if ( !fragmentType
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
            .IsClass )
         {
            result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Fragments must be classes; the fragment " + fragmentType + " however is not a class.", compositeModel ) );
         }
         else
         {
            var baseTypes = new HashSet<Type>( fragmentType.GetAllParentTypes() );
            baseTypes.Remove( fragmentType );
            if ( fragmentType
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
               .IsAbstract )
            {
               foreach ( var fMethod in fragmentType.GetAllInstanceMethods() )
               {
                  if ( fMethod.IsAbstract && !compositeModel.Methods.Any( cMethod => AreSameFragmentMethods( fMethod, Types.FindMethodImplicitlyImplementingMethod( fragmentType, cMethod.NativeInfo ) ) ) )
                  {
                     result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Found abstract method " + fMethod + " in " + fragmentType + ", with no corresponding composite method.", compositeModel ) );
                  }
               }
            }
            foreach ( var baseType in baseTypes )
            {
               if ( baseType.IsGenericType() )
               {
                  var baseTypeGDef = baseType.GetGenericTypeDefinition();
                  if ( compositeModel.GetAllCompositeTypes().Except( compositeModel.PublicTypes ).SelectMany( cType => cType.GetAllParentTypes() ).Any( pType => pType.GetGenericDefinitionIfGenericType().Equals( baseTypeGDef ) ) &&
                       baseType.GetGenericArguments().Any( gArg => gArg.IsArray || gArg.IsByRef || gArg.IsPointer ) )
                  {
                     result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Fragment type implements composite type with either array, by ref or pointer generic argument. This is not allowed, as fragment might not be instantiatable in cases of non-array or non-by-ref or non-pointer generic argument of composite type.", compositeModel ) );
                  }
               }
            }
         }
      }

      protected virtual void ValidateCompositeTypeModel( CompositeValidationResult result, CompositeModel compositeModel )
      {
         // TODO validate that none of fragments have abstract non-composite methods.

         if ( compositeModel.PublicTypes.Any() )
         {
            Type firstType = compositeModel.PublicTypes.First();
            Type[] gArgs = firstType.GetGenericArguments();
            Int32 gArgsCount = gArgs.Length;
            foreach ( Type cType in compositeModel.GetAllCompositeTypes() )
            {
               this.ValidateCompositeType( result, compositeModel, cType );
            }

            foreach ( Type fType in compositeModel.GetAllFragmentTypes() )
            {
               this.ValidateFragmentType( result, compositeModel, fType );
            }

            foreach ( Type type in compositeModel.PublicTypes )
            {
               this.ValidateCompositeType( result, compositeModel, type );

               Type[] typeGargs = type.GetGenericArguments();
               Int32 typeCount = typeGargs.Length;

               if ( typeCount > 0 )
               {
                  if ( type.IsGenericTypeDefinition() != firstType.IsGenericTypeDefinition() || typeGargs.Take( Math.Min( gArgsCount, typeCount ) ).Where( ( tGArg, idx ) => tGArg.IsGenericParameter != gArgs[idx].IsGenericParameter ).Any() )
                  {
                     result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "All type interfaces must be either genericless, generic type definitions, or having all generic parameters closed.", compositeModel ) );
                  }
                  else
                  {
                     // TODO generic arguments constraints
                  }
               }
            }
            if ( compositeModel.PublicTypes.Count( pType => !pType
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
               .IsInterface ) > 1 )
            {
               result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Composite types must contain at most one class, but found the following classes:\n" + String.Join( ", ", compositeModel.PublicTypes.Select( pType => !pType
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
                  .IsInterface ) ), compositeModel ) );
            }
         }
         else
         {
            result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Composite must have at least one public type", compositeModel ) );
         }
      }

      protected virtual void ValidateInjectableModel( CompositeValidationResult result, CompositeModel compositeModel, AbstractInjectableModel model )
      {
         Int32 amount = model.GetAttributesMarkedWith( typeof( InjectionScopeAttribute ) ).Count;
         if ( amount > 1 )
         {
            result.InjectionValidationErrors.Add( ValidationErrorFactory.NewInjectionError( "Only one injection permitted for field or parameter.", model ) );
         }
         else
         {
            Attribute attr = model.InjectionScope;
            if ( attr != null )
            {
               InjectionService injectionService = compositeModel.ApplicationModel.InjectionService;
               if ( injectionService.HasFunctionalityFor( attr ) )
               {
                  if ( !model.IsOptional )
                  {
                     var validationResult = injectionService.InjectionPossible( model );
                     if ( validationResult == null || !validationResult.InjectionPossible )
                     {
                        result.InjectionValidationErrors.Add( ValidationErrorFactory.NewInjectionError( "Injection was not possible" + ( validationResult == null ? "." : ( ": " + validationResult.AdditionalMessage ) ), model ) );
                     }
                  }
               }
               else if ( !model.IsOptional )
               {
                  result.InjectionValidationErrors.Add( ValidationErrorFactory.NewInjectionError( "Could not find injection functionality for attribute " + attr + ".", model ) );
               }
            }
            else if ( model is FieldModel )
            {
               result.InternalValidationErrors.Add( ValidationErrorFactory.NewInternalError( "Injection attribute was null", model ) );
            }
         }
      }

      protected virtual void ValidateSpecialMethodModel( CompositeValidationResult result, CompositeModel compositeModel, SpecialMethodModel methodModel )
      {
         if ( methodModel.NativeInfo.IsGenericMethodDefinition )
         {
            result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Special methods can not be generic method definitions.", compositeModel, methodModel ) );
         }
         else
         {
            var declType = methodModel.NativeInfo.DeclaringType;
            var genName = Qi4CSGeneratedAssemblyAttribute.GetGeneratedAssemblyName( declType.GetAssembly() );
            if ( ( methodModel.NativeInfo.IsAssembly || methodModel.NativeInfo.IsFamilyAndAssembly )
                && !IsTypeVisible( declType, genName ) )
            {
               result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The type " + declType + " is not visible. Consider either making it public, or internal with combination of applying " + typeof( InternalsVisibleToAttribute ) + " with argument " + typeof( Qi4CSGeneratedAssemblyAttribute ) + ".ASSEMBLY_NAME to the assembly.", compositeModel, methodModel ) );
            }
            foreach ( var pModel in methodModel.Parameters )
            {
               this.ValidateParameter( result, compositeModel, pModel );
            }
         }
      }

      protected virtual void ValidateEventModel( CompositeValidationResult result, CompositeModel compositeModel, EventModel eventModel )
      {
         if ( !typeof( MulticastDelegate ).IsAssignableFrom( eventModel.NativeInfo.EventHandlerType ) )
         {
            result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "All event types must be sub-types of " + typeof( MulticastDelegate ) + ".", compositeModel, eventModel ) );
         }
         else
         {
            IEnumerable<EventInvocationStyleAttribute> attrs = eventModel.AllAttributes.OfType<EventInvocationStyleAttribute>();
            if ( attrs.Count() > 1 )
            {
               result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Maximum of one " + typeof( EventInvocationStyleAttribute ) + " may be applied on event.", compositeModel, eventModel ) );
            }
            else
            {
               EventInvocationStyleAttribute attr = attrs.FirstOrDefault();
               if ( attr != null && attr.RethrowException != null )
               {
                  if ( !typeof( Exception ).IsAssignableFrom( attr.RethrowException ) )
                  {
                     result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The exception type must be derived from " + typeof( Exception ), compositeModel, eventModel ) );
                  }
                  else if ( attr.RethrowException.GetConstructor( new Type[] { typeof( Exception[] ) } ) == null )
                  {
                     result.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "The exception to rethrow on event " + eventModel + " invocation must have a public constructor with the following parameters: " + typeof( Exception[] ) + ".", compositeModel, eventModel ) );
                  }
               }
            }
         }
         //else
         //{
         //   Type[] violatingTypes = this.GetAllFragmentMethodsOf( eventModel.AddMethod )
         //      .Concat( this.GetAllFragmentMethodsOf( eventModel.RemoveMethod ) )
         //      .Where( fMethod => !fMethod.NativeInfo.IsAbstract )
         //      .Select( fMethod => fMethod.NativeInfo.DeclaringType )
         //      .Distinct()
         //      .ToArray();
         //   if ( violatingTypes.Any() )
         //   {
         //      result.AddStructureError( new StructureValidationErrorImpl( compositeModel, eventModel, "The following fragment types have non-abstract event declaration: " + String.Join( ", ", (Object[]) violatingTypes ) + "." ) );
         //   }
         //}
      }

      protected static Boolean AreSameFragmentMethods( MethodInfo first, MethodInfo second )
      {
         var result = first != null
            && second != null
            && first.Name == second.Name
            && first.ReturnType == second.ReturnType;
         if ( result )
         {
            var p1 = first.GetParameters();
            var p2 = second.GetParameters();
            result = p1.Length == p2.Length
            && p1.Where( ( p, idx ) => p.ParameterType == p2[idx].ParameterType ).Count() == p1.Length;
         }
         return result;
      }
   }
}
