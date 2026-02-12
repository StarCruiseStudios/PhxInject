// -----------------------------------------------------------------------------
// <copyright file="SpecContainerBuilderMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Types;
using Phx.Inject.Generator.Incremental.Stage2.Core.Model.SpecContainer;
using Phx.Inject.Generator.Incremental.Util;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage2.Core.Pipeline.SpecContainer;

/// <summary>
///     Mapper for transforming specification builder metadata into spec container builder models.
/// </summary>
internal static class SpecContainerBuilderMapper {
    /// <summary> Gets the singleton mapper instance. </summary>
    public static readonly SpecContainerBuilderMapperInstance Instance = new();

    /// <summary>
    ///     Maps specification builder method metadata to a spec container builder model.
    /// </summary>
    /// <param name="metadata"> The builder method metadata to map. </param>
    /// <returns> The mapped spec container builder model. </returns>
    public static SpecContainerBuilderModel Map(SpecBuilderMethodMetadata metadata) {
        return Instance.Map(metadata);
    }

    /// <summary>
    ///     Maps specification builder reference metadata to a spec container builder model.
    /// </summary>
    /// <param name="metadata"> The builder reference metadata to map. </param>
    /// <returns> The mapped spec container builder model. </returns>
    public static SpecContainerBuilderModel Map(SpecBuilderReferenceMetadata metadata) {
        return Instance.Map(metadata);
    }
}

/// <summary>
///     Instance mapper for transforming specification builder metadata into spec container builder models.
/// </summary>
internal class SpecContainerBuilderMapperInstance {
    private static SpecContainerFactoryInvocationModel PlaceholderArgument(QualifiedTypeMetadata parameterType) {
        return new SpecContainerFactoryInvocationModel(
            EquatableList<SpecContainerFactorySingleInvocationModel>.Empty,
            parameterType,
            null,
            parameterType.Location
        );
    }

    /// <summary>
    ///     Maps specification builder method metadata to a spec container builder model.
    /// </summary>
    /// <param name="metadata"> The builder method metadata to map. </param>
    /// <returns> The mapped spec container builder model. </returns>
    public SpecContainerBuilderModel Map(SpecBuilderMethodMetadata metadata) {
        var arguments = metadata.Parameters
            .Select(PlaceholderArgument)
            .ToList();
        return new SpecContainerBuilderModel(
            metadata.BuiltType.TypeMetadata,
            "Bld_" + metadata.BuilderMethodName,
            metadata.BuilderMethodName,
            SpecBuilderMemberType.Method,
            arguments,
            metadata.Location
        );
    }

    /// <summary>
    ///     Maps specification builder reference metadata to a spec container builder model.
    /// </summary>
    /// <param name="metadata"> The builder reference metadata to map. </param>
    /// <returns> The mapped spec container builder model. </returns>
    public SpecContainerBuilderModel Map(SpecBuilderReferenceMetadata metadata) {
        var arguments = metadata.Parameters
            .Select(PlaceholderArgument)
            .ToList();
        return new SpecContainerBuilderModel(
            metadata.BuiltType.TypeMetadata,
            "RefBld_" + metadata.BuilderReferenceName,
            metadata.BuilderReferenceName,
            SpecBuilderMemberType.Reference,
            arguments,
            metadata.Location
        );
    }
}
