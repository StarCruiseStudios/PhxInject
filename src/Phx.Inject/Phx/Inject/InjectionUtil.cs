// -----------------------------------------------------------------------------
// <copyright file="InjectionUtil.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject;

public static class InjectionUtil {
    public static IReadOnlyList<T> Combine<T>(params IReadOnlyList<T>[] lists) {
        return lists.SelectMany(list => list).ToList();
    }

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

    public static IReadOnlyDictionary<K, V> Combine<K, V>(params IReadOnlyDictionary<K, V>[] dicts) {
        var combinedDictionary = new Dictionary<K, V>();
        foreach (IReadOnlyDictionary<K, V> dict in dicts) {
            foreach (KeyValuePair<K, V> kvp in dict) {
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
