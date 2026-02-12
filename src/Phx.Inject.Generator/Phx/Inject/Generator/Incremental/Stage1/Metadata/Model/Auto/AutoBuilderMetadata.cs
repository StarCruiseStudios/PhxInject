// -----------------------------------------------------------------------------
// <copyright file="AutoBuilderMetadata.cs" company="Star Cruise Studios LLC">
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

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Auto;

/// <summary>
///     Metadata representing an analyzed auto-generated builder method.
///     <para>
///         <strong>Auto-Builder Convention:</strong> Static methods annotated with [Builder] attribute
///         on a class are automatically discovered and incorporated into the dependency graph. The method's
///         first parameter typically matches the containing class type, representing the instance to configure.
///     </para>
///     <para>
///         <strong>Discovery Pattern:</strong> The generator scans classes for static void methods with
///         [Builder] attributes, extracting configuration logic that can be composed with factory creation.
///         This enables separation of construction from configuration.
///     </para>
/// </summary>
/// <param name="AutoBuilderMethodName"> The name of the static builder method discovered. </param>
/// <param name="BuiltType"> The type being configured (typically the first parameter type). </param>
/// <param name="Parameters"> All parameters including the target instance and dependencies. </param>
/// <param name="AutoBuilderAttributeMetadata"> The [AutoBuilder] attribute metadata. </param>
/// <param name="Location"> The source location of the static builder method. </param>
internal record AutoBuilderMetadata(
    string AutoBuilderMethodName,
    QualifiedTypeMetadata BuiltType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    AutoBuilderAttributeMetadata AutoBuilderAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }