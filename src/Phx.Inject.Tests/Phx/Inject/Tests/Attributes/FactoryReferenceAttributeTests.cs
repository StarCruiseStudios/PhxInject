// -----------------------------------------------------------------------------
// <copyright file="FactoryReferenceAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="FactoryReferenceAttribute"/>.
/// </summary>
public class FactoryReferenceAttributeTests : LoggingTestClass {
    
    [Test]
    public void DefaultConstructor_SetsDefaultFabricationMode() {
        var attribute = When("Creating FactoryReferenceAttribute with default constructor",
            () => new FactoryReferenceAttribute());
        
        Then("FabricationMode is Recurrent",
            () => Verify.That(attribute.FabricationMode.IsEqualTo(FabricationMode.Recurrent)));
    }
    
    [Test]
    public void ParameterizedConstructor_SetsFabricationMode() {
        var attribute = When("Creating FactoryReferenceAttribute with FabricationMode",
            () => new FactoryReferenceAttribute(FabricationMode.ContainerScoped));
        
        Then("FabricationMode is set correctly",
            () => Verify.That(attribute.FabricationMode.IsEqualTo(FabricationMode.ContainerScoped)));
    }
    
    [Test]
    public void FabricationMode_CanBeChanged() {
        var attribute = Given("A FactoryReferenceAttribute",
            () => new FactoryReferenceAttribute());
        
        When("Setting FabricationMode",
            () => attribute.FabricationMode = FabricationMode.Container);
        
        Then("FabricationMode is updated",
            () => Verify.That(attribute.FabricationMode.IsEqualTo(FabricationMode.Container)));
    }
    
    [Test]
    public void AttributeUsage_AllowsFieldAndProperty() {
        var usage = When("Getting AttributeUsage",
            () => typeof(FactoryReferenceAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Field target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Field).IsTrue()));
        Then("Allows Property target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Property).IsTrue()));
    }
}
