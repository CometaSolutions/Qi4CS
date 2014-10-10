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
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Runtime.Common;
using Qi4CS.Core.Runtime.Model;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Core.Runtime.Instance
{
   public interface CompositeModelTypeInstanceScopeSupport
   {
      CompositeModelTypeModelScopeSupport ModelScopeSupport { get; }

      CompositeInstance CreateInstance( CompositeInstanceStructureOwner structureOwner, CompositeModel model, IEnumerable<Type> publicCompositeTypes, UsesContainerQuery usesContainer );

      CompositeBuilder CreateBuilder( StructureServiceProviderSPI structureServiceProvider, Type[] compositeTypes );

      Boolean TryGetInstanceFromCompositeOrFragment( Object composite, out CompositeInstance instance );

      void ApplicationActivating( ApplicationSPI application );
      void ApplicationPassivating( ApplicationSPI application );
   }

   public abstract class AbstractModelTypeInstanceScopeSupport : CompositeModelTypeInstanceScopeSupport
   {
      private readonly CompositeModelTypeModelScopeSupport _modelScopeSupport;

      protected static readonly IEnumerable<EventHandler<ApplicationActivationArgs>> EMPTY_LISTENERS = Enumerable.Repeat<EventHandler<ApplicationActivationArgs>>( null, 0 );

      public AbstractModelTypeInstanceScopeSupport( CompositeModelTypeModelScopeSupport modelScopeSupport )
      {
         ArgumentValidator.ValidateNotNull( "Model-scope support", modelScopeSupport );

         this._modelScopeSupport = modelScopeSupport;
      }

      #region CompositeModelTypeInstanceScopeSupport Members

      public CompositeModelTypeModelScopeSupport ModelScopeSupport
      {
         get
         {
            return this._modelScopeSupport;
         }
      }

      public virtual CompositeInstance CreateInstance( CompositeInstanceStructureOwner structureOwner, CompositeModel model, IEnumerable<Type> publicCompositeTypes, UsesContainerQuery usesContainer )
      {
         return new CompositeInstanceImpl( structureOwner, model, publicCompositeTypes, usesContainer );
      }

      public virtual CompositeBuilder CreateBuilder( StructureServiceProviderSPI structureServiceProvider, Type[] compositeTypes )
      {
         ThrowIfGenericParams( compositeTypes );
         CompositeModel model = structureServiceProvider.Structure.ModelInfoContainer.GetCompositeModelInfo( this.ModelScopeSupport.AssemblyScopeSupport.ModelType, compositeTypes ).Model;
         UsesContainerMutable uses = UsesContainerMutableImpl.CreateWithParent( model.UsesContainer );
         return new CompositeBuilderImpl( compositeTypes, uses, (CompositeInstanceImpl) this.CreateInstance( structureServiceProvider.Structure, model, compositeTypes, uses.Query ) );
      }

      public virtual Boolean TryGetInstanceFromCompositeOrFragment( Object composite, out CompositeInstance instance )
      {
         var cType = composite.GetType();
         var codeGenInfo = this._modelScopeSupport.CodeGenerationInfo;
         var result = cType.GetAssembly().GetCustomAttributes( ).OfType<Qi4CSGeneratedAssemblyAttribute>().Any();
         if ( result )
         {
            var field = composite.GetType()
#if WINDOWS_PHONE_APP
.GetTypeInfo().GetDeclaredField(codeGenInfo.CompositeInstanceFieldName)
#else
               .GetField( codeGenInfo.CompositeInstanceFieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly )
#endif
;
            result = field != null;
            if (result && codeGenInfo.CompositeInstanceFieldType.IsAssignableFrom(field.FieldType))
            {
               instance = (CompositeInstance) field.GetValue( composite );
            }
            else
            {
               instance = null;
            }
         }
         else
         {
            instance = null;
         }
         return result;
      }

      public virtual void ApplicationActivating( ApplicationSPI application )
      {
         // Nothing to do
      }

      public virtual void ApplicationPassivating( ApplicationSPI application )
      {
         // Nothing to do
      }

      #endregion


      /// <summary>
      /// Throws a new instance of <see cref="InvalidCompositeTypeException"/> if <paramref name="compositeType"/> contains generic parameters (its <see cref="Type.ContainsGenericParameters"/> property is <c>true</c>).
      /// </summary>
      /// <param name="compositeType">The type to check.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="compositeType"/> is <c>null</c>.</exception>
      /// <exception cref="InvalidCompositeTypeException">If <see cref="Type.ContainsGenericParameters"/> returns <c>true</c> for <paramref name="compositeType"/>.</exception>
      public static void ThrowIfGenericParams( Type compositeType )
      {
         ArgumentValidator.ValidateNotNull( "Composite type", compositeType );
         if ( compositeType.ContainsGenericParameters() )
         {
            throw new InvalidCompositeTypeException( compositeType.Singleton(), "Not all generic parameters are closed." );
         }
      }

      /// <summary>
      /// Throws a new instance of <see cref="InvalidCompositeTypeException"/> if any of the types in <paramref name="compositeTypes"/> contains generic parameters (its <see cref="Type.ContainsGenericParameters"/> property is <c>true</c>).
      /// </summary>
      /// <param name="compositeTypes">The types to check.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="compositeTypes"/> is <c>null</c> or any of the types in <paramref name="compositeTypes"/> is <c>null</c>.</exception>
      /// <exception cref="InvalidCompositeTypeException">If If <see cref="Type.ContainsGenericParameters"/> returns <c>true</c> for any of the types in <paramref name="compositeTypes"/>.</exception>
      public static void ThrowIfGenericParams( IEnumerable<Type> compositeTypes )
      {
         ArgumentValidator.ValidateNotNull( "Composite types", compositeTypes );

         var violatingTypes = compositeTypes.Where( type =>
         {
            ArgumentValidator.ValidateNotNull( "Composite type", type );
            return type.ContainsGenericParameters();
         } );

         if ( violatingTypes.Any() )
         {
            throw new InvalidCompositeTypeException( compositeTypes, "The generic parameters of types {" + String.Join( ", ", violatingTypes ) + "} are not closed." );
         }
      }
   }

   public abstract class AbstractPlainModelTypeInstanceScopeSupport : AbstractModelTypeInstanceScopeSupport
   {
      protected AbstractPlainModelTypeInstanceScopeSupport( AbstractPlainCompositeModelTypeModelScopeSupport modelScopeSupport )
         : base( modelScopeSupport )
      {

      }
   }

   public abstract class AbstractServiceModelTypeInstanceScopeSupport : AbstractModelTypeInstanceScopeSupport
   {
      protected AbstractServiceModelTypeInstanceScopeSupport( AbstractServiceModelTypeModelScopeSupport modelScopeSupport )
         : base( modelScopeSupport )
      {

      }

      public override CompositeInstance CreateInstance( CompositeInstanceStructureOwner structureOwner, CompositeModel model, IEnumerable<Type> publicCompositeTypes, UsesContainerQuery usesContainer )
      {
         return this
            .GetServiceContainerFor( structureOwner )
            .GetService( structureOwner, model, publicCompositeTypes, usesContainer, ( (ServiceCompositeModel) model ).ServiceID );
      }

      public override CompositeBuilder CreateBuilder( StructureServiceProviderSPI structureServiceProvider, Type[] compositeTypes )
      {
         CompositeModelType mType = this.ModelScopeSupport.AssemblyScopeSupport.ModelType;
         throw new InvalidCompositeModelTypeException( mType, "Composite builders can not be used to create composites of type " + mType + "." );
      }

      public override void ApplicationActivating( ApplicationSPI application )
      {
         foreach ( var structureOwner in this.GetAllStructureOwners( application, true ) )
         {
            foreach ( var model in this.GetAllModels( structureOwner, true ).OfType<ServiceCompositeModel>().Where( model => model.ActivateWithApplication ) )
            {
               ( (ServiceCompositeInstanceImpl) this.CreateInstance( structureOwner, model, model.PublicTypes, model.UsesContainer ) ).RunActivationActionIfNeeded();
            }

            foreach ( var existingService in this.ReOrderExistingServicesOfServiceContainer( this.GetServiceContainerFor( structureOwner ).ExistingServices ) )
            {
               ( (ServiceCompositeInstanceImpl) existingService ).RunActivationActionIfNeeded();
            }
         }
      }

      public override void ApplicationPassivating( ApplicationSPI application )
      {
         this.DoForServicesInPassivationOrder( application, instance => instance.DisableLazyActivation() );

         LinkedList<Exception> list = null;
         this.DoForServicesInPassivationOrder( application, instance =>
         {
            try
            {
               instance.RunPassivationActionIfNeeded();
            }
            catch ( Exception exc )
            {
               if ( list == null )
               {
                  list = new LinkedList<Exception>();
               }
               list.AddLast( exc );
            }
         } );
         if ( list != null && list.Any() )
         {
            throw new AggregateException( list );
         }
      }

      private void DoForServicesInPassivationOrder( ApplicationSPI application, Action<ServiceCompositeInstanceImpl> action )
      {
         foreach ( CompositeInstanceStructureOwner structureOwner in this.GetAllStructureOwners( application, false ) )
         {
            // Don't need to get services which are not created
            //this.GetAllModels( structureOwner, false ).OfType<ServiceCompositeModel>().Where(model => model.SpecialMethods.Any(sMethod => sMethod.AllAttributes.OfType<PassivateAttribute>().Any()))
            foreach ( var service in this.ReOrderExistingServicesOfServiceContainer( this.GetServiceContainerFor( structureOwner ).ExistingServices ) )
            {
               action( (ServiceCompositeInstanceImpl) service );
            }
         }
      }

      protected abstract IEnumerable<CompositeInstanceStructureOwner> GetAllStructureOwners( ApplicationSPI application, Boolean isActivation );
      protected abstract IEnumerable<CompositeModel> GetAllModels( CompositeInstanceStructureOwner structureOwner, Boolean isActivation );
      protected abstract ServiceContainer GetServiceContainerFor( CompositeInstanceStructureOwner structureOwner );

      protected virtual IEnumerable<ServiceCompositeInstance> ReOrderExistingServicesOfServiceContainer( IEnumerable<ServiceCompositeInstance> services )
      {
         return services;
      }
   }

   //   public abstract class AbstractValueModelTypeInstanceScopeSupport : AbstractModelTypeInstanceScopeSupport
   //   {
   //      protected AbstractValueModelTypeInstanceScopeSupport( AbstractValueModelTypeModelScopeSupport modelScopeSupport )
   //         : base( modelScopeSupport )
   //      {

   //      }
   //   }
}
