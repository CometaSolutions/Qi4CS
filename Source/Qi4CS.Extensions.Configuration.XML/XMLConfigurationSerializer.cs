/*
 * Copyright 2014 Stanislav Muhametsin. All rights Reserved.
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
using System.Text;
using Qi4CS.Extensions.Configuration.Instance;
using System.Reflection;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Instance;
using System.Xml.Linq;
using CommonUtils;
using System.IO;
using System.Xml.XPath;

namespace Qi4CS.Extensions.Configuration.XML
{
   /// <summary>
   /// This enumeration describes whether XML serializer requests stream for serializing or deserializing.
   /// </summary>
   public enum SerializationAction
   {
      /// <summary>
      /// XML serializer requests stream for serializing.
      /// </summary>
      SERIALIZING,

      /// <summary>
      /// XML serializer requests stream for deserializing.
      /// </summary>
      DESERIALIZING
   }

   /// <summary>
   /// This is callback which will be used to get stream to write data to when serializing or deserializing.
   /// </summary>
   public interface StreamCallback
   {
      /// <summary>
      /// This method should check whether it is possible to serialize or deserialize given configuration to given resource (usually file name).
      /// </summary>
      /// <param name="resource">The textual resource identifier, usually file path.</param>
      /// <param name="serializationAction">Whether to serialize or deserialize the configuration.</param>
      /// <param name="stream">If <c>true</c> is returned, then this should contain the stream which can be read or written, depending on whether deserializing or serializing process is in question. If return value is not <c>true</c>, this value is ignored.</param>
      /// <returns><c>true</c> if it is possible to serialize or deserialize to given <paramref name="resource"/>; <c>false</c> otherwise.</returns>
      Boolean TryGetStreamFor( String resource, SerializationAction serializationAction, out Stream stream );
   }


   internal class XMLConfigurationSerializer : ConfigurationSerializer
   {

      //      private static readonly PropertyInfo LIST_ITEM_PROPERTY = TypeUtil.LoadPropertyOrThrow( typeof( IList<> ), "Item" );
      //      private static readonly MethodInfo LIST_ITEM_GETTER = LIST_ITEM_PROPERTY.GetGetMethod();
      //      private static readonly MethodInfo LIST_ITEM_SETTER = LIST_ITEM_PROPERTY.GetSetMethod();
      private static readonly ConstructorInfo LIST_CTOR = typeof( List<> ).LoadConstructorOrThrow( 0 );
      private static readonly MethodInfo LIST_ADD_METHOD = typeof( ICollection<> ).LoadMethodOrThrow( "Add", 1 );

      private static readonly PropertyInfo DIC_ITEM_PROPERTY = typeof( IDictionary<,> ).LoadPropertyOrThrow( "Item" );
      private static readonly MethodInfo DIC_ITEM_GETTER = DIC_ITEM_PROPERTY.GetGetMethod();
      //      private static readonly MethodInfo DIC_ITEM_SETTER = DIC_ITEM_PROPERTY.GetSetMethod();
      private static readonly ConstructorInfo DIC_CTOR = typeof( Dictionary<,> ).LoadConstructorOrThrow( 0 );
      private static readonly MethodInfo DIC_ADD_METHOD = typeof( IDictionary<,> ).LoadMethodOrThrow( "Add", 2 );

      private static readonly PropertyInfo DIC_KEYS_PROPERTY = typeof( IDictionary<,> ).LoadPropertyOrThrow( "Keys" );
      private static readonly MethodInfo DIC_KEYS_GETTER = DIC_KEYS_PROPERTY.GetGetMethod();

#pragma warning disable 649

      [Uses]
      private StreamCallback _streamCallback;

      [Structure]
      private StructureServiceProvider _ssp;

      [Structure]
      private ApplicationSPI _application;

      [Uses]
      private IList<XMLConfigurationSerializerHelper> _customSerializers;

#pragma warning restore 649

      public virtual Object Deserialize( Type type, Qi4CSConfigurationResource resource )
      {
         var xmlResource = (XMLConfigurationResource) resource;

         Stream fs; XDocument doc;
         if ( this._streamCallback.TryGetStreamFor( xmlResource.DocumentResource, SerializationAction.DESERIALIZING, out fs ) )
         {
            using ( fs )
            {
               doc = XDocument.Load( fs );
            }
         }
         else
         {
            throw new NotSupportedException( "Can not get stream for " + xmlResource.DocumentResource + "." );
         }

         XElement element;
         var xpath = xmlResource.XPath;
         if ( String.IsNullOrEmpty( xpath ) )
         {
            element = doc.Root;
         }
         else
         {
            // Get the actual element to deserialize from
            element = doc.XPathSelectElement( xpath );
            CheckForXPathElement( xpath, element );
         }
         Boolean dummy;
         return this.Deserialize( type, element, null, out dummy );
      }

      public virtual void Serialize( Object contents, Qi4CSConfigurationResource resource )
      {
         var xmlResource = (XMLConfigurationResource) resource;

         Stream fs;

         XDocument existingFullDoc = null;
         var xpath = xmlResource.XPath;
         if ( !String.IsNullOrEmpty( xpath ) )
         {
            if ( this._streamCallback.TryGetStreamFor( xmlResource.DocumentResource, SerializationAction.DESERIALIZING, out fs ) )
            {
               using ( fs )
               {
                  existingFullDoc = XDocument.Load( fs );
               }
            }
            else
            {
               throw new NotSupportedException( "Can not get stream for " + xmlResource.DocumentResource + "." );
            }
         }

         if ( this._streamCallback.TryGetStreamFor( xmlResource.DocumentResource, SerializationAction.SERIALIZING, out fs ) )
         {
            using ( fs )
            {
               var existingElement = existingFullDoc == null ? null : existingFullDoc.XPathSelectElement( xpath );
               if ( existingFullDoc != null )
               {
                  CheckForXPathElement( xpath, existingElement );
               }

               var type = this._application.GetCompositeInstance( contents ).ModelInfo.Model.PublicTypes.First();
               var element = this.Serialize( contents, type, existingElement == null ? type.Name : existingElement.Name.LocalName, null );
               if ( existingElement == null )
               {
                  element.Save( fs );
               }
               else
               {
                  existingElement.ReplaceWith( element );
                  existingFullDoc.Save( fs );
               }
            }
         }
         else
         {
            throw new NotSupportedException( "Can not get stream for " + xmlResource.DocumentResource + "." );
         }
      }

      private Object Deserialize( Type type, XElement element, Object curVal, out Boolean replace )
      {
         replace = true;
         if ( type.IsNullable( out type ) && String.IsNullOrEmpty( element.Value ) )
         {
            return null;
         }
         else if ( type.IsEnum )
         {
            return Enum.Parse( type, element.Value, true );
         }
         else
         {
            switch ( Type.GetTypeCode( type ) )
            {
               case TypeCode.Boolean:
                  Boolean parsedBool;
                  return Boolean.TryParse( element.Value, out parsedBool ) && parsedBool;
               case TypeCode.Byte:
                  return Byte.Parse( element.Value );
               case TypeCode.Char:
                  Char c;
                  if ( Char.TryParse( element.Value, out c ) )
                  {
                     return c;
                  }
                  else
                  {
                     throw new InvalidOperationException( "Failed to parse " + element.Value + " as character." );
                  }
               case TypeCode.DateTime:
                  return DateTime.Parse( element.Value );
               case TypeCode.Decimal:
                  return Decimal.Parse( element.Value );
               case TypeCode.Double:
                  return Double.Parse( element.Value );
               case TypeCode.Int16:
                  return Int16.Parse( element.Value );
               case TypeCode.Int32:
                  return Int32.Parse( element.Value );
               case TypeCode.Int64:
                  return Int64.Parse( element.Value );
               case TypeCode.SByte:
                  return SByte.Parse( element.Value );
               case TypeCode.Single:
                  return Single.Parse( element.Value );
               case TypeCode.String:
                  return element.Value;
               case TypeCode.UInt16:
                  return UInt16.Parse( element.Value );
               case TypeCode.UInt32:
                  return UInt32.Parse( element.Value );
               case TypeCode.UInt64:
                  return UInt64.Parse( element.Value );
               case TypeCode.Object:
                  if ( typeof( System.Text.RegularExpressions.Regex ).Equals( type ) )
                  {
                     return new System.Text.RegularExpressions.Regex( element.Value );
                  }
                  // TODO parse XElement here.
                  else
                  {
                     var isGeneric = type.IsGenericType;
                     Object result;
                     if ( type.IsArray && type.GetArrayRank() == 1 )
                     {
                        var itemType = type.GetElementType();
                        var len = element.Elements().Count();

                        replace = curVal == null;
                        result = curVal ?? Array.CreateInstance( itemType, len );

                        var idx = 0;
                        foreach ( var el in element.Elements() )
                        {
                           Boolean dummy;
                           ( (Array) result ).SetValue( this.Deserialize( itemType, el, null, out dummy ), idx );
                           ++idx;
                        }
                     }
                     else if ( isGeneric && typeof( IList<> ).Equals( type.GetGenericTypeDefinition() ) )
                     {
                        var itemType = type.GetGenericArguments()[0];

                        replace = curVal == null;
                        result = curVal ?? ( (ConstructorInfo) MethodBase.GetMethodFromHandle( LIST_CTOR.MethodHandle, typeof( List<> ).MakeGenericType( itemType ).TypeHandle ) )
                           .Invoke( null );

                        var addMethod = MethodBase.GetMethodFromHandle( LIST_ADD_METHOD.MethodHandle, type.TypeHandle );

                        foreach ( var el in element.Elements() )
                        {
                           Boolean dummy;
                           addMethod.Invoke( result, new[] { this.Deserialize( itemType, el, null, out dummy ) } );
                        }
                     }
                     else if ( isGeneric && typeof( IDictionary<,> ).Equals( type.GetGenericTypeDefinition() ) )
                     {
                        var itemType = type.GetGenericArguments()[1];

                        replace = curVal == null;
                        result = curVal ?? ( (ConstructorInfo) MethodBase.GetMethodFromHandle( DIC_CTOR.MethodHandle, typeof( Dictionary<,> ).MakeGenericType( typeof( String ), itemType ).TypeHandle ) )
                           .Invoke( null );

                        var addMethod = MethodBase.GetMethodFromHandle( DIC_ADD_METHOD.MethodHandle, type.TypeHandle );
                        foreach ( var el in element.Elements() )
                        {
                           Boolean dummy;
                           addMethod.Invoke( result, new[] { el.Name.LocalName, this.Deserialize( itemType, el, null, out dummy ) } );
                        }
                     }
                     else
                     {
                        // Check custom serializers
                        result = null;
                        var deserialized = false;
                        foreach ( var cs in this._customSerializers )
                        {
                           if ( cs.CanDeserialize( element, type ) )
                           {
                              result = cs.Deserialize( element, type );
                              deserialized = true;
                              break;
                           }
                        }

                        if ( !deserialized )
                        {
                           // Parse recursively all content.
                           var resultBuilder = this._ssp.NewPlainCompositeBuilder( type );
                           var proto = resultBuilder.PrototypeFor( type );
                           foreach ( var prop in type.GetAllParentTypes().SelectMany( t => t.GetProperties( System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public ) ) )
                           {
                              var propElement = element.Element( prop.Name );
                              if ( propElement != null )
                              {
                                 Boolean replaceThis;
                                 var newVal = this.Deserialize( prop.PropertyType, propElement, prop.GetGetMethod().Invoke( proto, null ), out replaceThis );
                                 if ( replaceThis )
                                 {
                                    prop.GetSetMethod().Invoke( proto, new[] { newVal } );
                                 }
                              }
                           }
                           result = resultBuilder.InstantiateWithType( type );
                        }
                     }
                     return result;
                  }
               default:
                  throw new NotSupportedException( "Unsupported type code: " + Type.GetTypeCode( type ) + "." );
            }
         }
      }

      private XElement Serialize( Object obj, Type type, String elName, PropertyInfo currentProperty )
      {
         XElement result;
         if ( obj != null )
         {
            result = new XElement( elName );

            if ( typeof( System.Text.RegularExpressions.Regex ).Equals( type ) )
            {
               type = typeof( String );
            }

            type.IsNullable( out type );

            switch ( Type.GetTypeCode( type ) )
            {
               case TypeCode.Boolean:
               case TypeCode.Byte:
               case TypeCode.Char:
               case TypeCode.DateTime:
               case TypeCode.Decimal:
               case TypeCode.Double:
               case TypeCode.Int16:
               case TypeCode.Int32:
               case TypeCode.Int64:
               case TypeCode.SByte:
               case TypeCode.Single:
               case TypeCode.UInt16:
               case TypeCode.UInt32:
               case TypeCode.UInt64:
                  result.Value = obj.ToStringSafe();
                  break;
               case TypeCode.String:
                  result.Value = obj.ToStringSafe();
                  break;
               case TypeCode.Object:
                  var isGeneric = type.IsGenericType;

                  if ( type.IsArray && type.GetArrayRank() == 1 )
                  {
                     var itemType = type.GetElementType();
                     foreach ( var item in (Array) obj )
                     {
                        result.AddIfNotNull( this.Serialize( item, itemType, GetItemTypeName( itemType, currentProperty ), null ) );
                     }
                  }
                  else if ( isGeneric && typeof( IList<> ).Equals( type.GetGenericTypeDefinition() ) )
                  {
                     var itemType = type.GetGenericArguments()[0];
                     foreach ( var item in (System.Collections.IEnumerable) obj )
                     {
                        result.AddIfNotNull( this.Serialize( item, itemType, GetItemTypeName( itemType, currentProperty ), null ) );
                     }
                  }
                  else if ( isGeneric && typeof( IDictionary<,> ).Equals( type.GetGenericTypeDefinition() ) )
                  {
                     var itemType = type.GetGenericArguments()[1];
                     var itemGetter = MethodBase.GetMethodFromHandle( DIC_ITEM_GETTER.MethodHandle, type.TypeHandle );
                     foreach ( var key in (System.Collections.IEnumerable) MethodBase.GetMethodFromHandle( DIC_KEYS_GETTER.MethodHandle, type.TypeHandle ).Invoke( obj, null ) )
                     {
                        result.AddIfNotNull( this.Serialize( itemGetter.Invoke( obj, new[] { key } ), itemType, key.ToString(), null ) );
                     }
                  }
                  else
                  {
                     // Check custom serializers
                     var serialized = false;
                     foreach ( var cs in this._customSerializers )
                     {
                        if ( cs.CanSerialize( obj, type ) )
                        {
                           cs.Serialize( obj, type, result );
                           serialized = true;
                           break;
                        }
                     }

                     if ( !serialized )
                     {
                        // Serialize recursively all properties
                        foreach ( var prop in type.GetAllParentTypes().SelectMany( t => t.GetProperties( BindingFlags.Instance | BindingFlags.Public ) ) )
                        {
                           result.AddIfNotNull( this.Serialize( prop.GetGetMethod().Invoke( obj, null ), prop.PropertyType, prop.Name, prop ) );
                        }
                     }
                  }
                  break;
               default:
                  throw new NotSupportedException( "Unsupported type code: " + Type.GetTypeCode( type ) + "." );
            }
         }
         else
         {
            result = null;
         }

         return result;
      }

      private static String GetItemTypeName( Type itemType, PropertyInfo currentProperty )
      {
         //var ca = currentProperty == null ? null : currentProperty.GetCustomAttributes( true ).OfType<ListOrArrayElementNameAttribute>().FirstOrDefault();
         return /*ca == null || String.IsNullOrEmpty( ca.ElementName ) ?*/ itemType.Name/* : ca.ElementName*/;
      }

      private static void CheckForXPathElement( String xpath, XElement element )
      {
         if ( element == null )
         {
            throw new NotSupportedException( "Could not find element with XPath \"" + xpath + "\"." );
         }
      }
   }

   /// <summary>
   /// This interface provides required callbacks for XML configuration serializer to serialize custom types.
   /// Typically each instance of this interface is provided for each type that needs to be serialized.
   /// </summary>
   public interface XMLConfigurationSerializerHelper
   {
      /// <summary>
      /// Checks whether this <see cref="XMLConfigurationSerializerHelper"/> can serialize given object.
      /// </summary>
      /// <param name="obj">The object to serialize.</param>
      /// <param name="type">The type of the object.</param>
      /// <returns><c>true</c> if this <see cref="XMLConfigurationSerializerHelper"/> can serialize given object; <c>false</c> otherwise.</returns>
      Boolean CanSerialize( Object obj, Type type );

      /// <summary>
      /// Performs serialization of the object with given parent <see cref="XElement"/>.
      /// </summary>
      /// <param name="obj">The object to serialize.</param>
      /// <param name="type">The type of the object.</param>
      /// <param name="parent">The parent <see cref="XElement"/>.</param>
      void Serialize( Object obj, Type type, XElement parent );

      /// <summary>
      /// Checks whether this <see cref="XMLConfigurationSerializerHelper"/> can deserialize object of given type from given <see cref="XElement"/>.
      /// </summary>
      /// <param name="element">The <see cref="XElement"/> containing serialized data.</param>
      /// <param name="type">The deserialized object type.</param>
      /// <returns><c>true</c> if this <see cref="XMLConfigurationSerializerHelper"/> can deserialize object of given type from given <see cref="XElement"/>; <c>false</c> otherwise.</returns>
      Boolean CanDeserialize( XElement element, Type type );

      /// <summary>
      /// Performs deserialization of the given <see cref="XElement"/> into object of given type.
      /// </summary>
      /// <param name="element">The <see cref="XElement"/> containing serialized data.</param>
      /// <param name="type">The type of the object.</param>
      /// <returns>Deserialized object of given type.</returns>
      Object Deserialize( XElement element, Type type );
   }

   /// <summary>
   /// This is helper class implementing <see cref="XMLConfigurationSerializerHelper"/> but performing all of its functionality through callbacks provided to constructor.
   /// </summary>
   public sealed class XMLConfigurationSerializerWithCallbacks : XMLConfigurationSerializerHelper
   {
      private readonly Func<Object, Type, Boolean> _canSerialize;
      private readonly Action<Object, Type, XElement> _serialize;
      private readonly Func<XElement, Type, Boolean> _canDeserialize;
      private readonly Func<XElement, Type, Object> _deserialize;

      /// <summary>
      /// Creates new instance of <see cref="XMLConfigurationSerializerWithCallbacks"/> with given callbacks.
      /// </summary>
      /// <param name="canSerialize">The implementation for <see cref="XMLConfigurationSerializerHelper.CanSerialize"/> method.</param>
      /// <param name="serialize">The implementation for <see cref="XMLConfigurationSerializerHelper.Serialize"/> method.</param>
      /// <param name="canDeserialize">The implementation for <see cref="XMLConfigurationSerializerHelper.CanDeserialize"/> method.</param>
      /// <param name="deserialize">The implementation for <see cref="XMLConfigurationSerializerHelper.Deserialize"/> method.</param>
      /// <exception cref="ArgumentNullException">If any of <paramref name="canSerialize"/>, <paramref name="serialize"/>, <paramref name="canDeserialize"/>, or <paramref name="deserialize"/> is <c>null</c>.</exception>
      public XMLConfigurationSerializerWithCallbacks(
         Func<Object, Type, Boolean> canSerialize,
         Action<Object, Type, XElement> serialize,
         Func<XElement, Type, Boolean> canDeserialize,
         Func<XElement, Type, Object> deserialize
         )
      {
         ArgumentValidator.ValidateNotNull( "Serialization check callback", canSerialize );
         ArgumentValidator.ValidateNotNull( "Serialization callback", serialize );
         ArgumentValidator.ValidateNotNull( "Deserialization check callback", canDeserialize );
         ArgumentValidator.ValidateNotNull( "Deserialization callback", deserialize );

         this._canSerialize = canSerialize;
         this._serialize = serialize;
         this._canDeserialize = canDeserialize;
         this._deserialize = deserialize;
      }

      /// <inheritdoc />
      public Boolean CanSerialize( Object obj, Type type )
      {
         return this._canSerialize( obj, type );
      }

      /// <inheritdoc />
      public void Serialize( Object obj, Type type, XElement parent )
      {
         this._serialize( obj, type, parent );
      }

      /// <inheritdoc />
      public Boolean CanDeserialize( XElement element, Type type )
      {
         return this._canDeserialize( element, type );
      }

      /// <inheritdoc />
      public Object Deserialize( XElement element, Type type )
      {
         return this._deserialize( element, type );
      }
   }

   /// <summary>
   /// This is default implementation for <see cref="StreamCallback"/> that will use file system to open streams.
   /// </summary>
   public class DotNETStreamHelper : StreamCallback
   {
      /// <inheritdoc />
      public Boolean TryGetStreamFor( String resource, SerializationAction serializationAction, out Stream stream )
      {
         var retVal = SerializationAction.SERIALIZING == serializationAction ? true : File.Exists( resource );
         var isSerializing = SerializationAction.SERIALIZING == serializationAction;
         stream = retVal ? new FileStream(
            resource,
            isSerializing ? FileMode.Create : FileMode.Open,
            isSerializing ? FileAccess.Write : FileAccess.Read,
            isSerializing ? FileShare.None : FileShare.Read ) : null;
         return retVal;
      }
   }
}

public static partial class E_Qi4CSConfigurationXML
{
   internal static void AddIfNotNull( this XContainer container, XElement element )
   {
      if ( element != null )
      {
         container.Add( element );
      }
   }
}
