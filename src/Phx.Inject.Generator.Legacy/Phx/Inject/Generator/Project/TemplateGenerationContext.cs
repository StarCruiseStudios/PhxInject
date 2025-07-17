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

internal record TemplateGenerationContext(
    InjectorDef Injector,
    IReadOnlyDictionary<TypeModel, InjectorDef> Injectors,
    IReadOnlyDictionary<TypeModel, SpecContainerDef> SpecContainers,
    IReadOnlyDictionary<TypeModel, DependencyImplementationDef> DependencyImplementations,
    ISymbol Symbol,
    IGeneratorContext ParentContext
) : IGeneratorContext {
    public GeneratorSettings GeneratorSettings { get; } = ParentContext.GeneratorSettings;
    public string Description { get; } = "template generation";
    public IExceptionAggregator Aggregator { get; set; } = ParentContext.Aggregator;
    public GeneratorExecutionContext ExecutionContext { get; } = ParentContext.ExecutionContext;
    public int ContextDepth { get; } = ParentContext.ContextDepth + 1;

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
