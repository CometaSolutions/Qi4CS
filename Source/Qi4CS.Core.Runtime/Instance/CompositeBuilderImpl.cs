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
using System.Collections.Generic;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.SPI.Common;

namespace Qi4CS.Core.Runtime.Instance
{
   public class CompositeBuilderImpl : CompositeBuilder
   {

      //      private readonly IEnumerable<Type> _compositeTypes;
      private readonly CompositeInstanceImpl _instance;
      private readonly UsesContainerMutable _usesContainer;

      public CompositeBuilderImpl( IEnumerable<Type> actualCompositeTypes, UsesContainerMutable usings, CompositeInstanceImpl compositeInstance )
      {
         //         this._compositeTypes = actualCompositeTypes;
         this._usesContainer = usings;
         this._instance = compositeInstance;
      }

      #region CompositeBuilder<CompositeType> Members

      //public TComposite NewCopyFromPrototype()
      //{
      //   return this.NewCopyFromPrototype( null );
      //}

      //public virtual TComposite NewCopyFromPrototype( Action<CompositeBuilder<TComposite>> initAction )
      //{
      //   this.ThrowIfNotPrototype( "This composite state is no longer a prototype, therefore creating a copy is not allowed." );

      //   UsesContainerMutable newUses;
      //   CompositeInstanceImpl newInstance = this._instance.Clone( this._compositeTypes, out newUses );
      //   CompositeBuilder<TComposite> newBuilder = new CompositeBuilderImpl<TComposite>(
      //      this._compositeTypes,
      //      newUses,
      //      newInstance
      //      );
      //   if ( initAction != null )
      //   {
      //      initAction( newBuilder );
      //   }
      //   return newBuilder.Instantiate();
      //}

      public Object InstantiateWithType( Type other )
      {
         this._instance.DisablePrototype( -1, null, null );
         return this._instance.GetCompositeForType( other );
      }

      public Object PrototypeFor( Type prototypeType )
      {
         this.ThrowIfNotPrototype();
         return this._instance.Composites[prototypeType];
      }

      public CompositeBuilder Use( params Object[] objects )
      {
         this._usesContainer.Use( objects );
         return this;
      }

      public CompositeBuilder UseWithName( String name, Object value )
      {
         this._usesContainer.UseWithName( name, value );
         return this;
      }

      #endregion

      protected void ThrowIfNotPrototype()
      {
         this.ThrowIfNotPrototype( "The composite has been created and is no longer a prototype." );
      }

      protected void ThrowIfNotPrototype( String msg )
      {
         if ( !this._instance.IsPrototype )
         {
            throw new InvalidOperationException( msg );
         }
      }
   }
}
