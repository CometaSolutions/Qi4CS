/*
 * Copyright (c) 2008, Rickard Öberg.
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
using CommonUtils;
using Qi4CS.Core.API.Common;
using System.Threading;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// The <see cref="Application"/> represents whole Qi4CS application.
   /// It provides some information about the application, and methods to activate and passivate it.
   /// </summary>
   public interface Application : ActivationObservable<ApplicationActivationArgs, ApplicationPassivationArgs>
   {
      /// <summary>
      /// Gets the name of the application.
      /// </summary>
      /// <value>The name of the application.</value>
      String Name { get; }

      /// <summary>
      /// Gets the mode of the application. E.g. <c>"test"</c>, <c>"development"</c>, <c>"production"</c>, etc.
      /// </summary>
      /// <value>The mode of the application.</value>
      String Mode { get; }

      /// <summary>
      /// Gets the textual version value of the application. The value can be in any format.
      /// </summary>
      /// <value>The textual version value of the application. The value can be in any format.</value>
      String Version { get; }

      /// <summary>
      /// Calling this method will activate this application, if it is not already active or activating.
      /// </summary>
      /// <remarks>
      /// Specifically, all services that are specified to activate with the application, will be activated as well.
      /// </remarks>
      void Activate();

      /// <summary>
      /// Calling this method will passivate this application, if it is not already passive or passivating.
      /// </summary>
      /// <exception cref="ApplicationPassivationException">If exceptions occur in event handlers during passivation. All active services will be passivated nevertheless.</exception>
      void Passivate();

      /// <summary>
      /// Checks whether this application is active, i.e. <see cref="Activate"/> method has completed successfully.
      /// </summary>
      /// <value><c>true</c> if this application is active; <c>false</c> otherwise</value>
      Boolean Active { get; }

      /// <summary>
      /// Checks whether this application is passive, i.e. the application has not yet been activated or <see cref="Passivate"/> method has completed with or without exceptions.
      /// </summary>
      /// <value><c>true</c> if this application is passive; <c>false</c> otherwise.</value>
      Boolean Passive { get; }

      /// <summary>
      /// The purpose of this property is to provide safe and certain mechanism to detect application passivation from another thread.
      /// This is useful in e.g. services which have some kind of background thread running during application activation.
      /// Please see remarks for proper usage of this property.
      /// </summary>
      /// <value>The <see cref="CancellationToken"/> which will get cancelled once application is passivated.</value>
      /// <remarks>
      /// <para>
      /// Consider the following example service and mixin for it.
      /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\ApplicationPassivationTokenSample.cs" region="PassivationTokenCode1" language="C#" />
      /// </para>
      /// <para>
      /// The typical implementation of <c>ThreadMainMethod</c> would be something like this:
      /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\ApplicationPassivationTokenSample.cs" region="PassivationTokenCode2" language="C#" />
      /// However, the problem with the above code is that if application is passivated and re-activated quickly enough, the check in the while loop of <c>ThreadMainMethod</c> won't notice it, and one will end up with *two* threads running the same task (because of re-activation starting the thread again).
      /// </para>
      /// <para>
      /// Using <see cref="PassivationToken"/> property helps this scenario, but it must be used in a correct way.
      /// Here is the typical usecase scenario, which is *not* correct:
      /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\ApplicationPassivationTokenSample.cs" region="PassivationTokenCode3" language="C#" />
      /// If, once again, passivation and re-activation happens quickly enough, the check in the <c>while</c>-loop won't notice the cancellation request (since after re-activation, the <see cref="CancellationToken.IsCancellationRequested"/> property will be <c>false</c> again).
      /// </para>
      /// <para>
      /// The correct way to use this is to acquire the <see cref="CancellationToken"/> in the activation method, when the application is still active.
      /// Then, the token should be passed on to the main thread method, which will use it to check for cancellation.
      /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\ApplicationPassivationTokenSample.cs" region="PassivationTokenCode4" language="C#" />
      /// </para>
      /// </remarks>
      CancellationToken PassivationToken { get; }
   }

   /// <summary>
   /// Instances of this class will be passed as event argument parameter for <see cref="ActivationObservable{T,U}.AfterActivation"/> event during the <see cref="Instance.Application.Activate"/> call.
   /// </summary>
   public sealed class ApplicationActivationArgs : EventArgs
   {
      private readonly Application _application;

      /// <summary>
      /// Creates a new instance of <see cref="ApplicationActivationArgs"/>.
      /// </summary>
      /// <param name="app">The Qi4CS <see cref="Application"/>.</param>
      /// <remarks>Qi4CS will take care of creating instances of <see cref="ApplicationActivationArgs"/>, so user code will not need to use this.</remarks>
      public ApplicationActivationArgs( Application app )
      {
         ArgumentValidator.ValidateNotNull( "Application", app );

         this._application = app;
      }

      /// <summary>
      /// Gets the Qi4CS <see cref="Application"/> being activated.
      /// </summary>
      /// <value>The Qi4CS <see cref="Application"/> being activated.</value>
      public Application Application
      {
         get
         {
            return this._application;
         }
      }
   }

   /// <summary>
   /// Instances of this class will be passed as event argument parameter for <see cref="ActivationObservable{T,U}.AfterPassivation"/> event during the <see cref="Instance.Application.Passivate"/> call.
   /// </summary>
   public sealed class ApplicationPassivationArgs : EventArgs
   {
      private readonly Application _application;

      /// <summary>
      /// Creates a new instance of <see cref="ApplicationPassivationArgs"/>.
      /// </summary>
      /// <param name="app">The Qi4CS <see cref="Application"/>.</param>
      /// <remarks>Qi4CS will take care of creating instances of <see cref="ApplicationPassivationArgs"/>, so user code will not need to use this.</remarks>
      public ApplicationPassivationArgs( Application app )
      {
         ArgumentValidator.ValidateNotNull( "Application", app );

         this._application = app;
      }

      /// <summary>
      /// Gets the Qi4CS <see cref="Application"/> being passivated.
      /// </summary>
      /// <value>The Qi4CS <see cref="Application"/> being passivated.</value>
      public Application Application
      {
         get
         {
            return this._application;
         }
      }
   }

}
