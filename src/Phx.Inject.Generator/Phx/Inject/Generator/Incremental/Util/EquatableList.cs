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
///     An immutable list that implements value-based equality for use in incremental generators.
/// </summary>
/// <typeparam name="T"> The type of elements in the list. </typeparam>
internal sealed class EquatableList<T>(IEnumerable<T> items) : IEquatable<EquatableList<T>>, IReadOnlyList<T> {
    /// <summary> An empty equatable list. </summary>
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
    ///     Merges two incremental value providers of equatable lists into one.
    /// </summary>
    /// <param name="left"> The left provider. </param>
    /// <param name="right"> The right provider. </param>
    /// <returns> A merged incremental value provider. </returns>
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
