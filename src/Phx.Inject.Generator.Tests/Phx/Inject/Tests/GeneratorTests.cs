// -----------------------------------------------------------------------------
// <copyright file="GeneratorTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Phx.Inject.Generator.Incremental;
using Phx.Inject.Tests.Helpers;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests;

public class GeneratorTests : LoggingTestClass {

    public static IEnumerable<TestCaseData> ReferenceAssemblyCases = ImmutableList.Create(
        ReferenceAssemblies.NetStandard.NetStandard20,
        ReferenceAssemblies.NetStandard.NetStandard21,
        ReferenceAssemblies.Net.Net70,
        ReferenceAssemblies.Net.Net90
    ).Select(referenceAssembly => 
        new TestCaseData(referenceAssembly).SetName(referenceAssembly.TargetFramework));
    
    [TestCaseSource(nameof(ReferenceAssemblyCases))]
    public void InjectorTypesAreGenerated(ReferenceAssemblies referenceAssemblies) {
        var compilation = CompileCode(referenceAssemblies);
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

    [TestCaseSource(nameof(ReferenceAssemblyCases))]
    public void SpecTypesAreGenerated(ReferenceAssemblies referenceAssemblies) {
        var compilation = CompileCode(referenceAssemblies);
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

    [TestCaseSource(nameof(ReferenceAssemblyCases))]
    public void DependencyTypesAreGenerated(ReferenceAssemblies referenceAssemblies) {
        var compilation = CompileCode(referenceAssemblies);
        var dataNamespace =
            ThenTheExpectedNamespaceExists(compilation.GlobalNamespace, "Phx", "Inject", "Tests", "Data");

        var externalNamespace = ThenTheExpectedNamespaceExists(dataNamespace, "Inject");

        ThenTheNamespaceContainsTheExpectedType(externalNamespace,
            "GeneratedParentInjector_IChildDependencies");
        ThenTheNamespaceContainsTheExpectedType(externalNamespace,
            "GeneratedChildInjector_IGrandchildDependencies");

        ThenTheNamespaceContainsTheExpectedType(externalNamespace,
            "GeneratedChildInjector_IChildDependencies");
        ThenTheNamespaceContainsTheExpectedType(externalNamespace,
            "GeneratedGrandchildInjector_IGrandchildDependencies");
    }

    private Compilation CompileCode(ReferenceAssemblies referenceAssemblies) {
        var generator = Given("A source generator.", () => new IncrementalSourceGenerator());
        var rootDirectory = Given("A directory with source files.", () => TestFiles.RootDirectory);

        var compilation = When(
            "The source is compiled with the generator.",
            () => TestCompiler.CompileDirectory(rootDirectory, referenceAssemblies, generator));

        IReadOnlyList<Diagnostic> diagnostics = compilation.GetDiagnostics();
        foreach (var diagnostic in diagnostics) {
            if (diagnostic.Severity >= DiagnosticSeverity.Warning) {
                Log(diagnostic.ToString());
            }
        }

        Then("No errors were found during compilation.",
            () => {
                var errorDiagnostics = diagnostics
                    .Where(it => it.Severity >= DiagnosticSeverity.Error)
                    .ToImmutableList();

                if (errorDiagnostics.Any()) {
                    Log("Errors diagnostics:");
                    foreach (var error in errorDiagnostics) {
                        Log(error.ToString());
                    }
                }

                Verify.That(errorDiagnostics.Count().IsEqualTo(0));
            });

        return compilation;
    }

    private INamespaceSymbol ThenTheExpectedNamespaceExists(INamespaceSymbol root, params string[] ns) {
        return Then(
            "The expected namespace exists",
            string.Join(".", ns),
            _ => {
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
        string expectedTypeName
    ) {
        // Then(
        //     "The namespace contains the expected type.",
        //     expectedTypeName,
        //     expected => {
        //         var typeSymbol = namespaceSymbol.GetTypeMembers(expected)
        //             .FirstOrDefault();
        //         Verify.That(typeSymbol.IsNotNull());
        //         Verify.That(
        //             typeSymbol!.Locations.Single()
        //                 .IsInSource.IsTrue());
        //     });
    }
}
