// -----------------------------------------------------------------------------
//  <copyright file="SpecLinkDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;

namespace Phx.Inject.Generator.Extract.Descriptors;

internal record SpecLinkDesc(
    QualifiedTypeModel InputType,
    QualifiedTypeModel ReturnType,
    Location Location
) : IDescriptor {
    public interface IExtractor {
        SpecLinkDesc Extract(
            AttributeData linkAttribute,
            Location linkLocation,
            ExtractorContext extractorCtx
        );
    }

    public class Extractor : IExtractor {
        public SpecLinkDesc Extract(
            AttributeData linkAttribute,
            Location linkLocation,
            ExtractorContext extractorCtx
        ) {
            if (linkAttribute.ConstructorArguments.Length != 2) {
                throw Diagnostics.InternalError.AsFatalException(
                    "Link attribute must have only an input and return type specified.",
                    linkLocation,
                    extractorCtx);
            }

            var inputTypeArgument = linkAttribute.ConstructorArguments[0].Value as ITypeSymbol;
            var returnTypeArgument = linkAttribute.ConstructorArguments[1].Value as ITypeSymbol;

            if (inputTypeArgument == null || returnTypeArgument == null) {
                throw Diagnostics.InvalidSpecification.AsException(
                    "Link attribute must specify non-null types.",
                    linkLocation,
                    extractorCtx);
            }

            var inputType = TypeModel.FromTypeSymbol(inputTypeArgument);
            var returnType = TypeModel.FromTypeSymbol(returnTypeArgument);

            return new SpecLinkDesc(
                new QualifiedTypeModel(inputType, NoQualifier.Instance),
                new QualifiedTypeModel(returnType, NoQualifier.Instance),
                linkLocation);
        }
    }
}
