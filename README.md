# Phx.Inject

Compile time dependency injection for .NET.

Written as a Roslyn Source Generator, PHX.Inject will analyze dependency 
specifications defined in your code and generate the source code that performs
the injection and linking at build time. This results in blazing fast injection
at runtime, and quick identification of dependency issues at compile time.

## Set up
PHX.Inject can be installed as a Nuget package using the .NET CLI.
```shell
dotnet add package Phx.Inject.Generator
```

## Getting Started
The simplest set up consists of a `Specification` and an `Injector`.
```csharp
using Phx.Inject;

[Specification]
internal static class TestSpecification {
    [Factory]
    internal static int GetIntValue() {
        return 10;
    }
}

[Injector(
    typeof(TestSpecification)
)]
public interface ITestInjector {
    public int GetInt();
}
```

The dependencies can be retrieved in code by instantiating the generated
injector.
```csharp
ITestInjector injector = new GeneratedTestInjector();
int myInt = injector.GetInt();

Verify.That(myInt.IsEqualTo(10));
```

Specifications contain Factories and Builders that define how dependencies
should be linked and constructed. The Injector is the interface used to 
construct and access those dependencies.


The example above will generate a new class `GeneratedTestInjector` that
implements the `ITestInjector` interface. The `GetInt()` method will be linked
to the `TestSpecification.GetIntValue()` method based on the return type of the
methods.

## Something More Complicated
The power of a Dependency Injection Framework comes from it's ability to define
a graph of dependencies, one node at a time, and automatically resolve the links
between each of those nodes.
```csharp
using Phx.Inject;

[Specification]
internal static class TestSpecification {
    [Factory]
    internal static MyClass GetMyClass(int intValue) {
        return new MyClass(intValue);
    }
    
    [Builder]
    internal static void GetMyBuiltClass(MyBuiltClass target, int intValue) {
        target.Value = intValue;
    }
     
    [Factory]
    internal static int GetIntValue() {
        return 10;
    }
}

[Injector(
    typeof(TestSpecification)
)]
public interface ITestInjector {
    public MyClass GetMyClass();
    public void Build(MyBuiltClass target);
}
```

In this example, the `GetMyClass` factory in the specification accepts an 
argument. This argument will be invoked with the linked int value dependency in 
the generated injector.

This example also contains a builder. A builder is similar to a factory, except
the instance is not constructed by the method, it is passed in and the method
will set up properties on the object. This pattern is useful when you do not
have control of the instantiation of the instance, but still want it configured
with values from the dependency graph.

```csharp
ITestInjector injector = new GeneratedTestInjector();
MyClass myClass = injector.GetMyClass();
MyBuiltClass myBuiltClass = new MyBuiltClass();
injector.Build(myBuiltClass);

Verify.That(myClass.Value.IsEqualTo(10));
Verify.That(myBuiltClass.Value.IsEqualTo(10));
```

See the [Documentation](Documentation/Index.md) for more details.

---

<div align="center">
Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.</br>
Licensed under the Apache License, Version 2.0.</br>
See http://www.apache.org/licenses/LICENSE-2.0 for full license information.</br>
</div>
