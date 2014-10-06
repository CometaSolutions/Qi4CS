/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
using System.Text;
using Qi4CS.Core.API.Model;
using System.Threading;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Samples.Qi4CSDocumentation
{
   #region PassivationTokenCode1
   public interface MyServiceWithBackgroundThread
   {
   }

   public class MyServiceWithBackgroundThreadMixin : MyServiceWithBackgroundThread
   {
      [Structure]
      private Application _application;

      [Activate]
      protected void Activate()
      {
         new Thread( this.ThreadMainMethod ).Start();
      }

      private void ThreadMainMethod()
      {
         // do some constant task in while loop
      }
   }
   #endregion

   public class Dummy1
   {
      private Application _application;

      #region PassivationTokenCode2
      private void ThreadMainMethod()
      {
         while ( this._application.Active )
         {
            // Do something
         }
      }
      #endregion
   }

   public class Dummy2
   {
      private Application _application;

      #region PassivationTokenCode3
      private void ThreadMainMethod()
      {
         while ( !this._application.PassivationToken.IsCancellationRequested )
         {
            // Do something
         }
      }
      #endregion
   }
   namespace Dummeh
   {
      #region PassivationTokenCode4
      public class MyServiceWithBackgroundThreadMixin : MyServiceWithBackgroundThread
      {
         [Structure]
         private Application _application;

         [Activate]
         protected void Activate()
         {
            var token = this._application.PassivationToken;
            new Thread( () => this.ThreadMainMethod( token ) ).Start();
         }

         private void ThreadMainMethod( CancellationToken token )
         {
            while ( !token.IsCancellationRequested )
            {
               // Do something
            }
         }
      }
      #endregion
   }
}
