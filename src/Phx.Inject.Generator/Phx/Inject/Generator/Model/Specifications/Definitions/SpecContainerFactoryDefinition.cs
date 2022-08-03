// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerFactoryDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    // internal delegate SpecContainerFactoryDefinition CreateSpecContainerFactoryDefinition(
    //         InjectorDescriptor injectorDescriptor,
    //         SpecDescriptor specDescriptor,
    //         SpecFactoryDescriptor specFactoryDescriptor,
    //         IDefinitionGenerationContext context
    // );

    internal record SpecContainerFactoryDefinition(
            TypeModel ReturnType,
            string FactoryMethodName,
            TypeModel SpecContainerType,
            TypeModel SpecContainerCollectionType,
            SpecContainerInstanceHolderDefinition InstanceHolder,
            IEnumerable<SpecContainerFactoryInvocationDefinition> Arguments,
            Location Location
    ) : IDefinition {
        // public class Builder {
        //     private readonly CreateSpecReferenceDefinition createSpecReference;
        //     private readonly CreateSpecContainerType createSpecContainerType;
        //     private readonly CreateSpecContainerCollectionType createSpecContainerCollectionType;
        //     private readonly CreateSpecContainerInstanceHolderDefinition createInstanceHolder;
        //     private readonly CreateSpecContainerFactoryInvocationDefinition createSpecContainerFactoryInvocation;
        //
        //     public Builder(
        //             CreateSpecReferenceDefinition createSpecReference,
        //             CreateSpecContainerType createSpecContainerType,
        //             CreateSpecContainerCollectionType createSpecContainerCollectionType,
        //             CreateSpecContainerInstanceHolderDefinition createInstanceHolder,
        //             CreateSpecContainerFactoryInvocationDefinition createSpecContainerFactoryInvocation
        //     ) {
        //         this.createSpecReference = createSpecReference;
        //         this.createSpecContainerType = createSpecContainerType;
        //         this.createSpecContainerCollectionType = createSpecContainerCollectionType;
        //         this.createInstanceHolder = createInstanceHolder;
        //         this.createSpecContainerFactoryInvocation = createSpecContainerFactoryInvocation;
        //     }
        //
        //     public SpecContainerFactoryDefinition Build(
        //             SpecFactoryDescriptor specFactoryDescriptor,
        //             SpecDescriptor specDescriptor,
        //             IDefinitionGenerationContext context
        //             // InjectorDescriptor injectorDescriptor,
        //     ) {
        //         var specReference = createSpecReference(specDescriptor, context);
        //         var instanceHolder = createInstanceHolder(specFactoryDescriptor, context);
        //
        //         var arguments = specFactoryDescriptor.Parameters.Select(
        //                 argumentType => {
        //                     if (!context.FactoryRegistrations.TryGetValue(
        //                                 RegistrationIdentifier.FromQualifiedTypeDescriptor(argumentType),
        //                                 out var factoryRegistration)) {
        //                         throw new InjectionException(
        //                                 Diagnostics.IncompleteSpecification,
        //                                 $"Cannot find factory for type {argumentType} required by factory method "
        //                                 + $"{specFactoryDescriptor.FactoryMethodName} in specification {specDescriptor.SpecType} "
        //                                 + $"in injector type {context.InjectorType}.",
        //                                 specFactoryDescriptor.Location);
        //                     }
        //
        //                     return createSpecContainerFactoryInvocation(
        //                             injectorDescriptor,
        //                             factoryRegistration,
        //                             argumentType.Location);
        //                 }).ToImmutableList();
        //
        //         return new SpecContainerFactoryDefinition(
        //                 ReturnType: specFactoryDescriptor.ReturnType.TypeModel,
        //                 specFactoryDescriptor.FactoryMethodName,
        //                 SpecContainerCollectionType: context.SpecContainerCollectionType,
        //                 specReference,
        //                 instanceHolder,
        //                 arguments,
        //                 specFactoryDescriptor.Location
        //         );
        //     }
        // }
    }
}
