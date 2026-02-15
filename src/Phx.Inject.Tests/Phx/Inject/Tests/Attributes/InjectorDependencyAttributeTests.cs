// -----------------------------------------------------------------------------
// <copyright file="InjectorDependencyAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="InjectorDependencyAttribute"/>.
/// </summary>
public class InjectorDependencyAttributeTests : LoggingTestClass {
    
    [Test]
    public void Constructor_CreatesInstance() {
        var attribute = When("Creating InjectorDependencyAttribute",
            () => new InjectorDependencyAttribute());
        
        Then("Attribute is not null",
            () => Verify.That(attribute.IsNotNull()));
    }
    
    [Test]
    public void AttributeUsage_AllowsInterface() {
        var usage = When("Getting AttributeUsage",
            () => typeof(InjectorDependencyAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Interface target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Interface).IsTrue()));
    }
}
