// -----------------------------------------------------------------------------
//  <copyright file="ChildDependencies.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Tests.Data.Model;

namespace Phx.Inject.Tests.Data.Inject;

[PhxInject(TabSize = 3)]
internal class GeneratorSettings { }

[Specification]
internal interface IChildDependencies {
    [Label(ParentSpecification.LeftLeaf)]
    [Factory]
    public ILeaf GetLeftLeaf();

    [Label(ParentSpecification.RightLeaf)]
    [Factory]
    public ILeaf GetRightLeaf();
}

[Specification]
internal interface IGrandchildDependencies {
    [Factory]
    public Node GetNode();
}
