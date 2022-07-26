// -----------------------------------------------------------------------------
//  <copyright file="ILabelInjector.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Data.Inject {
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;

    [Injector(
        specifications: new[] { 
            typeof(LabeledLeafSpecification)
        }
     )]
    internal interface ILabelInjector {
        public ILeaf GetDefaultLeaf();

        [Label("NonDefaultLeafA")]
        [Factory]
        public ILeaf GetNonDefaultLeafA();

        [Label("NonDefaultLeafB")]
        [Factory]
        public ILeaf GetNonDefaultLeafB();

        [NamedLeafA]
        public ILeaf GetAttributeNamedLeafA();

        [NamedLeafB]
        public ILeaf GetAttributeNamedLeafB();

        [Label("NamedLeafA")]
        [Factory]
        public ILeaf GetStringNamedLeafA();

        [Label("NamedLeafA")]
        [Factory]
        public StringLeaf GetNamedStringLeaf();

        public Node GetNode();

        [Label("NonDefaultLeafA")]
        public void BuildLabeledLazyType(LazyType type);

        public void BuildUnlabeledLazyType(LazyType type);
    }
}