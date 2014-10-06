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
using System.Linq;
using System.Reflection;
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.SPI.Instance
{
   /// <summary>
   /// This interface provides information about composite instance of a certain model.
   /// </summary>
   public interface CompositeInstance
   {
      /// <summary>
      /// Gets the information about the model of this composite.
      /// </summary>
      /// <value>The information about the model of this composite.</value>
      /// <seealso cref="ModelInfo"/>
      /// <seealso cref="CompositeModel"/>
      CompositeModelInfo ModelInfo { get; }

      /// <summary>
      /// Gets the Qi4CS structural element this <see cref="CompositeInstance"/> belongs to.
      /// </summary>
      /// <value>The Qi4CS structural element this <see cref="CompositeInstance"/> belongs to.</value>
      /// <seealso cref="CompositeInstanceStructureOwner"/>
      CompositeInstanceStructureOwner StructureOwner { get; }

      /// <summary>
      /// Gets the <see cref="CompositeState"/> of this <see cref="CompositeInstance"/>.
      /// </summary>
      /// <value>The <see cref="CompositeState"/> of this <see cref="CompositeInstance"/>.</value>
      /// <seealso cref="CompositeState"/>
      CompositeState State { get; }

      /// <summary>
      /// Checks whether this <see cref="CompositeInstance"/> is still in prototype stage.
      /// </summary>
      /// <value><c>true</c> if this <see cref="CompositeInstance"/> is still in prototype stage; <c>false</c> otherwise.</value>
      /// <remarks>
      /// Composites are initially in prototype stage, meaning that constraints are not checked when methods are invoked and immutable properties may be set.
      /// After prototype stage is disabled (by <see cref="API.Instance.CompositeBuilder.InstantiateWithType(Type)"/> method), all constraints will be checked and immutable properties no longer may be set.
      /// </remarks>
      Boolean IsPrototype { get; }

      /// <summary>
      /// Gets the container for objects injectable by <see cref="API.Model.UsesAttribute"/>.
      /// </summary>
      /// <value>The container for objects injectable by <see cref="API.Model.UsesAttribute"/>.</value>
      /// <seealso cref="UsesContainerQuery"/>
      UsesContainerQuery UsesContainer { get; }

      /// <summary>
      /// Gets <see cref="InvocationInfo"/> for composite method invocation chain in this thread.
      /// Will be <c>null</c> if this thread is not within composite method invocation chain, or if current composite method invocation chain does not require <see cref="InvocationInfo"/>.
      /// </summary>
      /// <value><see cref="InvocationInfo"/> for composite method invocation chain in this thread. May be <c>null</c>.</value>
      InvocationInfo InvocationInfo { get; }

      /// <summary>
      /// Gets all the instances of composite objects of this <see cref="CompositeInstance"/>, with their type as key.
      /// The parent types of composite objects are also used.
      /// </summary>
      /// <value>All the instances of composite objects of this <see cref="CompositeInstance"/>, with their type as key.</value>
      DictionaryQuery<Type, Object> Composites { get; }

      /// <summary>
      /// Gets the mapping from a native <see cref="MethodInfo"/> to corresponding <see cref="CompositeMethodModel"/>.
      /// </summary>
      /// <value>The mapping from a native <see cref="MethodInfo"/> to corresponding <see cref="CompositeMethodModel"/>.</value>
      DictionaryQuery<MethodInfo, CompositeMethodModel> MethodToModelMapping { get; }
   }

   /// <summary>
   /// This interface contains information about composite method invocation chain.
   /// </summary>
   public interface InvocationInfo
   {
      /// <summary>
      /// Gets the native <see cref="MethodInfo"/> of the composite method being invoked.
      /// </summary>
      /// <value>The native <see cref="MethodInfo"/> of the composite method being invoked.</value>
      MethodInfo CompositeMethod { get; }

      /// <summary>
      /// Gets the current <see cref="AbstractFragmentMethodModel"/> being invoked.
      /// May be <c>null</c> if no fragment method is being invoked.
      /// </summary>
      /// <value>The current <see cref="AbstractFragmentMethodModel"/> being invoked or <c>null</c>.</value>
      AbstractFragmentMethodModel FragmentMethodModel { get; }
   }
}

public static partial class E_Qi4CS
{
   /// <summary>
   /// Gets reference to composite of given type, when type is known at compile time.
   /// </summary>
   /// <typeparam name="TComposite">The type of the composite referece.</typeparam>
   /// <param name="instance">The <see cref="CompositeInstance"/>.</param>
   /// <returns>The reference to composite of type <typeparamref name="TComposite"/>, when type is known at compile time.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="instance"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If composite of given type is not found.</exception>
   public static TComposite GetCompositeForType<TComposite>( this CompositeInstance instance )
   {
      return (TComposite) GetCompositeForType( instance, typeof( TComposite ) );
   }

   /// <summary>
   /// Gets reference to composite of given type.
   /// </summary>
   /// <param name="instance">The <see cref="CompositeInstance"/>.</param>
   /// <param name="compositeType">The type of the composite. If it is <see cref="Object"/>, then composite reference of one of the public types is returned.</param>
   /// <returns>The reference to composite of type <paramref name="compositeType"/>.</returns>
   /// <exception cref="NullReferenceException">If <paramref name="instance"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentNullException">If <paramref name="compositeType"/> is <c>null</c>.</exception>
   /// <exception cref="ArgumentException">If composite of given type is not found.</exception>
   public static Object GetCompositeForType( this CompositeInstance instance, Type compositeType )
   {
      ArgumentValidator.ValidateNotNull( "Composite type", compositeType );
      Object result;
      if ( Object.Equals( typeof( Object ), compositeType ) )
      {
         result = instance.Composites.Values.FirstOrDefault( composite => instance.ModelInfo.Model.PublicTypes.Any( pType => pType.GetGenericDefinitionIfContainsGenericParameters().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( composite.GetType() ) ) );
      }
      else
      {
         instance.Composites.TryGetValue( compositeType, out result );
      }
      if ( result == null )
      {
         throw new ArgumentException( "Did not found composites of type " + compositeType + "." );
      }
      return result;
   }
}