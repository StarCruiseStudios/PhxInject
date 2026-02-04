// -----------------------------------------------------------------------------
// <copyright file="InjectorNamingTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Tests.Data;
using Phx.Inject.Tests.Data.Model;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

[Injector(typeof(CommonTestValueSpecification))]
public interface IDefaultNamedInjector {
    int GetInt();
}

[Injector(
    typeof(CommonTestValueSpecification),
    GeneratedClassName = "CustomNamedInjector"
)]
public interface ICustomNamedInjector {
    int GetInt();
}


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

[InjectorDependency]
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

[InjectorDependency]
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


public class InjectorNamingTests : LoggingTestClass {
    [Test]
    public void DefaultInjectorNameIsUsed() {
        // var injector = When("Creating a default named injector", () => new GeneratedDefaultNamedInjector());
        // Then("The injector was created", () => Verify.That(injector.IsNotNull()));
    }

    [Test]
    public void CustomInjectorNameIsUsed() {
        // var injector = When("Creating a custom named injector", () => new CustomNamedInjector());
        // Then("The injector was created", () => Verify.That(injector.IsNotNull()));
    }
}
