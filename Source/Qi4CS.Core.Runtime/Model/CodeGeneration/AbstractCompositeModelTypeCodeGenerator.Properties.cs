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
using System.Linq;
using CILAssemblyManipulator.Logical;
using CILAssemblyManipulator.Physical;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public abstract partial class AbstractCompositeModelTypeCodeGenerator
   {
      protected const String PROPERTY_FIELD_PREFIX = "_property";
      protected const String PROPERTY_METHOD_PREFIX = "Property";
      protected const String PROPERTY_GETTER_POSTFIX = "Getter";
      protected const String PROPERTY_GETTER32_POSTFIX = "Getter32";
      protected const String PROPERTY_SETTER_POSTFIX = "Setter";
      protected const String PROPERTY_EXCHANGE_POSTFIX = "Exchange";
      protected const String PROPERTY_COMPARE_EXCHANGE_POSTFIX = "CompareExchange";

      protected void EmitPropertySetterMethod(
         PropertyModel propertyModel,
         CompositeTypeGenerationInfo thisGenerationInfo,
         CILField propertyField,
         CILTypeBase fieldType,
         CILTypeBase propertyType,
         CompositeMethodGenerationInfo methodGenInfo,
         Action<CILField, MethodIL> writeAction
      )
      {
         // Code for setting properties:
         // internal void Property<idx>Setter(<property type> value )
         // {
         //    CompositeInstance instance = this._instance;
         //    <if the property is immutable>
         //    if ( !this._instance.IsPrototype )
         //    {
         //       throw new InvalidOperationException( "Can not set immutable propery " + QualifiedName.FromTypeAndName( <declaring type>, <name> ) + " for a non-prototype composite instance." );
         //    }
         //    <end if>
         //    <write property field>
         // }
         var il = methodGenInfo.IL;
         il.EmitLoadThisField( thisGenerationInfo.CompositeField )
           .EmitStoreLocal( methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, this.ctx.NewWrapper( this.codeGenInfo.CompositeInstanceFieldType ) ) );

         this.EmitThrowIfApplicationNotActive( methodGenInfo );

         this.EmitProcessParameters( propertyModel.SetterMethod, true, thisGenerationInfo, methodGenInfo );

         this.EmitThrowIfViolations( thisGenerationInfo, methodGenInfo, propertyModel.SetterMethod );

         this.EmitCheckPropertyImmutability( propertyModel, thisGenerationInfo, il );

         writeAction( propertyField, il );

         il
            .EmitPop()
            .EmitReturn();
      }

      protected void EmitCheckPropertyImmutability(
         PropertyModel propertyModel,
         CompositeTypeGenerationInfo thisGenerationInfo,
         MethodIL il
         )
      {
         if ( propertyModel.IsImmutable )
         {
            var setPropertyLabelWrapper = il.DefineLabel();
            il
               .EmitLoadThisField( thisGenerationInfo.CompositeField )
               .EmitCall( IS_PROTOTYPE_GETTER )
               .EmitBranch( BranchType.IF_TRUE, setPropertyLabelWrapper )

               .EmitLoadString( "Can not set immutable property " )
               .EmitReflectionObjectOf( thisGenerationInfo.Parents[this.ctx.NewWrapperAsType( propertyModel.NativeInfo.DeclaringType )] )
               .EmitLoadString( propertyModel.NativeInfo.Name )
               .EmitCall( QNAME_FROM_TYPE_AND_NAME )
               .EmitCall( TO_STRING_METHOD )
               .EmitLoadString( "for a non-prototype composite instance." )
               .EmitCall( STRING_CONCAT_METHOD_3 )
               .EmitThrowNewException( INVALID_OPERATION_EXCEPTION_CTOR_WITH_STRING )

               .MarkLabel( setPropertyLabelWrapper );
         }
      }

      protected void EmitPropertyGetterMethod(
         PropertyModel propertyModel,
         CompositeTypeGenerationInfo thisGenerationInfo,
         CILField propertyField,
         CILTypeBase propertyType,
         CompositeMethodGenerationInfo methodGenInfo,
         Action<CILField, MethodIL> readAction
         )
      {
         // Code for getting properties:
         // public <property type> Property<idx>Getter( )
         // {
         //    CompositeInstance instance = this._instance;
         //    <check application active>
         //    var result = <read property field>;
         //    <check constraints>
         //    return result;
         // }
         var il = methodGenInfo.IL;

         il.EmitLoadThisField( thisGenerationInfo.CompositeField )
           .EmitStoreLocal( methodGenInfo.GetOrCreateLocal( LB_C_INSTANCE, this.ctx.NewWrapper( this.codeGenInfo.CompositeInstanceFieldType ) ) );
         this.EmitThrowIfApplicationNotActive( methodGenInfo );

         var resultB = methodGenInfo.GetOrCreateLocal( LB_RESULT, methodGenInfo.ReturnType );

         readAction( propertyField, il );
         il.EmitStoreLocal( resultB );

         this.EmitProcessParameters( propertyModel.GetterMethod, false, thisGenerationInfo, methodGenInfo );
         this.EmitProcessResult( propertyModel.GetterMethod, thisGenerationInfo, methodGenInfo );

         this.EmitThrowIfViolations( thisGenerationInfo, methodGenInfo, propertyModel.GetterMethod );

         il.EmitLoadLocal( resultB )
           .EmitReturn();
      }

      protected void EmitPropertyExchangeMethod(
         PropertyModel propertyModel,
         CompositeTypeGenerationInfo thisGenerationInfo,
         CILField propertyField,
         CILTypeBase fieldType,
         CILTypeBase propertyType,
         CILMethod propertySetter,
         CompositeMethodGenerationInfo methodGenInfo,
         Action<CILField, MethodIL> exchangeAction
         )
      {
         var il = methodGenInfo.IL;
         this.EmitThrowIfApplicationNotActiveWithoutLocalVariable( thisGenerationInfo, il );

         this.EmitCheckPropertyImmutability( propertyModel, thisGenerationInfo, il );

         exchangeAction( propertyField, il );
         il
            .EmitCastToType( fieldType, propertyType )
            .EmitReturn();
      }

      protected void EmitPropertyCompareExchangeMethod(
         PropertyModel propertyModel,
         CompositeTypeGenerationInfo thisGenerationInfo,
         CILField propertyField,
         CompositeMethodGenerationInfo methodGenInfo,
         Action<CILField, MethodIL> compareExchangeAction
         )
      {
         var il = methodGenInfo.IL;
         this.EmitThrowIfApplicationNotActiveWithoutLocalVariable( thisGenerationInfo, il );

         this.EmitCheckPropertyImmutability( propertyModel, thisGenerationInfo, il );

         // return Interlocked.CompareExchange(ref this._property, <arg-2>, <arg-1>);
         compareExchangeAction( propertyField, il );
         il.EmitReturn();
      }

      protected void EmitPropertyRelatedThings(
         CompositeTypeGenerationInfo thisGenerationInfo,
         CompositeMethodGenerationInfo thisMethodGenerationInfo,
         PropertyModel propertyModel,
         Type genericPropertyMixinType
         )
      {
         var nPropertyInfo = propertyModel.NativeInfo;
         var propertyInfo = this.ctx.NewWrapper( nPropertyInfo );
         if ( this.IsCompositeGeneratedProperty( thisGenerationInfo, propertyModel, genericPropertyMixinType ) )
         {
            var propertyIdx = thisGenerationInfo.AutoGeneratedPropertyInfos.Count;
            var propertyType = TypeGenerationUtils.CreateTypeForEmitting( propertyInfo.GetPropertyType(), thisGenerationInfo.GenericArguments, null );

            CILTypeBase fieldType;
            Action<CILField, MethodIL> readMethod;
            Action<CILField, MethodIL> read32Method;
            Action<CILField, MethodIL> writeMethod;
            Action<CILField, MethodIL> compareExchangeMethodEmitter;
            this.GetReadAndWriteMethods( propertyType, out readMethod, out read32Method, out writeMethod, out compareExchangeMethodEmitter, out fieldType );

            // Field:
            // private <property type or Object> _property<idx>;
            var propertyField = thisGenerationInfo.Builder.AddField(
               PROPERTY_FIELD_PREFIX + propertyIdx,
               fieldType,
               FieldAttributes.Private
               );

            // Field getter
            var getter = thisGenerationInfo.Builder.AddMethod(
               PROPERTY_METHOD_PREFIX + propertyIdx + PROPERTY_GETTER_POSTFIX,
               MethodAttributes.Private | MethodAttributes.HideBySig,
               CallingConventions.HasThis );
            this.EmitPropertyGetterMethod( propertyModel, thisGenerationInfo, propertyField, propertyType, (CompositeMethodGenerationInfo) new CompositeMethodGenerationInfoImpl( getter, null, null ).WithReturnType( propertyType )/*.WithParameters( propertyInfo.GetIndexParameters().Select( p => Tuple.Create( p.ParameterType, p.Attributes ) ) )*/, readMethod );

            // Field getter for 32-bit processes, if required
            CILMethod getter32 = null;
            if ( read32Method != null )
            {
               getter32 = thisGenerationInfo.Builder.AddMethod(
                  PROPERTY_METHOD_PREFIX + propertyIdx + PROPERTY_GETTER32_POSTFIX,
                  MethodAttributes.Private | MethodAttributes.HideBySig,
                  CallingConventions.HasThis );
               this.EmitPropertyGetterMethod( propertyModel, thisGenerationInfo, propertyField, propertyType, (CompositeMethodGenerationInfo) new CompositeMethodGenerationInfoImpl( getter32, null, null ).WithReturnType( propertyType )/*.WithParameters( propertyInfo.GetIndexParameters().Select( p => Tuple.Create( p.ParameterType, p.Attributes ) ) )*/, read32Method );
            }
            // Field setter
            var setter = thisGenerationInfo.Builder.AddMethod(
               PROPERTY_METHOD_PREFIX + propertyIdx + PROPERTY_SETTER_POSTFIX,
               MethodAttributes.Public | MethodAttributes.HideBySig, // TODO MethodAttributes.Assembly when [InternalsVisibleTo(...)] attribute will be applied to all generated assemblies.
               CallingConventions.HasThis );
            this.EmitPropertySetterMethod( propertyModel, thisGenerationInfo, propertyField, fieldType, propertyType, (CompositeMethodGenerationInfo) new CompositeMethodGenerationInfoImpl( setter, null, null ).WithParameters( Enumerable.Repeat( Tuple.Create( propertyType, ParameterAttributes.None ), 1 ) ), writeMethod );

            // Exchange method
            var exchangeMethod = thisGenerationInfo.Builder.AddMethod(
               PROPERTY_METHOD_PREFIX + propertyIdx + PROPERTY_EXCHANGE_POSTFIX,
               MethodAttributes.Private | MethodAttributes.HideBySig,
               CallingConventions.HasThis );
            this.EmitPropertyExchangeMethod( propertyModel, thisGenerationInfo, propertyField, fieldType, propertyType, setter, (CompositeMethodGenerationInfo) new CompositeMethodGenerationInfoImpl( exchangeMethod, null, null ).WithReturnType( propertyType ).WithParameters( Enumerable.Repeat( Tuple.Create( propertyType, ParameterAttributes.None ), 1 ) ), writeMethod );

            // CompareExchange method
            var compareExchangeMethod = thisGenerationInfo.Builder.AddMethod(
               PROPERTY_METHOD_PREFIX + propertyIdx + PROPERTY_COMPARE_EXCHANGE_POSTFIX,
               MethodAttributes.Private | MethodAttributes.HideBySig,
               CallingConventions.HasThis );
            this.EmitPropertyCompareExchangeMethod( propertyModel, thisGenerationInfo, propertyField, (CompositeMethodGenerationInfo) new CompositeMethodGenerationInfoImpl( compareExchangeMethod, null, null ).WithReturnType( propertyType ).WithParameters( Enumerable.Repeat( Tuple.Create( propertyType, ParameterAttributes.None ), 2 ) ), compareExchangeMethodEmitter );

            thisGenerationInfo.AutoGeneratedPropertyInfos.Add( nPropertyInfo, new PropertyGenerationInfo(
               this.EmitRefMethodForPropertyOrEvent( propertyField, PROPERTY_METHOD_PREFIX + propertyIdx + REF_INVOKER_METHOD_SUFFIX ),
               propertyModel,
               propertyField,
               getter,
               getter32,
               setter,
               exchangeMethod,
               compareExchangeMethod,
               propertyType
               ) );
         }

         if ( this.NeedToEmitAdditionalMemberInfo( thisGenerationInfo, propertyInfo.Name, ( parent, name ) => parent.DeclaredProperties.FirstOrDefault( p => Object.Equals( p.Name, name ) ) ) )
         {
            // Need to define property if we inherit property directly from interface
            var name = propertyInfo.Name;
            if ( thisGenerationInfo.RawPropertyInfos.Keys.Any( pInfo => pInfo.Name.Equals( name ) ) )
            {
               // We already have property with the same name from different type
               name = QualifiedName.FromMemberInfo( nPropertyInfo ).ToString();
            }

            CILProperty pBuilder;
            if ( !thisGenerationInfo.RawPropertyInfos.TryGetValue( nPropertyInfo, out pBuilder ) )
            {
               pBuilder = thisGenerationInfo.Builder.AddProperty(
                  name,
                  propertyInfo.Attributes
                  );
               thisGenerationInfo.RawPropertyInfos.Add( nPropertyInfo, pBuilder );
            }

            if ( thisMethodGenerationInfo.MethodFromModel.Equals( propertyInfo.GetMethod ) )
            {
               pBuilder.GetMethod = thisMethodGenerationInfo.Builder;
            }
            else if ( thisMethodGenerationInfo.MethodFromModel.Equals( propertyInfo.SetMethod ) )
            {
               pBuilder.SetMethod = thisMethodGenerationInfo.Builder;
            }
            else
            {
               throw new InternalException( "Found a property, but neither setter nor getter matched the method being emitted. Property is " + propertyInfo + ", method is " + thisMethodGenerationInfo.MethodFromModel + "." );
            }
         }
      }

      protected Boolean IsCompositeGeneratedProperty(
         CompositeTypeGenerationInfo thisGenerationInfo,
         PropertyModel propertyModel,
         Type genericPropertyMixinType
         )
      {
         return !thisGenerationInfo.AutoGeneratedPropertyInfos.ContainsKey( propertyModel.NativeInfo ) &&
              propertyModel.GetterMethod != null &&
              propertyModel.SetterMethod != null &&
              genericPropertyMixinType.Equals( propertyModel.GetterMethod.Mixin.NativeInfo.DeclaringType ) &&
              genericPropertyMixinType.Equals( propertyModel.SetterMethod.Mixin.NativeInfo.DeclaringType );
      }

      protected void GetReadAndWriteMethods(
         CILTypeBase propertyType,
         out Action<CILField, MethodIL> read,
         out Action<CILField, MethodIL> read32,
         out Action<CILField, MethodIL> write,
         out Action<CILField, MethodIL> compareExchange,
         out CILTypeBase fieldType
         )
      {
         fieldType = propertyType;
         //         Boolean isBooleanProperty = BOOLEAN_TYPE.Equals( propertyType );
         // We must ask whether type is value type before asking whether it is enum, because type builder proxies throw if asked directly if it is enum.
         Action<CILTypeBase, CILTypeBase, MethodIL> fieldToPropertyCast = null;
         Action<CILTypeBase, CILTypeBase, MethodIL> propertyToFieldCast = null;
         CILMethod iExchangeMethod;
         CILMethod iCompareExchangeMethod;
         var tc = propertyType.GetTypeCode( CILTypeCode.Empty );
         switch ( tc )
         {
            case CILTypeCode.Object:
            case CILTypeCode.SystemObject:
            case CILTypeCode.Type:
            case CILTypeCode.String:
            case CILTypeCode.DateTime:
            case CILTypeCode.Decimal:
            case CILTypeCode.Empty:
            case CILTypeCode.IntPtr:
            case CILTypeCode.UIntPtr:
               var isObject = tc == CILTypeCode.SystemObject;
               var isIntPtr = tc == CILTypeCode.IntPtr;
               // TODO if generic parameter has same constraint as Interlocked.Exchange<T>, we can use the Interlocked.Exchange<T> directly.
               if ( propertyType.IsGenericParameter()
                  || ( propertyType.IsValueType() /*&& !propertyType.IsInterface() && !isObject*/
                     && ( this.isSilverLight || !isIntPtr )
                  ) )
               {
                  // TODO if value type => maybe use placeholder class? (Like System.Lazy<T> does)
                  fieldType = OBJECT_TYPE;
               }
               // SL doesn't have IntPtr or Object Interlocked.Exchange methods
               iExchangeMethod = ( this.isSilverLight || ( !isIntPtr && !isObject ) ) ? INTERLOCKED_EXCHANGE_METHOD_GDEF.MakeGenericMethod( fieldType ) :
                  ( isIntPtr ? INTERLOCKED_EXCHANGE_INT_PTR_METHOD : INTERLOCKED_EXCHANGE_OBJECT_METHOD );
               // SL doesn't have IntPtr Interlocked.CompareExchange method (but oddly enough has the Object variation)
               iCompareExchangeMethod = isObject ? INTERLOCKED_COMPARE_EXCHANGE_OBJECT_METHOD :
                  ( ( this.isSilverLight || !isIntPtr ) ? INTERLOCKED_COMPARE_EXCHANGE_METHOD_GDEF.MakeGenericMethod( fieldType ) : INTERLOCKED_COMPARE_EXCHANGE_INT_PTR_METHOD );
               fieldToPropertyCast = ( fType, pType, il ) => il.EmitCastToType( fType, pType );
               propertyToFieldCast = ( fType, pType, il ) => il.EmitCastToType( pType, fType );
               break;
            case CILTypeCode.Int64:
            case CILTypeCode.UInt64:
               fieldType = INT64_TYPE;
               iExchangeMethod = INTERLOCKED_EXCHANGE_I64_METHOD;
               iCompareExchangeMethod = INTERLOCKED_COMPARE_EXCHANGE_I64_METHOD;
               break;
            case CILTypeCode.Boolean:
               fieldType = INT32_TYPE;
               iExchangeMethod = INTERLOCKED_EXCHANGE_I32_METHOD;
               iCompareExchangeMethod = INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD;
               fieldToPropertyCast = ( fType, pType, il ) => il.EmitLoadInt32( 0 ).EmitCeq().EmitLoadInt32( 0 ).EmitCeq();
               propertyToFieldCast = ( fType, pType, il ) => { var isTrueLabelWrapper = il.DefineLabel(); var afterConversionLabelWrapper = il.DefineLabel(); il.EmitBranch( BranchType.IF_TRUE, isTrueLabelWrapper ); il.EmitLoadInt32( 0 ); il.EmitBranch( BranchType.ALWAYS, afterConversionLabelWrapper ); il.MarkLabel( isTrueLabelWrapper ); il.EmitLoadInt32( 1 ); il.MarkLabel( afterConversionLabelWrapper ); };
               break;
            case CILTypeCode.Byte:
            case CILTypeCode.Char:
            case CILTypeCode.Int16:
            case CILTypeCode.Int32:
            case CILTypeCode.SByte:
            case CILTypeCode.UInt16:
            case CILTypeCode.UInt32:
               fieldType = INT32_TYPE;
               iExchangeMethod = INTERLOCKED_EXCHANGE_I32_METHOD;
               iCompareExchangeMethod = INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD;
               break;
            case CILTypeCode.Double:
               iExchangeMethod = this.isSilverLight ? INTERLOCKED_EXCHANGE_I64_METHOD : INTERLOCKED_EXCHANGE_R64_METHOD;
               iCompareExchangeMethod = this.isSilverLight ? INTERLOCKED_COMPARE_EXCHANGE_I64_METHOD : INTERLOCKED_COMPARE_EXCHANGE_R64_METHOD;
               fieldType = this.isSilverLight ? INT64_TYPE : DOUBLE_TYPE;
               if ( this.isSilverLight )
               {
                  fieldToPropertyCast = ( fType, pType, il ) => il.EmitCall( INT64_BITS_TO_DOUBLE );
                  propertyToFieldCast = ( fType, pType, il ) => il.EmitCall( DOUBLE_BITS_TO_INT64 );
               }
               break;
            case CILTypeCode.Single:
               iExchangeMethod = this.isSilverLight ? INTERLOCKED_EXCHANGE_I32_METHOD : INTERLOCKED_EXCHANGE_R32_METHOD;
               iCompareExchangeMethod = this.isSilverLight ? INTERLOCKED_COMPARE_EXCHANGE_I32_METHOD : INTERLOCKED_COMPARE_EXCHANGE_R32_METHOD;
               fieldType = this.isSilverLight ? INT32_TYPE : SINGLE_TYPE;
               if ( this.isSilverLight )
               {
                  // TODO this is slow
                  // Field to property -> BitConverter.ToInt32(BitConverter.GetBytes(<field>), 0);
                  // Property to field -> BitConverter.ToSingle(BitConverter.GetBytes(<property>), 0);
                  fieldToPropertyCast = ( fType, pType, il ) => il.EmitCall( GET_BYTES_INT32 ).EmitLoadInt32( 0 ).EmitCall( BYTES_TO_SINGLE );
                  propertyToFieldCast = ( fType, pType, il ) => il.EmitCall( GET_BYTES_SINGLE ).EmitLoadInt32( 0 ).EmitCall( BYTES_TO_INT32 );
               }
               break;
            default:
               throw new ArgumentException( "Invalid type code: " + tc );
         }
         if ( fieldToPropertyCast == null )
         {
            fieldToPropertyCast = ( fType, pType, il ) => il.EmitNumericConversion( fType, pType, false );
         }
         if ( propertyToFieldCast == null )
         {
            propertyToFieldCast = ( fType, pType, il ) => il.EmitNumericConversion( pType, fType, false );
         }


         var fieldTypeActual = fieldType;

         read = new Action<CILField, MethodIL>( ( field, il ) =>
         {
            // just load this field
            il.EmitLoadThisField( field );
            fieldToPropertyCast( fieldTypeActual, propertyType, il );
         } );
         read32 = !this.isSilverLight && CILTypeCode.Int64 == fieldType.GetTypeCode( CILTypeCode.Empty ) ? new Action<CILField, MethodIL>( ( field, il ) =>
         {
            // Interlocked.Read(ref this.<field>);
            il
               .EmitLoadThisFieldAddress( field )
               .EmitCall( INTERLOCKED_READ_I64_METHOD );
            fieldToPropertyCast( fieldTypeActual, propertyType, il );
         } ) : null;
         write = ( field, il ) =>
         {
            // Interlocked.Exchange(ref this.<field>, (<field type>)value);
            il
               .EmitLoadThisFieldAddress( field )
               .EmitLoadArg( 1 );
            propertyToFieldCast( fieldTypeActual, propertyType, il );
            il.EmitCall( iExchangeMethod );
         };
         compareExchange = ( field, il ) =>
         {
            // Interlocked.CompareExchange(ref this._property, <arg-2>, <arg-1>);
            il
               .EmitLoadThisFieldAddress( field )
               .EmitLoadArg( 2 );
            propertyToFieldCast( fieldTypeActual, propertyType, il );
            il.EmitLoadArg( 1 );
            propertyToFieldCast( fieldTypeActual, propertyType, il );
            il.EmitCall( iCompareExchangeMethod );
            fieldToPropertyCast( fieldTypeActual, propertyType, il );
         };
      }

      protected Boolean CanEmitDefaultValueForPropertyModel( PropertyModel propModel )
      {
         return propModel.IsPartOfCompositeState() && ( propModel.DefaultValueCreator != null || propModel.IsUseDefaults() );
      }

      protected void EmitLoadDefaultValueForPropertyModel(
         PropertyModel propModel,
         CompositeTypeGenerationInfo propertyDeclaringTypeGenInfo,
         MethodGenerationInfo methodGenInfo
         )
      {
         var il = methodGenInfo.IL;
         var pInfo = this.ctx.NewWrapper( propModel.NativeInfo );
         var propType = propertyDeclaringTypeGenInfo.AutoGeneratedPropertyInfos[propModel.NativeInfo].PropertyType;
         var declType = pInfo.DeclaringType;
         UseDefaultsAttribute udAttr;
         if ( propModel.DefaultValueCreator != null )
         {

            // Get creator from property model
            il.EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_C_INSTANCE ) )
              .EmitCall( MODEL_INFO_GETTER )
              .EmitCall( MODEL_GETTER )
              .EmitCall( C_METHODS_GETTER )
              .EmitLoadInt32( propModel.GetterMethod.MethodIndex )
              .EmitCall( COMPOSITE_METHODS_INDEXER )
              .EmitCall( PROPERTY_MODEL_GETTER )
              .EmitCall( DEFAULT_CREATOR_GETTER );
            if ( WillDefaultValueCreatorParameterBeNull( pInfo ) )
            {
               il.EmitLoadNull();
            }
            else
            {
               il.EmitReflectionObjectOf( propertyDeclaringTypeGenInfo.Parents[declType] )
                  .EmitLoadString( pInfo.Name )
                  .EmitCall( GET_PROPERTY_INFO_METHOD );
            }
            il
               .EmitLoadLocal( methodGenInfo.GetLocalOrThrow( LB_C_INSTANCE ) )
               .EmitCall( STRUCTURE_OWNER_GETTER_METHOD )
               .EmitCall( APPLICATION_GETTER_METHOD )
               .EmitCall( DEFAULT_CREATOR_INVOKER )
               .EmitCastToType( DEFAULT_CREATOR_INVOKER.GetReturnType(), TypeGenerationUtils.CreateTypeForEmitting( propType, propertyDeclaringTypeGenInfo.GenericArguments, null ) );
         }
         else if ( propModel.IsUseDefaults( out udAttr ) )
         {
            var propTypeGDef = propModel.NativeInfo.PropertyType.GetGenericDefinitionIfGenericType();
            System.Reflection.MethodBase defaultValueCtor;
            CILTypeBase nullableParameter;
            if ( propType.IsNullable( out nullableParameter ) )
            {
               il.EmitLoadDefault( nullableParameter, aType => methodGenInfo.GetOrCreateLocalBasedOnType( aType ) )
                 .EmitNewObject( GetMethodBase( propType, NULLABLE_CTOR, propertyDeclaringTypeGenInfo ) );
            }
            else if ( STRING_TYPE.Equals( propType ) )
            {
               il.EmitLoadString( DEFAULT_STRING );
            }
            else if ( DEFAULT_CREATORS.TryGetValue( propTypeGDef, out defaultValueCtor ) )
            {
               if ( udAttr.ActualType != null )
               {
                  var otherCtor = udAttr.ActualType
                     .GetConstructors( System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public )
                     .FirstOrDefault( ctor => ctor.GetParameters().Length == 0 );
                  if ( otherCtor != null )
                  {
                     defaultValueCtor = otherCtor;
                  }
               }
               if ( defaultValueCtor is System.Reflection.ConstructorInfo )
               {
                  il.EmitNewObject( GetMethodBase( propType, this.ctx.NewWrapper( (System.Reflection.ConstructorInfo) defaultValueCtor ), propertyDeclaringTypeGenInfo ) );
               }
               else if ( defaultValueCtor is System.Reflection.MethodInfo )
               {
                  if ( ( (System.Reflection.MethodInfo) defaultValueCtor ).IsStatic )
                  {
                     il.EmitCall( GetMethodBase( propType, this.ctx.NewWrapper( (System.Reflection.MethodInfo) defaultValueCtor ), propertyDeclaringTypeGenInfo ) );
                  }
                  else
                  {
                     throw new InternalException( "The default creators contained non-static method for type " + propTypeGDef + "." );
                  }
               }
               else
               {
                  throw new InternalException( "Unknown default value creator " + defaultValueCtor + "." );
               }
            }
            else if ( propType.IsArray() || IENUMERABLE_GDEF_TYPE.Equals( TypeGenerationUtils.GenericDefinitionIfGArgsHaveGenericParams( propType as CILType ) ) )// || IENUMERABLE_NO_GDEF_TYPE.Equals( propType ) )
            {
               var elementType = propType.IsArray() ? ( (CILType) propType ).ElementType : /*( IENUMERABLE_NO_GDEF_TYPE.Equals( propType ) ? OBJECT_TYPE :*/ ( (CILType) propType ).GenericArguments[0]/* )*/;
               il.EmitLoadInt32( 0 )
                 .EmitNewArray( TypeGenerationUtils.CreateTypeForEmitting( elementType, propertyDeclaringTypeGenInfo.GenericArguments, null ) );
            }
            else if ( !propType.IsValueType() )
            {
               var ctorType = udAttr.ActualType == null ? (CILType) propType : this.ctx.NewWrapperAsType( udAttr.ActualType );
               var eDefaultValueCtor = ctorType.Constructors.FirstOrDefault( ctor => !ctor.Parameters.Any() );
               if ( eDefaultValueCtor != null )
               {
                  il.EmitNewObject( GetMethodBase( ctorType, eDefaultValueCtor, propertyDeclaringTypeGenInfo ) );
               }
               else
               {
                  throw new NotSupportedException( "Tried to use " + USE_DEFAULTS_ATTRIBUTE_TYPE + " on a type (" + ctorType + ") with no parameterless constructor." );
               }
            }
            else
            {
               throw new InternalException( "Could not emit default value for " + pInfo.GetPropertyType() + "." );
            }
         }
      }

      private static TMethod GetMethodBase<TMethod>( CILTypeBase propType, TMethod method, TypeGenerationInfo thisGenInfo )
         where TMethod : CILMethodBase
      {
         var declType = method.DeclaringType;
         return TypeGenerationUtils.GetMethodForEmitting( t => TypeGenerationUtils.CreateTypeForEmittingCILType( TypeGenerationUtils.GenericDefinitionIfGArgsHaveGenericParams( declType ).MakeGenericType( propType.GetGenericArgumentsArray() ), thisGenInfo.GenericArguments, null ), method );
         //var declType = method.DeclaringType;
         //return propType.ContainsGenericParameters() ?
         //   TypeGenerationUtils.GetMethodForEmitting( type => TypeGenerationUtils.CreateTypeForEmitting( declType.IsGenericType ?
         //      declType.GetGenericTypeDefinition().MakeGenericType( propType.GetGenericArguments() ) :
         //      declType, thisGenInfo.GenericArguments, null ), method ) :
         //   (TMethod) MethodBase.GetMethodFromHandle( method.MethodHandle, ( declType.IsGenericType ?
         //      declType.GetGenericTypeDefinition().MakeGenericType( propType.GetGenericArguments() ) :
         //      declType ).TypeHandle );
      }

      public static Boolean WillDefaultValueCreatorParameterBeNull( CILProperty pInfo )
      {
         return !pInfo.DeclaringType.ContainsGenericParameters();
      }
   }
}
#endif
