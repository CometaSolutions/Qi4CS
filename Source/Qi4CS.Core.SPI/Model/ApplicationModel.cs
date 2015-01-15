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
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This is model for the whole Qi4CS application.
   /// </summary>
   /// <typeparam name="TApplicationInstance">The type of Qi4CS applications that can be created from this model.</typeparam>
   public interface ApplicationModel<out TApplicationInstance> : ValidatableItem
      where TApplicationInstance : ApplicationSPI
   {
      /// <summary>
      /// Lazy initializes the validation result of this model and returns it.
      /// </summary>
      /// <value>Lazily initialized validation result of this model.</value>
      /// <seealso cref="ApplicationValidationResultIQ"/>
      ApplicationValidationResultIQ ValidationResult { get; }

      /// <summary>
      /// Creates a new instance of architecture-specific sub-type of <see cref="ApplicationSPI"/> based on this <see cref="ApplicationModel{T}"/>.
      /// </summary>
      /// <param name="applicationName">The name of the application. May be <c>null</c>. This value will be directly visible through <see cref="API.Instance.Application.Name"/> property.</param>
      /// <param name="mode">The mode of the application. May be <c>null</c>. Typical values may be <c>"development"</c>, <c>"production"</c>, <c>"staging"</c> or <c>"test"</c>. This value will be directly visible through <see cref="API.Instance.Application.Mode"/> property.</param>
      /// <param name="version">The textual version information of the application. May be <c>null</c>. This value will be directly visible through <see cref="API.Instance.Application.Version"/> property.</param>
      /// <returns>A new instance of architecture-specific sub-type of <see cref="ApplicationSPI"/> with given name, mode and version.</returns>
      /// <exception cref="InvalidApplicationModelException">If this application model is not valid, that is, result of <see cref="ValidationResult"/> property is <c>true</c> for <see cref="AbstractValidationResultIQ.HasAnyErrors()"/> method.</exception>
      /// <remarks>Calling this method will cause validation to occur, if it is not already done.</remarks>
      /// <seealso cref="ValidationResult"/>
      TApplicationInstance NewInstance( String applicationName, String mode, String version );

      /// <summary>
      /// Gets the <see cref="InjectionService"/> of this application model.
      /// </summary>
      /// <value>The <see cref="InjectionService"/> of this application model.</value>
      /// <seealso cref="InjectionService"/>
      InjectionService InjectionService { get; }

      /// <summary>
      /// Gets the type of mixin responsible for getting and setting properties belonging to composite's state.
      /// Typical value is <see cref="T:Qi4CS.Core.Runtime.Instance.GenericPropertyMixin"/>
      /// </summary>
      /// <value>The type for mixin responsible for getting and setting properties belonging to composite's state.</value>
      Type GenericPropertyMixinType { get; }

      /// <summary>
      /// Gets the type of mixin responsible for adding and removing events belonging to composite's state.
      /// Typical value is <see cref="T:Qi4CS.Core.Runtime.Instance.GenericEventMixin"/>.
      /// </summary>
      /// <value>The type of mixin responsible for adding and removing events belonging to composite's state.</value>
      Type GenericEventMixinType { get; }

      /// <summary>
      /// Gets the type that all generic fragments must implement.
      /// Typical value is <see cref="API.Instance.GenericInvocator"/>.
      /// </summary>
      /// <value>The type that all generic fragments must implement.</value>
      Type GenericFragmentBaseType { get; }

      /// <summary>
      /// Gets the instance of <see cref="CollectionsWithRoles.API.CollectionsFactory"/> associated with this application model.
      /// This property may be used to easily get instance of <see cref="CollectionsWithRoles.API.CollectionsFactory"/> without depending on <see cref="CollectionsWithRoles.Implementation.CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY"/> field.
      /// </summary>
      /// <value>The instance of <see cref="CollectionsWithRoles.API.CollectionsFactory"/> associated with this application model.</value>
      CollectionsFactory CollectionsFactory { get; }

      /// <summary>
      /// Gets all the composite models belonging to this application model.
      /// The key of the dictionary is <see cref="CompositeModel.CompositeModelID"/> and value is <see cref="CompositeModel"/>.
      /// </summary>
      /// <value>All the composite models belonging to this application model.</value>
      DictionaryQuery<Int32, CompositeModel> CompositeModels { get; }

      /// <summary>
      /// Gets all assemblies which will need to have their Qi4CS-generated version available.
      /// </summary>
      /// <value>All assemblies which will need to have their Qi4CS-generated version available.</value>
      SetQuery<System.Reflection.Assembly> AffectedAssemblies { get; }

      ///// <summary>
      ///// This event will be fired when application is validated for the first time.
      ///// This teven is thus called at most once during application model lifetime.
      ///// </summary>
      ///// <seealso cref="ApplicationValidationArgs"/>
      //event EventHandler<ApplicationValidationArgs> ApplicationValidationEvent;

      // <summary>
      // During the validation of the application, every time a single composite model is validated, this event will be fired.
      // Notice that application validation only happens when validation result cache is cleared.
      // See <see cref="Validate"/> for more information about when the validation result cache is cleared.
      // </summary>
      // <seealso cref="CompositeValidationArgs"/>
      //event EventHandler<CompositeValidationArgs> CompositeValidationEvent;

      /// <summary>
      /// This event is fired whenever a new application instance is created via <see cref="NewInstance(String, String, String)"/> method.
      /// </summary>
      /// <seealso cref="ApplicationCreationArgs"/>
      event EventHandler<ApplicationCreationArgs> ApplicationInstanceCreatedEvent;

      /// <summary>
      /// This event is fired whenever generated types are looked up for new application instance being created via <see cref="NewInstance(String, String, String)"/> method.
      /// </summary>
      /// <seealso cref="ApplicationCodeResolveArgs"/>
      event EventHandler<ApplicationCodeResolveArgs> ApplicationCodeResolveEvent;

      /// <summary>
      /// This event is fired just before Qi4CS model tries to load generated assembly.
      /// The main purpose is to give modification ability to the generated assembly name (e.g. adding version and key tokens).
      /// </summary>
      /// <seealso cref="AssemblyLoadingArgs"/>
      event EventHandler<AssemblyLoadingArgs> GeneratedAssemblyLoadingEvent;

#if QI4CS_SDK

      /// <summary>
      /// This event is fired whenever code is genereated via <see cref="GenerateCode(CILAssemblyManipulator.API.CILReflectionContext, Boolean)"/> method.
      /// By registering to this event, extensions may generate additional types to the generated assemblies.
      /// </summary>
      /// <seealso cref="ApplicationCodeGenerationArgs"/>
      event EventHandler<ApplicationCodeGenerationArgs> ApplicationCodeGenerationEvent;

      /// <summary>
      /// Generates all composite types required for successful application instance creation.
      /// </summary>
      /// <param name="reflectionContext">The <see cref="CILAssemblyManipulator.API.CILReflectionContext"/> to use when generating code.</param>
      /// <param name="isSilverlight">Whether the code being emitted should be run on Silverlight 5. See remarks for more information.</param>
      /// <returns>A dictionary containing mapping from already-existing native assemblies to generated assemblies.</returns>
      /// <remarks>
      /// <para>
      /// Calling this method will cause validation to occur, if it is not already done.
      /// </para>
      /// <para>
      /// When <paramref name="isSilverlight"/> is <c>true</c>, the following things change in generated code.
      /// <list type="bullet">
      /// <item><description>Since there is no <see cref="M:System.Threading.Interlocked.Read(System.Int64@)"/> method in SL5, there will be no atomic reads for 64-bit integers within 32-bit process in either of those.</description></item>
      /// <item><description>Quite a lot of <c>Exchange</c> and <c>CompareExchange</c> method overloads are missing from <see cref="System.Threading.Interlocked"/> class. The code generation will modify field types and make appropriate conversions as required.</description></item>
      /// </list>
      /// </para>
      /// </remarks>
      /// <seealso cref="ValidationResult"/>
      DictionaryQuery<System.Reflection.Assembly, CILAssemblyManipulator.API.CILAssembly> GenerateCode( CILAssemblyManipulator.API.CILReflectionContext reflectionContext, Boolean isSilverlight );

#endif
   }

   ///// <summary>
   ///// Base class for <see cref="ApplicationValidationArgs"/>.
   ///// In future, other classes may subclass this class.
   ///// </summary>
   ///// <typeparam name="TResult">The mutable type of the validation result.</typeparam>
   ///// <typeparam name="TResultIQ">The immutable type of the validation result.</typeparam>
   //public abstract class AbstractValidationArgs<TResult, TResultIQ> : EventArgs
   //   where TResult : class, AbstractValidationResult<TResult, TResultIQ>
   //   where TResultIQ : AbstractValidationResultIQ
   //{
   //   private readonly TResult _result;

   //   /// <summary>
   //   /// Creates a new instance of <see cref="AbstractValidationArgs{T,U}"/> with given validation reslt.
   //   /// </summary>
   //   /// <param name="result">The validation result.</param>
   //   /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <c>null</c>.</exception>
   //   protected AbstractValidationArgs( TResult result )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Result", result );

   //      this._result = result;
   //   }

   //   /// <summary>
   //   /// Gets the mutable type of the validation result associated with this event.
   //   /// </summary>
   //   /// <value>The mutable type of the validation result associated with this event.</value>
   //   public TResult CurrentResult
   //   {
   //      get
   //      {
   //         return this._result;
   //      }
   //   }
   //}

   ///// <summary>
   ///// The event args type used in <see cref="ApplicationModel{T}.ApplicationValidationEvent"/> event.
   ///// </summary>
   //public sealed class ApplicationValidationArgs : AbstractValidationArgs<ApplicationValidationResult, ApplicationValidationResultIQ>
   //{
   //   /// <summary>
   //   /// Creates new instance of <see cref="ApplicationValidationArgs"/>.
   //   /// </summary>
   //   /// <param name="result">The <see cref="ApplicationValidationResult"/> associated with this event.</param>
   //   /// <exception cref="ArgumentNullException">If <paramref name="result"/> is <c>null</c>.</exception>
   //   public ApplicationValidationArgs( ApplicationValidationResult result )
   //      : base( result )
   //   {
   //   }
   //}

   // <summary>
   // 
   // </summary>
   //public abstract class CompositeValidationArgs : AbstractValidationArgs<CompositeValidationResult, CompositeValidationResultIQ>
   //{
   //   private readonly CompositeModel _model;

   //   public CompositeValidationArgs(
   //      CompositeModel model,
   //      CompositeValidationResult result
   //      )
   //      : base( result )
   //   {
   //      ArgumentValidator.ValidateNotNull( "Instance model", model );

   //      this._model = model;
   //   }

   //   public CompositeModel CompositeModel
   //   {
   //      get
   //      {
   //         return this._model;
   //      }
   //   }
   //}

   /// <summary>
   /// The event args type used in <see cref="ApplicationModel{T}.ApplicationInstanceCreatedEvent"/> event.
   /// </summary>
   public sealed class ApplicationCreationArgs : EventArgs
   {
      private readonly ApplicationSPI _instance;

      /// <summary>
      /// Creates new instance of <see cref="ApplicationCreationArgs"/>.
      /// </summary>
      /// <param name="instance">The newly created application instance.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is <c>null</c>.</exception>
      public ApplicationCreationArgs( ApplicationSPI instance )
      {
         ArgumentValidator.ValidateNotNull( "Application instance", instance );
         this._instance = instance;
      }

      /// <summary>
      /// Gets the newly created application instance.
      /// </summary>
      /// <value>The newly created application instance.</value>
      public ApplicationSPI Instance
      {
         get
         {
            return this._instance;
         }
      }
   }

   /// <summary>
   /// The event args type used in <see cref="ApplicationModel{T}.ApplicationCodeResolveEvent"/> event.
   /// </summary>
   public sealed class ApplicationCodeResolveArgs : EventArgs
   {
      private readonly DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> _genResults;
      private readonly DictionaryQuery<System.Reflection.Assembly, System.Reflection.Assembly> _assemblies;

      /// <summary>
      /// Creates new instance of <see cref="ApplicationCodeResolveArgs"/>.
      /// </summary>
      /// <param name="genResults">Type generation results.</param>
      /// <param name="assemblies">A dictionary containing mapping from already-existing native assemblies and generated assemblies.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="genResults"/> or <paramref name="assemblies"/> is <c>null</c>.</exception>
      public ApplicationCodeResolveArgs(
         DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> genResults,
         DictionaryQuery<System.Reflection.Assembly, System.Reflection.Assembly> assemblies
         )
      {
         ArgumentValidator.ValidateNotNull( "Generation results", genResults );
         ArgumentValidator.ValidateNotNull( "Assemblies", assemblies );

         this._genResults = genResults;
         this._assemblies = assemblies;
      }

      /// <summary>
      /// Gets the mapping from <see cref="CompositeModel"/> to <see cref="PublicCompositeTypeGenerationResult"/>.
      /// </summary>
      /// <value>The mapping from <see cref="CompositeModel"/> to <see cref="PublicCompositeTypeGenerationResult"/>.</value>
      public DictionaryQuery<CompositeModel, PublicCompositeTypeGenerationResult> GenerationResults
      {
         get
         {
            return this._genResults;
         }
      }

      /// <summary>
      /// Gets the mapping from already-existing native assemblies and generated assemblies.
      /// </summary>
      /// <value>The mapping from already-existing native assemblies and generated assemblies.</value>
      public DictionaryQuery<System.Reflection.Assembly, System.Reflection.Assembly> Assemblies
      {
         get
         {
            return this._assemblies;
         }
      }
   }

   /// <summary>
   /// The event args type used in <see cref="ApplicationModel{T}.GeneratedAssemblyLoadingEvent"/>.
   /// </summary>
   public sealed class AssemblyLoadingArgs : EventArgs
   {
      private readonly String _originalFullAssemblyName;
      private readonly String _qi4CSAssemblyName;

      /// <summary>
      /// Creates new instance of <see cref="AssemblyLoadingArgs"/>.
      /// </summary>
      /// <param name="originalFullName">The full name of the original assembly.</param>
      /// <param name="qi4CSSimpleName">The simple name of the Qi4CS generated assembly.</param>
      public AssemblyLoadingArgs( String originalFullName, String qi4CSSimpleName )
      {
         ArgumentValidator.ValidateNotEmpty( "Original assembly name", originalFullName );
         ArgumentValidator.ValidateNotEmpty( "Qi4CS generated assembly name", qi4CSSimpleName );

         this._originalFullAssemblyName = originalFullName;
         this._qi4CSAssemblyName = qi4CSSimpleName;
      }

      /// <summary>
      /// Gets the full assembly name of the original assembly.
      /// </summary>
      /// <value>The full assembly name of the original assembly.</value>
      public String OriginalAssemblyName
      {
         get
         {
            return this._originalFullAssemblyName;
         }
      }

      /// <summary>
      /// Gets the simple name of the Qi4CS generated assembly.
      /// </summary>
      /// <value>The simple name of the Qi4CS generated assembly.</value>
      public String Qi4CSGeneratedAssemblyName
      {
         get
         {
            return this._qi4CSAssemblyName;
         }
      }

      /// <summary>
      /// Gets or sets the version of Qi4CS generated assembly.
      /// </summary>
      /// <value>The version of Qi4CS generated assembly.</value>
      public Version Version { get; set; }

      /// <summary>
      /// Gets or sets the culture of Qi4CS generated assembly.
      /// </summary>
      /// <value>The culture of Qi4CS generated assembly.</value>
      public String Culture { get; set; }

      /// <summary>
      /// Gets or sets the public key token of Qi4CS generated assembly.
      /// </summary>
      /// <value>The public key token of Qi4CS generated assembly.</value>
      /// <remarks>This value will be ignored if <see cref="PublicKey"/> is not <c>null</c> or empty.</remarks>
      public Byte[] PublicKeyToken { get; set; }

      /// <summary>
      /// Gets or sets the full public key of Qi4CS generated assembly.
      /// </summary>
      /// <value>The full public key of Qi4CS generated assembly.</value>
      public Byte[] PublicKey { get; set; }
   }

#if QI4CS_SDK

   /// <summary>
   /// The event args type used in <see cref="ApplicationModel{T}.ApplicationCodeGenerationEvent"/> event.
   /// </summary>
   public sealed class ApplicationCodeGenerationArgs : EventArgs
   {
      private readonly DictionaryQuery<CompositeModel, DictionaryQuery<System.Reflection.Assembly, CILAssemblyManipulator.API.CILType>> _gInfo;

      /// <summary>
      /// Creates new instance of <see cref="ApplicationCodeGenerationArgs"/>.
      /// </summary>
      /// <param name="generationInfo">Type generation information, see <see cref="TypeGenerationInformation"/> property for more information.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="generationInfo"/> is <c>null</c>.</exception>
      public ApplicationCodeGenerationArgs( DictionaryQuery<CompositeModel, DictionaryQuery<System.Reflection.Assembly, CILAssemblyManipulator.API.CILType>> generationInfo )
      {
         ArgumentValidator.ValidateNotNull( "Reflection builders", generationInfo );

         this._gInfo = generationInfo;
      }

      /// <summary>
      /// Gets the information about generated types.
      /// The keys are instances of the <see cref="CompositeModel"/>.
      /// The values are instances of dictionaries with already-existing assemblies of all types related to a single <see cref="CompositeModel"/> and values are instances of generated <see cref="CILAssemblyManipulator.API.CILType"/> types.
      /// </summary>
      /// <value>The information about generated types.</value>
      public DictionaryQuery<CompositeModel, DictionaryQuery<System.Reflection.Assembly, CILAssemblyManipulator.API.CILType>> TypeGenerationInformation
      {
         get
         {
            return this._gInfo;
         }
      }
   }

#endif
}
