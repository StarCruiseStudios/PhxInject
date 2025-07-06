// -----------------------------------------------------------------------------
//  <copyright file="SpecLinkDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SpecLinkDesc(
    QualifiedTypeModel InputType,
    QualifiedTypeModel ReturnType,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        SpecLinkDesc Extract(
            LinkAttributeDesc link,
            Location linkLocation,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        public SpecLinkDesc Extract(
            LinkAttributeDesc link,
            Location linkLocation,
            ExtractorContext extractorCtx
        ) {
            var inputType = TypeModel.FromTypeSymbol(link.InputType);
            var returnType = TypeModel.FromTypeSymbol(link.OutputType);

            var inputQualifierAttribute = link.InputQualifier?
                .TryGetQualifierAttributeFromAttributeType(link)
                .GetOrThrow(extractorCtx);
            IQualifier inputQualifier = inputQualifierAttribute != null
                ? new AttributeQualifier(inputQualifierAttribute)
                : link.InputLabel != null
                    ? new LabelQualifier(link.InputLabel)
                    : NoQualifier.Instance;
            
            var outputQualifierAttribute = link.OutputQualifier?
                .TryGetQualifierAttributeFromAttributeType(link)
                .GetOrThrow(extractorCtx);
            IQualifier outputQualifier = outputQualifierAttribute != null
                ? new AttributeQualifier(outputQualifierAttribute)
                : link.OutputLabel != null
                    ? new LabelQualifier(link.OutputLabel)
                    : NoQualifier.Instance;

            return new SpecLinkDesc(
                new QualifiedTypeModel(inputType, inputQualifier),
                new QualifiedTypeModel(returnType, outputQualifier),
                linkLocation);
        }
    }
}
