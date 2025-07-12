// -----------------------------------------------------------------------------
// <copyright file="BuilderReferenceAttributeMetadata.cs" company="Star Cruise Studios LLC">
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

internal record BuilderReferenceAttributeMetadata(AttributeMetadata AttributeMetadata) : IDescriptor {
    public const string BuilderReferenceAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(BuilderReferenceAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        BuilderReferenceAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, BuilderReferenceAttributeClassName);
        }

        public BuilderReferenceAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            if (attributedSymbol is
                not IFieldSymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
                and not IPropertySymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Builder reference {attributedSymbol.Name} must be a public or internal static property or field."
                    + $"{attributedSymbol.IsStatic}, {attributedSymbol.DeclaredAccessibility}",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            var attribute =
                attributeExtractor.ExtractOne(attributedSymbol, BuilderReferenceAttributeClassName, currentCtx);
            return new BuilderReferenceAttributeMetadata(attribute);
        }
    }
}
