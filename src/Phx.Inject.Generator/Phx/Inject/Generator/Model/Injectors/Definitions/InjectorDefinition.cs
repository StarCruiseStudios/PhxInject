﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    // internal delegate InjectorDefinition CreateInjectorDefinition(
    //         InjectorDescriptor injectorDescriptor,
    //         IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations,
    //         IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
    // );

    internal record InjectorDefinition(
            TypeModel InjectorType,
            TypeModel InjectorInterfaceType,
            InjectorSpecContainerCollectionDefinition SpecContainerCollection,
            IEnumerable<InjectorChildFactoryDefinition> ChildFactories,
            IEnumerable<InjectorProviderDefinition> Providers,
            IEnumerable<InjectorBuilderDefinition> Builders,
            Location Location
    ) : IDefinition {
        // public class Builder {
        //     private readonly CreateExternalDependencyProviderMethodDefinition createExternalDependency;
        //     private readonly CreateInjectorProviderMethodDefinition createInjectorProviderMethod;
        //     private readonly CreateInjectorBuilderMethodDefinition createInjectorBuilderMethod;
        //     private readonly CreateSpecContainerCollectionDefinition createSpecContainerCollection;
        //
        //     public Builder(
        //             CreateExternalDependencyProviderMethodDefinition createExternalDependency,
        //             CreateInjectorProviderMethodDefinition createInjectorProviderMethod,
        //             CreateInjectorBuilderMethodDefinition createInjectorBuilderMethod,
        //             CreateSpecContainerCollectionDefinition createSpecContainerCollection
        //     ) {
        //         this.createExternalDependency = createExternalDependency;
        //         this.createInjectorProviderMethod = createInjectorProviderMethod;
        //         this.createInjectorBuilderMethod = createInjectorBuilderMethod;
        //         this.createSpecContainerCollection = createSpecContainerCollection;
        //     }
        //
        //     public InjectorDefinition Build(
        //             InjectorDescriptor injectorDescriptor,
        //             IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations,
        //             IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
        //     ) {
        //         var externalDependencies = injectorDescriptor.ExternalDependencies.Select(
        //                         externalDependency => createExternalDependency(
        //                                 externalDependency,
        //                                 factoryRegistrations))
        //                 .ToImmutableList();
        //         var providerMethods = injectorDescriptor.Providers.Select(
        //                         provider => createInjectorProviderMethod(
        //                                 provider,
        //                                 injectorDescriptor,
        //                                 factoryRegistrations))
        //                 .ToImmutableList();
        //         var builderMethods = injectorDescriptor.Builders.Select(
        //                         builder => createInjectorBuilderMethod(
        //                                 builder,
        //                                 injectorDescriptor,
        //                                 builderRegistrations))
        //                 .ToImmutableList();
        //
        //         var specContainerCollection = createSpecContainerCollection(injectorDescriptor);
        //
        //         return new InjectorDefinition(
        //                 InjectorType: injectorDescriptor.InjectorType,
        //                 InjectorInterfaceType: injectorDescriptor.InjectorInterfaceType,
        //                 specContainerCollection,
        //                 externalDependencies,
        //                 providerMethods,
        //                 builderMethods,
        //                 injectorDescriptor.Location);
        //     }
        // }
    }
}
