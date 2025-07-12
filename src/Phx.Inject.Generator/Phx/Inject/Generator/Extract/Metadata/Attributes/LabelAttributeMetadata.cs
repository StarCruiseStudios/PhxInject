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
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record LabelAttributeMetadata(string Label, AttributeMetadata AttributeMetadata)
    : ITypeQualifierAttributeMetadata {
    public const string LabelAttributeClassName = $"{SourceGenerator.PhxInjectNamespace}.{nameof(LabelAttribute)}";

    public IQualifier Qualifier { get; } = new LabelQualifier(Label);
    public Location Location { get; } = AttributeMetadata.Location;

    public interface IAttributeExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        LabelAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class AttributeExtractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IAttributeExtractor {
        public static readonly IAttributeExtractor Instance =
            new AttributeExtractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, LabelAttributeClassName);
        }

        public LabelAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext generatorCtx) {
            var attribute = attributeExtractor.ExtractOne(attributedSymbol, LabelAttributeClassName, generatorCtx);
            var labels = attribute.AttributeData.ConstructorArguments
                .Where(argument => argument.Type.GetFullyQualifiedName() == TypeNames.StringClassName)
                .Select(argument => (string)argument.Value!)
                .ToImmutableList();

            if (labels.Count == 1) {
                return new LabelAttributeMetadata(labels.Single(), attribute);
            }

            throw Diagnostics.InvalidSpecification.AsException(
                $"Label for symbol {attributedSymbol.Name} must provide one label value.",
                attribute.Location,
                generatorCtx);
        }
    }

    public interface IStringExtractor {
        LabelAttributeMetadata Extract(
            string label,
            AttributeMetadata attributeMetadata,
            IGeneratorContext generatorCtx);
    }

    public class StringExtractor : IStringExtractor {
        public static readonly IStringExtractor Instance = new StringExtractor();

        public LabelAttributeMetadata Extract(
            string label,
            AttributeMetadata attributeMetadata,
            IGeneratorContext generatorCtx) {
            VerifyExtract(label, attributeMetadata, generatorCtx);

            return new LabelAttributeMetadata(label, attributeMetadata);
        }

        private bool VerifyExtract(string label, AttributeMetadata attributeMetadata, IGeneratorContext? generatorCtx) {
            if (generatorCtx != null) {
                if (string.IsNullOrWhiteSpace(label)) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Cannot extract label from a null or empty string\"{label}\".",
                        attributeMetadata.Location,
                        generatorCtx);
                }
            }

            return true;
        }
    }
}
