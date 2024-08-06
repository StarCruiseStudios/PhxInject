// -----------------------------------------------------------------------------
//  <copyright file="InjectorDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Definitions {
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Definitions;
    using Phx.Inject.Generator.Specifications;

    internal delegate InjectorDefinition CreateInjectorDefinition(DefinitionGenerationContext context);

    internal record InjectorDefinition(
        TypeModel InjectorType,
        TypeModel InjectorInterfaceType,
        IEnumerable<TypeModel> Specifications,
        IEnumerable<TypeModel> ConstructedSpecifications,
        IEnumerable<TypeModel> ExternalDependencies,
        IEnumerable<InjectorProviderDefinition> Providers,
        IEnumerable<InjectorBuilderDefinition> Builders,
        IEnumerable<InjectorChildFactoryDefinition> ChildFactories,
        Location Location
    ) : IDefinition {
        public TypeModel SpecContainerCollectionType { get; }
            = TypeHelpers.CreateSpecContainerCollectionType(InjectorType);

        public class Builder {
            public InjectorDefinition Build(DefinitionGenerationContext context) {
                var constructedSpecifications = context.Injector.SpecificationsTypes
                    .Where(spec => {
                        var specDescriptor = context.GetSpec(spec, context.Injector.Location);
                        return specDescriptor.InstantiationMode == SpecInstantiationMode.Instantiated;
                    })
                    .Where(spec => context.Injector.ExternalDependencyInterfaceTypes.Contains(spec) == false)
                    .ToImmutableList();

                var providers = context.Injector.Providers
                    .Select(
                        provider => {
                            var factoryInvocation = context.GetSpecContainerFactoryInvocation(
                                provider.ProvidedType,
                                provider.Location);

                            return new InjectorProviderDefinition(
                                provider.ProvidedType,
                                provider.ProviderMethodName,
                                factoryInvocation,
                                provider.Location);
                        })
                    .ToImmutableList();

                var builders = context.Injector.Builders
                    .Select(
                        builder => {
                            var builderInvocation = context.GetSpecContainerBuilderInvocation(
                                context.Injector.InjectorType,
                                builder.BuiltType,
                                builder.Location);

                            return new InjectorBuilderDefinition(
                                builder.BuiltType,
                                builder.BuilderMethodName,
                                builderInvocation,
                                builder.Location);
                        })
                    .ToImmutableList();

                var childFactories = context.Injector.ChildFactories
                    .Select(
                        factory => new InjectorChildFactoryDefinition(
                            factory.ChildInjectorType,
                            factory.InjectorChildFactoryMethodName,
                            factory.Parameters,
                            factory.Location))
                    .ToImmutableList();

                return new InjectorDefinition(
                    context.Injector.InjectorType,
                    context.Injector.InjectorInterfaceType,
                    context.Injector.SpecificationsTypes,
                    constructedSpecifications,
                    context.Injector.ExternalDependencyInterfaceTypes,
                    providers,
                    builders,
                    childFactories,
                    context.Injector.Location);
            }
        }
    }
}
