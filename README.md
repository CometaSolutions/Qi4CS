# Qi4CS

Qi4CS is a version of Qi4j ( https://github.com/Qi4j/qi4j-sdk ) for CLR platform.
It is written in C# and, despite the abbreviation, can be used with VB.NET or managed C/C++ languages as well. It should work in Mono as well, but that has not yet been thoroughly tested.

API documentation and guides about getting started are located at http://www.cometasolutions.fi/qi4cs/documentation/ .

NuGet packages:

https://www.nuget.org/packages/Qi4CS.Core/ (direct link: http://www.nuget.org/api/v2/package/Qi4CS.Core/ )

https://www.nuget.org/packages/Qi4CS.Extensions.Configuration/ (direct link: http://www.nuget.org/api/v2/package/Qi4CS.Extensions.Configuration/ )

https://www.nuget.org/packages/Qi4CS.Extensions.Configuration.XML/ (direct link: http://www.nuget.org/api/v2/package/Qi4CS.Extensions.Configuration.XML/  )

https://www.nuget.org/packages/Qi4CS.Extensions.Functional/ (direct link: http://www.nuget.org/api/v2/package/Qi4CS.Extensions.Functional/ )

https://www.nuget.org/packages/Qi4CS.SDK/ (direct link: http://www.nuget.org/api/v2/package/Qi4CS.SDK/ )

## Quickguide to use

Add a reference to `Qi4CS.Core` NuGet package (or download directly from http://nuget.org/api/v2/package/Qi4CS.Core ).
Implement class `Qi4CSModelProviderSkeleton` or interface `Qi4CSModelProvider` from namespace `Qi4CS.Core.Bootstrap.Model`.
This class will be responsible for bootstrap process: define composites, visibility rules, etc.

Instantiate your class in Main method, acquire Qi4CS model by using its `Model` property, and use `NewInstance` method of the model to acquire your Qi4CS application.
Activate your application and start using it (create composites, find services, etc ).

Once your program needs to shut down, it is good practice to passivate the Qi4CS application.

## Example

This example will use `SingletonArchitecture` as architecture for Qi4CS application.
The architecture is very simple (no visibility rules or structure) and good for small examples like this or testing.

Let's start by defining a very simple domain interface and implementation for it.
```cs
public interface MyComposite
{
  String MyMethod();
}

public class MyCompositeMixin : MyComposite
{
  public virtual string MyMethod()
  {
    return "It works!";
  }
}
```
Notice the `virtual` keyword.
It is required in order for Qi4CS code generation to work properly (if there are non-virtual methods implementing domain interface methods, Qi4CS will mark them as errors during compile stage).

Then, we should add the reference to `Qi4CS.Core` NuGet package and implement the class `Qi4CSModelProviderSkeleton`.
```cs
public class MyQi4CSModelProvider : Qi4CSModelProviderSkeleton<SingletonApplicationModel>
{
   protected override Qi4CS.Core.Bootstrap.Assembling.ApplicationArchitecture<SingletonApplicationModel> BuildArchitecture()
   {
      var arch = Qi4CS.Core.Architectures.Assembling.Qi4CSArchitectureFactory.NewSingletonArchitecture();

      arch.CompositeAssembler
         .NewPlainComposite()
         .OfTypes(typeof(MyComposite))
         .WithMixins(typeof(MyCompositeMixin));

      return arch;
   }
}
```
Roughly speaking, this will tell the Qi4CS runtime that composites that implement type `MyComposite` should delegate execution of methods to class `MyCompositeMixin`.

Finally, the `Main` method is the following code.
```cs
static void Main(string[] args)
{
   var qApp = new MyQi4CSModelProvider().Model.NewInstance("", "", "");
   qApp.Activate();

   Console.WriteLine(
      qApp.StructureServices.NewPlainComposite<MyComposite>().MyMethod()
      );

   qApp.Passivate();

   Console.ReadLine();
}
```
This quick sample should print `It works!` and wait for user input before shutting down.