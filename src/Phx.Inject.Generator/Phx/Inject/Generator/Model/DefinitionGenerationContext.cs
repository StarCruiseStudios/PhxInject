// -----------------------------------------------------------------------------
//  <copyright file="DefinitionGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Controller;
    using Phx.Inject.Generator.Input;
    using Phx.Inject.Generator.Model.External.Descriptors;
    using Phx.Inject.Generator.Model.Injectors.Descriptors;
    using Phx.Inject.Generator.Model.Specifications.Definitions;
    using Phx.Inject.Generator.Model.Specifications.Descriptors;

    internal record DefinitionGenerationContext(
            InjectorDescriptor Injector,
            IReadOnlyDictionary<TypeModel, InjectorDescriptor> Injectors,
            IReadOnlyDictionary<TypeModel, SpecDescriptor> Specifications,
            IReadOnlyDictionary<TypeModel, ExternalDependencyDescriptor> ExternalDependencies,
            IReadOnlyDictionary<RegistrationIdentifier, FactoryRegistration> FactoryRegistrations,
            IReadOnlyDictionary<RegistrationIdentifier, BuilderRegistration> BuilderRegistrations,
            GeneratorExecutionContext GenerationContext
    ) {
        public InjectorDescriptor GetInjector(TypeModel type, Location location) {
            if (Injectors.TryGetValue(type, out var injector)) {
                return injector;
            }

            throw new InjectionException(
                        Diagnostics.IncompleteSpecification,
                        $"Cannot find required injector type {type}.",
                        location);

        }

        public SpecDescriptor GetSpec(TypeModel type, Location location) {
            if (Specifications.TryGetValue(type, out var spec)) {
                return spec;
            }

            throw new InjectionException(
                    Diagnostics.IncompleteSpecification,
                    $"Cannot find required specification type {type}.",
                    location);
        }

        public ExternalDependencyDescriptor GetExternalDependency(TypeModel type, Location location) {
            if (ExternalDependencies.TryGetValue(type, out var dep)) {
                return dep;
            }

            throw new InjectionException(
                    Diagnostics.IncompleteSpecification,
                    $"Cannot find required external dependency type {type}.",
                    location);
        }

        public SpecContainerFactoryInvocationDefinition GetSpecContainerFactoryInvocation(
                QualifiedTypeModel returnedType,
                Location location
        ) {
            var key = RegistrationIdentifier.FromQualifiedTypeDescriptor(returnedType);
            if (FactoryRegistrations.TryGetValue(key, out var factoryRegistration)) {
                var specContainerType = SymbolProcessors.CreateSpecContainerType(
                        Injector.InjectorType,
                        factoryRegistration.Specification.SpecType);
                return new SpecContainerFactoryInvocationDefinition(
                        specContainerType,
                        factoryRegistration.FactoryDescriptor.FactoryMethodName,
                        factoryRegistration.FactoryDescriptor.Location);
            }

            throw new InjectionException(
                    Diagnostics.IncompleteSpecification,
                    $"Cannot find factory for type {returnedType}.",
                    location);
        }

        public SpecContainerBuilderInvocationDefinition GetSpecContainerBuilderInvocation(
                TypeModel injectorType,
                QualifiedTypeModel builtType,
                Location location
        ) {
            var key = RegistrationIdentifier.FromQualifiedTypeDescriptor(builtType);
            if (BuilderRegistrations.TryGetValue(key, out var builderRegistration)) {
                var specContainerType = SymbolProcessors.CreateSpecContainerType(
                        injectorType,
                        builderRegistration.Specification.SpecType);
                return new SpecContainerBuilderInvocationDefinition(
                        specContainerType,
                        builderRegistration.BuilderDescriptor.BuilderMethodName,
                        builderRegistration.BuilderDescriptor.Location);
            }

            throw new InjectionException(
                    Diagnostics.IncompleteSpecification,
                    $"Cannot find builder for type {builtType}.",
                    location);
        }
    }
}
