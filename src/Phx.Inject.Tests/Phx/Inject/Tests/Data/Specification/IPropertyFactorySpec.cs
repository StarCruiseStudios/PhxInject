// -----------------------------------------------------------------------------
//  <copyright file="IPropertyFactorySpec.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    internal interface IPropertyFactorySpec {
        [Factory]
        public ILeaf Leaf { get; }
    }

    internal record PropertyFactorySpec(ILeaf Leaf) : IPropertyFactorySpec;

    [Specification]
    internal static class PropertyFactoryStaticSpec {
        public const string StringValue = "Hello";

        [Factory]
        public static StringLeaf StringLeaf { get; } = new(StringValue);
    }
}
