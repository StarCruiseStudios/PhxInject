// -----------------------------------------------------------------------------
//  <copyright file="InjectionMapper.cs" company="Star Cruise Studios LLC">
//      Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.
//      Licensed under the Apache License 2.0 License.
//      See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
//  </copyright>
// -----------------------------------------------------------------------------

namespace Phx.Inject.Generator.Map {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Phx.Inject.Generator.Construct.Definitions;
    using Phx.Inject.Generator.Extract.Model;

    internal class InjectionMapper : IInjectionMapper {
        private readonly IInjectorMapper injectorMapper;
        private readonly ISpecContainerMapper specContainerMapper;

        public InjectionMapper(IInjectorMapper injectorMapper, ISpecContainerMapper specContainerMapper) {
            this.injectorMapper = injectorMapper;
            this.specContainerMapper = specContainerMapper;
        }

        public InjectionDefinition MapToDefinition(
                InjectorModel injectorModel,
                IEnumerable<SpecificationModel> specModels
        ) {
            IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations
                    = new Dictionary<RegistrationIdentifier, FactoryRegistration>();
            IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations
                    = new Dictionary<RegistrationIdentifier, BuilderRegistration>();

            var injectorSpecModels = injectorModel.Specifications.Select(
                    specType => {
                        var specModel = specModels.Where(model => model.SpecificationType == specType)
                                .DefaultIfEmpty()
                                .Single();
                        if (specModel == null) {
                            throw new InvalidOperationException(
                                    $"Cannot find specification of type {specType} required by injector {injectorModel.InjectorInterface}.");
                        }

                        return specModel;
                    });

            foreach (var specModel in injectorSpecModels) {
                foreach (var factory in specModel.Factories) {
                    factoryRegistrations.Add(
                            factory.ReturnType.ToRegistrationIdentifier(),
                            new FactoryRegistration(specModel.SpecificationType, factory));
                }

                foreach (var builder in specModel.Builders) {
                    builderRegistrations.Add(
                            builder.BuiltType.ToRegistrationIdentifier(),
                            new BuilderRegistration(specModel.SpecificationType, builder));
                }
            }

            foreach (var specModel in injectorSpecModels) {
                foreach (var link in specModel.Links) {
                    if (factoryRegistrations.TryGetValue(
                                new RegistrationIdentifier(link.InputType.ToTypeDefinition(), link.InputQualifier),
                                out var targetRegistration)) {
                        factoryRegistrations.Add(
                                new RegistrationIdentifier(link.ReturnType.ToTypeDefinition(), link.ReturnQualifier),
                                targetRegistration);
                    } else {
                        throw new InvalidOperationException(
                                $"Cannot link {link.ReturnType}[{link.ReturnQualifier}] to unknown type {link.InputType}[{link.InputQualifier}].");
                    }
                }
            }

            var injectorDefinition = injectorMapper.MapToDefinition(
                    injectorModel,
                    factoryRegistrations,
                    builderRegistrations);
            var specContainerDefintions = injectorSpecModels
                    .Select(
                            specModel => specContainerMapper.MapToDefinition(
                                    specModel,
                                    injectorModel,
                                    factoryRegistrations))
                    .ToImmutableList();

            return new InjectionDefinition(
                    injectorDefinition,
                    specContainerDefintions);
        }
    }
}
