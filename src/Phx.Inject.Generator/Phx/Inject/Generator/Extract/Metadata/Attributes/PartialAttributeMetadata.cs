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
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record PartialAttributeMetadata(AttributeMetadata Attribute) : IDescriptor {
    public const string PartialAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(PartialAttribute)}";

    private static readonly IImmutableSet<string> PartialTypes = ImmutableHashSet.CreateRange(new[] {
        TypeNames.IReadOnlyListClassName, TypeNames.ISetClassName, TypeNames.IReadOnlyDictionaryClassName
    });

    public Location Location { get; } = Attribute.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        PartialAttributeMetadata Extract(
            TypeModel partialType,
            ISymbol attributedSymbol,
            IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);
        private readonly AttributeMetadata.IAttributeExtractor attributeExtractor;

        internal Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) {
            this.attributeExtractor = attributeExtractor;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, PartialAttributeClassName);
        }

        public PartialAttributeMetadata Extract(
            TypeModel partialType,
            ISymbol attributedSymbol,
            IGeneratorContext generatorCtx) {
            if (attributedSymbol is not IMethodSymbol
                and not IPropertySymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Partial factory {attributedSymbol.Name} must be a public or internal static method or property.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }

            if (!PartialTypes.Contains(partialType.NamespacedBaseTypeName)) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Partial factories must return one of [{string.Join(", ", PartialTypes)}].",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }

            var attribute = attributeExtractor.ExtractOne(attributedSymbol, PartialAttributeClassName, generatorCtx);
            return new PartialAttributeMetadata(attribute);
        }
    }
}
