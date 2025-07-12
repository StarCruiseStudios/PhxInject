// -----------------------------------------------------------------------------
//  <copyright file="DefGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Descriptors;
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Generator.Map;

internal record DefGenerationContext : IGeneratorContext {
    public InjectorMetadata Injector { get; }
    public IReadOnlyDictionary<TypeModel, InjectorMetadata> Injectors { get; }
    public IReadOnlyDictionary<TypeModel, SpecDesc> Specifications { get; }
    public IReadOnlyDictionary<TypeModel, DependencyMetadata> Dependencies { get; }
    public IReadOnlyDictionary<RegistrationIdentifier, List<FactoryRegistration>> FactoryRegistrations { get; set; }
    public IReadOnlyDictionary<RegistrationIdentifier, BuilderRegistration> BuilderRegistrations { get; set; }

    public DefGenerationContext(
        InjectorMetadata injector,
        IReadOnlyDictionary<TypeModel, InjectorMetadata> injectors,
        IReadOnlyDictionary<TypeModel, SpecDesc> specifications,
        IReadOnlyDictionary<TypeModel, DependencyMetadata> dependencies,
        IReadOnlyDictionary<RegistrationIdentifier, List<FactoryRegistration>> factoryRegistrations,
        IReadOnlyDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations,
        IGeneratorContext parentContext
    ) {
        Description = null;
        Injector = injector;
        Injectors = injectors;
        Specifications = specifications;
        Dependencies = dependencies;
        FactoryRegistrations = factoryRegistrations;
        BuilderRegistrations = builderRegistrations;
        Symbol = null;
        Aggregator = parentContext.Aggregator;
        ParentContext = parentContext;
        ExecutionContext = parentContext.ExecutionContext;
        ContextDepth = parentContext.ContextDepth + 1;
    }
    public string? Description { get; }
    public ISymbol? Symbol { get; private init; }
    public IExceptionAggregator Aggregator { get; set; }
    public IGeneratorContext? ParentContext { get; }
    public GeneratorExecutionContext ExecutionContext { get; }
    public int ContextDepth { get; }

    public InjectorMetadata GetInjector(TypeModel type, Location location) {
        if (Injectors.TryGetValue(type, out var injector)) {
            return injector;
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find required injector type {type} while generating injection for type {Injector.InjectorInterfaceType}.",
            location,
            this);
    }

    public SpecDesc GetSpec(TypeModel type, Location location) {
        if (Specifications.TryGetValue(type, out var spec)) {
            return spec;
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find required specification type {type} while generating injection for type {Injector.InjectorInterfaceType}.",
            location,
            this);
    }

    public DependencyMetadata GetDependency(TypeModel type, Location location) {
        if (Dependencies.TryGetValue(type, out var dep)) {
            return dep;
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find required dependency type {type} while generating injection for type {Injector.InjectorInterfaceType}.",
            location,
            this);
    }

    public SpecContainerFactoryInvocationDef GetSpecContainerFactoryInvocation(
        QualifiedTypeModel returnedType,
        Location location
    ) {
        if (FactoryRegistrations.Count == 0) {
            throw Diagnostics.InternalError.AsFatalException(
                $"Cannot search factory for type {returnedType} before factory registrations are created  while generating injection for type {Injector.InjectorInterfaceType}.",
                location,
                this);
        }

        TypeModel? runtimeFactoryProvidedType = null;
        var factoryType = returnedType;
        if (returnedType.TypeModel.NamespacedBaseTypeName == TypeNames.FactoryClassName) {
            factoryType = returnedType with {
                TypeModel = returnedType.TypeModel.TypeArguments.Single()
            };
            runtimeFactoryProvidedType = factoryType.TypeModel;
        }

        var key = RegistrationIdentifier.FromQualifiedTypeModel(factoryType);
        if (FactoryRegistrations.TryGetValue(key, out var factoryRegistration)) {
            IReadOnlyList<SpecContainerFactorySingleInvocationDef> singleInvocationDefs = factoryRegistration
                .Select(reg => {
                    var specContainerType = TypeHelpers.CreateSpecContainerType(
                        Injector.InjectorType,
                        reg.Specification.SpecType);
                    return new SpecContainerFactorySingleInvocationDef(
                        specContainerType,
                        reg.FactoryDesc.GetSpecContainerFactoryName(this),
                        reg.FactoryDesc.Location
                    );
                })
                .ToImmutableList();

            return new SpecContainerFactoryInvocationDef(
                singleInvocationDefs,
                factoryType,
                runtimeFactoryProvidedType,
                location);
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find factory for type {factoryType} while generating injection for type {Injector.InjectorInterfaceType}.",
            location,
            this);
    }

    public SpecContainerBuilderInvocationDef GetSpecContainerBuilderInvocation(
        TypeModel injectorType,
        QualifiedTypeModel builtType,
        Location location
    ) {
        if (BuilderRegistrations.Count == 0) {
            throw Diagnostics.InternalError.AsFatalException(
                $"Cannot search for builder for type {builtType} before builder registrations are created  while generating injection for type {Injector.InjectorInterfaceType}.",
                location,
                this);
        }

        var key = RegistrationIdentifier.FromQualifiedTypeModel(builtType);
        if (BuilderRegistrations.TryGetValue(key, out var builderRegistration)) {
            var specContainerType = TypeHelpers.CreateSpecContainerType(
                injectorType,
                builderRegistration.Specification.SpecType);
            return new SpecContainerBuilderInvocationDef(
                specContainerType,
                builderRegistration.BuilderDesc.GetSpecContainerBuilderName(this),
                builderRegistration.BuilderDesc.Location);
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find builder for type {builtType} while generating injection for type {Injector.InjectorInterfaceType}.",
            location,
            this);
    }

    public DefGenerationContext GetChildContext(ISymbol? symbol) {
        return new DefGenerationContext(
            Injector,
            Injectors,
            Specifications,
            Dependencies,
            FactoryRegistrations,
            BuilderRegistrations,
            this
        ) {
            Symbol = symbol
        };
    }
}
