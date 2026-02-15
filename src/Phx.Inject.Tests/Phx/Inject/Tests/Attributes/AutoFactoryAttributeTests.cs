// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="AutoFactoryAttribute"/>.
/// </summary>
public class AutoFactoryAttributeTests : LoggingTestClass {
    
    [Test]
    public void DefaultConstructor_SetsDefaultFabricationMode() {
        var attribute = When("Creating AutoFactoryAttribute with default constructor",
            () => new AutoFactoryAttribute());
        
        Then("FabricationMode is Recurrent",
            () => Verify.That(attribute.FabricationMode.IsEqualTo(FabricationMode.Recurrent)));
    }
    
    [Test]
    public void ParameterizedConstructor_SetsFabricationMode() {
        var attribute = When("Creating AutoFactoryAttribute with FabricationMode",
            () => new AutoFactoryAttribute(FabricationMode.Scoped));
        
        Then("FabricationMode is set correctly",
            () => Verify.That(attribute.FabricationMode.IsEqualTo(FabricationMode.Scoped)));
    }
    
    [Test]
    public void FabricationMode_CanBeChanged() {
        var attribute = Given("An AutoFactoryAttribute",
            () => new AutoFactoryAttribute());
        
        When("Setting FabricationMode",
            () => attribute.FabricationMode = FabricationMode.ContainerScoped);
        
        Then("FabricationMode is updated",
            () => Verify.That(attribute.FabricationMode.IsEqualTo(FabricationMode.ContainerScoped)));
    }
    
    [Test]
    public void AttributeUsage_AllowsClassAndConstructor() {
        var usage = When("Getting AttributeUsage",
            () => typeof(AutoFactoryAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Class target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Class).IsTrue()));
        Then("Allows Constructor target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Constructor).IsTrue()));
    }
}
