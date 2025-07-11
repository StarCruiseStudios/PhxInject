// -----------------------------------------------------------------------------
// <copyright file="QualifierAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class QualifierAttributeMetadata : AttributeMetadata {
    public const string QualifierAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(QualifierAttribute)}";

    public QualifierAttributeMetadata(ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) { }

    public QualifierAttributeMetadata(ISymbol attributedSymbol, INamedTypeSymbol attributeTypeSymbol)
        : base(attributedSymbol, attributeTypeSymbol) { }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<QualifierAttributeMetadata> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(
            AttributeExtractor.Instance,
            AttributeHelper.Instance);

        private readonly IAttributeExtractor attributeExtractor;
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(
            IAttributeExtractor attributeExtractor,
            IAttributeHelper attributeHelper
        ) {
            this.attributeExtractor = attributeExtractor;
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributedSymbol.GetAttributes()
                .Any(attributeData => attributeExtractor.CanExtract(attributeData.AttributeClass!));
        }

        public IResult<QualifierAttributeMetadata> Extract(ISymbol attributedSymbol) {
            var attributeDatas = attributedSymbol.GetAttributes()
                .Where(attributeData => attributeExtractor.CanExtract(attributeData.AttributeClass!))
                .ToImmutableList();

            foreach (var attributeData in attributeDatas) {
                var attributeResult = attributeExtractor.Extract(attributedSymbol, attributeData.AttributeClass!);
                if (!attributeResult.IsOk) {
                    return attributeResult;
                }
            }

            return attributeDatas.Count switch {
                1 => attributeHelper.ExpectSingleAttribute(attributedSymbol,
                    attributeDatas.Single().AttributeClass!.ToString()!,
                    attributeData => Result.Ok(new QualifierAttributeMetadata(attributedSymbol, attributeData))),
                > 1 => Result.Error<QualifierAttributeMetadata>(
                    $"Type {attributedSymbol.Name} can only have one {QualifierAttributeClassName}. Found {attributeDatas.Count}.",
                    attributedSymbol.Locations.First(),
                    Diagnostics.InvalidSpecification),
                _ => Result.Error<QualifierAttributeMetadata>(
                    $"Type {attributedSymbol.Name} must have an {QualifierAttributeClassName}.",
                    attributedSymbol.Locations.First(),
                    Diagnostics.InvalidSpecification)
            };
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            foreach (var attributeData in attributedSymbol.GetAttributes()
                .Where(attributeData => attributeExtractor.CanExtract(attributeData.AttributeClass!))
            ) {
                attributeExtractor.ValidateAttributedType(attributeData.AttributeClass!, generatorCtx);
            }
        }
    }

    public interface IAttributeExtractor {
        bool CanExtract(ISymbol attributeTypeSymbol);
        IResult<QualifierAttributeMetadata> Extract(ISymbol attributedSymbol, ISymbol attributeTypeSymbol);
        void ValidateAttributedType(ISymbol attributeTypeSymbol, IGeneratorContext generatorCtx);
    }

    public class AttributeExtractor : IAttributeExtractor {
        public static IAttributeExtractor Instance = new AttributeExtractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal AttributeExtractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributeTypeSymbol) {
            return attributeHelper.HasAttribute(attributeTypeSymbol, QualifierAttributeClassName);
        }

        public IResult<QualifierAttributeMetadata> Extract(ISymbol attributedSymbol, ISymbol attributeTypeSymbol) {
            if (attributeTypeSymbol is not INamedTypeSymbol namedSymbol) {
                return Result.Error<QualifierAttributeMetadata>(
                    $"Expected qualifier type {attributeTypeSymbol.Name} to be a type.",
                    attributedSymbol.Locations.First(),
                    Diagnostics.InvalidSpecification);
            }

            return attributeHelper.ExpectSingleAttribute(
                attributeTypeSymbol,
                QualifierAttributeClassName,
                attributeData => Result.Ok(
                    new QualifierAttributeMetadata(attributedSymbol, namedSymbol)));
        }

        public void ValidateAttributedType(ISymbol attributeTypeSymbol, IGeneratorContext generatorCtx) {
            if (attributeTypeSymbol is not INamedTypeSymbol namedSymbol) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Expected qualifier type {attributeTypeSymbol.Name} to be a type.",
                    attributeTypeSymbol.Locations.First(),
                    generatorCtx);
            }

            if (namedSymbol.BaseType?.ToString() != TypeNames.AttributeClassName) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Expected qualifier type {attributeTypeSymbol.Name} to be an Attribute type.",
                    attributeTypeSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
