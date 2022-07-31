// -----------------------------------------------------------------------------
//  <copyright file="InjectorBuilderMethodDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Model.Descriptors;

    internal delegate InjectorBuilderMethodDefinition CreateInjectorBuilderMethodDefinition(
            InjectorBuilderDescriptor injectorBuilderDescriptor,
            InjectorDescriptor injectorDescriptor,
            IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
    );

    internal record InjectorBuilderMethodDefinition(
            TypeModel BuiltType,
            string InjectorMethodName,
            SpecContainerBuilderInvocationDefinition SpecContainerBuilderInvocation,
            Location Location
    ) : IDefinition {
        public class Builder {
            private CreateSpecContainerBuilderInvocationDefinition createSpecContainerBuilderInvocation;

            public Builder(CreateSpecContainerBuilderInvocationDefinition createSpecContainerBuilderInvocation) {
                this.createSpecContainerBuilderInvocation = createSpecContainerBuilderInvocation;
            }

            public InjectorBuilderMethodDefinition Build(
                    InjectorBuilderDescriptor builderDescriptor,
                    InjectorDescriptor injectorDescriptor,
                    IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
            ) {
                if (!builderRegistrations.TryGetValue(
                            RegistrationIdentifier.FromQualifiedTypeDescriptor(builderDescriptor.BuiltType),
                            out var builderRegistration)) {
                    throw new InjectionException(
                            Diagnostics.IncompleteSpecification,
                            $"Cannot find builder for type {builderDescriptor.BuiltType} required by builder method in injector {injectorDescriptor.InjectorInterfaceType}.",
                            builderDescriptor.Location);
                }

                var specContainerBuilderInvocation = createSpecContainerBuilderInvocation(
                        injectorDescriptor,
                        builderRegistration,
                        builderDescriptor.Location);

                return new InjectorBuilderMethodDefinition(
                        builderDescriptor.BuiltType.TypeModel,
                        builderDescriptor.BuilderMethodName,
                        specContainerBuilderInvocation,
                        builderDescriptor.Location);
            }
        }
    }
}
