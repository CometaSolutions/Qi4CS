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
using System.Threading;

namespace Qi4CS.Core.Runtime.Instance
{
   public interface FragmentInstance
   {
      Object Fragment { get; set; }
      void SetNextInfo( Int32 methodIndex, Int32 concernIndex );
      Tuple<Int32, Int32> GetNextInfo();
      void SetMethodResult( Int32 methodIndex, Object result, Exception exception );
      Tuple<Int32, Object, Exception> GetMethodResult();
   }

   public class FragmentInstanceImpl : FragmentInstance
   {

      private Object _fragment;

      private Tuple<Int32, Int32> _nextInfo;
      private Tuple<Int32, Object, Exception> _methodResult;

      public FragmentInstanceImpl( Int32 methodIndex, Int32 concernIndex, Object result, Exception exception )
      {
         this._fragment = null;
         this._nextInfo = Tuple.Create( methodIndex, concernIndex );
         this._methodResult = Tuple.Create( methodIndex, result, exception );
      }

      public FragmentInstanceImpl()
      {
         // Initialize all values to null
      }

      #region FragmentInstance Members


      public Object Fragment
      {
         get
         {
            return this._fragment;
         }
         set
         {
            Interlocked.CompareExchange( ref this._fragment, value, null );
         }
      }

      public void SetNextInfo( Int32 methodIndex, Int32 concernIndex )
      {
         this._nextInfo = Tuple.Create( methodIndex, concernIndex );
      }

      public void SetMethodResult( Int32 methodIndex, Object result, Exception exception )
      {
         this._methodResult = Tuple.Create( methodIndex, result, exception );
      }

      public Tuple<Int32, Int32> GetNextInfo()
      {
         return this._nextInfo;
      }

      public Tuple<Int32, Object, Exception> GetMethodResult()
      {
         return this._methodResult;
      }

      #endregion
   }
}
