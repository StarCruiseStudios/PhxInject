// -----------------------------------------------------------------------------
//  <copyright file="DefinitionGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Common.Definitions {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.External.Descriptors;
    using Phx.Inject.Generator.Injectors.Descriptors;
    using Phx.Inject.Generator.Specifications.Definitions;
    using Phx.Inject.Generator.Specifications.Descriptors;

    internal record DefinitionGenerationContext(
            InjectorDescriptor Injector,
            IReadOnlyDictionary<TypeModel, InjectorDescriptor> Injectors,
            IReadOnlyDictionary<TypeModel, SpecDescriptor> Specifications,
            IReadOnlyDictionary<TypeModel, ExternalDependencyDescriptor> ExternalDependencies,
            IReadOnlyDictionary<RegistrationIdentifier, List<FactoryRegistration>> FactoryRegistrations,
            IReadOnlyDictionary<RegistrationIdentifier, BuilderRegistration> BuilderRegistrations,
            GeneratorExecutionContext GenerationContext
    ) {
        public InjectorDescriptor GetInjector(TypeModel type, Location location) {
            if (Injectors.TryGetValue(type, out var injector)) {
                return injector;
            }

            throw new InjectionException(
                    Diagnostics.IncompleteSpecification,
                    $"Cannot find required injector type {type}"
                    + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                    location);
        }

        public SpecDescriptor GetSpec(TypeModel type, Location location) {
            if (Specifications.TryGetValue(type, out var spec)) {
                return spec;
            }

            throw new InjectionException(
                    Diagnostics.IncompleteSpecification,
                    $"Cannot find required specification type {type}"
                    + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                    location);
        }

        public ExternalDependencyDescriptor GetExternalDependency(TypeModel type, Location location) {
            if (ExternalDependencies.TryGetValue(type, out var dep)) {
                return dep;
            }

            throw new InjectionException(
                    Diagnostics.IncompleteSpecification,
                    $"Cannot find required external dependency type {type}"
                    + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                    location);
        }

        public SpecContainerFactoryInvocationDefinition GetSpecContainerFactoryInvocation(
                QualifiedTypeModel returnedType,
                Location location
        ) {
            if (FactoryRegistrations.Count == 0) {
                throw new InjectionException(
                        Diagnostics.InternalError,
                        $"Cannot search factory for type {returnedType} before factory registrations are created "
                        + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                        location);
            }

            TypeModel? runtimeFactoryProvidedType = null;
            var factoryType = returnedType;
            if (returnedType.TypeModel.QualifiedBaseTypeName == TypeHelpers.FactoryTypeName) {
                factoryType = returnedType with {
                    TypeModel = returnedType.TypeModel.TypeArguments.Single()
                };
                runtimeFactoryProvidedType = factoryType.TypeModel;
            }

            var key = RegistrationIdentifier.FromQualifiedTypeDescriptor(factoryType);
            if (FactoryRegistrations.TryGetValue(key, out var factoryRegistration)) {
                var singleInvocationDefinitions = factoryRegistration.Select(reg => {
                    var specContainerType = TypeHelpers.CreateSpecContainerType(
                        Injector.InjectorType,
                        reg.Specification.SpecType);
                    return new SpecContainerFactorySingleInvocationDefinition(
                        specContainerType,
                        reg.FactoryDescriptor.GetSpecContainerFactoryName(),
                        reg.FactoryDescriptor.Location
                    );
                }).ToList();

                return new SpecContainerFactoryInvocationDefinition(
                    singleInvocationDefinitions,
                    factoryType,
                    runtimeFactoryProvidedType,
                    location);
            }

            throw new InjectionException(
                    Diagnostics.IncompleteSpecification,
                    $"Cannot find factory for type {factoryType}"
                    + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                    location);
        }

        public SpecContainerBuilderInvocationDefinition GetSpecContainerBuilderInvocation(
                TypeModel injectorType,
                QualifiedTypeModel builtType,
                Location location
        ) {
            if (BuilderRegistrations.Count == 0) {
                throw new InjectionException(
                        Diagnostics.InternalError,
                        $"Cannot search builder for type {builtType} before builder registrations are created "
                        + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                        location);
            }

            var key = RegistrationIdentifier.FromQualifiedTypeDescriptor(builtType);
            if (BuilderRegistrations.TryGetValue(key, out var builderRegistration)) {
                var specContainerType = TypeHelpers.CreateSpecContainerType(
                        injectorType,
                        builderRegistration.Specification.SpecType);
                return new SpecContainerBuilderInvocationDefinition(
                        specContainerType,
                        builderRegistration.BuilderDescriptor.GetSpecContainerBuilderName(),
                        builderRegistration.BuilderDescriptor.Location);
            }

            throw new InjectionException(
                    Diagnostics.IncompleteSpecification,
                    $"Cannot find builder for type {builtType}"
                    + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                    location);
        }
    }
}
