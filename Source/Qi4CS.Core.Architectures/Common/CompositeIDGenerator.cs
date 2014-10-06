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
using System.Threading;

namespace Qi4CS.Core.Architectures.Common
{
   /// <summary>
   /// This is helper class for Qi4CS architectures to generated ID's for composites.
   /// </summary>
   /// <seealso cref="Bootstrap.Assembling.AbstractCompositeAssemblyDeclaration.AffectedCompositeIDs"/>
   /// <seealso cref="SPI.Model.CompositeModel.CompositeModelID"/>
   public sealed class CompositeIDGenerator
   {
      private Int32 _current;
      private readonly Func<Int32> _function;

      /// <summary>
      /// Creates a new instance of <see cref="CompositeIDGenerator"/>.
      /// </summary>
      /// <param name="generationFunction">The function to generate composite IDs.</param>
      /// <remarks>If <paramref name="generationFunction"/> is <c>null</c>, a default function which atomically increments integer by <c>1</c> (first returned value <c>1</c>) is used.</remarks>
      public CompositeIDGenerator( Func<Int32> generationFunction = null )
      {
         this._current = 0;
         this._function = () => Interlocked.Increment( ref this._current );
      }

      /// <summary>
      /// Gets the function that generates composite IDs.
      /// </summary>
      /// <value>The function that generates composite IDs.</value>
      public Func<Int32> IDGeneratorFunction
      {
         get
         {
            return this._function;
         }
      }
   }
}
