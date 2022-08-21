// -----------------------------------------------------------------------------
//  <copyright file="GeneratorTests.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
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
            var injectorNamespace = ThenTheExpectedNamespaceExists(compilation.GlobalNamespace, "Phx", "Inject", "Tests", "Data", "Inject");

            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "CustomInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedConstructedInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedLabelInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedNestedSpecInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedRawInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedParentInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedChildInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedGrandchildInjector");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedPropertyFactoryInjector");
            // ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedFactoryReferenceInjector");
        }

        [Test]
        public void SpecTypesAreGenerated() {
            var compilation = CompileCode();
            var specNamespace = ThenTheExpectedNamespaceExists(compilation.GlobalNamespace, "Phx", "Inject", "Tests", "Data", "Specification");

            ThenTheNamespaceContainsTheExpectedType(specNamespace, "CustomInjector_LazySpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "CustomInjector_LeafLinks");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "CustomInjector_LeafSpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "CustomInjector_RootSpecification");

            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedConstructedInjector_IConstructedSpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedConstructedInjector_NonConstructedSpecification");

            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedLabelInjector_LabeledLeafSpecification");

            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedNestedSpecInjector_OuterNestedSpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedNestedSpecInjector_OuterNestedSpecification_Inner");

            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedRawInjector_LazySpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedRawInjector_LeafLinks");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedRawInjector_LeafSpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedRawInjector_RootSpecification");

            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedParentInjector_ParentSpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedChildInjector_ChildSpecification");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedGrandchildInjector_GrandchildSpecification");
            
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedPropertyFactoryInjector_IPropertyFactorySpec");
            ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedPropertyFactoryInjector_PropertyFactoryStaticSpec");
            
            // ThenTheNamespaceContainsTheExpectedType(specNamespace, "GeneratedFactoryReferenceInjector_FactoryReferenceSpec");
        }

        [Test]
        public void ExternalDependencyTypesAreGenerated() {
            var compilation = CompileCode();
            var dataNamespace = ThenTheExpectedNamespaceExists(compilation.GlobalNamespace, "Phx", "Inject", "Tests", "Data");

            var injectorNamespace = ThenTheExpectedNamespaceExists(dataNamespace, "Inject");
            var externalNamespace = ThenTheExpectedNamespaceExists(dataNamespace, "External");

            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedParentInjector_IChildExternalDependencies");
            ThenTheNamespaceContainsTheExpectedType(injectorNamespace, "GeneratedChildInjector_IGrandchildExternalDependencies");

            ThenTheNamespaceContainsTheExpectedType(externalNamespace, "GeneratedChildInjector_IChildExternalDependencies");
            ThenTheNamespaceContainsTheExpectedType(externalNamespace, "GeneratedGrandchildInjector_IGrandchildExternalDependencies");
        }

        private Compilation CompileCode() {
            var renderSettings = new RenderSettings(
                    ShouldWriteFiles: true,
                    OutputPath: Path.Join(TestContext.CurrentContext.TestDirectory, "Generated"));
            var generator = Given("A source generator.", () => new SourceGenerator(renderSettings));
            var rootDirectory = Given("A directory with source files.", () => TestFiles.RootDirectory);

            return When(
                    "The source is compiled with the generator.",
                    () => TestCompiler.CompileDirectory(rootDirectory, generator));
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
                            Verify.That(currentNamespace.IsNotNull(), $"Could not find namespace member '{namespaceName}'.");
                        }

                        return currentNamespace!;
                    });
        }

        private void ThenTheNamespaceContainsTheExpectedType(INamespaceSymbol namespaceSymbol, string expectedTypeName) {
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
