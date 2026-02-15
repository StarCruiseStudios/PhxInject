// -----------------------------------------------------------------------------
// <copyright file="InjectorAttributeTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for <see cref="InjectorAttribute"/>.
/// </summary>
public class InjectorAttributeTests : LoggingTestClass {
    
    [Test]
    public void Constructor_WithSpecifications_SetsSpecifications() {
        var specs = Given("Specification types",
            () => new[] { typeof(string), typeof(int) });
        
        var attribute = When("Creating InjectorAttribute",
            () => new InjectorAttribute(specs));
        
        Then("Specifications are set correctly",
            () => Verify.That(attribute.Specifications.Count().IsEqualTo(2)));
    }
    
    [Test]
    public void Constructor_WithGeneratedClassName_SetsProperty() {
        var className = Given("A class name", () => "MyCustomInjector");
        var specs = Given("Specification types", () => new[] { typeof(string) });
        
        var attribute = When("Creating InjectorAttribute with class name",
            () => new InjectorAttribute(className, specs));
        
        Then("GeneratedClassName is set",
            () => Verify.That(attribute.GeneratedClassName.IsEqualTo(className)));
        Then("Specifications are set",
            () => Verify.That(attribute.Specifications.Count().IsEqualTo(1)));
    }
    
    [Test]
    public void GeneratedClassName_DefaultsToNull() {
        var attribute = When("Creating InjectorAttribute without class name",
            () => new InjectorAttribute(typeof(string)));
        
        Then("GeneratedClassName is null",
            () => Verify.That(attribute.GeneratedClassName.IsNull()));
    }
    
    [Test]
    public void GeneratedClassName_CanBeSet() {
        var attribute = Given("An InjectorAttribute",
            () => new InjectorAttribute(typeof(string)));
        
        When("Setting GeneratedClassName",
            () => attribute.GeneratedClassName = "CustomName");
        
        Then("GeneratedClassName is updated",
            () => Verify.That(attribute.GeneratedClassName.IsEqualTo("CustomName")));
    }
    
    [Test]
    public void AttributeUsage_AllowsInterface() {
        var usage = When("Getting AttributeUsage",
            () => typeof(InjectorAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
                .FirstOrDefault() as AttributeUsageAttribute);
        
        Then("Allows Interface target",
            () => Verify.That(usage!.ValidOn.HasFlag(AttributeTargets.Interface).IsTrue()));
    }
}
