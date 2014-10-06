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
using System.Linq;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.Runtime.Assembling;

namespace Qi4CS.Core.Architectures.Assembling
{
   /// <summary>
   /// This is factory class for creating <see cref="LayeredArchitecture"/>s and <see cref="SingletonArchitecture"/>s.
   /// </summary>
   /// <seealso cref="LayeredArchitecture"/>
   /// <seealso cref="SingletonArchitecture"/>
   public static class Qi4CSArchitectureFactory
   {
      private static readonly CompositeModelTypeAssemblyScopeSupport LAYERED_PLAIN_COMPOSITE_SUPPORT = new LayeredPlainCompositeModelTypeAssemblyScopeSupport();
      // TODO implementing value composite model type should be done via extension
      //public static readonly CompositeModelTypeAssemblyScopeSupport VALUE_SUPPORT = new LayeredValueModelTypeAssemblyScopeSupport();
      private static readonly CompositeModelTypeAssemblyScopeSupport LAYERED_SERVICE_SUPPORT = new LayeredServiceModelTypeAssemblyScopeSupport();

      private static readonly CompositeModelTypeAssemblyScopeSupport SINGLETON_PLAIN_COMPOSITE_SUPPORT = new SingletonPlainCompositeModelTypeAssemblyScopeSupport();
      // TODO implementing value composite model type should be done via extension
      //public static readonly CompositeModelTypeAssemblyScopeSupport VALUE_SUPPORT = new SingletonValueModelTypeAssemblyScopeSupport();
      private static readonly CompositeModelTypeAssemblyScopeSupport SINGLETON_SERVICE_INSTANCE = new SingletonApplicationServiceAssemblyScopeSupport();

      /// <summary>
      /// Creates new instance of <see cref="LayeredArchitecture"/> with support for at least <see cref="API.Instance.CompositeModelType.PLAIN"/> and <see cref="API.Instance.CompositeModelType.SERVICE"/>.
      /// Support for additional <see cref="API.Instance.CompositeModelType"/>s may be provided via parameters to this method.
      /// </summary>
      /// <param name="additionalSupports">Additional support for other <see cref="API.Instance.CompositeModelType"/>s than <see cref="API.Instance.CompositeModelType.PLAIN"/> and <see cref="API.Instance.CompositeModelType.SERVICE"/>.</param>
      /// <returns>A new instance of <see cref="LayeredArchitecture"/>.</returns>
      /// <remarks>
      /// If <paramref name="additionalSupports"/> is <c>null</c>, it is ignored.
      /// Any <c>null</c> elements within <paramref name="additionalSupports"/> will be ignored.
      /// </remarks>
      public static LayeredArchitecture NewLayeredArchitecture( params CompositeModelTypeAssemblyScopeSupport[] additionalSupports )
      {
         return new LayeredArchitectureImpl(
            new[] {
            LAYERED_PLAIN_COMPOSITE_SUPPORT,
            //VALUE_SUPPORT,
            LAYERED_SERVICE_SUPPORT,
            }.Concat( additionalSupports.FilterNulls() )
            );
      }


      /// <summary>
      /// Creates new instance of <see cref="SingletonArchitecture"/> with support for at least <see cref="API.Instance.CompositeModelType.PLAIN"/> and <see cref="API.Instance.CompositeModelType.SERVICE"/>.
      /// Support for additional <see cref="API.Instance.CompositeModelType"/>s may be provided via parameters to this method.
      /// </summary>
      /// <param name="additionalSupports">Additional support for other <see cref="API.Instance.CompositeModelType"/>s than <see cref="API.Instance.CompositeModelType.PLAIN"/> and <see cref="API.Instance.CompositeModelType.SERVICE"/>.</param>
      /// <returns>A new instance of <see cref="SingletonArchitecture"/>.</returns>
      /// <remarks>
      /// If <paramref name="additionalSupports"/> is <c>null</c>, it is ignored.
      /// Any <c>null</c> elements within <paramref name="additionalSupports"/> will be ignored.
      /// </remarks>
      public static SingletonArchitecture NewSingletonArchitecture( params CompositeModelTypeAssemblyScopeSupport[] additionalSupports )
      {
         return new SingletonArchitectureImpl(
            new[] { 
            SINGLETON_PLAIN_COMPOSITE_SUPPORT,
            //VALUE_SUPPORT,
            SINGLETON_SERVICE_INSTANCE
            }.Concat( additionalSupports.FilterNulls() )
            );
      }
   }
}
