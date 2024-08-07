# PHX.Inject Specifications
Specifications are classes that contain Factories and Builders that define how 
dependencies should be linked and constructed. They define the nodes in the
dependency graph and the information the framework uses to connect the nodes
together.

## Specification Definitions
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

## Factories
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

## Partial Factories
Multiple factories from the dependency graph can be combined into a single 
collection by using the `[Partial]` attribute. This attribute can be used on
factories that return a `List`, `HashSet`, or `Dictionary`, and all partial
factories that return the same qualified type will be combined into a single 
collection. Partial factories cannot be declared with the same qualified type as
non-partial factories.

```csharp
[Specification]
internal static class TestSpecification {
    [Factory]
    [Partial]
    internal static List<MyClass> GetMyClassList1() {
        return new List<MyClass> { new MyClass(10) };
    }
    
    [Factory]
    [Partial]
    internal static List<MyClass> GetMyClassList2() {
        return new List<MyClass> { new MyClass(20) };
    }
    
    [Factory]
    internal static MyClassConsumer GetMyClassConsumer(List<MyClass> myClasses) {
        // The injected `myClasses` list will contain both `MyClass` instances.
        return new MyClassConsumer(myClasses);
    }
    
    [Factory]
    [Partial]
    internal static HashSet<MyClass> GetMyClassSet() {
        return new HashSet<MyClass> { new MyClass(10) };
    }
    
    [Factory]
    [Label("MyClassDictionary")]
    [Partial]
    internal static Dictionary<string, MyClass> GetMyClassSet() {
        return new Dictionary<string, ILeaf> {
            { "key1", new MyClass(10) }
        };
    }
}
```

## Builders
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

## Fabrication Modes
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

## Qualifiers
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

## Factory and Builder References
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
    internal static Action<int, MyOtherClass> BuildMyOtherClass = MyOtherClass.Inject;
}
```

## Links
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

## Auto Dependencies
Dependencies that are non-static, non-abstract, and public, and that contain a
single constructor with parameters that are provided in the dependency graph or
that also fit this criteria, can be automatically linked by the framework. 
Simply declare the dependency as a parameter in a factory method and the
framework will automatically link the dependency.

Auto dependencies can also be scoped by applying a `Factory` attribute to the 
class. This will ensure that they are scoped to the injector that contains them.
By default, auto dependencies are `Recurrent`.

```csharp
[Factory(FabricationMode.Scoped)]
public class AutoTypeWithFabricationMode {
    public int X { get; } = 10;
}

public class AutoType {
    public AutoTypeWithFabricationMode Y { get; }
    
    public AutoType(AutoTypeWithFabricationMode y) {
        Y = y;
    }
}
```

## Runtime Factories
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

## Constructed Specifications
See [Constructed Injectors](Injector.md#constructed-injectors).