# Phx.Inject Documentation

Compile time dependency injection for .NET.

Written as a Roslyn Source Generator, PHX.Inject will analyze dependency
specifications defined in your code and generate the source code that performs
the injection and linking at build time. This results in blazing fast injection
at runtime, and quick identification of dependency issues at compile time.

## Injectors
:::api-snippet Phx.Inject.InjectorAttribute.remarks[ApiDoc:Injector]


:::api-snippet Phx.Inject.InjectorAttribute.example[ApiDoc:Injector]

### Generated Injector Naming
:::api-snippet Phx.Inject.InjectorAttribute.GeneratedClassName.remarks[ApiDoc]


:::api-snippet Phx.Inject.InjectorAttribute.GeneratedClassName.example

### Providers
:::api-snippet Phx.Inject.InjectorAttribute.remarks[ApiDoc:Provider]


:::api-snippet Phx.Inject.InjectorAttribute.example[ApiDoc:Provider]

### Activators
Activators are methods that initialize a provided value using values from the
dependency graph. They will be linked to a builder in the injector's set of
specifications based on the type of the first parameter and the qualifier of the
method.
```csharp
[Injector(
    typeof(TestSpecification)
)]
public interface ITestInjector {
    /// Activators must always return void and contain a single parameter of the
    /// type that is to be injected.
    public void Build(MyClass class);
    
    /// Qualifier attributes should be placed on the activator method, not on the
    /// parameter.
    [Label("MyLabel")]
    public void BuildOther(MyClass class);
}
```

### Scope
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

### Child Injectors
A child injector is an injector that is not constructed directly by the calling
code, but that is constructed by another "parent" injector. This child injector
will have access to the dependencies provided by the parent injector through an
`Dependency` specification interface, but the parent will not have implicit access to
dependencies provided by the child. `Dependency` specifications can only define
Factories, not Factory References, Builders, or BuilderReferences, and the
factories cannot accept any arguments.

```csharp
[Specification]
internal static ChildSpecification {
    internal static MyClass GetMyClass(int intValue) {
        return new MyClass(intValue);
    }
}

/// The dependency interface defines the types that must be provided
/// by the parent injector.
[Specification]
internal interface IChildDependencies {
    /// Qualifier attributes could also be used to differentiate dependencies
    /// with the same type.
    [Factory]
    public int GetIntValue();
}

[Injector(
    typeof(ChildSpecification)
)]
[Dependency(typeof(IChildDependencies))]
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
[Dependency(...)]
internal interface ISessionInjector {
    /// Session credentials are shared by all dependencies created within a
    /// session.
    public Credentials GetSessionCredentials();
    
    [ChildInjector]
    public IRequestInjector GetRequestInjector();
}

[Injector(...)]
[Dependency(...)]
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
> **Note:** The dependency interface implementation does not accept values from the child injector.
> This means that factory defined in the parent cannot be injected with a value from the child,
> if it is invoked from a factory on the child.

### Constructed Injectors
Sometimes a dependency is not known at compile time and has to be inserted into
the dependency graph at runtime. This can still be done in a safe and compile
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

## Specifications
Specifications are classes that contain Factories and Builders that define how
dependencies should be linked and constructed. They define the nodes in the
dependency graph and the information the framework uses to connect the nodes
together.

### Specification Definitions
Specifications are annotated with the `Specification` attribute and must be
either a static class or an interface (See [Constructed Specifications](#constructed-specifications)).
They must contain one or more `Factory`, `Builder`, `FactoryReference` `BuilderReference` or `Link`.

```csharp
[Specification]
internal static class TestSpecification {
    [Factory]
    internal static int GetIntValue() {
        return 10;
    }
}
```

### Factories
Factories are methods or read only properties with a non-void return type. They
define how a type is constructed and what dependencies it has. Factory methods
can contain parameters which will be linked to other values in the dependency
graph based on their type and qualifier attributes.

```csharp
[Specification]
internal static class TestSpecification {
    [Factory]
    internal static int IntValue => 10;
    
    [Factory]
    internal static MyClass GetMyClass(int intValue) {
        return new MyClass(intValue);
    }
}
```

### Partial Factories
Multiple factories from the dependency graph can be combined into a single
collection by using the `[Partial]` attribute. This attribute can be used on
factories that return a `IReadOnlyList`, `ISet`, or `IReadOnlyDictionary`, and
all partial factories that return the same qualified type will be combined into
a single collection. Partial factories cannot be declared with the same
qualified type as non-partial factories.

```csharp
[Specification]
internal static class TestSpecification {
    [Factory]
    [Partial]
    internal static IReadOnlyList<MyClass> GetMyClassList1() {
        return new List<MyClass> { new MyClass(10) };
    }
    
    [Factory]
    [Partial]
    internal static IReadOnlyList<MyClass> GetMyClassList2() {
        return new List<MyClass> { new MyClass(20) };
    }
    
    [Factory]
    internal static MyClassConsumer GetMyClassConsumer(IReadOnlyList<MyClass> myClasses) {
        // The injected `myClasses` list will contain both `MyClass` instances.
        return new MyClassConsumer(myClasses);
    }
    
    [Factory]
    [Partial]
    internal static ISet<MyClass> GetMyClassSet() {
        return new HashSet<MyClass> { new MyClass(10) };
    }
    
    [Factory]
    [Label("MyClassDictionary")]
    [Partial]
    internal static IReadOnlyDictionary<string, MyClass> GetMyClassSet() {
        return new Dictionary<string, ILeaf> {
            { "key1", new MyClass(10) }
        };
    }
}
```

### Builders
Builders are methods used to initialize an existing object with values from the
dependency graph. They must have a `void` return type, and the first parameter
must be the type that is to be initialized. Further parameters after the first
will be linked to other values in the dependency graph based on their type and
qualifier attributes.

```csharp
[Specification]
internal static class TestSpecification {
    [Factory]
    internal static int IntValue => 10;
    
    [Builder]
    internal static void BuildMyClass(MyClass target, int intValue) {
        target.Value = intValue;
    }
}
```

### Fabrication Modes
The Fabrication mode defines the behavior of the factory method. By default, all
factories are `Recurrent`, meaning that the factory will be invoked and a new
value will be constructed each time the dependency provided by the factory is
required.

This can be modified to use the `Scoped` fabrication mode, which will construct
the dependency once and reuse that same instance each time the dependency is
required. This scope is tied to the lifetime of the containing injector.

```csharp
[Specification]
internal static class TestSpecification {
    [Factory]
    internal static int IntValue => new Random().Next(100);
    
    [Factory(FabricationMode.Scoped)]
    internal static string StringValue => "Hello";
}
```

A factory defined with the `Container` fabrication mode will construct a new
instance each time it is invoked, similar to the `Recurrent` mode, but any
dependencies that use the `ContainerScoped` fabrication mode will be reused
within the container.

> **Note:** A `Container` scoped factory consumed by another `Container` scoped factory
> will define a new container, and will not share any `ContainerScoped` dependencies
> with the parent container.

```csharp
[Specification]
internal static class TestSpecification {
    private static int currentInt = 0;
    
    [Factory(FabricationMode.ContainerScoped)]
    internal static int GetInt() {
        return currentInt++;
    }
    
    [Factory]
    [Partial]
    internal static List<IntLeaf> GetIntLeaf1(IntLeaf leaf) {
        return new List<IntLeaf> { leaf };
    }
    
    [Factory]
    [Partial]
    internal static List<IntLeaf> GetIntLeaf2(IntLeaf leaf) {
        return new List<IntLeaf> { leaf };
    }

    [Factory(FabricationMode.Container)]
    internal static Node GetNode(List<IntLeaf> leaves) {
        var left = leaves[0];
        var right = leaves[1];
        return new Node(left, right);
    }
}
```
In the example above, each `Node` will contain `IntLeaf` instances that both
have the same `int` value. But each new `Node` instance created will use a
different `int` value.

### Qualifiers
Qualifiers are attributes that differentiate dependencies of the same type.

One type of qualifier is the built in `Label` attribute that uses a given
string as the unique qualifier.

```csharp
[Specification]
internal static class TestSpecification {
    [Factory]
    [Label("MyInt")]
    internal static int IntValue => 10;
    
    [Factory]
    [Label("MyOtherInt")]
    internal static int OtherInt => 11;
    
    [Factory]
    internal static MyClass GetMyClass([Label("MyInt")] int myInt) {
        return new MyClass(myInt);
    }
    
    [Factory]
    internal static MyOtherClass GetMyOtherClass([Label("MyOtherInt")] int myOtherInt) {
        return new MyOtherClass(myOtherInt);
    }
}
```

Custom qualifier attributes can also be defined.

```csharp
[Qualifier]
[AttributeUsage(QualifierAttribute.Usage)]
internal class QualifierAAttribute : Attribute { }
    
[Qualifier]
[AttributeUsage(QualifierAttribute.Usage)]
internal class QualifierBAttribute : Attribute { }

[Specification]
internal static class TestSpecification {
    [Factory]
    [QualifierA]
    internal static int IntValue => 10;
    
    [Factory]
    [QualifierB]
    internal static int OtherInt => 11;
    
    [Factory]
    internal static MyClass GetMyClass([QualifierA] int myInt) {
        return new MyClass(myInt);
    }
    
    [Factory]
    internal static MyOtherClass GetMyOtherClass([QualifierB] int myOtherInt) {
        return new MyOtherClass(myOtherInt);
    }
}
```

The absence of a qualifier is itself a unique qualifier and will not be linked
to a dependency of the same type with a qualifier. This can be done manually
using qualifiers on a new factory method.
```csharp
[Specification]
internal static class TestSpecification {
    /// A labeled `int` factory.
    [Factory]
    [Label("MyInt")]
    internal static int IntValue => new Random().Next(100);
    
    /// An unlabeled `int` factory that passes the value of the labelled `int`.
    [Factory]
    internal static int GetInt([Label("MyInt")] int intValue) {
        return intValue;
    }
}
```

### Factory and Builder References
There are times when a factory or builder method is already defined. This could
be invoked using a normal factory or builder method.
```csharp
internal class MyClass {
    public int Value { get; private set; } = 0;
    
    private MyClass() { }
    
    public static MyClass Construct(int value) {
        var myClass = new MyClass();
        myClass.Value = value;
        return myClass;
    }
}

internal class MyOtherClass {
    public int Value { get; private set; } = 0;

    public static void Inject(MyClass target, int value) {
        myClass.Value = value;
    }
}
```
```csharp
[Specification]
internal static class TestSpecification {
    [Factory]
    internal static MyClass GetMyClass(int intValue) {
        return MyClass.Construct(intValue);
    }
    
    [Builder]
    internal static void BuildMyOtherClass(MyOtherClass target, int intValue) {
        MyOtherClass.Inject(target, intValue);
    }
}
```

To reduce this boilerplate code, the `FactoryReference` and `BuilderReference`
attributes are provided, and can be implemented as properties of type
`Func` and `Action`.
```csharp
[Specification]
internal static class TestSpecification {
    [FactoryReference]
    internal static Func<int, MyClass> GetMyClass = MyClass.Construct;
    
    [BuilderReference]
    internal static Action<MyOtherClass, int> BuildMyOtherClass = MyOtherClass.Inject;
}
```

### Links
Another feature that can be used to reduce boilerplate code, is the `Link`
attribute. This is used to specify that a factory of one type should be used
when a dependency of one of its base types is needed.
```csharp
[Specification]
[Link(typeof(MyClass), typeof(IMyClass))]
internal static class TestSpecification {
    [Factory]
    internal static MyClass GetMyClass(int intValue) {
        return new MyClass(intValue);
    }
    
    [Factory]
    internal static MyOtherClass GetMyOtherClass(IMyClass myClass) {
        return new MyOtherClass(myClass);
    }
}
```
The above specification is equivalent to this specification with the boilerplate
code expanded.
```csharp
[Specification]
internal static class TestSpecification {
    [Factory]
    internal static MyClass GetMyClass(int intValue) {
        return new MyClass(intValue);
    }
    
    [Factory]
    internal static IMyClass GetIMyClass(MyClass myClass) {
        return myClass;
    }
    
    [Factory]
    internal static MyOtherClass GetMyOtherClass(IMyClass myClass) {
        return new MyOtherClass(myClass);
    }
}
```

### Auto Dependencies
Dependencies that are non-static, non-abstract, and public, and that contain a
single constructor with parameters or required properties that are provided in
the dependency graph or that also fit this criteria, can be automatically linked
by the framework. Simply declare the dependency as a parameter in a factory
method and the framework will automatically link the dependency.

Auto dependencies can also be scoped by applying a `Factory` attribute to the
class. This will ensure that they are scoped to the injector that contains them.
By default, auto dependencies are `Recurrent`.

An auto dependency can also define `Link`s from itself.

```csharp
[Factory(FabricationMode.Scoped)]
public class AutoTypeWithFabricationMode {
    public int X { get; } = 10;
}

public record AutoTypeWithRequiredProperty {
    public required int X { get; init; }
}

public interface IAutoType {
    AutoTypeWithFabricationMode Y { get; }
    AutoTypeWithRequiredProperty Z { get; }
}

[Link(typeof(AutoType), typeof(IAutoType))]
public class AutoType : IAutoType {
    public AutoTypeWithFabricationMode Y { get; }
    public AutoTypeWithRequiredProperty Z { get; }
    
    public AutoType(AutoTypeWithFabricationMode y, AutoTypeWithRequiredProperty z) {
        Y = y;
        Z = z;
    }
}

```

### Auto Builders
Static methods that are public or internal on a type can be used as a builder
method if the first parameter is the same type as the containing type and it
has the `Builder` attribute.

Auto builders can also be labeled by applying a `Qualifer` attribute to the method.

```csharp
public class AutoBuilderType {
    public int X { get; private set; }
    
    [Builder]
    public static void Build(AutoBuilderType target, int x) {
        target.X = x;
    }
    
    [Builder]
    [Label("DoubleX")]
    public static void BuildDoubleX(AutoBuilderType target, int x) {
        target.X = x + x;
    }
}

[Injector]
public interface IAutoBuilderInjector {
    public AutoBuilderType BuildAutoBuilderType(AutoBuilderType target);
    
    [Label("DoubleX")]
    public AutoBuilderType BuildDoubleXAutoBuilderType(AutoBuilderType target);
}
```

### Runtime Factories
Sometimes a factory method needs to be called one or more times at runtime, instead
of when the injector is created. This can be done using a runtime factory by
declaring a dependency on a `Factory<T>` type.

```csharp
[Specification]
internal static class RuntimeFactorySpecification {
    [Factory]
    internal static MyClass GetMyClass(int intValue) {
        return new MyClass(intValue);
    }
    
    [Factory]
    internal static MyOtherClass GetMyOtherClass(Phx.Inject.Factory<MyClass> factory) {
        return new MyOtherClass(factory.Create);
    }
}
```

Instead of injector the constructed instance of `MyClass`, this will inject a
`Factory` instance that can be used to create an instance of `MyClass` at runtime.

### Constructed Specifications
See [Constructed Injectors](#constructed-injectors).