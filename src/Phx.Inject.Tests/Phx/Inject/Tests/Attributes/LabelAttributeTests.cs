// -----------------------------------------------------------------------------
// <copyright file="LabelAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="LabelAttribute"/>.
/// </summary>
public class LabelAttributeTests : LoggingTestClass {
    
    [Test]
    public void Constructor_WithLabel_SetsLabel() {
        var expectedLabel = Given("A label string", () => "TestLabel");
        
        var attribute = When("Creating LabelAttribute",
            () => new LabelAttribute(expectedLabel));
        
        Then("Label is set correctly",
            () => Verify.That(attribute.Label.IsEqualTo(expectedLabel)));
    }
    
    [Test]
    public void Label_IsReadOnly() {
        var attribute = Given("A LabelAttribute",
            () => new LabelAttribute("TestLabel"));
        
        var property = When("Getting Label property info",
            () => typeof(LabelAttribute).GetProperty(nameof(LabelAttribute.Label)));
        
        Then("Property has no setter",
            () => Verify.That(property!.CanWrite.IsFalse()));
    }
    
    [Test]
    public void AttributeUsage_MatchesQualifierUsage() {
        var usage = When("Getting AttributeUsage",
            () => typeof(LabelAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Valid targets match QualifierAttribute.Usage",
            () => Verify.That(usage!.ValidOn.IsEqualTo(QualifierAttribute.Usage)));
    }
    
    [Test]
    public void AttributeUsage_NotInherited() {
        var usage = When("Getting AttributeUsage",
            () => typeof(LabelAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Inherited is false",
            () => Verify.That(usage!.Inherited.IsFalse()));
    }
}
