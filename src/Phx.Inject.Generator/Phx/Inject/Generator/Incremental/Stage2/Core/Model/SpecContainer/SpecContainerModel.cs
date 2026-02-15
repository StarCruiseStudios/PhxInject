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
///     Code generation model for a specification container that aggregates factory and builder methods.
/// </summary>
/// <param name="SpecContainerType">The specification container implementation type.</param>
/// <param name="SpecificationType">The specification type contained.</param>
/// <param name="SpecInstantiationMode">How the specification is instantiated (Static, Instantiated, or Dependency).</param>
/// <param name="FactoryMethodDefs">Factory method definitions in this container.</param>
/// <param name="BuilderMethodDefs">Builder method definitions in this container.</param>
/// <param name="Location">The source location where this container is defined.</param>
/// <remarks>
///     Consolidates all factory and builder methods from a specification into a single generated
///     container class. The injector delegates to containers instead of duplicating factory logic.
/// </remarks>
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
    Static = 0,
    Instantiated = 1,
    Dependency = 2
}
