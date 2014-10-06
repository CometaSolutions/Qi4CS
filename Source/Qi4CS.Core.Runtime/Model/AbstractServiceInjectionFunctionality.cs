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
using System.Reflection;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public abstract class AbstractServiceInjectionFunctionality : InjectionFunctionality
   {
      private static ConstructorInfo SERVICE_REF_CTOR = typeof( ServiceReferenceInfo<> ).LoadConstructorOrThrow( 1 );
      private static MethodInfo ENUMERABLE_CAST_METHOD = typeof( Enumerable ).LoadMethodGDefinitionOrThrow( "Cast" );

      #region InjectionFunctionality Members

      public ValidationResult InjectionPossible( SPI.Model.AbstractInjectableModel model )
      {
         var models = this.FindSuitableModels( model.CompositeModel, model, model.InjectionScope, model.TargetType, GetActualServiceType( model.TargetType ).GetGenericDefinitionIfContainsGenericParameters() );

         ValidationResult retVal = null;
         foreach ( var tuple in GetQualifierTypes( model ) )
         {
            var qType = tuple.Item2.QualifierType;
            if ( qType == null )
            {
               retVal = new ValidationResult( false, "Service qualifier callback type was null." );
            }
            else if ( !typeof( ServiceQualifier ).IsAssignableFrom( qType ) )
            {
               retVal = new ValidationResult( false, "Qualifier type " + qType + " must implement " + typeof( ServiceQualifier ) + "." );
            }
            else if ( qType.ContainsGenericParameters )
            {
               retVal = new ValidationResult( false, "Qualifier type " + qType + " must not contain open generic parameters." );
            }
            else if ( !qType.GetConstructors( BindingFlags.Public | BindingFlags.Instance ).Any( ctor => ctor.GetParameters().Length == 0 ) )
            {
               retVal = new ValidationResult( false, "Qualifier type " + qType + "must contain parameterless constructor." );
            }

            if ( retVal != null )
            {
               break;
            }

            var qInstance = (ServiceQualifier) tuple.Item2.QualifierType.LoadConstructorOrThrow( 0 ).Invoke( null );
            models = models.Where( sModel => qInstance.Qualifies( sModel, tuple.Item1 ) );

         }

         return retVal ?? new ValidationResult( models.Any(), "No suitable services found." ); // retVal ?? this.IsInjectionPossible( model.CompositeModel, model, model.InjectionScope, model.TargetType, TypeUtils.TypeUtil.GenericDefinitionIfContainsGenericParams( GetActualServiceType( model.TargetType ) ) );
      }

      public Object ProvideInjection( SPI.Instance.CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         var scope = model.InjectionScope;
         // First check if we are trying to reference ourselves
         //var gDef = TypeUtils.TypeUtil.GenericDefinitionIfGenericType( targetType );
         Object result = null;
         var provider = instance.StructureOwner.StructureServices;
         var actualServiceType = GetActualServiceType( targetType );
         var refs = provider.FindServices( actualServiceType );
         foreach ( var tuple in GetQualifierTypes( model ) )
         {
            var qInstance = (ServiceQualifier) tuple.Item2.QualifierType.LoadConstructorOrThrow( 0 ).Invoke( null );
            refs = refs.Where( sRef => qInstance.Qualifies( sRef, tuple.Item1 ) );
         }

         if ( Object.Equals( actualServiceType, targetType ) )
         {
            result = refs.Select( sRef => sRef.GetServiceWithType( actualServiceType ) ).FirstOrDefault();
         }
         else
         {
            if ( targetType.IsGenericType )
            {
               var gArgs = targetType.GetGenericArguments();
               if ( Object.Equals( typeof( IEnumerable<> ), targetType.GetGenericTypeDefinition() ) )
               {
                  var enumerableType = gArgs[0];
                  if ( enumerableType.IsGenericType
                     && Object.Equals( typeof( ServiceReferenceInfo<> ), enumerableType.GetGenericTypeDefinition() )
                     )
                  {
                     result = GetTypedEnumerable( refs.Select( sRef => MakeRefToInfo( sRef, enumerableType.GetGenericArguments()[0] ) ), enumerableType );
                  }
                  else
                  {
                     result = GetTypedEnumerable( refs.Select( sRef => sRef.GetServiceWithType( actualServiceType ) ), enumerableType );
                  }
               }
               else
               {
                  result = MakeRefToInfo( refs.FirstOrDefault(), gArgs[0] );
               }
            }
         }
         return result;
      }

      public InjectionTime GetInjectionTime( SPI.Model.AbstractInjectableModel model )
      {
         return GetQualifierTypes( model ).Any( tuple => tuple.Item2.ChangesDuringRuntime ) ?
            InjectionTime.ON_METHOD_INVOKATION :
            InjectionTime.ON_CREATION;
      }

      #endregion

      protected abstract IEnumerable<ServiceCompositeModel> FindSuitableModels( SPI.Model.CompositeModel compositeModel, SPI.Model.AbstractInjectableModel model, Attribute scope, Type targetType, Type serviceType );

      public static Type GetActualServiceType( Type serviceType )
      {
         Type actualServiceType = serviceType;
         if ( actualServiceType.IsGenericType && Object.Equals( typeof( IEnumerable<> ), actualServiceType.GetGenericTypeDefinition() ) )
         {
            actualServiceType = actualServiceType.GetGenericArguments()[0];
         }
         if ( actualServiceType.IsGenericType && Object.Equals( typeof( ServiceReferenceInfo<> ), actualServiceType.GetGenericTypeDefinition() ) )
         {
            actualServiceType = actualServiceType.GetGenericArguments()[0];
         }

         return actualServiceType;
      }

      private static Object MakeRefToInfo( ServiceReference sRef, Type sType )
      {
         return ( (ConstructorInfo) MethodBase.GetMethodFromHandle( SERVICE_REF_CTOR.MethodHandle, SERVICE_REF_CTOR.DeclaringType.MakeGenericType( sType ).TypeHandle ) )
            .Invoke( new Object[] { sRef } );
      }

      private static IEnumerable<Tuple<Attribute, ServiceQualifierAttribute>> GetQualifierTypes( AbstractInjectableModel model )
      {
         return model.GetAttributesMarkedWith( typeof( ServiceQualifierAttribute ) )
            .Select( attr => Tuple.Create( attr, (ServiceQualifierAttribute) model.GetAttributesOfAttribute( attr.GetType() )[typeof( ServiceQualifierAttribute )][0] ) );
      }

      private static Object GetTypedEnumerable( IEnumerable<Object> enumerable, Type elementType )
      {
         return ENUMERABLE_CAST_METHOD
            .MakeGenericMethod( elementType )
            .Invoke( null, new Object[] { enumerable } );
      }
   }
}
