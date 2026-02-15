// -----------------------------------------------------------------------------
// <copyright file="FabricationModeTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

/// <summary>
/// Tests for <see cref="FabricationMode"/> enum.
/// </summary>
public class FabricationModeTests : LoggingTestClass {
    
    [Test]
    public void Recurrent_HasCorrectValue() {
        var mode = When("Getting Recurrent mode", () => FabricationMode.Recurrent);
        
        Then("Value is 0", () => Verify.That(((int)mode).IsEqualTo(0)));
    }
    
    [Test]
    public void Scoped_HasCorrectValue() {
        var mode = When("Getting Scoped mode", () => FabricationMode.Scoped);
        
        Then("Value is 1", () => Verify.That(((int)mode).IsEqualTo(1)));
    }
    
    [Test]
    public void Container_HasCorrectValue() {
        var mode = When("Getting Container mode", () => FabricationMode.Container);
        
        Then("Value is 2", () => Verify.That(((int)mode).IsEqualTo(2)));
    }
    
    [Test]
    public void ContainerScoped_HasCorrectValue() {
        var mode = When("Getting ContainerScoped mode", () => FabricationMode.ContainerScoped);
        
        Then("Value is 3", () => Verify.That(((int)mode).IsEqualTo(3)));
    }
    
    [Test]
    public void AllValues_AreDistinct() {
        var recurrent = Given("Recurrent", () => FabricationMode.Recurrent);
        var scoped = Given("Scoped", () => FabricationMode.Scoped);
        var container = Given("Container", () => FabricationMode.Container);
        var containerScoped = Given("ContainerScoped", () => FabricationMode.ContainerScoped);
        
        Then("All values are distinct", () => {
            Verify.That(recurrent.IsNotEqualTo(scoped));
            Verify.That(recurrent.IsNotEqualTo(container));
            Verify.That(recurrent.IsNotEqualTo(containerScoped));
            Verify.That(scoped.IsNotEqualTo(container));
            Verify.That(scoped.IsNotEqualTo(containerScoped));
            Verify.That(container.IsNotEqualTo(containerScoped));
        });
    }
}
