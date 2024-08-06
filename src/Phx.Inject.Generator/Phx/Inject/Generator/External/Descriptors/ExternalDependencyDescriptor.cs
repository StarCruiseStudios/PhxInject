// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyDescriptor.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.External.Descriptors {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Descriptors;
    using Phx.Inject.Generator.Specifications;
    using Phx.Inject.Generator.Specifications.Descriptors;

    internal delegate ExternalDependencyDescriptor CreateExternalDependencyDescriptor(
        ITypeSymbol externalDependencyInterfaceSymbol,
        DescriptorGenerationContext context
    );

    internal record ExternalDependencyDescriptor(
        TypeModel ExternalDependencyInterfaceType,
        IEnumerable<ExternalDependencyProviderDescriptor> Providers,
        Location Location
    ) : IDescriptor {
        public SpecDescriptor GetSpecDescriptor() {
            var factories = Providers.Select(
                    provider => new SpecFactoryDescriptor(
                        provider.ProvidedType,
                        provider.ProviderMethodName,
                        SpecFactoryMemberType.Method,
                        ImmutableList<QualifiedTypeModel>.Empty,
                        SpecFactoryMethodFabricationMode.Recurrent,
                        provider.isPartial,
                        provider.Location))
                .ToImmutableList();

            return new SpecDescriptor(
                ExternalDependencyInterfaceType,
                SpecInstantiationMode.Instantiated,
                factories,
                ImmutableList<SpecBuilderDescriptor>.Empty,
                ImmutableList<SpecLinkDescriptor>.Empty,
                Location);
        }

        public class Builder {
            private readonly CreateExternalDependencyProviderDescriptor createExternalDependencyProvider;

            public Builder(CreateExternalDependencyProviderDescriptor createExternalDependencyProvider) {
                this.createExternalDependencyProvider = createExternalDependencyProvider;
            }

            public ExternalDependencyDescriptor Build(
                ITypeSymbol externalDependencyInterfaceSymbol,
                DescriptorGenerationContext context
            ) {
                var externalDependencyInterfaceLocation = externalDependencyInterfaceSymbol.Locations.First();
                var externalDependencyInterfaceType = TypeModel.FromTypeSymbol(externalDependencyInterfaceSymbol);

                var providers = externalDependencyInterfaceSymbol
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Select(method => createExternalDependencyProvider(method, context))
                    .ToImmutableList();

                return new ExternalDependencyDescriptor(
                    externalDependencyInterfaceType,
                    providers,
                    externalDependencyInterfaceLocation);
            }
        }
    }
}
