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
using System.Collections.Generic;
using System.Linq;
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.SPI.Model;
using System.Reflection;

namespace Qi4CS.Core.Runtime.Model
{
   public interface CompositeTypeModel
   {
      ListQuery<Type> PublicCompositeGenericArguments { get; }
      DictionaryQuery<Type, TypeBindingInformation> FragmentTypeInfos { get; }
      DictionaryQuery<Type, TypeBindingInformation> ConcernInvocationTypeInfos { get; }
      DictionaryQuery<Type, TypeBindingInformation> SideEffectInvocationTypeInfos { get; }
      DictionaryQuery<Type, TypeBindingInformation> PrivateCompositeTypeInfos { get; }
   }

   public class CompositeTypeModelImpl : CompositeTypeModel
   {
      private class InjectableModelTargetTypeComparer : IEqualityComparer<ThisTypeInfo>
      {

         #region IEqualityComparer<AbstractInjectableModel> Members

         public bool Equals( ThisTypeInfo x, ThisTypeInfo y )
         {
            return x.resolvedTargetType.Equals( y.resolvedTargetType );
         }

         public int GetHashCode( ThisTypeInfo obj )
         {
            return obj.resolvedTargetType.GetHashCode();
         }

         #endregion
      }

      private class GDefComparer : IEqualityComparer<ThisTypeInfo>
      {

         #region IEqualityComparer<Type> Members

         public Boolean Equals( ThisTypeInfo x, ThisTypeInfo y )
         {
            return Object.Equals( x.resolvedTargetType.GetGenericDefinitionIfContainsGenericParameters(), y.resolvedTargetType.GetGenericDefinitionIfContainsGenericParameters() );
         }

         public Int32 GetHashCode( ThisTypeInfo obj )
         {
            return obj.resolvedTargetType.GetGenericDefinitionIfContainsGenericParameters().GetHashCodeSafe();
         }

         #endregion
      }

      private class ThisTypeInfo
      {
         internal readonly Type resolvedTargetType;
         internal readonly Type resolvedDeclaringType;
         internal readonly AbstractInjectableModel model;
         internal Type resolvedTargetTypeFromModel;

         internal ThisTypeInfo( AbstractInjectableModel aModel )
         {
            this.model = aModel;
            this.resolvedTargetType = aModel.TargetType;
            this.resolvedDeclaringType = aModel.DeclaringType;
         }
      }

      private static readonly IEqualityComparer<ThisTypeInfo> INJECTABLE_MODEL_EQ_COMPARER = new InjectableModelTargetTypeComparer();
      private static readonly IEqualityComparer<ThisTypeInfo> GDEF_EQ_COMPARER = new GDefComparer();
      //      private static Type[] EMPTY_TYPES = new Type[0];

      private readonly ListQuery<Type> _publicCompositeGenericArguments;
      private readonly DictionaryQuery<Type, TypeBindingInformation> _privateCompositeTypeInfos;
      private readonly DictionaryQuery<Type, TypeBindingInformation> _fragmentTypeInfos;
      private readonly DictionaryQuery<Type, TypeBindingInformation> _concernInvocationTypeInfos;
      private readonly DictionaryQuery<Type, TypeBindingInformation> _sideEffectInvocationTypeInfos;

      public CompositeTypeModelImpl( CompositeModel compositeModel, CompositeValidationResultMutable vResult )
      {
         var collectionsFactory = compositeModel.ApplicationModel.CollectionsFactory;
         this._publicCompositeGenericArguments = collectionsFactory.NewListProxy( GetCompositeModelPublicTypeGenericArguments( compositeModel ).ToList() ).CQ;

         var injThisTypes = compositeModel.GetAllInjectableModelsWithInjectionScope<ThisAttribute>()
            .Select( inj => new ThisTypeInfo( inj ) )
            .ToArray();
         var thisTypesInModel = compositeModel.Methods
            .Select( m => m.NativeInfo.DeclaringType )
            .Concat( compositeModel.PublicTypes )
            .Concat( injThisTypes.Select( inj => inj.resolvedTargetType ) )
            .Concat( compositeModel.GetAllFragmentTypes().SelectMany( ft => ft.GetAllParentTypes( false ) ) )
            .GetBottomTypes();

         foreach ( var injThisType in injThisTypes )
         {
            injThisType.resolvedTargetTypeFromModel = thisTypesInModel.First( tType => injThisType.resolvedTargetType.GetGenericDefinitionIfContainsGenericParameters().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( tType ) );
         }

         var thisTypesToProcess = new HashSet<ThisTypeInfo>( injThisTypes.GetBottomTypes( model => model.resolvedTargetType ).Distinct( INJECTABLE_MODEL_EQ_COMPARER ) );

         thisTypesToProcess.ExceptWith( thisTypesToProcess.Where( type => compositeModel.PublicTypes.Any( role => type.resolvedTargetType.GetGenericDefinitionIfGenericType().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( role ) ) ).ToArray() );

         // TODO check that if type binding has more than one list with indirect bindings -> throw or add error.

         var privateComposites = new Dictionary<Type, TypeBindingInformationState>();
         while ( thisTypesToProcess.Any() )
         {
            var processedTypes = new HashSet<ThisTypeInfo>( GDEF_EQ_COMPARER );
            foreach ( var thisType in thisTypesToProcess )
            {
               if ( this.ModifyTypeBindingInfoDictionary( privateComposites, this.NewTypeBindingInformation( null, collectionsFactory, compositeModel, null, thisType.resolvedTargetTypeFromModel, thisType.resolvedTargetType, GetBottommostDeclaringType( thisType.model ), privateComposites ) ) )
               {
                  processedTypes.Add( thisType );
               }
            }
            thisTypesToProcess.ExceptWith( thisTypesToProcess.Where( t => processedTypes.Any( p => t.resolvedTargetType.GetGenericDefinitionIfContainsGenericParameters().Equals( p.resolvedTargetType.GetGenericDefinitionIfContainsGenericParameters() ) ) ).ToArray() );
            if ( processedTypes.Count == 0 )
            {
               // We are stuck
               vResult.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Could not deduce generic bindings for types " + String.Join( ", ", thisTypesToProcess.Select( ttp => ttp.resolvedDeclaringType ) ), compositeModel ) );
               break;
            }
            else
            {
               processedTypes.Clear();
            }
         }

         var fragmentTypeInfos = new Dictionary<Type, TypeBindingInformationState>();

         foreach ( var fragmentType in compositeModel.GetAllFragmentTypes() )
         {
            // TODO check - if any parent is public or private composite type (genericdefifgenerictype), error
            if ( fragmentType.IsGenericType() )
            {
               var it = FindImplementingTypeFrom( fragmentType, compositeModel.PublicTypes );
               if ( it != null || !fragmentType.ContainsGenericParameters() )
               {
                  this.ModifyTypeBindingInfoDictionary( fragmentTypeInfos, this.NewTypeBindingInformation( vResult, collectionsFactory, compositeModel, null, fragmentType, fragmentType, fragmentType, privateComposites ) );
               }
               else
               {
                  // Private composite fragment
                  // Process all base types + injectable models of this fragment, and all their generic arguments as DFS
                  var dependentStates = Enumerable.Repeat<Tuple<Int32, TypeBindingInformationState>>( null, fragmentType.GetGenericArguments().Length ).ToList();

                  foreach ( var typeToProcess in fragmentType.GetAllParentTypes( false )
                     .Concat(
                        compositeModel.GetAllInjectableModels()
                        .Where( inj => fragmentType.GetClassHierarchy().Contains( inj.DeclaringType ) )
                        .Select( inj => inj.TargetType )
                     ) )
                  {
                     if ( dependentStates.Any( item => item == null ) )
                     {
                        var gArgs = typeToProcess.GetGenericArguments();
                        for ( var i = 0; i < gArgs.Length; ++i )
                        {
                           foreach ( var typeOrGArg in gArgs[i].AsDepthFirstEnumerable( t => t.GetGenericArgumentsSafe() ) )
                           {
                              if ( typeOrGArg.IsGenericParameter && dependentStates[typeOrGArg.GenericParameterPosition] == null )
                              {
                                 TypeBindingInformationState other;
                                 if ( !privateComposites.TryGetValue( typeToProcess.GetGenericDefinitionIfContainsGenericParameters(), out other )
                                    && compositeModel.PublicTypes.Contains( typeToProcess )
                                    )
                                 {
                                    other = new TypeBindingInformationState( collectionsFactory );
                                    other.NativeInfo = typeToProcess;
                                 }
                                 if ( other != null )
                                 {
                                    dependentStates[typeOrGArg.GenericParameterPosition] = Tuple.Create( i, other );
                                 }
                              }
                           }
                        }
                     }
                     else
                     {
                        break;
                     }
                  }
                  if ( !dependentStates.Any( item => item == null ) )
                  {
                     var state = new TypeBindingInformationState( collectionsFactory );
                     state.NativeInfo = fragmentType;
                     var max = Math.Max( 1, dependentStates.Max( dState => dState.Item2.Bindings.CQ.Count ) );
                     if ( dependentStates.Select( dState => dState.Item2.Bindings.CQ.Count ).Where( c => c > 1 ).Distinct().Count() > 1 )
                     {
                        throw new NotImplementedException( "Very complex private type binding is not yet supported." );
                     }
                     else
                     {
                        for ( var i = 0; i < max; ++i )
                        {
                           state.Bindings.Add( collectionsFactory.NewListProxy( dependentStates.Select( dState =>
                           {
                              AbstractGenericTypeBinding gtb;
                              if ( dState.Item2.Bindings.CQ.Any() )
                              {
                                 // Private composite binding
                                 gtb = dState.Item2.Bindings.CQ[i].CQ[dState.Item1];
                              }
                              else
                              {
                                 var iState = new IndirectGenericTypeBindingState();
                                 iState.GenericDefinition = dState.Item2.NativeInfo.GetGenericDefinitionIfGenericType();
                                 iState.GenericIndex = dState.Item1;
                                 gtb = new IndirectGenericTypeBindingImpl( iState );
                              }
                              return gtb;
                           } ).ToList() ) );
                        }
                        this.ModifyTypeBindingInfoDictionary( fragmentTypeInfos, state );
                     }
                  }
               }
            }
            else
            {
               var state = new TypeBindingInformationState( collectionsFactory );
               state.NativeInfo = fragmentType;
               this.ModifyTypeBindingInfoDictionary( fragmentTypeInfos, state );
            }
         }

         this._fragmentTypeInfos = collectionsFactory.NewDictionaryProxy( fragmentTypeInfos.ToDictionary( kvp => kvp.Key, kvp => (TypeBindingInformation) new TypeBindingInformationImpl( kvp.Value ) ) ).CQ;
         this._concernInvocationTypeInfos = collectionsFactory.NewDictionaryProxy( this.CreateInvocationInfos<ConcernForAttribute>( vResult, collectionsFactory, compositeModel, privateComposites ).ToDictionary( kvp => kvp.Key, kvp => (TypeBindingInformation) new TypeBindingInformationImpl( kvp.Value ) ) ).CQ;
         this._sideEffectInvocationTypeInfos = collectionsFactory.NewDictionaryProxy( this.CreateInvocationInfos<SideEffectForAttribute>( vResult, collectionsFactory, compositeModel, privateComposites ).ToDictionary( kvp => kvp.Key, kvp => (TypeBindingInformation) new TypeBindingInformationImpl( kvp.Value ) ) ).CQ;
         this._privateCompositeTypeInfos = collectionsFactory.NewDictionaryProxy( privateComposites.ToDictionary( kvp => kvp.Key, kvp => (TypeBindingInformation) new TypeBindingInformationImpl( kvp.Value ) ) ).CQ;
      }

      private static IEnumerable<Type> GetCompositeModelPublicTypeGenericArguments( CompositeModel model )
      {
         return model.PublicTypes
            .Where( pType => pType.ContainsGenericParameters() )
            .SelectMany( pType => pType.GetGenericArguments() );
      }

      #region Helper methods

      private Boolean ModifyTypeBindingInfoDictionary(
         IDictionary<Type, TypeBindingInformationState> dic,
         TypeBindingInformationState newState
         )
      {
         Boolean result = newState != null;
         if ( result )
         {
            Type key = newState.NativeInfo;
            if ( dic.ContainsKey( key ) )
            {
               var bindings = dic[key].Bindings;
               foreach ( var binding in newState.Bindings.CQ )
               {
                  if ( !bindings.CQ.Any( iBinding => iBinding.CQ.SequenceEqual( binding.CQ ) ) )
                  {
                     bindings.Add( binding );
                  }
               }
            }
            else
            {
               dic.Add( key, newState );
            }
         }
         return result;
      }

      private IDictionary<Type, TypeBindingInformationState> CreateInvocationInfos<AttributeType>(
         CompositeValidationResultMutable vResult,
         CollectionsFactory collectionsFactory,
         CompositeModel compositeModel,
         IDictionary<Type, TypeBindingInformationState> privateComposites
         )
         where AttributeType : Attribute
      {
         var injModels = compositeModel.GetAllInjectableModelsWithInjectionScope<AttributeType>();
         var invocationTypes = injModels.GetBottomTypes( model => model.TargetType );
         var invocationInfos = new Dictionary<Type, TypeBindingInformationState>();
         foreach ( var model in invocationTypes )
         {
            var actualType = model.TargetType;
            this.ModifyTypeBindingInfoDictionary( invocationInfos, this.NewTypeBindingInformation( vResult, collectionsFactory, compositeModel, model, actualType, actualType, GetBottommostDeclaringType( model ), privateComposites ) );
         }

         return invocationInfos;
      }

      private Boolean ProcessGenericTypeBindingFromPublicComposite(
         CompositeModel cModel,
         CollectionsFactory cf,
         Tuple<Type, Type> pcType,
         Type type,
         out ListProxy<AbstractGenericTypeBinding> result
         )
      {
         var retyrn = type.IsGenericParameter;
         if ( retyrn )
         {
            result = cf.NewListProxy( new List<AbstractGenericTypeBinding>() );
         }
         else
         {
            var array = type.GetGenericArguments().Select( t =>
            {
               AbstractGenericTypeBinding gtb = null;
               var gIdx = Array.IndexOf( pcType.Item1.GetGenericArguments(), t );
               var retyrn2 = !t.IsGenericParameter || gIdx != -1;
               if ( retyrn2 )
               {
                  if ( t.IsGenericParameter )
                  {
                     var state = new IndirectGenericTypeBindingState();
                     state.GenericDefinition = cModel.PublicTypes.First( pt => pt.GetAllParentTypes().Select( ptt => ptt.GetGenericDefinitionIfGenericType() ).Any( ptt => ptt.Equals( pcType.Item2.GetGenericDefinitionIfGenericType() ) ) );
                     state.GenericIndex = gIdx;
                     gtb = new IndirectGenericTypeBindingImpl( state );
                  }
                  else
                  {
                     var state = new DirectGenericTypeBindingState();
                     state.Type = t;
                     retyrn2 = this.ProcessGenericTypeBindingFromPublicComposite( cModel, cf, pcType, t, out state._innerBindings );
                     gtb = new DirectGenericTypeBindingImpl( state );
                  }
               }
               else
               {

               }
               return Tuple.Create( retyrn2, gtb );
            } ).ToArray();
            retyrn = array.Count( tuple => tuple.Item1 ) == type.GetGenericArguments().Length;
            if ( retyrn )
            {
               result = cf.NewListProxy( array.Select( a => a.Item2 ).ToList() );
            }
            else
            {
               result = null;
            }
         }
         return retyrn;
      }

      private Boolean ProcessGenericTypeBindingFromPrivateComposite(
         CompositeModel cModel,
         CollectionsFactory cf,
         Type pcType,
         Type type,
         ListQuery<AbstractGenericTypeBinding> currentPrivateCompositeBindings,
         out ListProxy<AbstractGenericTypeBinding> result
         )
      {
         var retyrn = type.IsGenericParameter;
         if ( retyrn )
         {
            result = cf.NewListProxy( new List<AbstractGenericTypeBinding>() );
         }
         else
         {
            var array = type.GetGenericArguments().Select( t =>
            {
               AbstractGenericTypeBinding gtb = null;
               Boolean retyrn2;
               if ( t.IsGenericParameter )
               {
                  var stk = new Stack<Type>();
                  retyrn2 = false;
                  // Use DFS to search generic arguments and find binding to public composite
                  // TODO replace this code with .AsDepthFirstIEnumerable()
                  var gArgs = pcType.GetGenericArguments();
                  for ( var i = 0; i < gArgs.Length; ++i )
                  {
                     stk.Push( gArgs[i] );
                     while ( !retyrn2 && stk.Any() )
                     {
                        var current = stk.Pop();
                        if ( current.IsGenericParameter )
                        {
                           if ( t == current )
                           {
                              // We've found the correct one
                              gtb = currentPrivateCompositeBindings[i];
                              retyrn2 = true;
                           }
                        }
                        else if ( current.IsGenericType() )
                        {
                           foreach ( var ig in current.GetGenericArguments() )
                           {
                              stk.Push( ig );
                           }
                        }
                     }
                     if ( retyrn2 )
                     {
                        break;
                     }
                  }
               }
               else
               {
                  var state = new DirectGenericTypeBindingState();
                  state.Type = t;
                  retyrn2 = this.ProcessGenericTypeBindingFromPrivateComposite( cModel, cf, pcType, t, currentPrivateCompositeBindings, out state._innerBindings );
                  gtb = new DirectGenericTypeBindingImpl( state );
               }
               return Tuple.Create( retyrn2, gtb );
            } ).ToArray();
            retyrn = array.Count( tuple => tuple.Item1 ) == type.GetGenericArguments().Length;
            if ( retyrn )
            {
               result = cf.NewListProxy( array.Select( a => a.Item2 ).ToList() );
            }
            else
            {
               result = null;
            }
         }
         return retyrn;
      }

      private Boolean NewGenericTypeBinding(
         CollectionsFactory collectionsFactory,
         CompositeModel compositeModel,
         Type memberType,
         Type memberTypeDeclaringType,
         ListProxy<ListProxy<AbstractGenericTypeBinding>> currentBindings,
         IDictionary<Type, TypeBindingInformationState> privateComposites
         )
      {
         var result = !memberType.IsGenericType();
         if ( !result )
         {
            result = !memberType.IsGenericParameter;
            if ( result )
            {
               // First, try to find from public types
               var pt = FindImplementingTypeFrom( memberTypeDeclaringType, compositeModel.PublicTypes );
               result = pt != null;
               ListProxy<AbstractGenericTypeBinding> list;
               if ( result )
               {
                  if ( this.ProcessGenericTypeBindingFromPublicComposite( compositeModel, collectionsFactory, pt, memberType, out list ) )
                  {
                     currentBindings.Add( list );
                  }
               }
               else
               {
                  result = !memberType.ContainsGenericParameters();
                  if ( result )
                  {
                     result = this.ProcessGenericTypeBindingFromPublicComposite( compositeModel, collectionsFactory, Tuple.Create<Type, Type>( memberTypeDeclaringType, null ), memberType, out list );
                     if ( result )
                     {
                        currentBindings.Add( list );
                     }
                  }
                  else
                  {
                     // Otherwise, try to find from existing private composite bindings
                     pt = FindImplementingTypeFrom( memberTypeDeclaringType, privateComposites.Keys );
                     result = pt != null;
                     if ( result )
                     {
                        var eBindings = privateComposites[pt.Item2].Bindings.CQ;
                        var array = eBindings.Select( b =>
                        {
                           var success = this.ProcessGenericTypeBindingFromPrivateComposite( compositeModel, collectionsFactory, pt.Item1, memberType, b.CQ, out list );
                           return Tuple.Create( success, list );
                        } ).ToArray();
                        if ( array.Count( tuple => tuple.Item1 ) == eBindings.Count )
                        {
                           currentBindings.AddRange( array.Select( tuple => tuple.Item2 ) );
                        }
                     }
                  }
               }
            }
         }
         return result;
      }

      private static Tuple<Type, Type> FindImplementingTypeFrom(
         Type type,
         IEnumerable<Type> typesToSearch
         )
      {
         return type.GetAllParentTypes()
                .Select( t => Tuple.Create( t, typesToSearch.SelectMany( tt => tt.GetAllParentTypes() ).FirstOrDefault( tt => Types.AreStructurallySame( t, tt, true ) ) ) )
                .Where( tuple => tuple.Item2 != null )
                .GetBottomTypes( tuple => tuple.Item1 )
                .FirstOrDefault();
      }

      private static Boolean ContainsGenericArgWithIndex( Type gArg, Int32 position )
      {
         Boolean result = false;
         if ( gArg.IsGenericParameter )
         {
            result = gArg
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
.DeclaringMethod == null && gArg.GenericParameterPosition == position;
         }
         else
         {
            result = gArg.GetGenericArguments().Any( inner => ContainsGenericArgWithIndex( inner, position ) );
         }

         return result;
      }

      private TypeBindingInformationState NewTypeBindingInformation(
         CompositeValidationResultMutable vResult,
         CollectionsFactory collectionsFactory,
         CompositeModel compositeModel,
         AbstractInjectableModel injModel,
         Type nInfo,
         Type type,
         Type declaringType,
         IDictionary<Type, TypeBindingInformationState> privateComposites
         )
      {
         TypeBindingInformationState state = new TypeBindingInformationState( collectionsFactory );
         state.NativeInfo = nInfo.GetGenericDefinitionIfContainsGenericParameters();

         ListProxy<ListProxy<AbstractGenericTypeBinding>> bindings = collectionsFactory.NewListProxy<ListProxy<AbstractGenericTypeBinding>>();
         Boolean allBindingsOK = this.NewGenericTypeBinding( collectionsFactory, compositeModel, type, declaringType, bindings, privateComposites );
         if ( allBindingsOK )
         {
            state.Bindings.AddRange( bindings.CQ );
         }
         else if ( vResult != null )
         {
            vResult.StructureValidationErrors.Add( ValidationErrorFactory.NewStructureError( "Failed to deduce generic bindings for " + ( injModel == null ? (Object) type : injModel ) + ".", compositeModel, injModel as AbstractMemberInfoModel<Object> ) );
         }
         return allBindingsOK ? state : null;
      }

      private static Type GetBottommostDeclaringType( AbstractInjectableModel iModel )
      {
         var useWholeHierarchy = iModel is ParameterModel;
         //if (!useFragments)
         //{
         //   var owner = ((ParameterModel)iModel).Owner ;
         //   useFragments = owner is SpecialMethodModel || owner is ConstructorModel;
         //}

         //return (useFragments ?
         //   iModel.CompositeModel.GetAllFragmentTypes() :
         //   iModel.CompositeModel.GetAllCompositeTypes() )
         //   .First( f => f.GetClassHierarchy().Contains( iModel.DeclaringType ) );

         return iModel.CompositeModel
            .GetAllFragmentTypes()
            .First( f =>
               ( useWholeHierarchy ? f.GetAllParentTypes() : f.GetClassHierarchy() )
               .Contains( iModel.DeclaringType )
               );
      }

      #endregion

      #region CompositeTypeModel Members

      public ListQuery<Type> PublicCompositeGenericArguments
      {
         get
         {
            return this._publicCompositeGenericArguments;
         }
      }

      public DictionaryQuery<Type, TypeBindingInformation> FragmentTypeInfos
      {
         get
         {
            return this._fragmentTypeInfos;
         }
      }

      public DictionaryQuery<Type, TypeBindingInformation> ConcernInvocationTypeInfos
      {
         get
         {
            return this._concernInvocationTypeInfos;
         }
      }

      public DictionaryQuery<Type, TypeBindingInformation> SideEffectInvocationTypeInfos
      {
         get
         {
            return this._sideEffectInvocationTypeInfos;
         }
      }

      public DictionaryQuery<Type, TypeBindingInformation> PrivateCompositeTypeInfos
      {
         get
         {
            return this._privateCompositeTypeInfos;
         }
      }

      #endregion
   }

   public interface AbstractGenericTypeBinding
   {
   }

   public abstract class AbstractGenericTypeBindingImpl : AbstractGenericTypeBinding
   {
      //      private readonly AbstractGenericTypeBindingState _state;

      public AbstractGenericTypeBindingImpl( AbstractGenericTypeBindingState state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         //         this._state = state;
      }

      public override abstract bool Equals( object obj );
      public override abstract int GetHashCode();
   }

   public class AbstractGenericTypeBindingState
   {

   }

   public interface DirectGenericTypeBinding : AbstractGenericTypeBinding
   {
      ListQuery<AbstractGenericTypeBinding> InnerBindings { get; }
      Type TypeOrGenericDefinition { get; }
   }

   public class DirectGenericTypeBindingImpl : AbstractGenericTypeBindingImpl, DirectGenericTypeBinding
   {
      private readonly DirectGenericTypeBindingState _state;

      public DirectGenericTypeBindingImpl( DirectGenericTypeBindingState state )
         : base( state )
      {
         this._state = state;
      }

      #region DirectGenericTypeBindingNew Members

      public ListQuery<AbstractGenericTypeBinding> InnerBindings
      {
         get
         {
            return this._state.InnerBindings.CQ;
         }
      }

      public Type TypeOrGenericDefinition
      {
         get
         {
            return this._state.Type;
         }
      }

      #endregion

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) ||
            ( obj is DirectGenericTypeBinding &&
             Object.Equals( this._state.Type, ( (DirectGenericTypeBinding) obj ).TypeOrGenericDefinition ) &&
             Object.Equals( this.InnerBindings, ( (DirectGenericTypeBinding) obj ).InnerBindings )
            );
      }

      public override Int32 GetHashCode()
      {
         return this._state.Type.GetHashCodeSafe();
      }
   }

   public class DirectGenericTypeBindingState : AbstractGenericTypeBindingState
   {
      internal ListProxy<AbstractGenericTypeBinding> _innerBindings;
      private Type _type;

      public DirectGenericTypeBindingState()
      {
      }

      public Type Type
      {
         get
         {
            return this._type;
         }
         set
         {
            this._type = value;
         }
      }

      public ListProxy<AbstractGenericTypeBinding> InnerBindings
      {
         get
         {
            return this._innerBindings;
         }
      }
   }

   public interface IndirectGenericTypeBinding : AbstractGenericTypeBinding
   {
      Int32 GenericIndex { get; }
      Type GenericDefinition { get; }
   }

   public class IndirectGenericTypeBindingImpl : AbstractGenericTypeBindingImpl, IndirectGenericTypeBinding
   {
      private readonly IndirectGenericTypeBindingState _state;

      public IndirectGenericTypeBindingImpl( IndirectGenericTypeBindingState state )
         : base( state )
      {
         this._state = state;
      }

      #region IndirectGenericTypeBinding Members

      public Int32 GenericIndex
      {
         get
         {
            return this._state.GenericIndex;
         }
      }

      public Type GenericDefinition
      {
         get
         {
            return this._state.GenericDefinition;
         }
      }

      #endregion

      public override Boolean Equals( Object obj )
      {
         return Object.ReferenceEquals( this, obj ) ||
            ( obj is IndirectGenericTypeBinding &&
              this._state.GenericIndex.Equals( ( (IndirectGenericTypeBinding) obj ).GenericIndex )
            );
      }

      public override Int32 GetHashCode()
      {
         return this._state.GenericIndex.GetHashCode();
      }
   }

   public class IndirectGenericTypeBindingState : AbstractGenericTypeBindingState
   {
      private Int32 _genericIndex;
      private Type _gDef;

      public IndirectGenericTypeBindingState()
      {
         this._genericIndex = -1;
      }

      public Int32 GenericIndex
      {
         get
         {
            return this._genericIndex;
         }
         set
         {
            this._genericIndex = value;
         }
      }

      public Type GenericDefinition
      {
         get
         {
            return this._gDef;
         }
         set
         {
            this._gDef = value;
         }
      }
   }

   public interface TypeBindingInformation : AbstractMemberInfoModel<Type>
   {
      ListQuery<ListQuery<AbstractGenericTypeBinding>> GenericBindings { get; }
   }

   public class TypeBindingInformationImpl : AbstractMemberInfoModelImmutable<Type>, TypeBindingInformation
   {

      private readonly TypeBindingInformationState _state;

      public TypeBindingInformationImpl( TypeBindingInformationState state )
         : base( state )
      {
         this._state = state;
         state.NativeInfo = state.NativeInfo.GetGenericDefinitionIfGenericType();
      }

      #region TypeBindingInformation Members

      public ListQuery<ListQuery<AbstractGenericTypeBinding>> GenericBindings
      {
         get
         {
            return this._state.Bindings.MQ.IQ;
         }
      }

      #endregion
   }

   public class TypeBindingInformationState : AbstractMemberInfoModelState<Type>
   {
      private ListWithRoles<ListProxy<AbstractGenericTypeBinding>, ListProxyQuery<AbstractGenericTypeBinding>, ListQuery<AbstractGenericTypeBinding>> _bindings;

      public TypeBindingInformationState( CollectionsFactory collectionsFactory )
      {
         this._bindings = collectionsFactory.NewList<ListProxy<AbstractGenericTypeBinding>, ListProxyQuery<AbstractGenericTypeBinding>, ListQuery<AbstractGenericTypeBinding>>();
      }

      public ListWithRoles<ListProxy<AbstractGenericTypeBinding>, ListProxyQuery<AbstractGenericTypeBinding>, ListQuery<AbstractGenericTypeBinding>> Bindings
      {
         get
         {
            return this._bindings;
         }
      }
   }
}
