// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="AutoBuilderAttribute"/>.
/// </summary>
public class AutoBuilderAttributeTests : LoggingTestClass {
    
    [Test]
    public void Constructor_CreatesInstance() {
        var attribute = When("Creating AutoBuilderAttribute", 
            () => new AutoBuilderAttribute());
        
        Then("Attribute is not null", 
            () => Verify.That(attribute.IsNotNull()));
    }
    
    [Test]
    public void AttributeUsage_AllowsMethod() {
        var usage = When("Getting AttributeUsage",
            () => typeof(AutoBuilderAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Usage is not null", 
            () => Verify.That(usage.IsNotNull()));
        Then("Allows Method target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Method).IsTrue()));
    }
    
    [Test]
    public void AttributeUsage_NotInherited() {
        var usage = When("Getting AttributeUsage",
            () => typeof(AutoBuilderAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Inherited is false",
            () => Verify.That(usage!.Inherited.IsFalse()));
    }
}
