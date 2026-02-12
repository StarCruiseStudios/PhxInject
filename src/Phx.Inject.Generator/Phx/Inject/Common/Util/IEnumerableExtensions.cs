// -----------------------------------------------------------------------------
// <copyright file="IEnumerableExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Collections.Immutable;

#endregion

namespace Phx.Inject.Common.Util;

/// <summary>
/// Extension methods for working with <see cref="IEnumerable{T}"/> collections.
/// 
/// PURPOSE:
/// - Provides collection manipulation utilities for common generator patterns
/// - Simplifies conditional collection building
/// - Supports immutable collection transformations
/// 
/// WHY THIS EXISTS:
/// Source generators frequently work with collections that need to be:
/// 1. Built conditionally (add items only if they exist/are valid)
/// 2. Converted between mutable and immutable forms
/// 3. Grouped or transformed while maintaining immutability
/// 
/// These extensions support functional-style collection building that avoids
/// mutations and null-checking boilerplate.
/// 
/// COMMON PATTERNS SUPPORTED:
/// 
/// 1. Conditional collection building:
///    var items = baseItems
///        .AppendIfNotNull(optionalItem1)
///        .AppendIfNotNull(optionalItem2);
///    
///    Without this, you'd need:
///    var items = baseItems.ToList();
///    if (optionalItem1 != null) items.Add(optionalItem1);
///    if (optionalItem2 != null) items.Add(optionalItem2);
/// 
/// 2. Multi-map construction:
///    Dictionary&lt;string, List&lt;Type&gt;&gt; mutable = BuildMap();
///    IReadOnlyDictionary&lt;string, IReadOnlyList&lt;Type&gt;&gt; immutable = 
///        mutable.ToImmutableMultiMap();
/// 
/// DESIGN DECISIONS:
/// 
/// 1. Why AppendIfNotNull instead of Where(x => x != null)?
///    - More explicit intent (optional single item vs filtering a collection)
///    - Chainable for building collections incrementally
///    - Works naturally with nullable reference types
/// 
/// 2. Why ToImmutableMultiMap?
///    - Generators build dictionaries of lists during analysis
///    - Need to convert to immutable forms for caching/threading
///    - Common pattern deserves a dedicated method
/// 
/// 3. Why the commented-out CreateTypeMap?
///    - Shows the pattern this file was evolving toward
///    - Left for reference during refactoring
///    - May be uncommented when error handling is standardized
/// 
/// PERFORMANCE CONSIDERATIONS:
/// - AppendIfNotNull is lazy (doesn't allocate if null)
/// - ToImmutableMultiMap creates new immutable structures (one-time cost at stage boundaries)
/// - For large collections, consider evaluating once with .ToList() before multiple iterations
/// </summary>
public static class IEnumerableExtensions {
    /// <summary>
    ///     Appends an element to a sequence only if the element is not null.
    /// </summary>
    /// <remarks>
    /// USE CASE:
    /// Building collections where some elements are optional/conditional.
    /// Example: Gathering attributes where base class attributes may not exist.
    /// 
    /// LAZY EVALUATION:
    /// Returns the original sequence if element is null (no allocation/enumeration).
    /// Only creates a new sequence if element is non-null.
    /// 
    /// PATTERN:
    /// Chainable for multiple optional items:
    ///   collection
    ///     .AppendIfNotNull(item1)
    ///     .AppendIfNotNull(item2)
    ///     .ToList();
    /// </remarks>
    /// <typeparam name="TSource"> The type of elements in the sequence. </typeparam>
    /// <param name="source"> The source sequence. </param>
    /// <param name="element"> The element to append if not null. </param>
    /// <returns> A sequence with the element appended if not null, otherwise the original sequence. </returns>
    public static IEnumerable<TSource> AppendIfNotNull<TSource>(this IEnumerable<TSource> source, TSource? element) {
        return element == null
            ? source
            : source.Append(element);
    }

    /// <summary>
    ///     Converts a dictionary to an immutable multi-map with immutable list values.
    /// </summary>
    /// <remarks>
    /// MULTI-MAP PATTERN:
    /// A multi-map is a dictionary where each key maps to multiple values (a collection).
    /// Common in generators for grouping related items:
    /// - Type name → list of members
    /// - Attribute type → list of annotated symbols
    /// - Namespace → list of types
    /// 
    /// WHY IMMUTABLE:
    /// Incremental generators cache results between invocations. Immutable collections:
    /// - Are thread-safe (IDE may access from multiple threads)
    /// - Have value-based equality (support proper caching)
    /// - Prevent accidental mutation bugs
    /// 
    /// TYPE CONSTRAINTS:
    /// - TList must be a collection type with a default constructor
    /// - TKey must be non-null (dictionary key requirement)
    /// - Converts List&lt;T&gt; values to IReadOnlyList&lt;T&gt; for immutability
    /// 
    /// USAGE:
    ///   var builders = new Dictionary&lt;string, List&lt;MethodInfo&gt;&gt;();
    ///   // ... populate builders ...
    ///   IReadOnlyDictionary&lt;string, IReadOnlyList&lt;MethodInfo&gt;&gt; immutable = 
    ///       builders.ToImmutableMultiMap();
    /// </remarks>
    /// <typeparam name="TKey"> The type of keys in the dictionary. </typeparam>
    /// <typeparam name="TValue"> The type of values in the lists. </typeparam>
    /// <typeparam name="TList"> The type of list containing the values. </typeparam>
    /// <param name="source"> The source dictionary. </param>
    /// <returns> An immutable dictionary with immutable list values. </returns>
    public static IReadOnlyDictionary<TKey, IReadOnlyList<TValue>> ToImmutableMultiMap<TKey, TValue, TList>(
        this IDictionary<TKey, TList> source
    )
        where TList : IEnumerable<TValue>, new()
        where TKey : notnull {
        return source.ToImmutableDictionary(
            element => element.Key,
            IReadOnlyList<TValue> (element) => element.Value.ToImmutableList());
    }

    // COMMENTED OUT - HISTORICAL PATTERN:
    // This method shows a pattern for building type-keyed dictionaries with error handling.
    // Left here for reference during refactoring. May be uncommented once error aggregation
    // is standardized across the generator pipeline.
    //
    // The pattern demonstrates:
    // - Duplicate detection during dictionary building
    // - Error context capture via SelectCatching
    // - Integration with diagnostic aggregation
    // - Type-safe mapping with ISourceCodeElement constraint
    //
    // internal static IReadOnlyDictionary<TypeModel, R> CreateTypeMap<T, R>(
    //     this IEnumerable<T> elements,
    //     Func<T, TypeModel> extractKey,
    //     Func<T, R> extractValue,
    //     IGeneratorContext generatorCtx
    // ) where R : ISourceCodeElement {
    //     var map = ImmutableDictionary.CreateBuilder<TypeModel, R>();
    //     elements.SelectCatching(
    //             generatorCtx.Aggregator,
    //             element => $"Mapping metadata for element {element}",
    //             element => {
    //                 var key = extractKey(element);
    //                 if (map.ContainsKey(key)) {
    //                     throw Diagnostics.InternalError.AsException(
    //                         $"Duplicate metadata for type {typeof(T).Name}.",
    //                         key.Location,
    //                         generatorCtx);
    //                 }
    //
    //                 var value = extractValue(element);
    //                 map.Add(key, value);
    //                 return (key, value);
    //             })
    //         .ToImmutableList();
    //     return map.ToImmutable();
    // }
}
