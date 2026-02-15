// -----------------------------------------------------------------------------
// <copyright file="FactoryAttributeTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests.Attributes;

/// <summary>
/// Tests for <see cref="FactoryAttribute"/>.
/// </summary>
public class FactoryAttributeTests : LoggingTestClass {
    
    [Test]
    public void DefaultConstructor_SetsDefaultFabricationMode() {
        var attribute = When("Creating FactoryAttribute with default constructor",
            () => new FactoryAttribute());
        
        Then("FabricationMode is Recurrent",
            () => Verify.That(attribute.FabricationMode.IsEqualTo(FabricationMode.Recurrent)));
    }
    
    [Test]
    public void ParameterizedConstructor_SetsFabricationMode() {
        var attribute = When("Creating FactoryAttribute with FabricationMode",
            () => new FactoryAttribute(FabricationMode.Container));
        
        Then("FabricationMode is set correctly",
            () => Verify.That(attribute.FabricationMode.IsEqualTo(FabricationMode.Container)));
    }
    
    [Test]
    public void FabricationMode_CanBeChanged() {
        var attribute = Given("A FactoryAttribute",
            () => new FactoryAttribute());
        
        When("Setting FabricationMode",
            () => attribute.FabricationMode = FabricationMode.Scoped);
        
        Then("FabricationMode is updated",
            () => Verify.That(attribute.FabricationMode.IsEqualTo(FabricationMode.Scoped)));
    }
    
    [Test]
    public void AttributeUsage_AllowsMethodAndProperty() {
        var usage = When("Getting AttributeUsage",
            () => typeof(FactoryAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Method target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Method).IsTrue()));
        Then("Allows Property target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Property).IsTrue()));
    }
}
