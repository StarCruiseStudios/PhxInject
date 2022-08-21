// -----------------------------------------------------------------------------
//  <copyright file="IntLeaf.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Model {
    internal class IntLeaf : ILeaf {
        public int Value { get; }

        public IntLeaf(int value) {
            Value = value;
        }

        public static IntLeaf Construct(int value) {
            return new IntLeaf(value);
        }
    }
}
