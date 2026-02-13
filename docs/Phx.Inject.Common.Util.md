# <a id="Phx_Inject_Common_Util"></a> Namespace Phx.Inject.Common.Util

### Classes

 [FunctionalExtensions](Phx.Inject.Common.Util.FunctionalExtensions.md)

Extension methods for functional programming patterns inspired by Kotlin/Scala.

PURPOSE:
- Enables functional transformation and side-effect patterns in C#
- Reduces temporary variables and improves code flow
- Makes complex transformations more readable through chaining

WHY THIS EXISTS:
C# has limited built-in support for functional patterns that are common in other languages.
These extensions bring three key patterns:

1. Let: Transform a value inline (Kotlin's let, Scala's map)
2. Also: Perform side effects while passing through a value (Kotlin's also, Scala's tap)
3. Then: Terminate a chain with a side effect (Kotlin's also at end of chain)

PATTERN COMPARISON:

Traditional C#:
  var temp = GetValue();
  var transformed = Transform(temp);
  Log(transformed);
  DoSomething(transformed);
  return transformed;

With functional extensions:
  return GetValue()
      .Let(Transform)
      .Also(Log)
      .Also(DoSomething);

DESIGN DECISIONS:

1. Why "Let" instead of "Map" or "Select"?
   - LINQ already has Select() for IEnumerable
   - "Let" is Kotlin terminology, familiar to many developers
   - Avoids confusion with LINQ operators

2. Why both "Also" and "Then"?
   - Also: Returns the value (chainable, for multiple side effects)
   - Then: Returns void (terminal operation, signals end of chain)
   - Different signatures prevent accidental misuse

3. Why are these useful in generators?
   - Generators involve multi-stage transformations (syntax → symbols → metadata → code)
   - Each stage may need logging, validation, or diagnostics
   - These patterns make transformation pipelines explicit and linear

USAGE EXAMPLES:

Transformation chain:
  var metadata = symbol
      .Let(ExtractInfo)
      .Let(ValidateInfo)
      .Also(info =&gt; Log($"Processing {info.Name}"))
      .Let(CreateMetadata);

Builder pattern enhancement:
  return new StringBuilder()
      .Also(sb =&gt; sb.AppendLine("// Generated code"))
      .Also(sb =&gt; sb.AppendLine($"class {className}"))
      .Also(sb =&gt; AppendMembers(sb, members))
      .ToString();

Side effects without breaking flow:
  var result = CalculateValue()
      .Also(val =&gt; Debug.Assert(val &gt; 0, "Expected positive"))
      .Also(val =&gt; metrics.Record(val))
      .Let(val =&gt; val * 2);

PERFORMANCE CONSIDERATIONS:
- Zero overhead: Calls inline to the provided function/action
- No allocations beyond what the lambda captures
- JIT can inline these for optimal performance
- Equivalent performance to writing the code manually

 [IEnumerableExtensions](Phx.Inject.Common.Util.IEnumerableExtensions.md)

Extension methods for working with <xref href="System.Collections.Generic.IEnumerable%601" data-throw-if-not-resolved="false"></xref> collections.

PURPOSE:
- Provides collection manipulation utilities for common generator patterns
- Simplifies conditional collection building
- Supports immutable collection transformations

WHY THIS EXISTS:
Source generators frequently work with collections that need to be:
1. Built conditionally (add items only if they exist/are valid)
2. Converted between mutable and immutable forms
3. Grouped or transformed while maintaining immutability

These extensions support functional-style collection building that avoids
mutations and null-checking boilerplate.

COMMON PATTERNS SUPPORTED:

1. Conditional collection building:
   var items = baseItems
       .AppendIfNotNull(optionalItem1)
       .AppendIfNotNull(optionalItem2);

   Without this, you'd need:
   var items = baseItems.ToList();
   if (optionalItem1 != null) items.Add(optionalItem1);
   if (optionalItem2 != null) items.Add(optionalItem2);

2. Multi-map construction:
   Dictionary&lt;string, List&lt;Type&gt;&gt; mutable = BuildMap();
   IReadOnlyDictionary&lt;string, IReadOnlyList&lt;Type&gt;&gt; immutable = 
       mutable.ToImmutableMultiMap();

DESIGN DECISIONS:

1. Why AppendIfNotNull instead of Where(x =&gt; x != null)?
   - More explicit intent (optional single item vs filtering a collection)
   - Chainable for building collections incrementally
   - Works naturally with nullable reference types

2. Why ToImmutableMultiMap?
   - Generators build dictionaries of lists during analysis
   - Need to convert to immutable forms for caching/threading
   - Common pattern deserves a dedicated method

3. Why the commented-out CreateTypeMap?
   - Shows the pattern this file was evolving toward
   - Left for reference during refactoring
   - May be uncommented when error handling is standardized

PERFORMANCE CONSIDERATIONS:
- AppendIfNotNull is lazy (doesn't allocate if null)
- ToImmutableMultiMap creates new immutable structures (one-time cost at stage boundaries)
- For large collections, consider evaluating once with .ToList() before multiple iterations

