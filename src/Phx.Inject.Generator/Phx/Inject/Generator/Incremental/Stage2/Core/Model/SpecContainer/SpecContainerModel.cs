// -----------------------------------------------------------------------------
// <copyright file="SpecContainerModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;

/// <summary>
///     Model representing a specification container for code generation.
/// </summary>
/// <param name="SpecContainerType"> The specification container implementation type. </param>
/// <param name="SpecificationType"> The specification type contained. </param>
/// <param name="SpecInstantiationMode"> The instantiation mode for the specification. </param>
/// <param name="FactoryMethodDefs"> The factory method definitions in this container. </param>
/// <param name="BuilderMethodDefs"> The builder method definitions in this container. </param>
/// <param name="Location"> The source location where this container is defined. </param>
internal record SpecContainerModel(
    TypeMetadata SpecContainerType,
    TypeMetadata SpecificationType,
    SpecInstantiationMode SpecInstantiationMode,
    IEnumerable<SpecContainerFactoryModel> FactoryMethodDefs,
    IEnumerable<SpecContainerBuilderModel> BuilderMethodDefs,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;

/// <summary>
///     Specifies how a specification is instantiated.
/// </summary>
internal enum SpecInstantiationMode {
    /// <summary> The specification is static. </summary>
    Static = 0,
    /// <summary> The specification is instantiated by the injector. </summary>
    Instantiated = 1,
    /// <summary> The specification is provided by a dependency injector. </summary>
    Dependency = 2
}
