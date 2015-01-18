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
using CILAssemblyManipulator.API;
using CommonUtils;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public interface CompositeModelTypeCodeGenerator
   {
      void EmitTypesForModel( CompositeModel model, CompositeTypeModel typeModel, Assembly assemblyBeingProcessed, CILModule mob, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo );
      void EmitMembersForModel( CompositeModel model, CompositeTypeModel typeModel, Assembly assemblyBeingProcessed, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo );
      void EmitILForModel( CompositeModel model, CompositeTypeModel typeModel, Assembly assemblyBeingProcessed, CILModule module, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo );

      // TODO move CompositeEmittingInfo to AbstractCompositeModelTypeCodeGenerator constructor.
      //void EmitFragmentMethods( CompositeModel model, CompositeTypeModel typeModel, Assembly assemblyBeingProcessed, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo );
      //void EmitCompositeMethosAndInvocationInfos( CompositeModel model, CompositeTypeModel typeModel, Assembly assemblyBeingProcessed, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo );
      //void EmitCompositeExtraMethods( CompositeModel model, CompositeTypeModel typeModel, Assembly assemblyBeingProcessed, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo );
      //void EmitCompositeConstructors( CompositeModel model, CompositeTypeModel typeModel, Assembly assemblyBeingProcessed, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo );
      //void EmitCompositeFactory( CompositeModel model, Assembly assemblyBeingProcessed, CILModule module, CompositeCodeGenerationInfo codeGenerationInfo, CompositeEmittingInfo emittingInfo );
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
      private readonly ConcurrentDictionary<Tuple<Assembly, CompositeModel>, CompositeTypeGenerationInfo> _publicCompositeGenerationInfo;
      private readonly ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> _compositeTypeGenerationInfos;
      private readonly ConcurrentDictionary<TypeBindingInformation, Tuple<IList<FragmentTypeGenerationInfo>, Object>> _fragmentTypeGenerationInfos;
      private readonly ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> _concernInvocationTypeGenInfos;
      private readonly ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> _sideEffectInvocationTypeGenInfos;
      private readonly ConcurrentDictionary<FragmentTypeGenerationInfo, CompositeMethodGenerationInfo> _fragmentCreationMethods;
      private readonly IDictionary<CompositeModel, CompositeEmittingIDInfo> _idInfos;
      private readonly ConcurrentDictionary<CompositeTypeGenerationInfo, CompositeTypeGenerationInfo> _typesWithExtraMethods;
      private readonly ConcurrentDictionary<CompositeTypeGenerationInfo, CompositeTypeGenerationInfo> _typesWithCtors;
      private readonly ConcurrentDictionary<CompositeTypeGenerationInfo, CompositeTypeGenerationInfo> _typesWithCompositeMethods;
      private readonly ConcurrentDictionary<CompositeModel, ConcurrentDictionary<Int32, TypeGenerationInfo>> _allGenerationInfos;
      private readonly ConcurrentDictionary<CompositeTypeModel, IDictionary<CILType, TypeBindingInformation>> _emulatedFragmentTypeInfos;
      private readonly ConcurrentDictionary<CompositeMethodModel, CompositeMethodGenerationInfo> _compositeMethodGenerationInfos;
      private readonly ConcurrentDictionary<CompositeTypeGenerationInfo, CompositeConstructorGenerationInfo> _compositeCtors;
      private readonly CILReflectionContext _ctx;

      public CompositeEmittingInfo( CILReflectionContext ctx, IEnumerable<CompositeModel> models )
      {
         this._ctx = ctx;
         this._publicCompositeGenerationInfo = new ConcurrentDictionary<Tuple<Assembly, CompositeModel>, CompositeTypeGenerationInfo>( EqualityComparer<Tuple<Assembly, CompositeModel>>.Default );
         this._compositeTypeGenerationInfos = new ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>>( ReferenceEqualityComparer<TypeBindingInformation>.ReferenceBasedComparer );
         this._fragmentTypeGenerationInfos = new ConcurrentDictionary<TypeBindingInformation, Tuple<IList<FragmentTypeGenerationInfo>, Object>>( ReferenceEqualityComparer<TypeBindingInformation>.ReferenceBasedComparer );
         this._concernInvocationTypeGenInfos = new ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>>( ReferenceEqualityComparer<TypeBindingInformation>.ReferenceBasedComparer );
         this._sideEffectInvocationTypeGenInfos = new ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>>( ReferenceEqualityComparer<TypeBindingInformation>.ReferenceBasedComparer );
         this._fragmentCreationMethods = new ConcurrentDictionary<FragmentTypeGenerationInfo, CompositeMethodGenerationInfo>();
         this._idInfos = models.ToDictionary( model => model, model => new CompositeEmittingIDInfo() );
         this._typesWithExtraMethods = new ConcurrentDictionary<CompositeTypeGenerationInfo, CompositeTypeGenerationInfo>( ReferenceEqualityComparer<CompositeTypeGenerationInfo>.ReferenceBasedComparer );
         this._typesWithCtors = new ConcurrentDictionary<CompositeTypeGenerationInfo, CompositeTypeGenerationInfo>( ReferenceEqualityComparer<CompositeTypeGenerationInfo>.ReferenceBasedComparer );
         this._typesWithCompositeMethods = new ConcurrentDictionary<CompositeTypeGenerationInfo, CompositeTypeGenerationInfo>( ReferenceEqualityComparer<CompositeTypeGenerationInfo>.ReferenceBasedComparer );
         this._allGenerationInfos = new ConcurrentDictionary<CompositeModel, ConcurrentDictionary<Int32, TypeGenerationInfo>>();
         this._emulatedFragmentTypeInfos = new ConcurrentDictionary<CompositeTypeModel, IDictionary<CILType, TypeBindingInformation>>();
         this._compositeMethodGenerationInfos = new ConcurrentDictionary<CompositeMethodModel, CompositeMethodGenerationInfo>();
         this._compositeCtors = new ConcurrentDictionary<CompositeTypeGenerationInfo, CompositeConstructorGenerationInfo>();
      }

      public void RegisterGenerationInfo( CompositeModel compositeModel, TypeGenerationInfo genInfo, Int32 typeID )
      {
         this._allGenerationInfos.GetOrAdd( compositeModel, muudel => new ConcurrentDictionary<Int32, TypeGenerationInfo>() ).TryAdd( typeID, genInfo );
      }

      public void RegisterCompositeMethodGenerationInfo( CompositeMethodModel model, CompositeMethodGenerationInfo info )
      {
         this._compositeMethodGenerationInfos.TryAdd( model, info );
      }

      public CompositeMethodGenerationInfo GetCompositeMethodGenerationInfo( CompositeMethodModel model )
      {
         return this._compositeMethodGenerationInfos[model];
      }

      public IDictionary<Int32, TypeGenerationInfo> GetGenerationInfosByTypeID( CompositeModel compositeModel )
      {
         return this._allGenerationInfos[compositeModel];
      }

      public void AddPublicCompositeGenerationInfo( Assembly assembly, CompositeModel compositeModel, CompositeTypeGenerationInfo typeGenInfo )
      {
         this._publicCompositeGenerationInfo.TryAdd( Tuple.Create( assembly, compositeModel ), typeGenInfo );
         //Interlocked.CompareExchange( ref this._firstPublicTypeGenInfo, typeGenInfo, null );
      }

      public Boolean IsMainCompositeGenerationInfo( CompositeTypeGenerationInfo typeGenInfo, Assembly assemblyBeingProcessed )
      {
         return typeGenInfo.CompositeModel.MainCodeGenerationType.Assembly.Equals( assemblyBeingProcessed );
         //return Object.ReferenceEquals( this._firstPublicTypeGenInfo, typeGenInfo );
      }

      public Boolean IsMainCompositeGenerationInfo( CompositeTypeGenerationInfo typeGenInfo, CILType mainCompositeTypeAttributeType )
      {
         return typeGenInfo.Builder.CustomAttributeData.Any( d => d.Constructor.DeclaringType.Equals( mainCompositeTypeAttributeType ) );
      }

      public IEnumerable<CompositeTypeGenerationInfo> PublicCompositeGenerationInfos
      {
         get
         {
            return this._publicCompositeGenerationInfo.Values;
         }
      }

      public ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> CompositeTypeGenerationInfos
      {
         get
         {
            return this._compositeTypeGenerationInfos;
         }
      }

      public ConcurrentDictionary<TypeBindingInformation, Tuple<IList<FragmentTypeGenerationInfo>, Object>> FragmentTypeGenerationInfos
      {
         get
         {
            return this._fragmentTypeGenerationInfos;
         }
      }

      public ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> ConcernTypeGenerationInfos
      {
         get
         {
            return this._concernInvocationTypeGenInfos;
         }
      }

      public ConcurrentDictionary<TypeBindingInformation, Tuple<IList<CompositeTypeGenerationInfo>, Object>> SideEffectTypeGenerationInfos
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
            return this._publicCompositeGenerationInfo.Values.Concat( this._compositeTypeGenerationInfos.Values.SelectMany( value => value.Item1 ) ).Distinct();
         }
      }

      public ConcurrentDictionary<FragmentTypeGenerationInfo, CompositeMethodGenerationInfo> FragmentCreationMethods
      {
         get
         {
            return this._fragmentCreationMethods;
         }
      }

      public ConcurrentDictionary<CompositeTypeGenerationInfo, CompositeConstructorGenerationInfo> CompositeConstructors
      {
         get
         {
            return this._compositeCtors;
         }
      }


      public CompositeTypeGenerationInfo GetPublicComposite( CompositeModel model, Assembly assembly )
      {
         CompositeTypeGenerationInfo result;
         this._publicCompositeGenerationInfo.TryGetValue( Tuple.Create( assembly, model ), out result );
         return result;
      }

      public IEnumerable<Tuple<Assembly, CompositeTypeGenerationInfo>> GetAllPublicComposites( CompositeModel model )
      {
         return this._publicCompositeGenerationInfo.Where( kvp => model.Equals( kvp.Key.Item2 ) ).Select( kvp => Tuple.Create( kvp.Key.Item1, kvp.Value ) );
      }

      public Int32 NewPrivateCompositeID( CompositeModel model )
      {
         return Interlocked.Increment( ref this._idInfos[model].currentPrivateCompositeID );
      }

      public Int32 NewFragmentID( CompositeModel model )
      {
         return Interlocked.Increment( ref this._idInfos[model].currentFragmentID );
      }

      public Int32 NewConcernInvocationID( CompositeModel model )
      {
         return Interlocked.Increment( ref this._idInfos[model].currentConcernInvocationID );
      }

      public Int32 NewSideEffectInvocationID( CompositeModel model )
      {
         return Interlocked.Increment( ref this._idInfos[model].currentSideEffectInvocationID );
      }

      public Int32 NewCompositeTypeID( CompositeModel model )
      {
         return Interlocked.Increment( ref this._idInfos[model].currentCompositeTypeID );
      }

      public Boolean TryAddTypeWithExtraMethods( CompositeTypeGenerationInfo type )
      {
         return this._typesWithExtraMethods.TryAdd( type, type );
      }

      public Boolean TryAddTypeWithCtor( CompositeTypeGenerationInfo type )
      {
         return this._typesWithCtors.TryAdd( type, type );
      }

      public Boolean TryAddTypeWithCompositeMethods( CompositeTypeGenerationInfo type )
      {
         return this._typesWithCompositeMethods.TryAdd( type, type );
      }

      public IDictionary<CILType, TypeBindingInformation> GetEmulatedFragmentTypeBindingInfos( CompositeTypeModel typeModel )
      {
         return this._emulatedFragmentTypeInfos.GetOrAdd( typeModel, tm => tm.FragmentTypeInfos.ToDictionary( kvp => kvp.Key.NewWrapperAsType( this._ctx ), kvp => kvp.Value ) );
      }
   }
}
#endif