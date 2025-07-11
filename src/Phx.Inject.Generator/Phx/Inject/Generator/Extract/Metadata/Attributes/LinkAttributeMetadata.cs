// -----------------------------------------------------------------------------
// <copyright file="LinkAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class LinkAttributeMetadata : AttributeMetadata {
    public const string LinkAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(LinkAttribute)}";

    public ITypeSymbol InputType { get; }
    public ITypeSymbol OutputType { get; }
    public INamedTypeSymbol? InputQualifier { get; }
    public string? InputLabel { get; }
    public INamedTypeSymbol? OutputQualifier { get; }
    public string? OutputLabel { get; }

    public LinkAttributeMetadata(
        ITypeSymbol inputType,
        ITypeSymbol outputType,
        INamedTypeSymbol? inputQualifier,
        string? inputLabel,
        INamedTypeSymbol? outputQualifier,
        string? outputLabel,
        ISymbol attributedSymbol,
        AttributeData attributeData
    ) : base(attributedSymbol, attributeData) {
        InputType = inputType;
        OutputType = outputType;
        InputQualifier = inputQualifier;
        InputLabel = inputLabel;
        OutputQualifier = outputQualifier;
        OutputLabel = outputLabel;
    }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IReadOnlyList<IResult<LinkAttributeMetadata>> ExtractAll(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);

        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, LinkAttributeClassName);
        }

        public IReadOnlyList<IResult<LinkAttributeMetadata>> ExtractAll(ISymbol attributedSymbol) {
            return attributeHelper.GetAttributes(
                attributedSymbol,
                LinkAttributeClassName,
                attributeData => {
                    if (attributeData.ConstructorArguments.Length != 2) {
                        return Result.Error<LinkAttributeMetadata>(
                            "Link attribute must have only an input and output type specified.",
                            GetAttributeLocation(attributeData, attributedSymbol),
                            Diagnostics.InternalError);
                    }

                    var inputTypeArgument = attributeData.ConstructorArguments[0].Value as ITypeSymbol;
                    var returnTypeArgument = attributeData.ConstructorArguments[1].Value as ITypeSymbol;

                    if (inputTypeArgument == null || returnTypeArgument == null) {
                        return Result.Error<LinkAttributeMetadata>(
                            "Link attribute must specify non-null types.",
                            GetAttributeLocation(attributeData, attributedSymbol),
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
                            GetAttributeLocation(attributeData, attributedSymbol),
                            Diagnostics.InvalidSpecification);
                    }

                    if (outputLabel != null && outputQualifier != null) {
                        return Result.Error<LinkAttributeMetadata>(
                            "Link attribute cannot specify both output label and qualifier.",
                            GetAttributeLocation(attributeData, attributedSymbol),
                            Diagnostics.InvalidSpecification);
                    }

                    return Result.Ok(new LinkAttributeMetadata(
                        inputTypeArgument,
                        returnTypeArgument,
                        inputQualifier,
                        inputLabel,
                        outputQualifier,
                        outputLabel,
                        attributedSymbol,
                        attributeData));
                });
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not ITypeSymbol {
                    TypeKind: TypeKind.Interface or TypeKind.Class,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    "Link can only be declared on a public or internal class or interface.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
