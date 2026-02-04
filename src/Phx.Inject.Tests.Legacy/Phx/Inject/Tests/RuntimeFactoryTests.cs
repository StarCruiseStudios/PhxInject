// -----------------------------------------------------------------------------
// <copyright file="RuntimeFactoryTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
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
internal static class RuntimeFactorySpecification {
    [Factory]
    internal static ILeaf GetLeaf() {
        return new IntLeaf(10);
    }

    [Factory]
    [Label("LabeledLeaf")]
    internal static ILeaf GetLabeledLeaf() {
        return new IntLeaf(42);
    }

    [Factory]
    internal static LeafFactory GetLeafFactory(Factory<ILeaf> factory) {
        return new LeafFactory(factory.Create);
    }

    [Factory]
    [Label("LabeledLeaf")]
    internal static LeafFactory GetLabeledLeafFactory([Label("LabeledLeaf")] Factory<ILeaf> factory) {
        return new LeafFactory(factory.Create);
    }
}

[Injector(typeof(RuntimeFactorySpecification))]
public interface IRuntimeFactoryInjector {
    LeafFactory GetLeafFactory();
    Factory<ILeaf> GetLeafRuntimeFactory();

    [Label("LabeledLeaf")]
    LeafFactory GetLabeledLeafFactory();

    [Label("LabeledLeaf")]
    Factory<ILeaf> GetLabeledLeafRuntimeFactory();
}

#endregion injector

public class RuntimeFactoryTests : LoggingTestClass {
    [Test]
    public void RuntimeFactoryIsInjected() {
        IRuntimeFactoryInjector injector = Given("A test injector.", () => new GeneratedRuntimeFactoryInjector());

        var leafFactory = When("Getting a type injected with a runtime factory", () => injector.GetLeafFactory());

        var leaf = Then("The runtime factory can be used to create an instance",
            () => {
                var leaf = leafFactory.CreateLeaf();
                Verify.That(leaf.IsNotNull());
                return leaf;
            });
        Then("The correct value was returned",
            () => {
                Verify.That(leaf.IsType<IntLeaf>());
                Verify.That((leaf as IntLeaf)!.Value.IsEqualTo(10));
            });
    }

    [Test]
    public void LabeledRuntimeFactoryIsInjected() {
        IRuntimeFactoryInjector injector = Given("A test injector.", () => new GeneratedRuntimeFactoryInjector());

        var leafFactory = When("Getting a type injected with a runtime factory",
            () => injector.GetLabeledLeafFactory());

        var leaf = Then("The runtime factory can be used to create an instance",
            () => {
                var leaf = leafFactory.CreateLeaf();
                Verify.That(leaf.IsNotNull());
                return leaf;
            });
        Then("The correct value was returned",
            () => {
                Verify.That(leaf.IsType<IntLeaf>());
                Verify.That((leaf as IntLeaf)!.Value.IsEqualTo(42));
            });
    }

    [Test]
    public void RuntimeFactoryIsProvidedOnInjector() {
        IRuntimeFactoryInjector injector = Given("A test injector.", () => new GeneratedRuntimeFactoryInjector());

        Factory<ILeaf> leafFactory = When("Getting a type injected with a runtime factory",
            () => injector.GetLeafRuntimeFactory());

        var leaf = Then("The runtime factory can be used to create an instance",
            () => {
                var leaf = leafFactory.Create();
                Verify.That(leaf.IsNotNull());
                return leaf;
            });
        Then("The correct value was returned",
            () => {
                Verify.That(leaf.IsType<IntLeaf>());
                Verify.That((leaf as IntLeaf)!.Value.IsEqualTo(10));
            });
    }

    [Test]
    public void LabeledRuntimeFactoryIsProvidedOnInjector() {
        IRuntimeFactoryInjector injector = Given("A test injector.", () => new GeneratedRuntimeFactoryInjector());

        Factory<ILeaf> leafFactory = When("Getting a type injected with a runtime factory",
            () => injector.GetLabeledLeafRuntimeFactory());

        var leaf = Then("The runtime factory can be used to create an instance",
            () => {
                var leaf = leafFactory.Create();
                Verify.That(leaf.IsNotNull());
                return leaf;
            });
        Then("The correct value was returned",
            () => {
                Verify.That(leaf.IsType<IntLeaf>());
                Verify.That((leaf as IntLeaf)!.Value.IsEqualTo(42));
            });
    }
}
