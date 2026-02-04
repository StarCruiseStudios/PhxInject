// -----------------------------------------------------------------------------
// <copyright file="TestCompiler.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Phx.Test;
using Phx.Validation;

namespace Phx.Inject.Tests.Helpers;

public static class TestCompiler {
    private static readonly CSharpParseOptions ParserOptions = new(LanguageVersion.CSharp13);

    private static readonly CSharpCompilationOptions CompilationOptions = new(
        OutputKind.DynamicallyLinkedLibrary,
        nullableContextOptions: NullableContextOptions.Enable);

    public static Compilation CompileDirectory(string directory, ReferenceAssemblies referenceAssemblies, params IIncrementalGenerator[] generators) {
        var directoryAbsolutePath = Path.Combine(TestContext.CurrentContext.TestDirectory, directory);
        var enumerationOptions = new EnumerationOptions {
            RecurseSubdirectories = true
        };
        var filesInDirectory = Directory.GetFiles(directoryAbsolutePath, "*.cs", enumerationOptions);

        IReadOnlyList<SyntaxTree> syntaxTrees = filesInDirectory.Select(File.ReadAllText)
            .Select(ParseText)
            .ToImmutableList();

        return Compile(syntaxTrees, referenceAssemblies, generators);
    }

    public static Compilation CompileText(
        string text,
        ReferenceAssemblies referenceAssemblies, 
        string[]? additionalFiles = null,
        params IIncrementalGenerator[] generators
    ) {
        var builder = ImmutableArray.CreateBuilder<SyntaxTree>();
        builder.Add(ParseText(text));

        if (additionalFiles is not null) {
            foreach (var additionalFile in additionalFiles) {
                var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, additionalFile);
                var fileText = File.ReadAllText(filePath);
                var syntaxTree = ParseText(fileText);
                builder.Add(syntaxTree);
            }
        }

        return Compile(builder.ToImmutable(), referenceAssemblies, generators);
    }

    private static SyntaxTree ParseText(string text) {
        return CSharpSyntaxTree.ParseText(text, ParserOptions, encoding: Encoding.UTF8);
    }

    private static Compilation Compile(IEnumerable<SyntaxTree> syntaxTrees, ReferenceAssemblies referenceAssemblies, IIncrementalGenerator[] generators) {
        var references = referenceAssemblies
            .ResolveAsync(null, default)
            .Result
            .Concat(
                Directory.GetFiles(TestContext.CurrentContext.TestDirectory, "*.dll")
                    .Where(it => !it.EndsWith(Assembly.GetExecutingAssembly().Location))
                    .Select(filePath => MetadataReference.CreateFromFile(filePath)))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            "Phx.Inject.Tests.Data",
            syntaxTrees,
            references,
            CompilationOptions);

        if (generators.Length > 0) {
            return RunGenerators(compilation, generators);
        }

        return compilation;
    }

    private static Compilation RunGenerators(Compilation compilation, IIncrementalGenerator[] generators) {
        var driver = CSharpGeneratorDriver.Create(
                generators.Select(it => it.AsSourceGenerator()),
                parseOptions: ParserOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _);

        var results = driver.GetRunResult().Results;
        var failureResults = results
            .Where(it => it.Exception != null)
            .Select(it => it.Exception!.ToString())
            .Concat(results.SelectMany(it => it.Diagnostics)
                .Where(it => it.Severity == DiagnosticSeverity.Error)
                .Select(it => it.ToString()))
            .ToImmutableList();
        Verify.That(failureResults.Any().IsFalse(), $"Compilation failed due to: {string.Join("\n", failureResults)}");

        return updatedCompilation;
    }
}
