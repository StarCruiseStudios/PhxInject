// -----------------------------------------------------------------------------
// <copyright file="LinkAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="LinkAttribute"/>.
/// </summary>
public class LinkAttributeTests : LoggingTestClass {
    
    [Test]
    public void Constructor_WithInputAndOutput_SetsProperties() {
        var inputType = Given("Input type", () => typeof(string));
        var outputType = Given("Output type", () => typeof(int));
        
        var attribute = When("Creating LinkAttribute",
            () => new LinkAttribute(inputType, outputType));
        
        Then("Input is set correctly",
            () => Verify.That(attribute.Input.IsEqualTo(inputType)));
        Then("Output is set correctly",
            () => Verify.That(attribute.Output.IsEqualTo(outputType)));
    }
    
    [Test]
    public void QualifierProperties_DefaultToNull() {
        var attribute = When("Creating LinkAttribute",
            () => new LinkAttribute(typeof(string), typeof(int)));
        
        Then("InputLabel is null",
            () => Verify.That(attribute.InputLabel.IsNull()));
        Then("InputQualifier is null",
            () => Verify.That(attribute.InputQualifier.IsNull()));
        Then("OutputLabel is null",
            () => Verify.That(attribute.OutputLabel.IsNull()));
        Then("OutputQualifier is null",
            () => Verify.That(attribute.OutputQualifier.IsNull()));
    }
    
    [Test]
    public void QualifierProperties_CanBeSet() {
        var attribute = Given("A LinkAttribute",
            () => new LinkAttribute(typeof(string), typeof(int)));
        
        When("Setting qualifier properties", () => {
            attribute.InputLabel = "InputLabelValue";
            attribute.OutputLabel = "OutputLabelValue";
            attribute.InputQualifier = typeof(object);
            attribute.OutputQualifier = typeof(bool);
        });
        
        Then("Properties are updated", () => {
            Verify.That(attribute.InputLabel.IsEqualTo("InputLabelValue"));
            Verify.That(attribute.OutputLabel.IsEqualTo("OutputLabelValue"));
            Verify.That(attribute.InputQualifier.IsEqualTo(typeof(object)));
            Verify.That(attribute.OutputQualifier.IsEqualTo(typeof(bool)));
        });
    }
    
    [Test]
    public void AttributeUsage_AllowsClassAndInterface() {
        var usage = When("Getting AttributeUsage",
            () => typeof(LinkAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Class target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Class).IsTrue()));
        Then("Allows Interface target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Interface).IsTrue()));
    }
    
    [Test]
    public void AttributeUsage_AllowsMultiple() {
        var usage = When("Getting AttributeUsage",
            () => typeof(LinkAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("AllowMultiple is true",
            () => Verify.That(usage!.AllowMultiple.IsTrue()));
    }
}
