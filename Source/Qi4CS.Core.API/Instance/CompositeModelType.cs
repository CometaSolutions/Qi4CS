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

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// Each instance of this class represents a single composite model type (e.g. service or plain composite).
   /// </summary>
   /// <remarks>
   /// This class is implemented as extensible enum without id codes.
   /// A code to extend enum is:
   /// <code source="..\Qi4CS.Samples\Qi4CSDocumentation\CodeContent.cs" region="CompositeModelTypeCode1" language="C#" />
   /// The instances of this class are mainly used as keys for dictionaries so reference equality without the hassle of unique code allocation is top priority.
   /// </remarks>
   public abstract class CompositeModelType
   {
      private sealed class ServiceCompositeModel : CompositeModelType
      {
         public override String ToString()
         {
            return "ServiceComposite";
         }
      }

      private sealed class PlainCompositeModel : CompositeModelType
      {
         public override String ToString()
         {
            return "PlainComposite";
         }
      }

      //private sealed class ValueCompositeModel : CompositeModelType
      //{
      //   public override String ToString()
      //   {
      //      return "ValueComposite";
      //   }
      //}

      /// <summary>
      /// This instance represents the service composite model type.
      /// </summary>
      /// <remarks>
      /// Services composites are like plain composites in all aspects except their lifecycle.
      /// The creation and activation and passivation of service composites is managed by Qi4CS.
      /// Therefore they can not be created via <see cref="CompositeBuilder"/>.
      /// Use <see cref="StructureServiceProvider.FindServices(System.Collections.Generic.IEnumerable{Type})"/> method to find services.
      /// </remarks>
      public static readonly CompositeModelType SERVICE = new ServiceCompositeModel();

      /// <summary>
      /// This instance represents the plain composite model type.
      /// </summary>
      /// <remarks>
      /// Plain composites can have properties and events as part of their state.
      /// They can be instantiated via <see cref="CompositeBuilder"/> interface, available by calling <see cref="StructureServiceProvider.NewCompositeBuilder(CompositeModelType, System.Collections.Generic.IEnumerable{Type})"/> method.
      /// </remarks>
      public static readonly CompositeModelType PLAIN = new PlainCompositeModel();

      /// <inheritdoc/>
      public abstract override String ToString();
   }
}
