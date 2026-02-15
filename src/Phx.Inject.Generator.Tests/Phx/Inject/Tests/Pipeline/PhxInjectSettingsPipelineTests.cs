// -----------------------------------------------------------------------------
// <copyright file="PhxInjectSettingsPipelineTests.cs" company="Star Cruise Studios LLC">
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
/// Tests for PhxInjectSettingsPipeline processing PhxInject settings.
/// </summary>
public class PhxInjectSettingsPipelineTests : LoggingTestClass {
    public static IEnumerable<TestCaseData> MultiFrameworkCases => new[] {
        new TestCaseData(ReferenceAssemblies.NetStandard.NetStandard20)
            .SetName("NetStandard20"),
        new TestCaseData(ReferenceAssemblies.Net.Net90)
            .SetName("Net90")
    };

    [TestCaseSource(nameof(MultiFrameworkCases))]
    public void Process_ValidPhxInjectSettings_ExtractsMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Valid PhxInject settings", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [assembly: PhxInject(TabSize = 4, GeneratedFileExtension = ""g.cs"")]
            
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
    public void Process_TabSizeSetting_ExtractsCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Settings with TabSize", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [assembly: PhxInject(TabSize = 2)]
            
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
    public void Process_GeneratedFileExtensionSetting_ExtractsCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Settings with GeneratedFileExtension", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [assembly: PhxInject(GeneratedFileExtension = ""generated.cs"")]
            
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
    public void Process_NullableEnabledSetting_ExtractsCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Settings with NullableEnabled", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [assembly: PhxInject(NullableEnabled = true)]
            
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
    public void Process_DefaultSettings_UsesDefaults(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Source without PhxInject settings", () => @"
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
    public void Process_MultipleSettingsAttributes_UsesFirst(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Multiple PhxInject settings attributes", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [assembly: PhxInject(TabSize = 4)]
            [assembly: PhxInject(TabSize = 2)]
            
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
    public void Process_AllSettings_ExtractsAll(ReferenceAssemblies referenceAssemblies) {
        var source = Given("All PhxInject settings specified", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [assembly: PhxInject(
                TabSize = 4, 
                GeneratedFileExtension = ""g.cs"", 
                NullableEnabled = true)]
            
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
    public void Process_SettingsSingleInstance_OnlyOnePerCompilation(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Source with settings", () => @"
            using Phx.Inject;
            
            namespace TestNamespace {
            [assembly: PhxInject(TabSize = 4)]
            
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

        Then("Both injectors generated with same settings",
            () => {
                var injectorA = compilation.GetTypeByMetadataName(
                    "TestNamespace.IInjectorA");
                var injectorB = compilation.GetTypeByMetadataName(
                    "TestNamespace.IInjectorB");
                Verify.That(injectorA.IsNotNull());
                Verify.That(injectorB.IsNotNull());
            });
    }
}
