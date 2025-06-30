// -----------------------------------------------------------------------------
//  <copyright file="FactoryReferenceTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using NUnit.Framework;
using Phx.Inject.Tests.Data.Model;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

#region injector

[Specification]
internal static class BuilderReferenceSpec {
    [Label("Field")]
    [BuilderReference]
    internal static readonly Action<TestBuilderReferenceObject, int> BuildBuilderReferenceTypeField =
        TestBuilderReferenceObject.Inject;

    [Factory]
    internal static int IntValue {
        get => 10;
    }

    [BuilderReference]
    internal static Action<TestBuilderReferenceObject, int> BuildBuilderReferenceType {
        get => TestBuilderReferenceObject.Inject;
    }

    [FactoryReference(FabricationMode.Scoped)]
    internal static Func<int, IntLeaf> GetIntLeaf {
        get => IntLeaf.Construct;
    }
}

[Injector(
    typeof(BuilderReferenceSpec)
)]
internal interface IBuilderReferenceInjector {
    public void Build(TestBuilderReferenceObject testBuilderReferenceObject);

    [Label("Field")]
    public void BuildField(TestBuilderReferenceObject testBuilderReferenceObject);
}

#endregion injector

public class BuilderReferenceTests : LoggingTestClass {
    [Test]
    public void ABuilderReferencePropertyIsGenerated() {
        IBuilderReferenceInjector injector = Given("A test injector.",
            () => new GeneratedBuilderReferenceInjector());
        var buidlerReferenceType =
            Given("An uninitialized builder reference type.", () => new TestBuilderReferenceObject());

        When("An injector builder method using a builder reference property is invoked.",
            () => injector.Build(buidlerReferenceType));

        Then("The builder reference type is initialized.", () => Verify.That(buidlerReferenceType.Value.IsNotNull()));
    }

    [Test]
    public void ABuilderReferenceFieldIsGenerated() {
        IBuilderReferenceInjector injector = Given("A test injector.",
            () => new GeneratedBuilderReferenceInjector());
        var builderReferenceType =
            Given("An uninitialized builder reference type.", () => new TestBuilderReferenceObject());

        When("An injector builder method using a builder reference field is invoked.",
            () => injector.BuildField(builderReferenceType));

        Then("The builder reference type is initialized.", () => Verify.That(builderReferenceType.Value.IsNotNull()));
    }
}
