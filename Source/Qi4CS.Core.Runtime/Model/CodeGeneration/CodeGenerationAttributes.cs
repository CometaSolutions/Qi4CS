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
using System;

namespace Qi4CS.Core.Runtime.Model
{
   [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
   public sealed class CompositeTypeIDAttribute : Attribute
   {
      private readonly Int32 _id;

      public CompositeTypeIDAttribute( Int32 id )
      {
         this._id = id;
      }

      public Int32 CompositeTypeID
      {
         get
         {
            return this._id;
         }
      }
   }

   [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
   public sealed class NoPoolNeededAttribute : Attribute
   {
   }

   [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
   public sealed class MainPublicCompositeTypeAttribute : Attribute
   {
   }

   public abstract class AbstractAttributeWithIndex : Attribute
   {
      private readonly Int32 _index;

      public AbstractAttributeWithIndex( Int32 index )
      {
         this._index = index;
      }

      public Int32 Index
      {
         get
         {
            return this._index;
         }
      }
   }

   public sealed class ConstructorModelIndexAttribute : AbstractAttributeWithIndex
   {
      public ConstructorModelIndexAttribute( Int32 index )
         : base( index )
      {

      }
   }

   public sealed class CompositeMethodModelIndexAttribute : AbstractAttributeWithIndex
   {
      public CompositeMethodModelIndexAttribute( Int32 index )
         : base( index )
      {

      }
   }

   public sealed class SpecialMethodModelIndexAttribute : AbstractAttributeWithIndex
   {
      public SpecialMethodModelIndexAttribute( Int32 index )
         : base( index )
      {

      }
   }

   [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
   public abstract class AbstractCompositeRelatedCodeGenerationAttribute : Attribute
   {
      private readonly Int32 _compositeID;

      public AbstractCompositeRelatedCodeGenerationAttribute( Int32 compositeID )
      {
         this._compositeID = compositeID;
      }

      public Int32 CompositeID
      {
         get
         {
            return this._compositeID;
         }
      }
   }

   //[AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
   public class CompositeTypesAttribute : AbstractCompositeRelatedCodeGenerationAttribute
   {
      public CompositeTypesAttribute( Int32 compositeID )
         : base( compositeID )
      {
      }

      public Type[] PublicCompositeTypes { get; set; }
      public Type[] PrivateCompositeTypes { get; set; }
      public Type[] FragmentTypes { get; set; }
      public Type[] ConcernInvokationHandlerTypes { get; set; }
      public Type[] SideEffectInvocationHandlerTypes { get; set; }
      public Type CompositeFactoryType { get; set; }
   }

   [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
   public class PublicCompositeGenericBindingInfoAttribute : Attribute
   {
      public Type[] GenericTypes { get; set; }
   }
}
