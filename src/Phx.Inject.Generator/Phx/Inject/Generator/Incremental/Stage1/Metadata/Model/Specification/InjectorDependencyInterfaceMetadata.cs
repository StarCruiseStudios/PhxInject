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
///     <para>
///         <strong>InjectorDependency Pattern:</strong> Enables composition by defining the contract for
///         dependencies that a child injector requires from its parent. The parent provides implementations
///         via this interface, creating a clear boundary between injector hierarchies.
///     </para>
///     <para>
///         <strong>Constraints:</strong> Can ONLY contain Factories (no Builders or References).
///         All factories must be parameterless, as they represent the parent's ability to provide
///         dependencies on-demand without requiring context from the child.
///     </para>
/// </summary>
/// <param name="InjectorDependencyInterfaceType"> The dependency contract interface type. </param>
/// <param name="FactoryMethods"> Parameterless factory methods defining required parent dependencies. </param>
/// <param name="FactoryProperties"> Parameterless factory properties for parent-provided dependencies. </param>
/// <param name="InjectorDependencyAttributeMetadata"> The [InjectorDependency] attribute metadata. </param>
/// <param name="Location"> The source location of the interface definition. </param>
internal record InjectorDependencyInterfaceMetadata(
    TypeMetadata InjectorDependencyInterfaceType,
    EquatableList<SpecFactoryMethodMetadata> FactoryMethods,
    EquatableList<SpecFactoryPropertyMetadata> FactoryProperties,
    InjectorDependencyAttributeMetadata InjectorDependencyAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
