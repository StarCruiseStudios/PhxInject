// -----------------------------------------------------------------------------
// <copyright file="LinkAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record LinkAttributeMetadata(
    ITypeSymbol InputType,
    ITypeSymbol OutputType,
    INamedTypeSymbol? InputQualifier,
    string? InputLabel,
    INamedTypeSymbol? OutputQualifier,
    string? OutputLabel,
    AttributeMetadata AttributeMetadata
) : IDescriptor {
    public const string LinkAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(LinkAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public override string ToString() {
        var inputType = InputQualifier != null
            ? $"[@{InputQualifier}]{InputType}"
            : InputLabel != null
                ? $"[\"{InputLabel}\"]{InputType}"
                : InputType.ToString();
        var outputType = OutputQualifier != null
            ? $"[@{OutputQualifier}]{OutputType}"
            : OutputLabel != null
                ? $"[\"{OutputLabel}\"]{OutputType}"
                : OutputType.ToString();

        return $"LinkAttribute: {InputType} -> {OutputType}";
    }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IReadOnlyList<LinkAttributeMetadata> ExtractAll(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor(
        AttributeMetadata.IAttributeExtractor attributeExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, LinkAttributeClassName);
        }

        public IReadOnlyList<LinkAttributeMetadata> ExtractAll(
            ISymbol attributedSymbol,
            IGeneratorContext generatorCtx) {
            return attributeExtractor.ExtractAll(attributedSymbol, LinkAttributeClassName, generatorCtx)
                .SelectCatching(
                    generatorCtx.Aggregator,
                    attribute => $"extracting link attribute from {attributedSymbol}",
                    attribute => {
                        var attributeData = attribute.AttributeData;
                        if (attribute.AttributeData.ConstructorArguments.Length != 2) {
                            throw Diagnostics.InvalidSpecification.AsException(
                                "Link attribute must have only an input and output type specified.",
                                attribute.Location,
                                generatorCtx);
                        }

                        var inputTypeArgument = attributeData.ConstructorArguments[0].Value as ITypeSymbol;
                        var returnTypeArgument = attributeData.ConstructorArguments[1].Value as ITypeSymbol;

                        if (inputTypeArgument == null || returnTypeArgument == null) {
                            throw Diagnostics.InvalidSpecification.AsException(
                                "Link attribute must specify input and output types.",
                                attribute.Location,
                                generatorCtx);
                        }

                        var inputLabel = attributeData.NamedArguments
                            .FirstOrDefault(arg => arg.Key == nameof(LinkAttribute.InputLabel))
                            .Value.Value as string;

                        var inputQualifier = attributeData.NamedArguments
                            .FirstOrDefault(arg => arg.Key == nameof(LinkAttribute.InputQualifier))
                            .Value.Value as INamedTypeSymbol;

                        var outputLabel = attributeData.NamedArguments
                            .FirstOrDefault(arg => arg.Key == nameof(LinkAttribute.OutputLabel))
                            .Value.Value as string;

                        var outputQualifier = attributeData.NamedArguments
                            .FirstOrDefault(arg => arg.Key == nameof(LinkAttribute.OutputQualifier))
                            .Value.Value as INamedTypeSymbol;

                        if (inputLabel != null && inputQualifier != null) {
                            throw Diagnostics.InvalidSpecification.AsException(
                                "Link attribute cannot specify both input label and qualifier.",
                                attribute.Location,
                                generatorCtx);
                        }

                        if (outputLabel != null && outputQualifier != null) {
                            throw Diagnostics.InvalidSpecification.AsException(
                                "Link attribute cannot specify both output label and qualifier.",
                                attribute.Location,
                                generatorCtx);
                        }

                        return new LinkAttributeMetadata(
                            inputTypeArgument,
                            returnTypeArgument,
                            inputQualifier,
                            inputLabel,
                            outputQualifier,
                            outputLabel,
                            attribute);
                    })
                .ToImmutableList();
        }
    }
}
