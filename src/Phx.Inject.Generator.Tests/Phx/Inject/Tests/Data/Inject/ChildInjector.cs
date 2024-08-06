// -----------------------------------------------------------------------------
//  <copyright file="ChildInjector.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Inject {
    using Phx.Inject.Tests.Data.Model;

    [Injector(typeof(ParentSpecification))]
    internal interface IParentInjector {
        [ChildInjector]
        public IChildInjector GetChildInjector();
    }

    [Injector(typeof(ChildSpecification))]
    [ExternalDependency(typeof(IChildExternalDependencies))]
    internal interface IChildInjector {
        [ChildInjector]
        public IGrandchildInjector GetGrandchildInjector();
    }

    [Injector(typeof(GrandchildSpecification))]
    [ExternalDependency(typeof(IGrandchildExternalDependencies))]
    internal interface IGrandchildInjector {
        public Root GetRoot();
    }
}
