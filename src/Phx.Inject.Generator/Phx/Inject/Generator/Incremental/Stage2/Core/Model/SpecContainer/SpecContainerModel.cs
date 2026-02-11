// -----------------------------------------------------------------------------
// <copyright file="SpecContainerModel.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Util;

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;

internal record SpecContainerModel(
    TypeMetadata SpecContainerType,
    TypeMetadata SpecificationType,
    SpecInstantiationMode SpecInstantiationMode,
    IEnumerable<SpecContainerFactoryModel> FactoryMethodDefs,
    IEnumerable<SpecContainerBuilderModel> BuilderMethodDefs,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;

internal enum SpecInstantiationMode {
    Static = 0,
    Instantiated = 1,
    Dependency = 2
}
