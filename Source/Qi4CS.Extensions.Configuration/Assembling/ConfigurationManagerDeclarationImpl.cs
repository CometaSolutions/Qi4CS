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
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Extensions.Configuration.Instance;

namespace Qi4CS.Extensions.Configuration.Assembling
{
   internal class ConfigurationManagerDeclarationImpl : ConfigurationManagerDeclaration
   {
      private readonly ServiceCompositeAssemblyDeclaration _decl;
      private readonly ConfigurationManagerInfo _info;

      internal ConfigurationManagerDeclarationImpl( ServiceCompositeAssemblyDeclaration decl, ConfigurationManagerInfo info )
      {
         ArgumentValidator.ValidateNotNull( "Service declaration", decl );
         ArgumentValidator.ValidateNotNull( "Configuration manager info", info );

         this._decl = decl;
         this._info = info;
      }

      #region ConfigurationManagerDeclaration Members

      public ConfigurationCompositeDefaultInfo WithDefaultsFor( params Type[] types )
      {
         return new ConfigurationCompositeDefaultInfoImpl( this, this._info, types.EmptyIfNull() );
      }

      public ServiceCompositeAssemblyDeclaration ServiceDeclaration
      {
         get
         {
            return this._decl;
         }
      }

      #endregion
   }
}
