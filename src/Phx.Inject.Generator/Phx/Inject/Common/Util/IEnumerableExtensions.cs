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
///     Extension methods for working with <see cref="IEnumerable{T}"/> collections.
/// </summary>
public static class IEnumerableExtensions {
    /// <summary>
    ///     Appends an element to a sequence only if the element is not null.
    /// </summary>
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
