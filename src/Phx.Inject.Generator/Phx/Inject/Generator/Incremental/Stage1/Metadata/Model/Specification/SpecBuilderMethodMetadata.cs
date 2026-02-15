// -----------------------------------------------------------------------------
// <copyright file="SpecBuilderMethodMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata for a builder method declared in a specification class.
/// </summary>
/// <param name="BuilderMethodName">The name of the builder method.</param>
/// <param name="BuiltType">The qualified type that is configured (typically the first parameter type).</param>
/// <param name="Parameters">Parameters including the target instance and additional dependencies.</param>
/// <param name="BuilderAttributeMetadata">The [Builder] attribute metadata.</param>
/// <param name="Location">The source location of the method definition.</param>
/// <remarks>
///     Builder methods configure existing instances via property/field injection. Distinguished
///     from factories by void return type and first parameter being the target instance.
/// </remarks>
internal record SpecBuilderMethodMetadata(
    string BuilderMethodName,
    QualifiedTypeMetadata BuiltType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    BuilderAttributeMetadata BuilderAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
