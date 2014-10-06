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
using CollectionsWithRoles.API;
using CollectionsWithRoles.Implementation;
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   public abstract class ModelWithAttributesMutable<TMember> : AbstractMemberInfoModelMutable<TMember>
      where TMember : class
   {
      private readonly ModelWithAttributesState<TMember> _state;
#pragma warning disable 414
      private readonly ModelWithAttributesImmutable<TMember> _immutable;
#pragma warning restore 414

      public ModelWithAttributesMutable( ModelWithAttributesState<TMember> state, ModelWithAttributesImmutable<TMember> immutable )
         : base( state, immutable )
      {
         this._state = state;
         this._immutable = immutable;
      }

      public DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>> AttributesByMarkingAttribute
      {
         get
         {
            return this._state.AttributesByMarkingAttribute;
         }
      }

      public DictionaryProxy<Type, DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>> AttributesOfAttributes
      {
         get
         {
            return this._state.AttributesOfAttributes;
         }
      }

      public ListProxy<Attribute> AllAttributes
      {
         get
         {
            return this._state.AllAttributes;
         }
      }
   }

   public class ModelWithAttributesImmutable<TMember> : AbstractMemberInfoModelImmutable<TMember>, ModelWithAttributes
      where TMember : class
   {
      public static readonly ListQuery<Attribute> EMPTY_ATTRIBUTES = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewListProxy<Attribute>().CQ;
      public static readonly DictionaryQuery<Type, ListQuery<Attribute>> EMPTY_ATTRIBUTES_SET = CollectionsFactorySingleton.DEFAULT_COLLECTIONS_FACTORY.NewDictionaryProxy<Type, ListQuery<Attribute>>().CQ;


      private readonly ModelWithAttributesState<TMember> _state;

      public ModelWithAttributesImmutable( ModelWithAttributesState<TMember> state )
         : base( state )
      {
         this._state = state;
      }

      #region ModelWithAttributes Members

      public ListQuery<Attribute> GetAttributesMarkedWith( Type markingAttribute )
      {
         ListQuery<Attribute> result;
         if ( markingAttribute == null )
         {
            result = this.AllAttributes;
         }
         else
         {
            ListProxy<Attribute> proxy;
            if ( this._state.AttributesByMarkingAttribute.CQ.TryGetValue( markingAttribute, out proxy ) )
            {
               result = proxy.CQ;
            }
            else
            {
               result = EMPTY_ATTRIBUTES;
            }
         }
         return result;
      }

      public ListQuery<Attribute> AllAttributes
      {
         get
         {
            return this._state.AllAttributes.CQ;
         }
      }

      public DictionaryQuery<Type, ListQuery<Attribute>> GetAttributesOfAttribute( Type attributeType )
      {
         DictionaryQuery<Type, ListQuery<Attribute>> result;
         DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>> proxy;
         if ( this._state.AttributesOfAttributes.CQ.TryGetValue( attributeType, out proxy ) )
         {
            result = proxy.MQ.IQ;
         }
         else
         {
            result = EMPTY_ATTRIBUTES_SET;
         }
         return result;
      }

      #endregion
   }

   public class ModelWithAttributesState<TMember> : AbstractMemberInfoModelState<TMember>
      where TMember : class
   {
      private readonly DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>> _attributes;
      private readonly DictionaryProxy<Type, DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>> _attributesOfAttributes;
      private readonly ListProxy<Attribute> _allAttributes;

      public ModelWithAttributesState( CollectionsFactory collectionsFactory )
      {
         this._allAttributes = collectionsFactory.NewListProxy<Attribute>();
         this._attributes = collectionsFactory.NewDictionary<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>();
         this._attributesOfAttributes = collectionsFactory.NewDictionaryProxy<Type, DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>>();
      }

      public DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>> AttributesByMarkingAttribute
      {
         get
         {
            return this._attributes;
         }
      }

      public DictionaryProxy<Type, DictionaryWithRoles<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>> AttributesOfAttributes
      {
         get
         {
            return this._attributesOfAttributes;
         }
      }

      public ListProxy<Attribute> AllAttributes
      {
         get
         {
            return this._allAttributes;
         }
      }


      public override String ToString()
      {
         String result = null;
         if ( this.NativeInfo != null )
         {
            result = String.Join( "\n", this._allAttributes.CQ.Select( attr => "[" + attr + "]" ) );
         }
         else
         {
            result = base.ToString();
         }
         return result;
      }
   }
}
