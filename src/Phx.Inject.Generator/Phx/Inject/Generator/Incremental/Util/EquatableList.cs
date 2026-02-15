// -----------------------------------------------------------------------------
// <copyright file="EquatableList.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using System.Collections;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#endregion

namespace Phx.Inject.Generator.Incremental.Util;

/// <summary>
///     Immutable list with structural equality semantics, optimized for Roslyn incremental generators.
/// </summary>
/// <typeparam name="T">The type of elements. Must implement value equality correctly.</typeparam>
/// <param name="items">Source sequence to populate the list. Copied immediately to ensure immutability.</param>
/// <remarks>
///     ## Design Rationale
///
///     Roslyn's incremental generators rely on value equality to cache pipeline results. Standard
///     .NET collections use reference equality, causing unnecessary recompilation when logically
///     identical lists are reconstructed. This type provides structural equality while maintaining
///     immutability guarantees required by incremental generators.
///     
///     ## Performance Characteristics
///
///     - Construction: O(n) - copies all elements into an <see cref="ImmutableList{T}"/>
///     - Equality: O(n) worst case - short-circuits on count mismatch or first difference
///     - GetHashCode: O(n) - computed once and cached by value types/records
///     - Indexing: O(log n) - uses ImmutableList's tree structure
///     
///     ## Immutability Contract
///
///     Once constructed, the list contents never change. This is essential for incremental generators
///     where cached results must remain stable. Modifications require creating a new instance.
///     
///     ## Equality Semantics
///
///     Two lists are equal if they contain the same elements in the same order, using
///     <see cref="EqualityComparer{T}.Default"/> for element comparison. Type <c>T</c> must implement
///     proper value equality or the incremental cache will behave incorrectly.
///     
///     ## Thread Safety
///
///     Immutable and thread-safe for reads. Multiple threads can safely enumerate or index
///     the same instance concurrently.
///     
///     ## Usage Pattern
///
///     Used extensively in metadata records to represent collections of types, parameters,
///     attributes, etc. The value-equality enables Roslyn to recognize when a collection hasn't
///     changed semantically, avoiding unnecessary downstream pipeline re-execution.
/// </remarks>
internal sealed class EquatableList<T>(IEnumerable<T> items) : IEquatable<EquatableList<T>>, IReadOnlyList<T> {
    /// <summary>
    ///     Singleton empty list instance to avoid allocations for common empty case.
    /// </summary>
    /// <remarks>
    ///     Prefer using this constant over constructing new empty instances.
    ///     Reduces memory pressure and improves equality comparison performance.
    /// </remarks>
    public static readonly EquatableList<T> Empty = new(ImmutableList<T>.Empty);
    
    /// <summary>
    ///     Creates an equatable list from a sequence of items.
    /// </summary>
    /// <param name="items"> The items to include in the list. </param>
    /// <returns> A new equatable list. </returns>
    public static EquatableList<T> Create(IEnumerable<T> items) => new(items);
    
    /// <summary>
    ///     Creates an equatable list from an array of items.
    /// </summary>
    /// <param name="items"> The items to include in the list. </param>
    /// <returns> A new equatable list. </returns>
    public static EquatableList<T> Create(params T[] items) => new(items);
    
    /// <summary>
    ///     Merges two incremental value providers of lists into a single combined list.
    /// </summary>
    /// <param name="left">First source of list values.</param>
    /// <param name="right">Second source of list values.</param>
    /// <returns>
    ///     An incremental provider that concatenates items from both sources.
    ///     Re-executes only when either input changes.
    /// </returns>
    /// <remarks>
    ///     Used in pipeline orchestration to aggregate results from parallel pipeline branches.
    ///     Roslyn's incremental compiler will cache this merge operation, only recomputing
    ///     when one of the source lists changes.
    /// </remarks>
    public static IncrementalValueProvider<EquatableList<T>> Merge(
        IncrementalValueProvider<EquatableList<T>> left,
        IncrementalValueProvider<EquatableList<T>> right
    ) {
        return left.Combine(right).Select((pair, _) => pair.Left.Concat(pair.Right).ToEquatableList());
    }
    
    /// <summary> The immutable list of items. </summary>
    private readonly ImmutableList<T> items = items.ToImmutableList();
    
    /// <inheritdoc />
    public int Count => items.Count;

    /// <inheritdoc />
    public T this[int index] => items[index];

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public bool Equals(EquatableList<T>? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (items.Count != other.items.Count) return false;

        for (int i = 0; i < items.Count; i++) {
            if (!EqualityComparer<T>.Default.Equals(items[i], other.items[i])) {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EquatableList<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() {
        int hash = 17;
        foreach (var item in items) {
            hash = hash * 31 + EqualityComparer<T>.Default.GetHashCode(item);
        }
        return hash;
    }
}

/// <summary>
///     Extension methods for creating <see cref="EquatableList{T}"/> instances.
/// </summary>
internal static class EquatableListExtensions {
    /// <summary>
    ///     Converts a sequence to an equatable list.
    /// </summary>
    /// <typeparam name="T"> The type of elements. </typeparam>
    /// <param name="items"> The items to convert. </param>
    /// <returns> A new equatable list. </returns>
    public static EquatableList<T> ToEquatableList<T>(this IEnumerable<T> items) => new(items);
}
