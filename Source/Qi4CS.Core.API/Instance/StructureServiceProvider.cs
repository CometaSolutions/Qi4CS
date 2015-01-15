/*
 * Copyright (c) 2007-2009, Rickard Öberg.
 * Copyright (c) 2007-2009, Niclas Hedhman.
 * Various files.
 * See NOTICE file.
 * 
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
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This interface provides way to create composites and find services in Qi4CS application.
   /// Instances of this interface is available through <see cref="API.Model.StateAttribute"/> injection.
   /// </summary>
   public interface StructureServiceProvider
   {
      /// <summary>
      /// Gets the Qi4CS <see cref="Application"/> of this <see cref="StructureServiceProvider"/>.
      /// </summary>
      /// <value>The Qi4CS <see cref="Application"/> of this <see cref="StructureServiceProvider"/>.</value>
      Application Application { get; }

      /// <summary>
      /// Creates a new <see cref="CompositeBuilder"/> for specified composite model type using specified composite types to find the correct composite.
      /// </summary>
      /// <param name="compositeModelType">The composite model type.</param>
      /// <param name="compositeTypes">The public types of the composite.</param>
      /// <returns><see cref="CompositeBuilder"/> of matching composite model type and composite types.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="compositeModelType"/> or <paramref name="compositeTypes"/> is <c>null</c>.</exception>
      /// <exception cref="InvalidCompositeModelTypeException">If <paramref name="compositeModelType"/> is not supported in the Qi4CS <see cref="API.Instance.Application"/> this <see cref="StructureServiceProvider"/> belongs to. This exception is also thrown if <paramref name="compositeModelType"/> is <see cref="CompositeModelType.SERVICE"/>, since Qi4CS runtime manages the lifecycle of service composites.</exception>
      /// <exception cref="InvalidCompositeTypeException">If <paramref name="compositeTypes"/> contains a generic type with open generic arguments.</exception>
      /// <exception cref="NoSuchCompositeTypeException">If no suitable composite is found with public types containing <paramref name="compositeTypes"/>.</exception>
      /// <exception cref="API.Instance.AmbiguousTypeException">If too many composites found with public types containing <paramref name="compositeTypes"/>.</exception>
      /// <seealso cref="CompositeModelType"/>
      CompositeBuilder NewCompositeBuilder( CompositeModelType compositeModelType, IEnumerable<Type> compositeTypes );

      /// <summary>
      /// Searches for all services with given public types.
      /// </summary>
      /// <param name="serviceTypes">The public types of the service composites to have.</param>
      /// <returns>All found services via <see cref="ServiceReference"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="serviceTypes"/> is <c>null</c>.</exception>
      IEnumerable<ServiceReference> FindServices( IEnumerable<Type> serviceTypes );
   }

   /// <summary>
   /// This is a helper wrapper class to hold type argument of <see cref="E_Qi4CS.NewCompositeBuilder{T}( StructureServiceProvider, CompositeModelType )"/> in the result of that method.
   /// This way one can instantiate composite without supplying the same type argument again for <see cref="E_Qi4CS.Instantiate{T}(CompositeBuilder)"/> method.
   /// </summary>
   /// <typeparam name="TComposite">The type of the composite.</typeparam>
   public sealed class CompositeBuilderInfo<TComposite> : UsesProvider<CompositeBuilderInfo<TComposite>>
   {
      private readonly CompositeBuilder _builder;

      /// <summary>
      /// Creates a new instance of <see cref="CompositeBuilderInfo{T}"/> wrapping existing <see cref="CompositeBuilder"/>.
      /// Qi4CS runtime will use this constructor so there is no need to user code to invoke this.
      /// </summary>
      /// <param name="builder">The <see cref="CompositeBuilder"/> to wrap.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is <c>null</c>.</exception>
      public CompositeBuilderInfo( CompositeBuilder builder )
      {
         ArgumentValidator.ValidateNotNull( "Builder", builder );
         this._builder = builder;
      }

      /// <summary>
      /// Gets the wrapped <see cref="CompositeBuilder"/>.
      /// </summary>
      /// <value>
      /// The wrapped <see cref="CompositeBuilder"/>.
      /// </value>
      public CompositeBuilder Builder
      {
         get
         {
            return this._builder;
         }
      }

      /// <summary>
      /// <inheritdoc cref="CompositeBuilder.InstantiateWithType(Type)" />
      /// </summary>
      /// <returns>The reference to composite typed as <typeparamref name="TComposite"/>.</returns>
      /// <exception cref="CompositeInstantiationException"><inheritdoc cref="CompositeBuilder.InstantiateWithType(Type)" /></exception>
      /// <remarks>
      /// <inheritdoc cref="CompositeBuilder.InstantiateWithType(Type)" />
      /// </remarks>
      public TComposite Instantiate()
      {
         return this._builder.Instantiate<TComposite>();
      }

      /// <summary>
      /// <inheritdoc cref="CompositeBuilder.PrototypeFor(Type)" />
      /// </summary>
      /// <returns>The reference to composite prototype typed as <typeparamref name="TComposite"/>.</returns>
      /// <exception cref="InvalidOperationException"><inheritdoc cref="CompositeBuilder.PrototypeFor(Type)" /></exception>
      public TComposite Prototype()
      {
         return this._builder.Prototype<TComposite>();
      }

      /// <inheritdoc/>
      public CompositeBuilderInfo<TComposite> UseWithName( String name, Object value )
      {
         this._builder.UseWithName( name, value );
         return this;
      }
   }

   /// <summary>
   /// This is a helper wrapper class to hold type argument of <see cref="E_Qi4CS.FindService{T}( StructureServiceProvider )"/> and <see cref="E_Qi4CS.FindServices{T}( StructureServiceProvider )"/> methods in the result of those methods.
   /// This way one can find services of specific type and get the reference to actual service without supplying the same type argument again for <see cref="E_Qi4CS.GetService{T}(ServiceReference)"/> method.
   /// </summary>
   /// <typeparam name="TService">The type of the service.</typeparam>
   /// <seealso cref="Model.ServiceAttribute"/>
   public sealed class ServiceReferenceInfo<TService> : ServiceReference
   {
      private readonly ServiceReference _ref;

      /// <summary>
      /// Creates a new instance of <see cref="ServiceReferenceInfo{T}"/> wrapping original <see cref="ServiceReference"/>.
      /// Qi4CS runtime will use this constructor so there is no need to user code to invoke this.
      /// </summary>
      /// <param name="sRef">The <see cref="ServiceReference"/> to wrap.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="sRef"/> is <c>null</c>.</exception>
      public ServiceReferenceInfo( ServiceReference sRef )
      {
         ArgumentValidator.ValidateNotNull( "Service reference", sRef );
         this._ref = sRef;
      }

      /// <inheritdoc/>
      public Boolean Active
      {
         get
         {
            return this._ref.Active;
         }
      }

      /// <inheritdoc/>
      public String ServiceID
      {
         get
         {
            return this._ref.ServiceID;
         }
      }

      /// <inheritdoc/>
      public void Activate()
      {
         this._ref.Activate();
      }

      /// <inheritdoc/>
      public Object GetServiceWithType( Type serviceType )
      {
         return this._ref.GetServiceWithType( serviceType );
      }

      /// <summary>
      /// <inheritdoc cref="ServiceReference.GetServiceWithType(Type)" />
      /// </summary>
      /// <returns>Reference to service typed as <typeparamref name="TService"/>.</returns>
      /// <exception cref="ArgumentException"><inheritdoc cref="ServiceReference.GetServiceWithType(Type)" /></exception>
      public TService GetService()
      {
         return this._ref.GetService<TService>();
      }

      /// <summary>
      /// Gets the <see cref="ServiceReference"/> that this <see cref="ServiceReferenceInfo{T}"/> is wrapping.
      /// </summary>
      /// <value>the <see cref="ServiceReference"/> that this <see cref="ServiceReferenceInfo{T}"/> is wrapping.</value>
      public ServiceReference Reference
      {
         get
         {
            return this._ref;
         }
      }
   }
}

public static partial class E_Qi4CS
{

   /// <summary>
   /// Helper method to create new composite builder with specified <see cref="CompositeModelType"/> and only one type to be used as search criteria.
   /// </summary>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <param name="compositeModelType">The <see cref="CompositeModelType"/> to be used when searching for suitable composite model.</param>
   /// <param name="compositeType">The type of the composite, to be used as search criterion when searching for suitable composite model.</param>
   /// <returns>A new <see cref="CompositeBuilder"/> instance matching the search criteria.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/>
   public static CompositeBuilder NewCompositeBuilder( this StructureServiceProvider ssp, CompositeModelType compositeModelType, Type compositeType )
   {
      return ssp.NewCompositeBuilder( compositeModelType, compositeType.Singleton() );
   }

   /// <summary>
   /// Helper method to create new composite builder with specified <see cref="CompositeModelType"/> and only one type, known at compile time, to be used as search criteria.
   /// </summary>
   /// <typeparam name="TComposite">The type of the composite, to be used as search criterion when searching for suitable composite model.</typeparam>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/></param>
   /// <param name="compositeModelType">The <see cref="CompositeModelType"/> to be used when searching for suitable composite model.</param>
   /// <returns>A new <see cref="CompositeBuilderInfo{T}"/> instance matching the search criteria.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/>
   /// <seealso cref="CompositeBuilderInfo{T}"/>
   public static CompositeBuilderInfo<TComposite> NewCompositeBuilder<TComposite>( this StructureServiceProvider ssp, CompositeModelType compositeModelType )
   {
      return new CompositeBuilderInfo<TComposite>( ssp.NewCompositeBuilder( compositeModelType, typeof( TComposite ).Singleton() ) );
   }

   /// <summary>
   /// Helper method to create new <see cref="CompositeBuilder"/> for plain composites (composite model type of <see cref="CompositeModelType.PLAIN"/>) with only one type, known at compile time, to be used as search criterion.
   /// </summary>
   /// <typeparam name="TComposite">The type of the plain composite.</typeparam>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <returns>A new instance of <see cref="CompositeBuilderInfo{T}"/> for plain composites, matching the search criteria.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/>
   /// <seealso cref="CompositeBuilderInfo{T}"/>
   public static CompositeBuilderInfo<TComposite> NewPlainCompositeBuilder<TComposite>( this StructureServiceProvider ssp )
   {
      return ssp.NewCompositeBuilder<TComposite>( CompositeModelType.PLAIN );
   }

   /// <summary>
   /// Helper method to create new <see cref="CompositeBuilder"/> for plain composites (composite model type of <see cref="CompositeModelType.PLAIN"/>) with only one type to be used as search criterion.
   /// </summary>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <param name="compositeType">The type of the plain composite.</param>
   /// <returns>A new instance of <see cref="CompositeBuilder"/> for plain composites, matching the search criteria.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/>
   public static CompositeBuilder NewPlainCompositeBuilder( this StructureServiceProvider ssp, Type compositeType )
   {
      return ssp.NewCompositeBuilder( CompositeModelType.PLAIN, compositeType.Singleton() );
   }

   /// <summary>
   /// Helper method to create new <see cref="CompositeBuilder"/> for plain composites (compositem odel type of <see cref="CompositeModelType.PLAIN"/>) with given types to be used as search criterion.
   /// </summary>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <param name="compositeTypes">The types of the plain composite.</param>
   /// <returns>A new instance of <see cref="CompositeBuilder"/> for plain composites, matching the search criteria.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/>
   public static CompositeBuilder NewPlainCompositeBuilder( this StructureServiceProvider ssp, IEnumerable<Type> compositeTypes )
   {
      return ssp.NewCompositeBuilder( CompositeModelType.PLAIN, compositeTypes );
   }

   // TODO implementing value composite model type should be done via extension
   //public static CompositeBuilderInfo<TComposite> NewValueBuilder<TComposite>( this StructureServiceProvider ssp )
   //{
   //   return ssp.NewCompositeBuilder<TComposite>( CompositeModelType.VALUE );
   //}

   //public static CompositeBuilder NewValueBuilder( this StructureServiceProvider ssp, Type compositeType )
   //{
   //   return ssp.NewCompositeBuilder( CompositeModelType.VALUE, compositeType.Singleton() );
   //}

   //public static CompositeBuilder NewValueBuilder( this StructureServiceProvider ssp, IEnumerable<Type> compositeTypes )
   //{
   //   return ssp.NewCompositeBuilder( CompositeModelType.VALUE, compositeTypes );
   //}

   /// <summary>
   /// Helper method to find a first suitable <see cref="ServiceReferenceInfo{T}"/> using given type, known at compile time, as search criterion.
   /// </summary>
   /// <typeparam name="TService">The type of the service.</typeparam>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <returns>A first <see cref="ServiceReferenceInfo{T}"/> matching search criterion.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">If no suitable services found.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/>
   /// <seealso cref="ServiceReferenceInfo{T}"/>
   public static ServiceReferenceInfo<TService> FindService<TService>( this StructureServiceProvider ssp )
   {
      return ssp.FindServices<TService>().First();
   }

   /// <summary>
   /// Helper method to find a first suitable <see cref="ServiceReferenceInfo{T}"/> using given type as search criterion.
   /// </summary>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <param name="serviceType">The type of the service.</param>
   /// <returns>A first <see cref="ServiceReference"/> matching search criterion.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">If no suitable services found.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/>
   /// <seealso cref="ServiceReference"/>
   public static ServiceReference FindService( this StructureServiceProvider ssp, Type serviceType )
   {
      return ssp.FindServices( serviceType.Singleton() ).First();
   }

   /// <summary>
   /// Helper method to find first suitable <see cref="ServiceReference"/> using given types as search criterion.
   /// </summary>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <param name="serviceTypes">The types of the service.</param>
   /// <returns>A first <see cref="ServiceReference"/> matching search criterion.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <exception cref="InvalidOperationException">If no suitable services found.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/>
   /// <seealso cref="ServiceReference"/>
   public static ServiceReference FindService( this StructureServiceProvider ssp, IEnumerable<Type> serviceTypes )
   {
      return ssp.FindServices( serviceTypes ).First();
   }

   /// <summary>
   /// Helper method to find all suitable services using given type as search criterion.
   /// </summary>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <param name="serviceType">The type of the service.</param>
   /// <returns>All <see cref="ServiceReference"/>s matching search criterion.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/>
   /// <seealso cref="ServiceReference"/>
   public static IEnumerable<ServiceReference> FindServices( this StructureServiceProvider ssp, Type serviceType )
   {
      return ssp.FindServices( serviceType.Singleton() );
   }

   /// <summary>
   /// Helper method to find all suitable services using single type, known at compile time, as search criterion.
   /// </summary>
   /// <typeparam name="TService">The type of the service.</typeparam>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <returns>All <see cref="ServiceReferenceInfo{T}"/>s matching search criterion.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/> method for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.FindServices(IEnumerable{Type})"/>
   /// <seealso cref="ServiceReferenceInfo{T}"/>
   public static IEnumerable<ServiceReferenceInfo<TService>> FindServices<TService>( this StructureServiceProvider ssp )
   {
      return ssp.FindServices( typeof( TService ).Singleton() ).ToArray()
            .Select( sRef => new ServiceReferenceInfo<TService>( sRef ) );
   }

   /// <summary>
   /// Shortcut method to create a composite of specified <see cref="CompositeModelType"/> without further setting up of <see cref="CompositeBuilder"/>.
   /// </summary>
   /// <typeparam name="TComposite">The type of the composite.</typeparam>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <param name="compositeModelType">The <see cref="CompositeModelType"/> of the composite.</param>
   /// <returns>A new instance of composite with given <see cref="CompositeModelType"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/> and <see cref="CompositeBuilder.InstantiateWithType(Type)"/> methods for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/>
   /// <seealso cref="CompositeBuilder.InstantiateWithType(Type)"/>
   public static TComposite NewComposite<TComposite>( this StructureServiceProvider ssp, CompositeModelType compositeModelType )
   {
      return ssp.NewCompositeBuilder<TComposite>( compositeModelType ).Instantiate();
   }

   /// <summary>
   /// Shortcut method to create a plain composite (composite with <see cref="CompositeModelType.PLAIN"/> as composite model type) without further setting up of <see cref="CompositeBuilder"/>.
   /// </summary>
   /// <typeparam name="TComposite">The type of the composite.</typeparam>
   /// <param name="ssp">The <see cref="StructureServiceProvider"/>.</param>
   /// <returns>A new instance of plain composite.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="ssp"/> is <c>null</c>.</exception>
   /// <remarks>
   /// See <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/> and <see cref="CompositeBuilder.InstantiateWithType(Type)"/> methods for more exception scenarios.
   /// </remarks>
   /// <seealso cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, IEnumerable{Type})"/>
   /// <seealso cref="CompositeBuilder.InstantiateWithType(Type)"/>
   public static TComposite NewPlainComposite<TComposite>( this StructureServiceProvider ssp )
   {
      return NewComposite<TComposite>( ssp, CompositeModelType.PLAIN );
   }
}

