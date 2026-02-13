# <a id="Phx_Inject_Factory_1"></a> Class Factory<T\>

Namespace: [Phx.Inject](Phx.Inject.md)  
Assembly: Phx.Inject.Generator.dll  

A type that can be used to inject a dependency on the factory method for a class instead of the
class itself.

```csharp
public sealed class Factory<T>
```

#### Type Parameters

`T` 

The type of the dependency.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[Factory<T\>](Phx.Inject.Factory\-1.md)

#### Inherited Members

[object.Equals\(object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\)), 
[object.Equals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\-system\-object\)), 
[object.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.object.gethashcode), 
[object.GetType\(\)](https://learn.microsoft.com/dotnet/api/system.object.gettype), 
[object.ReferenceEquals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals), 
[object.ToString\(\)](https://learn.microsoft.com/dotnet/api/system.object.tostring)

#### Extension Methods

[FunctionalExtensions.Also<Factory<T\>\>\(Factory<T\>, Action<Factory<T\>\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Also\_\_1\_\_\_0\_System\_Action\_\_\_0\_\_), 
[GeneratorIgnoredExtensions.GeneratorIgnored<Factory<T\>\>\(Factory<T\>\)](Phx.Inject.Generator.Incremental.Util.GeneratorIgnoredExtensions.md\#Phx\_Inject\_Generator\_Incremental\_Util\_GeneratorIgnoredExtensions\_GeneratorIgnored\_\_1\_\_\_0\_), 
[FunctionalExtensions.Let<Factory<T\>, TResult\>\(Factory<T\>, Func<Factory<T\>, TResult\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Let\_\_2\_\_\_0\_System\_Func\_\_\_0\_\_\_1\_\_), 
[FunctionalExtensions.Then<Factory<T\>\>\(Factory<T\>, Action<Factory<T\>\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Then\_\_1\_\_\_0\_System\_Action\_\_\_0\_\_)

## Constructors

### <a id="Phx_Inject_Factory_1__ctor_System_Func__0__"></a> Factory\(Func<T\>\)

Initializes a new instance of the Factory class.

```csharp
public Factory(Func<T> factory)
```

#### Parameters

`factory` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<T\>

The factory method used to construct a new instance of the dependency class.

## Methods

### <a id="Phx_Inject_Factory_1_Create"></a> Create\(\)

Creates a new instance of the dependency.

```csharp
public T Create()
```

#### Returns

 T

A new instance of type T.

