// -----------------------------------------------------------------------------
// <copyright file="LabelAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal class LabelAttributeMetadata : AttributeDesc {
    public const string LabelAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(LabelAttribute)}";

    public string Label { get; }
    public LabelAttributeMetadata(string label, ISymbol attributedSymbol, AttributeData attributeData)
        : base(attributedSymbol, attributeData) {
        Label = label;
    }

    public LabelAttributeMetadata(string label, ISymbol attributedSymbol, INamedTypeSymbol attributeTypeSymbol)
        : base(attributedSymbol, attributeTypeSymbol) {
        Label = label;
    }

    public interface IExtractor : IAttributeMetadataExtractor<LabelAttributeMetadata> { }

    public class Extractor : IExtractor {
        public static IExtractor Instance = new Extractor(AttributeHelper.Instance);
        private readonly IAttributeHelper attributeHelper;

        internal Extractor(IAttributeHelper attributeHelper) {
            this.attributeHelper = attributeHelper;
        }

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeHelper.HasAttribute(attributedSymbol, LabelAttributeClassName);
        }

        public IResult<LabelAttributeMetadata> Extract(ISymbol attributedSymbol) {
            return attributeHelper.ExpectSingleAttribute(
                attributedSymbol,
                LabelAttributeClassName,
                attributeData => {
                    var labels = attributeData.ConstructorArguments
                        .Where(argument => argument.Type!.ToString() == TypeNames.StringClassName)
                        .Select(argument => (string)argument.Value!)
                        .ToImmutableList();

                    if (labels.Count == 1) {
                        return Result.Ok(new LabelAttributeMetadata(labels.Single(), attributedSymbol, attributeData));
                    }

                    return Result.Error<LabelAttributeMetadata>(
                        $"Label for symbol {attributedSymbol.Name} must provide one label value.",
                        attributeData.GetLocation() ?? attributedSymbol.Locations.First(),
                        Diagnostics.InvalidSpecification);
                });
        }

        public void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx) { }
    }
}
