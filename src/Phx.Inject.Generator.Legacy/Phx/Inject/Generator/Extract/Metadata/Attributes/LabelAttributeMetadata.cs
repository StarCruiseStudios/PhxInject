// -----------------------------------------------------------------------------
// <copyright file="LabelAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
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
        LabelAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class AttributeExtractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IAttributeExtractor {
        public static readonly IAttributeExtractor Instance =
            new AttributeExtractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, LabelAttributeClassName);
        }

        public LabelAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            var attribute = attributeExtractor.ExtractOne(attributedSymbol, LabelAttributeClassName, currentCtx);
            var labels = attribute.AttributeData.ConstructorArguments
                .Where(argument => argument.Type.GetFullyQualifiedName() == TypeNames.StringPrimitiveTypeName)
                .Select(argument => (string)argument.Value!)
                .ToImmutableList();

            if (labels.Count == 1) {
                return new LabelAttributeMetadata(labels.Single(), attribute);
            }

            throw Diagnostics.InvalidSpecification.AsException(
                $"Label for symbol {attributedSymbol.Name} must provide one label value.",
                attribute.Location,
                currentCtx);
        }
    }

    public interface IStringExtractor {
        LabelAttributeMetadata Extract(
            string label,
            AttributeMetadata attributeMetadata,
            IGeneratorContext currentCtx);
    }

    public class StringExtractor : IStringExtractor {
        public static readonly IStringExtractor Instance = new StringExtractor();

        public LabelAttributeMetadata Extract(
            string label,
            AttributeMetadata attributeMetadata,
            IGeneratorContext currentCtx) {
            VerifyExtract(label, attributeMetadata, currentCtx);

            return new LabelAttributeMetadata(label, attributeMetadata);
        }

        private bool VerifyExtract(string label, AttributeMetadata attributeMetadata, IGeneratorContext? currentCtx) {
            if (currentCtx != null) {
                if (string.IsNullOrWhiteSpace(label)) {
                    throw Diagnostics.InvalidSpecification.AsException(
                        $"Cannot extract label from a null or empty string\"{label}\".",
                        attributeMetadata.Location,
                        currentCtx);
                }
            }

            return true;
        }
    }
}
