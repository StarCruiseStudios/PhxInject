// -----------------------------------------------------------------------------
//  <copyright file="IFactoryReferenceInjector.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Inject {
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;

    [Injector(
            typeof(FactoryReferenceSpec)
    )]
    internal interface IFactoryReferenceInjector {
        public IntLeaf GetIntLeaf();
        public StringLeaf GetStringLeaf();

        public void Build(LazyType lazyType);

        [Label("Field")]
        public void BuildField(LazyType lazyType);
    }
}
