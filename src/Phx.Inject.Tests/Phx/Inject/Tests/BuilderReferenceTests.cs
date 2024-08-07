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
    internal static class BuilderReferenceSpec {
        [Factory]
        internal static int IntValue => 10;

        [BuilderReference]
        internal static Action<LazyType, ILeaf> BuildLazyType => LazyType.Inject;

        [Label("Field")]
        [BuilderReference]
        internal static readonly Action<LazyType, ILeaf> BuildLazyTypeField = LazyType.Inject;

        [FactoryReference(FabricationMode.Scoped)]
        internal static Func<int, IntLeaf> GetIntLeaf => IntLeaf.Construct;
    }
    
    [Injector(
        typeof(BuilderReferenceSpec)
    )]
    internal interface IBuilderReferenceInjector {
        public void Build(LazyType lazyType);

        [Label("Field")]
        public void BuildField(LazyType lazyType);
    }
    
    #endregion injector
    
    public class BuilderReferenceTests : LoggingTestClass {
        [Test]
        public void AnInjectorBuilderPropertyIsGenerated() {
            IBuilderReferenceInjector injector = Given("A test injector.",
                () => new GeneratedBuilderReferenceInjector());
            var lazyType = Given("An uninitialized lazy type.", () => new LazyType());

            When("An injector builder method using a builder reference property is invoked.",
                () => injector.Build(lazyType));

            Then("The lazy type is initialized.", () => Verify.That(lazyType.Value.IsNotNull()));
        }

        [Test]
        public void AnInjectorBuilderFieldIsGenerated() {
            IBuilderReferenceInjector injector = Given("A test injector.",
                () => new GeneratedBuilderReferenceInjector());
            var lazyType = Given("An uninitialized lazy type.", () => new LazyType());

            When("An injector builder method using a builder reference field is invoked.",
                () => injector.BuildField(lazyType));

            Then("The lazy type is initialized.", () => Verify.That(lazyType.Value.IsNotNull()));
        }
    }
}
