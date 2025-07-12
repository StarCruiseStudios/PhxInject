// -----------------------------------------------------------------------------
// <copyright file="BuilderAttributeMetadata.cs" company="Star Cruise Studios LLC">
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

internal record BuilderAttributeMetadata(AttributeMetadata AttributeMetadata) : IDescriptor {
    public const string BuilderAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(BuilderAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        BuilderAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, BuilderAttributeClassName);
        }

        public BuilderAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            if (attributedSymbol is not IMethodSymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Builder {attributedSymbol.Name} must be a public or internal static method.",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            var attribute = attributeExtractor.ExtractOne(attributedSymbol, BuilderAttributeClassName, currentCtx);
            return new BuilderAttributeMetadata(attribute);
        }
    }
}
