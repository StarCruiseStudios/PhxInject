// -----------------------------------------------------------------------------
// <copyright file="InjectorInterfaceMetadata.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Validators;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;

/// <summary>
///     Metadata for a user-declared injector interface.
///     See <see cref="Phx.Inject.InjectorAttribute"/>.
/// </summary>
/// <param name="InjectorInterfaceType">The type metadata of the injector interface.</param>
/// <param name="Providers">Provider methods that return constructed objects (factory pattern).</param>
/// <param name="Activators">Activator methods that initialize existing objects (builder pattern).</param>
/// <param name="ChildProviders">Child provider methods that create scoped sub-containers (hierarchy pattern).</param>
/// <param name="InjectorAttributeMetadata">The [Injector] attribute metadata containing specification types.</param>
/// <param name="DependencyAttributeMetadata">Optional [Dependency] attribute. If present, this is a child injector.</param>
/// <param name="Location">The source location of the interface definition.</param>
/// <remarks>
///     <para>
///     Injector interfaces define the API for accessing dependencies. The generator creates
///     concrete classes implementing these interfaces. Three method types: Providers (return instances),
///     Activators (initialize objects), ChildProviders (create scoped sub-containers).
///     </para>
///     <para>
///     If <see cref="DependencyAttributeMetadata"/> is present, this is a child injector receiving
///     a parent injector reference in its constructor, enabling hierarchical scopes.
///     </para>
/// </remarks>
internal record InjectorInterfaceMetadata(
    TypeMetadata InjectorInterfaceType,
    EquatableList<InjectorProviderMetadata> Providers,
    EquatableList<InjectorActivatorMetadata> Activators,
    EquatableList<InjectorChildProviderMetadata> ChildProviders,
    InjectorAttributeMetadata InjectorAttributeMetadata,
    DependencyAttributeMetadata? DependencyAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement {
    /// <summary>
    ///     Gets the validator that defines the structural requirements for injector interface types.
    /// </summary>
    public static readonly ICodeElementValidator ElementValidator =
        InterfaceElementValidator.PublicInterface;
}
