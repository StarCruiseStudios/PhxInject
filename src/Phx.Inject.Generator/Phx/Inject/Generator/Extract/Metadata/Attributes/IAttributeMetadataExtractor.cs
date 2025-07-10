// -----------------------------------------------------------------------------
// <copyright file="IAttributeMetadataExtractor.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2025 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Descriptors;

namespace Phx.Inject.Generator.Extract.Metadata.Attributes;

internal interface IAttributeMetadataExtractor<T> where T : AttributeDesc {
    bool CanExtract(ISymbol attributedSymbol);
    IResult<T> Extract(ISymbol attributedSymbol);
    void ValidateAttributedType(ISymbol attributedSymbol, IGeneratorContext generatorCtx);
}

internal static class AttributeMetadataExtensions {
    public static T? TryExtract<T>(
        this IAttributeMetadataExtractor<T> extractor,
        ISymbol attributedSymbol,
        IGeneratorContext generatorCtx)
        where T : AttributeDesc {
        return extractor.CanExtract(attributedSymbol)
            ? extractor.Extract(attributedSymbol)
                .GetOrThrow(generatorCtx)
                .Also(_ => extractor.ValidateAttributedType(attributedSymbol, generatorCtx))
            : null;
    }

    public static T Expect<T>(
        this IAttributeMetadataExtractor<T> extractor,
        ISymbol attributedSymbol,
        IGeneratorContext generatorCtx)
        where T : AttributeDesc {
        if (!extractor.CanExtract(attributedSymbol)) {
            throw Diagnostics.InvalidSpecification.AsException(
                $"Type {attributedSymbol} must declare an {InjectorAttributeMetadata.InjectorAttributeClassName}.",
                attributedSymbol.Locations.First(),
                generatorCtx);
        }

        return extractor.Extract(attributedSymbol)
            .GetOrThrow(generatorCtx)
            .Also(_ => extractor.ValidateAttributedType(attributedSymbol, generatorCtx));
    }
}
