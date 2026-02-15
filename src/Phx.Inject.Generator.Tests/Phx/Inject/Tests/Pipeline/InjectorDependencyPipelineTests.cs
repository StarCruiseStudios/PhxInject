// -----------------------------------------------------------------------------
// <copyright file="InjectorDependencyPipelineTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for InjectorDependencyPipeline processing InjectorDependency interface declarations.
/// </summary>
public class InjectorDependencyPipelineTests : LoggingTestClass {
    public static IEnumerable<TestCaseData> MultiFrameworkCases => new[] {
        new TestCaseData(ReferenceAssemblies.NetStandard.NetStandard20)
            .SetName("NetStandard20"),
        new TestCaseData(ReferenceAssemblies.Net.Net90)
            .SetName("Net90")
    };

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_ValidInjectorDependency_ExtractsMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Valid injector dependency interface", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            public interface IExternalDependency {
                int GetInt();
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetString([Dependency] IExternalDependency dep) {
                    return dep.GetInt().ToString();
                }
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string GetString(IExternalDependency dep);
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
    public void Process_InjectorDependencyWithMethods_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector dependency with multiple methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            public interface IExternalDependency {
                int GetInt();
                string GetString();
                double GetDouble();
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetResult([Dependency] IExternalDependency dep) {
                    return $""{dep.GetInt()}, {dep.GetString()}, {dep.GetDouble()}"";
                }
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string GetResult(IExternalDependency dep);
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
    public void Process_InjectorDependencyWithProperties_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector dependency with properties", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            public interface IExternalDependency {
                int IntValue { get; }
                string StringValue { get; }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetResult([Dependency] IExternalDependency dep) {
                    return $""{dep.IntValue}, {dep.StringValue}"";
                }
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string GetResult(IExternalDependency dep);
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
    public void Process_InjectorDependencyWithGenericTypes_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector dependency with generic types", () => @"
            using System.Collections.Generic;
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            public interface IExternalDependency {
                List<int> GetIntList();
                Dictionary<string, int> GetDictionary();
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetResult([Dependency] IExternalDependency dep) {
                    return string.Join("","", dep.GetIntList());
                }
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string GetResult(IExternalDependency dep);
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
    public void Process_InjectorDependencyOnClass_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("InjectorDependency on class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            public class ExternalDependency {
                public int GetInt() => 42;
            }
            
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_InternalInjectorDependency_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Internal injector dependency", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            internal interface IExternalDependency {
                int GetInt();
            }
            
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_MultipleInjectorDependencies_ProcessesAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Multiple injector dependencies", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            public interface IDepA {
                int GetInt();
            }
            
            [InjectorDependency]
            public interface IDepB {
                string GetString();
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetResult(
                    [Dependency] IDepA depA, 
                    [Dependency] IDepB depB) {
                    return $""{depA.GetInt()}, {depB.GetString()}"";
                }
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string GetResult(IDepA depA, IDepB depB);
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
    public void Process_InjectorDependencyWithParameters_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector dependency with parameterized methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            public interface IExternalDependency {
                string Format(int value);
                int Calculate(int a, int b);
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetResult([Dependency] IExternalDependency dep) {
                    return dep.Format(dep.Calculate(1, 2));
                }
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string GetResult(IExternalDependency dep);
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
    public void Process_InjectorDependencyInNamespace_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector dependency in namespace", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            namespace MyApp.Dependencies {
                [InjectorDependency]
                public interface IExternalDependency {
                    int GetInt();
                }
            }
            
            namespace MyApp {
                using MyApp.Dependencies;
                
                [Specification]
                public static class TestSpec {
                    [Factory]
                    public static string GetResult([Dependency] IExternalDependency dep) {
                        return dep.GetInt().ToString();
                    }
                }
                
                [Injector(typeof(TestSpec))]
                public interface ITestInjector {
                    string GetResult(IExternalDependency dep);
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
    public void Process_InjectorDependencyWithLabels_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector dependency with labels", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            public interface IExternalDependency {
                [Label(""Primary"")]
                int GetPrimaryInt();
                
                [Label(""Secondary"")]
                int GetSecondaryInt();
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetResult([Dependency] IExternalDependency dep) {
                    return $""{dep.GetPrimaryInt()}, {dep.GetSecondaryInt()}"";
                }
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string GetResult(IExternalDependency dep);
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
