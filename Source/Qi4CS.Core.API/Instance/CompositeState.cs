/*
 * Copyright (c) 2008, Rickard Öberg.
 * See NOTICE file.
 * 
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
using System.Reflection;
using CollectionsWithRoles.API;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// This interface holds the full state of the composite, which includes properties and events.
   /// The instances of this interface may be obtained via <see cref="API.Model.StateAttribute"/> injection.
   /// </summary>
   /// <seealso cref="API.Model.StateAttribute"/>
   /// <seealso cref="QualifiedName"/>
   public interface CompositeState
   {
      /// <summary>
      /// Gets all the properties of the composite, with the <see cref="QualifiedName"/> of each property as key.
      /// </summary>
      /// <value>All the properties of the composite.</value>
      /// <seealso cref="CompositeProperty"/>
      /// <seealso cref="QualifiedName"/>
      DictionaryQuery<QualifiedName, CompositeProperty> Properties { get; }

      /// <summary>
      /// Gets the mapping from each method related to some property or event of the composite to the <see cref="QualifiedName"/> of that property or event.
      /// </summary>
      /// <value>the mapping from each method related to some property or event of the composite to the <see cref="QualifiedName"/> of that property or event.</value>
      /// <remarks>This property is useful in e.g. generic fragments where you don't want to create a new <see cref="QualifiedName"/> on every invocation.</remarks>
      DictionaryQuery<MethodInfo, QualifiedName> QualifiedNamesForMethods { get; }

      /// <summary>
      /// Gets all the events of the composite, with the <see cref="QualifiedName"/> of each event as key.
      /// </summary>
      /// <value>All the events of the composite.</value>
      /// <seealso cref="CompositeEvent"/>
      /// <seealso cref="QualifiedName"/>
      DictionaryQuery<QualifiedName, CompositeEvent> Events { get; }
   }
}
