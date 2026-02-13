# <a id="Phx_Inject_Generator_Incremental_Util_LocationExtensions"></a> Class LocationExtensions

Namespace: [Phx.Inject.Generator.Incremental.Util](Phx.Inject.Generator.Incremental.Util.md)  
Assembly: Phx.Inject.Generator.dll  

Extension methods for Roslyn <xref href="Microsoft.CodeAnalysis.Location" data-throw-if-not-resolved="false"></xref> objects to support incremental generator patterns.

PURPOSE:
- Provides null-safe operations for locations throughout the generator pipeline
- Integrates Location instances with the incremental generator's caching system via GeneratorIgnored
- Ensures diagnostic messages have valid locations even when source information is unavailable

WHY THIS EXISTS:
Roslyn's Location can be null in various scenarios (generated code, compiler-synthesized members, metadata).
In incremental generators, proper location handling is critical for:
1. Accurate diagnostic reporting that points users to the exact source of issues
2. Correct caching behavior - locations affect equality comparisons but shouldn't trigger regeneration
3. Safe downstream code that doesn't need constant null checks

INCREMENTAL GENERATOR INTEGRATION:
The GeneratorIgnored wrapper tells the incremental generator that Location changes should NOT
trigger regeneration. This is essential because:
- Location changes (file moves, line number shifts) don't affect semantic meaning
- Without this, any code reformatting would invalidate the entire cache
- Diagnostics still get accurate locations, but they don't participate in equality checks

```csharp
public static class LocationExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[LocationExtensions](Phx.Inject.Generator.Incremental.Util.LocationExtensions.md)

#### Inherited Members

[object.Equals\(object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\)), 
[object.Equals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\-system\-object\)), 
[object.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.object.gethashcode), 
[object.GetType\(\)](https://learn.microsoft.com/dotnet/api/system.object.gettype), 
[object.MemberwiseClone\(\)](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone), 
[object.ReferenceEquals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals), 
[object.ToString\(\)](https://learn.microsoft.com/dotnet/api/system.object.tostring)

## Methods

### <a id="Phx_Inject_Generator_Incremental_Util_LocationExtensions_OrNone_Microsoft_CodeAnalysis_Location_"></a> OrNone\(Location?\)

Returns the <xref href="Microsoft.CodeAnalysis.Location" data-throw-if-not-resolved="false"></xref> or <xref href="Microsoft.CodeAnalysis.Location.None" data-throw-if-not-resolved="false"></xref> if the value is <code> null </code>.

```csharp
public static Location OrNone(this Location? location)
```

#### Parameters

`location` [Location](https://learn.microsoft.com/dotnet/api/microsoft.codeanalysis.location)?

The location to return.

#### Returns

 [Location](https://learn.microsoft.com/dotnet/api/microsoft.codeanalysis.location)

An instance of <xref href="Microsoft.CodeAnalysis.Location" data-throw-if-not-resolved="false"></xref>.

#### Remarks

PATTERN: Null-coalescing for safe location handling.
Use this at API boundaries where Location? needs to become Location for downstream consumers.
Location.None is a special sentinel that indicates "no source location" rather than null.

