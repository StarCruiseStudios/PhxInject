// -----------------------------------------------------------------------------
// <copyright file="SpecClassMetadata.cs" company="Star Cruise Studios LLC">
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
///     Metadata for a user-declared specification class.
///     See <see cref="Phx.Inject.SpecificationAttribute"/>.
/// </summary>
/// <param name="SpecType">The type metadata of the specification class.</param>
/// <param name="FactoryMethods">Factory methods that create and return new instances.</param>
/// <param name="FactoryProperties">Factory properties that expose instance creation via getters.</param>
/// <param name="FactoryReferences">Wrapped factory methods exposed as Func delegates.</param>
/// <param name="BuilderMethods">Builder methods that configure existing instances (void return).</param>
/// <param name="BuilderReferences">Wrapped builder methods exposed as Action delegates.</param>
/// <param name="Links">Link attributes that connect this specification to injectors.</param>
/// <param name="SpecAttributeMetadata">The [Specification] attribute metadata.</param>
/// <param name="Location">The source location of the class definition.</param>
/// <remarks>
///     Specifications define HOW dependencies are constructed. Factories create instances,
///     Builders configure existing instances, References wrap methods as delegates.
/// </remarks>
internal record SpecClassMetadata(
    TypeMetadata SpecType,
    EquatableList<SpecFactoryMethodMetadata> FactoryMethods,
    EquatableList<SpecFactoryPropertyMetadata> FactoryProperties,
    EquatableList<SpecFactoryReferenceMetadata> FactoryReferences,
    EquatableList<SpecBuilderMethodMetadata> BuilderMethods,
    EquatableList<SpecBuilderReferenceMetadata> BuilderReferences,
    EquatableList<LinkAttributeMetadata> Links,
    SpecificationAttributeMetadata SpecAttributeMetadata,
    GeneratorIgnored<LocationInfo?> Location
) : ISourceCodeElement { }
