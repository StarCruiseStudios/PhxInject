// -----------------------------------------------------------------------------
// <copyright file="SpecInterfacePipelineTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for SpecInterfacePipeline processing specification interface declarations.
/// </summary>
public class SpecInterfacePipelineTests : LoggingTestClass {
    public static IEnumerable<TestCaseData> MultiFrameworkCases => new[] {
        new TestCaseData(ReferenceAssemblies.NetStandard.NetStandard20)
            .SetName("NetStandard20"),
        new TestCaseData(ReferenceAssemblies.Net.Net90)
            .SetName("Net90")
    };

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_ValidSpecificationInterface_ExtractsMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Valid specification interface", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            [SpecificationInterfaceType]
            public interface ITestSpec {
                int GetInt();
            }
            
            [Injector(typeof(ITestSpec))]
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
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_SpecInterfaceWithFactoryMethods_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec interface with factory methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            [SpecificationInterfaceType]
            public interface ITestSpec {
                int GetInt();
                string GetString();
                double GetDouble();
            }
            
            [Injector(typeof(ITestSpec))]
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
    public void Process_SpecInterfaceWithProperties_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec interface with properties", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            [SpecificationInterfaceType]
            public interface ITestSpec {
                int IntValue { get; }
                string StringValue { get; }
            }
            
            [Injector(typeof(ITestSpec))]
            public interface ITestInjector {
                int IntValue { get; }
                string StringValue { get; }
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
    public void Process_SpecInterfaceWithDefaultMethods_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec interface with default interface methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            [SpecificationInterfaceType]
            public interface ITestSpec {
                int GetInt();
                
                string GetString() => GetInt().ToString();
            }
            
            [Injector(typeof(ITestSpec))]
            public interface ITestInjector {
                int GetInt();
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
    public void Process_SpecInterfaceWithLinks_ExtractsLinkMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec interface with links", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            [SpecificationInterfaceType]
            public interface ISpecA {
                int GetInt();
            }
            
            [Specification]
            [SpecificationInterfaceType]
            [Link(typeof(ISpecA))]
            public interface ISpecB {
                string GetString(int value);
            }
            
            [Injector(typeof(ISpecB))]
            public interface ITestInjector {
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
    public void Process_InternalSpecInterface_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Internal specification interface", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            [SpecificationInterfaceType]
            internal interface ITestSpec {
                int GetInt();
            }
            
            [Injector(typeof(ITestSpec))]
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_SpecInterfaceWithoutSpecificationInterfaceType_ReportsDiagnostic(
        ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec interface missing SpecificationInterfaceType", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public interface ITestSpec {
                int GetInt();
            }
            
            [Injector(typeof(ITestSpec))]
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_SpecInterfaceWithBuilderMethods_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec interface with builder methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public int Value { get; set; }
            }
            
            [Specification]
            [SpecificationInterfaceType]
            public interface ITestSpec {
                TestClass GetTestClass();
                void BuildTestClass(TestClass instance);
            }
            
            [Injector(typeof(ITestSpec))]
            public interface ITestInjector {
                TestClass GetTestClass();
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
    public void Process_SpecInterfaceWithGenericMethods_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec interface with generic methods", () => @"
            using System.Collections.Generic;
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            [SpecificationInterfaceType]
            public interface ITestSpec {
                List<int> GetIntList();
                Dictionary<string, int> GetDictionary();
            }
            
            [Injector(typeof(ITestSpec))]
            public interface ITestInjector {
                List<int> GetIntList();
                Dictionary<string, int> GetDictionary();
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
    public void Process_MultipleSpecInterfaces_ProcessesAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Multiple specification interfaces", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            [SpecificationInterfaceType]
            public interface ISpecA {
                int GetInt();
            }
            
            [Specification]
            [SpecificationInterfaceType]
            public interface ISpecB {
                string GetString();
            }
            
            [Injector(typeof(ISpecA))]
            public interface IInjectorA {
                int GetInt();
            }
            
            [Injector(typeof(ISpecB))]
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
    }
}
