﻿// -----------------------------------------------------------------------------
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
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record FactoryAttributeMetadata(FactoryFabricationMode FabricationMode, AttributeMetadata AttributeMetadata)
    : IDescriptor {
    public const string FactoryAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(FactoryAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        FactoryAttributeMetadata ExtractFactory(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
        FactoryAttributeMetadata ExtractAutoFactory(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
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

        public FactoryAttributeMetadata ExtractFactory(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
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
                    generatorCtx);
            }

            var attribute = attributeExtractor.ExtractOne(attributedSymbol, FactoryAttributeClassName, generatorCtx);
            var fabricationMode =
                fabricationModeExtractor.Extract(attributedSymbol, attribute.AttributeData, generatorCtx);
            return new FactoryAttributeMetadata(fabricationMode, attribute);
        }

        public FactoryAttributeMetadata ExtractAutoFactory(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
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
                    generatorCtx);
            }

            var attribute = attributeExtractor.ExtractOne(attributedSymbol, FactoryAttributeClassName, generatorCtx);
            var fabricationMode =
                fabricationModeExtractor.Extract(attributedSymbol, attribute.AttributeData, generatorCtx);
            return new FactoryAttributeMetadata(fabricationMode, attribute);
        }
    }
}
