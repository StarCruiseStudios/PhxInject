// -----------------------------------------------------------------------------
//  <copyright file="LinkDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using System;
    using Microsoft.CodeAnalysis;

    internal delegate LinkDescriptor CreateLinkDescriptor(AttributeData linkAttribute, Location linkLocation);

    internal record LinkDescriptor(
            QualifiedTypeDescriptor InputType,
            QualifiedTypeDescriptor ReturnType,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public LinkDescriptor Build(AttributeData linkAttribute, Location linkLocation) {
                if (linkAttribute.ConstructorArguments.Length != 2) {
                    throw new InjectionException(
                            Diagnostics.InternalError,
                            $"Link attribute must have only an input and return type specified.",
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

                return new LinkDescriptor(
                        new QualifiedTypeDescriptor(inputType, QualifiedTypeDescriptor.NoQualifier, linkLocation),
                        new QualifiedTypeDescriptor(returnType, QualifiedTypeDescriptor.NoQualifier, linkLocation),
                        linkLocation);
            }
        }
    }
}
