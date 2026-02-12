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
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Injector;

/// <summary>
///     Metadata representing an analyzed injector interface.
/// </summary>
/// <param name="InjectorInterfaceType"> The type metadata of the injector interface. </param>
/// <param name="Providers"> The list of provider methods in the injector. </param>
/// <param name="Activators"> The list of activator methods in the injector. </param>
/// <param name="ChildProviders"> The list of child provider methods in the injector. </param>
/// <param name="InjectorAttributeMetadata"> The [Injector] attribute metadata. </param>
/// <param name="DependencyAttributeMetadata"> The optional [Dependency] attribute metadata. </param>
/// <param name="Location"> The source location of the interface definition. </param>
internal record InjectorInterfaceMetadata(
    TypeMetadata InjectorInterfaceType,
    EquatableList<InjectorProviderMetadata> Providers,
    EquatableList<InjectorActivatorMetadata> Activators,
    EquatableList<InjectorChildProviderMetadata> ChildProviders,
    InjectorAttributeMetadata InjectorAttributeMetadata,
    DependencyAttributeMetadata? DependencyAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
