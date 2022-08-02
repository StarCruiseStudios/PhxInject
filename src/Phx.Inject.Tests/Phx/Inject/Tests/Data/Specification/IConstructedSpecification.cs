// -----------------------------------------------------------------------------
//  <copyright file="IConstructedSpecification.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    internal interface IConstructedSpecification {
        [Factory]
        public int GetIntValue();
    }

    [Specification]
    internal static class NonConstructedSpecification {
        [Factory]
        public static IntLeaf GetIntLeaf(int intValue) {
            return new IntLeaf(intValue);
        }
    }

    internal class ConstructedSpecificationImplementation : IConstructedSpecification {
        public const int IntValue = 101;
        public int GetIntValue() {
            return IntValue;
        }
    }
}
