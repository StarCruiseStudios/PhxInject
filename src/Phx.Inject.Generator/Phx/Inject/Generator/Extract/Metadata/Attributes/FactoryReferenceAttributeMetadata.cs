// -----------------------------------------------------------------------------
// <copyright file="FactoryReferenceAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record FactoryReferenceAttributeMetadata(
    FactoryFabricationMode FabricationMode,
    AttributeMetadata AttributeMetadata) : IMetadata {
    public const string FactoryReferenceAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(FactoryReferenceAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        FactoryReferenceAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class Extractor(
        AttributeMetadata.IAttributeExtractor attributeExtractor,
        FactoryFabricationModeMetadata.IExtractor fabricationModeExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance,
            FactoryFabricationModeMetadata.Extractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, FactoryReferenceAttributeClassName);
        }

        public FactoryReferenceAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            if (attributedSymbol is not IFieldSymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
                and not IPropertySymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Factory reference {attributedSymbol.Name} must be a public or internal static property or field.",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            var attribute =
                attributeExtractor.ExtractOne(attributedSymbol, FactoryReferenceAttributeClassName, currentCtx);
            var fabricationMode =
                fabricationModeExtractor.Extract(attributedSymbol, attribute.AttributeData, currentCtx);
            return new FactoryReferenceAttributeMetadata(fabricationMode, attribute);
        }
    }
}
