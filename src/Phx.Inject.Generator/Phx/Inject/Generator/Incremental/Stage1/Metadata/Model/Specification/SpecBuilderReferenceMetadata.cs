// -----------------------------------------------------------------------------
// <copyright file="SpecBuilderReferenceMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata representing an analyzed specification builder reference.
///     <para>
///         Builder References: Wrap existing builder methods (e.g., injection methods,
///         configuration functions) as Action delegates, enabling reuse of configuration logic defined
///         outside the specification.
///     </para>
///     <para>
///         Key Distinction: References delegate to existing void methods for configuring
///         instances. Example: <c>[BuilderReference] Action&lt;MyClass, int&gt; Configure = MyClass.Inject;</c>
///     </para>
/// </summary>
/// <param name="BuilderReferenceName"> The name of the builder reference property. </param>
/// <param name="BuiltType"> The qualified type that is configured by the referenced builder. </param>
/// <param name="Parameters"> Parameters including the target instance and configuration dependencies. </param>
/// <param name="BuilderReferenceAttributeMetadata"> The [BuilderReference] attribute metadata. </param>
/// <param name="Location"> The source location of the reference definition. </param>
internal record SpecBuilderReferenceMetadata(
    string BuilderReferenceName,
    QualifiedTypeMetadata BuiltType,
    EquatableList<QualifiedTypeMetadata> Parameters,
    BuilderReferenceAttributeMetadata BuilderReferenceAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
