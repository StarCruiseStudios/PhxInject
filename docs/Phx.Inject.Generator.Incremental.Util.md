# <a id="Phx_Inject_Generator_Incremental_Util"></a> Namespace Phx.Inject.Generator.Incremental.Util

### Classes

 [GeneratorIgnored<T\>](Phx.Inject.Generator.Incremental.Util.GeneratorIgnored\-1.md)

Type wrapper that excludes a value from incremental generator equality comparisons,
preventing unnecessary regeneration when the wrapped value changes.

 [GeneratorIgnoredExtensions](Phx.Inject.Generator.Incremental.Util.GeneratorIgnoredExtensions.md)

Extension methods for wrapping values in <xref href="Phx.Inject.Generator.Incremental.Util.GeneratorIgnoredExtensions.GeneratorIgnored%60%601(%60%600)" data-throw-if-not-resolved="false"></xref> instances.

 [LocationExtensions](Phx.Inject.Generator.Incremental.Util.LocationExtensions.md)

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

