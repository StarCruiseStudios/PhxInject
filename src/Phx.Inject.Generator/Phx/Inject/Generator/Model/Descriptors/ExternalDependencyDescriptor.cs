// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Descriptors {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal delegate ExternalDependencyDescriptor CreateExternalDependencyDescriptor(
            ITypeSymbol externalDependencySymbol
    );

    internal record ExternalDependencyDescriptor(
            TypeModel ExternalDependencyInterfaceType,
            IEnumerable<ExternalDependencyProviderDescriptor> Providers,
            Location Location
    ) : IDescriptor {
        public class Builder {
            private readonly CreateExternalDependencyProviderDescriptor createProvider;

            public Builder(CreateExternalDependencyProviderDescriptor createProvider) {
                this.createProvider = createProvider;
            }

            public ExternalDependencyDescriptor Build(ITypeSymbol externalDependencySymbol) {
                var externalDependencyLocation = externalDependencySymbol.Locations.First();

                if (externalDependencySymbol.TypeKind != TypeKind.Interface) {
                    throw new InjectionException(
                            Diagnostics.InvalidSpecification,
                            $"External dependency type {externalDependencySymbol.Name} must be an interface.",
                            externalDependencyLocation);
                }

                var externalDependencyInterfaceType = TypeModel.FromTypeSymbol(externalDependencySymbol);
                var externalDependencyMethods = externalDependencySymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>();
                var providerMethods = externalDependencyMethods
                        .Select(method => createProvider(method))
                        .ToImmutableList();

                return new ExternalDependencyDescriptor(
                        externalDependencyInterfaceType,
                        providerMethods,
                        externalDependencyLocation);
            }
        }
    }
}
