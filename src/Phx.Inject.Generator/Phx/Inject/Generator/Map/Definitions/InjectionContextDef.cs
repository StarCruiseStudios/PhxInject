// -----------------------------------------------------------------------------
//  <copyright file="InjectionContextDef.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License, Version 2.0.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Phx.Inject.Common.Exceptions;
using Phx.Inject.Common.Model;
using Phx.Inject.Generator.Extract.Descriptors;
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
            DefGenerationContext parentCtx
        ) {
            var factoryRegistrations = new Dictionary<RegistrationIdentifier, List<FactoryRegistration>>();
            var builderRegistrations = new Dictionary<RegistrationIdentifier, BuilderRegistration>();

            IReadOnlyList<SpecDesc> specDescs = parentCtx.Specifications.Values.ToImmutableList();

            // Create a registration for all of the spec descriptors' factory and builder methods.
            foreach (var specDesc in specDescs) {
                var currentCtx = parentCtx.GetChildContext(specDesc.SpecType.TypeSymbol);
                foreach (var factory in specDesc.Factories) {
                    var key = RegistrationIdentifier.FromQualifiedTypeModel(factory.ReturnType);
                    if (!factoryRegistrations.TryGetValue(key, out var registrationList)) {
                        registrationList = new List<FactoryRegistration>();
                        factoryRegistrations.Add(key, registrationList);
                    }

                    registrationList.Add(new FactoryRegistration(specDesc, factory));
                }

                foreach (var builder in specDesc.Builders) {
                    builderRegistrations.Add(
                        RegistrationIdentifier.FromQualifiedTypeModel(builder.BuiltType),
                        new BuilderRegistration(specDesc, builder));
                }
            }

            // Create a registration for all of the spec descriptors' links. This must be done after all factory methods
            // have been registered to ensure that the link is valid.
            foreach (var specDesc in specDescs) {
                foreach (var link in specDesc.Links) {
                    if (factoryRegistrations.TryGetValue(
                        RegistrationIdentifier.FromQualifiedTypeModel(link.InputType),
                        out var targetRegistration)) {
                        factoryRegistrations.Add(
                            RegistrationIdentifier.FromQualifiedTypeModel(link.ReturnType),
                            targetRegistration);
                    } else {
                        throw Diagnostics.IncompleteSpecification.AsException(
                            $"Cannot find factory for type {link.InputType} required by link in specification {specDesc.SpecType}.",
                            link.Location,
                            parentCtx);
                    }
                }
            }

            foreach (var factoryRegistration in factoryRegistrations.Values) {
                if (factoryRegistration.Count > 1 && !factoryRegistration.All(it => it.FactoryMetadata.isPartial)) {
                    parentCtx.Aggregator.AggregateMany<FactoryRegistration, FactoryRegistration>(
                        factoryRegistration,
                        registration => $"registering factory {registration.FactoryMetadata.ReturnType}",
                        registration => throw Diagnostics.InvalidSpecification.AsException(
                            $"Factory for type {registration.FactoryMetadata.ReturnType} must be unique or all factories must be partial.",
                            registration.FactoryMetadata.Location,
                            parentCtx));
                }
            }

            var generatorCtx = parentCtx with {
                FactoryRegistrations = factoryRegistrations,
                BuilderRegistrations = builderRegistrations
            };

            var injectorDef = injectorDefMapper.Map(generatorCtx);

            IReadOnlyList<SpecContainerDef> specContainerDefs = specDescs
                .Select(specDesc => specContainerDefMapper.Map(specDesc, generatorCtx))
                .ToImmutableList();

            IReadOnlyList<DependencyImplementationDef> dependencyImplementationDefs = injectorMetadata.ChildFactories
                .Select(childFactory => generatorCtx.GetInjector(
                    childFactory.ChildInjectorType,
                    childFactory.Location))
                .Select(childInjector => childInjector.DependencyInterfaceType)
                .OfType<TypeModel>()
                .GroupBy(dependencyType => dependencyType)
                .Select(dependencyTypeGroup => dependencyTypeGroup.First())
                .Select(dependencyType => generatorCtx.GetDependency(
                    dependencyType,
                    injectorMetadata.Location))
                .Select(dependency => dependencyImplementationDefMapper.Map(
                    dependency,
                    generatorCtx))
                .ToImmutableList();

            return new InjectionContextDef(
                injectorDef,
                specContainerDefs,
                dependencyImplementationDefs,
                injectorMetadata.Location);
        }
    }
}
