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
//using System;
//using System.Reflection;
//using CommonUtils;

//namespace Qi4CS.Core.Runtime.Model
//{
//   public class WeakEventHandlerWrapperForCodeGeneration
//   {
//      // TODO TODO TODO
//      // This method is missing from .NET 4.0 Portable Profile. Either upgrade to .NET 4.5 Portable Profile, or use different technique when invoking weak events.
//      private static readonly MethodInfo GET_METHOD_HANDLE_VALUE = typeof( RuntimeMethodHandle ).LoadGetterOrThrow( "Value" );

//      private readonly EventHandlerInfoForCodeGeneration[] _array;
//      private readonly Int32 _elementCount;

//      /// <summary>
//      /// Constructor for combining.
//      /// </summary>
//      /// <param name="wrapper">Existing handlers. May be null.</param>
//      /// <param name="newElements">Handlers to add. May not be null.</param>
//      private WeakEventHandlerWrapperForCodeGeneration( WeakEventHandlerWrapperForCodeGeneration wrapper, EventHandlerInfoForCodeGeneration[] newElements )
//      {
//         ArgumentValidator.ValidateNotNull( "New elements", newElements );
//         Int32 copyStartIdx;
//         var newArrSize = GetNewArraySize( wrapper, newElements.Length );
//         if ( wrapper == null )
//         {
//            copyStartIdx = 0;
//         }
//         else
//         {
//            copyStartIdx = wrapper._elementCount;
//         }
//         this._array = new EventHandlerInfoForCodeGeneration[newArrSize];
//         if ( wrapper != null )
//         {
//            System.Array.Copy( wrapper._array, this._array, wrapper._elementCount );
//         }
//         System.Array.Copy( newElements, 0, this._array, copyStartIdx, newElements.Length );
//         this._elementCount = copyStartIdx + newElements.Length;
//      }

//      /// <summary>
//      /// Clean-up constructor.
//      /// </summary>
//      /// <param name="wrapper">Existing handlers. Must not be null.</param>
//      /// <param name="deadInfos">Amount of 'dead' infos.</param>
//      private WeakEventHandlerWrapperForCodeGeneration( WeakEventHandlerWrapperForCodeGeneration wrapper, Int32 deadInfos )
//      {
//         ArgumentValidator.ValidateNotNull( "Existing wrapper", wrapper );
//         var newArrSize = GetNewArraySize( wrapper, -deadInfos );
//         this._array = new EventHandlerInfoForCodeGeneration[newArrSize];
//         this._elementCount = wrapper._elementCount - deadInfos;

//         var thisIdx = 0;
//         for ( var otherIdx = 0; otherIdx < wrapper._elementCount; ++otherIdx )
//         {
//            EventHandlerInfoForCodeGeneration info = wrapper._array[otherIdx];
//            if ( !IsDead( info ) )
//            {
//               this._array[thisIdx] = info;
//               ++thisIdx;
//            }
//         }
//      }

//      /// <summary>
//      /// Constructor for removing.
//      /// </summary>
//      /// <param name="wrapper">Existing event handlers. Must not be null.</param>
//      /// <param name="amountOfDelegates">The amount of delegates to remove.</param>
//      /// <param name="firstIndex">The first index to remove.</param>
//      private WeakEventHandlerWrapperForCodeGeneration( WeakEventHandlerWrapperForCodeGeneration wrapper, Int32 amountOfDelegates, Int32 firstIndex )
//      {
//         ArgumentValidator.ValidateNotNull( "Existing wrapper", wrapper );
//         var newArrSize = GetNewArraySize( wrapper, -amountOfDelegates );
//         this._array = new EventHandlerInfoForCodeGeneration[newArrSize];
//         this._elementCount = wrapper._elementCount - amountOfDelegates;

//         System.Array.Copy( wrapper._array, 0, this._array, 0, firstIndex );
//         if ( firstIndex + amountOfDelegates < wrapper._elementCount )
//         {
//            System.Array.Copy( wrapper._array, firstIndex + amountOfDelegates, this._array, firstIndex, this._elementCount - firstIndex );
//         }
//      }

//      public EventHandlerInfoForCodeGeneration[] Array
//      {
//         get
//         {
//            return this._array;
//         }
//      }

//      public Int32 ElementCount
//      {
//         get
//         {
//            return this._elementCount;
//         }
//      }

//      private static Int32 GetNewArraySize( WeakEventHandlerWrapperForCodeGeneration existing, Int32 amountOfOthers )
//      {
//         Int32 newArrSize;
//         if ( existing == null )
//         {
//            newArrSize = amountOfOthers;
//         }
//         else
//         {
//            Int32 existingCount = existing._elementCount;
//            Int32 existingLength = existing._array.Length;
//            newArrSize = existingLength;
//            Int32 newCount = existingCount + amountOfOthers;
//            if ( amountOfOthers > 0 )
//            {
//               while ( newArrSize < newCount )
//               {
//                  // Need to allocate new array
//                  newArrSize *= 2;
//               }
//            }
//            else
//            {
//               while ( newArrSize >= newCount * 2 )
//               {
//                  // Need to shrink array
//                  newArrSize /= 2;
//               }
//            }
//         }
//         return newArrSize;
//      }

//      private static Int32 FindFromExisting( WeakEventHandlerWrapperForCodeGeneration existing, Delegate[] delegates )
//      {
//         Int32 result = -1;
//         if ( existing != null )
//         {
//            Int32 delLength = delegates.Length;
//            Int32 exLength = existing._elementCount;
//            Int32 current = exLength - delLength;
//            while ( result == -1 && current >= 0 )
//            {
//               Boolean areEqual = true;
//               for ( Int32 idx = 0; idx < delLength; ++idx )
//               {
//                  if ( !Object.ReferenceEquals( existing._array[idx + current].Target, delegates[idx].Target ) &&
//                       existing._array[idx + current].Method == (IntPtr) GET_METHOD_HANDLE_VALUE.Invoke( delegates[idx].Method.MethodHandle, null ) )
//                  {
//                     areEqual = false;
//                     break;
//                  }
//               }
//               if ( areEqual )
//               {
//                  result = current;
//               }
//               else
//               {
//                  --current;
//               }
//            }
//         }
//         return result;
//      }

//      public static Boolean IsDead( EventHandlerInfoForCodeGeneration info )
//      {
//         return info.ShouldHaveTarget && info.Target == null;
//      }

//      public static WeakEventHandlerWrapperForCodeGeneration Combine( WeakEventHandlerWrapperForCodeGeneration existing, EventHandlerInfoForCodeGeneration[] additional )
//      {
//         return new WeakEventHandlerWrapperForCodeGeneration( existing, additional );
//      }

//      public static WeakEventHandlerWrapperForCodeGeneration Remove( WeakEventHandlerWrapperForCodeGeneration existing, Delegate[] toRemove )
//      {
//         Int32 idx = FindFromExisting( existing, toRemove );
//         return idx == -1 ? existing : ( existing._elementCount - toRemove.Length == 0 ? null : new WeakEventHandlerWrapperForCodeGeneration( existing, toRemove.Length, idx ) );
//      }

//      public static WeakEventHandlerWrapperForCodeGeneration CleanUp( WeakEventHandlerWrapperForCodeGeneration wrapper )
//      {
//         WeakEventHandlerWrapperForCodeGeneration result;
//         if ( wrapper != null )
//         {
//            EventHandlerInfoForCodeGeneration[] array = wrapper._array;
//            Int32 deadInfos = 0;
//            for ( Int32 idx = 0; idx < wrapper._elementCount; ++idx )
//            {
//               EventHandlerInfoForCodeGeneration info = array[idx];
//               if ( IsDead( info ) )
//               {
//                  ++deadInfos;
//               }
//            }

//            if ( deadInfos <= 0 )
//            {
//               result = wrapper;
//            }
//            else if ( deadInfos < wrapper._elementCount )
//            {
//               result = new WeakEventHandlerWrapperForCodeGeneration( wrapper, deadInfos );
//            }
//            else
//            {
//               result = null;
//            }
//         }
//         else
//         {
//            result = null;
//         }
//         return result;
//      }
//   }

//   public class EventHandlerInfoForCodeGeneration
//   {
//      // TODO TODO TODO
//      // This method is missing from .NET 4.0 Portable Profile. Either upgrade to .NET 4.5 Portable Profile, or use different technique when invoking weak events.
//      private static readonly MethodInfo GET_FUNCTION_POINTER = typeof( RuntimeMethodHandle ).LoadMethodOrThrow( "GetFunctionPointer", 0 );

//      private readonly WeakReference _target;
//      private readonly Boolean _shouldHaveTarget;
//      private readonly IntPtr _method;

//      public EventHandlerInfoForCodeGeneration( Delegate del )
//      {
//         Object trgt = del.Target;

//         this._shouldHaveTarget = trgt != null;
//         this._target = this._shouldHaveTarget ? new WeakReference( del.Target ) : null;
//         this._method = (IntPtr) GET_FUNCTION_POINTER.Invoke( del.Method.MethodHandle, null );
//      }

//      public Object Target
//      {
//         get
//         {
//            return this._shouldHaveTarget ? this._target.Target : null;
//         }
//      }

//      public Boolean ShouldHaveTarget
//      {
//         get
//         {
//            return this._shouldHaveTarget;
//         }
//      }

//      public IntPtr Method
//      {
//         get
//         {
//            return this._method;
//         }
//      }
//   }
//}
