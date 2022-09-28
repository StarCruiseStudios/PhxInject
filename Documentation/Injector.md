# PHX.Inject Injectors
The Injector is the interface used to construct and access dependencies in the
dependency graph. An injector will always be an interface annotated with the
`Injector` attribute and will contain methods and properties used by your 
application as access points into the dependency graph, as well as a list of
specifications used to inform the framework how to construct the dependency
graph.

## Generated Injector Naming
By default, the generated injector will be named by prefixing the name of the
injector interface with "Generated", after removing the "I" prefix if there is
one.
* The interface `ITestInjector` generates `GeneratedTestInjector`.
* The interface `ApplicationInjector` generates `GeneratedApplicationInjector`.

To explicitly define the generated injector name, use the optional 
`GeneratedClassName` property on the `Injector` attribute of the interface.
```csharp
[Injector(
    generatedClassName: "CustomInjector",
    typeof(TestSpecification)
)]
public interface ITestInjector {
    // ...
}
```
The generated injector will always use the same namespace as the injector 
interface.

## Providers
Providers are parameterless methods that are defined on the injector interface. 
They will be linked to a factory in the injector's set of specifications based
on the return type and qualifier attributes of the provider.
```csharp
[Injector(
    typeof(TestSpecification)
)]
public interface ITestInjector {
    /// Providers must always be parameterless and have a non void return type.
    /// They can be named anything you want.
    public int GetMyInt();
    
    /// Qualifiers can be used to differentiate between dependencies of the same
    /// type.
    [Label("MyLabel")]
    public int GetOtherInt();
    
    /// Providers are linked based on the qualifiers AND return type. So 
    /// qualifier attributes can be reused with different types.
    [Label("MyLabel")]
    public string GetString();
}
```

## Builders
Builders are methods that initialize a provided value using values from the
dependency graph. They will be linked to a builder in the injector's set of
specifications based on the type of the first parameter and the qualifier of the
method.
```csharp
[Injector(
    typeof(TestSpecification)
)]
public interface ITestInjector {
    /// Builders must always return void and contain a single parameter of the
    /// type that is to be injected.
    public void Build(MyClass class);
    
    /// Qualifier attributes should be placed on the builder method, not on the
    /// parameter.
    [Label("MyLabel")]
    pubilc void BuildOther(MyClass class);
}
```

## Scope
By default, all factories will construct a new instance each time they are
invoked. This behavior can be changed so that a factory will construct an
instance the first time it is invoked, and return the same instance each time it
is invoked after that. The lifetime of this "scoped" dependency is tied to the
lifetime of the injector. i.e. a single injector will always use the same scoped
instance, but creating a new injector will create a new instance.
```csharp
[Specification]
internal static TestSpecification {
    [Factory(FabricationMode.Scoped)]
    internal static MyClass GetMyClass() {
        return new MyClass();
    }
}

[Injector(
    typeof(TestSpecification)
)]
public interface ITestInjector {
    public MyClass GetMyClass();
}
```
```csharp
var injector = new GeneratedTestInjector();
var myClass1a = injector.GetMyClass();
var myClass1b = injector.GetMyClass();

var injector2 = new GeneratedTestInjector();
var myClass2 = injector2.GetMyClass();

Verify.That(Object.ReferenceEquals(myClass1a, myClass1b).IsTrue());
Verify.That(Object.ReferenceEquals(myClass1a, myClass2).IsFalse());
```

## Child Injectors
A child injector is an injector that is not constructed directly by the calling
code, but that is constructed by another "parent" injector. This child injector
will have access to the dependencies provided by the parent injector through an
`ExternalDependency` interface, but the parent will not have implicit access to
dependencies provided by the child.

```csharp
[Specification]
internal static ChildSpecification {
    internal static MyClass GetMyClass(int intValue) {
        return new MyClass(intValue);
    }
}

/// The external dependency interface defines the types that must be provided
/// by the parent injector.
internal interface IChildExternalDependencies {
    /// Qualifier attributes could also be used to differentiate dependencies
    /// with the same type.
    public int GetIntValue();
}

[Injector(
    typeof(ChildSpecification)
)]
[ExternalDependency(typeof(IChildExternalDependencies))]
internal interface IChildInjector {
    public MyClass GetMyClass();
}
```
```csharp
[Specification]
internal static ParentSpecification {
    internal static int GetIntValue() {
        return 10;
    }
}

[Injector(
    typeof(ParentSpecification)
)]
internal interface IParentInjector {
    [ChildInjector]
    public IChildInjector GetChildInjector();
}
```
```csharp
var parentInjector = new GeneratedParentInjector();
var childInjector = parentInjector.GetChildInjector();
var myClass = childInjector.GetMyClass();

Verify.That(myClass.Value.IsEqualTo(10));
```

Child injectors can be useful for defining different scopes and lifetimes within
a single dependency graph.

```csharp
[Injector(...)]
internal interface IApplicationInjector {
    /// All dependencies share the same application config.
    public AppConfig GetApplicationConfig();
    
    [ChildInjector]
    public ISessionInjector GetSessionInjector();
}

[Injector(...)]
[ExternalDependency(...)]
internal interface ISessionInjector {
    /// Session credentials are shared by all dependencies created within a
    /// session.
    public Credentials GetSessionCredentials();
    
    [ChildInjector]
    public IRequestInjector GetRequestInjector();
}

[Injector(...)]
[ExternalDependency(...)]
internal interface IRequestInjector {
    /// A unique request ID is shared by all dependencies while a request is
    /// processing.
    public string GetRequestId();
}
```

```csharp
// On Application startup:
var appInjector = new GeneratedAppInjector();

// Each time a new session is started within the same application:
var sessionInjector = appInjector.GetSessionInjector();

// Each time a new request is created within a session:
var requestInjector = sessionInjector.GetRequestInjector();

requestInjector.GetRequestId();
```

## Constructed Injectors
Sometimes a dependency is not known at compile time and has to be inserted into
the dependecy graph at runtime. This can still be done in a safe and compile
time verifiable way using Constructed Injectors and Constructed Specifications.
```csharp
/// Instead of using a static class, declare the specficiation as an interface.
[Specification]
internal interface IConstructedSpecification {
    [Factory]
    public int GetIntValue();
}

/// The injector is defined in the normal way, with a reference to the 
/// constructed specification.
[Injector(
    typeof(IConstructedSpecification)
)]
internal interface IConstructedInjector {
    public int GetIntValue();
}
```

```csharp
/// Any type that implements the constructed specification interface can be used.
IConstructedSpecification spec = new ConstructedSpecificationImpl();

/// The generated injector will accept the specification as a constructor argument.
IConstructedInjector injector = new GeneratedConstructedInjector(spec);
```

Constructed injectors can also be used as child injectors.

```csharp
[Injector(...)]
internal interface IParentInjector {
    /// The child injector method should contain parameters for any constructed
    /// specifications need to be passed to the child injector.
    [ChildInjector]
    public IConstructedInjector GetChildInjector(IConstructedSpecification spec);
}
```