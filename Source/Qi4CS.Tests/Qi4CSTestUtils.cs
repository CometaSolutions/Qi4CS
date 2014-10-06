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
using System.IO;
using System.Linq;
using System.Text;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.API.Model;

namespace Qi4CS.Tests
{
   public static class Qi4CSTestUtils
   {
      public static AppDomainEmulator CreateTestAppDomain( String name )
      {
         var s = new AppDomainSetup();
         s.ApplicationBase = Environment.CurrentDirectory;
         //s.ShadowCopyFiles = "true";
         //s.CachePath = Path.Combine( s.ApplicationBase, "qi4cs_test_shadowcache" );
         //s.ApplicationName = "qi4cs";
         //s.ShadowCopyDirectories = s.ApplicationBase;
         return new AppDomainEmulator( name, s );
      }
   }

   public class AppDomainEmulator : IDisposable
   {

      private readonly AppDomain _domain;

      public AppDomainEmulator( String name, AppDomainSetup setyyp )
      {
         this._domain = AppDomain.CreateDomain( name, null, setyyp );
      }


      public void DoCallBack( Action action )
      {
         this._domain.DoCallBack( new CrossAppDomainDelegate( action ) );
      }

      #region IDisposable Members

      public void Dispose()
      {
         //var path = Path.Combine( this._domain.SetupInformation.CachePath, this._domain.SetupInformation.ApplicationName );
         AppDomain.Unload( this._domain );
         foreach ( var fn in Directory.EnumerateFiles( Environment.CurrentDirectory, "*" + Qi4CSGeneratedAssemblyAttribute.ASSEMBLY_NAME_SUFFIX + ".dll", SearchOption.TopDirectoryOnly ) )
         {
            try
            {
               File.Delete( fn );
            }
            catch
            {
               // Ignore - eg. multipleactivationtest causes this
            }
         }
         //Directory.Delete( path, true ); // Currently, this throws exception
      }

      #endregion
   }
}
