// -----------------------------------------------------------------------------
// <copyright file="SpecificationAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record SpecificationAttributeMetadata(AttributeMetadata AttributeMetadata) : IDescriptor {
    public const string SpecificationAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(SpecificationAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        SpecificationAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, SpecificationAttributeClassName);
        }

        public SpecificationAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            if (attributedSymbol is not { DeclaredAccessibility: Accessibility.Public or Accessibility.Internal }
                or not ITypeSymbol { IsStatic: true, TypeKind: TypeKind.Class }
                and not ITypeSymbol { TypeKind: TypeKind.Interface }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Specification type {attributedSymbol.Name} must be a static class or interface.",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            var attribute =
                attributeExtractor.ExtractOne(attributedSymbol, SpecificationAttributeClassName, currentCtx);
            return new SpecificationAttributeMetadata(attribute);
        }
    }
}
