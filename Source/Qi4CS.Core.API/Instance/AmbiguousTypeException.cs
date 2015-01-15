/*
 * Copyright (c) 2008, Niclas Hedhman.
 * See NOTICE file.
 * 
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
using System.Collections.Generic;
using System.Linq;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This exception is thrown when more than one composite is found given type search criteria in <see cref="API.Instance.StructureServiceProvider.NewCompositeBuilder(API.Instance.CompositeModelType, IEnumerable{Type})"/>.
   /// </summary>
   /// <example>
   /// With the following composite structure
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="AmbiguousTypeExceptionCode1" language="C#" />
   /// where <c>FirstComposite</c> and <c>SecondComposite</c> are registered plain composites, the code <c>var builder = ssp.NewPlainCompositeBuilder&lt;Parent&gt;();</c>, where <c>ssp</c> is <see cref="API.Instance.StructureServiceProvider"/>, would produce <see cref="AmbiguousTypeException"/> since there are more than one composite implementing <c>Parent</c>.
   /// </example>
   /// <seealso cref="API.Instance.StructureServiceProvider.NewCompositeBuilder(API.Instance.CompositeModelType, IEnumerable{Type})"/>
   public class AmbiguousTypeException : Exception
   {
      private readonly IEnumerable<Type> _types;
      private readonly IEnumerable<IEnumerable<Type>> _matchingTypes;

      /// <summary>
      /// Constructs a new <see cref="AmbiguousTypeException"/>.
      /// </summary>
      /// <param name="types">The types that were used to search for composite.</param>
      /// <param name="matchingTypes">The types of composites that were found.</param>
      public AmbiguousTypeException( IEnumerable<Type> types, IEnumerable<IEnumerable<Type>> matchingTypes )
         : base( "Ambiguous types {" + String.Join( ", ", types ) + "}, the following types match: " + String.Join( ", ", matchingTypes.Select( mTypes => "{" + String.Join( ", ", mTypes ) + "}" ) ) + "." )
      {
         this._types = types;
         this._matchingTypes = matchingTypes;
      }

      /// <summary>
      /// Gets the types that were used to search for composites.
      /// </summary>
      /// <value>The types that were used to search for composites.</value>
      public IEnumerable<Type> TypesSearched
      {
         get
         {
            return this._types;
         }
      }

      /// <summary>
      /// Gets the types of composites that were found.
      /// </summary>
      /// <value>The types of composites that were found.</value>
      public IEnumerable<IEnumerable<Type>> FoundTypes
      {
         get
         {
            return this._matchingTypes;
         }
      }
   }
}
