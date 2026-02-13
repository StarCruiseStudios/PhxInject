# <a id="Phx_Inject_Generator_Incremental_Util_GeneratorIgnored_1"></a> Class GeneratorIgnored<T\>

Namespace: [Phx.Inject.Generator.Incremental.Util](Phx.Inject.Generator.Incremental.Util.md)  
Assembly: Phx.Inject.Generator.dll  

Type wrapper that excludes a value from incremental generator equality comparisons,
preventing unnecessary regeneration when the wrapped value changes.

```csharp
public class GeneratorIgnored<T>
```

#### Type Parameters

`T` 

The type of value being wrapped. Typically non-semantic data like source locations.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[GeneratorIgnored<T\>](Phx.Inject.Generator.Incremental.Util.GeneratorIgnored\-1.md)

#### Inherited Members

[object.Equals\(object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\)), 
[object.Equals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\-system\-object\)), 
[object.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.object.gethashcode), 
[object.GetType\(\)](https://learn.microsoft.com/dotnet/api/system.object.gettype), 
[object.MemberwiseClone\(\)](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone), 
[object.ReferenceEquals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals), 
[object.ToString\(\)](https://learn.microsoft.com/dotnet/api/system.object.tostring)

#### Extension Methods

[FunctionalExtensions.Also<GeneratorIgnored<T\>\>\(GeneratorIgnored<T\>, Action<GeneratorIgnored<T\>\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Also\_\_1\_\_\_0\_System\_Action\_\_\_0\_\_), 
[GeneratorIgnoredExtensions.GeneratorIgnored<GeneratorIgnored<T\>\>\(GeneratorIgnored<T\>\)](Phx.Inject.Generator.Incremental.Util.GeneratorIgnoredExtensions.md\#Phx\_Inject\_Generator\_Incremental\_Util\_GeneratorIgnoredExtensions\_GeneratorIgnored\_\_1\_\_\_0\_), 
[FunctionalExtensions.Let<GeneratorIgnored<T\>, TResult\>\(GeneratorIgnored<T\>, Func<GeneratorIgnored<T\>, TResult\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Let\_\_2\_\_\_0\_System\_Func\_\_\_0\_\_\_1\_\_), 
[FunctionalExtensions.Then<GeneratorIgnored<T\>\>\(GeneratorIgnored<T\>, Action<GeneratorIgnored<T\>\>\)](Phx.Inject.Common.Util.FunctionalExtensions.md\#Phx\_Inject\_Common\_Util\_FunctionalExtensions\_Then\_\_1\_\_\_0\_System\_Action\_\_\_0\_\_)

## Remarks

<p>Purpose:</p>
<p>
Roslyn's incremental generators use value equality on model objects to determine if
regeneration is needed. However, some data (like source file locations) is essential
for diagnostics but irrelevant for code generation decisions. Wrapping such values
prevents spurious regeneration when, for example, code is reformatted without changing
semantics.
</p>

<p>Equality Semantics:</p>
<p>
Two <code>GeneratorIgnored&lt;T&gt;</code> instances are always considered equal regardless
of their wrapped values, as long as they wrap the same type <code>T</code>. This intentionally
violates normal equality contracts but is correct in the context of incremental
compilation where we want to signal "this data doesn't affect output."
</p>

<p>Usage Pattern:</p>
<p>
Primarily used for <xref href="Phx.Inject.Generator.Incremental.Util.LocationInfo" data-throw-if-not-resolved="false"></xref> fields in metadata records. Location
data is needed to report diagnostics at the correct source positions but shouldn't
trigger recompilation if a type definition is moved within a file.
</p>

<p>Performance:</p>
<p>
Minimal overhead - just a single object allocation and type-based hash code.
Avoids expensive deep comparisons of location data during incremental compilation checks.
</p>

<p>When NOT to use:</p>
<p>
Do not wrap semantic data that affects code generation. If changing the value should
trigger regeneration, do not use this wrapper.
</p>

## Constructors

### <a id="Phx_Inject_Generator_Incremental_Util_GeneratorIgnored_1__ctor__0_"></a> GeneratorIgnored\(T\)

Type wrapper that excludes a value from incremental generator equality comparisons,
preventing unnecessary regeneration when the wrapped value changes.

```csharp
public GeneratorIgnored(T value)
```

#### Parameters

`value` T

The value to wrap. Its changes will not trigger incremental regeneration.

#### Remarks

<p>Purpose:</p>
<p>
Roslyn's incremental generators use value equality on model objects to determine if
regeneration is needed. However, some data (like source file locations) is essential
for diagnostics but irrelevant for code generation decisions. Wrapping such values
prevents spurious regeneration when, for example, code is reformatted without changing
semantics.
</p>

<p>Equality Semantics:</p>
<p>
Two <code>GeneratorIgnored&lt;T&gt;</code> instances are always considered equal regardless
of their wrapped values, as long as they wrap the same type <code>T</code>. This intentionally
violates normal equality contracts but is correct in the context of incremental
compilation where we want to signal "this data doesn't affect output."
</p>

<p>Usage Pattern:</p>
<p>
Primarily used for <xref href="Phx.Inject.Generator.Incremental.Util.LocationInfo" data-throw-if-not-resolved="false"></xref> fields in metadata records. Location
data is needed to report diagnostics at the correct source positions but shouldn't
trigger recompilation if a type definition is moved within a file.
</p>

<p>Performance:</p>
<p>
Minimal overhead - just a single object allocation and type-based hash code.
Avoids expensive deep comparisons of location data during incremental compilation checks.
</p>

<p>When NOT to use:</p>
<p>
Do not wrap semantic data that affects code generation. If changing the value should
trigger regeneration, do not use this wrapper.
</p>

## Properties

### <a id="Phx_Inject_Generator_Incremental_Util_GeneratorIgnored_1_Value"></a> Value

Gets the wrapped value.

```csharp
public T Value { get; }
```

#### Property Value

 T

#### Remarks

Access this when you need the actual data (e.g., reporting a diagnostic).
Do not use this value in equality comparisons that affect incremental compilation.

## Methods

### <a id="Phx_Inject_Generator_Incremental_Util_GeneratorIgnored_1_Equals_Phx_Inject_Generator_Incremental_Util_GeneratorIgnored__0__"></a> Equals\(GeneratorIgnored<T\>?\)

```csharp
public virtual bool Equals(GeneratorIgnored<T>? other)
```

#### Parameters

`other` [GeneratorIgnored](Phx.Inject.Generator.Incremental.Util.GeneratorIgnored\-1.md)<T\>?

#### Returns

 [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### <a id="Phx_Inject_Generator_Incremental_Util_GeneratorIgnored_1_GetHashCode"></a> GetHashCode\(\)

```csharp
public override int GetHashCode()
```

#### Returns

 [int](https://learn.microsoft.com/dotnet/api/system.int32)

### <a id="Phx_Inject_Generator_Incremental_Util_GeneratorIgnored_1_ToString"></a> ToString\(\)

```csharp
public override string ToString()
```

#### Returns

 [string](https://learn.microsoft.com/dotnet/api/system.string)

