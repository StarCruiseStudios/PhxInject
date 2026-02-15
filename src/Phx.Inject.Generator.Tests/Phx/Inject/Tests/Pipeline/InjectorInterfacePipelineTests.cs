// -----------------------------------------------------------------------------
// <copyright file="InjectorInterfacePipelineTests.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Phx.Inject.Generator.Incremental;
using Phx.Inject.Tests.Helpers;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests.Pipeline;

/// <summary>
/// Tests for InjectorInterfacePipeline processing injector interface declarations.
/// </summary>
public class InjectorInterfacePipelineTests : LoggingTestClass {
    public static IEnumerable<TestCaseData> MultiFrameworkCases => new[] {
        new TestCaseData(ReferenceAssemblies.NetStandard.NetStandard20)
            .SetName("NetStandard20"),
        new TestCaseData(ReferenceAssemblies.Net.Net90)
            .SetName("Net90")
    };

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_ValidInjectorInterface_GeneratesImplementation(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Valid injector interface", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory] public static int GetInt() => 42;
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                int GetInt();
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));

        Then("Injector type is generated",
            () => {
                var injectorType = compilation.GetTypeByMetadataName(
                    "TestNamespace.ITestInjector");
                Verify.That(injectorType.IsNotNull());
            });
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_InjectorWithProviders_ExtractsProviderMethods(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector with multiple providers", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory] public static int GetInt() => 42;
                [Factory] public static string GetString() => ""test"";
                [Factory] public static double GetDouble() => 3.14;
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                int GetInt();
                string GetString();
                double GetDouble();
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_InjectorWithActivators_ExtractsActivatorMethods(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector with activator methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public int Value { get; set; }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory] public static TestClass GetTestClass() => new TestClass();
                [Builder] public static void BuildTestClass(TestClass instance) {
                    instance.Value = 42;
                }
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                TestClass GetTestClass();
                void BuildTestClass(TestClass instance);
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_InjectorWithChildProviders_ExtractsChildProviderMethods(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector with child providers", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class ParentSpec {
                [Factory] public static int GetInt() => 42;
            }
            
            [Specification]
            public static class ChildSpec {
                [Factory] public static string GetString(int value) => value.ToString();
            }
            
            [Injector(typeof(ParentSpec))]
            public interface IParentInjector {
                int GetInt();
                
                [ChildInjector(typeof(ChildSpec))]
                IChildInjector GetChildInjector();
            }
            
            [Injector(typeof(ChildSpec))]
            public interface IChildInjector {
                string GetString();
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_MissingInjectorAttribute_GeneratesNoImplementation(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Interface without Injector attribute", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public interface INotAnInjector {
                int GetInt();
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));

        Then("No generated implementation exists",
            () => {
                var generatedTypes = compilation.SyntaxTrees
                    .SelectMany(tree => tree.GetRoot().DescendantNodes())
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
                    .Where(c => c.Identifier.Text.Contains("NotAnInjector"))
                    .ToList();
                Verify.That(generatedTypes.Count.IsEqualTo(0));
            });
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_InjectorOnNonInterface_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector attribute on class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory] public static int GetInt() => 42;
            }
            
            [Injector(typeof(TestSpec))]
            public class NotAnInterface {
                public int GetInt() => 0;
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_MultipleInjectors_ProcessesAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Multiple injector interfaces", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class SpecA {
                [Factory] public static int GetInt() => 1;
            }
            
            [Specification]
            public static class SpecB {
                [Factory] public static string GetString() => ""test"";
            }
            
            [Injector(typeof(SpecA))]
            public interface IInjectorA {
                int GetInt();
            }
            
            [Injector(typeof(SpecB))]
            public interface IInjectorB {
                string GetString();
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));

        Then("Both injectors are generated",
            () => {
                var injectorA = compilation.GetTypeByMetadataName(
                    "TestNamespace.IInjectorA");
                var injectorB = compilation.GetTypeByMetadataName(
                    "TestNamespace.IInjectorB");
                Verify.That(injectorA.IsNotNull());
                Verify.That(injectorB.IsNotNull());
            });
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_GeneratedClassName_ExtractsCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector interface with specific name", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class MyApplicationSpec {
                [Factory] public static int GetInt() => 42;
            }
            
            [Injector(typeof(MyApplicationSpec))]
            public interface IMyApplicationInjector {
                int GetInt();
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));

        Then("Generated class has expected name",
            () => {
                var generatedType = compilation.GetTypeByMetadataName(
                    "TestNamespace.IMyApplicationInjector");
                Verify.That(generatedType.IsNotNull());
            });
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_InjectorWithNamespace_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector in nested namespace", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            namespace MyApp.DI {
                [Specification]
                public static class TestSpec {
                    [Factory] public static int GetInt() => 42;
                }
                
                [Injector(typeof(TestSpec))]
                public interface ITestInjector {
                    int GetInt();
                }
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_InjectorWithGenericProvider_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector with generic provider", () => @"
            using System.Collections.Generic;
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory]
                public static List<int> GetIntList() => new List<int> { 1, 2, 3 };
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                List<int> GetIntList();
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_InjectorWithParameters_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector with parameterized methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetFormattedString(int value) => value.ToString();
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string GetFormattedString(int value);
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_InjectorWithLabels_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector with labeled providers", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory, Label(""Primary"")]
                public static int GetPrimaryInt() => 1;
                
                [Factory, Label(""Secondary"")]
                public static int GetSecondaryInt() => 2;
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                [Label(""Primary"")]
                int GetPrimaryInt();
                
                [Label(""Secondary"")]
                int GetSecondaryInt();
            }
        }
        ");

        var compilation = When("Compiling source",
            () => TestCompiler.CompileText(
                source,
                referenceAssemblies,
                null,
                new IncrementalSourceGenerator()));

        var errors = When("Getting errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));
    }
}
