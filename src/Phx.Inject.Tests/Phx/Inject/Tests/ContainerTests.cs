// -----------------------------------------------------------------------------
// <copyright file="ContainerTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Tests.Data.Model;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

#region injector

[Specification]
internal static class ContainerSpecification {
    private static int currentInt;

    [Factory(FabricationMode.ContainerScoped)]
    internal static int GetInt() {
        return currentInt++;
    }

    [Factory(FabricationMode.Scoped)]
    internal static StringLeaf GetStringLeaf(int value) {
        return new StringLeaf(value.ToString());
    }

    [Factory]
    [Partial]
    internal static List<IntLeaf> GetIntLeaf1(IntLeaf leaf) {
        return new List<IntLeaf> {
            leaf
        };
    }

    [Factory]
    [Partial]
    internal static List<IntLeaf> GetIntLeaf2(IntLeaf leaf) {
        return new List<IntLeaf> {
            leaf
        };
    }

    [Factory(FabricationMode.Container)]
    internal static Node GetNode(List<IntLeaf> leaves) {
        var left = leaves[0];
        var right = leaves[1];
        return new Node(left, right);
    }

    [Factory(FabricationMode.Container)]
    [Label("WithScoped")]
    internal static Node GetNode2(IntLeaf intLeaf, StringLeaf stringLeaf) {
        return new Node(intLeaf, stringLeaf);
    }
}

[Injector(typeof(ContainerSpecification))]
public interface IContainerInjector {
    Node GetNode();

    [Label("WithScoped")]
    Node GetNodeWithScoped();

    List<IntLeaf> GetIntLeaves();

    StringLeaf GetStringLeaf();
}

#endregion injector

public class ContainerTests : LoggingTestClass {
    [Test]
    public void ContainersReuseTypes() {
        IContainerInjector injector = Given("A test injector.", () => new GeneratedContainerInjector());

        var node = When("Getting a container type", () => injector.GetNode());

        Then("The container reuses the same instance of a type",
            () => {
                Verify.That(node.Left.IsReferenceNotEqualTo(node.Right));
                Verify.That((node.Left as IntLeaf)!.Value.IsEqualTo((node.Right as IntLeaf)!.Value));
            });
    }

    [Test]
    public void DifferentContainersDontReuseTypes() {
        IContainerInjector injector = Given("A test injector.", () => new GeneratedContainerInjector());

        var node1 = When("Getting a container type", () => injector.GetNode());
        var node2 = When("Getting another container type", () => injector.GetNode());

        Then("The first container reuses the same instance of a type",
            () => {
                Verify.That(node1.Left.IsReferenceNotEqualTo(node1.Right));
                Verify.That((node1.Left as IntLeaf)!.Value.IsEqualTo((node1.Right as IntLeaf)!.Value));
            });

        Then("The second container reuses the same instance of a type",
            () => {
                Verify.That(node2.Left.IsReferenceNotEqualTo(node2.Right));
                Verify.That((node2.Left as IntLeaf)!.Value.IsEqualTo((node2.Right as IntLeaf)!.Value));
            });

        Then("The container have different instances",
            () => {
                Verify.That((node1.Left as IntLeaf)!.Value.IsNotEqualTo((node2.Left as IntLeaf)!.Value));
                Verify.That((node1.Right as IntLeaf)!.Value.IsNotEqualTo((node2.Right as IntLeaf)!.Value));
            });
    }

    [Test]
    public void ChildContainersDontReuseTypes() {
        IContainerInjector injector = Given("A test injector.", () => new GeneratedContainerInjector());

        IReadOnlyList<IntLeaf> leafList = When("Getting a container type", () => injector.GetIntLeaves());
        var node = When("Getting another container type", () => injector.GetNode());

        Then("The first container reuses the same instance of a type",
            () => {
                Verify.That(leafList[0].IsReferenceNotEqualTo(leafList[1]));
                Verify.That(leafList[0].Value.IsEqualTo(leafList[1].Value));
            });

        Then("The second container reuses the same instance of a type",
            () => {
                Verify.That(node.Left.IsReferenceNotEqualTo(node.Right));
                Verify.That((node.Left as IntLeaf)!.Value.IsEqualTo((node.Right as IntLeaf)!.Value));
            });

        Then("The container have different instances",
            () => {
                Verify.That(leafList[0].Value.IsNotEqualTo((node.Left as IntLeaf)!.Value));
                Verify.That(leafList[1].Value.IsNotEqualTo((node.Right as IntLeaf)!.Value));
            });
    }

    [Test]
    public void ChildContainersDoReuseScopedTypes() {
        IContainerInjector injector = Given("A test injector.", () => new GeneratedContainerInjector());

        var node = When("Getting a container type", () => injector.GetNodeWithScoped());
        var stringLeaf = When("Getting a scoped type", () => injector.GetStringLeaf());

        Then("The scoped value was reused",
            () => { Verify.That(node.Right.IsReferenceEqualTo(stringLeaf)); });
    }

    [Test]
    public void PeerContainersDoReuseScopedTypes() {
        IContainerInjector injector = Given("A test injector.", () => new GeneratedContainerInjector());

        var node1 = When("Getting a container type", () => injector.GetNodeWithScoped());
        var node2 = When("Getting a container type", () => injector.GetNodeWithScoped());

        Then("The scoped value was reused",
            () => { Verify.That(node1.Right.IsReferenceEqualTo(node2.Right)); });

        Then("The unscoped value was not reused",
            () => { Verify.That((node1.Left as IntLeaf)!.Value.IsNotEqualTo((node2.Left as IntLeaf)!.Value)); });
    }
}
