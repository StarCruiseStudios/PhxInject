// -----------------------------------------------------------------------------
//  <copyright file="QualifierTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data;
    using Phx.Inject.Tests.Data.Model;
    using Phx.Test;
    using Phx.Validation;

    #region
    
    [Specification]
    internal static class QualifierSpecification {
        public const string AttributeNamedLeafAData = "AttributeNamedLeafA";
        public const string AttributeNamedLeafBData = "AttributeNamedLeafB";
        
        [Factory]
        [QualifierA]
        internal static ILeaf GetAttributeNamedLeafA() {
            return new StringLeaf(AttributeNamedLeafAData);
        }

        [Factory]
        [QualifierB]
        internal static ILeaf GetAttributeNamedLeafB() {
            return new StringLeaf(AttributeNamedLeafBData);
        }
    }
    
    [Injector(typeof(QualifierSpecification))]
    internal interface IQualifierTestInjector {
        [QualifierA]
        public ILeaf GetAttributeNamedLeafA();

        [QualifierB]
        public ILeaf GetAttributeNamedLeafB();
    }
    
    #endregion injector
    
    public class QualifierTests : LoggingTestClass {

        [Test]
        public void AnAttributeQualifiedFactoryIsDifferentThanAnotherAttributeQualifiedFactory() {
            IQualifierTestInjector injector = Given("A test injector.", () => new GeneratedQualifierTestInjector());

            var (leafA, leafB) = When(
                "Two different attribute qualified dependencies are retrieved.",
                () => (injector.GetAttributeNamedLeafA(), injector.GetAttributeNamedLeafB()));

            Then(
                "The correct first dependency was returned.",
                QualifierSpecification.AttributeNamedLeafAData,
                expected => Verify.That((leafA as StringLeaf)!.Value.IsEqualTo(expected)));
            Then(
                "The correct second dependency was returned.",
                QualifierSpecification.AttributeNamedLeafBData,
                expected => Verify.That((leafB as StringLeaf)!.Value.IsEqualTo(expected)));
        }
    }
}
