// -----------------------------------------------------------------------------
//  <copyright file="NestedSpecificationTests.cs" company="Star Cruise Studios LLC">
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
    internal static class OuterNestedSpecification {
        [Factory]
        public static IntLeaf GetIntLeaf(int intValue) {
            return new IntLeaf(intValue);
        }

        [Specification]
        internal static class Inner {
            public const int IntValue = 101;

            [Factory]
            public static int GetIntValue() {
                return IntValue;
            }
        }
    }
    
    [Injector(
        typeof(OuterNestedSpecification),
        typeof(OuterNestedSpecification.Inner))]
    internal interface INestedSpecInjector {
        public IntLeaf GetIntLeaf();
    }
    
    #endregion injector
    
    public class NestedSpecificationTests : LoggingTestClass {
        [Test]
        public void NestedSpecificationsAreReferencedCorrectly() {
            INestedSpecInjector injector = Given(
                "A test injector that references a nested spec.",
                () => new GeneratedNestedSpecInjector());

            var leaf = When(
                "A factory method is invoked on the injector.",
                () => injector.GetIntLeaf());

            Then(
                "The value from the nested spec was returned",
                () => Verify.That(leaf.Value.IsEqualTo(OuterNestedSpecification.Inner.IntValue)));
        }
    }
}
