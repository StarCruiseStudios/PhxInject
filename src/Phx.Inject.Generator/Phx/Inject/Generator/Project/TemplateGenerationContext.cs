// -----------------------------------------------------------------------------
//  <copyright file="TemplateGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Generator.Project;

internal record TemplateGenerationContext : IGeneratorContext {
    public InjectorDef Injector { get; }
    public IReadOnlyDictionary<TypeModel, InjectorDef> Injectors { get; }
    public IReadOnlyDictionary<TypeModel, SpecContainerDef> SpecContainers { get; }
    public IReadOnlyDictionary<TypeModel, DependencyImplementationDef> DependencyImplementations { get; }

    public TemplateGenerationContext(
        InjectorDef injector,
        IReadOnlyDictionary<TypeModel, InjectorDef> injectors,
        IReadOnlyDictionary<TypeModel, SpecContainerDef> specContainers,
        IReadOnlyDictionary<TypeModel, DependencyImplementationDef> dependencyImplementations,
        ISymbol? symbol,
        IGeneratorContext parentContext
    ) {
        Description = null;
        Symbol = symbol;
        Injector = injector;
        Injectors = injectors;
        SpecContainers = specContainers;
        DependencyImplementations = dependencyImplementations;
        Aggregator = parentContext.Aggregator;
        ParentContext = parentContext;
        ExecutionContext = parentContext.ExecutionContext;
        ContextDepth = parentContext.ContextDepth + 1;
    }
    public string? Description { get; }
    public ISymbol? Symbol { get; }
    public IExceptionAggregator Aggregator { get; set; }
    public IGeneratorContext? ParentContext { get; }
    public GeneratorExecutionContext ExecutionContext { get; }
    public int ContextDepth { get; }

    public InjectorDef GetInjector(TypeModel type, Location location) {
        if (Injectors.TryGetValue(type, out var injector)) {
            return injector;
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find required injector type {type}.",
            location,
            this);
    }

    public SpecContainerDef GetSpecContainer(TypeModel type, Location location) {
        if (SpecContainers.TryGetValue(type, out var spec)) {
            return spec;
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find required specification container type {type}.",
            location,
            this);
    }

    public DependencyImplementationDef GetDependency(TypeModel type, Location location) {
        if (DependencyImplementations.TryGetValue(type, out var dep)) {
            return dep;
        }

        throw Diagnostics.IncompleteSpecification.AsException(
            $"Cannot find required dependency type {type}.",
            location,
            this);
    }
}
