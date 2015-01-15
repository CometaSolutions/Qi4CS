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
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public class AbstractValidationResultMutableImpl<TState, TValidation, TValidationIQ> : AbstractValidationResult<TValidation, TValidationIQ>
      where TState : AbstractValidationResultState
      where TValidation : AbstractValidationResult<TValidation, TValidationIQ>
      where TValidationIQ : class, AbstractValidationResultIQ
   {
      private readonly TValidationIQ _immutable;
      private readonly TState _state;

      public AbstractValidationResultMutableImpl( TState state, TValidationIQ immutable )
      {
         ArgumentValidator.ValidateNotNull( "State", state );
         ArgumentValidator.ValidateNotNull( "Immutable", immutable );

         this._state = state;
         this._immutable = immutable;
      }

      public CollectionAdditionOnly<StructureValidationError> StructureValidationErrors
      {
         get
         {
            return this._state.StructureValidation.AO;
         }
      }

      public CollectionAdditionOnly<InjectionValidationError> InjectionValidationErrors
      {
         get
         {
            return this._state.InjectionValidation.AO;
         }
      }

      public CollectionAdditionOnly<InternalValidationError> InternalValidationErrors
      {
         get
         {
            return this._state.InternalValidation.AO;
         }
      }

      public override String ToString()
      {
         return this._state.ToString();
      }

      #region Mutable<TValidation,TValidationIQ> Members

      public TValidation MQ
      {
         get
         {
            return (TValidation) (Object) this;
         }
      }

      #endregion

      #region MutableQuery<TValidationIQ> Members

      public TValidationIQ IQ
      {
         get
         {
            return this._immutable;
         }
      }

      #endregion

      protected TState State
      {
         get
         {
            return this._state;
         }
      }
   }

   public class AbstractValidationResultImmutable<TState> : AbstractValidationResultIQ
      where TState : AbstractValidationResultState
   {
      private readonly TState _state;

      public AbstractValidationResultImmutable( TState state )
      {
         ArgumentValidator.ValidateNotNull( "State", state );

         this._state = state;
      }

      #region ApplicationValidationResult Members

      public ListQuery<InjectionValidationError> InjectionValidationErrors
      {
         get
         {
            return this._state.InjectionValidation.CQ;
         }
      }

      public ListQuery<StructureValidationError> StructureValidationErrors
      {
         get
         {
            return this._state.StructureValidation.CQ;
         }
      }

      public ListQuery<InternalValidationError> InternalValidationErrors
      {
         get
         {
            return this._state.InternalValidation.CQ;
         }
      }

      #endregion

      protected TState State
      {
         get
         {
            return this._state;
         }
      }

      public override String ToString()
      {
         return this._state.ToString();
      }

      #region AbstractValidationResult Members


      public virtual Boolean HasAnyErrors()
      {
         return this._state.InjectionValidation.CQ.Where( e => e != null ).Any() ||
           this._state.StructureValidation.CQ.Where( e => e != null ).Any() ||
           this._state.InternalValidation.CQ.Where( e => e != null ).Any();
      }

      #endregion
   }

   public class AbstractValidationResultState
   {
      private ListProxy<StructureValidationError> _structureValidation;
      private ListProxy<InjectionValidationError> _injectionValidation;
      private ListProxy<InternalValidationError> _internalValidation;

      public AbstractValidationResultState( CollectionsFactory cf )
      {
         this._structureValidation = cf.NewListProxy<StructureValidationError>();
         this._injectionValidation = cf.NewListProxy<InjectionValidationError>();
         this._internalValidation = cf.NewListProxy<InternalValidationError>();
      }

      public ListProxy<StructureValidationError> StructureValidation
      {
         get
         {
            return this._structureValidation;
         }
      }

      public ListProxy<InjectionValidationError> InjectionValidation
      {
         get
         {
            return this._injectionValidation;
         }
      }

      public ListProxy<InternalValidationError> InternalValidation
      {
         get
         {
            return this._internalValidation;
         }
      }

      public override String ToString()
      {
         return this._structureValidation.CQ.Where( e => e != null ).Any() || this._injectionValidation.CQ.Where( e => e != null ).Any() || this._internalValidation.CQ.Where( e => e != null ).Any() ? ( "[" + ToString( this._structureValidation ) + "],\n[" + ToString( this._injectionValidation ) + "],\n[" + ToString( this._internalValidation ) + "]" ) : "No errors.";
      }

      private static String ToString<T>( ListProxy<T> list )
         where T : class
      {
         return String.Join( ", ", list.CQ.Where( e => e != null ) );
      }
   }

   public class ApplicationValidationResultMutable : AbstractValidationResultMutableImpl<ApplicationValidationResultState, ApplicationValidationResult, ApplicationValidationResultIQ>, ApplicationValidationResult
   {
      public ApplicationValidationResultMutable( ApplicationValidationResultState state, ApplicationValidationResultIQ immutable )
         : base( state, immutable )
      {
      }

      public DictionaryWithRoles<CompositeModel, CompositeValidationResult, CompositeValidationResult, CompositeValidationResultIQ> CompositeValidationResults
      {
         get
         {
            return this.State.CompositeValidationResults;
         }
      }
   }

   public class ApplicationValidationResultImmutable : AbstractValidationResultImmutable<ApplicationValidationResultState>, ApplicationValidationResultIQ
   {

      public ApplicationValidationResultImmutable( ApplicationValidationResultState state )
         : base( state )
      {
      }

      #region ApplicationValidationResult Members

      public DictionaryQuery<CompositeModel, CompositeValidationResultIQ> CompositeValidationResults
      {
         get
         {
            return this.State.CompositeValidationResults.MQ.IQ;
         }
      }

      public ApplicationModel<SPI.Instance.ApplicationSPI> ApplicationModel
      {
         get
         {
            return this.State.Model;
         }
      }

      #endregion

      public override Boolean HasAnyErrors()
      {
         return base.HasAnyErrors() ||
                this.State.CompositeValidationResults.MQ.IQ.Values.SelectMany( cResult => cResult.InjectionValidationErrors ).Where( e => e != null ).Any() ||
                this.State.CompositeValidationResults.MQ.IQ.Values.SelectMany( cResult => cResult.StructureValidationErrors ).Where( e => e != null ).Any() ||
                this.State.CompositeValidationResults.MQ.IQ.Values.SelectMany( cResult => cResult.InternalValidationErrors ).Where( e => e != null ).Any();
      }
   }

   public class ApplicationValidationResultState : AbstractValidationResultState
   {
      private readonly ApplicationModel<ApplicationSPI> _model;
      private readonly DictionaryWithRoles<CompositeModel, CompositeValidationResult, CompositeValidationResult, CompositeValidationResultIQ> _composites;

      public ApplicationValidationResultState( ApplicationModel<ApplicationSPI> model )
         : base( model.CollectionsFactory )
      {
         ArgumentValidator.ValidateNotNull( "Application model", model );
         this._model = model;
         this._composites = this._model.CollectionsFactory.NewDictionary<CompositeModel, CompositeValidationResult, CompositeValidationResult, CompositeValidationResultIQ>(
            new Dictionary<CompositeModel, CompositeValidationResult>( ReferenceEqualityComparer<CompositeModel>.ReferenceBasedComparer )
            );
      }

      public DictionaryWithRoles<CompositeModel, CompositeValidationResult, CompositeValidationResult, CompositeValidationResultIQ> CompositeValidationResults
      {
         get
         {
            return this._composites;
         }
      }

      public ApplicationModel<ApplicationSPI> Model
      {
         get
         {
            return this._model;
         }
      }

      public override String ToString()
      {
         var erroneousCompositeResults = this._composites.CQ.Values.Where( result => result.IQ.HasAnyErrors() );
         return "Application validation results: " + base.ToString() + "\nComposite validation results:\n" + ( erroneousCompositeResults.Any() ? String.Join( "\n", erroneousCompositeResults ) : "No errors" );
      }
   }

   public class CompositeValidationResultMutable : AbstractValidationResultMutableImpl<CompositeValidationResultState, CompositeValidationResult, CompositeValidationResultIQ>, CompositeValidationResult
   {
      public CompositeValidationResultMutable( CompositeValidationResultState state, CompositeValidationResultIQ immutable )
         : base( state, immutable )
      {
      }

      public CompositeTypeModel TypeModel
      {
         set
         {
            this.State.TypeModel = value;
         }
      }
   }

   public class CompositeValidationResultImmutable : AbstractValidationResultImmutable<CompositeValidationResultState>, CompositeValidationResultIQ
   {
      public CompositeValidationResultImmutable( CompositeValidationResultState state )
         : base( state )
      {
      }

      #region CompositeValidationResult Members

      public CompositeTypeModel TypeModel
      {
         get
         {
            return this.State.TypeModel;
         }
      }

      #endregion
   }

   public class CompositeValidationResultState : AbstractValidationResultState
   {
      private CompositeTypeModel _typeModel;
      private readonly CompositeModel _compositeModel;

      public CompositeValidationResultState( CompositeModel model )
         : base( model.ApplicationModel.CollectionsFactory )
      {
         ArgumentValidator.ValidateNotNull( "Composite model", model );
         this._compositeModel = model;
      }

      public CompositeTypeModel TypeModel
      {
         get
         {
            return this._typeModel;
         }
         set
         {
            this._typeModel = value;
         }
      }

      public CompositeModel CompositeModel
      {
         get
         {
            return this._compositeModel;
         }
      }

      public override String ToString()
      {
         return "{" + String.Join( ",", this.CompositeModel.PublicTypes ) + "} " + base.ToString();
      }
   }
}
