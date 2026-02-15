// -----------------------------------------------------------------------------
// <copyright file="BuilderReferenceAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="BuilderReferenceAttribute"/>.
/// </summary>
public class BuilderReferenceAttributeTests : LoggingTestClass {
    
    [Test]
    public void Constructor_CreatesInstance() {
        var attribute = When("Creating BuilderReferenceAttribute",
            () => new BuilderReferenceAttribute());
        
        Then("Attribute is not null",
            () => Verify.That(attribute.IsNotNull()));
    }
    
    [Test]
    public void AttributeUsage_AllowsFieldAndProperty() {
        var usage = When("Getting AttributeUsage",
            () => typeof(BuilderReferenceAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Field target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Field).IsTrue()));
        Then("Allows Property target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Property).IsTrue()));
    }
}
