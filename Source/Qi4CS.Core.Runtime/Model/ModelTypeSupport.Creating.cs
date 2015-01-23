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
using CollectionsWithRoles.API;
using CommonUtils;
using Qi4CS.Core.Runtime.Assembling;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;
using System.Reflection;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.SPI.Common;
using Qi4CS.Core.Runtime.Instance;


namespace Qi4CS.Core.Runtime.Model
{
   public abstract partial class AbstractModelTypeModelScopeSupportBase
   {
      protected class MethodInfoComparerGenericLast : IComparer<Tuple<Type, MethodInfo>>
      {
         private readonly Type _genericType;

         public MethodInfoComparerGenericLast( Type genericType )
         {
            this._genericType = genericType;
         }

         #region IComparer<MethodInfo> Members

         public Int32 Compare( Tuple<Type, MethodInfo> first, Tuple<Type, MethodInfo> second )
         {
            var isFirstGeneric = this._genericType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( first.Item2.DeclaringType );
            var isSecondGeneric = this._genericType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( second.Item2.DeclaringType );

            Int32 result;
            if ( isFirstGeneric.Equals( isSecondGeneric ) )
            {
               result = 0;
            }
            else if ( isFirstGeneric && !isSecondGeneric )
            {
               result = 1;
            }
            else
            {
               result = -1;
            }
            return result;
         }

         #endregion
      }

      protected class PredictableMethodOrderComparer<T> : IComparer<T>
      {

         private readonly Func<T, MemberInfo> _methodExtractor;

         public PredictableMethodOrderComparer( Func<T, MemberInfo> methodExtractor )
         {
            this._methodExtractor = methodExtractor;
         }

         #region IComparer<MethodInfo> Members

         public Int32 Compare( T t1, T t2 )
         {
            var x = this._methodExtractor( t1 );
            var y = this._methodExtractor( t2 );

            var result = StringComparer.Ordinal.Compare( x.Name, y.Name );
            if ( result == 0 )
            {
               result = StringComparer.Ordinal.Compare( x.DeclaringType.FullName, y.DeclaringType.FullName );
               if ( result == 0 )
               {
                  if ( x is MethodBase && y is MethodBase )
                  {
                     result = x is MethodInfo ? StringComparer.Ordinal.Compare( ( (MethodInfo) x ).ReturnType.FullName, ( (MethodInfo) y ).ReturnType.FullName ) : 0;
                     if ( result == 0 && x is MethodInfo && y is MethodInfo )
                     {
                        var p1 = ( (MethodInfo) x ).GetParameters();
                        var p2 = ( (MethodInfo) y ).GetParameters();
                        result = p1.Length < p2.Length ? -1 : ( p1.Length > p2.Length ? 1 : 0 );
                        if ( result == 0 )
                        {
                           result = p1
                              .Select( ( p, idx ) => StringComparer.Ordinal.Compare( p.ParameterType.FullName, p2[idx].ParameterType.FullName ) )
                              .Where( r => r != 0 )
                              .FirstOrDefault();
                        }
                     }
                  }
                  else if ( x is FieldInfo && y is FieldInfo )
                  {
                     result = StringComparer.Ordinal.Compare( ( (FieldInfo) x ).FieldType.FullName, ( (FieldInfo) y ).FieldType.FullName );
                  }
               }
            }
            return result;
         }

         #endregion
      }


      protected class AttributeEqualityComparer : IEqualityComparer<Object>
      {

         #region IEqualityComparer<object> Members

         Boolean IEqualityComparer<Object>.Equals( Object x, Object y )
         {
            return Object.ReferenceEquals( x, y ) || ( x != null && y != null && x.GetType().Equals( y.GetType() ) && this.CheckAttribute( x, y ) );
         }

         Int32 IEqualityComparer<Object>.GetHashCode( Object obj )
         {
            return obj == null ? 0 : obj.GetHashCode();
         }

         #endregion

         protected Boolean CheckAttribute( Object x, Object y )
         {
            Type current = x.GetType();
            Boolean result = true;
            while ( result && current != null )
            {
               foreach ( var field in current.GetAllInstanceFields() )
               {
                  Object xVal = field.GetValue( x );
                  Object yVal = field.GetValue( y );
                  result = Object.Equals( xVal, yVal ) || ( xVal is Attribute && ( (IEqualityComparer<Object>) this ).Equals( xVal, yVal ) );
                  if ( !result )
                  {
                     break;
                  }
               }
               current = current.GetBaseType();
            }
            return result;
         }
      }

      private static readonly MethodInfoComparerGenericLast MIXIN_COMPARER = new MethodInfoComparerGenericLast( typeof( GenericInvocator ) );
      private static readonly IComparer<CompositeMethodModelMutable> COMPOSITE_METHOD_COMPARER = new PredictableMethodOrderComparer<CompositeMethodModelMutable>( model => model.NativeInfo );
      private static readonly IComparer<ConstructorModelMutable> COMPOSITE_CTOR_COMPARER = new PredictableMethodOrderComparer<ConstructorModelMutable>( model => model.NativeInfo );
      private static readonly IComparer<SpecialMethodModelMutable> COMPOSITE_SPECIAL_METHOD_COMPARER = new PredictableMethodOrderComparer<SpecialMethodModelMutable>( model => model.NativeInfo );
      private static readonly IComparer<FieldModelMutable> COMPOSITE_FIELD_COMPARER = new PredictableMethodOrderComparer<FieldModelMutable>( model => model.NativeInfo );

      private static readonly AttributeEqualityComparer ATTRIBUTE_EQUALITY_COMPARER = new AttributeEqualityComparer();
      private const Int32 RETURN_PARAMETER_POSITION = -1;

      private static readonly MethodInfo GENERIC_FRAGMENT_METHOD = ArchitectureDefaults.GENERIC_FRAGMENT_TYPE.LoadMethodOrThrow( "Invoke", null );

      private static readonly Func<Type, IEnumerable<Object>> TYPE_ATTRIBUTES_EXTRACTOR = type => type.GetCustomAttributes( true );
      private static readonly Func<FieldInfo, IEnumerable<Object>> FIELD_INFO_ATTRIBUTES_EXTRACTOR = field => field.GetCustomAttributes( true );
      private static readonly Func<MethodInfo, IEnumerable<Object>> METHOD_INFO_ATTRIBUTES_EXTRACTOR = method => method.GetCustomAttributes( true );
      //      private static readonly Func<ConstructorInfo, Object[]> CTOR_INFO_ATTRIBUTES_EXTRACTOR = ctor => ctor.GetCustomAttributes( true );
      private static readonly Func<ParameterInfo, IEnumerable<Object>> PARAMETER_INFO_ATTRIBUTES_EXTRACTOR = param => param.GetCustomAttributes( true );
      private static readonly Func<PropertyInfo, IEnumerable<Object>> PROPERTY_INFO_ATTRIBUTES_EXTRACTOR = pInfo => pInfo.GetCustomAttributes( true );
      private static readonly Func<EventInfo, IEnumerable<Object>> EVENT_INFO_ATTRIBUTES_EXTRACTOR = eInfo => eInfo.GetCustomAttributes( true );


      public CompositeModel NewCompositeModel( ApplicationModel<ApplicationSPI> appModel, CompositeAssemblyInfo compositeInfo, Func<Int32, Object, Attribute, Attribute> attributeTransformer, String architectureContainerID )
      {
         ArgumentValidator.ValidateNotNull( "Composite info", compositeInfo );

         var cf = appModel.CollectionsFactory;

         CompositeModelState state;
         CompositeModelImmutable resultImmutable;
         CompositeModelMutable result;
         var compositeModelType = compositeInfo.CompositeModelType;

         this.CreateCompositeModelObjects( compositeInfo, appModel.CollectionsFactory, out state, out resultImmutable, out result );
         state.MainCodeGenerationType = compositeInfo.MainCodeGenerationType;
         state.ModelType = compositeModelType;
         state.MetaInfoContainer = compositeInfo.MetaInfoContainer;
         state.ApplicationModel = appModel;
         state.CompositeModelID = compositeInfo.CompositeID;

         var propertyCache = new Dictionary<MethodInfo, PropertyInfo>();
         var propertyModelCache = new Dictionary<PropertyInfo, PropertyModelMutable>();

         var eventCache = new Dictionary<MethodInfo, EventInfo>();
         var eventModelCache = new Dictionary<EventInfo, EventModelMutable>();

         var processedCompositeTypes = new HashSet<Type>();

         var processedFragmentTypes = new HashSet<Type>();
         var processedMixins = new Dictionary<Type, Type>();
         var processedConcerns = new Dictionary<Type, Type>();
         var processedSideEffects = new Dictionary<Type, Type>();

         var publicCompositeTypes = new HashSet<Type>( compositeInfo.Types.GetBottomTypes() );
         var unprocessedTypes = new HashSet<Type>( publicCompositeTypes );

         // TODO refactor handling of defaults into building new CompositeAssemblyInfo -objects based on attributes declared on types.
         var defaultFragments = new Dictionary<FragmentModelType, ISet<Type>>();
         defaultFragments.Add( FragmentModelType.Constraint, new HashSet<Type>() );
         //         var defaultAppliesToFilters = new Dictionary<Type, Type>();
         var appliesFiltersCache = new Dictionary<Type, AppliesToFilter>();
         var appliesTypeCache = new Dictionary<Type, ISet<Type>>();
         ISet<MethodInfo> allCompositeMethods = new HashSet<MethodInfo>();

         while ( unprocessedTypes.Any() )
         {
            foreach ( Type currentCompositeTypeIter in unprocessedTypes )
            {
               Type currentCompositeType;
               currentCompositeTypeIter.IsLazy( out currentCompositeType );
               this.AddDefaults( compositeInfo, defaultFragments, currentCompositeType, attributeTransformer );

               var currentMixins = this.CollectFragments( compositeInfo, compositeInfo.GetFragmentAssemblyInfos( FragmentModelType.Mixin ).SelectMany( fragmentInfo => fragmentInfo.TypeInfos.Keys ), currentCompositeType, true, attributeTransformer );
               var currentConcerns = this.CollectFragments( compositeInfo, compositeInfo.GetFragmentAssemblyInfos( FragmentModelType.Concern ).SelectMany( fragmentInfo => fragmentInfo.TypeInfos.Keys ), currentCompositeType, false, attributeTransformer );
               var currentSideEffects = this.CollectFragments( compositeInfo, compositeInfo.GetFragmentAssemblyInfos( FragmentModelType.SideEffect ).SelectMany( fragmentInfo => fragmentInfo.TypeInfos.Keys ), currentCompositeType, false, attributeTransformer );

               ISet<Type> allCurrentFragments = new HashSet<Type>();
               allCurrentFragments.UnionWith( currentMixins.Values );
               allCurrentFragments.UnionWith( currentConcerns.Values );
               allCurrentFragments.UnionWith( currentSideEffects.Values );
               this.AddModelsForFragmentsIfNecessary( cf, result, compositeInfo, defaultFragments, true, allCurrentFragments, processedFragmentTypes, attributeTransformer );
               processedFragmentTypes.UnionWith( allCurrentFragments );

               this.MergeFragmentTypeDics( processedMixins, currentMixins );
               this.MergeFragmentTypeDics( processedConcerns, currentConcerns );
               this.MergeFragmentTypeDics( processedSideEffects, currentSideEffects );

               foreach ( var type in this.GetTypesForComposite( currentCompositeType, publicCompositeTypes.Contains( currentCompositeType ) ? null : new HashSet<Type>( currentMixins.Values.Concat( currentConcerns.Values ).Concat( currentSideEffects.Values ) ) ) )
               {
                  if ( !Object.Equals( typeof( Object ), type ) )
                  {
                     processedCompositeTypes.Add( type );
                     foreach ( var property in type.GetAllDeclaredInstanceProperties() )
                     {
                        var getter = property.GetGetMethod();
                        var setter = property.GetSetMethod();
                        if ( this.ShouldProcessAsCompositeMethod( getter, property, compositeInfo, attributeTransformer ) || this.ShouldProcessAsCompositeMethod( setter, property, compositeInfo, attributeTransformer ) )
                        {
                           if ( getter != null )
                           {
                              propertyCache[getter] = property;
                           }
                           if ( setter != null )
                           {
                              propertyCache[setter] = property;
                           }
                        }
                     }

                     foreach ( var eventInfo in type.GetAllDeclaredInstanceEvents() )
                     {
                        var adder = eventInfo.GetAddMethod();
                        var remover = eventInfo.GetRemoveMethod();
                        if ( this.ShouldProcessAsCompositeMethod( adder, eventInfo, compositeInfo, attributeTransformer ) || this.ShouldProcessAsCompositeMethod( remover, eventInfo, compositeInfo, attributeTransformer ) )
                        {
                           if ( adder != null )
                           {
                              eventCache[adder] = eventInfo;
                           }
                           if ( remover != null )
                           {
                              eventCache[remover] = eventInfo;
                           }
                        }
                     }

                     foreach ( MethodInfo method in type.GetAllDeclaredInstanceMethods() )
                     {
                        if ( this.ShouldProcessAsCompositeMethod( method, null, compositeInfo, attributeTransformer ) && !allCompositeMethods.Contains( method ) )
                        {
                           state.Methods.Add( this.NewCompositeMethodModel( cf, result, compositeInfo, processedMixins, processedConcerns, processedSideEffects, state.Methods.MQ.Count, method, propertyCache, propertyModelCache, eventCache, eventModelCache, defaultFragments[FragmentModelType.Constraint], appliesFiltersCache, appliesTypeCache, attributeTransformer ) );
                           allCompositeMethods.Add( method );
                        }
                     }
                  }
               }
            }
            //processedCompositeTypes.UnionWith( unprocessedTypes );
            ISet<Type> collectedTypes = this.CollectCompositeTypes( compositeInfo, result.IQ, processedCompositeTypes );
            if ( unprocessedTypes.SetEquals( collectedTypes ) )
            {
               throw new InternalException( "Stuck when processing composite types, unprocessed types are: " + String.Join( ", ", unprocessedTypes ) );
            }

            unprocessedTypes.UnionWith( collectedTypes );
            unprocessedTypes.ExceptWith( processedCompositeTypes );

         }

         // Order composite methods & fields in predictable way (many things are based on the order of these, so in order to avoid reflection-dependent order, we have to sort them predictably)
         MakeIndicesPredictable( result.Methods, COMPOSITE_METHOD_COMPARER, ( m, i ) => m.CompositeMethodIndex = i );
         MakeIndicesPredictable( result.Constructors, COMPOSITE_CTOR_COMPARER, ( m, i ) => m.ConstructorIndex = i );
         MakeIndicesPredictable( result.SpecialMethods, COMPOSITE_SPECIAL_METHOD_COMPARER, ( m, i ) => m.SpecialMethodIndex = i );
         MakeIndicesPredictable( result.Fields, COMPOSITE_FIELD_COMPARER, ( f, i ) => f.FieldIndex = i );

         // See if we can replace any of the missing fragments with default ones
         foreach ( CompositeMethodModelMutable cMethod in result.Methods.MQ )
         {
            // The policy for default fragments (still unsure on some things):
            // Default mixin: use if composite method mixin is missing
            // Default constraint: use if any of matching composite method constraint implementations is missing
            // Default concern: use always (if composite method does not have any concerns ?)
            // Default side-effect: use always (if composite method does not have any side-effects?)
            Type compositeMethodDeclaringType = cMethod.NativeInfo.DeclaringType;
            ISet<Type> currentMixins = new HashSet<Type>();
            if ( cMethod.Mixin == null && defaultFragments.ContainsKey( FragmentModelType.Mixin ) )
            {
               cMethod.Mixin = this.FindMixinModel( null, cMethod, this.CollectFragments( compositeInfo, defaultFragments[FragmentModelType.Mixin], compositeMethodDeclaringType, true, attributeTransformer ), appliesFiltersCache, appliesTypeCache, attributeTransformer );
               if ( cMethod.Mixin != null )
               {
                  currentMixins.Add( cMethod.Mixin.NativeInfo.DeclaringType );
               }
            }
            ISet<Type> currentConcerns = new HashSet<Type>();
            if ( defaultFragments.ContainsKey( FragmentModelType.Concern ) )
            {
               cMethod.Concerns.AddRange( this.FindConcernModels( null, cMethod, this.CollectFragments( compositeInfo, defaultFragments[FragmentModelType.Concern], compositeMethodDeclaringType, false, attributeTransformer ), appliesFiltersCache, appliesTypeCache, attributeTransformer ) );
               currentConcerns.UnionWith( cMethod.Concerns.MQ.Select( concern => concern.NativeInfo.DeclaringType ) );
            }
            ISet<Type> currentSideEffects = new HashSet<Type>();
            if ( defaultFragments.ContainsKey( FragmentModelType.SideEffect ) )
            {
               cMethod.SideEffects.AddRange( this.FindSideEffectModels( null, cMethod, this.CollectFragments( compositeInfo, defaultFragments[FragmentModelType.SideEffect], compositeMethodDeclaringType, false, attributeTransformer ), appliesFiltersCache, appliesTypeCache, attributeTransformer ) );
               currentSideEffects.UnionWith( cMethod.SideEffects.MQ.Select( sideEffect => sideEffect.NativeInfo.DeclaringType ) );
            }

            ISet<Type> allCurrentFragments = new HashSet<Type>();
            allCurrentFragments.UnionWith( currentMixins );
            allCurrentFragments.UnionWith( currentConcerns );
            allCurrentFragments.UnionWith( currentSideEffects );
            this.AddModelsForFragmentsIfNecessary( cf, result, compositeInfo, null, false, allCurrentFragments, processedFragmentTypes, attributeTransformer );
            processedFragmentTypes.UnionWith( allCurrentFragments );
            this.ProcessParametersForDefaultConstraints( compositeInfo, Enumerable.Repeat( cMethod.Result, 1 ).Concat( cMethod.Parameters.CQ ), defaultFragments[FragmentModelType.Constraint], attributeTransformer );
         }

         foreach ( ConstructorModelMutable ctorModel in result.Constructors.MQ )
         {
            this.ProcessParametersForDefaultConstraints( compositeInfo, ctorModel.Parameters.MQ, defaultFragments[FragmentModelType.Constraint], attributeTransformer );
         }

         foreach ( var sModel in result.SpecialMethods.MQ )
         {
            this.ProcessParametersForDefaultConstraints( compositeInfo, sModel.Parameters.MQ, defaultFragments[FragmentModelType.Constraint], attributeTransformer );
         }

         // See if we can replace any of the still missing mixins with default property getter/setter
         foreach ( CompositeMethodModelMutable cMethod in result.Methods.MQ )
         {
            PropertyModelMutable pModel = cMethod.PropertyModel;
            if ( pModel != null )
            {
               CompositeMethodModelMutable getter = pModel.GetterMethod;
               CompositeMethodModelMutable setter = pModel.SetterMethod;
               if ( getter != null &&
                    setter != null &&
                    getter.Mixin == null &&
                    setter.Mixin == null &&
                    getter.NativeInfo.GetParameters().Length == 0 &&
                    !Object.Equals( typeof( void ), getter.NativeInfo.ReturnType ) &&
                    setter.NativeInfo.GetParameters().Length == 1
                  )
               {
                  var mixin = appModel.GenericPropertyMixinType;
                  var mixinMethod = this.TryGetSameSignatureOrGeneric( cMethod.NativeInfo, mixin );
                  setter.Mixin = this.NewMixinMethodModel( setter, mixinMethod, mixin );
                  getter.Mixin = this.NewMixinMethodModel( getter, mixinMethod, mixin );
                  this.AddModelsForFragmentsIfNecessary( cf, result, compositeInfo, null, false, new HashSet<Type>( Enumerable.Repeat( mixin, 1 ) ), processedFragmentTypes, attributeTransformer );
               }
            }
            EventModelMutable eModel = cMethod.EventModel;
            if ( eModel != null )
            {
               CompositeMethodModelMutable adder = eModel.AddMethod;
               CompositeMethodModelMutable remover = eModel.RemoveMethod;
               if ( adder != null && remover != null && adder.Mixin == null && remover.Mixin == null )
               {
                  var mixin = appModel.GenericEventMixinType;
                  var mixinMethod = this.TryGetSameSignatureOrGeneric( cMethod.NativeInfo, mixin );
                  adder.Mixin = this.NewMixinMethodModel( adder, mixinMethod, mixin );
                  remover.Mixin = this.NewMixinMethodModel( remover, mixinMethod, mixin );
                  this.AddModelsForFragmentsIfNecessary( cf, result, compositeInfo, null, false, new HashSet<Type>( Enumerable.Repeat( mixin, 1 ) ), processedFragmentTypes, attributeTransformer );
               }
            }
         }

         this.ProcessReflectedInfoModels(
            cf,
            result,
            compositeInfo,
            propertyModelCache.Values,
            model => model.NativeInfo.DeclaringType,
            ( type, model ) => type.GetProperty( model.NativeInfo.Name ),
            PROPERTY_INFO_ATTRIBUTES_EXTRACTOR,
            attributeTransformer
            );
         foreach ( var pModel in propertyModelCache.Values )
         {
            pModel.PropertyIsImmutable = pModel.AllAttributes.CQ.OfType<ImmutableAttribute>().Any()
               || pModel.NativeInfo.DeclaringType.GetAllParentTypes().Any( dType => this.ProcessCustomAttributes( compositeInfo, dType, TYPE_ATTRIBUTES_EXTRACTOR, attributeTransformer ).OfType<ImmutableAttribute>().Any() );
         }

         this.ProcessReflectedInfoModels(
            cf,
            result,
            compositeInfo,
            eventModelCache.Values,
            model => model.NativeInfo.DeclaringType,
            ( type, model ) => type.GetEvent( model.NativeInfo.Name ),
            EVENT_INFO_ATTRIBUTES_EXTRACTOR,
            attributeTransformer
            );

         // Post-process parameters, using all attributes of possible properties
         foreach ( CompositeMethodModelMutable methodModel in result.Methods.MQ.Where( mModel => mModel.PropertyModel != null ) )
         {
            foreach ( ParameterModelMutable paramModel in
               new ParameterModelMutable[] { methodModel.Result }
               .Concat( methodModel.Parameters.CQ )
               .Where( paraModel => !Object.Equals( typeof( void ), paraModel.NativeInfo.ParameterType ) )
               )
            {
               PropertyModelMutable pModel = methodModel.PropertyModel;
               AttributeTargets target = paramModel.NativeInfo.Position >= 0 ? AttributeTargets.Parameter : AttributeTargets.ReturnValue;
               this.ProcessModelWithAttributesState( cf, pModel.AllAttributes.CQ.Where( attr => attr.GetType().GetCustomAttributes( true ).OfType<AttributeUsageAttribute>().All( attr2 => attr2.ValidOn.HasFlag( target ) ) ), paramModel );
               paramModel.ConstraintsMutable.AddRange(
                  this.GetConstraints( compositeInfo, pModel.AllAttributes.CQ, attributeTransformer )
                  .SelectMany( attr => this.NewConstraintModels( compositeInfo, attr, paramModel.NativeInfo.ParameterType, paramModel.IQ, compositeInfo.GetFragmentAssemblyInfo( FragmentModelType.Constraint ).TypeInfos.Keys, defaultFragments[FragmentModelType.Constraint], attributeTransformer ) )
               );
               this.ProcessParametersForDefaultConstraints( compositeInfo, Enumerable.Repeat( paramModel, 1 ), defaultFragments[FragmentModelType.Constraint], attributeTransformer );
               if ( pModel.AllAttributes.CQ.OfType<OptionalAttribute>().Any()
                  || ( ( pModel.NativeInfo.Attributes & PropertyAttributes.HasDefault ) != 0 && pModel.NativeInfo.GetConstantValue() == null )
                  )
               {
                  // Optional property
                  if ( pModel.GetterMethod != null )
                  {
                     pModel.GetterMethod.Result.IsOptional = true;
                  }
                  if ( pModel.SetterMethod != null )
                  {
                     pModel.SetterMethod.Parameters.CQ[0].IsOptional = true;
                  }
               }
            }
         }

         state.PublicTypes.UnionWith( publicCompositeTypes.Select( role => role.GetGenericDefinitionIfContainsGenericParameters() ) );

         this.PostProcessModel( result, compositeInfo, architectureContainerID );

         return result.IQ;
      }

      private static void MakeIndicesPredictable<T, TMQ, TIQ>( ListWithRoles<T, TMQ, TIQ> list, IComparer<T> comparer, Action<T, Int32> indexSetter )
         where T : Mutable<TMQ, TIQ>
         where TMQ : MutableQuery<TIQ>
      {
         var array = list.CQ.ToArray();
         Array.Sort( array, comparer );
         for ( var i = 0; i < array.Length; ++i )
         {
            indexSetter( array[i], i );
         }

         list.Clear();
         list.AddRange( array );
      }

      protected void MergeFragmentTypeDics( IDictionary<Type, Type> processed, IDictionary<Type, Type> current )
      {
         foreach ( var kvp in current )
         {
            processed[kvp.Key] = kvp.Value;
         }
      }

      protected IEnumerable<Type> GetTypesForComposite( Type currentCompositeType, ISet<Type> fragmentTypes )
      {
         IEnumerable<Type> currentCompositeTypes;
         if ( fragmentTypes == null || !fragmentTypes.Any( fType => currentCompositeType.GetGenericDefinitionIfContainsGenericParameters().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( fType ) ) )
         {
            currentCompositeTypes = Enumerable.Repeat( currentCompositeType, 1 );
         }
         else
         {
            currentCompositeTypes = this.ProcessThisTypeForSubTypes( currentCompositeType, fragmentTypes );
         }

         return currentCompositeType.IsInterface() ? currentCompositeTypes.SelectMany( cType => cType.GetImplementedInterfaces() ) : currentCompositeTypes.SelectMany( cType => cType.GetAllParentTypes() );
      }

      protected Boolean ShouldProcessAsCompositeMethod( MethodInfo method, MemberInfo currentEventOrProperty, CompositeAssemblyInfo assInfo, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         var result = method != null;
         if ( result )
         {
            // See if CompositeMethodAttribute is defined for this method or associated event/property
            var compositeMethodAttr = this.ProcessCustomAttributes( assInfo, method, METHOD_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer ).OfType<CompositeMethodAttribute>().FirstOrDefault();
            if ( compositeMethodAttr == null && currentEventOrProperty != null )
            {
               compositeMethodAttr = ( currentEventOrProperty is PropertyInfo ?
                  this.ProcessCustomAttributes( assInfo, (PropertyInfo) currentEventOrProperty, PROPERTY_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer ) :
                  this.ProcessCustomAttributes( assInfo, (EventInfo) currentEventOrProperty, EVENT_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer ) ).OfType<CompositeMethodAttribute>().FirstOrDefault();
            }

            CompositeMethodVisiblity methodVisiblity;
            // If no CompositeMethodAttribute is defined, then see if DefaultCompositeMethodVisibilityAttribute is defined for enclosing type or any of its base types or interfaces
            if ( compositeMethodAttr == null && !method.DeclaringType.IsInterface() )
            {
               // Correct order: base-type chain (interfaces may only contain public methods -> no point using DefaultCompositeMethodVisibilityAttribute for interfaces.
               var compositeMethodVisAttr = method.DeclaringType.GetClassHierarchy()
                  .SelectMany( t => t.GetCustomAttributes( true ).OfType<DefaultCompositeMethodVisibilityAttribute>() )
                  .FirstOrDefault();
               methodVisiblity = compositeMethodVisAttr == null ?
                  CompositeMethodVisiblity.Public :
                  compositeMethodVisAttr.CompositeMethodVisibility;
            }
            else
            {
               // CompositeMethodAttribute is found, so any non-private method is ok
               methodVisiblity = CompositeMethodVisiblity.Public | CompositeMethodVisiblity.Internal | CompositeMethodVisiblity.Protected | CompositeMethodVisiblity.ProtectedAndInternal | CompositeMethodVisiblity.ProtectedOrInternal;
            }
            result = method.DeclaringType.IsInterface() || ( methodVisiblity.MethodConsideredToBeCompositeMethod( method ) && !method.DeclaringType.GetImplementedInterfaces().SelectMany( iFace => iFace.GetPublicDeclaredInstanceMethods() ).Any( iFaceMethod => ReflectionHelper.FindMethodImplicitlyImplementingMethod( method.DeclaringType, iFaceMethod ) == method ) );
         }
         return result;
      }

      protected virtual void AddModelsForFragmentsIfNecessary(
         CollectionsFactory cf,
         CompositeModelMutable compositeModel,
          CompositeAssemblyInfo assInfo,
         IDictionary<FragmentModelType, ISet<Type>> defaultFragments,
         Boolean addDefaults,
         ISet<Type> currentFragments,
         ISet<Type> processedFragments,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         currentFragments.ExceptWith( processedFragments );
         this.AddModelsForFragments( cf, compositeModel, assInfo, defaultFragments, addDefaults, currentFragments, attributeTransformer );
         processedFragments.UnionWith( currentFragments );
      }

      protected virtual void AddModelsForFragments(
         CollectionsFactory cf,
         CompositeModelMutable compositeModel,
          CompositeAssemblyInfo assInfo,
         IDictionary<FragmentModelType, ISet<Type>> defaultFragments,
         Boolean addDefaults,
         IEnumerable<Type> fragments,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         foreach ( Type fragmentType in fragments )
         {
            if ( addDefaults )
            {
               this.AddDefaults( assInfo, defaultFragments, fragmentType, attributeTransformer );
            }
            this.AddConstructors( cf, compositeModel, assInfo, fragmentType, defaultFragments == null ? null : defaultFragments[FragmentModelType.Constraint], attributeTransformer );
            this.AddFields( cf, compositeModel, assInfo, fragmentType, attributeTransformer );
            this.AddSpecialMethods( cf, compositeModel, assInfo, fragmentType, defaultFragments == null ? null : defaultFragments[FragmentModelType.Constraint], attributeTransformer );
         }
      }

      protected virtual void ProcessParametersForDefaultConstraints( CompositeAssemblyInfo assInfo, IEnumerable<ParameterModelMutable> parameters, IEnumerable<Type> defaultConstraints, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         foreach ( ParameterModelMutable pModel in parameters )
         {
            for ( Int32 idx = 0; idx < pModel.ConstraintsMutable.MQ.Count; ++idx )
            {
               ConstraintModelMutable cModel = pModel.ConstraintsMutable.MQ[idx];
               if ( cModel.IQ.ConstraintType == null )
               {
                  pModel.ConstraintsMutable.RemoveAt( idx );
                  ConstraintModelMutable[] constraints = this.NewConstraintModels( assInfo, cModel.IQ.ConstraintAttribute, pModel.NativeInfo.ParameterType, pModel.IQ, defaultConstraints, null, attributeTransformer ).ToArray();
                  Int32 cIdx = 0;
                  while ( cIdx < constraints.Length )
                  {
                     pModel.ConstraintsMutable.Insert( idx, constraints[cIdx] );
                     ++idx;
                     ++cIdx;
                  }
               }
            }
         }
      }

      protected virtual void AddDefaults(
         CompositeAssemblyInfo assInfo,
         IDictionary<FragmentModelType, ISet<Type>> defaultFragments,
         Type currentComposite,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         foreach ( var attr in currentComposite.GetAllParentTypes().SelectMany( type => this.ProcessCustomAttributes( assInfo, type, TYPE_ATTRIBUTES_EXTRACTOR, attributeTransformer ) ).OfType<DefaultFragmentsAttribute>() )
         {
            if ( attr.Fragments != null )
            {
               defaultFragments
                  .GetOrAdd_NotThreadSafe( attr.FragmentModelType, () => new HashSet<Type>() )
                  .UnionWith( attr.Fragments.Where( f => f != null ) );
            }
         }
      }

      protected virtual void AddFields( CollectionsFactory cf, CompositeModelMutable compositeModel, CompositeAssemblyInfo assInfo, Type fragmentType, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         Int32 fieldIndex = compositeModel.Fields.CQ.Count;
         foreach ( var fieldInfo in fragmentType.GetClassHierarchy()
               .Distinct()
               .SelectMany( type => type.GetAllDeclaredInstanceFields() )
               .Where( field => this.GetInjectionScopes( this.ProcessCustomAttributes( assInfo, field, FIELD_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer ) ).Any()
                                && !compositeModel.Fields.CQ.Any( f => f.NativeInfo == field ) ) )
         {
            compositeModel.Fields.Add( this.NewFieldModel( cf, compositeModel, assInfo, fieldIndex, fieldInfo, attributeTransformer ) );
            ++fieldIndex;
         }
      }

      protected virtual void AddConstructors(
         CollectionsFactory cf,
         CompositeModelMutable compositeModel,
         CompositeAssemblyInfo assInfo,
         Type fragmentType,
         ISet<Type> defaultConstraints,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         Int32 ctorIndex = compositeModel.Constructors.MQ.Count;
         foreach ( ConstructorInfo ctorInfo in fragmentType.GetAllInstanceConstructors() )
         {
            compositeModel.Constructors.Add( this.NewConstructorModel( cf, compositeModel, assInfo, ctorInfo, ctorIndex, defaultConstraints, attributeTransformer ) );
            ++ctorIndex;
         }
      }

      protected virtual void AddSpecialMethods(
         CollectionsFactory cf,
         CompositeModelMutable compositeModel,
          CompositeAssemblyInfo assInfo,
         Type fragmentType,
         ISet<Type> defaultConstraints,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         Int32 idx = compositeModel.SpecialMethods.MQ.Count;
         foreach ( KeyValuePair<MethodInfo, Attribute[]> kvp in this.GetSpecialMethods( assInfo, fragmentType, attributeTransformer ) )
         {
            if ( !compositeModel.SpecialMethods.CQ.Any( s => s.NativeInfo == kvp.Key ) )
            {
               compositeModel.SpecialMethods.Add( this.NewSpecialMethodModel( cf, compositeModel, assInfo, kvp.Key, fragmentType, kvp.Value, idx, defaultConstraints, attributeTransformer ) );
               ++idx;
            }
         }
      }

      protected virtual IDictionary<Type, Type> CollectFragments(
         CompositeAssemblyInfo assInfo,
         IEnumerable<Type> declaredFragmentTypes,
         Type currentComposite,
         Boolean addIfNotInterface,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         IDictionary<Type, Type> result = declaredFragmentTypes
               .Where( fragmentType =>
                  ArchitectureDefaults.GENERIC_FRAGMENT_TYPE.IsAssignableFrom( fragmentType ) ||
                   currentComposite.GetAllParentTypes().Any( type => type.GetGenericDefinitionIfGenericType().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( fragmentType ) ) ||
                  ( !fragmentType.ContainsGenericParameters() && this.GetSpecialMethods( assInfo, fragmentType, attributeTransformer ).Any() ) // TODO a bit of performance killer
                  )
               .ToDictionary( fType => fType, fType => this.ProcessFragmentTypeFromAssembly( fType, currentComposite ) );

         // Remove all generic type definitions, for which we have found non-generic-type-definition version
         foreach ( var kvp in result.ToArray() )
         {
            Type type = kvp.Value;
            if ( type.IsGenericTypeDefinition() && result.Values.Any( another => !Object.Equals( another, type ) && another.IsGenericType() && another.GetGenericTypeDefinition().Equals( type ) ) )
            {
               result.Remove( kvp.Key );
            }
         }
         if ( addIfNotInterface && !currentComposite.IsInterface() )
         {
            result[currentComposite] = currentComposite;
         }

         return result;
      }

      // Tries to resolve generic type definitions if possible
      protected virtual Type ProcessFragmentTypeFromAssembly( Type fragmentType, Type currentCompositeType )
      {
         Type result = fragmentType;
         foreach ( var currentComposite in currentCompositeType.GetAllParentTypes() )
         {
            if (
               fragmentType.IsGenericTypeDefinition() &&
               currentComposite.IsGenericType() &&
               !currentComposite.ContainsGenericParameters() &&
                currentComposite.GetGenericTypeDefinition().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( fragmentType )
               )
            {
               var fragmentIFace = fragmentType.GetInterfaces().Single( iFace => iFace.GetGenericDefinitionIfContainsGenericParameters().Equals( currentComposite.GetGenericTypeDefinition() ) );
               var fiGArgs = fragmentIFace.GetGenericArguments();
               var cGArgs = currentComposite.GetGenericArguments();
               var fGArgs = fragmentType.GetGenericArguments();
               for ( var idx = 0; idx < fGArgs.Length; ++idx )
               {
                  var fArg = fGArgs[idx];
                  for ( var idx2 = 0; idx2 < fiGArgs.Length; ++idx2 )
                  {
                     if ( fiGArgs[idx2].Equals( fArg ) )
                     {
                        fArg = cGArgs[idx2];
                        break;
                     }
                  }
                  fGArgs[idx] = fArg;
               }
               result = fragmentType.MakeGenericType( fGArgs );
               break;
            }
         }
         return result;
      }

      protected virtual ISet<Type> CollectCompositeTypes( CompositeAssemblyInfo compositeInfo, CompositeModel compositeModel, ISet<Type> processedTypes )
      {
         return new HashSet<Type>(
            compositeModel.GetAllInjectableModelsWithInjectionScope<ThisAttribute>()
            .Select( model => model.TargetType )
            .Select( cType => cType.GetGenericDefinitionIfContainsGenericParameters() )
            .Where( cType => !processedTypes.Any( processedType => cType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( processedType ) ) )
            .GetBottomTypes()
         );
      }

      protected virtual CompositeMethodModelMutable NewCompositeMethodModel(
         CollectionsFactory cf,
         CompositeModelMutable owner,
         CompositeAssemblyInfo assInfo,
         IDictionary<Type, Type> mixinsToUse,
         IDictionary<Type, Type> concernsToUse,
         IDictionary<Type, Type> sideEffectsToUse,
         Int32 methodIndex,
         MethodInfo method,
         IDictionary<MethodInfo, PropertyInfo> propertyCache,
         IDictionary<PropertyInfo, PropertyModelMutable> propertyModelCache,
         IDictionary<MethodInfo, EventInfo> eventCache,
         IDictionary<EventInfo, EventModelMutable> eventModelCache,
         ISet<Type> defaultConstraints,
         IDictionary<Type, AppliesToFilter> appliesFiltersCache,
         IDictionary<Type, ISet<Type>> appliesTypeCache,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         ArgumentValidator.ValidateNotNull( "Composite model", owner );
         ArgumentValidator.ValidateNotNull( "Composite method", method );

         CompositeMethodModelState state = new CompositeMethodModelState( cf );
         CompositeMethodModelImmutable resultImmutable = new CompositeMethodModelImmutable( state );
         CompositeMethodModelMutable result = new CompositeMethodModelMutable( state, resultImmutable );

         PropertyModelMutable propertyModel = null;
         if ( propertyCache.ContainsKey( method ) )
         {
            PropertyInfo property = propertyCache[method];
            if ( !propertyModelCache.TryGetValue( property, out propertyModel ) )
            {
               propertyModel = this.NewPropertyModel( cf, property, assInfo );
               propertyModelCache.Add( property, propertyModel );
            }
            if ( method.Equals( property.GetGetMethod() ) )
            {
               propertyModel.GetterMethod = result;
            }
            else if ( method.Equals( property.GetSetMethod() ) )
            {
               propertyModel.SetterMethod = result;
            }
            else
            {
               throw new InternalException( "Method " + method + " was marked as part of property " + property + " but did not equal getter nor setter method." );
            }
         }
         EventModelMutable eventModel = null;
         if ( eventCache.ContainsKey( method ) )
         {
            EventInfo evt = eventCache[method];
            if ( !eventModelCache.TryGetValue( evt, out eventModel ) )
            {
               eventModel = this.NewEventModel( cf, evt );
               eventModelCache.Add( evt, eventModel );
            }
            if ( method.Equals( evt.GetAddMethod() ) )
            {
               eventModel.AddMethod = result;
            }
            else if ( method.Equals( evt.GetRemoveMethod() ) )
            {
               eventModel.RemoveMethod = result;
            }
            else
            {
               throw new InternalException( "Method " + method + " was marked as part of event " + evt + " but did not equal adder nor remover method." );
            }
         }

         state.Owner = owner;
         state.NativeInfo = method;
         state.CompositeMethodIndex = methodIndex;
         state.PropertyModel = propertyModel;
         state.EventModel = eventModel;

         state.Mixin = this.FindMixinModel( assInfo, result, mixinsToUse, appliesFiltersCache, appliesTypeCache, attributeTransformer );
         state.Concerns.AddRange( this.FindConcernModels( assInfo, result, concernsToUse, appliesFiltersCache, appliesTypeCache, attributeTransformer ) );
         state.SideEffects.AddRange( this.FindSideEffectModels( assInfo, result, sideEffectsToUse, appliesFiltersCache, appliesTypeCache, attributeTransformer ) );
         state.ResultModel = this.NewParameterModel( cf, assInfo, resultImmutable, method.ReturnParameter, defaultConstraints, attributeTransformer );
         state.Parameters.AddRange(
            method.GetParameters()
            .Select( param => this.NewParameterModel( cf, assInfo, resultImmutable, param, defaultConstraints, attributeTransformer ) )
            );
         return result;
      }

      protected virtual MixinMethodModelMutable FindMixinModel( CompositeAssemblyInfo compositeInfo, CompositeMethodModelMutable owner, IDictionary<Type, Type> typez, IDictionary<Type, AppliesToFilter> appliesFiltersCache, IDictionary<Type, ISet<Type>> appliesTypeCache, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         if ( compositeInfo != null )
         {
            Type possibleMixin = compositeInfo.Types.FirstOrDefault( pType => !pType.IsInterface() );
            if ( possibleMixin != null )
            {
               typez[possibleMixin] = possibleMixin;// = typez.Concat( Enumerable.Repeat( possibleMixin, 1 ) );
            }
         }
         return typez
            .Select( kvp => Tuple.Create( kvp.Key, this.TryGetSameSignatureOrGeneric( owner.NativeInfo, kvp.Value ) ) )
            .Where( tuple => tuple.Item2 != null && ( compositeInfo == null || this.DoesApply( compositeInfo, owner, tuple.Item2, compositeInfo.GetFragmentAssemblyInfo( FragmentModelType.Mixin ), tuple.Item1, appliesFiltersCache, appliesTypeCache, attributeTransformer ) ) )
            .OrderBy( mixinMethod => mixinMethod, MIXIN_COMPARER )
            .Select( tuple => this.NewMixinMethodModel( owner, tuple.Item2, typez[tuple.Item1] ) )
            .FirstOrDefault();
      }

      protected virtual IEnumerable<ConcernMethodModelMutable> FindConcernModels( CompositeAssemblyInfo compositeInfo, CompositeMethodModelMutable owner, IDictionary<Type, Type> typez, IDictionary<Type, AppliesToFilter> appliesFiltersCache, IDictionary<Type, ISet<Type>> appliesTypeCache, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         return typez
           .Select( kvp => Tuple.Create( kvp.Key, this.TryGetSameSignatureOrGeneric( owner.NativeInfo, kvp.Value ) ) )
           .Where( tuple => tuple.Item2 != null && ( compositeInfo == null || this.DoesApply( compositeInfo, owner, tuple.Item2, compositeInfo.GetFragmentAssemblyInfo( FragmentModelType.Concern ), tuple.Item1, appliesFiltersCache, appliesTypeCache, attributeTransformer ) ) )
           .Select( tuple => this.NewConcernMethodModel( owner, tuple.Item2, typez[tuple.Item1] ) );
      }

      protected virtual IEnumerable<SideEffectMethodModelMutable> FindSideEffectModels( CompositeAssemblyInfo compositeInfo, CompositeMethodModelMutable owner, IDictionary<Type, Type> typez, IDictionary<Type, AppliesToFilter> appliesFiltersCache, IDictionary<Type, ISet<Type>> appliesTypeCache, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         return typez
            .Select( kvp => Tuple.Create( kvp.Key, this.TryGetSameSignatureOrGeneric( owner.NativeInfo, kvp.Value ) ) )
            .Where( tuple => tuple.Item2 != null && ( compositeInfo == null || this.DoesApply( compositeInfo, owner, tuple.Item2, compositeInfo.GetFragmentAssemblyInfo( FragmentModelType.SideEffect ), tuple.Item1, appliesFiltersCache, appliesTypeCache, attributeTransformer ) ) )
            .Select( tuple => this.NewSideEffectMethodModel( owner, tuple.Item2, typez[tuple.Item1] ) );
      }

      protected virtual Boolean DoesApply(
         CompositeAssemblyInfo assInfo,
         CompositeMethodModelMutable owner,
         MethodInfo fragmentMethod,
         FragmentAssemblyInfo assemblyInfo,
         Type mixinTypeFromAssembly,
         IDictionary<Type, AppliesToFilter> appliesFiltersCache,
         IDictionary<Type, ISet<Type>> appliesTypeCache,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         Boolean result = true;
         FragmentTypeInfo fTypeInfo;
         if ( assemblyInfo.TypeInfos.TryGetValue( mixinTypeFromAssembly, out fTypeInfo ) )
         {
            ISet<Type> allFilters = appliesTypeCache.GetOrAdd_NotThreadSafe( fTypeInfo.FragmentType, () => (ISet<Type>) new HashSet<Type>(
               fTypeInfo.AppliesFilters.Concat(
                  this.ProcessCustomAttributes( assInfo, fTypeInfo.FragmentType, TYPE_ATTRIBUTES_EXTRACTOR, attributeTransformer )
                     .OfType<DefaultAppliesToAttribute>()
                     .SelectMany( attr => attr.Filters )
                  ).Where( t => t != null ) ) );
            ISet<AppliesToFilter> allFiltersReadyMade = fTypeInfo.AppliesReadyMades;

            result = !allFilters.Any() && !allFiltersReadyMade.Any();
            foreach ( AppliesToFilter filter in allFiltersReadyMade )
            {
               result = filter.AppliesTo( owner.NativeInfo, fragmentMethod );

               // One match is enough
               if ( result )
               {
                  break;
               }
            }

            if ( !result )
            {
               foreach ( Type applyFilter in allFilters )
               {
                  if ( typeof( AppliesToFilter ).IsAssignableFrom( applyFilter ) )
                  {
                     AppliesToFilter filter = null;
                     if ( !appliesFiltersCache.TryGetValue( applyFilter, out filter ) )
                     {
                        filter = (AppliesToFilter) applyFilter.LoadConstructorOrThrow( 0 ).Invoke( null );
                        appliesFiltersCache.Add( applyFilter, filter );
                     }
                     result = filter.AppliesTo( owner.NativeInfo, fragmentMethod );
                  } // TODO instead of processing it like this here, create a specific type of class implementing AppliesToFilter and delegate check to it.
                  else if ( typeof( Attribute ).IsAssignableFrom( applyFilter ) )
                  {
                     IEnumerable<Attribute> attrs = this.ProcessCustomAttributes( assInfo, owner.NativeInfo, METHOD_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer )
                        .Concat( this.ProcessCustomAttributes( assInfo, fragmentMethod, METHOD_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer ) );
                     if ( owner.Mixin != null )
                     {
                        attrs = attrs.Concat( this.ProcessCustomAttributes( assInfo, owner.Mixin.NativeInfo, METHOD_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer ) );
                     }
                     result = attrs.Any( attrib => applyFilter.IsAssignableFrom( attrib.GetType() ) );
                  }
                  else
                  {
                     result = owner.NativeInfo.DeclaringType.Equals( applyFilter );
                  }

                  // One match is enough
                  if ( result )
                  {
                     break;
                  }
               }
            }
         }
         return result;
      }

      protected virtual PropertyModelMutable NewPropertyModel( CollectionsFactory cf, PropertyInfo property, CompositeAssemblyInfo compositeInfo )
      {
         var state = new PropertyModelState( cf );
         var immutable = new PropertyModelImmutable( state );
         var result = new PropertyModelMutable( state, immutable );

         state.NativeInfo = property;
         Func<PropertyInfo, ApplicationSPI, Object> creator;
         if ( compositeInfo.DefaultProviders.TryGetValue( AbstractCompositeAssemblyDeclarationForNewImpl.ProcessPropertyInfoForDefaultProvider( property ), out creator ) )
         {
            if ( this.WillDefaultValueCreatorParameterBeNull( property ) )
            {
               var creatuur = creator;
               creator = ( pInfo, app ) => creatuur( property, app );
            }
            state.DefaultValueCreator = creator;
         }

         return result;
      }

      protected virtual EventModelMutable NewEventModel( CollectionsFactory cf, EventInfo evt )
      {
         EventModelState state = new EventModelState( cf );
         EventModelImmutable immutable = new EventModelImmutable( state );
         EventModelMutable result = new EventModelMutable( state, immutable );

         state.NativeInfo = evt;

         return result;
      }

      protected virtual MixinMethodModelMutable NewMixinMethodModel( CompositeMethodModelMutable compositeMethod, MethodInfo method, Type fragmentType )
      {
         ArgumentValidator.ValidateNotNull( "Composite method", compositeMethod );
         ArgumentValidator.ValidateNotNull( "Mixin method", method );

         MixinMethodModelState state = new MixinMethodModelState();
         MixinMethodModelImmutable resultImmutable = new MixinMethodModelImmutable( state );
         MixinMethodModelMutable result = new MixinMethodModelMutable( state, resultImmutable );

         state.CompositeMethod = compositeMethod;
         state.NativeInfo = method;
         state.FragmentType = fragmentType;
         state.IsGeneric = ArchitectureDefaults.GENERIC_FRAGMENT_TYPE.IsAssignableFrom( method.DeclaringType );
         return result;
      }

      protected virtual ConstructorModelMutable NewConstructorModel(
         CollectionsFactory cf,
         CompositeModelMutable composite,
         CompositeAssemblyInfo assInfo,
         ConstructorInfo constructor,
         Int32 ctorIndex,
         ISet<Type> defaultConstraints,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         ArgumentValidator.ValidateNotNull( "Composite", composite );
         ArgumentValidator.ValidateNotNull( "Constructor", constructor );

         var state = new ConstructorModelState( cf );
         var resultImmutable = new ConstructorModelImmutable( state );
         var result = new ConstructorModelMutable( state, resultImmutable );

         state.Composite = composite;
         state.NativeInfo = constructor;
         state.ConstructorIndex = ctorIndex;
         state.Parameters.AddRange(
            constructor.GetParameters()
            .Select( param => this.NewParameterModel( cf, assInfo, resultImmutable, param, defaultConstraints, attributeTransformer ) )
            );

         return result;
      }

      protected virtual ParameterModelMutable NewParameterModel(
         CollectionsFactory cf,
         CompositeAssemblyInfo compositeInfo,
         AbstractModelWithParameters owner,
         ParameterInfo param,
         ISet<Type> defaultConstraints,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         ArgumentValidator.ValidateNotNull( "Owner", owner );
         ArgumentValidator.ValidateNotNull( "Parameter", param );

         var state = new ParameterModelState( cf );
         var resultImmutable = new ParameterModelImmutable( state );
         var result = new ParameterModelMutable( state, resultImmutable );

         state.Owner = owner;
         state.NativeInfo = param;
         Attribute[] allAttributes = this.ExtractAllAttributes(
            owner,
            paramOwner => paramOwner is ConstructorModel || paramOwner is SpecialMethodModel ? Enumerable.Repeat<AbstractMemberInfoModel<MethodBase>>( (AbstractMemberInfoModel<MethodBase>) paramOwner, 1 ) : ( (CompositeMethodModel) paramOwner ).GetAllMethodModels().Where( mModel => !( mModel is AbstractFragmentMethodModel ) || !( (AbstractFragmentMethodModel) mModel ).IsGeneric ),
            model =>
               this.ProcessCustomAttributes( compositeInfo, param.Position >= 0 ? ( (AbstractMemberInfoModel<MethodBase>) model ).NativeInfo.GetParameters()[param.Position] : ( (AbstractMemberInfoModel<MethodInfo>) model ).NativeInfo.ReturnParameter, PARAMETER_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer )
            ).Cast<Attribute>().ToArray();

         this.ProcessModelWithAttributesState(
            cf,
            allAttributes,
            result
         );

         //this.ProcessInjectableOptionality( state, param, (Int32) param.Attributes, (Int32) ParameterAttributes.HasDefault, p => p.DefaultValue );
         //this.CheckParameterOptionality( result );
         state.IsOptional = state.AllAttributes.CQ.OfType<OptionalAttribute>().Any() || ( ( ( param.Attributes & ParameterAttributes.HasDefault ) != 0 ) && param.DefaultValue == null );
         state.TargetType = param.ParameterType;

         result.ConstraintsMutable.AddRange(
            this.GetConstraints( compositeInfo, result.AllAttributes.CQ, attributeTransformer )
            .SelectMany( attr => this.NewConstraintModels( compositeInfo, attr, result.NativeInfo.ParameterType, resultImmutable, compositeInfo.GetFragmentAssemblyInfo( FragmentModelType.Constraint ).TypeInfos.Keys, defaultConstraints, attributeTransformer )
         ) );

         var nameAttr = allAttributes.OfType<NameAttribute>().FirstOrDefault();
         state.Name = ( nameAttr == null || nameAttr.Name == null ) ? ( param.Position == RETURN_PARAMETER_POSITION ? "return value" : param.Name ) : nameAttr.Name;
         return result;
      }

      //private void CheckParameterOptionality( ParameterModelMutable pModel )
      //{
      //   if ( !pModel.IQ.IsOptional )
      //   {
      //      pModel.IsOptional = pModel.AllAttributes.CQ.OfType<OptionalAttribute>().Any() || ( ( ( pModel.NativeInfo.Attributes & ParameterAttributes.HasDefault ) != 0 ) && pModel.NativeInfo.DefaultValue == null );
      //   }
      //}

      //protected virtual void ProcessInjectableOptionality<T>( AbstractInjectableModelState<T> state, T nativeMember, Int32 attrs, Int32 hasDefaultAttrs, Func<T, Object> defaultExtractor )
      //{
      //   state.IsOptional = state.AllAttributes.CQ.OfType<OptionalAttribute>().Any() || ( ( ( attrs & hasDefaultAttrs ) != 0 ) && defaultExtractor( nativeMember ) == null );
      //}

      protected virtual ConcernMethodModelMutable NewConcernMethodModel( CompositeMethodModelMutable compositeMethod, MethodInfo method, Type fragmentType )
      {
         ArgumentValidator.ValidateNotNull( "Composite method", compositeMethod );
         ArgumentValidator.ValidateNotNull( "Concern method", method );

         var state = new ConcernMethodModelState();
         var resultImmutable = new ConcernMethodModelImmutable( state );
         var result = new ConcernMethodModelMutable( state, resultImmutable );

         state.CompositeMethod = compositeMethod;
         state.NativeInfo = method;
         state.FragmentType = fragmentType;
         state.IsGeneric = ArchitectureDefaults.GENERIC_FRAGMENT_TYPE.IsAssignableFrom( method.DeclaringType );

         return result;
      }

      protected virtual SideEffectMethodModelMutable NewSideEffectMethodModel( CompositeMethodModelMutable compositeMethod, MethodInfo method, Type fragmentType )
      {
         ArgumentValidator.ValidateNotNull( "Composite method", compositeMethod );
         ArgumentValidator.ValidateNotNull( "Side effect method", method );

         var state = new SideEffectMethodModelState();
         var resultImmutable = new SideEffectMethodModelImmutable( state );
         var result = new SideEffectMethodModelMutable( state, resultImmutable );

         state.CompositeMethod = compositeMethod;
         state.NativeInfo = method;
         state.FragmentType = fragmentType;
         state.IsGeneric = ArchitectureDefaults.GENERIC_FRAGMENT_TYPE.IsAssignableFrom( method.DeclaringType );

         return result;
      }

      protected virtual FieldModelMutable NewFieldModel( CollectionsFactory cf, CompositeModelMutable compositeModel, CompositeAssemblyInfo assInfo, Int32 fieldIndex, FieldInfo field, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         var state = new FieldModelState( cf );
         var resultImmutable = new FieldModelImmutable( state );
         var result = new FieldModelMutable( state, resultImmutable );

         state.NativeInfo = field;
         state.Composite = compositeModel;
         state.FieldIndex = fieldIndex;
         this.ProcessModelWithAttributesState( cf, this.ProcessCustomAttributes( assInfo, field, FIELD_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer ), result );
         state.IsOptional = state.AllAttributes.CQ.OfType<OptionalAttribute>().Any();
         state.TargetType = field.FieldType;
         return result;
      }

      protected virtual IEnumerable<ConstraintModelMutable> NewConstraintModels( CompositeAssemblyInfo assInfo, Attribute constraintAttribute, Type valueType, ParameterModel owner, IEnumerable<Type> declaredTypes, ISet<Type> defaultConstraints, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         IList<Attribute> allConstraintAttributes = new List<Attribute>();
         Stack<Attribute> constraintDeclarations = new Stack<Attribute>();
         constraintDeclarations.Push( constraintAttribute );
         while ( constraintDeclarations.Any() )
         {
            Attribute current = constraintDeclarations.Pop();
            allConstraintAttributes.Add( current );
            foreach ( Attribute attr in this.GetConstraints( assInfo, current.GetType().GetCustomAttributes( true ), attributeTransformer ) )
            {
               constraintDeclarations.Push( attr );
            }
         }

         if ( defaultConstraints != null )
         {
            defaultConstraints.UnionWith(
               allConstraintAttributes
               .SelectMany( attr => this.ProcessCustomAttributes( assInfo, attr.GetType(), TYPE_ATTRIBUTES_EXTRACTOR, attributeTransformer ).OfType<DefaultFragmentsAttribute>().Where( df => FragmentModelType.Constraint.Equals( df.FragmentModelType ) ) )
               .SelectMany( defaultAttr => defaultAttr.Fragments )
               );
         }

         IList<ConstraintModelMutable> resolvedConstraints = allConstraintAttributes.Select( attr => this.NewSingleConstraintModel( attr, valueType, owner, declaredTypes ) ).Where( model => model != null ).ToList();
         if ( !resolvedConstraints.Any() )
         {
            // Did not found any matching constraint - must signal it
            ConstraintModelState state = new ConstraintModelState();
            state.Owner = owner;
            state.ConstraintAttribute = constraintAttribute;
            state.ConstraintType = null;
            resolvedConstraints.Add( new ConstraintModelMutable( state, new ConstraintModelImmutable( state ) ) );
         }

         return resolvedConstraints;
      }

      protected virtual ConstraintModelMutable NewSingleConstraintModel( Attribute constraintAttribute, Type valueType, ParameterModel owner, IEnumerable<Type> declaredTypes )
      {
         ConstraintModelMutable mutable = null;
         Type constraintType = declaredTypes.Where( type => this.ConstraintMatches( type, constraintAttribute.GetType(), valueType ) ).FirstOrDefault();

         if ( constraintType != null )
         {
            ConstraintModelState state = new ConstraintModelState();
            ConstraintModelImmutable immutable = new ConstraintModelImmutable( state );
            mutable = new ConstraintModelMutable( state, immutable );

            state.Owner = owner;
            state.ConstraintAttribute = constraintAttribute;
            state.ConstraintType = constraintType;
         }

         return mutable;
      }

      protected virtual SpecialMethodModelMutable NewSpecialMethodModel(
         CollectionsFactory cf,
         CompositeModelMutable compositeModel,
         CompositeAssemblyInfo assInfo,
         MethodInfo method,
         Type fragmentType,
         IEnumerable<Attribute> specialAttributes,
         Int32 specialMethodIndex,
         ISet<Type> defaultConstraints,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
      {
         var state = new SpecialMethodModelState( cf );
         var resultImmutable = new SpecialMethodModelImmutable( state );
         var result = new SpecialMethodModelMutable( state, resultImmutable );

         state.CompositeModel = compositeModel;
         state.NativeInfo = method;
         state.SpecialMethodIndex = specialMethodIndex;
         state.FragmentType = fragmentType;
         this.ProcessModelWithAttributesState( cf, specialAttributes, result );

         state.Parameters.AddRange(
            method.GetParameters()
               .Select( param => this.NewParameterModel( cf, assInfo, resultImmutable, param, defaultConstraints, attributeTransformer ) )
            );
         state.ResultModel = this.NewParameterModel( cf, assInfo, resultImmutable, method.ReturnParameter, defaultConstraints, attributeTransformer );

         return result;
      }

      protected virtual IEnumerable<Attribute> GetConstraints( CompositeAssemblyInfo assInfo, IEnumerable<Object> attributes, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         return attributes.Where( attr => this.ProcessCustomAttributes( assInfo, attr.GetType(), TYPE_ATTRIBUTES_EXTRACTOR, attributeTransformer ).OfType<ConstraintDeclarationAttribute>().Any() ).Cast<Attribute>();
      }

      protected virtual Boolean ConstraintMatches( Type constraintType, Type attributeType, Type valueType )
      {
         return constraintType.GetInterfaces()
            .Any( iFace =>
               iFace.IsGenericType() &&
               Object.Equals( typeof( Constraint<,> ), iFace.GetGenericTypeDefinition() ) &&
                 iFace.GetGenericArguments()[0].IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( attributeType ) &&
               ( iFace.GetGenericArguments()[1].IsGenericParameter || iFace.GetGenericArguments()[1].GetGenericDefinitionIfGenericType().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( valueType.GetRootOfArrayByRefPointerType() ) )
               );
      }

      protected virtual IEnumerable<Attribute> GetInjectionScopes( IEnumerable<Object> processedAttributes )
      {
         return processedAttributes.Where( attr => attr.GetType().GetCustomAttributes( typeof( InjectionScopeAttribute ), true ).Any() ).OfType<Attribute>();
      }

      protected virtual IEnumerable<Attribute> GetSpecialScopes( IEnumerable<Object> processedAttributes )
      {
         return processedAttributes.Where( attr => attr.GetType().GetCustomAttributes( typeof( SpecialScopeAttribute ), true ).Any() ).OfType<Attribute>();
      }

      protected virtual Boolean IsOptional( IEnumerable<Object> attributes )
      {
         return attributes.OfType<OptionalAttribute>().Any();
      }

      protected virtual MethodInfo TryGetSameSignatureOrGeneric( MethodInfo compositeMethod, Type fragment )
      {
         return TryGetSameSignatureOrGeneric( compositeMethod, fragment, ArchitectureDefaults.GENERIC_FRAGMENT_TYPE );
      }

      protected virtual MethodInfo TryGetSameSignatureOrGeneric( MethodInfo compositeMethod, Type fragment, Type genericType )
      {
         MethodInfo result = null;
         if ( genericType != null && genericType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( fragment ) )
         {
            compositeMethod = GENERIC_FRAGMENT_METHOD;
         }
         if ( compositeMethod.DeclaringType.GetAllParentTypes().Any( dType => dType.GetGenericDefinitionIfGenericType().IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( fragment ) ) )
         {
            MethodInfo implMethod = ReflectionHelper.FindMethodImplicitlyImplementingMethod( fragment, compositeMethod );
            if ( implMethod != null && !implMethod.IsAbstract )
            {
               result = implMethod;
            }
         }
         return result;
      }

      protected virtual IDictionary<MethodInfo, Attribute[]> GetSpecialMethods( CompositeAssemblyInfo assInfo, Type fragmentType, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         IDictionary<MethodInfo, Attribute[]> result = new Dictionary<MethodInfo, Attribute[]>();
         foreach ( MethodInfo method in fragmentType.GetAllInstanceMethods() )
         {
            Attribute[] specialAttrs = this.GetSpecialScopes( this.ProcessCustomAttributes( assInfo, method, METHOD_INFO_ATTRIBUTES_EXTRACTOR, attributeTransformer ) ).ToArray();
            if ( specialAttrs.Any() )
            {
               result.Add( method, specialAttrs );
            }
         }

         return result;
      }

      protected virtual IEnumerable<Object> ExtractAllAttributes<RootModelType>( RootModelType rootModel, Func<RootModelType, IEnumerable<AbstractMemberInfoModel<MemberInfo>>> rootModelSelector, Func<AbstractMemberInfoModel<MemberInfo>, IEnumerable<Object>> singleExtractor )
      {
         return rootModelSelector( rootModel )
            .Where( model => model != null )
            .SelectMany( model => singleExtractor( model ) )
            .Distinct( ATTRIBUTE_EQUALITY_COMPARER );
      }

      protected virtual IList<Attribute> ProcessCustomAttributes<TReflectedElement>( CompositeAssemblyInfo assInfo, TReflectedElement reflectedElement, Func<TReflectedElement, IEnumerable<Object>> attributeExtractor, Func<Int32, Object, Attribute, Attribute> attributeTransformer )
      {
         return attributeExtractor( reflectedElement ).Cast<Attribute>().Select( attr => attributeTransformer( assInfo.CompositeID, reflectedElement, attr ) ).Where( attr => attr != null ).ToList();
      }

      protected virtual void ProcessModelWithAttributesState<TMember>( CollectionsFactory cf, IEnumerable<Attribute> processedAttributes, ModelWithAttributesMutable<TMember> model )
         where TMember : class
      {
         model.AllAttributes.AddRange( processedAttributes );
         foreach ( KeyValuePair<Type, ListProxy<Attribute>> kvp in this.CreateDictionaryFromAttributes( cf, processedAttributes ) )
         {
            if ( model.AttributesByMarkingAttribute.CQ.ContainsKey( kvp.Key ) )
            {
               model.AttributesByMarkingAttribute.MQ[kvp.Key].AddRange( kvp.Value.CQ );
            }
            else
            {
               model.AttributesByMarkingAttribute.Add( kvp );
            }
         }
         foreach ( KeyValuePair<Type, DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>> kvp in this.CreateAttributesOfAttributes( cf, processedAttributes ) )
         {
            DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>> dic;
            if ( model.AttributesOfAttributes.CQ.TryGetValue( kvp.Key, out dic ) )
            {
               foreach ( KeyValuePair<Type, ListProxy<Attribute>> kvp2 in kvp.Value.CQ )
               {
                  if ( dic.CQ.ContainsKey( kvp2.Key ) )
                  {
                     dic.MQ[kvp2.Key].AddRange( kvp2.Value.CQ );
                  }
                  else
                  {
                     dic.Add( kvp2 );
                  }
               }
            }
            else
            {
               model.AttributesOfAttributes.Add( kvp );
            }
         }
      }

      protected virtual IEnumerable<KeyValuePair<Type, ListProxy<Attribute>>> CreateDictionaryFromAttributes( CollectionsFactory cf, IEnumerable<Attribute> processedAttributes )
      {
         IDictionary<Type, IList<Attribute>> result = new Dictionary<Type, IList<Attribute>>();
         foreach ( Attribute attr in processedAttributes )
         {
            foreach ( Object markingAttribute in attr.GetType().GetCustomAttributes( true ) )
            {
               Type markingType = markingAttribute.GetType();
               IList<Attribute> list = null;
               if ( !result.TryGetValue( markingType, out list ) )
               {
                  list = new List<Attribute>();
                  result.Add( markingType, list );
               }
               list.Add( attr );
            }
         }
         return result.Select( kvp => new KeyValuePair<Type, ListProxy<Attribute>>( kvp.Key, cf.NewListProxy<Attribute>( kvp.Value ) ) );
      }

      protected virtual IEnumerable<KeyValuePair<Type, DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>>> CreateAttributesOfAttributes( CollectionsFactory cf, IEnumerable<Attribute> processedAttributes )
      {
         IDictionary<Type, IDictionary<Type, IList<Attribute>>> result = new Dictionary<Type, IDictionary<Type, IList<Attribute>>>();
         foreach ( Attribute attr in processedAttributes )
         {
            Type attributeType = attr.GetType();
            IDictionary<Type, IList<Attribute>> dic = null;
            if ( !result.TryGetValue( attributeType, out dic ) )
            {
               dic = new Dictionary<Type, IList<Attribute>>();
               result.Add( attributeType, dic );
            }
            foreach ( Attribute markingAttribute in attr.GetType().GetCustomAttributes( true ).Cast<Attribute>() )
            {
               Type attributeOfAttributeType = markingAttribute.GetType();
               IList<Attribute> set = null;
               if ( !dic.TryGetValue( attributeOfAttributeType, out set ) )
               {
                  set = new List<Attribute>();
                  dic.Add( attributeOfAttributeType, set );
               }
               set.Add( markingAttribute );
            }
         }
         return result.Select( kvp => new KeyValuePair<Type, DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>>( kvp.Key, cf.NewDictionary<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>( kvp.Value.ToDictionary( kvp2 => kvp2.Key, kvp2 => cf.NewListProxy( kvp2.Value ) ) ) ) );
      }

      protected virtual ISet<Type> ProcessThisTypeForSubTypes( Type thisType, ISet<Type> fragmentTypes )
      {
         var gDef = thisType.GetGenericDefinitionIfContainsGenericParameters();
         return new HashSet<Type>( fragmentTypes
            .SelectMany( fType => fType.GetImplementedInterfaces() )
            .Where( fIFace => gDef.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( fIFace ) )
            .SelectMany( fIFace => this.TransformThisTypeForSubType( thisType, fIFace ) )
            .GetBottomTypes()
            );
      }

      protected virtual IEnumerable<Type> TransformThisTypeForSubType( Type originalThisType, Type resolvedThisType )
      {
         IEnumerable<Type> result;
         if ( ( !originalThisType.IsGenericType() && resolvedThisType.IsGenericType() ) || ( originalThisType.IsGenericType() && resolvedThisType.IsGenericType() && resolvedThisType.GetGenericArguments().Length > originalThisType.GetGenericArguments().Length ) )
         {
            // TODO handle this (possible) error situation better.
            throw new InternalException( "Original this type was " + originalThisType + " but it resolved to " + resolvedThisType + ", having generic argument mismatch." );
         }

         if ( originalThisType.Equals( resolvedThisType ) || originalThisType.GetGenericDefinitionIfContainsGenericParameters().Equals( resolvedThisType.GetGenericDefinitionIfContainsGenericParameters() ) )
         {
            result = Enumerable.Repeat( originalThisType, 1 );
         }
         else if ( originalThisType.IsGenericType() && resolvedThisType.IsGenericType() )
         {
            var gArgs = originalThisType.GetGenericArguments();
            var gDef = originalThisType.GetGenericDefinitionIfGenericType();
            var matchingTypes = resolvedThisType.GetAllParentTypes()
               .Where( resolvedType => resolvedThisType.GetGenericDefinitionIfGenericType().Equals( gDef ) )
               .ToArray();
            result = Enumerable.Repeat<Type>( resolvedThisType.GetGenericTypeDefinition(), matchingTypes.Length )
               .Select( ( rType, idx ) => rType.MakeGenericType( gArgs ) );
         }
         else
         {
            result = Enumerable.Repeat( resolvedThisType, 1 );
         }
         return result;
      }


      protected void ProcessReflectedInfoModels<TModel, TMemberInfo>(
         CollectionsFactory cf,
         CompositeModelMutable compositeModel,
          CompositeAssemblyInfo assInfo,
         IEnumerable<TModel> models,
         Func<TModel, Type> reflectedInfoDeclaringTypeExtractor,
         Func<Type, TModel, TMemberInfo> memberInfoExtractor,
         Func<TMemberInfo, IEnumerable<Object>> attributesExtractor,
         Func<Int32, Object, Attribute, Attribute> attributeTransformer
         )
         where TModel : ModelWithAttributesMutable<TMemberInfo>
         where TMemberInfo : MemberInfo
      {
         foreach ( var model in models )
         {
            var reflectedInfoDeclaringType = reflectedInfoDeclaringTypeExtractor( model ).GetGenericDefinitionIfContainsGenericParameters();
            foreach ( var mInfo in compositeModel.Methods.MQ
                  .SelectMany( cMethod => cMethod.IQ.GetAllMethodModels() )
                  .Where( mModel => mModel != null )
                  .Select( mModel => mModel.NativeInfo.DeclaringType )
                  .Distinct()
                  .Where( dType => reflectedInfoDeclaringType.IsAssignableFrom_IgnoreGenericArgumentsForGenericTypes( dType ) )
                  .Select( dType => memberInfoExtractor( dType, model ) )
                  .Where( info => info != null )
                  )
            {
               this.ProcessModelWithAttributesState( cf, this.ProcessCustomAttributes( assInfo, mInfo, attributesExtractor, attributeTransformer ), model );
            }
         }
      }


      protected virtual void CreateCompositeModelObjects( Assembling.CompositeAssemblyInfo info, CollectionsFactory factory, out CompositeModelState state, out CompositeModelImmutable resultImmutable, out CompositeModelMutable result )
      {
         state = new CompositeModelState( factory );
         resultImmutable = new CompositeModelImmutable( state );
         result = new CompositeModelMutable( state, resultImmutable );
      }

      protected virtual Boolean WillDefaultValueCreatorParameterBeNull( PropertyInfo pInfo )
      {
         return !pInfo.DeclaringType.ContainsGenericParameters();
      }

      protected abstract void PostProcessModel( CompositeModelMutable model, CompositeAssemblyInfo info, String architectureContainerID );

   }

   public static class ReflectionExtensions
   {
      public static Object[] GetCustomAttributes( this Assembly assembly )
      {
         return assembly.GetCustomAttributes( true );
      }
   }
}

public static partial class E_Qi4CS
{
#if !WINDOWS_PHONE_APP
   private const BindingFlags GET_PUBLIC_DECLARED_INSTANCE_METHODS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
   private const BindingFlags GET_DECLARED_INSTANCE_METHODS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
   private const BindingFlags GET_INSTANCE_METHODS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
#endif

   internal static
#if WINDOWS_PHONE_APP
      IEnumerable<MethodInfo>
#else
 MethodInfo[]
#endif
 GetPublicDeclaredInstanceMethods( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().DeclaredMethods.Where(m => m.IsPublic && !m.IsStatic)
#else
 type.GetMethods( GET_PUBLIC_DECLARED_INSTANCE_METHODS )
#endif
;
   }

   internal static
#if WINDOWS_PHONE_APP
      IEnumerable<MethodInfo>
#else
 MethodInfo[]
#endif
 GetAllDeclaredInstanceMethods( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().DeclaredMethods.Where(m => !m.IsStatic)
#else
 type.GetMethods( GET_DECLARED_INSTANCE_METHODS )
#endif
;
   }

   internal static
#if WINDOWS_PHONE_APP
      IEnumerable<MethodInfo>
#else
 MethodInfo[]
#endif
 GetAllInstanceMethods( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetRuntimeMethods().Where(m => !m.IsStatic)
#else
 type.GetMethods( GET_INSTANCE_METHODS )
#endif
;
   }

   internal static
#if WINDOWS_PHONE_APP
      IEnumerable<PropertyInfo>
#else
 PropertyInfo[]
#endif
 GetAllDeclaredInstanceProperties( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().DeclaredProperties.Where(p => !p.GetSomeMethod().IsStatic)
#else
 type.GetProperties( GET_DECLARED_INSTANCE_METHODS )
#endif
;
   }

   internal static
#if WINDOWS_PHONE_APP
      IEnumerable<EventInfo>
#else
 EventInfo[]
#endif
 GetAllDeclaredInstanceEvents( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().DeclaredEvents.Where(e => !e.GetSomeMethod().IsStatic)
#else
 type.GetEvents( GET_DECLARED_INSTANCE_METHODS )
#endif
;
   }

   internal static
#if WINDOWS_PHONE_APP
      IEnumerable<FieldInfo>
#else
 FieldInfo[]
#endif
 GetAllInstanceFields( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
         // http://geertvanhorrik.com/2012/07/02/flattenhierarchy-for-static-members-in-winrt/ it seems .GetRuntimeFields() does indeed return all non-static fields including inherited onces (but not static (altho this may be fixed?))
 type.GetRuntimeFields().Where(f => !f.IsStatic)
#else
 type.GetFields( GET_INSTANCE_METHODS )
#endif
;
   }

   internal static
#if WINDOWS_PHONE_APP
      IEnumerable<FieldInfo>
#else
 FieldInfo[]
#endif
 GetAllDeclaredInstanceFields( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().DeclaredFields.Where(f => !f.IsStatic)
#else
 type.GetFields( GET_DECLARED_INSTANCE_METHODS )
#endif
;
   }

   internal static
#if WINDOWS_PHONE_APP
      IEnumerable<ConstructorInfo>
#else
 ConstructorInfo[]
#endif
 GetAllInstanceConstructors( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic)
#else
 type.GetConstructors( GET_DECLARED_INSTANCE_METHODS )
#endif
;
   }

   internal static Type GetBaseType( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().BaseType
#else
 type.BaseType
#endif
;
   }

   internal static Boolean IsInterface( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().IsInterface
#else
 type.IsInterface
#endif
;
   }

   internal static Boolean ContainsGenericParameters( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().ContainsGenericParameters
#else
 type.ContainsGenericParameters
#endif
;
   }

   internal static Boolean IsGenericType( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().IsGenericType
#else
 type.IsGenericType
#endif
;
   }

   internal static Boolean IsGenericTypeDefinition( this Type type )
   {
      return
#if WINDOWS_PHONE_APP
 type.GetTypeInfo().IsGenericTypeDefinition
#else
 type.IsGenericTypeDefinition
#endif
;
   }

   internal static System.Reflection.Assembly GetAssembly( this Type type )
   {
      return type
#if WINDOWS_PHONE_APP
.GetTypeInfo()
#endif
.Assembly;
   }

#if WINDOWS_PHONE_APP

   internal static Type GetType(this Assembly assembly, String typeName, Boolean throwOnError)
   {
      var retVal = assembly.GetType(typeName);
      if ( throwOnError && retVal == null)
      {
         throw new TypeLoadException("Can't fine type " + typeName + " from " + assembly + ".");
      }
      return retVal;
   }

   internal static IEnumerable<Object> GetCustomAttributes(this Type type, Boolean inherit)
   {
      return type.GetTypeInfo().GetCustomAttributes(inherit);
   }

   internal static IEnumerable<Object> GetCustomAttributes(this Type type, Type attributeType, Boolean inherit)
   {
      return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit);
   }

   internal static MethodInfo GetSetMethod(this PropertyInfo property)
   {
      var retVal = property.SetMethod;
      return retVal != null && retVal.IsPublic ? retVal : null;
   }

   internal static MethodInfo GetGetMethod(this PropertyInfo property)
   {
      var retVal = property.GetMethod;
      return retVal != null && retVal.IsPublic ? retVal : null;
   }

   internal static MethodInfo GetAddMethod(this EventInfo evt)
   {
      var retVal = evt.AddMethod;
      return retVal != null && retVal.IsPublic ? retVal : null;
   }

   internal static MethodInfo GetRemoveMethod(this EventInfo evt)
   {
      var retVal = evt.RemoveMethod;
      return retVal != null && retVal.IsPublic ? retVal : null;
   }

   internal static PropertyInfo GetProperty(this Type type, String name)
   {
      var retVal = type.GetRuntimeProperty(name);
      return retVal != null && retVal.GetSomeMethod().IsPublic ? retVal : null;
   }

   internal static EventInfo GetEvent(this Type type, String name)
   {
      var retVal = type.GetRuntimeEvent(name);
      return retVal != null && retVal.GetSomeMethod().IsPublic ? retVal : null;
   }

   internal static MethodInfo GetSomeMethod(this PropertyInfo property)
   {
      return property.SetMethod ?? property.GetMethod;
   }

   internal static MethodInfo GetSomeMethod(this EventInfo evt)
   {
      return evt.AddMethod ?? evt.RemoveMethod ?? evt.RaiseMethod;
   }

   internal static Boolean IsAssignableFrom(this Type type, Type other)
   {
      return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
   }

   internal static IEnumerable<Type> GetInterfaces(this Type type)
   {
      return type.GetTypeInfo().ImplementedInterfaces;
   }

   internal static ConstructorInfo GetConstructor(this Type type, Type[] ctorParamTypes)
   {
      return type.GetAllInstanceConstructors()
         .FirstOrDefault(c => SequenceEqualityComparer<IEnumerable<Type>, Type>.DefaultSequenceEqualityComparer.Equals(
            ctorParamTypes,
            c.GetParameters().Select(p => p.ParameterType))
            );
   }
#endif
}