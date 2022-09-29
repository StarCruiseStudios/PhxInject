// -----------------------------------------------------------------------------
//  <copyright file="NestedSpecificationTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data.Inject;
    using Phx.Inject.Tests.Data.Model;
    using Phx.Inject.Tests.Data.Specification;
    using Phx.Test;
    using Phx.Validation;

    public class ChildInjectorTests : LoggingTestClass {
        [Test] public void ChildInjectorsAreCreatedCorrectly() {
            IParentInjector parentInjector = Given(
                    "A parent injector.",
                    () => new GeneratedParentInjector());
            IChildInjector childInjector = Given(
                    "A child injector retrieved from the parent.",
                    () => parentInjector.GetChildInjector());
            IGrandchildInjector grandchildInjector = Given(
                    "A grandchild injector retrieved from the child.",
                    () => childInjector.GetGrandchildInjector());

            var root = When(
                    "A factory method is invoked on the grandchild injector.",
                    () => grandchildInjector.GetRoot());

            Then(
                    "The values from the parent are returned",
                    ParentSpecification.LeftLeaf,
                    (expected) => Verify.That((root.Node.Left as StringLeaf)!.Value.IsEqualTo(expected)));

            Then(
                    "The values from the parent are returned",
                    ParentSpecification.RightLeaf,
                    (expected) => Verify.That((root.Node.Right as StringLeaf)!.Value.IsEqualTo(expected)));
        }
    }
}
