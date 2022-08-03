// -----------------------------------------------------------------------------
//  <copyright file="ExternalDependencyImplementationDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.External.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    // delegate ExternalDependencyContainerDefinition CreateExternalDependencyContainerDefinition(
    //         TypeModel parentInjectorType,
    //         ExternalDependencyDescriptor externalDependencyDescriptor,
    //         IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations);

    internal record ExternalDependencyImplementationDefinition(
            TypeModel ExternalDependencyImplementationType,
            TypeModel ExternalDependencyInterfaceType,
            IEnumerable<ExternalDependencyProviderMethodDefinition> ProviderMethodDefinitions,
            Location Location
    ) : IDefinition {
        // public class Builder {
        //     private readonly CreateExternalDependencyProviderMethodDefinition
        //             createExternalDependencyProviderMethodDefinition;
        //
        //     public Builder(CreateExternalDependencyProviderMethodDefinition externalDependencyProviderMethodDefinition) {
        //         createExternalDependencyProviderMethodDefinition = externalDependencyProviderMethodDefinition;
        //     }
        //
        //     public ExternalDependencyContainerDefinition Build(
        //             TypeModel parentInjectorType,
        //             ExternalDependencyDescriptor externalDependencyDescriptor,
        //             IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations) {
        //         var providerDefinitions = externalDependencyDescriptor.Providers.Select(
        //                         provider => createExternalDependencyProviderMethodDefinition(
        //                                 externalDependencyDescriptor,
        //                                 provider,
        //                                 factoryRegistrations))
        //                 .ToImmutableList();
        //
        //         return new ExternalDependencyContainerDefinition(
        //                 ParentInjectorType: parentInjectorType,
        //                 ChildInjectorType: externalDependencyDescriptor.DeclaringInjectorInterfaceType,
        //                 ExternalDependencyInterfaceType: externalDependencyDescriptor.ExternalDependencyInterfaceType,
        //                 providerDefinitions,
        //                 externalDependencyDescriptor.Location);
        //     }
        // }
    }
}
