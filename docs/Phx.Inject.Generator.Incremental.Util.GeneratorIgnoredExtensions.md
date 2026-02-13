# <a id="Phx_Inject_Generator_Incremental_Util_GeneratorIgnoredExtensions"></a> Class GeneratorIgnoredExtensions

Namespace: [Phx.Inject.Generator.Incremental.Util](Phx.Inject.Generator.Incremental.Util.md)  
Assembly: Phx.Inject.Generator.dll  

Extension methods for wrapping values in <xref href="Phx.Inject.Generator.Incremental.Util.GeneratorIgnoredExtensions.GeneratorIgnored%60%601(%60%600)" data-throw-if-not-resolved="false"></xref> instances.

```csharp
public static class GeneratorIgnoredExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[GeneratorIgnoredExtensions](Phx.Inject.Generator.Incremental.Util.GeneratorIgnoredExtensions.md)

#### Inherited Members

[object.Equals\(object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\)), 
[object.Equals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\-system\-object\)), 
[object.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.object.gethashcode), 
[object.GetType\(\)](https://learn.microsoft.com/dotnet/api/system.object.gettype), 
[object.MemberwiseClone\(\)](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone), 
[object.ReferenceEquals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals), 
[object.ToString\(\)](https://learn.microsoft.com/dotnet/api/system.object.tostring)

## Remarks

Provides convenient fluent syntax for creating ignored wrappers, typically used
when constructing metadata records from Roslyn symbols.

## Methods

### <a id="Phx_Inject_Generator_Incremental_Util_GeneratorIgnoredExtensions_GeneratorIgnored__1___0_"></a> GeneratorIgnored<T\>\(T\)

Wraps a value so it won't affect incremental generator equality comparisons.

```csharp
public static GeneratorIgnored<T> GeneratorIgnored<T>(this T value)
```

#### Parameters

`value` T

The value to wrap.

#### Returns

 [GeneratorIgnored](Phx.Inject.Generator.Incremental.Util.GeneratorIgnored\-1.md)<T\>

A wrapped value that appears equal to all other <code>GeneratorIgnored&lt;T&gt;</code>
instances regardless of the wrapped value.

#### Type Parameters

`T` 

The type of value to wrap.

#### Examples

<pre><code class="lang-csharp">var location = symbol.Locations.First().GeneratorIgnored();
// location changes won't trigger regeneration</code></pre>

