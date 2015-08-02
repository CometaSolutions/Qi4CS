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
#if QI4CS_SDK
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CILAssemblyManipulator.Logical;
using CommonUtils;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.Runtime.Instance;

namespace Qi4CS.Core.Runtime.Model
{
   public interface CompositeModelTypeCodeGenerator
   {
      IDictionary<Assembly, CILType[]> EmitCodeForCompositeModel( CompositeModelEmittingArgs args );
   }

   public sealed class CompositeModelEmittingArgs
   {
      private readonly CompositeModel _model;
      private readonly CompositeTypeModel _typeModel;
      private readonly IDictionary<System.Reflection.Assembly, CILModule> _assemblies;

      public CompositeModelEmittingArgs(
         CompositeModel model,
         CompositeTypeModel typeModel,
         IDictionary<Assembly, CILModule> assemblies
         )
      {
         ArgumentValidator.ValidateNotNull( "Composite model", model );
         ArgumentValidator.ValidateNotNull( "Composite type model", typeModel );
         ArgumentValidator.ValidateNotNull( "Assembles", assemblies );

         this._model = model;
         this._typeModel = typeModel;
         this._assemblies = assemblies;
      }

      public CompositeModel Model
      {
         get
         {
            return this._model;
         }
      }
      public CompositeTypeModel TypeModel
      {
         get
         {
            return this._typeModel;
         }
      }

      public IDictionary<System.Reflection.Assembly, CILModule> Assemblies
      {
         get
         {
            return this._assemblies;
         }
      }
   }

   public class CompositeEmittingInfo
   {
      private class CompositeEmittingIDInfo
      {
         internal Int32 currentPrivateCompositeID;
         internal Int32 currentFragmentID;
         internal Int32 currentConcernInvocationID;
         internal Int32 currentSideEffectInvocationID;
         internal Int32 currentCompositeTypeID;

         internal CompositeEmittingIDInfo()
         {
            this.currentPrivateCompositeID = 0;
            this.currentFragmentID = 0;
            this.currentConcernInvocationID = 0;
            this.currentSideEffectInvocationID = 0;
            this.currentCompositeTypeID = 0;
         }
      }

      //private CompositeTypeGenerationInfo _firstPublicTypeGenInfo;
      private readonly IList<CompositeTypeGenerationInfo> _publicCompositeGenerationInfos;
      private readonly IDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> _compositeTypeGenerationInfos;
      private readonly IDictionary<TypeBindingInformation, Tuple<IList<FragmentTypeGenerationInfo>, Object>> _fragmentTypeGenerationInfos;
      private readonly IDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> _concernInvocationTypeGenInfos;
      private readonly IDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> _sideEffectInvocationTypeGenInfos;
      private readonly IDictionary<FragmentTypeGenerationInfo, CompositeMethodGenerationInfo> _fragmentCreationMethods;
      private readonly CompositeEmittingIDInfo _idInfo;
      private readonly IDictionary<CompositeModel, IDictionary<Int32, TypeGenerationInfo>> _allGenerationInfos;
      private readonly IDictionary<CompositeTypeModel, IDictionary<CILType, TypeBindingInformation>> _emulatedFragmentTypeInfos;
      private readonly IDictionary<CompositeMethodModel, CompositeMethodGenerationInfo> _compositeMethodGenerationInfos;
      private readonly CILReflectionContext _ctx;

      public CompositeEmittingInfo( CILReflectionContext ctx )
      {
         this._ctx = ctx;
         this._publicCompositeGenerationInfos = new List<CompositeTypeGenerationInfo>();
         this._compositeTypeGenerationInfos = new Dictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>>( ReferenceEqualityComparer<TypeBindingInformation>.ReferenceBasedComparer );
         this._fragmentTypeGenerationInfos = new Dictionary<TypeBindingInformation, Tuple<IList<FragmentTypeGenerationInfo>, Object>>( ReferenceEqualityComparer<TypeBindingInformation>.ReferenceBasedComparer );
         this._concernInvocationTypeGenInfos = new Dictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>>( ReferenceEqualityComparer<TypeBindingInformation>.ReferenceBasedComparer );
         this._sideEffectInvocationTypeGenInfos = new Dictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>>( ReferenceEqualityComparer<TypeBindingInformation>.ReferenceBasedComparer );
         this._fragmentCreationMethods = new Dictionary<FragmentTypeGenerationInfo, CompositeMethodGenerationInfo>();
         this._idInfo = new CompositeEmittingIDInfo();
         this._allGenerationInfos = new Dictionary<CompositeModel, IDictionary<Int32, TypeGenerationInfo>>();
         this._emulatedFragmentTypeInfos = new Dictionary<CompositeTypeModel, IDictionary<CILType, TypeBindingInformation>>();
         this._compositeMethodGenerationInfos = new Dictionary<CompositeMethodModel, CompositeMethodGenerationInfo>();
      }

      public void RegisterGenerationInfo( CompositeModel compositeModel, TypeGenerationInfo genInfo, Int32 typeID )
      {
         this._allGenerationInfos.GetOrAdd_NotThreadSafe( compositeModel, muudel => new Dictionary<Int32, TypeGenerationInfo>() ).Add( typeID, genInfo );
      }

      public void RegisterCompositeMethodGenerationInfo( CompositeMethodModel model, CompositeMethodGenerationInfo info )
      {
         this._compositeMethodGenerationInfos.Add( model, info );
      }

      public CompositeMethodGenerationInfo GetCompositeMethodGenerationInfo( CompositeMethodModel model )
      {
         return this._compositeMethodGenerationInfos[model];
      }

      public IDictionary<CompositeMethodModel, CompositeMethodGenerationInfo> CompositeMethodGenerationInfos
      {
         get
         {
            return this._compositeMethodGenerationInfos;
         }
      }

      public IDictionary<Int32, TypeGenerationInfo> GetGenerationInfosByTypeID( CompositeModel compositeModel )
      {
         return this._allGenerationInfos[compositeModel];
      }

      public void AddPublicCompositeGenerationInfo( CompositeTypeGenerationInfo typeGenInfo )
      {
         this._publicCompositeGenerationInfos.Add( typeGenInfo );
      }

      public IEnumerable<CompositeTypeGenerationInfo> PublicCompositeGenerationInfos
      {
         get
         {
            return this._publicCompositeGenerationInfos;
         }
      }

      public IDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> PrivateCompositeTypeGenerationInfos
      {
         get
         {
            return this._compositeTypeGenerationInfos;
         }
      }

      public IDictionary<TypeBindingInformation, Tuple<IList<FragmentTypeGenerationInfo>, Object>> FragmentTypeGenerationInfos
      {
         get
         {
            return this._fragmentTypeGenerationInfos;
         }
      }

      public IDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> ConcernTypeGenerationInfos
      {
         get
         {
            return this._concernInvocationTypeGenInfos;
         }
      }

      public IDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> SideEffectTypeGenerationInfos
      {
         get
         {
            return this._sideEffectInvocationTypeGenInfos;
         }
      }

      public IEnumerable<CompositeTypeGenerationInfo> AllCompositeGenerationInfos
      {
         get
         {
            return this._publicCompositeGenerationInfos.Concat( this._compositeTypeGenerationInfos.Values.SelectMany( value => value.Item1 ) ).Distinct();
         }
      }

      public IDictionary<FragmentTypeGenerationInfo, CompositeMethodGenerationInfo> FragmentCreationMethods
      {
         get
         {
            return this._fragmentCreationMethods;
         }
      }

      public Int32 NewPrivateCompositeID()
      {
         return Interlocked.Increment( ref this._idInfo.currentPrivateCompositeID );
      }

      public Int32 NewFragmentID()
      {
         return Interlocked.Increment( ref this._idInfo.currentFragmentID );
      }

      public Int32 NewConcernInvocationID()
      {
         return Interlocked.Increment( ref this._idInfo.currentConcernInvocationID );
      }

      public Int32 NewSideEffectInvocationID()
      {
         return Interlocked.Increment( ref this._idInfo.currentSideEffectInvocationID );
      }

      public Int32 NewCompositeTypeID()
      {
         return Interlocked.Increment( ref this._idInfo.currentCompositeTypeID );
      }

      public IDictionary<CILType, TypeBindingInformation> GetEmulatedFragmentTypeBindingInfos( CompositeTypeModel typeModel )
      {
         return this._emulatedFragmentTypeInfos.GetOrAdd_NotThreadSafe( typeModel, tm => tm.FragmentTypeInfos.ToDictionary( kvp => this._ctx.NewWrapperAsType( kvp.Key ), kvp => kvp.Value ) );
      }
   }
}
#endif