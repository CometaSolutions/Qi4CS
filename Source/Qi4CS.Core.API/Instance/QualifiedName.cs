/*
 * Copyright (c) 2009, Rickard Öberg.
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
using System;
using System.Reflection;
using System.Text;
using CommonUtils;

namespace Qi4CS.Core.API.Instance
{
   /// <summary>
   /// <para>
   /// <see cref="QualifiedName"/> represents some native element (e.g. <see cref="System.Reflection.PropertyInfo"/>) in such way that it can unambiguously reconstructed from string form.
   /// </para>
   /// </summary>
   /// <remarks>
   /// <para>
   /// A <see cref="QualifiedName"/> combines the name of e.g. some <see cref="System.Reflection.MemberInfo"/> and the name of its declaring type.
   /// In most Qi4CS usage scenarios one won't need to create instances of this class, as they are usually already created in <see cref="CompositeState"/> by Qi4CS runtime.
   /// </para>
   /// <para>
   /// <see cref="QualifiedName"/> consists of two parts, one being <see cref="QualifiedName.Type"/> which is assembly-qualified name of the declaring type.
   /// The other part is <see cref="QualifiedName.Name"/> which is the name of the property, event, or other reflection element.
   /// </para>
   /// <para>
   /// Instances of <see cref="QualifiedName"/> are immutable and may be treated as value objects, as <see cref="QualifiedName"/> overrides <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode()"/>, and implements <see cref="IComparable{T}"/>, <see cref="IComparable"/> and <see cref="IEquatable{T}"/>.
   /// This also means that instances of <see cref="QualifiedName"/> may be used as keys for <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> without any extra equality comparers.
   /// </para>
   /// </remarks>
   public sealed class QualifiedName : IComparable<QualifiedName>, IComparable, IEquatable<QualifiedName>
   {
      /// <summary>
      /// The separator between <see cref="QualifiedName.Type"/> and <see cref="QualifiedName.Name"/> in a string produced by <see cref="QualifiedName.ToString()"/> method.
      /// </summary>
      public const Char TYPE_NAME_PART_SEPARATOR = ':';

      /// <summary>
      /// The separator between nested types in <see cref="QualifiedName.Type"/> property.
      /// </summary>
      public const Char NESTED_TYPE_SEPARATOR = '+';

      /// <summary>
      /// The start mark of the assembly name in <see cref="QualifiedName.Type"/> property.
      /// </summary>
      public const Char ASSEMBLY_START = '[';

      /// <summary>
      /// The end mark of the assembly name in <see cref="QualifiedName.Type"/> property.
      /// </summary>
      public const Char ASSEMBLY_END = ']';

      /// <summary>
      /// The start mark of the generic type arguments in <see cref="QualifiedName.Type"/> property.
      /// </summary>
      public const Char GENERIC_ARGS_START = '<';

      /// <summary>
      /// The end mark of the generic type arguments in <see cref="QualifiedName.Type"/> property.
      /// </summary>
      public const Char GENERIC_ARGS_END = '>';

      private readonly String _type;
      private readonly String _name;
      //private readonly Lazy<String> _string;
      //private readonly Lazy<Int32> _hashCode;

      private QualifiedName( String type, String name )
      {
         ArgumentValidator.ValidateNotEmpty( "Type", type );
         ArgumentValidator.ValidateNotEmpty( "Name", name );

         this._type = type;
         this._name = name;
      }

      /// <summary>
      /// Gets the type part of this <see cref="QualifiedName"/>.
      /// </summary>
      /// <value>The type part of this <see cref="QualifiedName"/>.</value>
      /// <remarks>
      /// The type part always starts with <c>[</c> character, followed by full name of the assembly declaring the type, followed by <c>]</c> character.
      /// Then the namespace of the type is appended, with <c>.</c> as separator between namespace and type name, if namespace is not <c>null</c>.
      /// The nested types, if any, are separated by <c>+</c> character.
      /// Then the type name is appended.
      /// For the generic types, the type name is followed by <c>&lt;</c> character which is followed by the generic arguments separated with <c>", "</c> string, and finally the type part ends with <c>&gt;</c> character.
      /// </remarks>
      /// <example>
      /// <para>
      /// The type part of the <see cref="System.Object"/> will be <c>[mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.Object</c> in .NET 4/4.5 runtime.
      /// </para>
      /// <para>
      /// The type part of the <see cref="System.Collections.Generic.IList{T}">System.Collections.Generic.IList&lt;System.String&gt;</see> will be <c>[mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.Collections.Generic.IList`1&lt;[mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.String&gt;</c> in .NET 4/4.5 runtime.
      /// </para>
      /// </example>
      public String Type
      {
         get
         {
            return this._type;
         }
      }

      /// <summary>
      /// Gets the name part of this <see cref="QualifiedName"/>.
      /// </summary>
      /// <value>The name part of this <see cref="QualifiedName"/>.</value>
      public String Name
      {
         get
         {
            return this._name;
         }
      }

      /// <summary>
      /// Checks that given object is of type <see cref="QualifiedName"/> and has same <see cref="Type"/> and <see cref="Name"/> property values as this one.
      /// </summary>
      /// <param name="obj">The object to check equality against.</param>
      /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="QualifiedName"/> and has same <see cref="Type"/> and <see cref="Name"/> property values as this one; <c>false</c> otherwise.</returns>
      public override Boolean Equals( Object obj )
      {
         return this.Equals( obj as QualifiedName );
      }

      /// <summary>
      /// Gets the hash code based on <see cref="Type"/> and <see cref="Name"/> property values.
      /// </summary>
      /// <returns>The hash code based on <see cref="Type"/> and <see cref="Name"/> property values.</returns>
      public override Int32 GetHashCode()
      {
         return ( 17 * 23 + this._name.GetHashCodeSafe() ) * 23 + this._type.GetHashCodeSafe();
      }

      /// <summary>
      /// Returns the formal textual representation of this <see cref="QualifiedName"/>.
      /// It is the value of the <see cref="QualifiedName.Type"/> property appended by <c>:</c> character appended by the value of the <see cref="QualifiedName.Name"/> property.
      /// </summary>
      /// <returns>The formal textual representation of this <see cref="QualifiedName"/>.</returns>
      public override String ToString()
      {
         return this._type + TYPE_NAME_PART_SEPARATOR + this._name;
      }

      /// <summary>
      /// Creates a new instance of <see cref="QualifiedName"/> with given type and name parts.
      /// </summary>
      /// <param name="type">The type part for <see cref="QualifiedName"/>.</param>
      /// <param name="name">The name part for <see cref="QualifiedName"/></param>
      /// <returns>A new instance of <see cref="QualifiedName"/> with given type and name parts.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="type"/> or <paramref name="name"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If <paramref name="type"/> or <paramref name="name"/> is malformed (e.g. empty).</exception>
      public static QualifiedName FromStrings( String type, String name )
      {
         var result = new QualifiedName( type, name );
         var t = result.Type;
         var assEnd = t.IndexOf( ASSEMBLY_END );
         if ( t.IndexOf( ASSEMBLY_START ) != 0
             || assEnd == -1
             || assEnd == t.Length - 1 )
         {
            throw new ArgumentException( "Malformed type name: " + type + "." );
         }
         return result;
      }

      /// <summary>
      /// Creates a new isntance of <see cref="QualifiedName"/> from given <see cref="Type"/> and name.
      /// </summary>
      /// <param name="type">The native type from which construct the type part of the <see cref="QualifiedName"/>.</param>
      /// <param name="name">The name part of the <see cref="QualifiedName"/>.</param>
      /// <returns>A new isntance of <see cref="QualifiedName"/> from given <see cref="Type"/> and name.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="type"/> or <paramref name="name"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
      public static QualifiedName FromTypeAndName( Type type, String name )
      {
         return new QualifiedName( GetTypeName( type ), name );
      }

      /// <summary>
      /// Creates a new instance of <see cref="QualifiedName"/> from given <see cref="System.Reflection.MemberInfo"/>.
      /// </summary>
      /// <param name="info">The <see cref="System.Reflection.MemberInfo"/> to get the type and name parts from.</param>
      /// <returns>A new instance of <see cref="QualifiedName"/> from given <see cref="System.Reflection.MemberInfo"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="info"/> is <c>null</c>.</exception>
      public static QualifiedName FromMemberInfo( MemberInfo info )
      {
         ArgumentValidator.ValidateNotNull( "Member info", info );

         return new QualifiedName( GetTypeName( info.DeclaringType ), info.Name );
      }

      /// <summary>
      /// Gets the type part prefix for given <paramref name="type"/>.
      /// That is, an assembly part followed by namespace if it is not <c>null</c>.
      /// </summary>
      /// <param name="type">The <see cref="Type"/> to get type prefix from.</param>
      /// <returns>The type part prefix for <paramref name="type"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
      public static String GetTypePrefix( Type type )
      {
         var builder = new StringBuilder();
         GetTypeString( type, true, false, builder );
         return builder.ToString();
      }

      /// <summary>
      /// Gets the full type part for given <paramref name="type"/>.
      /// </summary>
      /// <param name="type">The <see cref="Type"/> to get type part from.</param>
      /// <returns>The full type part for given <paramref name="type"/>.</returns>
      /// <seealso cref="QualifiedName.Type"/>
      /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
      public static String GetTypeName( Type type )
      {
         var builder = new StringBuilder();
         GetTypeString( type, true, true, builder );
         return builder.ToString();
      }

      /// <summary>
      /// Gets the type part suffix for given <paramref name="type"/>.
      /// That is, a full type part without the assembly name.
      /// </summary>
      /// <param name="type">The <see cref="Type"/> to get type part suffix from.</param>
      /// <param name="nestedSeparator">The separator to use for nested types, default is <c>+</c>.</param>
      /// <returns>The type part suffix for given <paramref name="type"/>.</returns>
      /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
      public static String GetBareTypeName( Type type, Char nestedSeparator = NESTED_TYPE_SEPARATOR )
      {
         var builder = new StringBuilder();
         GetTypeString( type, false, true, builder, nestedSeparator );
         return builder.ToString();
      }

      private static void GetTypeString( Type type, Boolean appendAssembly, Boolean appendTypeName, StringBuilder builder, Char nestedSeparator = NESTED_TYPE_SEPARATOR )
      {
         ArgumentValidator.ValidateNotNull( "Type", type );

         if ( appendAssembly )
         {
            builder.Append( ASSEMBLY_START ).Append( type.Assembly.FullName ).Append( ASSEMBLY_END );
         }

         var ns = type.Namespace;
         if ( ns != null )
         {
            builder.Append( ns ).Append( "." );
         }

         if ( appendTypeName )
         {
            ProcessDeclaringType( type, builder, nestedSeparator );
            builder.Append( type.Name );
            if ( type.IsGenericType )
            {
               var gArgs = type.GetGenericArguments();
               builder.Append( GENERIC_ARGS_START );
               for ( var idx = 0; idx < gArgs.Length; ++idx )
               {
                  GetTypeString( gArgs[idx], appendAssembly, appendTypeName, builder );
                  if ( idx < gArgs.Length - 1 )
                  {
                     builder.Append( ", " );
                  }
               }
               builder.Append( GENERIC_ARGS_END );
            }
         }
      }

      private static void ProcessDeclaringType( Type type, StringBuilder builder, Char nestedSeparator )
      {
         if ( !type.IsGenericParameter )
         {
            var declType = type.DeclaringType;
            if ( declType != null )
            {
               ProcessDeclaringType( declType, builder, nestedSeparator );
               builder
                  .Append( type.Name )
                  .Append( nestedSeparator );
            }
         }
      }

      #region IComparable<QualifiedName> Members

      /// <inheritdoc />
      public Int32 CompareTo( QualifiedName other )
      {
         return this.ToString().CompareTo( other.ToStringSafe() );
      }

      #endregion

      Int32 IComparable.CompareTo( Object obj )
      {
         return this.CompareTo( obj as QualifiedName );
      }

      /// <inheritdoc />
      public Boolean Equals( QualifiedName other )
      {
         return Object.ReferenceEquals( this, other ) ||
            ( Object.Equals( this._type, other._type )
              && Object.Equals( this._name, other._name )
            );
      }
   }
}
