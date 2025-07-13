// -----------------------------------------------------------------------------
// <copyright file="FactoryAttributeMetadata.cs" company="Star Cruise Studios LLC">
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

internal record FactoryAttributeMetadata(FactoryFabricationMode FabricationMode, AttributeMetadata AttributeMetadata)
    : IMetadata {
    public const string FactoryAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(FactoryAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        FactoryAttributeMetadata ExtractFactory(ISymbol attributedSymbol, IGeneratorContext currentCtx);
        FactoryAttributeMetadata ExtractAutoFactory(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class Extractor(
        AttributeMetadata.IAttributeExtractor attributeExtractor,
        FactoryFabricationModeMetadata.IExtractor fabricationModeExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance,
            FactoryFabricationModeMetadata.Extractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, FactoryAttributeClassName);
        }

        public FactoryAttributeMetadata ExtractFactory(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            if (attributedSymbol is not IMethodSymbol {
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
                and not IPropertySymbol {
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Factory {attributedSymbol.Name} must be a public or internal method or property.",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            var attribute = attributeExtractor.ExtractOne(attributedSymbol, FactoryAttributeClassName, currentCtx);
            var fabricationMode =
                fabricationModeExtractor.Extract(attributedSymbol, attribute.AttributeData, currentCtx);
            return new FactoryAttributeMetadata(fabricationMode, attribute);
        }

        public FactoryAttributeMetadata ExtractAutoFactory(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            if (attributedSymbol is not ITypeSymbol {
                    TypeKind: TypeKind.Class,
                    IsStatic: false,
                    IsAbstract: false,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Auto factory {attributedSymbol.Name} must be a public or internal non-abstract class.",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            var attribute = attributeExtractor.ExtractOne(attributedSymbol, FactoryAttributeClassName, currentCtx);
            var fabricationMode =
                fabricationModeExtractor.Extract(attributedSymbol, attribute.AttributeData, currentCtx);
            return new FactoryAttributeMetadata(fabricationMode, attribute);
        }
    }
}
