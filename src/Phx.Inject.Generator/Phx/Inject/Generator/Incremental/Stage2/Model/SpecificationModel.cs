// -----------------------------------------------------------------------------
// <copyright file="SpecificationModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;

namespace Phx.Inject.Generator.Incremental.Stage2.Model;

/// <summary>
/// Unified domain model representing a specification.
/// Can be derived from: SpecClass, SpecInterface, InjectorDependency, AutoFactory, or AutoBuilder.
/// </summary>
internal record SpecificationModel(
    TypeMetadata SpecType,
    SpecInstantiationMode InstantiationMode,
    IEnumerable<FactoryModel> Factories,
    IEnumerable<BuilderModel> Builders,
    IEnumerable<LinkModel> Links
);

/// <summary>
/// Specification instantiation mode.
/// </summary>
internal enum SpecInstantiationMode {
    /// <summary>Static specification (static class).</summary>
    Static,
    /// <summary>Instantiated specification (interface or non-static class).</summary>
    Instantiated,
    /// <summary>Dependency specification (injector dependency interface).</summary>
    Dependency
}
