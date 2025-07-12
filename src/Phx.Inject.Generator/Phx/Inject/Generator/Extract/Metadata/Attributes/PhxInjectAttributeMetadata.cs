// -----------------------------------------------------------------------------
// <copyright file="PhxInjectAttributeDesc.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record PhxInjectAttributeMetadata(
    int? TabSize,
    string? GeneratedFileExtension,
    bool? NullableEnabled,
    bool? AllowConstructorFactories,
    AttributeMetadata AttributeMetadata
) : IDescriptor {
    public static readonly string PhxInjectAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(PhxInjectAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        PhxInjectAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);
        private readonly AttributeMetadata.IAttributeExtractor attributeExtractor;

        internal Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) {
            this.attributeExtractor = attributeExtractor;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, PhxInjectAttributeClassName);
        }

        public PhxInjectAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not ITypeSymbol { TypeKind: TypeKind.Class }) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"PhxInject settings type {attributedSymbol.Name} must be a class.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }

            var attribute = attributeExtractor.ExtractOne(attributedSymbol, PhxInjectAttributeClassName, generatorCtx);

            var tabSize = attribute.AttributeData.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.TabSize))
                .Value.Value as int?;

            var generatedFileExtension = attribute.AttributeData.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.GeneratedFileExtension))
                .Value.Value as string;

            var nullableEnabled = attribute.AttributeData.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.NullableEnabled))
                .Value.Value as bool?;

            var allowConstructorFactories = attribute.AttributeData.NamedArguments
                .FirstOrDefault(arg => arg.Key == nameof(PhxInjectAttribute.AllowConstructorFactories))
                .Value.Value as bool?;

            return new PhxInjectAttributeMetadata(
                tabSize,
                generatedFileExtension,
                nullableEnabled,
                allowConstructorFactories,
                attribute);
        }
    }
}
