// -----------------------------------------------------------------------------
//  <copyright file="SpecContainerDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Model.Specifications.Definitions {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Input;
    using Phx.Inject.Generator.Model.Specifications.Descriptors;

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
                var specContainerType = SymbolProcessors.CreateSpecContainerType(
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

                            return new SpecContainerFactoryDefinition(
                                    factory.ReturnType,
                                    factory.FactoryMethodName,
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

                            return new SpecContainerBuilderDefinition(
                                    builder.BuiltType.TypeModel,
                                    builder.BuilderMethodName,
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
