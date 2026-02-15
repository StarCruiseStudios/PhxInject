// -----------------------------------------------------------------------------
// <copyright file="MetadataPipelineTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for MetadataPipeline orchestration and multi-framework support.
/// </summary>
public class MetadataPipelineTests : LoggingTestClass {
    public static IEnumerable<TestCaseData> MultiFrameworkCases => new[] {
        new TestCaseData(ReferenceAssemblies.NetStandard.NetStandard20)
            .SetName("NetStandard20"),
        new TestCaseData(ReferenceAssemblies.Net.Net90)
            .SetName("Net90")
    };

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_ValidSourceCode_GeneratesMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Valid source with multiple attributes", () => @"
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

        var errors = When("Getting compilation errors",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("No errors occur",
            () => Verify.That(errors.Count.IsEqualTo(0)));

        Then("Generated injector type exists",
            () => {
                var generatedType = compilation.GetTypeByMetadataName(
                    "TestNamespace.ITestInjector");
                Verify.That(generatedType.IsNotNull());
            });
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_MultipleSpecifications_ProcessesAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Multiple specifications", () => @"
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
            public interface IInjectorA { int GetInt(); }
            
            [Injector(typeof(SpecB))]
            public interface IInjectorB { string GetString(); }
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

        Then("Both injectors generated",
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
    public void Process_EmptySource_NoErrors(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Empty source file", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class EmptyClass { }
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
    public void Process_AllPipelineSegments_MergesDiagnostics(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Source with multiple valid elements", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class MainSpec {
                [Factory] public static int GetInt() => 42;
                [Builder] public static void BuildInt(int value) { }
            }
            
            [AutoFactory(FabricationMode.Immediate)]
            public class AutoFactoryClass {
                public AutoFactoryClass(int value) { }
            }
            
            [Injector(typeof(MainSpec))]
            public interface IMainInjector {
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
    public void Process_WithSettings_AppliesSettings(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Source with PhxInject settings", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [assembly: PhxInject(TabSize = 4, GeneratedFileExtension = ""g.cs"")]
            
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
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_SpecificationInterface_ProcessesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Specification interface", () => @"
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
    public void Process_InjectorDependency_ProcessesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Injector dependency interface", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [InjectorDependency]
            public interface ITestDependency {
                int GetInt();
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetString([Dependency] ITestDependency dep) {
                    return dep.GetInt().ToString();
                }
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string GetString(ITestDependency dep);
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
    public void Process_AutoBuilder_ProcessesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoBuilder method", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public int Value { get; set; }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static TestClass GetTestClass() => new TestClass();
                
                [AutoBuilder]
                public static void BuildTestClass(TestClass instance, int value) {
                    instance.Value = value;
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
    public void Process_ComplexScenario_AllPipelineSegments(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Complex scenario with all pipeline types", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [assembly: PhxInject(TabSize = 4)]
            
            [InjectorDependency]
            public interface IExternalDep {
                int GetValue();
            }
            
            [Specification]
            public static class MainSpec {
                [Factory]
                public static string GetString([Dependency] IExternalDep dep) {
                    return dep.GetValue().ToString();
                }
            }
            
            [Specification]
            [SpecificationInterfaceType]
            public interface IHelperSpec {
                int GetInt();
            }
            
            [AutoFactory(FabricationMode.Immediate)]
            public class AutoClass {
                public AutoClass(int value) { }
            }
            
            [Injector(typeof(MainSpec))]
            public interface IMainInjector {
                string GetString(IExternalDep dep);
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
    public void Process_NullableEnabled_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Source with nullable enabled", () => @"
            #nullable enable
            using Phx.Inject;
            
            namespace TestNamespace {
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string? GetNullableString() => null;
                
                [Factory]
                public static int GetInt() => 42;
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                string? GetNullableString();
                int GetInt();
            }
            #nullable restore
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
