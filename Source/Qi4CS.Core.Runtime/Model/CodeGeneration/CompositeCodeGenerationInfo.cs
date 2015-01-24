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
using System.Linq;

namespace Qi4CS.Core.Runtime.Model
{
   public interface CompositeCodeGenerationInfo
   {
      String PublicCompositePrefix { get; }
      String PrivateCompositePrefix { get; }
      String FragmentPrefix { get; }
      String ConcernInvocationPrefix { get; }
      String SideEffectInvocationPrefix { get; }
      String CompositeInstanceFieldName { get; }
      String CompositeFactorySuffix { get; }
      Type CompositeInstanceFieldType { get; }
   }

   public class DefaultCompositeCodeGenerationInfo : CompositeCodeGenerationInfo
   {
      private const String PUBLIC_COMPOSITE_PREFIX = "Composite";
      private const String PRIVATE_COMPOSITE_PREFIX = "PrivateComposite";
      private const String FRAGMENT_PREFIX = "Fragment";
      //private const String FRAGMENT_NO_INSTANCE_POOL_SUFFIX = "NoPool";
      private const String CONCERN_INVOCATION_PREFIX = "ConcernInvocation";
      private const String SIDE_EFFECT_INVOCATION_PREFIX = "SideEffectInvocation";
      private const String COMPOSITE_INSTANCE_FIELD_NAME = "_instance";
      private const String COMPOSITE_FACTORY_SUFFIX = "Factory";

      private readonly Type _compositeInstanceFieldType;

      public DefaultCompositeCodeGenerationInfo( Type compositeInstanceFieldType )
      {
         this._compositeInstanceFieldType = compositeInstanceFieldType;
      }

      #region CompositeCodeGenerationInfo Members

      public String PublicCompositePrefix
      {
         get
         {
            return PUBLIC_COMPOSITE_PREFIX;
         }
      }

      public String PrivateCompositePrefix
      {
         get
         {
            return PRIVATE_COMPOSITE_PREFIX;
         }
      }

      public String FragmentPrefix
      {
         get
         {
            return FRAGMENT_PREFIX;
         }
      }

      //public String FragmentNoInstancePoolSuffix
      //{
      //   get
      //   {
      //      return FRAGMENT_NO_INSTANCE_POOL_SUFFIX;
      //   }
      //}

      public String ConcernInvocationPrefix
      {
         get
         {
            return CONCERN_INVOCATION_PREFIX;
         }
      }

      public String SideEffectInvocationPrefix
      {
         get
         {
            return SIDE_EFFECT_INVOCATION_PREFIX;
         }
      }

      public String CompositeInstanceFieldName
      {
         get
         {
            return COMPOSITE_INSTANCE_FIELD_NAME;
         }
      }

      public String CompositeFactorySuffix
      {
         get
         {
            return COMPOSITE_FACTORY_SUFFIX;
         }
      }

      public Type CompositeInstanceFieldType
      {
         get
         {
            return this._compositeInstanceFieldType;
         }
      }

      #endregion
   }
}
