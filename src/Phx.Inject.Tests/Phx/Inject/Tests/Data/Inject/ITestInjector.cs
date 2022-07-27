// -----------------------------------------------------------------------------
//  <copyright file="ITestInjector.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Inject {
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;

    [Injector(
            "CustomInjector",
            new[] {
                typeof(RootSpecification),
                typeof(LazySpecification),
                typeof(LeafSpecification),
                typeof(LeafLinks)
            })]
    internal interface ITestInjector {
        public Root GetRoot();

        public void Build(LazyType lazyType);
    }
}
