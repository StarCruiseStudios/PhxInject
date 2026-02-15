// -----------------------------------------------------------------------------
// <copyright file="DependencyAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="DependencyAttribute"/>.
/// </summary>
public class DependencyAttributeTests : LoggingTestClass {
    
    [Test]
    public void Constructor_WithType_SetsDependencyType() {
        var expectedType = Given("A type",
            () => typeof(string));
        
        var attribute = When("Creating DependencyAttribute",
            () => new DependencyAttribute(expectedType));
        
        Then("DependencyType is set correctly",
            () => Verify.That(attribute.DependencyType.IsEqualTo(expectedType)));
    }
    
    [Test]
    public void AttributeUsage_AllowsInterface() {
        var usage = When("Getting AttributeUsage",
            () => typeof(DependencyAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Interface target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Interface).IsTrue()));
    }
    
    [Test]
    public void DependencyType_IsReadOnly() {
        var attribute = Given("A DependencyAttribute",
            () => new DependencyAttribute(typeof(int)));
        
        var property = When("Getting DependencyType property info",
            () => typeof(DependencyAttribute).GetProperty(nameof(DependencyAttribute.DependencyType)));
        
        Then("Property has no setter",
            () => Verify.That(property!.CanWrite.IsFalse()));
    }
}
