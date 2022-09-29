// -----------------------------------------------------------------------------
//  <copyright file="IRawInjector.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Inject {
    using System.Diagnostics.CodeAnalysis;
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;

    [Injector(
            typeof(RootSpecification),
            typeof(LazySpecification),
            typeof(LeafSpecification),
            typeof(LeafLinks))]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal interface IRawInjector {
        public Root GetRoot();
        public Node GetNode();
        public IntLeaf GetIntLeaf();
        public StringLeaf GetStringLeaf();
        public ILeaf GetILeaf();

        public void Build(LazyType lazyType);
    }
}
