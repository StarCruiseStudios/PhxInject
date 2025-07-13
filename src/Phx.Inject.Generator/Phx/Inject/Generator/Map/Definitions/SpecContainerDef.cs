// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerDef.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata;

namespace Phx.Inject.Generator.Map.Definitions;

internal record SpecContainerDef(
    TypeModel SpecContainerType,
    TypeModel SpecificationType,
    SpecInstantiationMode SpecInstantiationMode,
    IEnumerable<SpecContainerFactoryDef> FactoryMethodDefs,
    IEnumerable<SpecContainerBuilderDef> BuilderMethodDefs,
    Location Location
) : IDefinition {
    public interface IMapper {
        SpecContainerDef Map(SpecMetadata specMetadata, DefGenerationContext defGenerationCtx);
    }

    public class Mapper : IMapper {
        public SpecContainerDef Map(SpecMetadata specMetadata, DefGenerationContext defGenerationCtx) {
            var specContainerType = TypeHelpers.CreateSpecContainerType(
                defGenerationCtx.Injector.InjectorType,
                specMetadata.SpecType);

            IReadOnlyList<SpecContainerFactoryDef> factories = specMetadata.Factories.Select(factory => {
                    IReadOnlyList<SpecContainerFactoryInvocationDef> arguments = factory.Parameters.Select(parameter =>
                            defGenerationCtx.GetSpecContainerFactoryInvocation(
                                parameter,
                                factory.Location))
                        .ToImmutableList();
                    IReadOnlyList<SpecContainerFactoryRequiredPropertyDef> requiredProperties = factory
                        .RequiredProperties
                        .Select(property =>
                            new SpecContainerFactoryRequiredPropertyDef(
                                property.PropertyName,
                                defGenerationCtx.GetSpecContainerFactoryInvocation(
                                    property.PropertyType,
                                    factory.Location),
                                factory.Location))
                        .ToImmutableList();
                    var specContainerFactoryMethodName = factory.GetSpecContainerFactoryName(defGenerationCtx);

                    return new SpecContainerFactoryDef(
                        factory.ReturnType,
                        specContainerFactoryMethodName,
                        factory.FactoryMemberName,
                        factory.SpecFactoryMemberType,
                        factory.FabricationMode,
                        arguments,
                        requiredProperties,
                        factory.Location);
                })
                .ToImmutableList();

            IReadOnlyList<SpecContainerBuilderDef> builders = specMetadata.Builders.Select(builder => {
                    IReadOnlyList<SpecContainerFactoryInvocationDef> arguments = builder.Parameters.Select(parameter =>
                            defGenerationCtx.GetSpecContainerFactoryInvocation(
                                parameter,
                                builder.Location))
                        .ToImmutableList();
                    var specContainerBuilderMethodName = builder.GetSpecContainerBuilderName(defGenerationCtx);

                    return new SpecContainerBuilderDef(
                        builder.BuiltType.TypeModel,
                        specContainerBuilderMethodName,
                        builder.BuilderMemberName,
                        builder.SpecBuilderMemberType,
                        arguments,
                        builder.Location);
                })
                .ToImmutableList();

            return new SpecContainerDef(
                specContainerType,
                specMetadata.SpecType,
                specMetadata.InstantiationMode,
                factories,
                builders,
                specMetadata.Location);
        }
    }
}
