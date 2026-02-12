// -----------------------------------------------------------------------------
// <copyright file="FunctionalExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Common.Util;

/// <summary>
///     Extension methods for functional programming patterns.
/// </summary>
public static class FunctionalExtensions {
    /// <summary>
    ///     Transforms a value using the provided function.
    /// </summary>
    /// <typeparam name="T"> The type of the input value. </typeparam>
    /// <typeparam name="TResult"> The type of the result. </typeparam>
    /// <param name="self"> The input value. </param>
    /// <param name="func"> The transformation function. </param>
    /// <returns> The result of applying the function to the input. </returns>
    public static TResult Let<T, TResult>(this T self, Func<T, TResult> func) {
        return func(self);
    }

    /// <summary>
    ///     Performs an action on a value and returns the value.
    /// </summary>
    /// <typeparam name="T"> The type of the value. </typeparam>
    /// <param name="self"> The value. </param>
    /// <param name="action"> The action to perform. </param>
    /// <returns> The original value. </returns>
    public static T Also<T>(this T self, Action<T> action) {
        action(self);
        return self;
    }

    /// <summary>
    ///     Performs an action on a value.
    /// </summary>
    /// <typeparam name="T"> The type of the value. </typeparam>
    /// <param name="self"> The value. </param>
    /// <param name="action"> The action to perform. </param>
    public static void Then<T>(this T self, Action<T> action) {
        action(self);
    }
}
