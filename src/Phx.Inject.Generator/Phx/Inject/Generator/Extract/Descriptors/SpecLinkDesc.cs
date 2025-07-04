// -----------------------------------------------------------------------------
//  <copyright file="SpecLinkDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
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
            ExtractorContext context
        );
    }

    public class Extractor : IExtractor {
        public SpecLinkDesc Extract(
            AttributeData linkAttribute,
            Location linkLocation,
            ExtractorContext context
        ) {
            if (linkAttribute.ConstructorArguments.Length != 2) {
                throw new InjectionException(
                    "Link attribute must have only an input and return type specified.",
                    Diagnostics.InternalError,
                    linkLocation,
                    context.GenerationContext);
            }

            var inputTypeArgument = linkAttribute.ConstructorArguments[0].Value as ITypeSymbol;
            var returnTypeArgument = linkAttribute.ConstructorArguments[1].Value as ITypeSymbol;

            if (inputTypeArgument == null || returnTypeArgument == null) {
                throw new InjectionException(
                    "Link attribute must specify non-null types.",
                    Diagnostics.InvalidSpecification,
                    linkLocation,
                    context.GenerationContext);
            }

            var inputType = TypeModel.FromTypeSymbol(inputTypeArgument);
            var returnType = TypeModel.FromTypeSymbol(returnTypeArgument);

            return new SpecLinkDesc(
                new QualifiedTypeModel(inputType, QualifiedTypeModel.NoQualifier),
                new QualifiedTypeModel(returnType, QualifiedTypeModel.NoQualifier),
                linkLocation);
        }
    }
}
