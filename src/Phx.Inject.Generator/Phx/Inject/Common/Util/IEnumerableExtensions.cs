// -----------------------------------------------------------------------------
// <copyright file="IEnumerableExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;

namespace Phx.Inject.Common.Util;

public static class IEnumerableExtensions {
    public static IEnumerable<TSource> AppendIfNotNull<TSource>(this IEnumerable<TSource> source, TSource? element) {
        return element == null
            ? source
            : source.Append(element);
    }

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
