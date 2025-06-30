// -----------------------------------------------------------------------------
//  <copyright file="TemplateGenerationContext.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Common;
using Phx.Inject.Generator.Definitions;
using Phx.Inject.Generator.Model;

namespace Phx.Inject.Generator.Templates;

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

        throw new InjectionException(
            Diagnostics.IncompleteSpecification,
            $"Cannot find required injector type {type}.",
            location);
    }

    public SpecContainerDef GetSpecContainer(TypeModel type, Location location) {
        if (SpecContainers.TryGetValue(type, out var spec)) {
            return spec;
        }

        throw new InjectionException(
            Diagnostics.IncompleteSpecification,
            $"Cannot find required specification container type {type}.",
            location);
    }

    public DependencyImplementationDef GetDependency(TypeModel type, Location location) {
        if (DependencyImplementations.TryGetValue(type, out var dep)) {
            return dep;
        }

        throw new InjectionException(
            Diagnostics.IncompleteSpecification,
            $"Cannot find required dependency type {type}.",
            location);
    }
}
