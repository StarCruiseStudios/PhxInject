﻿// -----------------------------------------------------------------------------
// <copyright file="CustomQualifierAttributeMetadata.cs" company="Star Cruise Studios LLC">
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

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record CustomQualifierAttributeMetadata(
    QualifierAttributeMetadata QualifierAttribute,
    AttributeMetadata AttributeMetadata
) : ITypeQualifierAttributeMetadata {
    public IQualifier Qualifier { get; } = new CustomQualifier(AttributeMetadata.AttributeTypeSymbol);
    public Location Location { get; } = AttributeMetadata.Location;

    public interface IAttributeExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        CustomQualifierAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class AttributeExtractor(
        QualifierAttributeMetadata.IExtractor qualifierAttributeExtractor,
        AttributeMetadata.IAttributeExtractor attributeExtractor
    ) : IAttributeExtractor {
        public static readonly IAttributeExtractor Instance = new AttributeExtractor(
            QualifierAttributeMetadata.Extractor.Instance,
            AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributedSymbol.GetAttributes()
                .Any(attributeData => qualifierAttributeExtractor.CanExtract(attributeData.GetNamedTypeSymbol()));
        }

        public CustomQualifierAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            var customQualifierAttributeData = attributedSymbol.GetAttributes()
                .Where(attributeData => qualifierAttributeExtractor.CanExtract(attributeData.GetNamedTypeSymbol()))
                .ToImmutableList();

            switch (customQualifierAttributeData.Count) {
                case 0:
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Type {attributedSymbol.Name} must have a custom qualifier attribute.",
                        attributedSymbol.GetLocationOrDefault(),
                        currentCtx);
                case > 1:
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Type {attributedSymbol.Name} can only have one custom qualifier attribute. Found {customQualifierAttributeData.Count}.",
                        attributedSymbol.GetLocationOrDefault(),
                        currentCtx);
            }

            var customQualifierType = customQualifierAttributeData.Single().GetNamedTypeSymbol();
            var qualifierAttribute = qualifierAttributeExtractor.Extract(customQualifierType, currentCtx);

            var customQualifierAttribute =
                attributeExtractor.ExtractOne(attributedSymbol,
                    customQualifierType.GetFullyQualifiedName(),
                    currentCtx);
            return new CustomQualifierAttributeMetadata(qualifierAttribute, customQualifierAttribute);
        }

        public void ValidateCustomQualifierType(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            foreach (var attributeData in attributedSymbol.GetAttributes()
                .Where(attributeData => qualifierAttributeExtractor.CanExtract(attributeData.GetNamedTypeSymbol()))
            ) {
                var namedSymbol = attributeData.GetNamedTypeSymbol();
                if (namedSymbol.BaseType?.GetFullyQualifiedName() != TypeNames.AttributeClassName) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Expected qualifier type {namedSymbol.Name} to be an Attribute type.",
                        namedSymbol.GetLocationOrDefault(),
                        currentCtx);
                }
            }
        }
    }

    public interface ITypeExtractor {
        bool CanExtract(ISymbol qualifierTypeSymbol);
        CustomQualifierAttributeMetadata Extract(
            ISymbol attributedTypeSymbol,
            ISymbol qualifierTypeSymbol,
            IGeneratorContext currentCtx);
    }

    public class TypeExtractor : ITypeExtractor {
        public static readonly ITypeExtractor Instance = new TypeExtractor(
            QualifierAttributeMetadata.Extractor.Instance,
            AttributeMetadata.TypeExtractor.Instance);

        private readonly AttributeMetadata.ITypeExtractor attributeTypeExtractor;
        private readonly QualifierAttributeMetadata.IExtractor qualifierAttributeExtractor;

        internal TypeExtractor(
            QualifierAttributeMetadata.IExtractor qualifierAttributeExtractor,
            AttributeMetadata.ITypeExtractor attributeTypeExtractor
        ) {
            this.qualifierAttributeExtractor = qualifierAttributeExtractor;
            this.attributeTypeExtractor = attributeTypeExtractor;
        }

        public bool CanExtract(ISymbol qualifierTypeSymbol) {
            return qualifierAttributeExtractor.CanExtract(qualifierTypeSymbol);
        }

        public CustomQualifierAttributeMetadata Extract(
            ISymbol attributedTypeSymbol,
            ISymbol qualifierTypeSymbol,
            IGeneratorContext currentCtx) {
            if (qualifierTypeSymbol is not INamedTypeSymbol qualifierTypeNameSymbol) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Expected qualifier type {qualifierTypeSymbol} to be a type.",
                    attributedTypeSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            if (qualifierTypeNameSymbol.BaseType?.GetFullyQualifiedName() != TypeNames.AttributeClassName) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Expected qualifier type {qualifierTypeNameSymbol.Name} to be an Attribute type.",
                    qualifierTypeNameSymbol.GetLocationOrDefault(),
                    currentCtx);
            }

            var qualifierAttribute = qualifierAttributeExtractor.Extract(qualifierTypeSymbol, currentCtx);
            var customQualifierAttribute =
                attributeTypeExtractor.Extract(attributedTypeSymbol, qualifierTypeNameSymbol, currentCtx);
            // var customQualifierAttribute = new AttributeMetadata(attributedTypeSymbol, qualifierTypeNameSymbol);

            return new CustomQualifierAttributeMetadata(qualifierAttribute, customQualifierAttribute);
        }
    }
}
