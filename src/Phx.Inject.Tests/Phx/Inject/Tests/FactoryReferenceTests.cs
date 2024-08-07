// -----------------------------------------------------------------------------
//  <copyright file="FactoryReferenceTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data.Model;
    using Phx.Test;
    using Phx.Validation;

    #region injector
    
    [Specification]
    [Link(typeof(IntLeaf), typeof(ILeaf))]
    internal static class FactoryReferenceSpec {
        [Factory]
        internal static int IntValue => 10;

        [Factory]
        internal static string StringValue => "stringvalue";

        [FactoryReference(FabricationMode.Scoped)]
        internal static Func<int, IntLeaf> GetIntLeaf => IntLeaf.Construct;

        [FactoryReference(FabricationMode.Scoped)]
        internal static readonly Func<string, StringLeaf> GetStringLeaf = StringLeaf.Construct;
    }
    
    [Injector(
        typeof(FactoryReferenceSpec)
    )]
    internal interface IFactoryReferenceInjector {
        public IntLeaf GetIntLeaf();
        public StringLeaf GetStringLeaf();
    }
    
    #endregion injector
    
    public class FactoryReferenceTests : LoggingTestClass {
        [Test]
        public void AnInjectorFactoryReferencePropertyIsGenerated() {
            IFactoryReferenceInjector injector = Given("A test injector.",
                () => new GeneratedFactoryReferenceInjector());

            var result = When("An injector method using a factory reference property is invoked.",
                () => injector.GetIntLeaf());

            Then("A valid value is constructed.", () => Verify.That(result.IsNotNull()));
        }

        [Test]
        public void AnInjectorFactoryReferenceFieldIsGenerated() {
            IFactoryReferenceInjector injector = Given("A test injector.",
                () => new GeneratedFactoryReferenceInjector());

            var result = When("An injector method using a factory reference field is invoked.",
                () => injector.GetStringLeaf());

            Then("A valid value is constructed.", () => Verify.That(result.IsNotNull()));
        }
    }
}
