// -----------------------------------------------------------------------------
//  <copyright file="SpecLinkDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Model;
using Phx.Inject.Common.Util;
using Phx.Inject.Generator.Extract.Metadata.Attributes;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SpecLinkDesc(
    QualifiedTypeModel InputType,
    QualifiedTypeModel ReturnType,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        SpecLinkDesc Extract(
            LinkAttributeMetadata link,
            Location linkLocation,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        private readonly QualifierAttributeMetadata.IAttributeExtractor qualifierAttributeExtractor;
        private readonly QualifierMetadata.IExtractor qualifierExtractor;

        public Extractor(
            QualifierMetadata.IExtractor qualifierExtractor,
            QualifierAttributeMetadata.IAttributeExtractor qualifierAttributeExtractor
        ) {
            this.qualifierExtractor = qualifierExtractor;
            this.qualifierAttributeExtractor = qualifierAttributeExtractor;
        }

        public Extractor() : this(
            QualifierMetadata.Extractor.Instance,
            QualifierAttributeMetadata.AttributeExtractor.Instance
        ) { }

        public SpecLinkDesc Extract(
            LinkAttributeMetadata link,
            Location linkLocation,
            ExtractorContext extractorCtx
        ) {
            var inputType = TypeModel.FromTypeSymbol(link.InputType);
            var returnType = TypeModel.FromTypeSymbol(link.OutputType);

            var inputQualifierAttribute = link.InputQualifier == null
                ? null
                : qualifierAttributeExtractor.Extract(link.AttributedSymbol, link.InputQualifier)
                    .GetOrThrow(extractorCtx)
                    .Also(_ => qualifierAttributeExtractor.ValidateAttributedType(link.InputQualifier, extractorCtx));
            var inputQualifier = inputQualifierAttribute != null
                ? new QualifierMetadata(new AttributeQualifier(inputQualifierAttribute), link.Location)
                : link.InputLabel != null
                    ? new QualifierMetadata(new LabelQualifier(link.InputLabel), link.Location)
                    : QualifierMetadata.NoQualifier;

            var outputQualifierAttribute = link.OutputQualifier == null
                ? null
                : qualifierAttributeExtractor.Extract(link.AttributedSymbol, link.OutputQualifier)
                    .GetOrThrow(extractorCtx)
                    .Also(_ => qualifierAttributeExtractor.ValidateAttributedType(link.OutputQualifier, extractorCtx));
            var outputQualifier = outputQualifierAttribute != null
                ? new QualifierMetadata(new AttributeQualifier(outputQualifierAttribute), link.Location)
                : link.OutputLabel != null
                    ? new QualifierMetadata(new LabelQualifier(link.OutputLabel), link.Location)
                    : QualifierMetadata.NoQualifier;

            return new SpecLinkDesc(
                new QualifiedTypeModel(inputType, inputQualifier),
                new QualifiedTypeModel(returnType, outputQualifier),
                linkLocation);
        }
    }
}
