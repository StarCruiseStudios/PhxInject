// -----------------------------------------------------------------------------
// <copyright file="ChildInjectorAttributeMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal record ChildInjectorAttributeMetadata(AttributeMetadata AttributeMetadata) : IDescriptor {
    public const string ChildInjectorAttributeClassName =
        $"{SourceGenerator.PhxInjectNamespace}.{nameof(ChildInjectorAttribute)}";

    public Location Location { get; } = AttributeMetadata.Location;

    public interface IExtractor {
        bool CanExtract(ISymbol attributedSymbol);
        ChildInjectorAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx);
    }

    public class Extractor(AttributeMetadata.IAttributeExtractor attributeExtractor) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(AttributeMetadata.AttributeExtractor.Instance);

        public bool CanExtract(ISymbol attributedSymbol) {
            return attributeExtractor.CanExtract(attributedSymbol, ChildInjectorAttributeClassName);
        }

        public ChildInjectorAttributeMetadata Extract(ISymbol attributedSymbol, IGeneratorContext currentCtx) {
            var attribute =
                attributeExtractor.ExtractOne(attributedSymbol, ChildInjectorAttributeClassName, currentCtx);
            return new ChildInjectorAttributeMetadata(attribute);
        }
    }
}
