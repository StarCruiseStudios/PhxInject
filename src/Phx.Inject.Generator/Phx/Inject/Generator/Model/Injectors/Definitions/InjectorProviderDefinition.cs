﻿// -----------------------------------------------------------------------------
//  <copyright file="InjectorProviderDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Specifications.Definitions;

    // internal delegate InjectorProviderMethodDefinition CreateInjectorProviderMethodDefinition(
    //         InjectorProviderDescriptor injectorProviderDescriptor,
    //         InjectorDescriptor injectorDescriptor,
    //         IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
    // );

    internal record InjectorProviderDefinition(
            TypeModel ProvidedType,
            string InjectorProviderMethodName,
            SpecContainerFactoryInvocationDefinition SpecContainerFactoryInvocation,
            Location Location
    ) : IDefinition {
        // public class Builder {
        //     private readonly CreateSpecContainerFactoryInvocationDefinition createSpecContainerFactoryInvocation;
        //
        //     public Builder(
        //             CreateSpecContainerFactoryInvocationDefinition createSpecContainerFactoryInvocation
        //     ) {
        //         this.createSpecContainerFactoryInvocation = createSpecContainerFactoryInvocation;
        //     }
        //
        //     public InjectorProviderMethodDefinition Build(
        //             InjectorProviderDescriptor providerDescriptor,
        //             InjectorDescriptor injectorDescriptor,
        //             IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
        //     ) {
        //         if (!factoryRegistrations.TryGetValue(
        //                     RegistrationIdentifier.FromQualifiedTypeDescriptor(providerDescriptor.ProvidedType),
        //                     out var factoryRegistration)) {
        //             throw new InjectionException(
        //                     Diagnostics.IncompleteSpecification,
        //                     $"Cannot find factory for type {providerDescriptor.ProvidedType} required by provider method in injector {injectorDescriptor.InjectorInterfaceType}.",
        //                     providerDescriptor.Location);
        //         }
        //
        //         var specContainerFactoryInvocation = createSpecContainerFactoryInvocation(
        //                 injectorDescriptor,
        //                 factoryRegistration,
        //                 providerDescriptor.Location);
        //
        //         return new InjectorProviderMethodDefinition(
        //                 providerDescriptor.ProvidedType.TypeModel,
        //                 providerDescriptor.ProviderMethodName,
        //                 specContainerFactoryInvocation,
        //                 providerDescriptor.Location);
        //     }
        // }
    }
}
