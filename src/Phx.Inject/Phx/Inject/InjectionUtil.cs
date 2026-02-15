// -----------------------------------------------------------------------------
// <copyright file="InjectionUtil.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

/// <summary>
///     Provides utility methods for combining collections in generated injector code.
/// </summary>
/// <remarks>
/// These methods are used by generated injector implementations to combine multiple
/// collection instances from different specifications or builders. They ensure that
/// combined sets and maps maintain uniqueness constraints and throw exceptions if
/// duplicate values or keys are detected.
///
/// ## Usage Context
///
/// These methods are typically invoked by generated code when an injector method
/// returns a collection type and multiple specifications provide values for that
/// collection. The generator emits calls to these methods to safely combine the results.
/// </remarks>
public static class InjectionUtil {
    /// <summary>
    ///     Combines multiple read-only lists into a single list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the lists.</typeparam>
    /// <param name="lists">The read-only lists to combine.</param>
    /// <returns>
    ///     A read-only list containing all elements from all input lists in order.
    /// </returns>
    public static IReadOnlyList<T> Combine<T>(params IReadOnlyList<T>[] lists) {
        return lists.SelectMany(list => list).ToList();
    }

    /// <summary>
    ///     Combines multiple sets into a single set, ensuring no duplicate values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sets.</typeparam>
    /// <param name="sets">The sets to combine.</param>
    /// <returns>
    ///     A set containing all unique elements from all input sets.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when duplicate values are found across different input sets.
    /// </exception>
    public static ISet<T> Combine<T>(params ISet<T>[] sets) {
        return sets.Aggregate(new HashSet<T>(),
            (acc, set) => {
                if (acc.Overlaps(set)) {
                    throw new InvalidOperationException(
                        $"Injected set values must be unique. Found duplicate values {string.Join(", ", acc.Intersect(set))}");
                }

                acc.UnionWith(set);
                return acc;
            });
    }

    /// <summary>
    ///     Combines multiple enumerable sequences into a single set, ensuring no duplicate values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequences.</typeparam>
    /// <param name="sets">The enumerable sequences to combine.</param>
    /// <returns>
    ///     A set containing all unique elements from all input sequences.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when duplicate values are found across different input sequences.
    /// </exception>
    public static ISet<T> CombineReadOnlySet<T>(params IEnumerable<T>[] sets) {
        return sets.Aggregate(new HashSet<T>(),
            (acc, set) => {
                foreach (var x in set) {
                    if (acc.Contains(x)) {
                        throw new InvalidOperationException(
                            $"Injected set values must be unique. Found duplicate value {x}.");
                    }

                    acc.Add(x);
                }

                return acc;
            });
    }

    /// <summary>
    ///     Combines multiple read-only dictionaries into a single dictionary, ensuring no duplicate keys.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionaries.</typeparam>
    /// <typeparam name="V">The type of values in the dictionaries.</typeparam>
    /// <param name="dicts">The read-only dictionaries to combine.</param>
    /// <returns>
    ///     A read-only dictionary containing all key-value pairs from all input dictionaries.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when duplicate keys are found across different input dictionaries.
    /// </exception>
    public static IReadOnlyDictionary<K, V> Combine<K, V>(params IReadOnlyDictionary<K, V>[] dicts) {
        var combinedDictionary = new Dictionary<K, V>();
        foreach (var dict in dicts) {
            foreach (var kvp in dict) {
                if (combinedDictionary.ContainsKey(kvp.Key)) {
                    throw new InvalidOperationException(
                        $"Injected map keys must be unique. Found duplicate key {kvp.Key}.");
                }

                combinedDictionary.Add(kvp.Key, kvp.Value);
            }
        }

        return combinedDictionary;
    }
}
