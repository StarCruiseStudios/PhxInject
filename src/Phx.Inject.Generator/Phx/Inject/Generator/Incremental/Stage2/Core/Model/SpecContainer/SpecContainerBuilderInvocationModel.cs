// -----------------------------------------------------------------------------
// <copyright file="SpecContainerBuilderInvocationModel.cs" company="Star Cruise Studios LLC">
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
///     Model representing an invocation of a specification container builder method.
/// </summary>
/// <param name="SpecContainerType"> The specification container type. </param>
/// <param name="BuilderMethodName"> The name of the builder method to invoke. </param>
/// <param name="Location"> The source location where this invocation is defined. </param>
internal record SpecContainerBuilderInvocationModel(
    TypeMetadata SpecContainerType,
    string BuilderMethodName,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement;
