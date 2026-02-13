# <a id="Phx_Inject_InjectionUtil"></a> Class InjectionUtil

Namespace: [Phx.Inject](Phx.Inject.md)  
Assembly: Phx.Inject.Generator.dll  

```csharp
public static class InjectionUtil
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[InjectionUtil](Phx.Inject.InjectionUtil.md)

#### Inherited Members

[object.Equals\(object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\)), 
[object.Equals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\-system\-object\)), 
[object.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.object.gethashcode), 
[object.GetType\(\)](https://learn.microsoft.com/dotnet/api/system.object.gettype), 
[object.MemberwiseClone\(\)](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone), 
[object.ReferenceEquals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals), 
[object.ToString\(\)](https://learn.microsoft.com/dotnet/api/system.object.tostring)

## Methods

### <a id="Phx_Inject_InjectionUtil_Combine__1_System_Collections_Generic_IReadOnlyList___0____"></a> Combine<T\>\(params IReadOnlyList<T\>\[\]\)

```csharp
public static IReadOnlyList<T> Combine<T>(params IReadOnlyList<T>[] lists)
```

#### Parameters

`lists` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist\-1)<T\>\[\]

#### Returns

 [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist\-1)<T\>

#### Type Parameters

`T` 

### <a id="Phx_Inject_InjectionUtil_Combine__1_System_Collections_Generic_ISet___0____"></a> Combine<T\>\(params ISet<T\>\[\]\)

```csharp
public static ISet<T> Combine<T>(params ISet<T>[] sets)
```

#### Parameters

`sets` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>\[\]

#### Returns

 [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>

#### Type Parameters

`T` 

### <a id="Phx_Inject_InjectionUtil_Combine__2_System_Collections_Generic_IReadOnlyDictionary___0___1____"></a> Combine<K, V\>\(params IReadOnlyDictionary<K, V\>\[\]\)

```csharp
public static IReadOnlyDictionary<K, V> Combine<K, V>(params IReadOnlyDictionary<K, V>[] dicts)
```

#### Parameters

`dicts` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary\-2)<K, V\>\[\]

#### Returns

 [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary\-2)<K, V\>

#### Type Parameters

`K` 

`V` 

### <a id="Phx_Inject_InjectionUtil_CombineReadOnlySet__1_System_Collections_Generic_IEnumerable___0____"></a> CombineReadOnlySet<T\>\(params IEnumerable<T\>\[\]\)

```csharp
public static ISet<T> CombineReadOnlySet<T>(params IEnumerable<T>[] sets)
```

#### Parameters

`sets` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>\[\]

#### Returns

 [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>

#### Type Parameters

`T` 

