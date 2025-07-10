// -----------------------------------------------------------------------------
// <copyright file="ChildInjectorAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class ChildInjectorAttributeMetadata : AttributeDesc {
    public const string ChildInjectorAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(ChildInjectorAttribute)}";

    public ChildInjectorAttributeMetadata(ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) { }

    public interface IExtractor : IAttributeMetadataExtractor<ChildInjectorAttributeMetadata> { }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, ChildInjectorAttributeClassName);
        }

        public IResult<ChildInjectorAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                ChildInjectorAttributeClassName,
                attributeData => Result.Ok(
                    new ChildInjectorAttributeMetadata(attributedSymbol, attributeData)));
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not IMethodSymbol {
                    ReturnsVoid: false,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Child injector factory {attributedSymbol.Name} must be a public or internal method with a return type.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
