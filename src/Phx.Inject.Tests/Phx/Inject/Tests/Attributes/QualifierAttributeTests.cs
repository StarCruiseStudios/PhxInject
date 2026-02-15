// -----------------------------------------------------------------------------
// <copyright file="QualifierAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="QualifierAttribute"/>.
/// </summary>
public class QualifierAttributeTests : LoggingTestClass {
    
    [Test]
    public void Constructor_CreatesInstance() {
        var attribute = When("Creating QualifierAttribute",
            () => new QualifierAttribute());
        
        Then("Attribute is not null",
            () => Verify.That(attribute.IsNotNull()));
    }
    
    [Test]
    public void Usage_IncludesExpectedTargets() {
        var usage = Given("QualifierAttribute.Usage constant",
            () => QualifierAttribute.Usage);
        
        Then("Includes Class", 
            () => Verify.That(usage.HasFlag(AttributeTargets.Class).IsTrue()));
        Then("Includes Struct", 
            () => Verify.That(usage.HasFlag(AttributeTargets.Struct).IsTrue()));
        Then("Includes Interface", 
            () => Verify.That(usage.HasFlag(AttributeTargets.Interface).IsTrue()));
        Then("Includes Constructor", 
            () => Verify.That(usage.HasFlag(AttributeTargets.Constructor).IsTrue()));
        Then("Includes Method", 
            () => Verify.That(usage.HasFlag(AttributeTargets.Method).IsTrue()));
        Then("Includes Property", 
            () => Verify.That(usage.HasFlag(AttributeTargets.Property).IsTrue()));
        Then("Includes Parameter", 
            () => Verify.That(usage.HasFlag(AttributeTargets.Parameter).IsTrue()));
        Then("Includes Field", 
            () => Verify.That(usage.HasFlag(AttributeTargets.Field).IsTrue()));
    }
    
    [Test]
    public void AttributeUsage_AllowsClass() {
        var usage = When("Getting AttributeUsage",
            () => typeof(QualifierAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Class target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Class).IsTrue()));
    }
    
    [Test]
    public void AttributeUsage_NotInherited() {
        var usage = When("Getting AttributeUsage",
            () => typeof(QualifierAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Inherited is false",
            () => Verify.That(usage!.Inherited.IsFalse()));
    }
}
