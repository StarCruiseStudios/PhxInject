// -----------------------------------------------------------------------------
// <copyright file="IEnumerableExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Common.Util;

public static class IEnumerableExtensions {
    public static IEnumerable<TSource> AppendIfNotNull<TSource>(this IEnumerable<TSource> source, TSource? element) {
        return element == null
            ? source
            : source.Append(element);
    }
}
