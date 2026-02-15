// -----------------------------------------------------------------------------
// <copyright file="SpecClassPipelineTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for SpecClassPipeline processing specification class declarations.
/// </summary>
public class SpecClassPipelineTests : LoggingTestClass {
    public static IEnumerable<TestCaseData> MultiFrameworkCases => new[] {
        new TestCaseData(ReferenceAssemblies.NetStandard.NetStandard20)
            .SetName("NetStandard20"),
        new TestCaseData(ReferenceAssemblies.Net.Net90)
            .SetName("Net90")
    };

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_ValidSpecificationClass_ExtractsMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Valid specification class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory]
                public static int GetInt() => 42;
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
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_SpecWithFactoryMethods_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec with multiple factory methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory]
                public static int GetInt() => 42;
                
                [Factory]
                public static string GetString() => ""test"";
                
                [Factory]
                public static double GetDouble() => 3.14;
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
    public void Process_SpecWithBuilderMethods_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec with builder methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public int Value { get; set; }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static TestClass GetTestClass() => new TestClass();
                
                [Builder]
                public static void BuildTestClass(TestClass instance) {
                    instance.Value = 42;
                }
            }
            
            [Injector(typeof(TestSpec))]
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
    public void Process_SpecWithFactoryProperties_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec with factory properties", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory]
                public static int IntValue => 42;
                
                [Factory]
                public static string StringValue => ""test"";
            }
            
            [Injector(typeof(TestSpec))]
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
    public void Process_SpecWithLinks_ExtractsLinkMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec with links", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class SpecA {
                [Factory]
                public static int GetInt() => 42;
            }
            
            [Specification]
            [Link(typeof(SpecA))]
            public static class SpecB {
                [Factory]
                public static string GetString(int value) => value.ToString();
            }
            
            [Injector(typeof(SpecB))]
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
    public void Process_InternalSpecClass_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Internal specification class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            internal static class TestSpec {
                [Factory]
                public static int GetInt() => 42;
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_NonStaticSpecClass_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Non-static specification class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public class TestSpec {
                [Factory]
                public int GetInt() => 42;
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_SpecWithFactoryReferences_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec with factory references", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class ExternalSpec {
                [Factory]
                public static int GetInt() => 42;
            }
            
            [Specification]
            public static class TestSpec {
                [FactoryReference(typeof(ExternalSpec))]
                public static int GetReferenceInt() => default;
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                int GetReferenceInt();
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
    public void Process_SpecWithBuilderReferences_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Spec with builder references", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public int Value { get; set; }
            }
            
            [Specification]
            public static class ExternalSpec {
                [Builder]
                public static void BuildTestClass(TestClass instance) {
                    instance.Value = 42;
                }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static TestClass GetTestClass() => new TestClass();
                
                [BuilderReference(typeof(ExternalSpec))]
                public static void BuildTestClassRef(TestClass instance) { }
            }
            
            [Injector(typeof(TestSpec))]
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
    public void Process_MultipleSpecClasses_ProcessesAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Multiple specification classes", () => @"
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
    }
}
