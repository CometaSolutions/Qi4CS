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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Instance;
using System.Reflection;
using CommonUtils;

namespace Qi4CS.Core.Runtime.Instance
{
   public abstract class CompositeStateParticipantImpl<TReflectionInfo, TModel> : CompositeStateParticipantSPI<TReflectionInfo, TModel>
      where TReflectionInfo : MemberInfo
      where TModel : class
   {
      private readonly TModel _model;
      private readonly TReflectionInfo _reflectionInfo;
      private readonly QualifiedName _qName;
      private readonly RefInvokerCallback _refInvoker;

      protected CompositeStateParticipantImpl( TModel model, TReflectionInfo reflectionInfo, RefInvokerCallback refInvoker )
      {
         ArgumentValidator.ValidateNotNull( "Model", model );
         ArgumentValidator.ValidateNotNull( "Reflection info", reflectionInfo );
         ArgumentValidator.ValidateNotNull( "'Ref' invoker", refInvoker );

         this._model = model;
         this._qName = QualifiedName.FromMemberInfo( reflectionInfo );
         this._reflectionInfo = reflectionInfo;
         this._refInvoker = refInvoker;
      }

      #region CompositeStateParticipant<TReflectionInfo> Members

      public TReflectionInfo ReflectionInfo
      {
         get
         {
            return this._reflectionInfo;
         }
      }

      public QualifiedName QualifiedName
      {
         get
         {
            return this._qName;
         }
      }

      public Boolean TryInvokeActionWithRef<TField>( ActionWithRef<TField> action )
      {
         Object result;
         return this._refInvoker( action, out result );
      }

      public Boolean TryInvokeFunctionWithRef<TField>( FunctionWithRef<TField> function, out TField result )
      {
         Object resultObj;
         var retVal = this._refInvoker( function, out resultObj );
         result = retVal ? (TField) resultObj : default( TField );
         return retVal;
      }

      #endregion

      #region CompositeStateParticipantSPI<TReflectionInfo,TModel> Members

      public TModel Model
      {
         get
         {
            return this._model;
         }
      }

      #endregion
   }

   public delegate Boolean RefInvokerCallback( Object delegateObject, out Object result );
}
