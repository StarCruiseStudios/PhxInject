// -----------------------------------------------------------------------------
//  <copyright file="SpecLinkDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata;
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
        private readonly CustomQualifierAttributeMetadata.ITypeExtractor qualifierAttributeTypeExtractor;
        private readonly QualifierMetadata.IExtractor qualifierExtractor;

        public Extractor(
            QualifierMetadata.IExtractor qualifierExtractor,
            CustomQualifierAttributeMetadata.ITypeExtractor qualifierAttributeTypeExtractor
        ) {
            this.qualifierExtractor = qualifierExtractor;
            this.qualifierAttributeTypeExtractor = qualifierAttributeTypeExtractor;
        }

        public Extractor() : this(
            QualifierMetadata.Extractor.Instance,
            CustomQualifierAttributeMetadata.TypeExtractor.Instance
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
                : qualifierAttributeTypeExtractor.Extract(link.Attribute.AttributedSymbol,
                    link.InputQualifier,
                    extractorCtx);
            var inputQualifier = inputQualifierAttribute != null
                ? new QualifierMetadata(
                    new CustomQualifier(inputQualifierAttribute.AttributeMetadata.AttributeTypeSymbol),
                    link.Attribute.Location)
                : link.InputLabel != null
                    ? new QualifierMetadata(new LabelQualifier(link.InputLabel), link.Attribute.Location)
                    : QualifierMetadata.NoQualifier;

            var outputQualifierAttribute = link.OutputQualifier == null
                ? null
                : qualifierAttributeTypeExtractor.Extract(link.Attribute.AttributedSymbol,
                    link.OutputQualifier,
                    extractorCtx);
            var outputQualifier = outputQualifierAttribute != null
                ? new QualifierMetadata(
                    new CustomQualifier(outputQualifierAttribute.AttributeMetadata.AttributeTypeSymbol),
                    link.Attribute.Location)
                : link.OutputLabel != null
                    ? new QualifierMetadata(new LabelQualifier(link.OutputLabel), link.Attribute.Location)
                    : QualifierMetadata.NoQualifier;

            return new SpecLinkDesc(
                new QualifiedTypeModel(inputType, inputQualifier),
                new QualifiedTypeModel(returnType, outputQualifier),
                linkLocation);
        }
    }
}
