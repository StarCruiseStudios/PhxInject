// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Injectors.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Model.Specifications.Definitions;

    // internal delegate InjectorBuilderMethodDefinition CreateInjectorBuilderMethodDefinition(
    //         InjectorBuilderDescriptor injectorBuilderDescriptor,
    //         InjectorDescriptor injectorDescriptor,
    //         IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
    // );

    internal record InjectorBuilderDefinition(
            TypeModel BuiltType,
            string InjectorBuilderMethodName,
            SpecContainerBuilderInvocationDefinition SpecContainerBuilderInvocation,
            Location Location
    ) : IDefinition {
        // public class Builder {
        //     private CreateSpecContainerBuilderInvocationDefinition createSpecContainerBuilderInvocation;
        //
        //     public Builder(CreateSpecContainerBuilderInvocationDefinition createSpecContainerBuilderInvocation) {
        //         this.createSpecContainerBuilderInvocation = createSpecContainerBuilderInvocation;
        //     }
        //
        //     public InjectorBuilderMethodDefinition Build(
        //             InjectorBuilderDescriptor builderDescriptor,
        //             InjectorDescriptor injectorDescriptor,
        //             IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
        //     ) {
        //         if (!builderRegistrations.TryGetValue(
        //                     RegistrationIdentifier.FromQualifiedTypeDescriptor(builderDescriptor.BuiltType),
        //                     out var builderRegistration)) {
        //             throw new InjectionException(
        //                     Diagnostics.IncompleteSpecification,
        //                     $"Cannot find builder for type {builderDescriptor.BuiltType} required by builder method in injector {injectorDescriptor.InjectorInterfaceType}.",
        //                     builderDescriptor.Location);
        //         }
        //
        //         var specContainerBuilderInvocation = createSpecContainerBuilderInvocation(
        //                 injectorDescriptor,
        //                 builderRegistration,
        //                 builderDescriptor.Location);
        //
        //         return new InjectorBuilderMethodDefinition(
        //                 builderDescriptor.BuiltType.TypeModel,
        //                 builderDescriptor.BuilderMethodName,
        //                 specContainerBuilderInvocation,
        //                 builderDescriptor.Location);
        //     }
        // }
    }
}
