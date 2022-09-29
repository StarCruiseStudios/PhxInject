// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Specifications.Definitions {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Definitions;
    using Phx.Inject.Generator.Specifications.Descriptors;

    internal delegate SpecContainerDefinition CreateSpecContainerDefinition(
            SpecDescriptor specDescriptor,
            DefinitionGenerationContext context
    );

    internal record SpecContainerDefinition(
            TypeModel SpecContainerType,
            TypeModel SpecificationType,
            SpecInstantiationMode SpecInstantiationMode,
            IEnumerable<SpecContainerFactoryDefinition> FactoryMethodDefinitions,
            IEnumerable<SpecContainerBuilderDefinition> BuilderMethodDefinitions,
            Location Location
    ) : IDefinition {
        public class Builder {
            public SpecContainerDefinition Build(SpecDescriptor specDescriptor, DefinitionGenerationContext context) {
                var specContainerType = TypeHelpers.CreateSpecContainerType(
                        context.Injector.InjectorType,
                        specDescriptor.SpecType);

                var factories = specDescriptor.Factories.Select(
                        factory => {
                            var arguments = factory.Parameters.Select(
                                            parameter =>
                                                    context.GetSpecContainerFactoryInvocation(
                                                            parameter,
                                                            factory.Location))
                                    .ToImmutableList();
                            var specContainerFactoryMethodName = factory.GetSpecContainerFactoryName();

                            return new SpecContainerFactoryDefinition(
                                    factory.ReturnType,
                                    specContainerFactoryMethodName,
                                    factory.FactoryMemberName,
                                    factory.SpecFactoryMemberType,
                                    factory.FabricationMode,
                                    arguments,
                                    factory.Location);
                        });

                var builders = specDescriptor.Builders.Select(
                        builder => {
                            var arguments = builder.Parameters.Select(
                                            parameter =>
                                                    context.GetSpecContainerFactoryInvocation(
                                                            parameter,
                                                            builder.Location))
                                    .ToImmutableList();
                            var specContainerBuilderMethodName = builder.GetSpecContainerBuilderName();

                            return new SpecContainerBuilderDefinition(
                                    builder.BuiltType.TypeModel,
                                    specContainerBuilderMethodName,
                                    builder.BuilderMemberName,
                                    builder.SpecBuilderMemberType,
                                    arguments,
                                    builder.Location);
                        });

                return new SpecContainerDefinition(
                        specContainerType,
                        specDescriptor.SpecType,
                        specDescriptor.InstantiationMode,
                        factories,
                        builders,
                        specDescriptor.Location);
            }
        }
    }
}
