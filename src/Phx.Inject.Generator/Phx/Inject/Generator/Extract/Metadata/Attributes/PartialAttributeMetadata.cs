// -----------------------------------------------------------------------------
// <copyright file="PartialAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record PartialAttributeMetadata(AttributeMetadata AttributeMetadata) : IDescriptor {
    public const string PartialAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(PartialAttribute)}";

    private static readonly IImmutableSet<string> PartialTypes = ImmutableHashSet.CreateRange(new[] {
        TypeNames.IReadOnlyListClassName, TypeNames.ISetClassName, TypeNames.IReadOnlyDictionaryClassName
    });

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        PartialAttributeMetadata Extract(
            TypeModel partialType,
            ISymbol attributedSymbol,
            IGeneratorContext currentCtx);
    }

    public class Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, PartialAttributeClassName);
        }

        public PartialAttributeMetadata Extract(
            TypeModel partialType,
            ISymbol attributedSymbol,
            IGeneratorContext currentCtx) {
            if (attributedSymbol is not IMethodSymbol
                and not IPropertySymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Partial factory {attributedSymbol.Name} must be a public or internal static method or property.",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            if (!PartialTypes.Contains(partialType.NamespacedBaseTypeName)) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Partial factories must return one of [{string.Join(", ", PartialTypes)}].",
                    attributedSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            var attribute = attributeExtractor.ExtractOne(attributedSymbol, PartialAttributeClassName, currentCtx);
            return new PartialAttributeMetadata(attribute);
        }
    }
}
