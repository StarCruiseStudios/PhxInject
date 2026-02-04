// -----------------------------------------------------------------------------
// <copyright file="LinkTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Tests.Data;
using Phx.Inject.Tests.Data.Model;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

using static CommonTestValueSpecification;

[Injector(
    "LinkTestInjector",
    typeof(CommonTestValueSpecification))]
public interface ILinkTestInjector {
    ILeaf GetLinkedType();
    
    [QualifierA]
    ILeaf GetQualifiedLinkedType();
    
    [Label(LabelA)]
    ILeaf GetDoubleLinkedType();
    
    [Label(LabelB)]
    ILeaf GetTripleLinkedType();
}

public class LinkTests : LoggingTestClass {
    [Test]
    public void LinksCanBeUsedToAccessInjectedValues() {
        ILinkTestInjector injector = Given("A test injector", () => new LinkTestInjector());

        var value = When("Getting a linked value", () => injector.GetLinkedType());

        Then("The expected type was injected", () => Verify.That(value.IsType<IntLeaf>()));
        Then("The expected value was injected",
            IntValue,
            expected => Verify.That((value as IntLeaf)!.Value.IsEqualTo(expected)));
    }
    [Test]
    public void LinksCanBeUsedToAccessQualifiedInjectedValues() {
        ILinkTestInjector injector = Given("A test injector", () => new LinkTestInjector());

        var qualifiedValue = When("Getting a qualified linked value", () => injector.GetQualifiedLinkedType());
        var doubleLinkedValue = When("Getting a double linked value", () => injector.GetDoubleLinkedType());
        var tripleLinkedValue = When("Getting a triple linked value", () => injector.GetTripleLinkedType());

        Then("The expected type was injected", () => Verify.That(qualifiedValue.IsType<IntLeaf>()));
        Then("The expected double linked type was injected", () => Verify.That(doubleLinkedValue.IsType<IntLeaf>()));
        Then("The expected triple linked type was injected", () => Verify.That(tripleLinkedValue.IsType<IntLeaf>()));
        Then("The expected value was injected",
            IntValue,
            expected => Verify.That((qualifiedValue as IntLeaf)!.Value.IsEqualTo(expected)));
        Then("The expected double linked value was injected",
            IntValue,
            expected => Verify.That((doubleLinkedValue as IntLeaf)!.Value.IsEqualTo(expected)));
        Then("The expected triple linked value was injected",
            IntValue,
            expected => Verify.That((tripleLinkedValue as IntLeaf)!.Value.IsEqualTo(expected)));
    }
}
