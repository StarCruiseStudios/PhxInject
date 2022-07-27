// -----------------------------------------------------------------------------
//  <copyright file="InjectorMapper.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Map {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Extract.Model;
    using static Construct.GenerationConstants;

    internal class InjectorMapper : IInjectorMapper {
        public InjectorDefinition MapToDefinition(
                InjectorModel injectorModel,
                IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations,
                IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
        ) {
            var injectorMethods = injectorModel.InjectionMethods.Select(
                            method => {
                                if (!factoryRegistrations.TryGetValue(
                                            method.ReturnType.ToRegistrationIdentifier(),
                                            out var factoryMethodRegistration)) {
                                    throw new InvalidOperationException(
                                            $"No Factory found for type {method.ReturnType}.");
                                }

                                var factoryMethodContainerInvocation = new FactoryMethodContainerInvocationDefinition(
                                        GetSpecificationContainerName(
                                                factoryMethodRegistration.SpecificationType.Name,
                                                injectorModel.InjectorType.Name),
                                        factoryMethodRegistration.FactoryModel.Name);
                                return new InjectorMethodDefinition(
                                        method.ReturnType.TypeModel.ToTypeDefinition(),
                                        method.Name,
                                        factoryMethodContainerInvocation);
                            })
                    .ToImmutableList();

            var injectorBuilderMethods = injectorModel.InjectionBuilderMethods.Select(
                            method => {
                                if (!builderRegistrations.TryGetValue(
                                            method.BuiltType.ToRegistrationIdentifier(),
                                            out var builderMethodRegistration)) {
                                    throw new InvalidOperationException(
                                            $"No Builder found for type {method.BuiltType}.");
                                }

                                var builderMethodContainerInvocation = new BuilderMethodContainerInvocationDefinition(
                                        GetSpecificationContainerName(
                                                builderMethodRegistration.SpecificationType.Name,
                                                injectorModel.InjectorType.Name),
                                        builderMethodRegistration.BuilderModel.Name);
                                return new InjectorBuilderMethodDefinition(
                                        method.BuiltType.TypeModel.ToTypeDefinition(),
                                        method.Name,
                                        builderMethodContainerInvocation);
                            })
                    .ToImmutableList();

            var specContainerTypes = injectorModel.Specifications.Select(
                            spec => {
                                var specContainerName = GetSpecificationContainerName(
                                        spec.Name,
                                        injectorModel.InjectorType.Name);
                                return (spec with { Name = specContainerName }).ToTypeDefinition();
                            })
                    .ToImmutableList();

            return new InjectorDefinition(
                    injectorModel.InjectorType.ToTypeDefinition(),
                    injectorModel.InjectorInterface.ToTypeDefinition(),
                    specContainerTypes,
                    injectorMethods,
                    injectorBuilderMethods);
        }
    }
}
