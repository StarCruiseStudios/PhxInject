// -----------------------------------------------------------------------------
// <copyright file="InjectorDef.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata;

namespace Phx.Inject.Generator.Map.Definitions;

internal record InjectorDef(
    TypeModel InjectorType,
    TypeModel InjectorInterfaceType,
    IEnumerable<TypeModel> Specifications,
    IEnumerable<TypeModel> ConstructedSpecifications,
    TypeModel? Dependency,
    IEnumerable<InjectorProviderDef> Providers,
    IEnumerable<InjectorBuilderDef> Builders,
    IEnumerable<InjectorChildFactoryDef> ChildFactories,
    Location Location
) : IDefinition {
    public TypeModel SpecContainerCollectionType { get; }
        = TypeHelpers.CreateSpecContainerCollectionType(InjectorType);

    public interface IMapper {
        InjectorDef Map(
            InjectorMetadata injector,
            InjectorRegistrations injectorRegistrations,
            IReadOnlyDictionary<TypeModel, SpecMetadata> specifications,
            DefGenerationContext currentCtx);
    }

    public class Mapper : IMapper {
        public InjectorDef Map(
            InjectorMetadata injector,
            InjectorRegistrations injectorRegistrations,
            IReadOnlyDictionary<TypeModel, SpecMetadata> specifications,
            DefGenerationContext currentCtx) {
            IReadOnlyList<TypeModel> constructedSpecifications = injector.SpecificationsTypes
                .Where(spec => {
                    var specMetadata = GetSpec(
                        injector,
                        specifications,
                        spec,
                        injector.Location,
                        currentCtx);
                    return specMetadata.InstantiationMode
                        is SpecInstantiationMode.Instantiated
                        or SpecInstantiationMode.Dependency;
                })
                .Where(spec => injector.DependencyInterfaceType != spec)
                .ToImmutableList();

            IReadOnlyList<InjectorProviderDef> providers = injector.Providers
                .Select(provider => {
                    var factoryInvocation = TypeHelpers.GetSpecContainerFactoryInvocation(
                        injector,
                        injectorRegistrations,
                        provider.ProvidedType,
                        provider.Location,
                        currentCtx);

                    return new InjectorProviderDef(
                        provider.ProvidedType,
                        provider.ProviderMethodName,
                        factoryInvocation,
                        provider.Location);
                })
                .ToImmutableList();

            IReadOnlyList<InjectorBuilderDef> builders = injector.Builders
                .Select(builder => {
                    var builderInvocation = GetSpecContainerBuilderInvocation(
                        injector,
                        injectorRegistrations,
                        injector.InjectorType,
                        builder.BuiltType,
                        builder.Location,
                        currentCtx);

                    return new InjectorBuilderDef(
                        builder.BuiltType,
                        builder.BuilderMethodName,
                        builderInvocation,
                        builder.Location);
                })
                .ToImmutableList();

            IReadOnlyList<InjectorChildFactoryDef> childFactories = injector.ChildFactories
                .Select(factory => new InjectorChildFactoryDef(
                    factory.ChildInjectorType,
                    factory.InjectorChildFactoryMethodName,
                    factory.Parameters,
                    factory.Location))
                .ToImmutableList();

            return new InjectorDef(
                injector.InjectorType,
                injector.InjectorInterfaceType,
                injector.SpecificationsTypes,
                constructedSpecifications,
                injector.DependencyInterfaceType,
                providers,
                builders,
                childFactories,
                injector.Location);
        }

        public SpecContainerBuilderInvocationDef GetSpecContainerBuilderInvocation(
            InjectorMetadata Injector,
            InjectorRegistrations injectorRegistrations,
            TypeModel injectorType,
            QualifiedTypeModel builtType,
            Location location,
            IGeneratorContext currentCtx
        ) {
            if (injectorRegistrations.BuilderRegistrations.Count == 0) {
                throw Diagnostics.InternalError.AsFatalException(
                    $"Cannot search for builder for type {builtType} before builder registrations are created  while generating injection for type {Injector.InjectorInterfaceType}.",
                    location,
                    currentCtx);
            }

            var key = RegistrationIdentifier.FromQualifiedTypeModel(builtType);
            if (injectorRegistrations.BuilderRegistrations.TryGetValue(key, out var builderRegistration)) {
                var specContainerType = TypeHelpers.CreateSpecContainerType(
                    injectorType,
                    builderRegistration.Specification.SpecType);
                return new SpecContainerBuilderInvocationDef(
                    specContainerType,
                    builderRegistration.BuilderMetadata.GetSpecContainerBuilderName(currentCtx),
                    builderRegistration.BuilderMetadata.Location);
            }

            throw Diagnostics.IncompleteSpecification.AsException(
                $"Cannot find builder for type {builtType} while generating injection for type {Injector.InjectorInterfaceType}.",
                location,
                currentCtx);
        }

        public SpecMetadata GetSpec(
            InjectorMetadata Injector,
            IReadOnlyDictionary<TypeModel, SpecMetadata> specifications,
            TypeModel type,
            Location location,
            IGeneratorContext currentCtx
        ) {
            if (specifications.TryGetValue(type, out var spec)) {
                return spec;
            }

            throw Diagnostics.IncompleteSpecification.AsException(
                $"Cannot find required specification type {type} while generating injection for type {Injector.InjectorInterfaceType}.",
                location,
                currentCtx);
        }
    }
}
