// -----------------------------------------------------------------------------
// <copyright file="MultiBindSpecification.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    public static class MultiBindSpecification {
        [Factory]
        [Partial]
        internal static List<ILeaf> GetListLeaf1() {
            return new List<ILeaf> {
                new IntLeaf(10)
            };
        }

        [Factory]
        [Partial]
        internal static List<ILeaf> GetListLeaf2() {
            return new List<ILeaf> {
                new IntLeaf(20)
            };
        }

        [Factory]
        [Partial]
        internal static HashSet<ILeaf> GetSetLeaf1() {
            return new HashSet<ILeaf> {
                new IntLeaf(30)
            };
        }

        [Factory]
        [Partial]
        internal static HashSet<ILeaf> GetSetLeaf2() {
            return new HashSet<ILeaf> {
                new IntLeaf(40)
            };
        }

        [Factory]
        [Partial]
        internal static Dictionary<string, ILeaf> GetDictLeaf1() {
            return new Dictionary<string, ILeaf> {
                {
                    "key1", new IntLeaf(50)
                }
            };
        }

        [Factory]
        [Partial]
        internal static Dictionary<string, ILeaf> GetDictLeaf2() {
            return new Dictionary<string, ILeaf> {
                {
                    "key2", new IntLeaf(60)
                }
            };
        }
    }
}
