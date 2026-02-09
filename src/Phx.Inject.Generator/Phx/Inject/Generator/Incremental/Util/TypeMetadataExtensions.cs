// -----------------------------------------------------------------------------
// <copyright file="TypeMetadataExtensions.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;

namespace Phx.Inject.Generator.Incremental.Util;

internal static class TypeMetadataExtensions {
    private const string GeneratedInjectorClassPrefix = "Generated";

    /// <summary>
    ///     Creates the generated injector type from the interface type and optional generated class name.
    /// </summary>
    public static TypeMetadata CreateInjectorType(
        this TypeMetadata injectorInterfaceType,
        string? generatedClassName
    ) {
        var baseName = string.IsNullOrEmpty(generatedClassName)
            ? $"{GeneratedInjectorClassPrefix}{injectorInterfaceType.BaseTypeName}"
            : generatedClassName!;
        return new TypeMetadata(
            injectorInterfaceType.NamespaceName,
            baseName,
            ImmutableList<TypeMetadata>.Empty,
            injectorInterfaceType.Location
        );
    }

    /// <summary>
    ///     Creates the spec container type for a given injector and spec (same convention as legacy TypeHelpers.CreateSpecContainerType).
    /// </summary>
    public static TypeMetadata CreateSpecContainerType(
        this TypeMetadata injectorType,
        TypeMetadata specType
    ) {
        var combinedBaseName = $"{injectorType.BaseTypeName}_{specType.BaseTypeName}";
        return specType with {
            BaseTypeName = combinedBaseName,
            TypeArguments = ImmutableList<TypeMetadata>.Empty
        };
    }
}
