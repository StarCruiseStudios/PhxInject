// -----------------------------------------------------------------------------
// <copyright file="QualifierAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record QualifierAttributeMetadata(AttributeMetadata AttributeMetadata) : IDescriptor {
    public const string QualifierAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(QualifierAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        QualifierAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, QualifierAttributeClassName);
        }

        public QualifierAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            if (attributedSymbol is not INamedTypeSymbol namedSymbol) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Expected qualifier type {attributedSymbol.Name} to be a type.",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            if (namedSymbol.BaseType?.GetFullyQualifiedName() != TypeNames.AttributeClassName) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Expected qualifier type {attributedSymbol.Name} to be an Attribute type.",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            var attribute = attributeExtractor.ExtractOne(attributedSymbol, QualifierAttributeClassName, currentCtx);
            return new QualifierAttributeMetadata(attribute);
        }
    }
}
