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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CollectionsWithRoles.API;
using NUnit.Framework;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Instance;
using Qi4CS.Core.SPI.Model;


namespace Qi4CS.Tests
{
   [Serializable]
   public abstract class AbstractModelTest
   {
      protected Int32 _injErrors;
      protected Int32 _structErrors;
      protected Int32 _internalErrors;

      protected void PerformTest( Int32 injectionErrors, Int32 structureErrors, Int32 internalErrors )
      {
         using ( var ad = Qi4CSTestUtils.CreateTestAppDomain( "Qi4CS Model Test" ) )
         {
            this._injErrors = injectionErrors;
            this._structErrors = structureErrors;
            this._internalErrors = internalErrors;

            ad.DoCallBack( () =>
            {
               var model = this.CreateApplicationArchitecture().CreateModel();
               var result = model.ValidationResult;
               Assert.AreEqual( this._injErrors, result.CompositeValidationResults.Values.SelectMany( cResult => cResult.InjectionValidationErrors ).Count() + result.InjectionValidationErrors.Count, "Injection errors: " + this.ToString( result.InjectionValidationErrors ) );
               Assert.AreEqual( this._structErrors, result.CompositeValidationResults.Values.SelectMany( cResult => cResult.StructureValidationErrors ).Count() + result.StructureValidationErrors.Count, "Structural errors: " + this.ToString( result.StructureValidationErrors ) );
               Assert.AreEqual( this._internalErrors, result.CompositeValidationResults.Values.SelectMany( cResult => cResult.InternalValidationErrors ).Count() + result.InternalValidationErrors.Count, "Internal errors: " + this.ToString( result.InternalValidationErrors ) );

               if ( this._injErrors > 0 || this._structErrors > 0 || this._internalErrors > 0 )
               {
                  Assert.Throws<InvalidApplicationModelException>( new TestDelegate( () => model.GenerateAndSaveAssemblies( CodeGeneration.CodeGenerationParallelization.NotParallel, logicalAssemblyProcessor: Qi4CSCodeGenHelper.EmittingArgumentsCallback ) ) );
                  Assert.Throws<InvalidApplicationModelException>( new TestDelegate( () => model.NewInstance( null, null, null ) ) );
               }
               else
               {
                  model.GenerateAndSaveAssemblies( CodeGeneration.CodeGenerationParallelization.NotParallel, logicalAssemblyProcessor: Qi4CSCodeGenHelper.EmittingArgumentsCallback );
                  model.NewInstance( null, null, null );
               }
            } );
         }
      }

      protected String ToString<T>( IEnumerable<T> enumerable )
      {
         return String.Join( ", ", enumerable.Select( obj => obj.ToString() ) );
      }

      protected abstract ApplicationArchitecture<ApplicationModel<ApplicationSPI>> CreateApplicationArchitecture();

   }
}
