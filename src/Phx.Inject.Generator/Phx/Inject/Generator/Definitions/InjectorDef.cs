// -----------------------------------------------------------------------------
//  <copyright file="InjectorDef.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Common;
using Phx.Inject.Generator.Model;

namespace Phx.Inject.Generator.Definitions;

internal record InjectorDef(
    TypeModel InjectorType,
    TypeModel InjectorInterfaceType,
    IEnumerable<TypeModel> Specifications,
    IEnumerable<TypeModel> ConstructedSpecifications,
    IEnumerable<TypeModel> Dependencies,
    IEnumerable<InjectorProviderDef> Providers,
    IEnumerable<ActivatorDef> Builders,
    IEnumerable<InjectorChildFactoryDef> ChildFactories,
    Location Location
) : IDefinition {
    public TypeModel SpecContainerCollectionType { get; }
        = TypeHelpers.CreateSpecContainerCollectionType(InjectorType);

    public interface IBuilder {
        InjectorDef Build(DefGenerationContext context);
    }

    public class Builder : IBuilder {
        public InjectorDef Build(DefGenerationContext context) {
            IReadOnlyList<TypeModel> constructedSpecifications = context.Injector.SpecificationsTypes
                .Where(spec => {
                    var specDesc = context.GetSpec(spec, context.Injector.Location);
                    return specDesc.InstantiationMode == SpecInstantiationMode.Instantiated;
                })
                .Where(spec => context.Injector.DependencyInterfaceTypes.Contains(spec) == false)
                .ToImmutableList();

            IReadOnlyList<InjectorProviderDef> providers = context.Injector.Providers
                .Select(provider => {
                    var factoryInvocation = context.GetSpecContainerFactoryInvocation(
                        provider.ProvidedType,
                        provider.Location);

                    return new InjectorProviderDef(
                        provider.ProvidedType,
                        provider.ProviderMethodName,
                        factoryInvocation,
                        provider.Location);
                })
                .ToImmutableList();

            IReadOnlyList<ActivatorDef> builders = context.Injector.Builders
                .Select(builder => {
                    var builderInvocation = context.GetSpecContainerBuilderInvocation(
                        context.Injector.InjectorType,
                        builder.BuiltType,
                        builder.Location);

                    return new ActivatorDef(
                        builder.BuiltType,
                        builder.BuilderMethodName,
                        builderInvocation,
                        builder.Location);
                })
                .ToImmutableList();

            IReadOnlyList<InjectorChildFactoryDef> childFactories = context.Injector.ChildFactories
                .Select(factory => new InjectorChildFactoryDef(
                    factory.ChildInjectorType,
                    factory.InjectorChildFactoryMethodName,
                    factory.Parameters,
                    factory.Location))
                .ToImmutableList();

            return new InjectorDef(
                context.Injector.InjectorType,
                context.Injector.InjectorInterfaceType,
                context.Injector.SpecificationsTypes,
                constructedSpecifications,
                context.Injector.DependencyInterfaceTypes,
                providers,
                builders,
                childFactories,
                context.Injector.Location);
        }
    }
}
