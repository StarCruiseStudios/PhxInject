// -----------------------------------------------------------------------------
//  <copyright file="SpecLinkDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Descriptors {
    using Microsoft.CodeAnalysis;

    internal delegate SpecLinkDescriptor CreateSpecLinkDescriptor(
            AttributeData linkAttribute,
            Location linkLocation,
            DescriptorGenerationContext context
    );

    internal record SpecLinkDescriptor(
            QualifiedTypeModel InputType,
            QualifiedTypeModel ReturnType,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public SpecLinkDescriptor Build(
                    AttributeData linkAttribute,
                    Location linkLocation,
                    DescriptorGenerationContext context
            ) {
                if (linkAttribute.ConstructorArguments.Length != 2) {
                    throw new InjectionException(
                            Diagnostics.InternalError,
                            "Link attribute must have only an input and return type specified.",
                            linkLocation);
                }

                var inputTypeArgument = linkAttribute.ConstructorArguments[0].Value as ITypeSymbol;
                var returnTypeArgument = linkAttribute.ConstructorArguments[1].Value as ITypeSymbol;

                if (inputTypeArgument == null || returnTypeArgument == null) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            "Link attribute must specify non-null types.",
                            linkLocation);
                }

                var inputType = TypeModel.FromTypeSymbol(inputTypeArgument);
                var returnType = TypeModel.FromTypeSymbol(returnTypeArgument);

                return new SpecLinkDescriptor(
                        new QualifiedTypeModel(inputType, QualifiedTypeModel.NoQualifier),
                        new QualifiedTypeModel(returnType, QualifiedTypeModel.NoQualifier),
                        linkLocation);
            }
        }
    }
}
