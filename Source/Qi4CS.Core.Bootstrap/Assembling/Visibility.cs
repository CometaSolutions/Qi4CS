/*
 * Copyright (c) 2007, Rickard Öberg.
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

namespace Qi4CS.Core.Bootstrap.Assembling
{
   /// <summary>
   /// Visibility defines how far composites can be 'seen' and therefore used in <see cref="LayeredArchitecture"/>.
   /// </summary>
   /// <remarks>
   /// <para>
   /// For new composite declaration in <see cref="LayeredCompositeAssembler"/>, the default visibility is <see cref="Visibility.MODULE"/>.
   /// This means that only composites within the same <see cref="Instance.Module"/> can use the composites of that composite declaration.
   /// It is possible to change the visibility through <see cref="LayeredAbstractCompositeAssemblyDeclaration.VisibleIn(Visibility)"/> method.
   /// See documentation of <see cref="Visibility.LAYER"/> and <see cref="Visibility.APPLICATION"/> for information about other visibilities.
   /// </para>
   /// </remarks>
   public enum Visibility
   {
      /// <summary>
      /// Composites with this visibility are only useable by composites within the same <see cref="ModuleArchitecture"/>.
      /// </summary>
      MODULE = 0,

      /// <summary>
      /// Composites with this visibility are only useable by composites of all <see cref="ModuleArchitecture"/>s within the same <see cref="LayerArchitecture"/>.
      /// </summary>
      LAYER = 1,

      /// <summary>
      /// Composites with this visibility are only useable by compositesof all <see cref="ModuleArchitecture"/>s within the same <see cref="LayerArchitecture"/> and any other <see cref="ModuleArchitecture"/>s within <see cref="LayerArchitecture"/>s directly or indirectly using this <see cref="LayerArchitecture"/>.
      /// </summary>
      APPLICATION = 2
   }
}
