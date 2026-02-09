// -----------------------------------------------------------------------------
// <copyright file="EquatableList.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections;
using System.Collections.Immutable;

namespace Phx.Inject.Generator.Incremental.Util;

internal sealed class EquatableList<T>(IEnumerable<T> items) : IEquatable<EquatableList<T>>, IReadOnlyList<T> {
    public static readonly EquatableList<T> Empty = new(ImmutableList<T>.Empty);
    public static EquatableList<T> Create(IEnumerable<T> items) => new(items);
    public static EquatableList<T> Create(params T[] items) => new(items);
    
    private readonly ImmutableList<T> items = items.ToImmutableList();
    public int Count => items.Count;

    public T this[int index] => items[index];

    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

    public override bool Equals(object? obj) => obj is EquatableList<T> other && Equals(other);

    public override int GetHashCode() {
        int hash = 17;
        foreach (var item in items) {
            hash = hash * 31 + EqualityComparer<T>.Default.GetHashCode(item);
        }
        return hash;
    }
}

internal static class EquatableListExtensions {
    public static EquatableList<T> ToEquatableList<T>(this IEnumerable<T> items) => new(items);
}
