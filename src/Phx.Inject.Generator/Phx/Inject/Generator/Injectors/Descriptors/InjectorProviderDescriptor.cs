// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Descriptors {
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Descriptors;

    internal delegate InjectorProviderDescriptor? CreateInjectorProviderDescriptor(
            IMethodSymbol providerMethod,
            DescriptorGenerationContext context
    );

    internal record InjectorProviderDescriptor(
            QualifiedTypeModel ProvidedType,
            string ProviderMethodName,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public InjectorProviderDescriptor? Build(
                    IMethodSymbol providerMethod,
                    DescriptorGenerationContext context
            ) {
                var providerLocation = providerMethod.Locations.First();

                if (providerMethod.ReturnsVoid) {
                    // This is a builder, not a provider.
                    return null;
                }

                if (providerMethod.GetChildInjectorAttributes().Any()) {
                    // This is an injector child factory, not a provider.
                    return null;
                }

                if (providerMethod.Parameters.Length > 0) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            $"Injector provider {providerMethod.Name} must not have any parameters.",
                            providerLocation);
                }

                var returnType = TypeModel.FromTypeSymbol(providerMethod.ReturnType);
                var qualifier = MetadataHelpers.GetQualifier(providerMethod);
                return new InjectorProviderDescriptor(
                        new QualifiedTypeModel(returnType, qualifier),
                        providerMethod.Name,
                        providerLocation);
            }
        }
    }
}
