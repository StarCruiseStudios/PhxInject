# <a id="Phx_Inject_Common_Util_IEnumerableExtensions"></a> Class IEnumerableExtensions

Namespace: [Phx.Inject.Common.Util](Phx.Inject.Common.Util.md)  
Assembly: Phx.Inject.Generator.dll  

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

```csharp
public static class IEnumerableExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[IEnumerableExtensions](Phx.Inject.Common.Util.IEnumerableExtensions.md)

#### Inherited Members

[object.Equals\(object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\)), 
[object.Equals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\-system\-object\)), 
[object.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.object.gethashcode), 
[object.GetType\(\)](https://learn.microsoft.com/dotnet/api/system.object.gettype), 
[object.MemberwiseClone\(\)](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone), 
[object.ReferenceEquals\(object, object\)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals), 
[object.ToString\(\)](https://learn.microsoft.com/dotnet/api/system.object.tostring)

## Methods

### <a id="Phx_Inject_Common_Util_IEnumerableExtensions_AppendIfNotNull__1_System_Collections_Generic_IEnumerable___0____0_"></a> AppendIfNotNull<TSource\>\(IEnumerable<TSource\>, TSource?\)

Appends an element to a sequence only if the element is not null.

```csharp
public static IEnumerable<TSource> AppendIfNotNull<TSource>(this IEnumerable<TSource> source, TSource? element)
```

#### Parameters

`source` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<TSource\>

The source sequence.

`element` TSource?

The element to append if not null.

#### Returns

 [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<TSource\>

A sequence with the element appended if not null, otherwise the original sequence.

#### Type Parameters

`TSource` 

The type of elements in the sequence.

#### Remarks

USE CASE:
Building collections where some elements are optional/conditional.
Example: Gathering attributes where base class attributes may not exist.

LAZY EVALUATION:
Returns the original sequence if element is null (no allocation/enumeration).
Only creates a new sequence if element is non-null.

PATTERN:
Chainable for multiple optional items:
  collection
    .AppendIfNotNull(item1)
    .AppendIfNotNull(item2)
    .ToList();

### <a id="Phx_Inject_Common_Util_IEnumerableExtensions_ToImmutableMultiMap__3_System_Collections_Generic_IDictionary___0___2__"></a> ToImmutableMultiMap<TKey, TValue, TList\>\(IDictionary<TKey, TList\>\)

Converts a dictionary to an immutable multi-map with immutable list values.

```csharp
public static IReadOnlyDictionary<TKey, IReadOnlyList<TValue>> ToImmutableMultiMap<TKey, TValue, TList>(this IDictionary<TKey, TList> source) where TKey : notnull where TList : IEnumerable<TValue>, new()
```

#### Parameters

`source` [IDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary\-2)<TKey, TList\>

The source dictionary.

#### Returns

 [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary\-2)<TKey, [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist\-1)<TValue\>\>

An immutable dictionary with immutable list values.

#### Type Parameters

`TKey` 

The type of keys in the dictionary.

`TValue` 

The type of values in the lists.

`TList` 

The type of list containing the values.

#### Remarks

MULTI-MAP PATTERN:
A multi-map is a dictionary where each key maps to multiple values (a collection).
Common in generators for grouping related items:
- Type name → list of members
- Attribute type → list of annotated symbols
- Namespace → list of types

WHY IMMUTABLE:
Incremental generators cache results between invocations. Immutable collections:
- Are thread-safe (IDE may access from multiple threads)
- Have value-based equality (support proper caching)
- Prevent accidental mutation bugs

TYPE CONSTRAINTS:
- TList must be a collection type with a default constructor
- TKey must be non-null (dictionary key requirement)
- Converts List&lt;T&gt; values to IReadOnlyList&lt;T&gt; for immutability

USAGE:
  var builders = new Dictionary&lt;string, List&lt;MethodInfo&gt;&gt;();
  // ... populate builders ...
  IReadOnlyDictionary&lt;string, IReadOnlyList&lt;MethodInfo&gt;&gt; immutable = 
      builders.ToImmutableMultiMap();

