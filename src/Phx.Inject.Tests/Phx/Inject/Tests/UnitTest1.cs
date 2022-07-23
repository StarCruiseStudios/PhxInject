// -----------------------------------------------------------------------------
//  <copyright file="UnitTest1.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using Phx.Inject.Generator;

    public class Tests {
        [Test]
        public void Test1() {
            var compilation = TestCompiler.CompileDirectory(TestFiles.RootDirectory, new SourceGenerator());

            var phxNamespace = compilation.GlobalNamespace.GetMembers("Phx").Single() as INamespaceSymbol;
            Assert.That(phxNamespace, Is.Not.Null);
            var injectNamespace = phxNamespace!.GetMembers("Inject").Single() as INamespaceSymbol;
            Assert.That(injectNamespace, Is.Not.Null);
            var testsNamespace = injectNamespace!.GetMembers("Tests").Single() as INamespaceSymbol;
            Assert.That(testsNamespace, Is.Not.Null);
            var dataNamespace = testsNamespace!.GetMembers("Data").Single() as INamespaceSymbol;
            Assert.That(dataNamespace, Is.Not.Null);

            var customInjector = dataNamespace!.GetTypeMembers("CustomInjector").Single();
            Assert.That(customInjector.Locations.Single().IsInSource, Is.True);
        }
    }
}