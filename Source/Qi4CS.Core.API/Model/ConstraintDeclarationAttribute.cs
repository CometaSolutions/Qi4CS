﻿/*
 * Copyright (c) 2007, Rickard Öberg.
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

namespace Qi4CS.Core.API.Model
{
   /// <summary>
   /// All attributes that are used to trigger Constraints must have this annotation.
   /// This way the Qi4CS runtime will separate constraint attributes from other kind of attributes.
   /// </summary>
   [AttributeUsage( AttributeTargets.Class )]
   public sealed class ConstraintDeclarationAttribute : Attribute
   {
   }
}