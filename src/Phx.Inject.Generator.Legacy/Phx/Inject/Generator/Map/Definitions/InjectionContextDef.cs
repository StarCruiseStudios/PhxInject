// -----------------------------------------------------------------------------
// <copyright file="InjectionContextDef.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Metadata;

namespace Phx.Inject.Generator.Map.Definitions;

internal record InjectionContextDef(
    InjectorDef Injector,
    IEnumerable<SpecContainerDef> SpecContainers,
    IEnumerable<DependencyImplementationDef> DependencyImplementations,
    Location Location
) : IDefinition {
    public interface IMapper {
        InjectionContextDef Map(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, InjectorMetadata> injectors,
            IReadOnlyDictionary<TypeModel, SpecMetadata> specifications,
            IReadOnlyDictionary<TypeModel, DependencyMetadata> dependencies,
            DefGenerationContext parentCtx
        );
    }

    public class Mapper : IMapper {
        private readonly DependencyImplementationDef.IMapper dependencyImplementationDefMapper;
        private readonly InjectorDef.IMapper injectorDefMapper;
        private readonly SpecContainerDef.IMapper specContainerDefMapper;

        public Mapper(
            InjectorDef.IMapper injectorDefMapper,
            SpecContainerDef.IMapper specContainerDefMapper,
            DependencyImplementationDef.IMapper dependencyImplementationDefMapper
        ) {
            this.injectorDefMapper = injectorDefMapper;
            this.specContainerDefMapper = specContainerDefMapper;
            this.dependencyImplementationDefMapper = dependencyImplementationDefMapper;
        }

        public Mapper() : this(
            new InjectorDef.Mapper(),
            new SpecContainerDef.Mapper(),
            new DependencyImplementationDef.Mapper()) { }

        public InjectionContextDef Map(
            InjectorMetadata injectorMetadata,
            IReadOnlyDictionary<TypeModel, InjectorMetadata> injectors,
            IReadOnlyDictionary<TypeModel, SpecMetadata> specifications,
            IReadOnlyDictionary<TypeModel, DependencyMetadata> dependencies,
            DefGenerationContext parentCtx
        ) {
            var factoryRegistrations = new Dictionary<RegistrationIdentifier, List<FactoryRegistration>>();
            var builderRegistrations = new Dictionary<RegistrationIdentifier, List<BuilderRegistration>>();

            IReadOnlyList<SpecMetadata> specMetadata = specifications.Values.ToImmutableList();

            // Create a registration for all of the spec metadata factory and builder methods.
            foreach (var spec in specMetadata) {
                var currentCtx = parentCtx.GetChildContext(spec.SpecType.TypeSymbol);
                foreach (var factory in spec.Factories) {
                    var key = RegistrationIdentifier.FromQualifiedTypeModel(factory.ReturnType);
                    if (!factoryRegistrations.TryGetValue(key, out var registrationList)) {
                        registrationList = new List<FactoryRegistration>();
                        factoryRegistrations.Add(key, registrationList);
                    }

                    registrationList.Add(new FactoryRegistration(spec, factory));
                }

                foreach (var builder in spec.Builders) {
                    var key = RegistrationIdentifier.FromQualifiedTypeModel(builder.BuiltType);
                    if (!builderRegistrations.TryGetValue(key, out var registrationList)) {
                        registrationList = new List<BuilderRegistration>();
                        builderRegistrations.Add(key, registrationList);
                    }

                    registrationList.Add(new BuilderRegistration(spec, builder));
                }
            }

            // Create a registration for all of the spec metadata links. This must be done after all factory methods
            // have been registered to ensure that the link is valid.
            foreach (var spec in specMetadata) {
                foreach (var link in spec.Links) {
                    if (factoryRegistrations.TryGetValue(
                        RegistrationIdentifier.FromQualifiedTypeModel(link.InputType),
                        out var targetRegistration)) {
                        factoryRegistrations.Add(
                            RegistrationIdentifier.FromQualifiedTypeModel(link.ReturnType),
                            targetRegistration);
                    } else {
                        throw Diagnostics.IncompleteSpecification.AsException(
                            $"Cannot find factory for type {link.InputType} required by link in specification {spec.SpecType}.",
                            link.Location,
                            parentCtx);
                    }
                }
            }

            foreach (var element in factoryRegistrations) {
                var factoryRegistrationIdentifier = element.Key;
                var factoryRegistration = element.Value;
                if (factoryRegistration.Count > 1 && !factoryRegistration.All(it => it.FactoryMetadata.isPartial)) {
                    parentCtx.Aggregator.AggregateMany<FactoryRegistration, FactoryRegistration>(
                        factoryRegistration,
                        registration => $"registering factory {registration.FactoryMetadata.ReturnType}",
                        registration => {
                            factoryRegistrations.Remove(factoryRegistrationIdentifier);
                            throw Diagnostics.InvalidSpecification.AsException(
                                $"Factory for type {registration.FactoryMetadata.ReturnType} must be unique or all factories must be partial.",
                                registration.FactoryMetadata.Location,
                                parentCtx);
                        });
                }
            }

            foreach (var element in builderRegistrations) {
                var builderRegistrationIdentifier = element.Key;
                var builderRegistration = element.Value;
                if (builderRegistration.Count > 1) {
                    parentCtx.Aggregator.AggregateMany<BuilderRegistration, BuilderRegistration>(
                        builderRegistration,
                        registration => $"registering builder {registration.BuilderMetadata.BuiltType}",
                        registration => {
                            builderRegistrations.Remove(builderRegistrationIdentifier);
                            throw Diagnostics.InvalidSpecification.AsException(
                                $"Builder for type {registration.BuilderMetadata.BuiltType} must be unique.",
                                registration.BuilderMetadata.Location,
                                parentCtx);
                        });
                }
            }

            var injectionRegistrations = new InjectorRegistrations(
                factoryRegistrations,
                builderRegistrations
                    .Select(it =>
                        new KeyValuePair<RegistrationIdentifier, BuilderRegistration>(it.Key, it.Value.Single()))
                    .ToImmutableDictionary()
            );

            var injectorDef =
                injectorDefMapper.Map(injectorMetadata, injectionRegistrations, specifications, parentCtx);

            IReadOnlyList<SpecContainerDef> specContainerDefs = specMetadata
                .Select(specMetadata =>
                    specContainerDefMapper.Map(injectorMetadata, specMetadata, injectionRegistrations, parentCtx))
                .ToImmutableList();

            IReadOnlyList<DependencyImplementationDef> dependencyImplementationDefs = injectorMetadata.ChildFactories
                .Select(childFactory => GetInjector(
                    injectorMetadata,
                    injectors,
                    childFactory.ChildInjectorType,
                    childFactory.Location,
                    parentCtx))
                .Select(childInjector => childInjector.DependencyInterfaceType)
                .OfType<TypeModel>()
                .GroupBy(dependencyType => dependencyType)
                .Select(dependencyTypeGroup => dependencyTypeGroup.First())
                .Select(dependencyType => GetDependency(
                    injectorMetadata,
                    dependencies,
                    dependencyType,
                    injectorMetadata.Location,
                    parentCtx))
                .Select(dependency => dependencyImplementationDefMapper.Map(
                    injectorMetadata,
                    injectionRegistrations,
                    dependency,
                    parentCtx))
                .ToImmutableList();

            return new InjectionContextDef(
                injectorDef,
                specContainerDefs,
                dependencyImplementationDefs,
                injectorMetadata.Location);
        }

        public InjectorMetadata GetInjector(
            InjectorMetadata injector,
            IReadOnlyDictionary<TypeModel, InjectorMetadata> injectors,
            TypeModel type,
            Location location,
            IGeneratorContext currentCtx) {
            if (injectors.TryGetValue(type, out var injectorMetadata)) {
                return injectorMetadata;
            }

            throw Diagnostics.IncompleteSpecification.AsException(
                $"Cannot find required injector type {type} while generating injection for type {injector.InjectorInterfaceType}.",
                location,
                currentCtx);
        }

        public DependencyMetadata GetDependency(
            InjectorMetadata Injector,
            IReadOnlyDictionary<TypeModel, DependencyMetadata> Dependencies,
            TypeModel type,
            Location location,
            IGeneratorContext currentCtx) {
            if (Dependencies.TryGetValue(type, out var dep)) {
                return dep;
            }

            throw Diagnostics.IncompleteSpecification.AsException(
                $"Cannot find required dependency type {type} while generating injection for type {Injector.InjectorInterfaceType}.",
                location,
                currentCtx);
        }
    }
}
