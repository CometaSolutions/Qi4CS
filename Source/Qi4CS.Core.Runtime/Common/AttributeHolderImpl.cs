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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CollectionsWithRoles.API;
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Core.Runtime.Common
{
   public class AttributeHolderImpl : AttributeHolder
   {

      private readonly DictionaryQuery<Type, ListQuery<Attribute>> _allAttributes;

      public AttributeHolderImpl(
         CollectionsFactory collectionsFactory,
         IEnumerable<MemberInfo> reflectedInfos
         )
      {
         Attribute[] attributes = reflectedInfos
            .SelectMany( info => info.GetCustomAttributes( true ) )
            .Distinct()
            .Cast<Attribute>()
            .ToArray();

         IDictionary<Type, ListProxy<Attribute>> dic = new Dictionary<Type, ListProxy<Attribute>>();
         foreach ( Attribute attr in attributes )
         {
            ListProxy<Attribute> list;
            if ( !dic.TryGetValue( attr.GetType(), out list ) )
            {
               list = collectionsFactory.NewListProxy<Attribute>();
               dic.Add( attr.GetType(), list );
            }
            list.Add( attr );
         }
         this._allAttributes = collectionsFactory.NewDictionary<Type, ListProxy<Attribute>, ListProxyQuery<Attribute>, ListQuery<Attribute>>( dic ).MQ.IQ;
      }

      #region AttributeHolder Members

      public CollectionsWithRoles.API.DictionaryQuery<Type, CollectionsWithRoles.API.ListQuery<Attribute>> AllAttributes
      {
         get
         {
            return this._allAttributes;
         }
      }

      #endregion
   }
}
