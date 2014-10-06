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
using System.Collections.Generic;
using System.Reflection;
using CollectionsWithRoles.API;
using Qi4CS.Core.API.Instance;

namespace Qi4CS.Core.Runtime.Instance
{
   public class CompositeStateImpl : CompositeState
   {
      private readonly DictionaryQuery<QualifiedName, CompositeProperty> _properties;
      private readonly DictionaryQuery<QualifiedName, CompositeEvent> _events;
      private readonly DictionaryQuery<MethodInfo, QualifiedName> _qNames;

      public CompositeStateImpl( CollectionsFactory collectionFactory, ListQuery<CompositeProperty> properties, ListQuery<CompositeEvent> events )
      {
         IDictionary<MethodInfo, QualifiedName> qNames = new Dictionary<MethodInfo, QualifiedName>();
         IDictionary<QualifiedName, CompositeProperty> propertiesDic = new Dictionary<QualifiedName, CompositeProperty>();
         IDictionary<QualifiedName, CompositeEvent> eventsDic = new Dictionary<QualifiedName, CompositeEvent>();
         foreach ( CompositeProperty property in properties )
         {
            PropertyInfo pInfo = property.ReflectionInfo;
            QualifiedName qName = property.QualifiedName;
            qNames.Add( pInfo.GetGetMethod(), qName );
            qNames.Add( pInfo.GetSetMethod(), qName );
            propertiesDic.Add( qName, property );
         }
         foreach ( CompositeEvent evt in events )
         {
            EventInfo eInfo = evt.ReflectionInfo;
            QualifiedName qName = evt.QualifiedName;
            qNames.Add( eInfo.GetAddMethod(), qName );
            qNames.Add( eInfo.GetRemoveMethod(), qName );
            eventsDic.Add( qName, evt );
         }

         this._qNames = collectionFactory.NewDictionaryProxy( qNames ).CQ;
         this._properties = collectionFactory.NewDictionaryProxy( propertiesDic ).CQ;
         this._events = collectionFactory.NewDictionaryProxy( eventsDic ).CQ;
      }

      #region CompositeState Members

      public DictionaryQuery<QualifiedName, CompositeProperty> Properties
      {
         get
         {
            return this._properties;
         }
      }

      public DictionaryQuery<MethodInfo, QualifiedName> QualifiedNamesForMethods
      {
         get
         {
            return this._qNames;
         }
      }

      public DictionaryQuery<QualifiedName, CompositeEvent> Events
      {
         get
         {
            return this._events;
         }
      }

      #endregion
   }
}
