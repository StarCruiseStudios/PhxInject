// -----------------------------------------------------------------------------
//  <copyright file="DefGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Definitions {
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Descriptors;
    using Phx.Inject.Generator.Model;

    internal record DefGenerationContext(
        InjectorDesc Injector,
        IReadOnlyDictionary<TypeModel, InjectorDesc> Injectors,
        IReadOnlyDictionary<TypeModel, SpecDesc> Specifications,
        IReadOnlyDictionary<TypeModel, DependencyDesc> Dependencies,
        IReadOnlyDictionary<RegistrationIdentifier, List<FactoryRegistration>> FactoryRegistrations,
        IReadOnlyDictionary<RegistrationIdentifier, BuilderRegistration> BuilderRegistrations,
        GeneratorExecutionContext GenerationContext
    ) {
        public InjectorDesc GetInjector(TypeModel type, Location location) {
            if (Injectors.TryGetValue(type, out var injector)) {
                return injector;
            }

            throw new InjectionException(
                Diagnostics.IncompleteSpecification,
                $"Cannot find required injector type {type}"
                + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                location);
        }

        public SpecDesc GetSpec(TypeModel type, Location location) {
            if (Specifications.TryGetValue(type, out var spec)) {
                return spec;
            }

            throw new InjectionException(
                Diagnostics.IncompleteSpecification,
                $"Cannot find required specification type {type}"
                + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                location);
        }

        public DependencyDesc GetDependency(TypeModel type, Location location) {
            if (Dependencies.TryGetValue(type, out var dep)) {
                return dep;
            }

            throw new InjectionException(
                Diagnostics.IncompleteSpecification,
                $"Cannot find required dependency type {type}"
                + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                location);
        }

        public SpecContainerFactoryInvocationDef GetSpecContainerFactoryInvocation(
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

            var key = RegistrationIdentifier.FromQualifiedTypeModel(factoryType);
            if (FactoryRegistrations.TryGetValue(key, out var factoryRegistration)) {
                var singleInvocationDefs = factoryRegistration.Select(reg => {
                    var specContainerType = TypeHelpers.CreateSpecContainerType(
                        Injector.InjectorType,
                        reg.Specification.SpecType);
                    return new SpecContainerFactorySingleInvocationDef(
                        specContainerType,
                        reg.FactoryDesc.GetSpecContainerFactoryName(),
                        reg.FactoryDesc.Location
                    );
                }).ToList();

                return new SpecContainerFactoryInvocationDef(
                    singleInvocationDefs,
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

        public SpecContainerBuilderInvocationDef GetSpecContainerBuilderInvocation(
            TypeModel injectorType,
            QualifiedTypeModel builtType,
            Location location
        ) {
            if (BuilderRegistrations.Count == 0) {
                throw new InjectionException(
                    Diagnostics.InternalError,
                    $"Cannot search for builder for type {builtType} before builder registrations are created "
                    + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                    location);
            }

            var key = RegistrationIdentifier.FromQualifiedTypeModel(builtType);
            if (BuilderRegistrations.TryGetValue(key, out var builderRegistration)) {
                var specContainerType = TypeHelpers.CreateSpecContainerType(
                    injectorType,
                    builderRegistration.Specification.SpecType);
                return new SpecContainerBuilderInvocationDef(
                    specContainerType,
                    builderRegistration.BuilderDesc.GetSpecContainerBuilderName(),
                    builderRegistration.BuilderDesc.Location);
            }

            throw new InjectionException(
                Diagnostics.IncompleteSpecification,
                $"Cannot find builder for type {builtType}"
                + $" while generating injection for type {Injector.InjectorInterfaceType}.",
                location);
        }
    }
}
