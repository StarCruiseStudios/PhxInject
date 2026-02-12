// -----------------------------------------------------------------------------
// <copyright file="InjectorDependencyInterfaceMetadata.cs" company="Star Cruise Studios LLC">
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

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;

/// <summary>
///     Metadata representing an analyzed injector dependency interface.
/// </summary>
/// <param name="InjectorDependencyInterfaceType"> The type metadata of the injector dependency interface. </param>
/// <param name="FactoryMethods"> The list of factory methods in the interface. </param>
/// <param name="FactoryProperties"> The list of factory properties in the interface. </param>
/// <param name="InjectorDependencyAttributeMetadata"> The [InjectorDependency] attribute metadata. </param>
/// <param name="Location"> The source location of the interface definition. </param>
internal record InjectorDependencyInterfaceMetadata(
    TypeMetadata InjectorDependencyInterfaceType,
    EquatableList<SpecFactoryMethodMetadata> FactoryMethods,
    EquatableList<SpecFactoryPropertyMetadata> FactoryProperties,
    InjectorDependencyAttributeMetadata InjectorDependencyAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
