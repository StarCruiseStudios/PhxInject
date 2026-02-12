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
///     Metadata representing an analyzed specification builder method.
///     <para>
///         Builder Methods: Configure or inject dependencies into EXISTING instances
///         rather than creating new ones. Distinguished from Factories by void return type and by
///         operating on an already-constructed instance (first parameter).
///     </para>
///     <para>
///         Use Case: Post-construction dependency injection, property injection,
///         or multi-phase initialization where the instance already exists but needs configuration.
///     </para>
/// </summary>
/// <param name="BuilderMethodName"> The name of the builder method. </param>
/// <param name="BuiltType"> The qualified type that is configured (typically the first parameter type). </param>
/// <param name="Parameters"> Parameters including the target instance and additional dependencies. </param>
/// <param name="BuilderAttributeMetadata"> The [Builder] attribute metadata. </param>
/// <param name="Location"> The source location of the method definition. </param>
internal record SpecBuilderMethodMetadata(
    string BuilderMethodName,
    QualifiedTypeMetadata BuiltType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    BuilderAttributeMetadata BuilderAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
