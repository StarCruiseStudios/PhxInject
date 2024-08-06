// -----------------------------------------------------------------------------
//  <copyright file="GeneratorTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests {
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using Phx.Inject.Generator;
    using Phx.Inject.Generator.Common.Render;
    using Phx.Inject.Tests.Helpers;
    using Phx.Test;
    using Phx.Validation;

    public class GeneratorTests : LoggingTestClass {
        [Test]
        public void InjectorTypesAreGenerated() {
            var compilation = CompileCode();
            var injectorNamespace = ThenTheExpectedNamespaceExists(compilation.GlobalNamespace,
                    "Phx",
                    "Inject",
                    "Tests",
                    "Data",
                    "Inject");

            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "CustomInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedParentInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedChildInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedGrandchildInjector");
        }

        [Test]
        public void SpecTypesAreGenerated() {
            var compilation = CompileCode();
            var specNamespace = ThenTheExpectedNamespaceExists(compilation.GlobalNamespace,
                    "Phx",
                    "Inject",
                    "Tests",
                    "Data",
                    "Inject");

            ThenTheNamespaceContainsTheExpectedType(specNamespace, "CustomInjector_LazySpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "CustomInjector_LeafLinks");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "CustomInjector_LeafSpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "CustomInjector_RootSpecification");

            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedParentInjector_ParentSpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedChildInjector_ChildSpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace,
                    "GeneratedGrandchildInjector_GrandchildSpecification");
        }

        [Test]
        public void ExternalDependencyTypesAreGenerated() {
            var compilation = CompileCode();
            var dataNamespace =
                    ThenTheExpectedNamespaceExists(compilation.GlobalNamespace, "Phx", "Inject", "Tests", "Data");

            var externalNamespace = ThenTheExpectedNamespaceExists(dataNamespace, "Inject");

            ThenTheNamespaceContainsTheExpectedType(externalNamespace,
                    "GeneratedParentInjector_IChildExternalDependencies");
            ThenTheNamespaceContainsTheExpectedType(externalNamespace,
                    "GeneratedChildInjector_IGrandchildExternalDependencies");

            ThenTheNamespaceContainsTheExpectedType(externalNamespace,
                    "GeneratedChildInjector_IChildExternalDependencies");
            ThenTheNamespaceContainsTheExpectedType(externalNamespace,
                    "GeneratedGrandchildInjector_IGrandchildExternalDependencies");
        }

        private Compilation CompileCode() {
            var renderSettings = new GeneratorSettings(
                    ShouldWriteFiles: true,  // SHOULD_WRITE_FILES
                    OutputPath: Path.Join(TestContext.CurrentContext.TestDirectory, "Generated"));
            var generator = Given("A source generator.", () => new SourceGenerator(renderSettings));
            var rootDirectory = Given("A directory with source files.", () => TestFiles.RootDirectory);

            var compilation = When(
                    "The source is compiled with the generator.",
                    () => TestCompiler.CompileDirectory(rootDirectory, generator));
            
            var diagnostics = compilation.GetDiagnostics();
            foreach (Diagnostic diagnostic in diagnostics) {
                if (diagnostic.Severity >= DiagnosticSeverity.Warning) {
                    Log(diagnostic.ToString());                    
                }
            }

            // Then("No errors were found during compilation.",
            //     () =>
            //         Verify.That(diagnostics.Where(it => it.Severity == DiagnosticSeverity.Error).Count().IsEqualTo(0)));
            
            return compilation;
        }

        private INamespaceSymbol ThenTheExpectedNamespaceExists(INamespaceSymbol root, params string[] ns) {
            return Then(
                    "The expected namespace exists",
                    string.Join(".", ns),
                    (_) => {
                        var currentNamespace = root;
                        foreach (var namespaceName in ns) {
                            currentNamespace = currentNamespace!.GetMembers(namespaceName)
                                    .First() as INamespaceSymbol;
                            Verify.That(currentNamespace.IsNotNull(),
                                    $"Could not find namespace member '{namespaceName}'.");
                        }

                        return currentNamespace!;
                    });
        }

        private void ThenTheNamespaceContainsTheExpectedType(
                INamespaceSymbol namespaceSymbol,
                string expectedTypeName) {
            Then(
                    "The namespace contains the expected type.",
                    expectedTypeName,
                    expected => {
                        var typeSymbol = namespaceSymbol.GetTypeMembers(expected)
                                .FirstOrDefault();
                        Verify.That(typeSymbol.IsNotNull());
                        Verify.That(
                                typeSymbol!.Locations.Single()
                                        .IsInSource.IsTrue());
                    });
        }
    }
}
