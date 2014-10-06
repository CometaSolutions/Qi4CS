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
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Core.SPI.Model
{
   /// <summary>
   /// This interface specifies the methods that are required to validate and provide injection values.
   /// </summary>
   public interface InjectionFunctionality
   {
      /// <summary>
      /// Checks whether the injection is possible based on Qi4CS model.
      /// </summary>
      /// <param name="model">The Qi4CS model of the injectable element.</param>
      /// <returns><see cref="ValidationResult"/> with information about whether the injection is possible.</returns>
      /// <remarks>
      /// Even if injection would be possible based on model (<see cref="ValidationResult.InjectionPossible"/> is <c>true</c>), it may still fail during runtime.
      /// This method is used to catch obvious cases when in no way injection is possible.
      /// This method should not throw any exceptions.
      /// </remarks>
      ValidationResult InjectionPossible( AbstractInjectableModel model );

      /// <summary>
      /// Gets information about when injection should be performed for given Qi4CS model.
      /// </summary>
      /// <param name="model">The Qi4CS model of the injectable element.</param>
      /// <returns>Information about when injection should be performed.</returns>
      /// <remarks>
      /// This method should not throw any exceptions.
      /// </remarks>
      /// <seealso cref="InjectionTime"/>
      InjectionTime GetInjectionTime( AbstractInjectableModel model );

      /// <summary>
      /// Provides a value for injection at runtime.
      /// If the value could not be provided, this method should return <c>null</c>.
      /// </summary>
      /// <param name="instance">The current <see cref="CompositeInstance"/>.</param>
      /// <param name="model">The injectable element model.</param>
      /// <param name="targetType">The target type.</param>
      /// <returns>A value for injection with given injection scope and target type, or <c>null</c> if the injection value could not be provided.</returns>
      /// <remarks>
      /// This method should not throw any exceptions.
      /// </remarks>
      Object ProvideInjection( CompositeInstance instance, AbstractInjectableModel model, Type targetType );
   }

   /// <summary>
   /// The injection service acts as a aggregator for <see cref="InjectionFunctionality"/>.
   /// The instances of this interface are available through <see cref="ApplicationModel{T}.InjectionService"/> property.
   /// Despite its name, it is not a Qi4CS service and thus not available through <see cref="API.Model.ServiceAttribute"/> injection.
   /// </summary>
   /// <seealso cref="InjectionFunctionality"/>
   public interface InjectionService : InjectionFunctionality
   {
      /// <summary>
      /// Checks whether this <see cref="InjectionService"/> has <see cref="InjectionFunctionality"/> instance for given injection scope attribute.
      /// </summary>
      /// <param name="attr">The injection scope attribute.</param>
      /// <returns><c>true</c> if this <see cref="InjectionService"/> has <see cref="InjectionFunctionality"/> instane for <paramref name="attr"/> injection scope, <c>false</c> otherwise.</returns>
      Boolean HasFunctionalityFor( Attribute attr );
   }

   /// <summary>
   /// When injection possibility is checked via <see cref="InjectionFunctionality.InjectionPossible(AbstractInjectableModel)"/> method, this type encapsulates the result of that method.
   /// This type combines the boolean value of whether injection is possible and optional textual message.
   /// </summary>
   public sealed class ValidationResult
   {
      private readonly Boolean _result;
      private readonly String _errorMessage;

      /// <summary>
      /// Creates new instance of <see cref="ValidationResult"/> with given injection possibility and additional message.
      /// </summary>
      /// <param name="injectionIsPossible"><c>true</c> if injection is possible, <c>false</c> otherwise.</param>
      /// <param name="additionalMessage">Textual message about the injection possibility result. May be <c>null</c>.</param>
      public ValidationResult( Boolean injectionIsPossible, String additionalMessage )
      {
         this._result = injectionIsPossible;
         this._errorMessage = injectionIsPossible ? null : additionalMessage;
      }

      /// <summary>
      /// Gets value whether injection is possible.
      /// </summary>
      /// <value><c>true</c> if injection is possible; <c>false</c> otherwise.</value>
      public Boolean InjectionPossible
      {
         get
         {
            return this._result;
         }
      }

      /// <summary>
      /// Gets the additional textual message.
      /// Will be <c>null</c> if no message is supplied.
      /// </summary>
      /// <value>The optional additional textual message.</value>
      public String AdditionalMessage
      {
         get
         {
            return this._errorMessage;
         }
      }
   }

   /// <summary>
   /// This enum tells when to provide injection value.
   /// </summary>
   public enum InjectionTime
   {
      /// <summary>
      /// This value tells Qi4CS runtime that injection should be provided once when fragment is created.
      /// Subsequent uses of fragment will not cause injection value refresh.
      /// </summary>
      ON_CREATION,

      /// <summary>
      /// This value tells Qi4CS runtime that injection should be provided every time when fragment is used.
      /// This means that every time composite method is invoked on fragment containing injections with this injection time, injection value will be refreshed for that fragment.
      /// </summary>
      ON_METHOD_INVOKATION
   }
}
