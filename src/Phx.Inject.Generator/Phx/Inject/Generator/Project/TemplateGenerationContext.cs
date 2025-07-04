// -----------------------------------------------------------------------------
//  <copyright file="TemplateGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Map.Definitions;

namespace Phx.Inject.Generator.Project;

internal record TemplateGenerationContext(
    InjectorDef Injector,
    IReadOnlyDictionary<TypeModel, InjectorDef> Injectors,
    IReadOnlyDictionary<TypeModel, SpecContainerDef> SpecContainers,
    IReadOnlyDictionary<TypeModel, DependencyImplementationDef>
        DependencyImplementations,
    GeneratorExecutionContext GenerationContext
) {
    public InjectorDef GetInjector(TypeModel type, Location location) {
        if (Injectors.TryGetValue(type, out var injector)) {
            return injector;
        }

        throw new InjectionException($"Cannot find required injector type {type}.",
            Diagnostics.IncompleteSpecification,
            location,
            GenerationContext);
    }

    public SpecContainerDef GetSpecContainer(TypeModel type, Location location) {
        if (SpecContainers.TryGetValue(type, out var spec)) {
            return spec;
        }

        throw new InjectionException($"Cannot find required specification container type {type}.",
            Diagnostics.IncompleteSpecification,
            location,
            GenerationContext);
    }

    public DependencyImplementationDef GetDependency(TypeModel type, Location location) {
        if (DependencyImplementations.TryGetValue(type, out var dep)) {
            return dep;
        }

        throw new InjectionException($"Cannot find required dependency type {type}.",
            Diagnostics.IncompleteSpecification,
            location,
            GenerationContext);
    }
}
