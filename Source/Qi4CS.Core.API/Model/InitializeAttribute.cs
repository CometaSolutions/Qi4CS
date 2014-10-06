﻿/*
 * Copyright (c) 2008, Rickard Öberg (
 * org.qi4j.api.mixin.Initializable class)
 * Copyright (c) 2011, Niclas Hedhman (idea from his message on Qi4j
 * mailing list).
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
   /// This special scope attribute is used mark methods which should be called each time after fragment is created and all fields have been injected.
   /// </summary>
   [SpecialScope, AttributeUsage( SpecialScopeAttribute.SPECIAL_SCOPE_USAGE )]
   public class InitializeAttribute : Attribute
   {
   }
}