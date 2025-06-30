// -----------------------------------------------------------------------------
//  <copyright file="ChildDependencies.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Tests.Data.Model;

namespace Phx.Inject.Tests.Data.Inject;

internal interface IChildDependencies {
    [Label(ParentSpecification.LeftLeaf)]
    public ILeaf GetLeftLeaf();

    [Label(ParentSpecification.RightLeaf)]
    public ILeaf GetRightLeaf();
}

internal interface IGrandchildDependencies {
    public Node GetNode();
}
