// -----------------------------------------------------------------------------
// <copyright file="InjectorModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;

namespace Phx.Inject.Generator.Incremental.Stage2.Model;

/// <summary>
/// Domain model representing an injector.
/// Combines data from InjectorInterface, InjectorAttribute, DependencyAttribute, and related specifications.
/// </summary>
internal record InjectorModel(
    TypeMetadata InjectorInterfaceType,
    TypeMetadata InjectorImplementationType,
    IEnumerable<TypeMetadata> SpecificationTypes,
    IEnumerable<TypeMetadata> ConstructedSpecificationTypes,
    TypeMetadata? DependencyInterfaceType,
    IEnumerable<ProviderModel> Providers,
    IEnumerable<ActivatorModel> Activators,
    IEnumerable<ChildInjectorModel> ChildInjectors
);
