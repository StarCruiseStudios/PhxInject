// -----------------------------------------------------------------------------
//  <copyright file="InjectorDefinition.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Injectors.Definitions {
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Phx.Inject.Generator.Common;
    using Phx.Inject.Generator.Common.Definitions;

    internal delegate InjectorDefinition CreateInjectorDefinition(DefinitionGenerationContext context);

    internal record InjectorDefinition(
            TypeModel InjectorType,
            TypeModel InjectorInterfaceType,
            IEnumerable<TypeModel> Specifications,
            IEnumerable<TypeModel> ExternalDependencies,
            IEnumerable<InjectorProviderDefinition> Providers,
            IEnumerable<InjectorBuilderDefinition> Builders,
            IEnumerable<InjectorChildFactoryDefinition> ChildFactories,
            Location Location
    ) : IDefinition {
        public TypeModel SpecContainerCollectionType { get; }
            = SymbolProcessors.GetSpecContainerCollectionType(InjectorType);

        public class Builder {
            public InjectorDefinition Build(DefinitionGenerationContext context) {
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
                                        factory.Location))
                        .ToImmutableList();

                return new InjectorDefinition(
                        context.Injector.InjectorType,
                        context.Injector.InjectorInterfaceType,
                        context.Injector.SpecificationsTypes,
                        context.Injector.ExternalDependencyInterfaceTypes,
                        providers,
                        builders,
                        childFactories,
                        context.Injector.Location);
            }
        }
    }
}
