/*
 * Copyright (c) 2008, Rickard Öberg.
 * See NOTICE file.
 * 
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
using Qi4CS.Core.SPI.Instance;

namespace Qi4CS.Core.Bootstrap.Instance
{
   /// <summary>
   /// This is instance-level structural construct of <see cref="Model.LayeredApplicationModel"/>.
   /// It extends <see cref="ApplicationSPI"/> to add methods specific for layered architecture.
   /// </summary>
   public interface LayeredApplication : ApplicationSPI
   {
      /// <summary>
      /// Find a <see cref="Layer"/> with specific name.
      /// Returns <c>null</c> if the layer can't be found or the name is <c>null</c>.
      /// </summary>
      /// <param name="layerName">The name of the <see cref="Layer"/> to find.</param>
      /// <returns>Layer with given name or <c>null</c>.</returns>
      Layer FindLayer( String layerName );

      /// <summary>
      /// Finds a <see cref="Module"/> with given name contained in the <see cref="Layer"/> with given name.
      /// Returns <c>null</c> if the layer or module can't be found or either of the parameters is <c>null</c>.
      /// </summary>
      /// <param name="layerName">The name of the <see cref="Layer"/> to find..</param>
      /// <param name="moduleName">The name of the <see cref="Module"/> to find.</param>
      /// <returns>A <see cref="Module"/> named as <paramref name="moduleName"/> contained within a layer named <paramref name="layerName"/>, or <c>null</c>.</returns>
      Module FindModule( String layerName, String moduleName );
   }
}