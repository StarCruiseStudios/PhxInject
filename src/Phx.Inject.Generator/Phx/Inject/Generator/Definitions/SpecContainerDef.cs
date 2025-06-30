// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerDef.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Common;
using Phx.Inject.Generator.Descriptors;
using Phx.Inject.Generator.Model;

namespace Phx.Inject.Generator.Definitions;

internal record SpecContainerDef(
    TypeModel SpecContainerType,
    TypeModel SpecificationType,
    SpecInstantiationMode SpecInstantiationMode,
    IEnumerable<SpecContainerFactoryDef> FactoryMethodDefs,
    IEnumerable<SpecContainerBuilderDef> BuilderMethodDefs,
    Location Location
) : IDefinition {
    public interface IBuilder {
        SpecContainerDef Build(SpecDesc specDesc, DefGenerationContext context);
    }

    public class Builder : IBuilder {
        public SpecContainerDef Build(SpecDesc specDesc, DefGenerationContext context) {
            var specContainerType = TypeHelpers.CreateSpecContainerType(
                context.Injector.InjectorType,
                specDesc.SpecType);

            IReadOnlyList<SpecContainerFactoryDef> factories = specDesc.Factories.Select(factory => {
                    IReadOnlyList<SpecContainerFactoryInvocationDef> arguments = factory.Parameters.Select(parameter =>
                            context.GetSpecContainerFactoryInvocation(
                                parameter,
                                factory.Location))
                        .ToImmutableList();
                    IReadOnlyList<SpecContainerFactoryRequiredPropertyDef> requiredProperties = factory
                        .RequiredProperties
                        .Select(property =>
                            new SpecContainerFactoryRequiredPropertyDef(
                                property.PropertyName,
                                context.GetSpecContainerFactoryInvocation(
                                    property.PropertyType,
                                    factory.Location),
                                factory.Location))
                        .ToImmutableList();
                    var specContainerFactoryMethodName = factory.GetSpecContainerFactoryName();

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

            IReadOnlyList<SpecContainerBuilderDef> builders = specDesc.Builders.Select(builder => {
                    IReadOnlyList<SpecContainerFactoryInvocationDef> arguments = builder.Parameters.Select(parameter =>
                            context.GetSpecContainerFactoryInvocation(
                                parameter,
                                builder.Location))
                        .ToImmutableList();
                    var specContainerBuilderMethodName = builder.GetSpecContainerBuilderName();

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
                specDesc.SpecType,
                specDesc.InstantiationMode,
                factories,
                builders,
                specDesc.Location);
        }
    }
}
