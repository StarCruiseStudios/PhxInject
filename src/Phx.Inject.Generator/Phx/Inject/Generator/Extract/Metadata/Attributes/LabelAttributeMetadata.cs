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

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        LabelAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
    }

    public class Extractor : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);
        private readonly AttributeMetadata.IAttributeExtractor attributeExtractor;

        internal Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) {
            this.attributeExtractor = attributeExtractor;
        }

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
}
