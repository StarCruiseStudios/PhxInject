// -----------------------------------------------------------------------------
// <copyright file="SpecContainerBuilderMapper.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Specification;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Types;
using Phx.Inject.Generator.Incremental.Stage2.Model.SpecContainer;

namespace Phx.Inject.Generator.Incremental.Stage2.Pipeline.SpecContainer;

internal static class SpecContainerBuilderMapper {
    public static readonly SpecContainerBuilderMapperInstance Instance = new();

    public static SpecContainerBuilderModel Map(SpecBuilderMethodMetadata metadata) {
        return Instance.Map(metadata);
    }

    public static SpecContainerBuilderModel Map(SpecBuilderReferenceMetadata metadata) {
        return Instance.Map(metadata);
    }
}

internal class SpecContainerBuilderMapperInstance {
    private static SpecContainerFactoryInvocationModel PlaceholderArgument(QualifiedTypeMetadata parameterType) {
        return new SpecContainerFactoryInvocationModel(
            ImmutableList<SpecContainerFactorySingleInvocationModel>.Empty,
            parameterType,
            null,
            parameterType.Location
        );
    }

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
