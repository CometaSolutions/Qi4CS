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
using Qi4CS.Core.SPI.Model;

namespace Qi4CS.Core.Runtime.Model
{
   /// <summary>
   /// TODO: consider making register/unregister methods internal. Thus it would not be possible to register/unregister stuff for a model, removing the need to reset validation result. Also making it a bit more robust (one could unregister something after application instance is created).
   /// </summary>
   public class InjectionServiceImpl : InjectionService
   {
      private readonly IDictionary<Type, InjectionFunctionality> _functionalities;

      /// <summary>
      /// TODO: refactor some stuff so that this ctor could just accept DictionaryQuery<Type, InjectionFunctionality> .
      /// </summary>
      public InjectionServiceImpl()
      {
         this._functionalities = new Dictionary<Type, InjectionFunctionality>();
      }

      #region InjectionService Members

      public Boolean HasFunctionalityFor( Attribute attr )
      {
         return attr != null && this._functionalities.ContainsKey( attr.GetType() );
      }

      #endregion

      #region InjectionFunctionality Members

      public ValidationResult InjectionPossible( SPI.Model.AbstractInjectableModel model )
      {
         var targetType = model.TargetType;
         var scope = model.InjectionScope;
         ValidationResult result;
         InjectionFunctionality func;
         if ( scope != null && this._functionalities.TryGetValue( scope.GetType(), out func ) )
         {
            result = func.InjectionPossible( model );
         }
         else
         {
            result = new ValidationResult( false, scope == null ?
               "Injection scope was null" :
               ( "No injection functionality found for injection scope " + scope.GetType() ) );
         }
         return result;
      }

      public Object ProvideInjection( SPI.Instance.CompositeInstance instance, AbstractInjectableModel model, Type targetType )
      {
         // Code generation will use Lazy<> by itself if needed, don't do check here.
         Object result;
         InjectionFunctionality func;
         var scope = model.InjectionScope;
         if ( scope != null && this._functionalities.TryGetValue( scope.GetType(), out func ) )
         {
            result = func.ProvideInjection( instance, model, targetType );
         }
         else
         {
            result = null;
         }
         return result;
      }

      public InjectionTime GetInjectionTime( SPI.Model.AbstractInjectableModel model )
      {
         InjectionFunctionality func;
         var scope = model.InjectionScope;
         if ( scope != null && this._functionalities.TryGetValue( scope.GetType(), out func ) )
         {
            return func.GetInjectionTime( model );
         }
         else
         {
            return InjectionTime.ON_CREATION;
         }
      }

      #endregion

      internal void RegisterInjectionFunctionality( params Tuple<Type, InjectionFunctionality>[] injectionFunctionalityInfos )
      {
         foreach ( var info in injectionFunctionalityInfos.Where( i => i != null && i.Item1 != null && i.Item2 != null && !Object.ReferenceEquals( this, i.Item2 ) ) )
         {
            foreach ( var parent in info.Item1.GetAllParentTypes() )
            {
               if ( !Object.Equals( typeof( Attribute ), parent ) && !Object.Equals( typeof( Object ), parent ) )
               {
                  this._functionalities[parent] = info.Item2;
               }
            }
         }
      }

      internal void UnregisterInjectionFunctionality( params Type[] scopes )
      {
         foreach ( var scope in scopes.Where( s => s != null ) )
         {
            InjectionFunctionality func;
            if ( this._functionalities.TryGetValue( scope, out func ) )
            {
               foreach ( var kvp in this._functionalities )
               {
                  if ( Object.ReferenceEquals( kvp.Value, scope ) )
                  {
                     this._functionalities.Remove( kvp.Key );
                  }
               }
            }
         }
      }
   }
}
