// -----------------------------------------------------------------------------
//  <copyright file="UnitTest1.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using NUnit.Framework;
    using Phx.Inject.Tests.Data;

    public class Tests {
        [Test]
        public void Test1() {
            ITestInjector injector = new CustomInjector();

            ILeaf leftLeaf = injector.GetRoot().Node.Left;
            Assert.That((leftLeaf as IntLeaf)!.Value, Is.EqualTo(10));
        }
    }
}