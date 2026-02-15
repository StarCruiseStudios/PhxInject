// -----------------------------------------------------------------------------
// <copyright file="FactoryTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

/// <summary>
/// Tests for <see cref="Factory{T}"/> wrapper class.
/// </summary>
public class FactoryTests : LoggingTestClass {
    
    [Test]
    public void Constructor_WithFunc_CreatesFactory() {
        var func = Given("A factory function", () => new Func<int>(() => 42));
        
        var factory = When("Creating Factory", () => new Factory<int>(func));
        
        Then("Factory is created", () => Verify.That(factory.IsNotNull()));
    }
    
    [Test]
    public void Create_ReturnsResultFromFunc() {
        var factory = Given("A factory for strings", 
            () => new Factory<string>(() => "test value"));
        
        var result = When("Calling Create", () => factory.Create());
        
        Then("Result is correct", () => Verify.That(result.IsEqualTo("test value")));
    }
    
    [Test]
    public void Create_CalledMultipleTimes_InvokesFuncEachTime() {
        var counter = Given("A counter", () => 0);
        var factory = Given("A factory that increments counter", 
            () => new Factory<int>(() => ++counter));
        
        var result1 = When("First Create call", () => factory.Create());
        var result2 = When("Second Create call", () => factory.Create());
        var result3 = When("Third Create call", () => factory.Create());
        
        Then("First result is 1", () => Verify.That(result1.IsEqualTo(1)));
        Then("Second result is 2", () => Verify.That(result2.IsEqualTo(2)));
        Then("Third result is 3", () => Verify.That(result3.IsEqualTo(3)));
    }
    
    [Test]
    public void Create_WithReferenceType_ReturnsNewInstanceEachTime() {
        var factory = Given("A factory for objects", 
            () => new Factory<object>(() => new object()));
        
        var result1 = When("First Create", () => factory.Create());
        var result2 = When("Second Create", () => factory.Create());
        
        Then("Results are different instances", 
            () => Verify.That(ReferenceEquals(result1, result2).IsFalse()));
    }
    
    [Test]
    public void Create_WithComplexType_WorksCorrectly() {
        var factory = Given("A factory for lists", 
            () => new Factory<List<int>>(() => new List<int> { 1, 2, 3 }));
        
        var result = When("Calling Create", () => factory.Create());
        
        Then("Result contains expected items", () => {
            Verify.That(result.Count.IsEqualTo(3));
            Verify.That(result[0].IsEqualTo(1));
            Verify.That(result[1].IsEqualTo(2));
            Verify.That(result[2].IsEqualTo(3));
        });
    }
    
    [Test]
    public void Create_WithNullableType_CanReturnNull() {
        var factory = Given("A factory that returns null", 
            () => new Factory<string?>(() => null));
        
        var result = When("Calling Create", () => factory.Create());
        
        Then("Result is null", () => Verify.That(result.IsNull()));
    }
}
