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
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// This is common interface for all QiCS application architectures.
   /// </summary>
   /// <typeparam name="TApplicationModel">The type of the application model that can be created from this <see cref="ApplicationArchitecture{T}"/>.</typeparam>
   /// <seealso cref="SingletonArchitecture"/>
   /// <seealso cref="LayeredArchitecture"/>
   public interface ApplicationArchitecture<out TApplicationModel> : UsesProvider<ApplicationArchitecture<TApplicationModel>>
      where TApplicationModel : ApplicationModel<ApplicationSPI>
   {
      /// <summary>
      /// Creates a new model from the current state of the architecture.
      /// The resulting model should be structurally immutable.
      /// </summary>
      /// <returns>A new Qi4CS model instance.</returns>
      /// <seealso cref="ApplicationModel{T}"/>
      TApplicationModel CreateModel();

      /// <summary>
      /// Gets the <see cref="CollectionsWithRoles.API.CollectionsFactory"/> of this <see cref="ApplicationArchitecture{T}"/>.
      /// This property may be used to easily get instance of <see cref="CollectionsWithRoles.API.CollectionsFactory"/> without depending on <see cref="CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY"/> field.
      /// </summary>
      /// <value>the <see cref="CollectionsWithRoles.API.CollectionsFactory"/> of this <see cref="ApplicationArchitecture{T}"/>.</value>
      CollectionsFactory CollectionsFactory { get; }

      /// <summary>
      /// Gets the additional <see cref="InjectionFunctionality"/> instances for this <see cref="ApplicationArchitecture{T}"/>.
      /// They will be registered to the <see cref="ApplicationModel{T}.InjectionService"/> during model creation in <see cref="CreateModel"/> method.
      /// </summary>
      /// <value>The additional <see cref="InjectionFunctionality"/> instances for this <see cref="ApplicationArchitecture{T}"/>.</value>
      /// <remarks>
      /// The key of the dictionary is the type of injection scope attribute, the value is <see cref="InjectionFunctionality"/> providing the functionality for that attribute.
      /// </remarks>
      DictionaryProxy<Type, InjectionFunctionality> AdditionalInjectionFunctionalities { get; }

      /// <summary>
      /// Gets all the assemblers that are currently within this <see cref="ApplicationArchitecture{T}"/>.
      /// </summary>
      /// <value>All the assemblers that are currently within this <see cref="ApplicationArchitecture{T}"/>.</value>
      IEnumerable<Assembler> AllAssemblers { get; }

      /// <summary>
      /// This event is invoked during creation of new <see cref="ApplicationModel{T}"/> in <see cref="CreateModel"/> method.
      /// It is invoked every time an attribute is processed, allowing bootstrap code to managed Qi4CS-agnostic DLLs.
      /// </summary>
      /// <seealso cref="AttributeProcessingArgs"/>
      event EventHandler<AttributeProcessingArgs> AttributeProcessingEvent;

      /// <summary>
      /// This event is invoked every time a new <see cref="ApplicationModel{T}"/> is successfully created in <see cref="CreateModel"/> method.
      /// </summary>
      /// <seealso cref="ApplicationModelCreatedArgs"/>
      event EventHandler<ApplicationModelCreatedArgs> ApplicationModelCreatedEvent;

      /// <summary>
      /// This event is invoked every time a new <see cref="CompositeModel"/> is successfully created in <see cref="CreateModel"/> method.
      /// </summary>
      /// <seealso cref="CompositeModelCreatedArgs"/>
      event EventHandler<CompositeModelCreatedArgs> CompositeModelCreatedEvent;
   }

   /// <summary>
   /// This is event argument class for <see cref="ApplicationArchitecture{T}.AttributeProcessingEvent"/> event.
   /// </summary>
   public class AttributeProcessingArgs : EventArgs
   {
      private readonly Attribute _oldAttribute;
      private readonly Int32 _compositeID;
      private readonly Object _reflectionElement;
      private Attribute _newAttribute;

      /// <summary>
      /// Creates a new instance of <see cref="AttributeProcessingArgs"/>.
      /// </summary>
      /// <param name="compositeID">The value of <see cref="CompositeModel.CompositeModelID"/>.</param>
      /// <param name="reflectionElement">The reflection element.</param>
      /// <param name="attribute">The attribute.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="reflectionElement"/> or <paramref name="attribute"/> is <c>null</c>.</exception>
      public AttributeProcessingArgs( Int32 compositeID, Object reflectionElement, Attribute attribute )
      {
         ArgumentValidator.ValidateNotNull( "Attribute", attribute );
         ArgumentValidator.ValidateNotNull( "Reflection element", reflectionElement );

         this._compositeID = compositeID;
         this._reflectionElement = reflectionElement;
         this._oldAttribute = attribute;
         this._newAttribute = attribute;
      }

      /// <summary>
      /// Gets the ID of the composite, which will be the value of <see cref="CompositeModel.CompositeModelID"/> of the composite model.
      /// </summary>
      /// <value>the ID of the composite.</value>
      /// <seealso cref="Assembling.AbstractCompositeAssemblyDeclaration.AffectedCompositeIDs"/>
      public Int32 CompositeID
      {
         get
         {
            return this._compositeID;
         }
      }

      /// <summary>
      /// Gets the reflection element this related attribute was obtained from.
      /// </summary>
      /// <value>The reflection element this related attribute was obtained from.</value>
      /// <remarks>
      /// This will always be either <see cref="Type"/>, <see cref="System.Reflection.MethodInfo"/>, <see cref="System.Reflection.ConstructorInfo"/>, <see cref="System.Reflection.ParameterInfo"/>, <see cref="System.Reflection.PropertyInfo"/> or <see cref="System.Reflection.EventInfo"/>.
      /// Since they all lack a common parent type, this property is typed <see cref="Object"/>.
      /// </remarks>
      public Object ReflectionElement
      {
         get
         {
            return this._reflectionElement;
         }
      }

      /// <summary>
      /// Gets the attribute applied to <see cref="ReflectionElement"/>.
      /// </summary>
      /// <value>The attribute applied to <see cref="ReflectionElement"/>.</value>
      public Attribute OldAttribute
      {
         get
         {
            return this._oldAttribute;
         }
      }

      /// <summary>
      /// Gets or sets the attribute which should be used instead of <see cref="OldAttribute"/> in bootstrap process.
      /// </summary>
      /// <value>The attribute which should be used instead of <see cref="OldAttribute"/> in bootstrap process.</value>
      public Attribute NewAttribute
      {
         get
         {
            return this._newAttribute;
         }
         set
         {
            this._newAttribute = value;
         }
      }
   }

   /// <summary>
   /// This is event argument class for <see cref="ApplicationArchitecture{T}.ApplicationModelCreatedEvent"/> event.
   /// </summary>
   public class ApplicationModelCreatedArgs : EventArgs
   {
      private readonly ApplicationArchitecture<ApplicationModel<ApplicationSPI>> _architecture;
      private readonly ApplicationModel<ApplicationSPI> _model;

      /// <summary>
      /// Creates a new instance of <see cref="ApplicationModelCreatedArgs"/>.
      /// </summary>
      /// <param name="architecture">The <see cref="ApplicationArchitecture{T}"/> which is creating the model.</param>
      /// <param name="model">The newly created <see cref="ApplicationModel{T}"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="architecture"/> or <paramref name="model"/> is <c>null</c>.</exception>
      public ApplicationModelCreatedArgs( ApplicationArchitecture<ApplicationModel<ApplicationSPI>> architecture, ApplicationModel<ApplicationSPI> model )
      {
         ArgumentValidator.ValidateNotNull( "Architecture", architecture );
         ArgumentValidator.ValidateNotNull( "Model", model );

         this._architecture = architecture;
         this._model = model;
      }

      /// <summary>
      /// Gets the <see cref="ApplicationArchitecture{T}"/> that is creating the <see cref="Model"/>.
      /// </summary>
      /// <value>The <see cref="ApplicationArchitecture{T}"/> that is creating the <see cref="Model"/>.</value>
      public ApplicationArchitecture<ApplicationModel<ApplicationSPI>> Architecture
      {
         get
         {
            return this._architecture;
         }
      }

      /// <summary>
      /// Gets the newly creatd <see cref="ApplicationModel{T}"/>.
      /// </summary>
      /// <value>The newly creatd <see cref="ApplicationModel{T}"/>.</value>
      public ApplicationModel<ApplicationSPI> Model
      {
         get
         {
            return this._model;
         }
      }
   }

   /// <summary>
   /// This is event argument class for <see cref="ApplicationArchitecture{T}.CompositeModelCreatedEvent"/> event.
   /// </summary>
   public class CompositeModelCreatedArgs : EventArgs
   {
      private readonly CompositeModel _compositeModel;

      /// <summary>
      /// Creates new instance of <see cref="CompositeModelCreatedArgs"/>.
      /// </summary>
      /// <param name="model">The newly created <see cref="CompositeModel"/>.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="model"/> is <c>null</c>.</exception>
      public CompositeModelCreatedArgs( CompositeModel model )
      {
         ArgumentValidator.ValidateNotNull( "Composite model", model );

         this._compositeModel = model;
      }

      /// <summary>
      /// Gets the newly created <see cref="CompositeModel"/>.
      /// </summary>
      /// <value>The newly created <see cref="CompositeModel"/>.</value>
      public CompositeModel Model
      {
         get
         {
            return this._compositeModel;
         }
      }
   }
}
