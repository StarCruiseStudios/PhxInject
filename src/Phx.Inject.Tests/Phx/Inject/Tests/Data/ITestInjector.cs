// -----------------------------------------------------------------------------
//  <copyright file="ITestInjector.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data {
    [Injector(
        generatedClassName: "CustomInjector",
        specifications: new[] {
            typeof(RootSpecification),
            typeof(LazySpecification),
            typeof(LeafSpecification)
        }
    )]
    internal interface ITestInjector {
        public Root GetRoot();

        // TODO:
        // public void Build(LazyType lazyType);
    }
}
