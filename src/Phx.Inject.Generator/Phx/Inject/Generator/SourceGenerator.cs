// -----------------------------------------------------------------------------
//  <copyright file="SourceGenerator.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Abstract;
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Map;
using Phx.Inject.Generator.Project;
using Phx.Inject.Generator.Render;

namespace Phx.Inject.Generator;

[Generator]
internal class SourceGenerator(
    SourceMetadata.IExtractor sourceExtractor,
    PhxInjectSettingsMetadata.IExtractor phxInjectSettingsExtractor
) : ISourceGenerator {
    public const string PhxInjectNamespace = "Phx.Inject";

    public SourceGenerator() : this(
        SourceMetadata.Extractor.Instance,
        PhxInjectSettingsMetadata.Extractor.Instance
    ) { }

    public void Initialize(GeneratorInitializationContext generatorInitializationContext) {
        generatorInitializationContext.RegisterForSyntaxNotifications(() => new SourceSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext executionContext) {
        GeneratorContext.UseContext(executionContext,
            currentCtx => {
                currentCtx.Aggregator.Aggregate(
                    "generating injection types",
                    () => {
                        // Abstract: Source code to syntax declarations.
                        var syntaxReceiver = currentCtx.ExecutionContext.SyntaxReceiver as SourceSyntaxReceiver
                            ?? throw Diagnostics.InternalError.AsFatalException(
                                $"Incorrect Syntax Receiver {currentCtx.ExecutionContext.SyntaxReceiver}.",
                                Location.None,
                                currentCtx);

                        // Extract: Syntax declarations to metadata.
                        IReadOnlyList<ITypeSymbol> settingsCandidates = syntaxReceiver.PhxInjectSettingsCandidates
                            .SelectCatching(
                                currentCtx.Aggregator,
                                syntaxNode =>
                                    $"extracting PhxInject settings from syntax {syntaxNode.Identifier.Text}",
                                syntaxNode => ExpectTypeSymbolFromDeclaration(syntaxNode, currentCtx))
                            .ToImmutableList();

                        IReadOnlyList<ITypeSymbol> injectorCandidates = syntaxReceiver.InjectorCandidates
                            .SelectCatching(
                                currentCtx.Aggregator,
                                syntaxNode => $"extracting injectors from syntax {syntaxNode.Identifier.Text}",
                                syntaxNode => ExpectTypeSymbolFromDeclaration(syntaxNode, currentCtx))
                            .ToImmutableList();

                        IReadOnlyList<ITypeSymbol> specificationCandidates = syntaxReceiver.SpecificationCandidates
                            .SelectCatching(
                                currentCtx.Aggregator,
                                syntaxNode => $"extracting specifications from syntax {syntaxNode.Identifier.Text}",
                                syntaxNode => ExpectTypeSymbolFromDeclaration(syntaxNode, currentCtx))
                            .ToImmutableList();

                        var settings = phxInjectSettingsExtractor
                            .Extract(settingsCandidates, currentCtx);
                        var sourceMetadata = sourceExtractor
                            .Extract(injectorCandidates, specificationCandidates, currentCtx);

                        // Map: Metadata to defs.
                        var injectionContextDefs = new SourceDefMapper(settings)
                            .Map(sourceMetadata, currentCtx);

                        // Project: Defs to templates.
                        var templates = new SourceTemplateProjector()
                            .Project(injectionContextDefs, currentCtx);

                        // Render: Templates to generated source.
                        new SourceRenderer(settings)
                            .Render(templates, currentCtx);
                    });
            });
    }
    private static ITypeSymbol ExpectTypeSymbolFromDeclaration(
        TypeDeclarationSyntax syntaxNode,
        IGeneratorContext currentCtx
    ) {
        var symbol = currentCtx.ExecutionContext.Compilation
            .GetSemanticModel(syntaxNode.SyntaxTree)
            .GetDeclaredSymbol(syntaxNode);
        return symbol as ITypeSymbol
            ?? throw Diagnostics.InternalError.AsException(
                $"Expected a type declaration, but found {symbol?.Kind.ToString() ?? "null"} for {syntaxNode.Identifier.Text}.",
                syntaxNode.GetLocation(),
                currentCtx);
    }
}
