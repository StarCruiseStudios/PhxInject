// -----------------------------------------------------------------------------
// <copyright file="AutoFactoryPipelineTests.cs" company="Star Cruise Studios LLC">
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

namespace Phx.Inject.Tests.Pipeline;

/// <summary>
/// Tests for AutoFactoryPipeline processing AutoFactory attribute declarations.
/// </summary>
public class AutoFactoryPipelineTests : LoggingTestClass {
    
    public static IEnumerable<TestCaseData> ReferenceAssemblyCases => ImmutableList.Create(
        ReferenceAssemblies.NetStandard.NetStandard20,
        ReferenceAssemblies.NetStandard.NetStandard21,
        ReferenceAssemblies.Net.Net70,
        ReferenceAssemblies.Net.Net90
    ).Select(referenceAssembly => 
        new TestCaseData(referenceAssembly).SetName(referenceAssembly.TargetFramework));

    [TestCaseSource(nameof(ReferenceAssemblyCases))]
    public void Process_ValidAutoFactoryClass_ExtractsMetadata(ReferenceAssemblies referenceAssemblies) {
        var source = Given("Valid AutoFactory class", () => 
            TestSourceBuilder.Create()
                .WithAutoFactory("TestClass", "FabricationMode.Recurrent", "int value")
                .WithSpecification()
                .WithInjector("ITestInjector", "TestSpec", "TestClass GetTestClass();", "int GetInt();")
                .Build());

        var generator = Given("A source generator", () => new IncrementalSourceGenerator());
        
        var compilation = When("Compiling source with generator",
            () => TestCompiler.CompileText(source, referenceAssemblies, null, generator));

        var errors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        Then("No errors occur", 0, expected => 
            Verify.That(errors.Count.IsEqualTo(expected)));
    }

    [TestCaseSource(nameof(ReferenceAssemblyCases))]
    public void Process_AutoFactoryWithMultipleConstructors_ProcessesCorrectly(ReferenceAssemblies referenceAssemblies) {
        var source = Given("AutoFactory with multiple constructors", () =>
            TestSourceBuilder.Create()
                .WithCustomType(@"
            [AutoFactory]
            public class MultiConstructorClass {
                public MultiConstructorClass(int value) { }
                public MultiConstructorClass(string text, int value) { }
            }")
                .WithSpecification("TestSpec", "string", "GetString", "\"test\"")
                .WithInjector("ITestInjector", "TestSpec", "MultiConstructorClass GetMultiConstructorClass();", "string GetString();", "int GetInt() => 42;")
                .Build());

        var generator = Given("A source generator", () => new IncrementalSourceGenerator());
        
        var compilation = When("Compiling source with generator",
            () => TestCompiler.CompileText(source, referenceAssemblies, null, generator));

        var errors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        Then("No errors occur", 0, expected => 
            Verify.That(errors.Count.IsEqualTo(expected)));
    }
}
