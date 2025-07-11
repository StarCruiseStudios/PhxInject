// -----------------------------------------------------------------------------
// <copyright file="BuilderReferenceAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class BuilderReferenceAttributeMetadata : AttributeMetadata {
    public const string BuilderReferenceAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(BuilderReferenceAttribute)}";

    public BuilderReferenceAttributeMetadata(ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) { }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<BuilderReferenceAttributeMetadata> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, BuilderReferenceAttributeClassName);
        }

        public IResult<BuilderReferenceAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                BuilderReferenceAttributeClassName,
                attributeData => Result.Ok(
                    new BuilderReferenceAttributeMetadata(attributedSymbol, attributeData)));
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not IFieldSymbol
                and not IPropertySymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Builder reference {attributedSymbol.Name} must be a public or internal static property or field."
                    + $"{attributedSymbol.IsStatic}, {attributedSymbol.DeclaredAccessibility}",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
