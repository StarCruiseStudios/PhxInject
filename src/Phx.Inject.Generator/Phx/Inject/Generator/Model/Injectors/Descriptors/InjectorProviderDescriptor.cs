// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Descriptors {
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;

    internal delegate InjectorProviderDescriptor? CreateInjectorProviderDescriptor(
            IMethodSymbol providerMethod,
            IDescriptorGenerationContext context
    );

    internal record InjectorProviderDescriptor(
            QualifiedTypeModel ProvidedType,
            string ProviderMethodName,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public InjectorProviderDescriptor? Build(
                    IMethodSymbol providerMethod,
                    IDescriptorGenerationContext context
            ) {
                var providerLocation = providerMethod.Locations.First();

                if (providerMethod.ReturnsVoid) {
                    // This is a builder, not a provider.
                    return null;
                }

                if (SymbolProcessors.GetChildInjectorAttributes(providerMethod).Any()) {
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
                var qualifier = SymbolProcessors.GetQualifier(providerMethod);
                return new InjectorProviderDescriptor(
                        new QualifiedTypeModel(returnType, qualifier),
                        providerMethod.Name,
                        providerLocation);
            }
        }
    }
}
