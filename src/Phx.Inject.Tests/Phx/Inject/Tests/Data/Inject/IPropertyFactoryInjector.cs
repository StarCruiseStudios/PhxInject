﻿// -----------------------------------------------------------------------------
//  <copyright file="IPropertyFactoryInjector.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Inject {
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;

    [Injector(
            specifications: new[] {
                typeof(IPropertyFactorySpec), typeof(PropertyFactoryStaticSpec)
            })]
    internal interface IPropertyFactoryInjector {
        public ILeaf GetLeaf();
        public StringLeaf GetStringLeaf();
    }
}
