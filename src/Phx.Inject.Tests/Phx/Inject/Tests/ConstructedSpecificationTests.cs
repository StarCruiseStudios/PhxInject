// -----------------------------------------------------------------------------
//  <copyright file="ConstructedSpecificationTests.cs" company="Star Cruise Studios LLC">
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

    #region Specifications
    
    [Specification]
    internal interface IConstructedSpecification {
        [Factory]
        public int GetIntValue();
    }
    
    internal class ConstructedSpecificationImplementation : IConstructedSpecification {
        private readonly int intValue;
        
        public int GetIntValue() {
            return intValue;
        }

        public ConstructedSpecificationImplementation(int intValue) {
            this.intValue = intValue;
        }
    }
    
    [Specification]
    [Link(typeof(IntLeaf), typeof(ILeaf))]
    internal static class NonConstructedSpecification {
        [Factory]
        public static IntLeaf GetIntLeaf(int intValue) {
            return new IntLeaf(intValue);
        }
    
        [Factory]
        public static OuterType GetOuterType(AutoType auto, AutoTypeWithRequiredProperties autoTypeWithRequiredProperties) {
            return new OuterType(auto, autoTypeWithRequiredProperties);
        }
    }
    
    #endregion Specifications
    
    #region Basic Constructed Injector

    [Injector(
        generatedClassName: "BasicConstructedInjector",
        typeof(IConstructedSpecification)
    )]
    internal interface IBasicConstructedInjector {
        public int GetIntValue();
    }
    
    #endregion Basic Constructed Injector
    
    #region Parent Constructed Injector
    
    [Injector(generatedClassName: "ConstructedParentInjector")]
    internal interface IConstructedParentInjector {
        [ChildInjector]
        public IConstructedChildInjector GetChildInjector(IConstructedSpecification iConstructedSpecification);
    }
    
    #endregion Parent Constructed Injector
    
    #region Child Constructed Injector

    [Injector(
        generatedClassName: "ConstructedChildInjector",
        typeof(IConstructedSpecification),
        typeof(NonConstructedSpecification))]
    internal interface IConstructedChildInjector {
        public IntLeaf GetIntLeaf();
        public ILeaf GetILeaf();
        public OuterType GetOuterType();
    }
    
    #endregion Child Constructed Injector
    
    public class ConstructedSpecificationTests : LoggingTestClass {
        [Test]
        public void ABasicConstructedSpecificationInjectorIsConstructed() {
            var intValue = Given("An int value", () => 42);
            IConstructedSpecification spec = Given("A constructed specification.", () => new ConstructedSpecificationImplementation(intValue));
            IBasicConstructedInjector injector = Given("An injector built with the spec.",() => new BasicConstructedInjector(spec));

            var actual = When("A factory method is invoked on the injector.", () => injector.GetIntValue());

            Then(
                "The value from the constructed spec was returned.", intValue,
                (expected) => Verify.That(actual.IsEqualTo(expected)));
        }
        
        [Test]
        public void AConstructedSpecificationInjectorCanBeAChild() {
            var intValue = Given("An int value", () => 42);
            IConstructedSpecification spec = Given("A constructed specification.", () => new ConstructedSpecificationImplementation(intValue));
            IConstructedParentInjector injector = Given("A test parent injector.", () => new ConstructedParentInjector());
        
            var childInjector = When("A child injector is retrieved that uses the constructed specification.",
                () => injector.GetChildInjector(spec));
        
            Then(
                "The value from the constructed spec was returned.", intValue,
                (expected) => Verify.That(childInjector.GetIntLeaf().Value.IsEqualTo(expected)));
        }
    }
}
