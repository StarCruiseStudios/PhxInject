// -----------------------------------------------------------------------------
//  <copyright file="ILabelInjector.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Inject {
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;

    [Injector(typeof(LabeledLeafSpecification))]
    internal interface ILabelInjector {
        public ILeaf GetDefaultLeaf();

        [Label("NonDefaultLeafA")]
        public ILeaf GetNonDefaultLeafA();

        [Label("NonDefaultLeafB")]
        public ILeaf GetNonDefaultLeafB();

        [NamedLeafA]
        public ILeaf GetAttributeNamedLeafA();

        [NamedLeafB]
        public ILeaf GetAttributeNamedLeafB();

        [Label("NamedLeafA")]
        public ILeaf GetStringNamedLeafA();

        [Label("NamedLeafA")]
        public StringLeaf GetNamedStringLeaf();

        public Node GetNode();

        [Label("NonDefaultLeafA")]
        public void BuildLabeledLazyType(LazyType type);

        public void BuildUnlabeledLazyType(LazyType type);
    }
}
