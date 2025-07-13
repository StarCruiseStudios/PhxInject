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
using Phx.Inject.Generator.Extract.Metadata;
using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Generator.Map;

internal record DefGenerationContext(
    InjectorMetadata Injector,
    IReadOnlyDictionary<TypeModel, InjectorMetadata> Injectors,
    IReadOnlyDictionary<TypeModel, SpecMetadata> Specifications,
    IReadOnlyDictionary<TypeModel, DependencyMetadata> Dependencies,
    IReadOnlyDictionary<RegistrationIdentifier, List<FactoryRegistration>> FactoryRegistrations,
    IReadOnlyDictionary<RegistrationIdentifier, BuilderRegistration> BuilderRegistrations,
    IGeneratorContext ParentContext
) : IGeneratorContext {
    public GeneratorSettings GeneratorSettings { get; } = ParentContext.GeneratorSettings;
    public string Description { get; } = "def generation";
    public ISymbol Symbol { get; private init; } = ParentContext.ExecutionContext.Compilation.Assembly;
    public IExceptionAggregator Aggregator { get; set; } = ParentContext.Aggregator;
    public GeneratorExecutionContext ExecutionContext { get; } = ParentContext.ExecutionContext;
    public int ContextDepth { get; } = ParentContext.ContextDepth + 1;

    public InjectorMetadata GetInjector(TypeModel type, Location location) {
        if (Injectors.TryGetValue(type, out var injector)) {
            return injector;
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find required injector type {type} while generating injection for type {Injector.InjectorInterfaceType}.",
            location,
            this);
    }

    public SpecMetadata GetSpec(TypeModel type, Location location) {
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
                        reg.FactoryMetadata.GetSpecContainerFactoryName(this),
                        reg.FactoryMetadata.Location
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
                builderRegistration.BuilderMetadata.GetSpecContainerBuilderName(this),
                builderRegistration.BuilderMetadata.Location);
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find builder for type {builtType} while generating injection for type {Injector.InjectorInterfaceType}.",
            location,
            this);
    }

    public DefGenerationContext GetChildContext(ISymbol symbol) {
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
