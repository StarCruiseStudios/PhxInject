// -----------------------------------------------------------------------------
//  <copyright file="TestCompiler.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Tests.Helpers {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class TestCompiler {
        public static readonly CSharpParseOptions parserOptions = new(LanguageVersion.CSharp9);

        public static readonly CSharpCompilationOptions compilationOptions = new(
                OutputKind.ConsoleApplication,
                nullableContextOptions: NullableContextOptions.Enable);

        public static Compilation CompileDirectory(string directory, params ISourceGenerator[] generators) {
            var directoryAbsolutePath = Path.Combine(TestContext.CurrentContext.TestDirectory, directory);
            var enumerationOptions = new EnumerationOptions {
                RecurseSubdirectories = true
            };
            var filesInDirectory = Directory.GetFiles(directoryAbsolutePath, "*.cs", enumerationOptions);

            var syntaxTrees = filesInDirectory.Select(filePath => File.ReadAllText(filePath))
                    .Select(ParseText)
                    .ToImmutableList();

            return Compile(syntaxTrees, generators);
        }

        public static Compilation CompileText(
                string text,
                string[]? additionalFiles = null,
                params ISourceGenerator[] generators
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

            return Compile(builder.ToImmutableArray(), generators);
        }

        private static SyntaxTree ParseText(string text) {
            return CSharpSyntaxTree.ParseText(text, parserOptions, encoding: Encoding.UTF8);
        }

        private static Compilation Compile(IEnumerable<SyntaxTree> syntaxTrees, ISourceGenerator[] generators) {
            var references = Directory.GetFiles(TestContext.CurrentContext.TestDirectory, "*.dll")
                    .Select(filePath => MetadataReference.CreateFromFile(filePath))
                    .Concat(
                            new MetadataReference[] {
                                MetadataReference.CreateFromFile(
                                        typeof(Binder).GetTypeInfo()
                                                .Assembly.Location)
                            })
                    .ToArray();

            var compilation = CSharpCompilation.Create(
                    "Phx.Inject.Tests.Data",
                    syntaxTrees,
                    references,
                    compilationOptions);

            if (generators.Length > 0) {
                return RunGenerators(compilation, generators);
            }

            return compilation;
        }

        private static Compilation RunGenerators(Compilation compilation, ISourceGenerator[] generators) {
            CSharpGeneratorDriver.Create(
                            ImmutableArray.Create(generators),
                            parseOptions: parserOptions)
                    .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var _);

            return updatedCompilation;
        }
    }
}
