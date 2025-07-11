// -----------------------------------------------------------------------------
// <copyright file="LinkAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record LinkAttributeMetadata(
    ITypeSymbol InputType,
    ITypeSymbol OutputType,
    INamedTypeSymbol? InputQualifier,
    string? InputLabel,
    INamedTypeSymbol? OutputQualifier,
    string? OutputLabel,
    AttributeMetadata Attribute
) {
    public const string LinkAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(LinkAttribute)}";

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IReadOnlyList<IResult<LinkAttributeMetadata>> ExtractAll(ISymbol attributedSymbol);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);
        private readonly AttributeMetadata.IAttributeExtractor attributeExtractor;

        internal Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) {
            this.attributeExtractor = attributeExtractor;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, LinkAttributeClassName);
        }

        public IReadOnlyList<IResult<LinkAttributeMetadata>> ExtractAll(ISymbol attributedSymbol) {
            return attributeExtractor.ExtractAll(attributedSymbol, LinkAttributeClassName)
                .Select(result => result.Map(attribute => {
                    var attributeData = attribute.AttributeData;
                    if (attribute.AttributeData.ConstructorArguments.Length != 2) {
                        return Result.Error<LinkAttributeMetadata>(
                            "Link attribute must have only an input and output type specified.",
                            attribute.Location,
                            Diagnostics.InternalError);
                    }

                    var inputTypeArgument = attributeData.ConstructorArguments[0].Value as ITypeSymbol;
                    var returnTypeArgument = attributeData.ConstructorArguments[1].Value as ITypeSymbol;

                    if (inputTypeArgument == null || returnTypeArgument == null) {
                        return Result.Error<LinkAttributeMetadata>(
                            "Link attribute must specify non-null types.",
                            attribute.Location,
                            Diagnostics.InvalidSpecification);
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
                        return Result.Error<LinkAttributeMetadata>(
                            "Link attribute cannot specify both input label and qualifier.",
                            attribute.Location,
                            Diagnostics.InvalidSpecification);
                    }

                    if (outputLabel != null && outputQualifier != null) {
                        return Result.Error<LinkAttributeMetadata>(
                            "Link attribute cannot specify both output label and qualifier.",
                            attribute.Location,
                            Diagnostics.InvalidSpecification);
                    }

                    return Result.Ok(new LinkAttributeMetadata(
                        inputTypeArgument,
                        returnTypeArgument,
                        inputQualifier,
                        inputLabel,
                        outputQualifier,
                        outputLabel,
                        attribute));
                }))
                .ToImmutableList();
        }
    }
}
