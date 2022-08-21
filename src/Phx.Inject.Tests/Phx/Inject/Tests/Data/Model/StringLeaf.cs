// -----------------------------------------------------------------------------
//  <copyright file="StringLeaf.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Model {
    internal class StringLeaf : ILeaf {
        public string Value { get; }

        public StringLeaf(string value) {
            Value = value;
        }

        public static StringLeaf Construct(string value) {
            return new StringLeaf(value);
        }
    }
}
