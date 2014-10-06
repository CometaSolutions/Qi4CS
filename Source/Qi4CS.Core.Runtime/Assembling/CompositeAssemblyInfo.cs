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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonUtils;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Runtime.Common;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Core.Runtime.Assembling
{
   public interface CompositeAssemblyInfo : UsesProvider<CompositeAssemblyInfo>
   {
      FragmentAssemblyInfo GetFragmentAssemblyInfo( FragmentModelType fragmentModelType );

      IEnumerable<FragmentAssemblyInfo> GetFragmentAssemblyInfos( params FragmentModelType[] fragmentModelTypes );

      UsesContainerQuery MetaInfoContainer { get; }

      CompositeModelType CompositeModelType { get; }

      ISet<Type> Types { get; }

      Type MainCodeGenerationType { get; set; }

      Int32 CompositeID { get; }

      IDictionary<PropertyInfo, Func<PropertyInfo, ApplicationSPI, Object>> DefaultProviders { get; }
   }

   public class CompositeAssemblyInfoImpl : CompositeAssemblyInfo
   {
      private readonly ISet<Type> _types;
      private readonly IDictionary<FragmentModelType, FragmentAssemblyInfo> _fragmentInfos;
      private readonly IDictionary<PropertyInfo, Func<PropertyInfo, ApplicationSPI, Object>> _defaultProviders;
      private readonly UsesContainerMutable _metaInfoContainer;
      private readonly CompositeModelType _modelType;
      private readonly Int32 _id;
      private Type _mainCodeGenerationType;

      public CompositeAssemblyInfoImpl( Int32 id, CompositeModelType modelType, UsesContainerQuery parentContainer )
      {
         ArgumentValidator.ValidateNotNull( "Composite model type", modelType );

         this._id = id;
         this._fragmentInfos = new Dictionary<FragmentModelType, FragmentAssemblyInfo>();
         this._defaultProviders = new Dictionary<PropertyInfo, Func<PropertyInfo, ApplicationSPI, Object>>();
         this._metaInfoContainer = UsesContainerMutableImpl.CreateWithParent( parentContainer );
         this._modelType = modelType;
         this._types = new HashSet<Type>();
      }

      #region CompositeAssemblyInfo Members

      public FragmentAssemblyInfo GetFragmentAssemblyInfo( FragmentModelType fragmentModelType )
      {
         FragmentAssemblyInfo result = null;
         if ( !this._fragmentInfos.TryGetValue( fragmentModelType, out result ) )
         {
            result = new FragmentAssemblyInfo();
            this._fragmentInfos.Add( fragmentModelType, result );
         }
         return result;
      }

      public IEnumerable<FragmentAssemblyInfo> GetFragmentAssemblyInfos( params FragmentModelType[] fragmentModelTypes )
      {
         return this._fragmentInfos.Where( info => fragmentModelTypes.Contains( info.Key ) ).Select( kvp => kvp.Value );
      }

      public UsesContainerQuery MetaInfoContainer
      {
         get
         {
            return this._metaInfoContainer.Query;
         }
      }

      public CompositeModelType CompositeModelType
      {
         get
         {
            return this._modelType;
         }
      }

      public ISet<Type> Types
      {
         get
         {
            return this._types;
         }
      }

      public Type MainCodeGenerationType
      {
         get
         {
            return this._mainCodeGenerationType;
         }
         set
         {
            this._mainCodeGenerationType = value;
         }
      }

      public Int32 CompositeID
      {
         get
         {
            return this._id;
         }
      }

      public IDictionary<PropertyInfo, Func<PropertyInfo, ApplicationSPI, Object>> DefaultProviders
      {
         get
         {
            return this._defaultProviders;
         }
      }

      #endregion

      #region UsesProvider Members

      public CompositeAssemblyInfo Use( params Object[] objects )
      {
         this._metaInfoContainer.Use( objects );
         return this;
      }

      public CompositeAssemblyInfo UseWithName( String name, Object value )
      {
         this._metaInfoContainer.UseWithName( name, value );
         return this;
      }

      #endregion
   }

}