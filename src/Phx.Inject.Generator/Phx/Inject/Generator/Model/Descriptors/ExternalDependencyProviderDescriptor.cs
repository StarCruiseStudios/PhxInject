// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyProviderDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;

    internal delegate ExternalDependencyProviderDescriptor CreateExternalDependencyProviderDescriptor(
            IMethodSymbol providerMethod
    );

    internal record ExternalDependencyProviderDescriptor(
            QualifiedTypeDescriptor ProvidedType,
            string ProviderMethodName,
            Location Location
    ) : IDescriptor {
        public class Builder {
            public ExternalDependencyProviderDescriptor Build(IMethodSymbol providerMethod) {
                var providerLocation = providerMethod.Locations.First();

                if (providerMethod.ReturnsVoid) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            $"External dependency provider {providerMethod.Name} must return a type.",
                            providerLocation);
                }

                if (providerMethod.Parameters.Length > 0) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            $"External dependency provider {providerMethod.Name} must not have any parameters.",
                            providerLocation);
                }

                var returnType = TypeModel.FromTypeSymbol(providerMethod.ReturnType);
                var qualifier = SymbolProcessors.GetQualifier(providerMethod);
                return new ExternalDependencyProviderDescriptor(
                        new QualifiedTypeDescriptor(returnType, qualifier, providerLocation),
                        providerMethod.Name,
                        providerLocation);
            }
        }
    }
}
