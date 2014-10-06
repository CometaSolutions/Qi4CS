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
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public abstract class AbstractFragmentMethodModelMutable : AbstractMethodModelMutable
   {
      private readonly AbstractFragmentMethodModelState _state;

      public AbstractFragmentMethodModelMutable( AbstractFragmentMethodModelState state, AbstractFragmentMethodModel immutable )
         : base( state, immutable )
      {
         this._state = state;
      }

      public CompositeMethodModelMutable CompositeMethodMutable
      {
         get
         {
            return this._state.CompositeMethod;
         }
      }

      public new AbstractFragmentMethodModel Immutable
      {
         get
         {
            return (AbstractFragmentMethodModel) base.Immutable;
         }
      }
   }

   public abstract class AbstractFragmentMethodModelImmutable : AbstractMethodModelImmutable, AbstractFragmentMethodModel
   {
      private readonly AbstractFragmentMethodModelState _state;

      public AbstractFragmentMethodModelImmutable( AbstractFragmentMethodModelState state )
         : base( state )
      {
         this._state = state;
      }

      #region AbstractFragmentMethodModel Members

      public CompositeMethodModel CompositeMethod
      {
         get
         {
            return this._state.CompositeMethod.IQ;
         }
      }

      public Boolean IsGeneric
      {
         get
         {
            return this._state.IsGeneric;
         }
      }

      #endregion

      #region AbstractMethodModelWithFragment Members

      public Type FragmentType
      {
         get
         {
            return this._state.FragmentType;
         }
      }

      #endregion
   }

   public class AbstractFragmentMethodModelState : AbstractMethodModelState
   {
      private CompositeMethodModelMutable _compositeMethod;
      private Boolean _isGeneric;
      private Type _fragmentType;

      public CompositeMethodModelMutable CompositeMethod
      {
         get
         {
            return this._compositeMethod;
         }
         set
         {
            this._compositeMethod = value;
         }
      }

      public Boolean IsGeneric
      {
         get
         {
            return this._isGeneric;
         }
         set
         {
            this._isGeneric = value;
         }
      }

      public Type FragmentType
      {
         get
         {
            return this._fragmentType;
         }
         set
         {
            this._fragmentType = value;
         }
      }
   }
}
