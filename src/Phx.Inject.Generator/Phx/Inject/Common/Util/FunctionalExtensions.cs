// -----------------------------------------------------------------------------
// <copyright file="FunctionalExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Common.Util;

public static class FunctionalExtensions {
    public static TResult Let<T, TResult>(this T self, Func<T, TResult> func) {
        return func(self);
    }

    public static T Also<T>(this T self, Action<T> action) {
        action(self);
        return self;
    }

    public static void Then<T>(this T self, Action<T> action) {
        action(self);
    }

    public static IEnumerable<TSource> AppendIfNotNull<TSource>(this IEnumerable<TSource> source, TSource? element) {
        return element == null
            ? source
            : source.Append(element);
    }
}
