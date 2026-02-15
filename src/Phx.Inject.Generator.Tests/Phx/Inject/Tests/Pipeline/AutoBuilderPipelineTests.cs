// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderPipelineTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for AutoBuilderPipeline processing AutoBuilder attribute declarations.
/// </summary>
public class AutoBuilderPipelineTests : LoggingTestClass {
    public static IEnumerable<TestCaseData> MultiFrameworkCases => new[] {
        new TestCaseData(ReferenceAssemblies.NetStandard.NetStandard20)
            .SetName("NetStandard20"),
        new TestCaseData(ReferenceAssemblies.Net.Net90)
            .SetName("Net90")
    };

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_ValidAutoBuilderMethod_ExtractsMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Valid AutoBuilder method", () => @"
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
                
                [Factory]
                public static int GetInt() => 42;
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
    public void Process_AutoBuilderWithMultipleParameters_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoBuilder with multiple parameters", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public int IntValue { get; set; }
                public string StringValue { get; set; }
                public double DoubleValue { get; set; }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static TestClass GetTestClass() => new TestClass();
                
                [AutoBuilder]
                public static void BuildTestClass(
                    TestClass instance, 
                    int intValue, 
                    string stringValue, 
                    double doubleValue) {
                    instance.IntValue = intValue;
                    instance.StringValue = stringValue;
                    instance.DoubleValue = doubleValue;
                }
                
                [Factory] public static int GetInt() => 42;
                [Factory] public static string GetString() => ""test"";
                [Factory] public static double GetDouble() => 3.14;
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
    public void Process_AutoBuilderWithOnlyInstanceParameter_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoBuilder with only instance parameter", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public void Initialize() { }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static TestClass GetTestClass() => new TestClass();
                
                [AutoBuilder]
                public static void BuildTestClass(TestClass instance) {
                    instance.Initialize();
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
    public void Process_MultipleAutoBuilders_ProcessesAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Multiple AutoBuilder methods", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public int Value1 { get; set; }
                public int Value2 { get; set; }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static TestClass GetTestClass() => new TestClass();
                
                [AutoBuilder]
                public static void BuildValue1(TestClass instance, int value) {
                    instance.Value1 = value;
                }
                
                [AutoBuilder]
                public static void BuildValue2(TestClass instance, int value) {
                    instance.Value2 = value * 2;
                }
                
                [Factory] public static int GetInt() => 42;
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
    public void Process_AutoBuilderOnPrivateMethod_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoBuilder on private method", () => @"
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
                private static void BuildTestClass(TestClass instance) {
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_AutoBuilderOnAbstractMethod_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoBuilder on abstract method", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public int Value { get; set; }
            }
            
            public abstract class AbstractSpec {
                [AutoBuilder]
                public abstract void BuildTestClass(TestClass instance);
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static TestClass GetTestClass() => new TestClass();
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_AutoBuilderWithNonVoidReturn_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoBuilder with non-void return", () => @"
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
                public static int BuildTestClass(TestClass instance) {
                    instance.Value = 42;
                    return 0;
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_AutoBuilderWithGenericParameters_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoBuilder with generic parameter", () => @"
            using System.Collections.Generic;
            using Phx.Inject;
            
            namespace TestNamespace {
            public class TestClass {
                public List<int> Values { get; set; }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static TestClass GetTestClass() => new TestClass();
                
                [AutoBuilder]
                public static void BuildTestClass(TestClass instance, List<int> values) {
                    instance.Values = values;
                }
                
                [Factory]
                public static List<int> GetIntList() => new List<int> { 1, 2, 3 };
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
    public void Process_AutoBuilderInNestedClass_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoBuilder in nested class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class OuterClass {
                public class InnerClass {
                    public int Value { get; set; }
                }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static OuterClass.InnerClass GetInnerClass() => new OuterClass.InnerClass();
                
                [AutoBuilder]
                public static void BuildInnerClass(OuterClass.InnerClass instance, int value) {
                    instance.Value = value;
                }
                
                [Factory]
                public static int GetInt() => 42;
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                OuterClass.InnerClass GetInnerClass();
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
    public void Process_AutoBuilderWithLabels_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoBuilder with labels", () => @"
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
                [Label(""Primary"")]
                public static void BuildPrimary(TestClass instance, int value) {
                    instance.Value = value;
                }
                
                [AutoBuilder]
                [Label(""Secondary"")]
                public static void BuildSecondary(TestClass instance, int value) {
                    instance.Value = value * 2;
                }
                
                [Factory, Label(""Primary"")]
                public static int GetPrimaryInt() => 1;
                
                [Factory, Label(""Secondary"")]
                public static int GetSecondaryInt() => 2;
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
}
