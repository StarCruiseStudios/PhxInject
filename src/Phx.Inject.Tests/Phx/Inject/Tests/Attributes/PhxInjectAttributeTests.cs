// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="PhxInjectAttribute"/>.
/// </summary>
public class PhxInjectAttributeTests : LoggingTestClass {
    
    [Test]
    public void DefaultConstructor_SetsDefaultValues() {
        var attribute = When("Creating PhxInjectAttribute with default constructor",
            () => new PhxInjectAttribute());
        
        Then("TabSize is default value",
            () => Verify.That(attribute.TabSize.IsEqualTo(PhxInjectAttribute.DefaultTabSize)));
        Then("GeneratedFileExtension is default value",
            () => Verify.That(attribute.GeneratedFileExtension.IsEqualTo(PhxInjectAttribute.DefaultGeneratedFileExtension)));
        Then("NullableEnabled is default value",
            () => Verify.That(attribute.NullableEnabled.IsEqualTo(PhxInjectAttribute.DefaultNullableEnabled)));
    }
    
    [Test]
    public void Properties_CanBeSet() {
        var attribute = Given("A PhxInjectAttribute",
            () => new PhxInjectAttribute());
        
        When("Setting properties", () => {
            attribute.TabSize = 2;
            attribute.GeneratedFileExtension = "custom.cs";
            attribute.NullableEnabled = false;
        });
        
        Then("Properties are updated", () => {
            Verify.That(attribute.TabSize.IsEqualTo(2));
            Verify.That(attribute.GeneratedFileExtension.IsEqualTo("custom.cs"));
            Verify.That(attribute.NullableEnabled.IsFalse());
        });
    }
    
    [Test]
    public void Constants_HaveExpectedValues() {
        Then("DefaultTabSize is 4",
            () => Verify.That(PhxInjectAttribute.DefaultTabSize.IsEqualTo(4)));
        Then("DefaultGeneratedFileExtension is correct",
            () => Verify.That(PhxInjectAttribute.DefaultGeneratedFileExtension.IsEqualTo("generated.cs")));
        Then("DefaultNullableEnabled is true",
            () => Verify.That(PhxInjectAttribute.DefaultNullableEnabled.IsTrue()));
    }
    
    [Test]
    public void AttributeUsage_AllowsClass() {
        var usage = When("Getting AttributeUsage",
            () => typeof(PhxInjectAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Class target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Class).IsTrue()));
    }
    
    [Test]
    public void AttributeUsage_NotInherited() {
        var usage = When("Getting AttributeUsage",
            () => typeof(PhxInjectAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Inherited is false",
            () => Verify.That(usage!.Inherited.IsFalse()));
    }
}
