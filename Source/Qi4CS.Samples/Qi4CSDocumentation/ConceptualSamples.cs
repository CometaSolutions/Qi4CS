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
using Qi4CS.Core.API.Instance;
using Qi4CS.Core.API.Model;
using Qi4CS.Core.Architectures.Assembling;
using Qi4CS.Core.Bootstrap.Model;
using Qi4CS.Core.Bootstrap.Assembling;
using Qi4CS.Core.SPI.Model;
using Qi4CS.Core.SPI.Instance;
using System.Runtime.CompilerServices;
using Qi4CS.Core.Bootstrap.Instance;

#pragma warning disable 1700
#region InternalsVisibleToCode
// Using InternalsVisibleTo in assembly which is not strongly signed
[assembly: InternalsVisibleTo( "MyAssemblyName" + Qi4CSGeneratedAssemblyAttribute.ASSEMBLY_NAME_SUFFIX )]

// Using InternalsVisibleTo in assembly which is strongly signed (full public key must be included)
[assembly: InternalsVisibleTo( "MyAssemblyName" + Qi4CSGeneratedAssemblyAttribute.ASSEMBLY_NAME_SUFFIX + ", PublicKey=00240000..." )]
#endregion
#pragma warning restore 1700

namespace Qi4CS.Samples.Qi4CSDocumentation
{
#pragma warning disable 649, 169

   internal class FiveMinuteGuideCode
   {
      #region FiveMinuteGuideCode1

      public interface MyDomainFunctionality
      {
         void DoSomething( String someParameter );
      }

      public class MyDomainFunctionalityMixin : MyDomainFunctionality
      {
         // The method implementing domain interface must be virtual
         // because generated composite types must be able to intercept
         // method calls.
         public virtual void DoSomething( String someParameter )
         {
            Console.WriteLine( "My parameter is: " + someParameter );
         }
      }

      #endregion

      #region FiveMinuteGuideCode2
      // Use singleton architecture for simplicity
      internal class MyQi4CSAppBootstrapper : Qi4CSModelProviderSkeleton<SingletonApplicationModel>
      {

         // Need to implement this method that will take care of creating Qi4CS architecture
         // and setting it up.
         protected override ApplicationArchitecture<SingletonApplicationModel> BuildArchitecture()
         {
            var architecture = Qi4CSArchitectureFactory.NewSingletonArchitecture();
            architecture.CompositeAssembler
               .NewPlainComposite()
               .OfTypes( typeof( MyDomainFunctionality ) )
               .WithMixins( typeof( MyDomainFunctionalityMixin ) );

            return architecture;
         }
      }
      #endregion

      #region FiveMinuteGuideCode3
      public class Program
      {
         void Main( String[] args )
         {
            // See previous code sample for code of MyQi4CSAppBootstrapper
            var bootstrapper = new MyQi4CSAppBootstrapper();
            bootstrapper.Model
               // Include reference to Qi4CS.CodeGeneration.DotNET.dll to use this method
               .GenerateAndSaveAssemblies();
            // Create instance of Qi4CS application
            var qApp = bootstrapper.Model.NewInstance( "My Qi4CS application", "My application mode", "0.1" );
            // After creating the Qi4CS application, it must be activated
            // in order to use it
            qApp.Activate();

            // Create a composite with type declared in bootstrapper
            var ssp = qApp.StructureServices;
            var composite = ssp.NewPlainComposite<MyDomainFunctionality>();
            // 'composite' will be proxy created by Qi4CS
            // which will pass control to MyDomainFunctionalityMixin.DoSomething(String)
            composite.DoSomething( "MyParameter" );

            // And once the lifecycle is over, the Qi4CS application should be
            // passivated in order to release possibly held resources
            qApp.Passivate();
         }
      }
      #endregion
   }

   internal class CompositeCreationCode
   {
      internal interface MyComposite { }
      internal interface MyCompositePartialType1 { }
      internal interface MyCompositePartialType2 { }

      public static void DummyMethod()
      {
         StructureServiceProvider ssp = null;
         Type myCompositeType = null;

         #region CompositeCreationCode1
         // Acquiring CompositeBuilderInfo
         CompositeBuilderInfo<MyComposite> cbi = ssp.NewPlainCompositeBuilder<MyComposite>();

         // Acquiring CompositeBuilder, type of composite not known at compile time
         CompositeBuilder cb = ssp.NewPlainCompositeBuilder( myCompositeType ); // myCompositeType is of System.Type

         // Acquiring CompositeBuilder and using multiple types to filter out possible composites
         CompositeBuilder cbm = ssp.NewPlainCompositeBuilder( new[] { typeof( MyCompositePartialType1 ), typeof( MyCompositePartialType2 ) } );
         #endregion

         {
            #region CompositeCreationCode2
            MyComposite composite = cbi.Instantiate();
            #endregion
         }

         {
            #region CompositeCreationCode3
            MyComposite composite = cb.Instantiate<MyComposite>();
            MyComposite composite2 = (MyComposite) cb.InstantiateWithType( typeof( MyComposite ) );
            // Both composite and composite2 will be same object.
            #endregion
         }

         {
            #region CompositeCreationCode4
            MyCompositePartialType1 composite1 = cbm.Instantiate<MyCompositePartialType1>();
            MyCompositePartialType2 composite2 = cbm.Instantiate<MyCompositePartialType2>();
            // composite1 and composite2 may be different objects.
            #endregion
         }
      }

      internal class PrototypeHolder
      {
         #region CompositeCreationCode5
         public interface MyComposite
         {
            String MyProperty { get; set; }
         }

         public abstract class MyCompositeMixin : MyComposite
         {
            public abstract String MyProperty { get; set; }
         }
         #endregion

         public void DummyMethod()
         {
            CompositeBuilderInfo<MyComposite> cbi = null;
            CompositeBuilder cb = null;

            {
               #region CompositeCreationCode6
               // 'cbi' is of type CompositeBuilderInfo<MyComposite>
               MyComposite prototype = cbi.Prototype();
               prototype.MyProperty = "InitialValue";
               MyComposite instance = cbi.Instantiate();
               // 'instance' and 'prototype' now reference to same object,
               // but setting instance.MyProperty to null will fail.
               #endregion
            }

            {
               #region CompositeCreationCode7
               // 'cb' is of type CompositeBuilder
               MyComposite prototype = cb.Prototype<MyComposite>();
               // Alternatively, prototype could be acquired this way:
               MyComposite prototype2 = (MyComposite) cb.PrototypeFor( typeof( MyComposite ) );
               // 'prototype' and 'prototype2' reference to same object
               prototype.MyProperty = "InitialValue";
               MyComposite instance = cb.Instantiate<MyComposite>();
               // 'instance, 'prototype' and 'prototype2' all reference
               // to the same object.
               // Setting instance.MyProperty to null will fail.
               #endregion
            }
         }
      }
   }

   internal class PrivateCompositesCode
   {
      #region PrivateCompositesCode1

      public interface MyComposite
      {
         String MyProperty { get; }
      }

      public interface MyCompositeState
      {
         String ActualProperty { get; set; }
      }

      public class MyCompositeMixin : MyComposite
      {
         [This]
         private MyCompositeState _state;

         public String MyProperty
         {
            get
            {
               // this._state will not be castable to MyComposite
               return this._state.ActualProperty;
            }
         }
      }

      #endregion

      private void DummyMethod()
      {
         #region PrivateCompositesCode2
         var architecture = Qi4CSArchitectureFactory.NewSingletonArchitecture();
         architecture.CompositeAssembler
            .NewPlainComposite()
            .OfTypes( typeof( MyComposite ) )
            .WithMixins( typeof( MyCompositeMixin ) );
         // MyCompositeState is not included in the .OfTypes method parameters.
         // Therefore, it will become a private composite type.
         #endregion
      }
      internal class RolesContainer
      {
         #region PrivateCompositesCode3

         public interface MyCompositeCommand
         {
            String MyProperty { set; }
            MyCompositeQuery QueryRole { get; }
         }

         public interface MyCompositeQuery
         {
            String MyProperty { get; }
         }

         #endregion

         #region PrivateCompositesCode4

         public interface MyCompositeState
         {
            String MyProperty { get; set; }
         }

         public class MyCompositeCommandMixin : MyCompositeCommand
         {
            [This]
            private MyCompositeState _state;

            [This]
            private MyCompositeQuery _queryRole;

            public String MyProperty
            {
               set
               {
                  this._state.MyProperty = value;
               }
            }

            public MyCompositeQuery QueryRole
            {
               get
               {
                  // Private composite instances are safe to be returned to caller
                  return this._queryRole;
               }
            }
         }

         public class MyCompositeQueryMixin : MyCompositeQuery
         {
            [This]
            private MyCompositeState _state;
            // this field will hold same instance as _state field of MyCompositeCommandMixin

            public String MyProperty
            {
               get
               {
                  return this._state.MyProperty;
               }
            }
         }
         #endregion
      }
   }

   internal class FragmentsCode
   {
      #region FragmentsCode1
      public interface MyService
      {
         event EventHandler SomeEvent;
      }

      public interface MyComposite
      {
         void RegisterToServiceEvent();
         void OtherMethod();
      }

      public class MyCompositeMixin : MyComposite
      {
         [Service]
         private MyService _service;

         [Invocation]
         private System.Reflection.MethodInfo _currentMethod;

         public virtual void RegisterToServiceEvent()
         {
            // At this point, it is guaranteed that this._currentMethod
            // will have a valid value (the value of MyComposite.RegisterToServiceEvent method)
            this._service.SomeEvent += _service_SomeEvent;
            // After this method exits, the this._currentMethod field
            // may be reset or set to null at any time
         }

         public virtual void OtherMethod()
         {
            // At this point, it is guaranteed that this._currentMethod
            // will have a valid value (the value of MyComposite.OtherMethod method)
         }

         private void _service_SomeEvent( object sender, EventArgs e )
         {
            // When this method is invoked, unless it is invoked from within
            // RegisterToServiceEvent method, the value of this._currentMethod
            // may be the same as it was during RegisterToServiceEvent method,
            // or null, or something else (like MyComposite.OtherMethod)

            // Therefore using this._currentMethod here is not safe!
            Console.WriteLine( "Event invoked" );
         }
      }
      #endregion
   }

   internal class Fragments2Code
   {
      internal class Code1Holder
      {
         #region Fragments2Code1
         public interface MyComposite
         {
            void SomeMethod();
         }

         public class MyCompositeGenericMixin : GenericInvocator
         {
            public Object Invoke( Object composite, System.Reflection.MethodInfo method, Object[] args )
            {
               // Here 'object' is same object that was used to invoke MyComposite.SomeMethod
               // 'method' is reflection object of MyComposite.SomeMethod
               // 'args' is empty object array in this case, since MyComposite.SomeMethod does not have any parameters.
               return null;
               // Qi4CS will ignore return value of 'void' methods.
               // If return type is value type, then returning 'null' will cause the return value to be default(<return type>);
            }
         }

         // GenericConcern implements GenericInvocator and provides a helper field for concerns
         // It is not required to extend GenericConcern, it is also possible to make generic concerns by directly
         // implementing GenericInvocator interface
         public class MyCompositeGenericConcern : GenericConcern
         {
            public override Object Invoke( object composite, System.Reflection.MethodInfo method, object[] args )
            {
               // Do some checks here if needed
               // Modifying elements of args-array is possible, the modification will be visible to next concerns/mixin

               // When done, proceed in composite method invocation chain
               return this.next.Invoke( composite, method, args );
            }
         }

         // GenericSideEffect implements GenericInvocator and provides a helper field for side-effects
         // It is not required to extend GenericSideEffect, it is also possible to make generic side-effects by directly
         // implementing GenericInvocator interface
         public class MyCompositeGenericSideEffect : GenericSideEffect
         {

            protected override void DoInvoke( object composite, System.Reflection.MethodInfo method, object[] args )
            {
               // Retrieve return value of concerns and mixins.
               // If the concern and mixin invocation threw an exception, retrieving the return value will throw that exception
               var retVal = this.result.Invoke( composite, method, args );

               // Do something with retVal

               // Notice - no return clause is needed if extending GenericSideEffect as return values of side-effects are
               // ignored.
            }
         }
         #endregion
      }
      internal class Code2Holder
      {
         #region Fragments2Code2
         public interface MyComposite
         {
            void FirstMethod();
            void SecondMethod();
         }

         public abstract class MyCompositeMixinFirst : MyComposite
         {
            // This method will be used as mixin method for MyComposite.FirstMethod
            public virtual void FirstMethod()
            {
               Console.WriteLine( "First method" );
            }

            // Leave this method abstract - let MyCompositeMixinSecond implement it
            public abstract void SecondMethod();
         }

         public abstract class MyCompositeMixinSecond : MyComposite
         {
            // Leave this method abstract - let MyCompositeMixinFirst implement it
            public abstract void FirstMethod();

            // This method will be used as mixin method for MyComposite.SeondMethod
            public virtual void SecondMethod()
            {
               Console.WriteLine( "Second method" );
            }
         }
         #endregion
      }
   }
#pragma warning restore 649, 169
}

