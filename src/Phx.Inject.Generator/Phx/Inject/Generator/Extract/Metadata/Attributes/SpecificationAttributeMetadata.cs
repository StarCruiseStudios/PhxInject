// -----------------------------------------------------------------------------
// <copyright file="SpecificationAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class SpecificationAttributeMetadata : AttributeMetadata {
    public const string SpecificationAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(SpecificationAttribute)}";

    public SpecificationAttributeMetadata(ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) { }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<SpecificationAttributeMetadata> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, SpecificationAttributeClassName);
        }

        public IResult<SpecificationAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                SpecificationAttributeClassName,
                attributeData => Result.Ok(
                    new SpecificationAttributeMetadata(attributedSymbol, attributeData)));
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not { DeclaredAccessibility: Accessibility.Public or Accessibility.Internal }
                or not ITypeSymbol { IsStatic: true, TypeKind: TypeKind.Class }
                and not ITypeSymbol { TypeKind: TypeKind.Interface }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Specification type {attributedSymbol.Name} must be a static class or interface.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
