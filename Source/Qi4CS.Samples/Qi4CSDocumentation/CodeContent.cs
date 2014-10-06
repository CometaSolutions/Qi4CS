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
using System.Reflection;
using System.Text;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.API.Common;
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.Bootstrap.Assembling;
using CILAssemblyManipulator.API;
using Qi4CS.Extensions.Functional.Assembling;
using Qi4CS.Extensions.Functional.Model;

#pragma warning disable 169,649
namespace Qi4CS.Samples.Qi4CSDocumentation
{
   internal class Sessional
   {

   }

   #region AppliesToCode1
   // This is our concern which should take care of any session stuff
   [DefaultAppliesTo( typeof( Sessional ) )] // Tells Qi4CS to apply this concern on methods with [Sessional] attributes
   public class SessionConcern : GenericConcern
   {
      public override Object Invoke( Object composite, MethodInfo method, Object[] args )
      {
         // do some session stuff
         return null;
      }
   }

   // This is our [Sessional] attribute
   public sealed class SessionalAttribute : Attribute { }

   // This is our mixin
   public class MyMixin : My
   {
      [Sessional]
      public void DoSomethingWithSession()
      {
         // This code will be wrapped in session
      }

      public void DoSomethingWithoutSession()
      {
         // This code will not be wrapped in session
      }
   }

   // This is our composite interface
   [DefaultMixins( typeof( MyMixin ) )]
   [DefaultConcerns( typeof( SessionConcern ) )]
   public interface My
   {
      void DoSomethingWithSession();
      void DoSomethingWithoutSession();
   }
   #endregion

   #region AppliesToCode2
   // This is our mixin
   [DefaultAppliesTo( typeof( B ) )] // Tells Qi4CS to apply this mixin on methods contained in B-interface
   public class GenericBMixin : GenericInvocator
   {
      public Object Invoke( Object composite, MethodInfo method, Object[] args )
      {
         // do some stuff related to B
         return null;
      }
   }

   // This is one role mixin
   public interface A
   {
      void MethodA();
   }

   // This is another role mixin
   public interface B
   {
      void MethodB();
   }

   // This is our composite interface
   [DefaultMixins( typeof( GenericBMixin ) )]
   public interface C : A, B
   {
      void MethodC();
   }
   #endregion

   #region AppliesToCode3
   // This is our mixin
   [DefaultAppliesTo( typeof( MyAppliesToFilter ) )] // Tells Qi4CS to apply this mixin on methods for which MyAppliesToFilter.AppliesTo method returns true
   public class MyGenericMixin : GenericConcern
   {
      public override Object Invoke( Object composite, MethodInfo method, Object[] args )
      {
         // do something
         return null;
      }
   }

   // This is our custom AppliesTo filter
   public class MyAppliesToFilter : AppliesToFilter
   {
      public Boolean AppliesTo( MethodInfo compositeMethod, MethodInfo fragmentMethod )
      {
         // Perform some logic whether the fragment method applies to composite method
         return true;
      }
   }

   // This is our composite interface
   [DefaultMixins( typeof( MyGenericMixin ) )]
   public interface MyComposite
   {
      void FirstMethod();
      void SecondMethod();
   }
   #endregion

   internal class OptionalCode
   {
      #region OptionalCode1
      public interface TestComposite
      {
         void MethodWithMandatoryParameter( String param );
         void MethodWithOptionalParameter( [Optional] String param );
      }
      #endregion

      #region OptionalCode2
      public interface TestComposite2
      {
         [Optional]
         String MyProperty { get; set; }
      }
      #endregion

      internal interface MyService { }

      #region OptionalCode3
      public interface TestComposite3
      {
         void DoSomething();
      }

      public class TestComposite3Mixin : TestComposite3
      {
         [Optional, Service]
         private MyService _service;

         public virtual void DoSomething()
         {
            // Here this._service might be null if suitable service could not be found at runtime
         }
      }
      #endregion
   }

   internal class AmbiguousTypeExceptionCode
   {
      #region AmbiguousTypeExceptionCode1
      public interface FirstComposite : Parent
      { }

      public interface SecondComposite : Parent
      { }

      public interface Parent
      { }
      #endregion
   }

   internal class ConcernOfCode
   {
      #region ConcernOfCode1
      public interface MyStuff
      {
         void DoSomething();
      }

      public class MyStuffConcern : MyStuff
      {
         [ConcernFor]
         private MyStuff next;

         public virtual void DoSomething()
         {
            // Do checks or modify stuff here

            // Continue the execution of composite method chain
            this.next.DoSomething();
         }
      }
      #endregion
   }

   internal class StateAttributeCode
   {
      #region StateAttributeCode1
      public interface MyComposite
      {
         String MyStringProperty { get; set; }
         EventHandler<EventArgs> MyDelegateProperty { get; set; }
         event EventHandler<EventArgs> MyEvent;
      }
      #endregion

      #region StateAttributeCode2
      public abstract class MyCompositeMixin : MyComposite
      {
         #region MyComposite Members

         // The properties and events from MyComposite interface must be marked as abstract in order for them to be part of composite state.
         public abstract String MyStringProperty { get; set; }
         public abstract EventHandler<EventArgs> MyDelegateProperty { get; set; }
         public abstract event EventHandler<EventArgs> MyEvent;

         #endregion

         // The code may acquire access to the properties and events either directly or via [State] injection
         [State]
         private CompositeProperty<String> _myStringProperty;

         [State( typeof( MyComposite ), "MyDelegateProperty" )]
         private CompositeProperty<EventHandler<EventArgs>> _delegateProperty;

         [State( "MyEvent" )]
         private EventHandler<EventArgs> _myEventInvocationAction;

         [State( typeof( MyComposite ), "MyEvent" )]
         private CompositeEvent<EventHandler<EventArgs>> _myEvent;

         [State]
         private CompositeState _state;

         [State( "MyStringProperty" )]
         private CompositeProperty _myStringPropertyWithoutGenericArgs;

         [State]
         private String _myStringPropertyValue;


      }
      #endregion
   }

   internal class DefaultMixinsAttributCode
   {
      #region DefaultMixinsAttributeCode1
      [DefaultMixins( typeof( MyBeerOrder ) )]
      public interface BeerOrder
      {

      }

      public class MyBeerOrder : BeerOrder
      {
         // Implementation of BeerOrder here
      }
      #endregion
   }

   public class SideEffectForAttributeCode
   {
      public class SomeResult { }

      #region SideEffectForAttributeCode1
      public interface MyStuff
      {
         SomeResult DoSomething();
      }

      public class MyStuffSideEffect : MyStuff
      {
         [SideEffectFor]
         private MyStuff result;

         public virtual SomeResult DoSomething()
         {
            var invocationResult = this.result.DoSomething();

            // Do side-effect stuff here

            // Return value is ignored by Qi4CS runtime, a null would work too for reference types
            return invocationResult;
         }
      }
      #endregion
   }

   internal class CompositeModelTypeCode
   {
      #region CompositeModelTypeCode1
      public sealed class MyCompositeModel : CompositeModelType
      {
         public override String ToString()
         {
            return "TextualNameOfYourCompositeModel";
         }
      }
      #endregion
   }

   internal class FunctionAggregatorDeclarationCode
   {
      #region FunctionAggregatorDeclarationCode1
      public interface MyFunctionality
      {
         String GetStringFor( Object obj );
      }

      public class FunctionalityForType : MyFunctionality
      {
         public virtual String GetStringFor( Object obj )
         {
            var type = (Type) obj; // Assume obj is Type
            return type.FullName + "_MySuffix";
         }
      }

      public class FunctionalityForString : MyFunctionality
      {
         public virtual String GetStringFor( Object obj )
         {
            var str = (String) obj; // Assume obj is string
            return str + "_MyOtherSuffix";
         }
      }
      #endregion

      private void Dummy( Assembler assembler )
      {
         #region FunctionAggregatorDeclarationCode2
         // Assume 'assembler' is variable of type Qi4CS.Core.Bootstrap.Assembling.Assembler
         assembler
           .NewService()
           .AsFunctionAggregator<Type, MyFunctionality>() // Specify that Type is used as key type when deciding which aggregated object to use. See extension method of E_Qi4CS_Functional in this assembly for more information.
             .WithFunctionType( FunctionAssemblerUtils.TypeBasedFunctionLookUp( 0 ) ) // FunctionAssemblerUtils is utility class in this namespace, and this method returns lambda to extract a type using Object.GetType() method from parameter at index 0 of method arguments
             .WithDefaultFunctions(
            // Add information about FunctionalityForType : only one key which is type of System.Type, and creation process instantiates FunctionalityForType as Qi4CS composite
               Tuple.Create<Type[], Func<StructureServiceProvider, MyFunctionality>>( new Type[] { typeof( Type ) }, ssp => ssp.NewPlainCompositeBuilder<FunctionalityForType>().Instantiate() ),
            // Add information about FunctionalityForString : only one key which is type of System.String, and creation process instantiates FunctionalityForString as Qi4CS composite
               Tuple.Create<Type[], Func<StructureServiceProvider, MyFunctionality>>( new Type[] { typeof( String ) }, ssp => ssp.NewPlainCompositeBuilder<FunctionalityForString>().Instantiate() )
             )
          .Done();
         // No need to do assembler.OfTypes( typeof( MyFunctionality ) ) - "WithFunctionType" method already did that.

         // Remember add composites being aggregated
         // This is not required if FunctionalityForType and FunctionalityForString are not composites but rather normal C# objects.
         assembler.NewPlainComposite().OfTypes( typeof( FunctionalityForType ) );
         assembler.NewPlainComposite().OfTypes( typeof( FunctionalityForString ) );
         #endregion
      }

      private void Dummy2( StructureServiceProvider ssp )
      {
         #region FunctionAggregatorDeclarationCode3
         // Assume 'ssp' is variable of type Qi4CS.Core.API.Structures.StructureServiceProvider
         var service = ssp.FindService<MyFunctionality>().GetService(); // Get the reference to service composite
         var typeStr = service.GetStringFor( typeof( Object ) ); // 'typeStr' now is "System.Object_MySuffix"
         var strStr = service.GetStringFor( "MyTestString" ); // 'strStr' now is "MyTestString_MyOtherSuffix"
         #endregion
      }
   }

   internal class FunctionInvocationDataAttributeCode
   {
      #region FunctionInvocationDataAttributeCode1
      public interface Composite1
      {
         void DoSomething();
      }

      public interface Composite2
      {
         void DoSomethingElse();
      }

      public class Mixin1 : Composite1
      {
         [FunctionInvocationData]
         private Dictionary<String, String> _someStringDictionary;

         public virtual void DoSomething()
         {
            Composite2 c = null; // Obtain instance of Composite2 either by creating or somehow else

            c.DoSomethingElse();

            // The _someStringDictionary will now have "TestKey", "TestValue" -key value pair, see Mixin2 for more information
         }
      }

      public class Mixin2 : Composite2
      {
         [FunctionInvocationData]
         private Dictionary<String, String> _someStringDictionary;

         public virtual void DoSomethingElse()
         {
            // If execution flow is now here because it was called in Mixin1 class, the _someStringDictionary here will point to the same instance as the _someStringDictionary in Mixin1 class.
            this._someStringDictionary.Add( "TestKey", "TestValue" );
         }
      }
      #endregion
   }

   internal class RoleAttributeCode
   {
      public class SomeResult { }
      #region RoleAttributeCode1
      public interface MyAggregateService
      {
         SomeResult DoSomething( [RoleParameter] String myString, [RoleParameter( "MyParamID" )] Int32 myInt32 );
      }
      #endregion
      #region RoleAttributeCode2
      public interface MyFunctionality
      {
         SomeResult DoSomething();
      }
      #endregion
      #region RoleAttributeCode3
      public class MyFunctionalityMixin : MyFunctionality
      {
         [Role]
         private String _myString; // This will hold the first parameter given to the method MyAggregateService.DoSomething

         [Role( "MyParamID" )]
         private Int32 _myInt32; // This will hold the second parameter given to the method MyAggregateService.DoSomething 

         public virtual SomeResult DoSomething()
         {
            // Do something...
            return new SomeResult();
         }
      }
      #endregion
   }
}
#pragma warning restore 169,649