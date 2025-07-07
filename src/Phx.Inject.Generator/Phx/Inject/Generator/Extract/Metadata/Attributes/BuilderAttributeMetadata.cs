// -----------------------------------------------------------------------------
// <copyright file="BuilderAttributeMetadata.cs" company="Star Cruise Studios LLC">
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

internal class BuilderAttributeMetadata : AttributeDesc {
    public const string BuilderAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(BuilderAttribute)}";

    public BuilderAttributeMetadata(ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) { }

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        IResult<BuilderAttributeMetadata> Extract(ISymbol attributedSymbol);
        void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        private readonly IAttributeHelper attributeHelper;

        public Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public Extractor() : this(new AttributeHelper()) { }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, BuilderAttributeClassName);
        }

        public IResult<BuilderAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                BuilderAttributeClassName,
                attributeData => Result.Ok(
                    new BuilderAttributeMetadata(attributedSymbol, attributeData)));
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            if (attributedSymbol is not IMethodSymbol {
                    IsStatic: true,
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal
                }
            ) {
                throw Diagnostics.InvalidSpecification.AsException(
                    $"Builder {attributedSymbol.Name} must be a public or internal static method.",
                    attributedSymbol.Locations.First(),
                    generatorCtx);
            }
        }
    }
}
