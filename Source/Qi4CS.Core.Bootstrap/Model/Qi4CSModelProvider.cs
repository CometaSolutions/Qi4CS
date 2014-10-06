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
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Bootstrap.Model
{
   /// <summary>
   /// During bootstrap process, the users of Qi4CS can implement this interface to centralize the bootstrap code.
   /// </summary>
   /// <typeparam name="TModel">The type of the <see cref="ApplicationModel{T}"/>.</typeparam>
   /// <remarks>
   /// This interface can be used by an IDE extension to create model and generate code during the compile.
   /// </remarks>
   public interface Qi4CSModelProvider<out TModel>
      where TModel : ApplicationModel<ApplicationSPI>
   {
      /// <summary>
      /// Gets or creates <see cref="ApplicationModel{T}"/> of the target application.
      /// </summary>
      /// <value>A <see cref="ApplicationModel{T}"/> of the target application.</value>
      TModel Model { get; }
   }

   /// <summary>
   /// This is skeleton implementation for <see cref="Qi4CSModelProvider{T}"/>.
   /// It implements the <see cref="Qi4CSModelProvider{T}.Model"/> property by using <see cref="Lazy{T}"/> field with <see cref="BuildArchitecture"/> as factory method.
   /// </summary>
   /// <typeparam name="TModel">The type of the <see cref="ApplicationModel{T}"/>.</typeparam>
   public abstract class Qi4CSModelProviderSkeleton<TModel> : Qi4CSModelProvider<TModel>
      where TModel : ApplicationModel<ApplicationSPI>
   {

      private readonly Lazy<TModel> _model;

      /// <summary>
      /// Creates a new instance of <see cref="Qi4CSModelProviderSkeleton{T}"/>.
      /// </summary>
      protected Qi4CSModelProviderSkeleton()
      {
         this._model = new Lazy<TModel>( () => this.BuildArchitecture().CreateModel(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication );
      }

      #region Qi4CSModelProvider<TModel> Members

      /// <inheritdoc />
      public TModel Model
      {
         get
         {
            return this._model.Value;
         }
      }

      #endregion

      /// <summary>
      /// This method should be overridden by subclasses.
      /// The code of the overridden method should create Qi4CS architecture and add all required composites for the whole Qi4CS application.
      /// </summary>
      /// <returns><see cref="ApplicationArchitecture{T}"/> with all required composites assembled.</returns>
      protected abstract ApplicationArchitecture<TModel> BuildArchitecture();

   }

   /// <summary>
   /// This is helper class to provide skeleton for <see cref="Qi4CSModelProvider{T}"/> without generic arguments.
   /// </summary>
   public abstract class Qi4CSModelProviderSkeleton : Qi4CSModelProviderSkeleton<ApplicationModel<ApplicationSPI>>
   {

   }
}
