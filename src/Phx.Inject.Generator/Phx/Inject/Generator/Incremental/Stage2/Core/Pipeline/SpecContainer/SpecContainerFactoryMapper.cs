// -----------------------------------------------------------------------------
// <copyright file="SpecContainerFactoryMapper.cs" company="Star Cruise Studios LLC">
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

internal static class SpecContainerFactoryMapper {
    public static readonly SpecContainerFactoryMapperInstance Instance = new();

    public static SpecContainerFactoryModel Map(SpecFactoryMethodMetadata metadata) {
        return Instance.Map(metadata);
    }

    public static SpecContainerFactoryModel Map(SpecFactoryPropertyMetadata metadata) {
        return Instance.Map(metadata);
    }

    public static SpecContainerFactoryModel Map(SpecFactoryReferenceMetadata metadata) {
        return Instance.Map(metadata);
    }

    public static SpecContainerFactoryModel Map(InjectorDependencyFactoryMethodMetadata metadata) {
        return Instance.Map(metadata);
    }

    public static SpecContainerFactoryModel Map(InjectorDependencyFactoryPropertyMetadata metadata) {
        return Instance.Map(metadata);
    }
}

internal class SpecContainerFactoryMapperInstance {
    private static SpecContainerFactoryInvocationModel PlaceholderArgument(
        QualifiedTypeMetadata parameterType
    ) {
        return new SpecContainerFactoryInvocationModel(
            EquatableList<SpecContainerFactorySingleInvocationModel>.Empty,
            parameterType,
            null,
            parameterType.Location
        );
    }

    public SpecContainerFactoryModel Map(SpecFactoryMethodMetadata metadata) {
        var arguments = metadata.Parameters
            .Select(PlaceholderArgument)
            .ToList();
        return new SpecContainerFactoryModel(
            metadata.FactoryReturnType,
            "Fac_" + metadata.FactoryMethodName,
            metadata.FactoryMethodName,
            SpecFactoryMemberType.Method,
            metadata.FactoryAttributeMetadata.FabricationMode,
            arguments,
            [],
            metadata.Location
        );
    }

    public SpecContainerFactoryModel Map(SpecFactoryPropertyMetadata metadata) {
        return new SpecContainerFactoryModel(
            metadata.FactoryReturnType,
            "PropFac_" + metadata.FactoryPropertyName,
            metadata.FactoryPropertyName,
            SpecFactoryMemberType.Property,
            metadata.FactoryAttributeMetadata.FabricationMode,
            [],
            [],
            metadata.Location
        );
    }

    public SpecContainerFactoryModel Map(SpecFactoryReferenceMetadata metadata) {
        var arguments = metadata.Parameters
            .Select(PlaceholderArgument)
            .ToList();
        return new SpecContainerFactoryModel(
            metadata.FactoryReturnType,
            "RefFac_" + metadata.FactoryReferenceName,
            metadata.FactoryReferenceName,
            SpecFactoryMemberType.Reference,
            metadata.FactoryReferenceAttributeMetadata.FabricationMode,
            arguments,
            [],
            metadata.Location
        );
    }

    public SpecContainerFactoryModel Map(InjectorDependencyFactoryMethodMetadata metadata) {
        return new SpecContainerFactoryModel(
            metadata.FactoryReturnType,
            "Fac_" + metadata.FactoryMethodName,
            metadata.FactoryMethodName,
            SpecFactoryMemberType.Method,
            metadata.FactoryAttributeMetadata.FabricationMode,
            [],
            [],
            metadata.Location
        );
    }

    public SpecContainerFactoryModel Map(InjectorDependencyFactoryPropertyMetadata metadata) {
        return new SpecContainerFactoryModel(
            metadata.FactoryReturnType,
            "PropFac_" + metadata.FactoryPropertyName,
            metadata.FactoryPropertyName,
            SpecFactoryMemberType.Property,
            metadata.FactoryAttributeMetadata.FabricationMode,
            [],
            [],
            metadata.Location
        );
    }
}
