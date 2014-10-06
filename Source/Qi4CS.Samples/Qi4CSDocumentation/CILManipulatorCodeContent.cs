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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CILAssemblyManipulator.API;
using Qi4CS.Core.API.Instance;
using Qi4CS.CodeGeneration.DotNET;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;

#pragma warning disable 169,649
namespace Qi4CS.Samples.Qi4CSDocumentation
{
   internal class CILReflectionContextCode
   {
      private void Dummy()
      {
         #region CILReflectionContextCode1
         using ( var ctx = CILReflectionContextFactory.NewContext( null, null ) )
         {
            // Add event handlers to various events of ctx

            // Then use the ctx, typically meaning emitting some code (possibly in parallel)

            var assembly = ctx.NewBlankAssembly( "MyAssembly" );
            var module = assembly.AddModule( "MyAssembly" );
            var type = module.AddType( "MyType", TypeAttributes.Public | TypeAttributes.Class );

            // Etc...
         }
         #endregion
      }
   }

   internal class MethodILCode
   {
      private class FieldType { }
      private FieldType _field;

      private void Dummy( FieldType valueToSet )
      {
         #region MethodILCode1
         var current = this._field;
         FieldType oldCurrent;
         do
         {
            oldCurrent = current;
            var newValue = valueToSet; // 'valueToSet' is what is emitted by calling emitNewValueOnTopOfStack
            current = Interlocked.CompareExchange( ref this._field, newValue, oldCurrent );
         } while ( !Object.ReferenceEquals( current, oldCurrent ) ); // replace ReferenceEquals with OpCodes.Bne_Un_S.
         #endregion
      }

      private void Dummy2( FieldType valueToSet )
      {
         #region MethodILCode2
         var current = this._field;
         FieldType oldCurrent;
         do
         {
            oldCurrent = current; // If loadCurrent action is not null, then 'current' will be what is emitted by calling loadCurrent
            var newValue = valueToSet; // 'valueToSet' is what is emitted by calling emitNewValueOnTopOfStack
            current = Interlocked.CompareExchange( ref this._field, newValue, oldCurrent );
         } while ( !Object.ReferenceEquals( current, oldCurrent ) ); // replace ReferenceEquals with OpCodes.Bne_Un_S.
         #endregion
      }
   }

   internal class DotNetReflectionContextCode
   {
      private Boolean shouldGenerateCode;

      private void Dummy()
      {
         #region DotNetReflectionContextCode1
         var applicationName = "MyApp"; // Custom defined application name
         var path = System.IO.Path.Combine( Environment.CurrentDirectory, "Qi4CS", applicationName );

         // Create Qi4CS architecture and model, assume model is in variable called 'model'
         var model = CreateQi4CSApplicationModel();

         Application qi4csApp;
         using ( new Qi4CSAssemblyResolver( path ) )
         {
            // Assume that we have a boolean variable 'shouldGenerateCode' which tells whether we should generate code before creating Qi4CS application instance
            if ( shouldGenerateCode )
            {
               // Pass 'null's as application mode and version in this example
               qi4csApp = model.NewInstance( applicationName, null, null );
            }
         }

         #endregion
      }

      private static ApplicationModel<ApplicationSPI> CreateQi4CSApplicationModel()
      {
         return null;
      }
   }

   #region EmitTypeDefOrTypeSpec

   public class ClassToBeEmitted<T>
   {
      public void MethodToBeEmitted()
      {
         var type1 = typeof( ClassToBeEmitted<> );
         var type2 = typeof( ClassToBeEmitted<T> );
      }
   }

   #endregion
}
#pragma warning restore 169,649