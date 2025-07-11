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
using Phx.Inject.Generator.Extract.Descriptors;
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Map;
using Phx.Inject.Generator.Project;
using Phx.Inject.Generator.Render;

namespace Phx.Inject.Generator;

[Generator]
internal class SourceGenerator : ISourceGenerator {
    public const string PhxInjectNamespace = "Phx.Inject";

    private readonly PhxInjectSettingsMetadata.IExtractor phxInjectSettingsExtractor;

    public SourceGenerator(PhxInjectSettingsMetadata.IExtractor phxInjectSettingsExtractor) {
        this.phxInjectSettingsExtractor = phxInjectSettingsExtractor;
    }

    public SourceGenerator() : this(new PhxInjectSettingsMetadata.Extractor()) { }

    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(() => new SourceSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext executionCtx) {
            GeneratorContext.UseContext(executionCtx,
                generatorCtx => {
                    generatorCtx.Aggregator.Aggregate(
                        "generating injection types",
                        () => {
                            // Abstract: Source code to syntax declarations.
                            var syntaxReceiver = generatorCtx.ExecutionContext.SyntaxReceiver as SourceSyntaxReceiver
                                ?? throw Diagnostics.InternalError.AsFatalException(
                                    $"Incorrect Syntax Receiver {generatorCtx.ExecutionContext.SyntaxReceiver}.",
                                    Location.None,
                                    generatorCtx);

                            // Extract: Syntax declarations to descriptors.
                            IReadOnlyList<ITypeSymbol> settingsCandidates = syntaxReceiver.PhxInjectSettingsCandidates
                                .SelectCatching(
                                    generatorCtx.Aggregator,
                                    syntaxNode =>
                                        $"extracting PhxInject settings from syntax {syntaxNode.Identifier.Text}",
                                    syntaxNode => ExpectTypeSymbolFromDeclaration(syntaxNode, generatorCtx))
                                .ToImmutableList();

                            IReadOnlyList<ITypeSymbol> injectorCandidates = syntaxReceiver.InjectorCandidates
                                .SelectCatching(
                                    generatorCtx.Aggregator,
                                    syntaxNode => $"extracting injectors from syntax {syntaxNode.Identifier.Text}",
                                    syntaxNode => ExpectTypeSymbolFromDeclaration(syntaxNode, generatorCtx))
                                .ToImmutableList();

                            IReadOnlyList<ITypeSymbol> specificationCandidates = syntaxReceiver.SpecificationCandidates
                                .SelectCatching(
                                    generatorCtx.Aggregator,
                                    syntaxNode => $"extracting specifications from syntax {syntaxNode.Identifier.Text}",
                                    syntaxNode => ExpectTypeSymbolFromDeclaration(syntaxNode, generatorCtx))
                                .ToImmutableList();

                            var settings = phxInjectSettingsExtractor
                                .Extract(settingsCandidates, generatorCtx);
                            var sourceDesc = new SourceDesc.Extractor()
                                .Extract(injectorCandidates, specificationCandidates, generatorCtx);

                            // Map: Descriptors to defs.
                            var injectionContextDefs = new SourceDefMapper(settings)
                                .Map(sourceDesc, generatorCtx);

                            // Project: Defs to templates.
                            var templates = new SourceTemplateProjector()
                                .Project(injectionContextDefs, generatorCtx);

                            // Render: Templates to generated source.
                            new SourceRenderer(settings)
                                .Render(templates, generatorCtx);
                        });
                });
    }
    private static ITypeSymbol ExpectTypeSymbolFromDeclaration(
        TypeDeclarationSyntax syntaxNode,
        IGeneratorContext generatorCtx
    ) {
        var symbol = generatorCtx.ExecutionContext.Compilation
            .GetSemanticModel(syntaxNode.SyntaxTree)
            .GetDeclaredSymbol(syntaxNode);
        return symbol as ITypeSymbol
            ?? throw Diagnostics.InternalError.AsException(
                $"Expected a type declaration, but found {symbol?.Kind.ToString() ?? "null"} for {syntaxNode.Identifier.Text}.",
                syntaxNode.GetLocation(),
                generatorCtx);
    }
}
