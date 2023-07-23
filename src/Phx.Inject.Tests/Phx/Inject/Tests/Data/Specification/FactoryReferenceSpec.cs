// -----------------------------------------------------------------------------
//  <copyright file="FactoryReferenceSpec.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Specification {
    using System;
    using Phx.Inject.Tests.Data.Model;

    [Specification]
    [Link(typeof(IntLeaf), typeof(ILeaf))]
    internal static class FactoryReferenceSpec {
        [Factory]
        internal static int IntValue => 10;

        [Factory]
        internal static string StringValue => "stringvalue";

        [BuilderReference]
        internal static Action<LazyType, ILeaf> BuildLazyType => LazyType.Inject;

        [Label("Field")]
        [BuilderReference]
        internal static readonly Action<LazyType, ILeaf> BuildLazyTypeField = LazyType.Inject;

        [FactoryReference(FabricationMode.Scoped)]
        internal static Func<int, IntLeaf> GetIntLeaf => IntLeaf.Construct;

        [FactoryReference(FabricationMode.Scoped)]
        internal static readonly Func<string, StringLeaf> GetStringLeaf = StringLeaf.Construct;
    }
}
