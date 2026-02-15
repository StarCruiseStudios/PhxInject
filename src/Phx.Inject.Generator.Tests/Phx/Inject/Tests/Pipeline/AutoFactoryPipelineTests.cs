// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryPipelineTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for AutoFactoryPipeline processing AutoFactory attribute declarations.
/// </summary>
public class AutoFactoryPipelineTests : LoggingTestClass {
    public static IEnumerable<TestCaseData> MultiFrameworkCases => new[] {
        new TestCaseData(ReferenceAssemblies.NetStandard.NetStandard20)
            .SetName("NetStandard20"),
        new TestCaseData(ReferenceAssemblies.Net.Net90)
            .SetName("Net90")
    };

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_ValidAutoFactoryClass_ExtractsMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Valid AutoFactory class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [AutoFactory(FabricationMode.Immediate)]
            public class TestClass {
                public TestClass(int value) { }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static int GetInt() => 42;
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                TestClass GetTestClass();
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
    public void Process_AutoFactoryWithFabricationMode_ExtractsMode(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoFactory with FabricationMode", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [AutoFactory(FabricationMode.Lazy)]
            public class LazyClass {
                public LazyClass(string value) { }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static string GetString() => ""test"";
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                LazyClass GetLazyClass();
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
    public void Process_AutoFactoryWithConstructorParameters_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoFactory with multiple constructor parameters", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [AutoFactory(FabricationMode.Immediate)]
            public class MultiParamClass {
                public MultiParamClass(int intValue, string stringValue, double doubleValue) { }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory] public static int GetInt() => 42;
                [Factory] public static string GetString() => ""test"";
                [Factory] public static double GetDouble() => 3.14;
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                MultiParamClass GetMultiParamClass();
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
    public void Process_AutoFactoryWithRequiredProperties_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoFactory with required properties", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [AutoFactory(FabricationMode.Immediate)]
            public class RequiredPropsClass {
                public RequiredPropsClass() { }
                
                public required int RequiredInt { get; init; }
                public required string RequiredString { get; init; }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory] public static int GetInt() => 42;
                [Factory] public static string GetString() => ""test"";
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                RequiredPropsClass GetRequiredPropsClass();
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
    public void Process_AutoFactoryOnAbstractClass_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoFactory on abstract class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [AutoFactory(FabricationMode.Immediate)]
            public abstract class AbstractClass {
                public AbstractClass(int value) { }
            }
            
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_AutoFactoryOnStaticClass_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoFactory on static class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [AutoFactory(FabricationMode.Immediate)]
            public static class StaticClass {
            }
            
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_AutoFactoryOnInterface_ReportsDiagnostic(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoFactory on interface", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [AutoFactory(FabricationMode.Immediate)]
            public interface ITestInterface {
            }
            
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

        var diagnostics = When("Getting diagnostics",
            () => compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList());

        Then("Diagnostic is reported",
            () => Verify.That(diagnostics.Any().IsTrue()));
    }

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_MultipleAutoFactories_ProcessesAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Multiple AutoFactory classes", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [AutoFactory(FabricationMode.Immediate)]
            public class ClassA {
                public ClassA(int value) { }
            }
            
            [AutoFactory(FabricationMode.Lazy)]
            public class ClassB {
                public ClassB(string value) { }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory] public static int GetInt() => 42;
                [Factory] public static string GetString() => ""test"";
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                ClassA GetClassA();
                ClassB GetClassB();
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
    public void Process_AutoFactoryWithGenericParameters_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoFactory with generic parameter", () => @"
            using System.Collections.Generic;
            using Phx.Inject;
            
            namespace TestNamespace {
            [AutoFactory(FabricationMode.Immediate)]
            public class GenericParamClass {
                public GenericParamClass(List<int> values) { }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory]
                public static List<int> GetIntList() => new List<int> { 1, 2, 3 };
            }
            
            [Injector(typeof(TestSpec))]
            public interface ITestInjector {
                GenericParamClass GetGenericParamClass();
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
    public void Process_AutoFactoryWithNestedClass_HandlesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoFactory on nested class", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            public class OuterClass {
                [AutoFactory(FabricationMode.Immediate)]
                public class InnerClass {
                    public InnerClass(int value) { }
                }
            }
            
            [Specification]
            public static class TestSpec {
                [Factory] public static int GetInt() => 42;
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
}
