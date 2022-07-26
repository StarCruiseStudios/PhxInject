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

        public InjectionDefinition MapToDefinition(InjectorModel injectorModel, IEnumerable<SpecificationModel> specModels) {
            IDictionary<RegistrationIdentifier, FactoryRegistration> factoryRegistrations = new Dictionary<RegistrationIdentifier, FactoryRegistration>();
            IDictionary<RegistrationIdentifier, BuilderRegistration> builderRegistrations = new Dictionary<RegistrationIdentifier, BuilderRegistration>();

            foreach (var specModel in specModels) {
                foreach (var factory in specModel.Factories) {
                    factoryRegistrations.Add(new RegistrationIdentifier(factory.ReturnType.ToTypeDefinition()), new FactoryRegistration(specModel.SpecificationType, factory));
                }

                foreach (var builder in specModel.Builders) {
                    builderRegistrations.Add(new RegistrationIdentifier(builder.BuiltType.ToTypeDefinition()), new BuilderRegistration(specModel.SpecificationType, builder));
                }
            }

            foreach (var specModel in specModels) {
                foreach (var link in specModel.Links) {
                    if (factoryRegistrations.TryGetValue(new RegistrationIdentifier(link.InputType.ToTypeDefinition()), out var targetRegistration)) {
                        factoryRegistrations.Add(new RegistrationIdentifier(link.ReturnType.ToTypeDefinition()), targetRegistration);
                    } else {
                        throw new InvalidOperationException($"Cannot link {link.ReturnType} to unknown type {link.InputType}.");
                    }
                }
            }

            var injectorDefinition = injectorMapper.MapToDefinition(injectorModel, factoryRegistrations, builderRegistrations);
            var specContainerDefintions = specModels.Where(specModel => specModel.Factories.Count > 0 || specModel.Builders.Count > 0)
                .Select(specModel => specContainerMapper.MapToDefinition(specModel, injectorModel, factoryRegistrations))
                .ToImmutableList();

            return new InjectionDefinition(
                injectorDefinition,
                specContainerDefintions);
        }
    }
}