// -----------------------------------------------------------------------------
//  <copyright file="GeneratorTests.cs" company="Star Cruise Studios LLC">
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
    using Phx.Inject.Tests.Helpers;
    using Phx.Test;
    using Phx.Validation;

    public class GeneratorTests : LoggingTestClass {
        [Test]
        public void InjectorTypesAreGenerated() {
            var generator = Given("A source generator.", () => new SourceGenerator());
            var rootDirectory = Given("A directory with source files.", () => TestFiles.RootDirectory);

            var compilation = When(
                    "The source is compiled with the generator.",
                    () => TestCompiler.CompileDirectory(rootDirectory, generator));

            var injectorNamespace = Then(
                    "The test data's namespace exists.",
                    "Phx.Inject.Tests.Data.Inject",
                    _ => {
                        var phxNamespace = compilation.GlobalNamespace.GetMembers("Phx")
                                .Single() as INamespaceSymbol;
                        Verify.That(phxNamespace.IsNotNull());
                        var injectNamespace = phxNamespace!.GetMembers("Inject")
                                .Single() as INamespaceSymbol;
                        Verify.That(injectNamespace.IsNotNull());
                        var testsNamespace = injectNamespace!.GetMembers("Tests")
                                .Single() as INamespaceSymbol;
                        Verify.That(testsNamespace.IsNotNull());
                        var dataNamespace = testsNamespace!.GetMembers("Data")
                                .Single() as INamespaceSymbol;
                        Verify.That(dataNamespace.IsNotNull());
                        var innerInjectNamespace = dataNamespace!.GetMembers("Inject")
                                .Single() as INamespaceSymbol;
                        Verify.That(innerInjectNamespace.IsNotNull());
                        return innerInjectNamespace;
                    });

            Then(
                    "The namespace contains the expected generated injector.",
                    "CustomInjector",
                    expected => {
                        var customInjector = injectorNamespace!.GetTypeMembers(expected)
                                .Single();
                        Verify.That(
                                customInjector.Locations.Single()
                                        .IsInSource.IsTrue());
                    });

            Then(
                    "The namespace contains the expected generated injector.",
                    "GeneratedLabelInjector",
                    expected => {
                        var customInjector = injectorNamespace!.GetTypeMembers(expected)
                                .Single();
                        Verify.That(
                                customInjector.Locations.Single()
                                        .IsInSource.IsTrue());
                    });

            Then(
                    "The namespace contains the expected generated injector.",
                    "GeneratedRawInjector",
                    expected => {
                        var customInjector = injectorNamespace!.GetTypeMembers(expected)
                                .Single();
                        Verify.That(
                                customInjector.Locations.Single()
                                        .IsInSource.IsTrue());
                    });
        }
    }
}
