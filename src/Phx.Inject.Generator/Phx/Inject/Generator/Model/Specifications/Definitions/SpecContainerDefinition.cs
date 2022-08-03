// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    // internal delegate SpecContainerDefinition CreateSpecContainerDefinition(
    //         SpecDescriptor specDescriptor,
    //         InjectorDescriptor injectorDescriptor,
    //         IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
    // );

    internal record SpecContainerDefinition(
            TypeModel ContainerType,
            TypeModel SpecificationType,
            SpecInstantiationMode SpecInstantiationMode,
            IEnumerable<SpecContainerInstanceHolderDefinition> InstanceHolderDeclarations,
            IEnumerable<SpecContainerFactoryDefinition> FactoryMethodDefinitions,
            IEnumerable<SpecContainerBuilderDefinition> BuilderMethodDefinitions,
            Location Location
    ) : IDefinition {
        // public class Builder {
        //     private readonly CreateSpecContainerType createSpecContainerType;
        //     private readonly CreateSpecReferenceDefinition createSpecReference;
        //
        //     private readonly CreateSpecContainerInstanceHolderDefinition createSpecContainerInstanceHolder;
        //     private readonly CreateSpecContainerFactoryDefinition createSpecContainerFactory;
        //     private readonly CreateSpecContainerBuilderMethodDefinition createSpecContainerBuilderMethod;
        //
        //     public Builder(
        //             CreateSpecContainerType createSpecContainerType,
        //             CreateSpecReferenceDefinition createSpecReference,
        //             CreateSpecContainerInstanceHolderDefinition createSpecContainerInstanceHolder,
        //             CreateSpecContainerFactoryDefinition createSpecContainerFactory,
        //             CreateSpecContainerBuilderMethodDefinition createSpecContainerBuilderMethod
        //     ) {
        //         this.createSpecContainerType = createSpecContainerType;
        //         this.createSpecReference = createSpecReference;
        //         this.createSpecContainerInstanceHolder = createSpecContainerInstanceHolder;
        //         this.createSpecContainerFactory = createSpecContainerFactory;
        //         this.createSpecContainerBuilderMethod = createSpecContainerBuilderMethod;
        //     }
        //
        //     public SpecContainerDefinition Build(
        //             SpecDescriptor specDescriptor,
        //             InjectorDescriptor injectorDescriptor,
        //             IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
        //     ) {
        //         var specContainerType = createSpecContainerType(
        //                 injectorDescriptor.InjectorType,
        //                 specDescriptor.SpecType);
        //         var specReference = createSpecReference(specDescriptor);
        //
        //         var instanceHolders = specDescriptor.Factories.Where(
        //                         factory => factory.FabricationMode == SpecFactoryMethodFabricationMode.Scoped)
        //                 .Select(factory => createSpecContainerInstanceHolder(factory))
        //                 .ToImmutableList();
        //
        //         var factoryMethods = specDescriptor.Factories
        //                 .Select(factory => createSpecContainerFactory(injectorDescriptor, specDescriptor, factory, factoryRegistrations))
        //                 .ToImmutableList();
        //
        //         var builderMethods = specDescriptor.Builders
        //                 .Select(
        //                         builder => createSpecContainerBuilderMethod(
        //                                 injectorDescriptor,
        //                                 specDescriptor,
        //                                 builder,
        //                                 factoryRegistrations))
        //                 .ToImmutableList();
        //
        //         return new SpecContainerDefinition(
        //                 specContainerType,
        //                 specReference,
        //                 instanceHolders,
        //                 factoryMethods,
        //                 builderMethods,
        //                 specDescriptor.Location);
        //     }
        // }
    }
}
