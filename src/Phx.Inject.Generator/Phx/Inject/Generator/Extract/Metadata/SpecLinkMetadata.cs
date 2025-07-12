// -----------------------------------------------------------------------------
//  <copyright file="SpecLinkDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Descriptors;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Metadata;

internal record SpecLinkMetadata(
    TypeModel ContainingSpecificationType,
    QualifiedTypeModel InputType,
    QualifiedTypeModel ReturnType,
    LinkAttributeMetadata Attribute,
    ISymbol AttributedSymbol
) : IDescriptor {
    public Location Location {
        get => AttributedSymbol.GetLocationOrDefault();
    }

    public interface IExtractor {
        bool CanExtract(TypeModel containingSpecificationType);
        IReadOnlyList<SpecLinkMetadata> ExtractAll(
            TypeModel containingSpecificationType,
            ExtractorContext parentCtx
        );
    }

    public class Extractor(
        LinkAttributeMetadata.IExtractor linkAttributeExtractor,
        QualifierMetadata.ICustomQualifierTypeExtractor customQualifierTypeExtractor,
        QualifierMetadata.ILabelStringExtractor labelStringExtractor
    ) : IExtractor {
        public static readonly IExtractor Instance = new Extractor(
            LinkAttributeMetadata.Extractor.Instance,
            QualifierMetadata.CustomQualifierTypeExtractor.Instance,
            QualifierMetadata.LabelStringExtractor.Instance
        );

        public bool CanExtract(TypeModel containingSpecificationType) {
            return VerifyExtract(containingSpecificationType, null);
        }

        public IReadOnlyList<SpecLinkMetadata> ExtractAll(
            TypeModel containingSpecificationType,
            ExtractorContext parentCtx
        ) {
            return parentCtx.UseChildContext(
                $"extracting links from {containingSpecificationType}",
                containingSpecificationType.TypeSymbol,
                currentCtx => {
                    VerifyExtract(containingSpecificationType, currentCtx);

                    return linkAttributeExtractor.ExtractAll(containingSpecificationType.TypeSymbol, currentCtx)
                        .SelectCatching(
                            currentCtx.Aggregator,
                            linkAttribute => $"Extracting link for {linkAttribute}",
                            linkAttribute => {
                                var inputQualifierMetadata =
                                    linkAttribute.InputQualifier != null
                                        ? customQualifierTypeExtractor.Extract(
                                            linkAttribute.AttributeMetadata.AttributedSymbol,
                                            linkAttribute.InputQualifier,
                                            currentCtx)
                                        : linkAttribute.InputLabel != null
                                            ? labelStringExtractor.Extract(
                                                linkAttribute.InputLabel,
                                                linkAttribute.AttributeMetadata,
                                                currentCtx)
                                            : QualifierMetadata.NoQualifier;
                                var outputQualifierMetadata =
                                    linkAttribute.OutputQualifier != null
                                        ? customQualifierTypeExtractor.Extract(
                                            linkAttribute.AttributeMetadata.AttributedSymbol,
                                            linkAttribute.OutputQualifier,
                                            currentCtx)
                                        : linkAttribute.OutputLabel != null
                                            ? labelStringExtractor.Extract(
                                                linkAttribute.OutputLabel,
                                                linkAttribute.AttributeMetadata,
                                                currentCtx)
                                            : QualifierMetadata.NoQualifier;
                                var inputType = linkAttribute.InputType.ToQualifiedTypeModel(inputQualifierMetadata);
                                var returnType = linkAttribute.OutputType.ToQualifiedTypeModel(outputQualifierMetadata);
                                var attributedSymbol = containingSpecificationType.TypeSymbol;

                                return new SpecLinkMetadata(
                                    containingSpecificationType,
                                    inputType,
                                    returnType,
                                    linkAttribute,
                                    attributedSymbol);
                            })
                        .ToImmutableList();
                });
        }

        private bool VerifyExtract(TypeModel containingSpecificationType, IGeneratorContext? currentCtx) {
            if (!linkAttributeExtractor.CanExtract(containingSpecificationType.TypeSymbol)) {
                return currentCtx == null
                    ? false
                    : throw Diagnostics.InternalError.AsException(
                        $"Specification type {containingSpecificationType} must have a {LinkAttributeMetadata.LinkAttributeClassName}.",
                        containingSpecificationType.Location,
                        currentCtx);
            }

            return true;
        }
    }
}
