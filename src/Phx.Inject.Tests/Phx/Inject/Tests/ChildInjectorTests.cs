// -----------------------------------------------------------------------------
//  <copyright file="ChildInjectorTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Tests.Data.Model;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

#region Parent

[Specification]
internal static class ParentSpecification {
    public const string LeftLeaf = "Left";
    public const string RightLeaf = "Right";

    [Factory]
    [Label(LeftLeaf)]
    internal static ILeaf GetLeftLeaf() {
        return new StringLeaf(LeftLeaf);
    }

    [Factory]
    [Label(RightLeaf)]
    internal static ILeaf GetRightLeaf() {
        return new StringLeaf(RightLeaf);
    }
}

[Injector(typeof(ParentSpecification))]
internal interface IParentInjector {
    [ChildInjector]
    public IChildInjector GetChildInjector();
}

#endregion Parent

#region Child

[Specification]
internal static class ChildSpecification {
    [Factory]
    internal static Node GetNode(
        [Label(ParentSpecification.LeftLeaf)] ILeaf left,
        [Label(ParentSpecification.RightLeaf)] ILeaf right) {
        return new Node(left, right);
    }
}

[Specification]
internal interface IChildDependencies {
    [Label(ParentSpecification.LeftLeaf)]
    [Factory]
    public ILeaf GetLeftLeaf();

    [Label(ParentSpecification.RightLeaf)]
    [Factory]
    public ILeaf GetRightLeaf();
}

[Injector(typeof(ChildSpecification))]
[Dependency(typeof(IChildDependencies))]
internal interface IChildInjector {
    [ChildInjector]
    public IGrandchildInjector GetGrandchildInjector();
}

#endregion Child

#region Grandchild

[Specification]
internal static class GrandchildSpecification {
    [Factory]
    internal static Root GetRoot(Node node, Node secondaryNode) {
        return new Root(node, secondaryNode);
    }
}

[Specification]
internal interface IGrandchildDependencies {
    [Factory]
    public Node Node { get; }
}

[Injector(typeof(GrandchildSpecification))]
[Dependency(typeof(IGrandchildDependencies))]
internal interface IGrandchildInjector {
    public Root GetRoot();
}

#endregion Grandchild

public class ChildInjectorTests : LoggingTestClass {
    [Test] public void ChildInjectorsHaveAccessToParentDependencies() {
        var parentInjector = Given("A parent injector", () => new GeneratedParentInjector());
        var childInjector = Given("A child injector", () => parentInjector.GetChildInjector());
        var grandchildInjector = Given("A grandchild injector", () => childInjector.GetGrandchildInjector());

        var root = When(
            "A factory method is invoked on the grandchild injector.",
            () => grandchildInjector.GetRoot());

        var node = Then(
            "The values from the child are returned",
            () => {
                var node = root.Node;
                Verify.That(node.IsNotNull());
                return node;
            });

        Then(
            "The values from the parent are returned",
            ParentSpecification.LeftLeaf,
            expected => Verify.That((node.Left as StringLeaf)!.Value.IsEqualTo(expected)));

        Then(
            "The values from the parent are returned",
            ParentSpecification.RightLeaf,
            expected => Verify.That((node.Right as StringLeaf)!.Value.IsEqualTo(expected)));
    }
}
